using TMPro;
using UnityEngine;

/// <summary>
/// SpatialCaptchaManager의 상태를 폴링하여 UI에 반영한다.
/// TextMeshPro 패키지가 프로젝트에 포함되어 있어야 한다.
/// </summary>
public class CaptchaUIController : MonoBehaviour
{
    // ───────────────────────────────────────────────
    // Inspector Fields
    // ───────────────────────────────────────────────

    [Header("Manager Reference")]
    [Tooltip("상태를 읽어올 SpatialCaptchaManager")]
    public SpatialCaptchaManager captchaManager;

    [Header("UI Elements")]
    [Tooltip("남은 시간 텍스트 (예: \"남은 시간: 45.3초\")")]
    public TMP_Text timerText;

    [Tooltip("인증 성공 시 표시할 패널")]
    public GameObject successPanel;

    [Tooltip("인증 실패 시 표시할 패널")]
    public GameObject failPanel;

    [Tooltip("캡차 진행 중 표시할 패널")]
    public GameObject captchaPanel;

    // ───────────────────────────────────────────────
    // Internal State
    // ───────────────────────────────────────────────

    /// <summary>성공/실패 패널 전환이 이미 수행됐는지 추적 (중복 호출 방지)</summary>
    private bool _resultShown = false;

    // ───────────────────────────────────────────────
    // Unity Lifecycle
    // ───────────────────────────────────────────────

    void Start()
    {
        if (captchaManager == null)
            Debug.LogError("[CaptchaUI] captchaManager가 연결되지 않았습니다.");

        if (timerText == null)
            Debug.LogWarning("[CaptchaUI] timerText가 연결되지 않았습니다.");
    }

    void Update()
    {
        if (captchaManager == null) return;

        // ── 1. 타이머 텍스트 매 프레임 갱신
        UpdateTimerText(captchaManager.RemainingTime);

        // ── 2. 결과 패널 전환 (한 번만 실행)
        if (_resultShown) return;

        if (captchaManager.IsSolved)
        {
            ShowSuccess();
        }
        else if (captchaManager.IsFailed)
        {
            ShowFail();
        }
    }

    // ───────────────────────────────────────────────
    // UI Update Helpers
    // ───────────────────────────────────────────────

    void UpdateTimerText(float remaining)
    {
        if (timerText == null) return;
        timerText.text = $"남은 시간: {remaining:F1}초";
    }

    void ShowSuccess()
    {
        _resultShown = true;

        SetPanelActive(captchaPanel, false);
        SetPanelActive(failPanel,    false);
        SetPanelActive(successPanel, true);

        Debug.Log("[CaptchaUI] 성공 패널 표시.");
    }

    void ShowFail()
    {
        _resultShown = true;

        SetPanelActive(captchaPanel,  false);
        SetPanelActive(successPanel,  false);
        SetPanelActive(failPanel,     true);

        Debug.Log("[CaptchaUI] 실패 패널 표시.");
    }

    /// <summary>null 안전 패널 활성/비활성</summary>
    void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }

    // ───────────────────────────────────────────────
    // Public API
    // ───────────────────────────────────────────────

    /// <summary>
    /// 캡차 시작 시 호출. captchaPanel을 활성화하고
    /// success/fail 패널을 숨긴 뒤 결과 플래그를 초기화한다.
    /// </summary>
    public void ShowCaptchaStart()
    {
        _resultShown = false;

        SetPanelActive(successPanel, false);
        SetPanelActive(failPanel,    false);
        SetPanelActive(captchaPanel, true);

        if (timerText != null && captchaManager != null)
            UpdateTimerText(captchaManager.RemainingTime);

        Debug.Log("[CaptchaUI] 캡차 시작 UI 표시.");
    }
}
