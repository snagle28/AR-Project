// ============================================================
//  hw2_script.cs  �  YOUR FILE
//
//  Fill in the two methods below.
//  All setup code is handled by hw2Base.cs � you do not need to read it.
// ============================================================

using UnityEngine;
using Oculus.Interaction;

public class raySelect : hwBase2
{
    public string name;
    // ================================================================
    //  TODO #1 � Change the object color based on the ray state
    // ================================================================
    //
    //  This method is called automatically whenever the ray state changes.
    //
    //  Variables to use:
    //    args.NewState                (InteractableStateChangeArgs) current ray state
    //    InteractableState.Hover      constant � ray is pointing at this object
    //    InteractableState.Select     constant � ray trigger is held down
    //    hoverColor                   (Color) color to apply on Hover
    //    selectColor                  (Color) color to apply on Select
    //    m_OriginalColor              (Color) original color � restore this on any other state
    //    m_Renderer.material.color    (Color) set this to actually change the visible color
    //
    //  Structure:
    //    if (args.NewState == InteractableState.Hover)
    //        m_Renderer.material.color = ???;
    //    else if (...)
    //        ...
    //    else
    //        ...
    //
    //  Ref: https://docs.unity3d.com/ScriptReference/Material-color.html
    //  Ref: https://developer.oculus.com/documentation/unity/unity-isdk-interactable-state/
    // ================================================================
    protected override void OnRayStateChanged(InteractableStateChangeArgs args)
    {
        if (args.NewState == InteractableState.Hover)
        {
            //m_Renderer.material.color = hoverColor;
        }
        else if (args.NewState == InteractableState.Select)
        {
            //m_Renderer.material.color = selectColor;
            Debug.Log("raySelect: " + gameObject.name + " SELECT state reached. Invoking GameManager.puzzleCompleted.");
            //if ()
            GameManager.puzzleCompleted.Invoke();
        }
        else
        {
            //m_Renderer.material.color = m_OriginalColor;
        }
    }

    // ================================================================
    //  TODO #2 � Scale the object based on two-hand distance
    // ================================================================
    //
    //  This method is called every frame while two hands are grabbing.
    //
    //  Variables to use:
    //    currentDist           (float)   current distance between the two hands  [parameter]
    //    m_InitialHandDistance (float)   hand distance at the moment grab started [read only]
    //    m_InitialScale        (Vector3) object scale at the moment grab started  [read only]
    //    minScale              (float)   minimum allowed scale (Inspector)        [read only]
    //    maxScale              (float)   maximum allowed scale (Inspector)        [read only]
    //    m_SavedScale          (Vector3) write your final scale result here       [write here]
    //
    //  Core idea:
    //    Hands twice as far apart as at start ? object should be twice as big.
    //    Hands half as far apart as at start  ? object should be half as big.
    //
    //  Structure:
    //    ? ratio    = currentDist / m_InitialHandDistance
    //    ? rawScale = m_InitialScale.x * ratio
    //    ? newScale = Mathf.Clamp(rawScale, minScale, maxScale)
    //    ? m_SavedScale = Vector3.one * newScale
    //
    //  Note: write to m_SavedScale, NOT transform.localScale directly.
    //
    //  Ref: https://docs.unity3d.com/ScriptReference/Mathf.Clamp.html
    //  Ref: https://docs.unity3d.com/ScriptReference/Vector3-one.html
    // ================================================================
    protected override void ApplyTwoHandScale(float currentDist)
    {
        // do nothing rn!!
        //float ratio = currentDist / m_InitialHandDistance;
        //float rawScale = m_InitialScale.x * ratio;
        //float newScale = Mathf.Clamp(rawScale, minScale, maxScale);
        //m_SavedScale = Vector3.one * newScale;
        //Debug.Log(m_SavedScale);
    }
}
