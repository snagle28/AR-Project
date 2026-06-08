using UnityEngine;


public class FilterManager : MonoBehaviour
{
    public OVRPassthroughLayer passthroughLayer;
    public Color redColor = new Color(1f, 0.2f, 0.2f, 1f);
    public Color blueColor = new Color(0.2f, 0.2f, 1f, 1f);

    [Header("Renderers to recolor")]
    public Renderer blueOnlyRenderer;
    public Renderer redOnlyRenderer;
    
    
    [Header("Cube Reset")]
    public Transform userTransform;
    public Transform[] cubesToReset;
    public float resetRadius = 2f;
    
    [Header("Cube Gameobjects to spawn")]
    public GameObject blueOnlyCube;
    public GameObject redOnlyCube;
    public GameObject neutralCube;

    //don't change these in inspector unless you need to test them (preventing clutter)
    [HideInInspector] public bool filterOn;
    [HideInInspector] public bool isRed = true;

    [HideInInspector] public bool hasTriedRedFilter;
    [HideInInspector] public bool hasTriedBlueFilter;
    public bool HasTriedBothFilters => hasTriedRedFilter && hasTriedBlueFilter;

    private Material blueOnlyMaterial;
    private Material redOnlyMaterial;

    private Color originalBlueOnlyColor;
    private Color originalRedOnlyColor;

    public hw2Skeleton interactionManager;

    //https://developer.oculus.com/documentation/unity/unity-passthrough-color-mapping
    //https://developer.oculus.com/reference/unity/latest/class_o_v_r_passthrough_layer

    protected virtual void Start()
    {
        if (passthroughLayer == null) passthroughLayer = UnityEngine.Object.FindAnyObjectByType<OVRPassthroughLayer>();
        if (userTransform == null) userTransform = Camera.main?.transform;

        //first, randomize the cube starting locations
resetCubes();
        if (blueOnlyRenderer != null)
        {
            blueOnlyMaterial = blueOnlyRenderer.material;
            originalBlueOnlyColor = blueOnlyMaterial.color;
        }

        if (redOnlyRenderer != null)
        {
            redOnlyMaterial = redOnlyRenderer.material;
            originalRedOnlyColor = redOnlyMaterial.color;
        }

        Color[] neutral = new Color[256];
        for (int i = 0; i < 256; i++)
        {
            float t = i / 255f;
            neutral[i] = new Color(t, t, t, 1f);
        }

        passthroughLayer.SetColorMap(neutral);

        ResetObjectColors();
    }

    protected virtual void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
        {
            filterOn = !filterOn;

            if (filterOn)
            {
                isRed = true;
                ApplyTint();
            }
            else
            {
                Color[] neutral = new Color[256];
                for (int i = 0; i < 256; i++)
                {
                    float t = i / 255f;
                    neutral[i] = new Color(t, t, t, 1f);
                }

                passthroughLayer.SetColorMap(neutral);
                ResetObjectColors();
            }
        }

        if (filterOn && OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch))
        {
            isRed = !isRed;
            ApplyTint();
        }

        //print(isRed);
    }

    protected virtual void ApplyTint()
    {
        Color tint = isRed ? redColor : blueColor;

        if (isRed)
        {
            hasTriedRedFilter = true;
        }
        else
        {
            hasTriedBlueFilter = true;
        }

        Color[] colorMap = new Color[256];

        for (int i = 0; i < 256; i++)
        {
            float t = i / 255f;
            colorMap[i] = new Color(tint.r * t, tint.g * t, tint.b * t, 1f);
        }

        passthroughLayer.SetColorMap(colorMap);
        ApplyObjectColors();
    }

    private void ApplyObjectColors()
    {
        if (blueOnlyMaterial != null)
        {
            blueOnlyMaterial.color = isRed ? originalBlueOnlyColor : blueColor;
        }

        if (redOnlyMaterial != null)
        {
            redOnlyMaterial.color = isRed ? redColor : originalRedOnlyColor;
        }
    }

    private void ResetObjectColors()
    {
        if (blueOnlyMaterial != null)
        {
            blueOnlyMaterial.color = originalBlueOnlyColor;
        }

        if (redOnlyMaterial != null)
        {
            redOnlyMaterial.color = originalRedOnlyColor;
        }
    }

    public void resetCubes()
    {
        if (userTransform == null || cubesToReset == null)
        {
            return;
        }

        for (int i = 0; i < cubesToReset.Length; i++)
        {
            if (cubesToReset[i] == null)
            {
                continue;
            }

            Vector2 randomCirclePosition = Random.insideUnitCircle * resetRadius;

            Vector3 newPosition = new Vector3(
                userTransform.position.x + randomCirclePosition.x,
                cubesToReset[i].position.y,
                userTransform.position.z + randomCirclePosition.y
            );

            cubesToReset[i].position = newPosition;
        }
    }
}

