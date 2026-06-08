using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PLYAnamorphosisMeshGenerator : MonoBehaviour
{
    [Header("Core References")]
    public Transform ahaPointView;
    public TextAsset plyFile;

    [Header("Object Settings")]
    public float objectScale = 0.05f;
    public int maxPoints = 3000;

    [Header("Smart Sampling")]
    public bool useGridSampling = true;
    public float gridSize = 0.03f;

    [Header("Anamorphosis Depth")]
    public float minDepth = 2.0f;
    public float maxDepth = 2.01f;

    [Header("Point Rendering")]
    public float pointSize = 0.006f;
    public Transform billboardCamera;

    [Header("Options")]
    public bool centerObject = true;
    public bool generateOnStart = false;

    private Mesh mesh;

    void Start()
    {
        if (generateOnStart)
        {
            GenerateFromPLY();
        }
    }

    [ContextMenu("Generate From PLY Mesh")]
    public void GenerateFromPLY()
    {
        if (ahaPointView == null || plyFile == null)
        {
            Debug.LogError("AhaPointView??? PLY File??? ???????????? ?????????.");
            return;
        }

        List<Vector3> originalPoints = ReadAsciiPLY(plyFile.text);

        if (originalPoints.Count == 0)
        {
            Debug.LogError("PLY?????? ???????????? ?????? ???????????????.");
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

        List<Vector3> finalPoints = new List<Vector3>();

        foreach (Vector3 p in originalPoints)
        {
            Vector3 localPoint = p * objectScale;
            Vector3 worldOriginalPoint = transform.TransformPoint(localPoint);

            Vector3 direction = worldOriginalPoint - ahaPointView.position;

            if (direction.sqrMagnitude < 0.0001f)
                continue;

            direction.Normalize();

            float depth = Random.Range(minDepth, maxDepth);
            Vector3 worldFinalPosition = ahaPointView.position + direction * depth;

            Vector3 localFinalPosition = transform.InverseTransformPoint(worldFinalPosition);
            finalPoints.Add(localFinalPosition);
        }

        CreateQuadMesh(finalPoints);

        // ?????? ?????? ?????? ??????
        var mf = GetComponent<MeshFilter>();
        if (mf != null && mf.sharedMesh != null)
        {
            var b = mf.sharedMesh.bounds;
            Debug.Log($"[PLYGen] {gameObject.name}: points={finalPoints.Count}" +
                      $" meshBounds center={b.center} size={b.size}" +
                      $" ahaView={ahaPointView?.position}");
        }
        else
        {
            Debug.LogWarning($"[PLYGen] {gameObject.name}: mesh generation failed ??? MeshFilter empty");
        }
    }

    void CreateQuadMesh(List<Vector3> points)
    {
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        float half = pointSize * 0.5f;

        // AhaPointView ???????????? quad??? orient.
        // billboardCamera(HMD ??????)??? ???????????? ????????????
        // generateOnStart=true ????????? ?????? ????????? ????????? ?????????.
        Vector3 toView = Vector3.forward;
        if (ahaPointView != null)
        {
            Vector3 dir = (ahaPointView.position - transform.position);
            if (dir.sqrMagnitude > 0.0001f)
                toView = dir.normalized;
        }

        // toView ??? ????????? right / up ??????
        Vector3 worldRight = Vector3.Cross(Vector3.up, toView).normalized;
        if (worldRight.sqrMagnitude < 0.001f)          // toView ??? world-up ??? ??? fallback
            worldRight = Vector3.right;
        Vector3 worldUp = Vector3.Cross(toView, worldRight).normalized;

        // mesh ????????? 0 ????????? local == world
        Vector3 localRight = transform.InverseTransformDirection(worldRight).normalized;
        Vector3 localUp    = transform.InverseTransformDirection(worldUp).normalized;

        for (int i = 0; i < points.Count; i++)
        {
            Vector3 p = points[i];

            int start = vertices.Count;

            vertices.Add(p + (-localRight - localUp) * half);
            vertices.Add(p + ( localRight - localUp) * half);
            vertices.Add(p + (-localRight + localUp) * half);
            vertices.Add(p + ( localRight + localUp) * half);

            triangles.Add(start + 0);
            triangles.Add(start + 2);
            triangles.Add(start + 1);

            triangles.Add(start + 2);
            triangles.Add(start + 3);
            triangles.Add(start + 1);
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().sharedMesh = mesh;
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
}