using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Spatial CAPTCHA 전체 흐름 관리.
/// Intro → Navigate → AtAhaPoint → Answering → Result
/// </summary>
public class GameFlowManager : MonoBehaviour
{
    public enum Phase { Intro, Navigate, AtAhaPoint, Answering, Result }

    /// <summary>
    /// 퍼즐 정답을 맞췄을 때 1회 발생하는 전역 이벤트.
    /// 팀의 CaptchaManager가 이 이벤트를 구독해 다음 퍼즐 진행 등을 트리거한다.
    /// (Self-Sufficiency: 이 스크립트는 CaptchaManager를 직접 참조하지 않는다)
    /// </summary>
    public static event System.Action OnPuzzleCompleted;

    [Header("Phase (Read-Only)")]
    [SerializeField] private Phase _phase = Phase.Intro;

    // ── Scene Objects ──────────────────────────────────────────────
    [Header("Key Objects")]
    public GameObject keyMesh;
    public GameObject keyFloorMarker;
    public GameObject keyView;

    [Header("Heart Objects")]
    public GameObject heartMesh;
    public GameObject heartFloorMarker;
    public GameObject heartView;

    [Header("Start Button")]
    public GazeButton startButton;

    [Header("UI Panels")]
    public GameObject introPanel;
    public GameObject navigatePanel;
    public GameObject atAhaPanel;
    public GameObject answerPanel;
    public GameObject successPanel;
    public GameObject failPanel;

    [Header("Answer Buttons")]
    public GazeButton btnKey;
    public GazeButton btnHeart;
    public GazeButton btnChair;
    public GazeButton btnDoll;

    [Header("Settings")]
    public float atAhaToAnswerDelay = 10f;
    public float failRetryDelay     = 3f;
    public float answerTimeLimit    = 30f;

    [Header("Timer Text")]
    public TMP_Text timerText;

    // ── State ──────────────────────────────────────────────────────
    private string          _selectedShape;
    private AhaPointTrigger _activeTrigger;
    private float           _answerTimer;
    private Vector3         _resultPanelPos;
    private Quaternion      _resultPanelRot = Quaternion.identity;
    private Transform       _centerEyeAnchor; // 런타임에 OVRCameraRig에서 찾아 캐시 (씬 직접 참조 금지)

    // ── Lifecycle ──────────────────────────────────────────────────
    void Start()
    {
        // 모든 메시/마커 비활성화
        SetGO(keyMesh,          false);
        SetGO(keyFloorMarker,   false);
        SetGO(keyView,          false);
        SetGO(heartMesh,        false);
        SetGO(heartFloorMarker, false);
        SetGO(heartView,        false);

        PositionPanelInFrontOfUser(introPanel);
        ShowOnly(introPanel);
        _phase = Phase.Intro;

        // START 버튼 이벤트 연결 (런타임)
        if (startButton != null)
            startButton.OnGazeConfirmed.AddListener(OnStartPressed);
        else
        {
            var gbs = Resources.FindObjectsOfTypeAll<GazeButton>();
            foreach (var gb in gbs)
                if (gb.name == "GazeButton_Start") { gb.OnGazeConfirmed.AddListener(OnStartPressed); break; }
        }

        Debug.Log("[GameFlow] Phase: Intro");
    }

    void Update()
    {
        if (_phase != Phase.Answering) return;
        _answerTimer -= Time.deltaTime;
        if (timerText != null)
            timerText.text = Mathf.CeilToInt(Mathf.Max(0, _answerTimer)).ToString();
        if (_answerTimer <= 0f) OnTimerExpired();
    }

    // ── Phase Transitions ──────────────────────────────────────────

    public void OnStartPressed()
    {
        if (_phase != Phase.Intro) return;
        StartNavigate();
    }

    void StartNavigate()
    {
        // Key / Heart 랜덤 선택
        _selectedShape = (Random.value < 0.5f) ? "Key" : "Heart";
        Debug.Log("[GameFlow] Selected: " + _selectedShape);

        if (_selectedShape == "Key")
        {
            SetGO(keyMesh, true); SetGO(keyFloorMarker, true); SetGO(keyView, true);
            _activeTrigger = keyFloorMarker?.GetComponent<AhaPointTrigger>();
        }
        else
        {
            SetGO(heartMesh, true); SetGO(heartFloorMarker, true); SetGO(heartView, true);
            _activeTrigger = heartFloorMarker?.GetComponent<AhaPointTrigger>();
        }

        if (_activeTrigger != null)
            _activeTrigger.onEntered.AddListener(OnAhaPointReached);
        else
            Debug.LogError("[GameFlow] AhaPointTrigger not found!");

        _phase = Phase.Navigate;
        PositionPanelInFrontOfUser(navigatePanel);
        ShowOnly(navigatePanel);
        Debug.Log("[GameFlow] Phase: Navigate");
    }

