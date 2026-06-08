using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class PLYAnamorphosisGenerator : MonoBehaviour
{
    [Header("Core References")]
    public Transform ahaPointView;
    public Transform pointsParent;
    public GameObject pointPrefab;

    [Header("PLY File")]
    public TextAsset plyFile;

    [Header("Object Settings")]
    public float objectScale = 0.05f;
    public int maxPoints = 1200;

    [Header("Smart Sampling")]
    public bool useGridSampling = true;
    public float gridSize = 0.04f;

    [Header("Anamorphosis Depth")]
    public float minDepth = 2.0f;
    public float maxDepth = 2.01f;

    [Header("Options")]
    public bool centerObject = true;
    public bool generateOnStart = false;

    void Start()
    {
        if (generateOnStart)
        {
            GenerateFromPLY();
        }
    }

    [ContextMenu("Generate From PLY")]
    public void GenerateFromPLY()
    {
        if (ahaPointView == null || pointsParent == null || pointPrefab == null || plyFile == null)
        {
            Debug.LogError("AhaPointView, PointsParent, PointPrefab, PLY File을 모두 연결해야 합니다.");
            return;
        }

        ClearPoints();

        List<Vector3> originalPoints = ReadAsciiPLY(plyFile.text);

        if (originalPoints.Count == 0)
        {
            Debug.LogError("PLY에서 포인트를 읽지 못했습니다.");
            return;
        }

        if (centerObject)
        {
            originalPoints = CenterPoints(originalPoints);
        }

        if (useGridSampling)
        {
            originalPoints = GridSamplePoints(originalPoints, gridSize);
        }

        if (originalPoints.Count > maxPoints)
        {
            originalPoints = RandomSamplePoints(originalPoints, maxPoints);
        }

        int created = 0;

        foreach (Vector3 p in originalPoints)
        {
            Vector3 localPoint = p * objectScale;
            Vector3 worldOriginalPoint = transform.TransformPoint(localPoint);

            Vector3 direction = worldOriginalPoint - ahaPointView.position;

            if (direction.sqrMagnitude < 0.0001f)
                continue;

            direction.Normalize();

            float depth = Random.Range(minDepth, maxDepth);
            Vector3 finalPosition = ahaPointView.position + direction * depth;

            GameObject point = Instantiate(pointPrefab, finalPosition, Quaternion.identity, pointsParent);
            point.name = "PLY_Point_" + created;

            created++;
        }

        Debug.Log("Generated PLY anamorphic points: " + created);
    }

    List<Vector3> ReadAsciiPLY(string text)
    {
        List<Vector3> points = new List<Vector3>();

        StringReader reader = new StringReader(text);
        string line;
        bool headerEnded = false;

        while ((line = reader.ReadLine()) != null)
        {
            if (!headerEnded)
            {
                if (line.StartsWith("end_header"))
                {
                    headerEnded = true;
                }
                continue;
            }

            string[] parts = line.Trim().Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 3)
                continue;

            if (
                float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
                float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float z)
            )
            {
                points.Add(new Vector3(x, y, z));
            }
        }

        return points;
    }

    List<Vector3> CenterPoints(List<Vector3> points)
    {
        Vector3 sum = Vector3.zero;

        foreach (Vector3 p in points)
        {
            sum += p;
        }

        Vector3 center = sum / points.Count;

        List<Vector3> centered = new List<Vector3>();

        foreach (Vector3 p in points)
        {
            centered.Add(p - center);
        }

        return centered;
    }

    List<Vector3> GridSamplePoints(List<Vector3> points, float size)
    {
        Dictionary<Vector3Int, Vector3> grid = new Dictionary<Vector3Int, Vector3>();

        foreach (Vector3 p in points)
        {
            Vector3Int cell = new Vector3Int(
                Mathf.FloorToInt(p.x / size),
                Mathf.FloorToInt(p.y / size),
                Mathf.FloorToInt(p.z / size)
            );

            if (!grid.ContainsKey(cell))
            {
                grid.Add(cell, p);
            }
        }

        return new List<Vector3>(grid.Values);
    }

    List<Vector3> RandomSamplePoints(List<Vector3> points, int count)
    {
        List<Vector3> copy = new List<Vector3>(points);
        List<Vector3> sampled = new List<Vector3>();

        for (int i = 0; i < count && copy.Count > 0; i++)
        {
            int index = Random.Range(0, copy.Count);
            sampled.Add(copy[index]);
            copy.RemoveAt(index);
        }

        return sampled;
    }

    void ClearPoints()
    {
        for (int i = pointsParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(pointsParent.GetChild(i).gameObject);
        }
    }
}