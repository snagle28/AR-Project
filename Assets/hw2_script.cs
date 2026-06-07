//remember: don't worry about base script

using UnityEngine;
using Oculus.Interaction;

public class hw2Skeleton : hw2Base
{
    public bool RightCubeSelected;
    public static event System.Action OnPuzzleSolved;

    protected override void OnRayStateChanged(InteractableStateChangeArgs args)
    {
        if (args.NewState == InteractableState.Select)
        {
            if (!RightCubeSelected)
            {
                RightCubeSelected = true;
                OnPuzzleSolved?.Invoke();
            }
        }
    }

    protected override void ApplyTwoHandScale(float currentDist)
    {
        // Do nothing so this cube cannot be scaled.
    }
}
