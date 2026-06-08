using UnityEngine;
using System.Collections.Generic;

public class PuzzleSpawner : MonoBehaviour
{
    [Header("Puzzle Configuration")]
    [SerializeField] private List<GameObject> puzzlePrefabs;
    [SerializeField] private float spawnDistance = 1.2f;
    [SerializeField] private bool spawnOnStart = true;

    [Header("References (Optional)")]
    [SerializeField] private Transform playerCamera;

    private void Start()
    {
        if (playerCamera == null)
        {
            if (Camera.main != null)
            {
                playerCamera = Camera.main.transform;
            }
        }

        if (spawnOnStart)
        {
            SpawnRandomPuzzle();
        }
    }

    public void SpawnRandomPuzzle()
    {
        if (puzzlePrefabs == null || puzzlePrefabs.Count == 0)
        {
            Debug.LogWarning("PuzzleSpawner: No prefabs assigned.");
            return;
        }

        if (playerCamera == null)
        {
            Debug.LogError("PuzzleSpawner: No camera found to determine eye height.");
            return;
        }

        // Select random puzzle
        int index = Random.Range(0, puzzlePrefabs.Count);
        GameObject prefab = puzzlePrefabs[index];

        if (prefab == null) return;

        // Calculate spawn position at eye height
        // We take the current camera position (eye height) and move it forward
        Vector3 spawnPos = playerCamera.position + playerCamera.forward * spawnDistance;

        // Calculate rotation to face the player, but keep it upright
        Vector3 lookDir = playerCamera.position - spawnPos;
        lookDir.y = 0; // Keep horizontal
        Quaternion spawnRot = Quaternion.identity;
        
        if (lookDir != Vector3.zero)
        {
            spawnRot = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
        }

        // Instantiate
        GameObject spawnedPuzzle = Instantiate(prefab, spawnPos, spawnRot);
        
        Debug.Log($"Spawned {spawnedPuzzle.name} at {spawnPos}");
    }
}
