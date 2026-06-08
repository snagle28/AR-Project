using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// AhaPoint 도착(IsSolved) 후 popupDelay초 뒤 4개 보기 버튼 팝업.
/// GazeButton을 통해 정답/오답을 판정하고 결과를 표시한다.
/// </summary>
public class CaptchaAnswerUI : MonoBehaviour
{
    // ───────────────────────────────────────────────
    // Inspector Fields
    // ───────────────────────────────────────────────

    [Header("References")]
    [Tooltip("정답 위치 도달 감지용 SpatialCaptchaManager")]
    public SpatialCaptchaManager captchaManager;

    [Tooltip("현재 선택된 형태 이름 조회용 CaptchaShapePool")]
    public CaptchaShapePool shapePool;

    [Tooltip("4개 보기 버튼의 부모 GameObject")]
    public GameObject answerPanel;

    [Tooltip("정답/오답 결과 표시 패널")]
    public GameObject resultPanel;

    [Tooltip("결과 텍스트 (TMP)")]
    public TMP_Text resultText;

    [Header("Settings")]
    [Tooltip("IsSolved 감지 후 팝업까지 대기 시간 (초)")]
    public float popupDelay = 3f;

    [Tooltip("보기 후보 전체 이름 목록 (정답 포함)")]
    public string[] allShapeNames = { "Key", "Heart", "Doll", "Chair" };

    // ───────────────────────────────────────────────
    // Private State
    // ───────────────────────────────────────────────

    /// <summary>팝업 코루틴이 이미 시작됐는지 (중복 방지)</summary>
    private bool _popupTriggered = false;

    /// <summary>현재 라운드 정답 이름</summary>
    private string _correctName = "";

    // ───────────────────────────────────────────────
    // Unity Lifecycle
    // ───────────────────────────────────────────────

    void Start()
    {
        if (captchaManager == null)
            Debug.LogError("[CaptchaAnswerUI] captchaManager가 연결되지 않았습니다.");
        if (shapePool == null)
            Debug.LogError("[CaptchaAnswerUI] shapePool이 연결되지 않았습니다.");

        SetPanelActive(answerPanel, false);
        SetPanelActive(resultPanel, false);
    }

    void Update()
    {
        if (_popupTriggered) return;
        if (captchaManager == null) return;

        if (captchaManager.IsSolved)
        {
            _popupTriggered = true;
            StartCoroutine(PopupAfterDelay());
        }
    }

    // ───────────────────────────────────────────────
    // Core Logic
    // ───────────────────────────────────────────────

    /// <summary>popupDelay초 후 answerPanel 표시</summary>
    private IEnumerator PopupAfterDelay()
    {
        yield return new WaitForSeconds(popupDelay);
        ShowAnswerPanel();
    }

    /// <summary>
    /// 정답을 포함한 4개 보기를 구성하고 answerPanel을 활성화한다.
    /// answerPanel 하위 GazeButton 컴포넌트를 찾아 각각 연결한다.
    /// </summary>
/// <summary>
    /// 정답을 포함한 4개 보기를 구성하고 answerPanel을 활성화한다.
    /// answerPanel 하위 GazeButton 컴포넌트를 찾아 각각 연결한다.
    /// </summary>
    public void ShowAnswerPanel()
    {
        if (answerPanel == null) return;
        if (shapePool == null)  return;

        // ── 1. 표시명(정답) 확정
        _correctName = shapePool.GetCurrentDisplayName();
        Debug.Log("[CaptchaAnswerUI] 정답: " + _correctName);

        // ── 2. 보기 후보 전체 목록 (shapePool 매핑 우선, 없으면 allShapeNames fallback)
        System.Collections.Generic.List<string> allNames = shapePool.GetAllDisplayNames();
        if (allNames == null || allNames.Count == 0)
        {
            allNames = new System.Collections.Generic.List<string>(allShapeNames);
        }

        // ── 3. 오답 후보 구성 (정답 제외)
        System.Collections.Generic.List<string> wrongCandidates =
            new System.Collections.Generic.List<string>();

        foreach (string name in allNames)
        {
            if (name != _correctName)
                wrongCandidates.Add(name);
        }

        // 오답 후보 셔플
        for (int i = wrongCandidates.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            string tmp = wrongCandidates[i];
            wrongCandidates[i] = wrongCandidates[j];
            wrongCandidates[j] = tmp;
        }

        // 정답 + 최대 3개 오답
        System.Collections.Generic.List<string> choices =
            new System.Collections.Generic.List<string>();
        choices.Add(_correctName);
        for (int i = 0; i < 3 && i < wrongCandidates.Count; i++)
            choices.Add(wrongCandidates[i]);

        // ── 4. 보기 셔플
        for (int i = choices.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            string tmp = choices[i];
            choices[i] = choices[j];
            choices[j] = tmp;
        }

        // ── 5. answerPanel 하위 GazeButton에 보기 할당
        GazeButton[] buttons = answerPanel.GetComponentsInChildren<GazeButton>(true);

        for (int i = 0; i < buttons.Length && i < choices.Count; i++)
        {
            string choiceName = choices[i];
            GazeButton btn    = buttons[i];

            TMPro.TMP_Text label = btn.GetComponentInChildren<TMPro.TMP_Text>(true);
            if (label != null)
                label.text = choiceName;

            btn.OnGazeConfirmed.RemoveAllListeners();

            string captured = choiceName;
            btn.OnGazeConfirmed.AddListener(() => CheckAnswer(captured));

            btn.gameObject.SetActive(true);
        }

        SetPanelActive(resultPanel, false);
        SetPanelActive(answerPanel, true);
    }

    /// <summary>
    /// 선택한 보기 이름과 정답을 비교해 결과를 표시한다.
    /// 오답이면 2초 후 answerPanel을 다시 활성화한다.
    /// </summary>
/// <summary>
    /// 선택한 보기 이름과 정답을 비교해 결과를 표시한다.
    /// 오답이면 2초 후 answerPanel을 다시 활성화한다.
    /// </summary>
    public void CheckAnswer(string selectedName)
    {
        SetPanelActive(answerPanel, false);

        if (selectedName == _correctName)
        {
            if (resultText != null)
                resultText.text = "Correct! \u2705";

            SetPanelActive(resultPanel, true);
            Debug.Log("[CaptchaAnswerUI] Correct: " + selectedName);
        }
        else
        {
            if (resultText != null)
                resultText.text = "Wrong \u274c Try again.";

            SetPanelActive(resultPanel, true);
            Debug.Log("[CaptchaAnswerUI] Wrong: " + selectedName + " (correct: " + _correctName + ")");

            StartCoroutine(RetryAfterDelay(2f));
        }
    }

    /// <summary>delay초 후 resultPanel을 숨기고 answerPanel을 다시 표시한다.</summary>
    private IEnumerator RetryAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetPanelActive(resultPanel, false);
        ShowAnswerPanel();
    }

    // ───────────────────────────────────────────────
    // Public API
    // ───────────────────────────────────────────────

    /// <summary>상태를 초기화하고 다음 라운드를 준비한다.</summary>
    public void ResetUI()
    {
        _popupTriggered = false;
        _correctName    = "";
        SetPanelActive(answerPanel, false);
        SetPanelActive(resultPanel, false);
        Debug.Log("[CaptchaAnswerUI] UI 리셋 완료.");
    }

    // ───────────────────────────────────────────────
    // Helpers
    // ───────────────────────────────────────────────

    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }
}
