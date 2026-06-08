using Oculus.Interaction;
using UnityEngine;

public class WrongCubeSelection : hw2Base
{
    public bool RightCubeSelected;
    public hw2Skeleton rightCubeVariable;
    [SerializeField] private FilterManager filterManager;

    protected override void OnRayStateChanged(InteractableStateChangeArgs args)
    {
        if (args.NewState == InteractableState.Select && filterManager.HasTriedBothFilters)
        {
            rightCubeVariable.RightCubeSelected = false;
            print("Wrong Cube Selected");
            print("resetting cubes and variables");
            filterManager.resetCubes();
        }
    }
    

    protected override void ApplyTwoHandScale(float currentDist)
    {
        // Do nothing so this cube cannot be scaled.
    }
}
