using UnityEngine;

public class SelectCube : MonoBehaviour
{
    public bool RightCubeSelected;

    private void OnMouseDown()
    {
        RightCubeSelected = true;
    }
}
