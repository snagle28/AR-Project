using Meta.XR.BuildingBlocks.AIBlocks;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static UnityEvent puzzleCompleted = new UnityEvent();
    [SerializeField] GameObject memoryPuzzle;
    [SerializeField] GameObject filterPuzzle;
    [SerializeField] GameObject splatPuzzle;
    [SerializeField] GameObject objectDetectionPuzzle;

    GameObject puzzle;

    int selection;

    // singleton bs
    private static GameManager Manager;
    public static GameManager Instance { get { return Manager; } }
    private void Awake()
    {
        if (Manager != null && Manager != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Manager = this;
        }

    }

    private void OnDestroy()
    {
        if (Manager == this)
        {
            puzzleCompleted.RemoveAllListeners();
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        puzzleCompleted.AddListener(onPuzzleCompleted);
        //selection = Random.Range(0, 4);
        selection = 3;

        Debug.Log("GameManager: Started. selection=" + selection);

        switch (selection)
        {
            case 0:
                puzzle = Instantiate(memoryPuzzle);
                break;
            case 1:
                puzzle = Instantiate(filterPuzzle);
                break;
            case 2:
                puzzle = Instantiate(splatPuzzle);
                break;
            case 3:
                puzzle = objectDetectionPuzzle;
                if (puzzle != null)
                {
                    var visualizer = puzzle.GetComponent<EditedObjectDetectionVisualizer>();
                    if (visualizer != null) visualizer.isActive = true;
                }
                break;
        }
        
        if (puzzle != null)
        {
            Debug.Log("GameManager: Active puzzle is " + puzzle.name);
        }
    }

    void onPuzzleCompleted()
    {
        Debug.Log("GameManager: onPuzzleCompleted triggered!");
        if (puzzle != null)
        {
            Debug.Log("GameManager: Disabling puzzle " + puzzle.name);
            
            // Specifically handle Object Detection Visualizer if it exists
            var visualizer = puzzle.GetComponent<EditedObjectDetectionVisualizer>();
            if (visualizer != null)
            {
                visualizer.isActive = false;
                visualizer.ShowBoundingBoxes = false;
                visualizer.textcan.SetActive(false);
            }

            puzzle.SetActive(false);
        }
        else
        {
            Debug.LogWarning("GameManager: onPuzzleCompleted called but current puzzle is null!");
            // Safety: Try to find the object detection puzzle anyway if the event fired
            if (objectDetectionPuzzle != null)
            {
                objectDetectionPuzzle.SetActive(false);
            }
        }
    }
}
