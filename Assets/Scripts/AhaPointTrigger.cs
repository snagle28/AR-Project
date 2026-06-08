using UnityEngine;
using UnityEngine.Events;

public class AhaPointTrigger : MonoBehaviour
{
    [Header("User")]
    [Tooltip("CenterEyeAnchor (OVRCameraRig 하위)")]
    public Transform userHead;

    [Header("Trigger")]
    public float triggerDistance = 0.5f;
    public bool isInside = false;

    [Header("Point Object")]
    public Transform pointsParent;

    [Header("Feedback")]
    public Color normalColor = new Color(0.8f, 0.8f, 0.8f, 0.6f);
    public Color ahaColor    = new Color(0.15f, 0.95f, 0.40f, 0.85f);

    [Header("Events")]
    public UnityEvent onEntered;
    public UnityEvent onExited;

    private Renderer[]          _renderers;
    private MaterialPropertyBlock _mpb;
    private static readonly int   _baseColorID = Shader.PropertyToID("_BaseColor");

    void Start()
    {
        _mpb = new MaterialPropertyBlock();
        if (pointsParent != null)
            _renderers = pointsParent.GetComponentsInChildren<Renderer>(true);
        ApplyColor(normalColor);
    }

    void Update()
    {
        if (userHead == null) return;

        // XZ 평면 거리만 사용 (Y 무시) — 머리 높이 변화에 무관
        float dist = Vector2.Distance(
            new Vector2(userHead.position.x, userHead.position.z),
            new Vector2(transform.position.x, transform.position.z));

        if (dist < triggerDistance && !isInside)
        {
            isInside = true;
            ApplyColor(ahaColor);
            onEntered?.Invoke();
            Debug.Log($"[AhaTrigger] ENTERED {gameObject.name}  dist={dist:F2}");
        }
        else if (dist >= triggerDistance && isInside)
        {
            isInside = false;
            ApplyColor(normalColor);
            onExited?.Invoke();
        }
    }

    void ApplyColor(Color c)
    {
        if (_renderers == null || _mpb == null) return;
        _mpb.SetColor(_baseColorID, c);
        foreach (var r in _renderers)
            if (r) r.SetPropertyBlock(_mpb);
    }

    // AhaPoint 진입 여부를 외부에서 확인
    public bool IsInside => isInside;
}
