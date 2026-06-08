using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 공간 기반 캡차 시스템.
/// 플레이어가 정답 위치(ahaPointFloorMarker)에 timeLimit 초 안에 서면 인증 성공.
/// </summary>
public class SpatialCaptchaManager : MonoBehaviour
{
    // ───────────────────────────────────────────────
    // Inspector Fields
    // ───────────────────────────────────────────────

    [Header("References")]
    [Tooltip("정답 위치 마커 (바닥 기준)")]
    public Transform ahaPointFloorMarker;

    [Tooltip("플레이어 머리(카메라) Transform")]
    public Transform userHead;

    [Tooltip("랜덤 PLY 형태 풀 (CaptchaShapePool 컴포넌트)")]
    public CaptchaShapePool shapePool;

    [Tooltip("PLY 메시 생성기 (PLYAnamorphosisMeshGenerator 컴포넌트)")]
    public PLYAnamorphosisMeshGenerator meshGenerator;

    [Header("Settings")]
    [Tooltip("정답 위치로 인정하는 XZ 거리 (미터)")]
    public float triggerDistance = 0.4f;

    [Tooltip("제한 시간 (초)")]
    public float timeLimit = 60f;

    [Header("Events")]
    [Tooltip("인증 성공 시 발생하는 이벤트 (Inspector에서 연결 가능)")]
    public UnityEvent onSolved;

    [Tooltip("인증 실패(시간 초과) 시 발생하는 이벤트 (Inspector에서 연결 가능)")]
    public UnityEvent onFailed;

    // ───────────────────────────────────────────────
    // State (Read-only in Inspector via property)
    // ───────────────────────────────────────────────

    [Header("State (Read-Only)")]
    [SerializeField] private bool _isSolved = false;
    [SerializeField] private bool _isFailed = false;
    [SerializeField] private float _remainingTime;

    public bool IsSolved => _isSolved;
    public bool IsFailed => _isFailed;
    public float RemainingTime => _remainingTime;

    // ───────────────────────────────────────────────
    // Unity Lifecycle
    // ───────────────────────────────────────────────

void Start()
    {
        _remainingTime = timeLimit;

        if (ahaPointFloorMarker == null)
            Debug.LogError("[SpatialCaptcha] ahaPointFloorMarker가 연결되지 않았습니다.");

        if (userHead == null)
            Debug.LogError("[SpatialCaptcha] userHead가 연결되지 않았습니다.");

        Debug.Log($"[SpatialCaptcha] 준비 완료. 제한 시간: {timeLimit}초 / 트리거 거리: {triggerDistance}m");
    }

    void Update()
    {
        // 이미 종료된 상태면 아무것도 하지 않음
        if (_isSolved || _isFailed) return;

        // 레퍼런스 누락 시 조용히 스킵
        if (ahaPointFloorMarker == null || userHead == null) return;

        // ── 1. 타이머 카운트다운
        _remainingTime -= Time.deltaTime;

        if (_remainingTime <= 0f)
        {
            _remainingTime = 0f;
            Fail();
            return;
        }

        // ── 2. 정답 위치 판정 (XZ 평면 거리만 사용, Y 무시)
        Vector2 userXZ   = new Vector2(userHead.position.x, userHead.position.z);
        Vector2 markerXZ = new Vector2(ahaPointFloorMarker.position.x, ahaPointFloorMarker.position.z);

        float dist = Vector2.Distance(userXZ, markerXZ);

        if (dist <= triggerDistance)
        {
            Solve();
        }
    }

    // ───────────────────────────────────────────────
    // Core Logic
    // ───────────────────────────────────────────────

    /// <summary>인증 성공 처리</summary>
    void Solve()
    {
        _isSolved = true;

        float elapsed = timeLimit - _remainingTime;
        Debug.Log($"[SpatialCaptcha] ✅ 인증 성공! 소요 시간: {elapsed:F1}초 / 남은 시간: {_remainingTime:F1}초");

        onSolved.Invoke();
    }

    /// <summary>시간 초과 실패 처리</summary>
    void Fail()
    {
        _isFailed = true;

        Debug.Log($"[SpatialCaptcha] ❌ 인증 실패. 제한 시간 {timeLimit}초 초과.");

        onFailed.Invoke();
    }

    // ───────────────────────────────────────────────
    // Public API
    // ───────────────────────────────────────────────

    /// <summary>
    /// shapePool에서 랜덤 PLY 형태를 골라 meshGenerator에 할당한 뒤
    /// 캡차를 초기화하고 메시를 생성한다.
    /// Inspector의 Button OnClick 또는 에디터 우클릭 ContextMenu로 호출 가능.
    /// </summary>
    [ContextMenu("Test Start Captcha")]
    public void StartCaptcha()
    {
        // ── 1. 레퍼런스 검사
        if (shapePool == null)
        {
            Debug.LogError("[SpatialCaptcha] shapePool이 연결되지 않았습니다.");
            return;
        }

        if (meshGenerator == null)
        {
            Debug.LogError("[SpatialCaptcha] meshGenerator가 연결되지 않았습니다.");
            return;
        }

        // ── 2. 랜덤 형태 선택
        TextAsset selectedShape = shapePool.GetRandomShape();

        if (selectedShape == null)
        {
            Debug.LogError("[SpatialCaptcha] shapePool에서 유효한 PLY 파일을 가져오지 못했습니다.");
            return;
        }

        // ── 3. MeshGenerator에 할당 후 생성
        meshGenerator.plyFile = selectedShape;
        meshGenerator.GenerateFromPLY();

        Debug.Log($"[SpatialCaptcha] StartCaptcha — 형태: {selectedShape.name}");

        // ── 4. 타이머 리셋 후 캡차 시작
        ResetCaptcha();
    }

    /// <summary>캡차를 초기 상태로 리셋</summary>
    public void ResetCaptcha()
    {
        _isSolved = false;
        _isFailed = false;
        _remainingTime = timeLimit;

        Debug.Log("[SpatialCaptcha] 리셋 완료.");
    }
}
