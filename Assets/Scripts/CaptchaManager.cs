using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CaptchaManager : MonoBehaviour
{
    [SerializeField] private GameObject completionUI;
    [SerializeField] private Color filterColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);

    private static CaptchaManager _instance;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    void OnEnable()
    {
        MemoryGame.OnGameWon += ShowCompletion;
        hw2Skeleton.OnPuzzleSolved += ShowCompletion;
    }

    void OnDisable()
    {
        MemoryGame.OnGameWon -= ShowCompletion;
        hw2Skeleton.OnPuzzleSolved -= ShowCompletion;
    }

    private void ShowCompletion()
    {
        Debug.Log("Captcha Completed!");
        if (completionUI != null)
        {
            completionUI.SetActive(true);
            
            // Re-assign camera if needed
            Canvas canvas = completionUI.GetComponent<Canvas>();
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null)
            {
                canvas.worldCamera = Camera.main;
            }
        }
        else
        {
            CreateCompletionUI();
        }
    }

    [ContextMenu("Test Show UI")]
    public void CreateCompletionUI()
    {
        // Create Canvas
        GameObject canvasGO = new GameObject("CaptchaCompletionCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = mainCam;
            canvas.planeDistance = 1.0f; // 1 meter away is standard for VR UI
        }
        else
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
        
        canvas.sortingOrder = 999;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasGO.AddComponent<GraphicRaycaster>();

        // Create Background Filter (Dark Gray)
        GameObject filterGO = new GameObject("DarkFilter");
        filterGO.transform.SetParent(canvasGO.transform, false);
        Image filterImage = filterGO.AddComponent<Image>();
        filterImage.color = filterColor;
        filterImage.raycastTarget = true; // Block interaction with game while popup is up
        
        RectTransform filterRT = filterGO.GetComponent<RectTransform>();
        filterRT.anchorMin = Vector2.zero;
        filterRT.anchorMax = Vector2.one;
        filterRT.sizeDelta = Vector2.zero;

        // Create Popup Text
        GameObject popupGO = new GameObject("CompletionPopup");
        popupGO.transform.SetParent(canvasGO.transform, false);
        
        TextMeshProUGUI text = popupGO.AddComponent<TextMeshProUGUI>();
        text.text = "CAPTCHA completed";
        text.fontSize = 60;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.raycastTarget = false;
        
        // Add Outline to make it "clearer" as requested
        text.outlineWidth = 0.2f;
        text.outlineColor = Color.black;

        RectTransform popupRT = popupGO.GetComponent<RectTransform>();
        popupRT.sizeDelta = new Vector2(1200, 400);
        popupRT.anchoredPosition = Vector2.zero;

        completionUI = canvasGO;
        
        // Ensure an EventSystem exists
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }
}
