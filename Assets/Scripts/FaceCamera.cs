using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, 0.8f); // 0.8 meters in front

    private Transform mainCameraTransform;

    void Start()
    {
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
            // Initialize position to avoid snapping at start
            transform.position = mainCameraTransform.position + mainCameraTransform.rotation * offset;
            transform.rotation = Quaternion.LookRotation(transform.position - mainCameraTransform.position);
        }
    }

    void LateUpdate()
    {
        if (mainCameraTransform != null)
        {
            // Target position: camera pos + rotated offset
            Vector3 targetPosition = mainCameraTransform.position + mainCameraTransform.rotation * offset;
            
            // Smoothly move towards target position
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
            
            // Face the camera smoothly
            Quaternion targetRotation = Quaternion.LookRotation(transform.position - mainCameraTransform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * followSpeed);
        }
    }
}
