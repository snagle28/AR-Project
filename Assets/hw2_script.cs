//remember: don't worry about base script

using UnityEngine;
using Oculus.Interaction;

public class hw2Skeleton : hw2Base
{
    public bool RightCubeSelected;
    [SerializeField] private FilterManager filterManager;

    protected override void OnRayStateChanged(InteractableStateChangeArgs args)
    {
        if (args.NewState == InteractableState.Select)
        {
            RightCubeSelected = true;
            if (filterManager != null)
            {
                filterManager.ShowSuccessPanel();
            }
        }
    }

    protected override void ApplyTwoHandScale(float currentDist)
    {
        // Do nothing so this cube cannot be scaled.
    }
}
