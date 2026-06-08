using UnityEngine;

public class Grid_Layout : MonoBehaviour
{

    public int columns = 5;

    public float cellWidth = 0.15f;
    public float cellHeight = 0.2f;
    public float spacing = 0.2f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ArrangeChildren();
    }

    // Update is called once per frame
    void ArrangeChildren()
    {
        float i = 0;
        float totalWidth = columns * (cellWidth + spacing) - spacing;
        float totalHeight = Mathf.Ceil((float)transform.childCount / columns) * (cellHeight + spacing) - spacing;

        foreach (Transform child in transform)
        {
            var row = (int)i / columns;
            var col = (int)i % columns;

            child.localPosition = new Vector3(
                col * (cellWidth + spacing) - totalWidth / 2f,
                -row * (cellHeight + spacing) - totalHeight / 2f,
                0f
            );
            i++;
        }
    }
}
