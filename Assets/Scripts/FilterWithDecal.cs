using UnityEngine;

public class FilterWithDecal : FilterManager
{
    private Renderer objectRenderer;
    private Material objectMaterial;
    private Color originalColor;
//new code
    protected override void Start()
    {
        base.Start();

        objectRenderer = GetComponent<Renderer>();
        objectMaterial = objectRenderer.material;
        originalColor = objectMaterial.color;
    }

    protected override void ApplyTint()
    {
        base.ApplyTint();

        objectMaterial.color = isRed ? redColor : blueColor;
    }

    protected override void Update()
    {
        base.Update();

        if (!filterOn)
        {
            objectMaterial.color = originalColor;
        }
    }
}
