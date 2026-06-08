using Oculus.Interaction;
using UnityEngine;

public class MemoryCards : MonoBehaviour
{
    [Header("Card Match ID")]
    [SerializeField] private int myID;

    private MemoryGame memoryGame;
    private RayInteractable rayInteractable;
    public Animator myAnimator;

    private bool faceUp = false;
    private bool isMatched = false;

    public bool FaceUp
    {
        get { return faceUp; }
    }

    public bool IsMatched
    {
        get { return isMatched; }
    }

    private void Awake()
    {
        if (rayInteractable == null)
        {
            rayInteractable = GetComponentInChildren<RayInteractable>(false);
        }

        if (myAnimator == null)
        {
            myAnimator = GetComponentInChildren<Animator>();
        }

        memoryGame = GetComponentInParent<MemoryGame>();
    }

    //get reference to memory game script
    public void GetMemoryScript(MemoryGame script)
    {
        memoryGame = script;
    }

    //STUFF FROM HW 2 BASE SCRIPT
    private void OnEnable()
    {
        if (rayInteractable != null)
        {
            rayInteractable.WhenStateChanged += OnRayStateChanged;
        }
    }

    private void OnDisable()
    {
        if (rayInteractable != null)
        {
            rayInteractable.WhenStateChanged -= OnRayStateChanged;
        }
    }

    private void OnRayStateChanged(InteractableStateChangeArgs args)
    {
        if (args.NewState == InteractableState.Hover)
        {
            print("hovering");
        }
        else if (args.NewState == InteractableState.Select)
        {
            print("selecting");
            SelectThisCard();
        }
        else
        {
            //idk
        }
    }

    private void SelectThisCard()
    {
        if (isMatched || faceUp)
        {
            return;
        }
        else if (memoryGame != null)
        {
            TriggerHaptic();
            memoryGame.CardSelected(this);
        }
    }

    private void TriggerHaptic()
    {
        // Simple haptic buzz for VR controllers
        StartCoroutine(HapticRoutine(0.1f, 0.1f, 0.1f));
    }

    private System.Collections.IEnumerator HapticRoutine(float duration, float frequency, float amplitude)
    {
        OVRInput.SetControllerVibration(frequency, amplitude, OVRInput.Controller.Active);
        yield return new WaitForSeconds(duration);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.Active);
    }

    public void FlipUp()
    {
        faceUp = true;

        if (myAnimator != null)
        {
            myAnimator.SetTrigger("ReadyToFlipUp");
        }
    }

    public void FlipDown()
    {
        faceUp = false;

        if (myAnimator != null)
        {
            myAnimator.SetTrigger("ReadyToFlipDown");
        }
    }

    public bool CheckIfMatches(MemoryCards otherCard)
    {
        if (otherCard == null)
        {
            return false;
        }

        return myID == otherCard.myID;
    }

    public void Match()
    {
        isMatched = true;
        faceUp = true;

        if (memoryGame != null)
        {
            memoryGame.cardsList.Remove(this);
        }
    }

    public void ifNotMatchedFlipDown()
    {
        foreach (MemoryCards card in memoryGame.cardsList)
        {
            if (card.myID == myID)
            {
                card.FlipDown();
                FlipDown();
                isMatched = false;
                return;
            }
        }
    }
    //leave face up once matched
}
