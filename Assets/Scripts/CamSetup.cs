using UnityEngine;

public class CamSetup : MonoBehaviour
{
    public OVRPassthroughLayer passthroughLayer;

    void Start()
    {
        if (passthroughLayer != null)
            passthroughLayer.hidden = false;
    }
}