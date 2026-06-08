// ============================================================
//  hw2Base.cs  —  PROVIDED (do not modify)
//
//  This base class handles all the setup and boilerplate for HW2.
//  Students only need to work in hw2_script.cs.
// ============================================================

using UnityEngine;
using Oculus.Interaction;

[RequireComponent(typeof(Grabbable))]
public abstract class hw2Base : MonoBehaviour, ITransformer
{
    // ── Inspector fields ────────────────────────────────────────
    
    // ── Internal state ──────────────────────────────────────────
    protected Renderer        m_Renderer;
    protected Color           m_OriginalColor;
    protected RayInteractable m_RayInteractable;
    protected IGrabbable      m_Grabbable;
    protected bool            m_StretchStarted;
    protected float           m_InitialHandDistance;
    protected Vector3         m_InitialScale;
    protected Vector3         m_SavedScale;

    // ── Setup ───────────────────────────────────────────────────
    void Awake()
    {
        m_Renderer        = GetComponent<Renderer>();
        m_OriginalColor   = m_Renderer.material.color;
        m_RayInteractable = GetComponentInChildren<RayInteractable>(false);
        m_SavedScale      = transform.localScale;

        var grabbable = GetComponent<Grabbable>();
        var oneGrab   = gameObject.AddComponent<GrabFreeTransformer>();
        grabbable.InjectOptionalOneGrabTransformer(oneGrab);
        grabbable.InjectOptionalTwoGrabTransformer(this);
    }

    // Applies the saved scale last, after all other transformers run
    void LateUpdate() => transform.localScale = m_SavedScale;

    // Subscribe/unsubscribe from ray state events
    void OnEnable()  => m_RayInteractable.WhenStateChanged += OnRayStateChanged;
    void OnDisable() => m_RayInteractable.WhenStateChanged -= OnRayStateChanged;

    // ── ITransformer ────────────────────────────────────────────
    public void Initialize(IGrabbable grabbable) => m_Grabbable = grabbable;
    public void BeginTransform()  => m_StretchStarted = false;
    public void EndTransform()    => m_StretchStarted = false;

    // On the first frame of a two-hand grab: capture reference values.
    // On subsequent frames: call ApplyTwoHandScale() (student implements this).
    public void UpdateTransform()
    {
        var pts = m_Grabbable.GrabPoints;
        if (pts.Count < 2) return;

        float currentDist = Vector3.Distance(pts[0].position, pts[1].position);

        if (!m_StretchStarted)
        {
            m_InitialHandDistance = currentDist;
            m_InitialScale        = m_SavedScale;
            m_StretchStarted      = true;
            return;
        }

        ApplyTwoHandScale(currentDist); // → student implements this in hw2_script.cs
    }

    // ── Abstract methods — students implement these ─────────────

    // Called whenever the ray state changes (Hover / Select / Normal)
    protected abstract void OnRayStateChanged(InteractableStateChangeArgs args);

    // Called every frame while two hands are grabbing.
    // currentDist = current distance between the two hands.
    // Store your result in m_SavedScale.
    protected abstract void ApplyTwoHandScale(float currentDist);
}
