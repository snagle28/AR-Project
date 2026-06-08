using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class GazeButton : MonoBehaviour
{
    [Header("Gaze Settings")]
    public Transform gazeSource;
    public float gazeTime = 2f;

    [Header("UI Feedback")]
    public Image gazeFillImage;

    [Header("Event")]
    public UnityEvent OnGazeConfirmed = new UnityEvent();

    private Collider _collider;
    private float    _gazeTimer = 0f;
    private bool     _isGazing  = false;
    private bool     _confirmed = false;

    void Awake() { _collider = GetComponent<Collider>(); }

    void Start()
    {
        if (gazeSource == null)
            Debug.LogError("[GazeButton] gazeSource not assigned: " + gameObject.name);
        ResetGaze();
    }

    void Update()
    {
        if (_confirmed) return;
        if (gazeSource == null) return;

        Ray ray = new Ray(gazeSource.position, gazeSource.forward);
        RaycastHit hit;
        bool hitSelf = Physics.Raycast(ray, out hit) && hit.collider == _collider;

        if (hitSelf)
        {
            if (!_isGazing) { _isGazing = true; _gazeTimer = 0f; }
            _gazeTimer += Time.deltaTime;
            SetFill(Mathf.Clamp01(_gazeTimer / gazeTime));
            if (_gazeTimer >= gazeTime) Confirm();
        }
        else
        {
            if (_isGazing) ResetGaze();
        }
    }

    void Confirm()
    {
        _confirmed = true;
        SetFill(1f);
        Debug.Log("[GazeButton] Confirmed: " + gameObject.name);
        OnGazeConfirmed.Invoke();
        gameObject.SetActive(false);
    }

    void ResetGaze()
    {
        _isGazing  = false;
        _gazeTimer = 0f;
        SetFill(0f);
    }

    public void ResetButton()
    {
        _confirmed = false;
        ResetGaze();
        gameObject.SetActive(true);
    }

    void SetFill(float value)
    {
        if (gazeFillImage != null) gazeFillImage.fillAmount = value;
    }
}
