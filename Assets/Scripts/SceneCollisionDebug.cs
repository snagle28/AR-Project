using UnityEngine;
using Meta.XR.MRUtilityKit;
using TMPro;
using Meta.XR;

public class SceneCollisionDebug : MonoBehaviour
{
    public Transform rayStartPoint;
    public float rayLength = 5;
    public EnvironmentRaycastManager envRayManager;
    //public TMPro.TextMeshPro debugText;
    public OVRHand hand;

    public GameObject memoryGameObject;
    public float surfaceOffset = 0.02f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = new Ray(rayStartPoint.position, rayStartPoint.forward);
        bool hasHit = envRayManager.Raycast(ray, out var hit, rayLength);

        if (hasHit)
        {
            Vector3 hitPoint = hit.point;
            Vector3 hitNormal = hit.normal;

            if (OVRInput.GetDown(OVRInput.Button.One) || hand.GetFingerIsPinching(OVRHand.HandFinger.Index))
            {
                MoveMemoryGame(hitPoint, hitNormal);
            }

            //Vector3 directionToCamera = Camera.main.transform.position - hitPoint;

            //debugText.transform.position = hitPoint;
            //debugText.transform.rotation = Quaternion.LookRotation(directionToCamera, Vector3.up) * Quaternion.Euler(0f, 180f, 0f);

            //debugText.text = "ENV HIT";
        }
    }

    private void MoveMemoryGame(Vector3 hitPoint, Vector3 hitNormal)
    {
        if (memoryGameObject == null)
        {
            Debug.LogWarning("Memory Game Object is not assigned.");
            return;
        }

        Vector3 targetPosition = hitPoint + hitNormal * surfaceOffset;
        Quaternion targetRotation = Quaternion.LookRotation(-hitNormal, Vector3.up);

        memoryGameObject.transform.SetPositionAndRotation(targetPosition, targetRotation);
        memoryGameObject.SetActive(true);
    }
}