    public void OnAhaPointReached()
    {
        if (_phase != Phase.Navigate) return;
        _phase = Phase.AtAhaPoint;
        PositionPanelInFrontOfUser(atAhaPanel);
        ShowOnly(atAhaPanel);
        Debug.Log("[GameFlow] Phase: AtAhaPoint — waiting " + atAhaToAnswerDelay + "s");
        StartCoroutine(ShowAnswerAfterDelay());
    }

    IEnumerator ShowAnswerAfterDelay()
    {
        yield return new WaitForSeconds(atAhaToAnswerDelay);
        PositionAnswerPanel();
        StartAnswering();
    }

// 카메라(유저) 정면 일정 거리에 패널을 배치한다.
    // IntroPanel/NavigatePanel/AtAhaPanel처럼 '특정 지점'이 아니라
    // '유저가 보고 있는 방향'에 떠야 하는 안내성 UI에 사용.
    // (Root에 베이크된 로컬 오프셋은 PuzzleSpawner가 정하는 스폰 회전에 따라
    //  엉뚱한 곳—심지어 유저 뒤쪽—에 나타날 수 있으므로 런타임에 재계산한다)
    void PositionPanelInFrontOfUser(GameObject panel, float distance = 1.2f)
    {
        if (panel == null || _centerEyeAnchor == null) return;

        Vector3 camPos = _centerEyeAnchor.position;
        Vector3 fwd = _centerEyeAnchor.forward; fwd.y = 0f;
        if (fwd == Vector3.zero) fwd = Vector3.forward; else fwd.Normalize();

        Vector3 panelPos = camPos + fwd * distance;
        panelPos.y = camPos.y;

        Vector3 look = camPos - panelPos; look.y = 0f;
        Quaternion panelRot = Quaternion.identity;
        if (look != Vector3.zero)
            panelRot = Quaternion.LookRotation(-look); // 정면(-Z)이 유저를 향하도록

        panel.transform.SetPositionAndRotation(panelPos, panelRot);
    }

    void PositionAnswerPanel()
    {
        if (answerPanel == null) return;

        // 활성 메시 오른쪽에 AnswerPanel 배치
        GameObject activeMesh = (_selectedShape == "Key") ? keyMesh : heartMesh;
        GameObject activeView = (_selectedShape == "Key") ? keyView  : heartView;
        if (activeMesh == null || activeView == null) return;

        Vector3 meshPos = activeMesh.transform.position;
        Vector3 viewPos = activeView.transform.position;
        Vector3 fwd     = (meshPos - viewPos).normalized;
        if (fwd == Vector3.zero) fwd = Vector3.forward;

        Vector3 right    = Vector3.Cross(Vector3.up, fwd).normalized;
        Vector3 panelPos = meshPos + right * 0.9f;
        panelPos.y = viewPos.y;

        Vector3 look = viewPos - panelPos; look.y = 0;
        Quaternion panelRot = Quaternion.identity;
        // World-space Canvas의 '정면(읽히는 방향)'은 로컬 -Z 쪽이다.
        // 따라서 -Z가 시청자(viewPos) 쪽을 향하도록 -look 방향으로 LookRotation을 적용해야
        // 사용자가 정면(똑바로 보이는 면)을 보게 된다. (look 그대로 쓰면 뒷면이 보여 뒤집혀 보인다)
        if (look != Vector3.zero)
            panelRot = Quaternion.LookRotation(-look);

        answerPanel.transform.SetPositionAndRotation(panelPos, panelRot);

        // Success/Fail 패널도 같은 자리에 뜨도록 위치/회전을 저장해둔다
        _resultPanelPos = panelPos;
        _resultPanelRot = panelRot;

        Debug.Log($"[GameFlow] AnswerPanel → {panelPos}");
    }

    void StartAnswering()
    {
        _phase = Phase.Answering;
        _answerTimer = answerTimeLimit;

        SetupAnswerButton(btnKey,   "Key");
        SetupAnswerButton(btnHeart, "Heart");
        SetupAnswerButton(btnChair, "Chair");
        SetupAnswerButton(btnDoll,  "Doll");

        ShowOnly(answerPanel);
        Debug.Log("[GameFlow] Phase: Answering");
    }

    void SetupAnswerButton(GazeButton btn, string shapeName)
    {
        if (btn == null) return;
        btn.gameObject.SetActive(true);
        btn.ResetButton();
        btn.OnGazeConfirmed.RemoveAllListeners();
        string captured = shapeName;
        btn.OnGazeConfirmed.AddListener(() => OnAnswerSelected(captured));
    }

void OnAnswerSelected(string selected)
    {
        if (_phase != Phase.Answering) return;
        _phase = Phase.Result;
        Debug.Log($"[GameFlow] Selected={selected} Correct={_selectedShape}");

        if (selected == _selectedShape)
        {
            ShowResultPanel(successPanel);
            Debug.Log("[GameFlow] CORRECT!");
            OnPuzzleCompleted?.Invoke();
        }
        else
        {
            ShowResultPanel(failPanel);
            Debug.Log("[GameFlow] WRONG.");
            StartCoroutine(RestartAfterDelay(failRetryDelay));
        }
    }

void OnTimerExpired()
    {
        if (_phase != Phase.Answering) return;
        _phase = Phase.Result;
        ShowResultPanel(failPanel);
        StartCoroutine(RestartAfterDelay(failRetryDelay));
    }

