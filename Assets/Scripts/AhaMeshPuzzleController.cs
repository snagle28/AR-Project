using UnityEngine;

public class AhaMeshPuzzleController : MonoBehaviour
{
    [Header("User")]
    public Transform userHead;

    [Header("Aha Point")]
    public Transform ahaPointFloorMarker;
    public float triggerDistance = 0.4f;

    [Header("Puzzle State")]
    public bool isSolved = false;

    [Header("Feedback")]
    public GameObject solvedEffectObject;

    void Update()
    {
        if (isSolved) return;
        if (userHead == null || ahaPointFloorMarker == null) return;

        Vector2 userXZ = new Vector2(userHead.position.x, userHead.position.z);
        Vector2 ahaXZ = new Vector2(ahaPointFloorMarker.position.x, ahaPointFloorMarker.position.z);

        float distance = Vector2.Distance(userXZ, ahaXZ);

        if (distance <= triggerDistance)
        {
            Solve();
        }
    }

    void Solve()
    {
        isSolved = true;

        Debug.Log(gameObject.name + " SOLVED");

        if (solvedEffectObject != null)
        {
            solvedEffectObject.SetActive(true);
        }
    }
}