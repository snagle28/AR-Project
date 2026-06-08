using UnityEngine;

/// <summary>
/// 플레이어와 AhaPointFloorMarker 사이의 XZ 거리에 따라
/// 포인트 클라우드 메시의 색상을 변경하고 파티클을 트리거한다.
/// MaterialPropertyBlock을 사용하므로 sharedMaterial을 오염시키지 않는다.
/// </summary>
public class CaptchaProximityFeedback : MonoBehaviour
{
    // ───────────────────────────────────────────────
    // Inspector Fields
    // ───────────────────────────────────────────────

    [Header("References")]
    [Tooltip("플레이어 머리(카메라) Transform")]
    public Transform userHead;

    [Tooltip("정답 위치 마커 Transform")]
    public Transform ahaPointFloorMarker;

    [Tooltip("색상을 변경할 포인트 클라우드 MeshRenderer")]
    public MeshRenderer pointCloudRenderer;

    [Tooltip("정답 위치 도달 시 재생할 파티클 (없으면 null)")]
    public ParticleSystem solvedParticle;

    [Header("Distance Thresholds")]
    [Tooltip("이 거리 이하: 하늘색 + 파티클 트리거")]
    public float nearDistance = 0.4f;

    [Tooltip("이 거리부터 흰색으로 Lerp 시작")]
    public float midDistance = 1.0f;

    [Tooltip("이 거리 이상: 회색 고정")]
    public float farDistance = 3.0f;

    [Header("Colors")]
    public Color farColor  = new Color(0.4f, 0.4f, 0.4f, 1f);  // 회색
    public Color midColor  = new Color(1f,   1f,   1f,   1f);  // 흰색
    public Color nearColor = new Color(0f,   0.8f, 1f,   1f);  // 하늘색

    // ───────────────────────────────────────────────
    // Private State
    // ───────────────────────────────────────────────

    private MaterialPropertyBlock _propBlock;
    private static readonly int _baseColorID = Shader.PropertyToID("_BaseColor");

    /// <summary>파티클이 이미 재생됐는지 추적 (중복 방지)</summary>
    private bool _particlePlayed = false;

    // ───────────────────────────────────────────────
    // Unity Lifecycle
    // ───────────────────────────────────────────────

    void Start()
    {
        _propBlock = new MaterialPropertyBlock();

        if (userHead == null)
            Debug.LogError("[ProximityFeedback] userHead가 연결되지 않았습니다.");
        if (ahaPointFloorMarker == null)
            Debug.LogError("[ProximityFeedback] ahaPointFloorMarker가 연결되지 않았습니다.");
        if (pointCloudRenderer == null)
            Debug.LogError("[ProximityFeedback] pointCloudRenderer가 연결되지 않았습니다.");

        // 초기 색상: 회색
        ApplyColor(farColor);
    }

    void Update()
    {
        if (userHead == null || ahaPointFloorMarker == null || pointCloudRenderer == null)
            return;

        // ── XZ 평면 거리 계산 (Y 무시)
        Vector2 userXZ   = new Vector2(userHead.position.x, userHead.position.z);
        Vector2 markerXZ = new Vector2(ahaPointFloorMarker.position.x, ahaPointFloorMarker.position.z);
        float dist = Vector2.Distance(userXZ, markerXZ);

        // ── 거리별 색상 결정
        Color targetColor;

        if (dist <= nearDistance)
        {
            // 근접 구간: 하늘색 고정 + 파티클
            targetColor = nearColor;
            TryPlayParticle();
        }
        else if (dist <= midDistance)
        {
            // near ~ mid 구간: nearColor → midColor Lerp
            float t = Mathf.InverseLerp(nearDistance, midDistance, dist);
            targetColor = Color.Lerp(nearColor, midColor, t);
            ResetParticleFlag();
        }
        else if (dist <= farDistance)
        {
            // mid ~ far 구간: midColor → farColor Lerp
            float t = Mathf.InverseLerp(midDistance, farDistance, dist);
            targetColor = Color.Lerp(midColor, farColor, t);
            ResetParticleFlag();
        }
        else
        {
            // far 초과: 회색 고정
            targetColor = farColor;
            ResetParticleFlag();
        }

        ApplyColor(targetColor);
    }

    // ───────────────────────────────────────────────
    // Helpers
    // ───────────────────────────────────────────────

    /// <summary>MaterialPropertyBlock으로 색상 적용</summary>
    void ApplyColor(Color color)
    {
        pointCloudRenderer.GetPropertyBlock(_propBlock);
        _propBlock.SetColor(_baseColorID, color);
        pointCloudRenderer.SetPropertyBlock(_propBlock);
    }

    /// <summary>파티클을 한 번만 재생</summary>
    void TryPlayParticle()
    {
        if (_particlePlayed) return;
        if (solvedParticle == null) return;

        _particlePlayed = true;
        solvedParticle.Play();
        Debug.Log("[ProximityFeedback] 파티클 재생.");
    }

    /// <summary>근접 구간을 벗어나면 플래그 리셋 (재진입 시 재생 가능)</summary>
    void ResetParticleFlag()
    {
        _particlePlayed = false;
    }
}