    // 결과(Success/Fail) 패널을 AnswerPanel과 동일한 위치·방향에 배치한 뒤 표시한다
    void ShowResultPanel(GameObject panel)
    {
        if (panel != null)
            panel.transform.SetPositionAndRotation(_resultPanelPos, _resultPanelRot);
        ShowOnly(panel);
    }

    IEnumerator RestartAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Restart();
    }

    void Restart()
    {
        if (_activeTrigger != null)
        {
            _activeTrigger.onEntered.RemoveListener(OnAhaPointReached);
            _activeTrigger = null;
        }
        SetGO(keyMesh, false); SetGO(keyFloorMarker, false); SetGO(keyView, false);
        SetGO(heartMesh, false); SetGO(heartFloorMarker, false); SetGO(heartView, false);
        StartNavigate();
    }

    // ── Helpers ────────────────────────────────────────────────────
    void ShowOnly(GameObject target)
    {
        foreach (var p in new[]{ introPanel, navigatePanel, atAhaPanel,
                                  answerPanel, successPanel, failPanel })
            if (p != null) p.SetActive(p == target);
    }

    void SetGO(GameObject go, bool active) { if (go != null) go.SetActive(active); }


void Awake()
    {
        // ── 자가 캘리브레이션 (Self-Calibration) ──────────────────────
        // 이 퍼즐은 다른 씬/프로젝트의 PuzzleSpawner가 임의의 위치·방향으로
        // 스폰할 수 있으므로, 씬에 의존하지 않고 스스로 자리를 잡아야 한다.
        //
        // 1) OVRCameraRig는 이름이 아니라 '컴포넌트 타입'으로 찾는다.
        //    (이 씬의 "[BuildingBlock] Camera Rig"라는 이름은 다른 프로젝트엔 없을 수 있음)
        var rig = FindObjectOfType<OVRCameraRig>();
        if (rig != null)
        {
            _centerEyeAnchor = rig.centerEyeAnchor;

            // 2) 스폰 위치 보정: PuzzleSpawner는 '플레이어 눈높이' 기준으로 배치하지만
            //    우리 퍼즐의 마커/메시는 모두 '바닥(Y=0 상대)' 기준으로 제작되어 있다.
            //    XZ와 회전(스포너가 정한 "플레이어를 바라보는 방향")은 그대로 둔 채,
            //    Y만 Rig의 바닥 기준 높이로 1회 스냅한다. (매 프레임 X — 드리프트 방지)
            Transform root = transform.root; // 프리팹화 시 Root가 별도 오브젝트일 수 있으므로 root 사용
            Vector3 pos = root.position;
            pos.y = rig.transform.position.y;
            root.position = pos;

            Debug.Log($"[GameFlow] Self-calibrated. Root snapped to floor Y={pos.y:F2} (rig='{rig.name}')");
        }
        else
        {
            Debug.LogWarning("[GameFlow] OVRCameraRig not found — skipping self-calibration (Editor preview?).");
        }

        // 3) AhaPointTrigger의 userHead를 인스펙터 고정 참조 대신 런타임 캐시로 덮어쓴다.
        if (_centerEyeAnchor != null)
        {
            foreach (var trig in transform.root.GetComponentsInChildren<AhaPointTrigger>(true))
                trig.userHead = _centerEyeAnchor;
        }

        // 4) GazeButton의 gazeSource도 동일한 이유로 프리팹화 시 null이 된다.
        //    (씬 바깥의 CenterEyeAnchor를 참조했기 때문에 프리팹 저장 시 자동으로 끊김)
        //    -> 모든 GazeButton에 런타임으로 재할당해야 '시선 응시'가 동작하고,
        //       Start 버튼 등이 정상적으로 사라진다.
        if (_centerEyeAnchor != null)
        {
            foreach (var gb in transform.root.GetComponentsInChildren<GazeButton>(true))
                gb.gazeSource = _centerEyeAnchor;
        }

        // 5) PointCloud(아나모르픽 점군) 메시를 1회 생성한다.
        //    PLYAnamorphosisMeshGenerator.generateOnStart는 false로 꺼져 있다
        //    (에디터에서 Play를 반복할 때마다 메시가 재생성/누적되는 것을 막기 위함).
        //    따라서 런타임에는 여기서 명시적으로 1회 호출해 줘야 점군이 실제로 보인다.
        foreach (var gen in transform.root.GetComponentsInChildren<PLYAnamorphosisMeshGenerator>(true))
            gen.GenerateFromPLY();
    }
}
