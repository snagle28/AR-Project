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
        // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        puzzleCompleted.AddListener(onPuzzleCompleted);
        int selection = Random.Range(0, 3);

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
                puzzle = Instantiate(objectDetectionPuzzle);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void onPuzzleCompleted()
    {
        Debug.Log("GUH");
        puzzle.SetActive(false);
        //objectDetection.SetActive(false);
    }
}
