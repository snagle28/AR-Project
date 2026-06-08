using UnityEngine;

public class ViewDependentPointObject : MonoBehaviour
{
    [Header("Core References")]
    public Transform ahaPoint;
    public Transform pointsParent;
    public GameObject pointPrefab;

    [Header("Original Object Shape")]
    public int pointCount = 800;
    public float objectScale = 0.8f;

    [Header("View-Dependent Reconstruction")]
    public float minDepth = 1.0f;
    public float maxDepth = 4.0f;

    [Header("Shape Type")]
    public ShapeType shapeType = ShapeType.Cube;

    public enum ShapeType
    {
        Cube,
        Sphere,
        Ring
    }

    void Start()
    {
        Generate();
    }

    [ContextMenu("Generate View-Dependent Object")]
    public void Generate()
    {
        if (ahaPoint == null)
        {
            Debug.LogError("AhaPoint가 연결되지 않았습니다.");
            return;
        }

        if (pointsParent == null)
        {
            Debug.LogError("Points Parent가 연결되지 않았습니다.");
            return;
        }

        if (pointPrefab == null)
        {
            Debug.LogError("Point Prefab이 연결되지 않았습니다.");
            return;
        }

        ClearPoints();

        for (int i = 0; i < pointCount; i++)
        {
            Vector3 originalPoint = GetOriginalShapePoint();

            Vector3 worldOriginalPoint = transform.TransformPoint(originalPoint * objectScale);

            Vector3 directionFromAha = worldOriginalPoint - ahaPoint.position;

            if (directionFromAha.sqrMagnitude < 0.0001f)
                continue;

            directionFromAha.Normalize();

            float depth = Random.Range(minDepth, maxDepth);

            Vector3 finalPointPosition = ahaPoint.position + directionFromAha * depth;

            GameObject point = Instantiate(pointPrefab, finalPointPosition, Quaternion.identity, pointsParent);
            point.name = "ViewDependentPoint_" + i;
        }
    }

    Vector3 GetOriginalShapePoint()
    {
        switch (shapeType)
        {
            case ShapeType.Sphere:
                return Random.onUnitSphere;

            case ShapeType.Ring:
                float angle = Random.Range(0f, Mathf.PI * 2f);
                float radius = Random.Range(0.35f, 0.5f);
                return new Vector3(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius,
                    0f
                );

            case ShapeType.Cube:
            default:
                return GetCubeSurfacePoint();
        }
    }

    Vector3 GetCubeSurfacePoint()
    {
        Vector3 p = new Vector3(
            Random.Range(-0.5f, 0.5f),
            Random.Range(-0.5f, 0.5f),
            Random.Range(-0.5f, 0.5f)
        );

        int face = Random.Range(0, 6);

        switch (face)
        {
            case 0: p.x = -0.5f; break;
            case 1: p.x = 0.5f; break;
            case 2: p.y = -0.5f; break;
            case 3: p.y = 0.5f; break;
            case 4: p.z = -0.5f; break;
            case 5: p.z = 0.5f; break;
        }

        return p;
    }

    void ClearPoints()
    {
        for (int i = pointsParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(pointsParent.GetChild(i).gameObject);
        }
    }
}