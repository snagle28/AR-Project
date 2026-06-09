using System;
using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKitSamples.EnvironmentPanelPlacement;
using UnityEngine;
using UnityEngine.InputSystem;

public class MemoryGame : MonoBehaviour
{
    private bool _gameReady = false;
    //private bool firstCardFlipped = false;
    public List<MemoryCards> cardsList = new List<MemoryCards>();
    public List<MemoryCards> matchedCards = new List<MemoryCards>();

    private bool checkingCards = false;
    private bool firstFlipDone = false;
    private int totalMatchedCards = 0;

    public int delayTime = 10;
    public float gameTime = 60f;
    [SerializeField] private MemoryGameUI _gameUI;
    [SerializeField] private EnvironmentPanelPlacement _panelPlacement;

    [Header("UI Panels")]
    public GameObject successPanel;
    public GameObject failPanel;

    [Header("Testing")]
    [SerializeField] private bool forceWin = false;
    [SerializeField] private bool forceFail = false;
    [SerializeField] private bool forceStart = false;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip bubblePopSound;
    [SerializeField] private AudioClip cardsMatchedSound;
    [SerializeField] private AudioClip countdownSound;
    [SerializeField] private AudioClip puzzleWinSound;

    private int _initialCardCount;
    private float _timer;
    private bool _isGameOver = false;
    private Transform _cameraTransform;

    void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        if (_panelPlacement == null) _panelPlacement = GetComponentInParent<EnvironmentPanelPlacement>();
        if (_gameUI == null) _gameUI = GetComponentInChildren<MemoryGameUI>();

        _initialCardCount = cardsList.Count;
        _timer = gameTime;

        // Find camera for panel positioning
        OVRCameraRig rig = FindAnyObjectByType<OVRCameraRig>();
        if (rig != null)
        {
            _cameraTransform = rig.centerEyeAnchor;
        }
        else
        {
            Camera mainCam = Camera.main;
            if (mainCam != null) _cameraTransform = mainCam.transform;
        }

        foreach (MemoryCards card in cardsList)
        {
            card.GetMemoryScript(this);
        }

        if (_gameUI != null)
        {
            _gameUI.SetText("Please click the bar above the set of cards. Use it to drag the cards onto a wall near you.");
        }

        // Wait for wall snap instead of starting immediately
        if (_panelPlacement != null)
        {
            _panelPlacement.OnFirstWallSnap += HandleFirstWallSnap;
        }
        else
        {
            // If no placement logic, maybe start immediately or log warning
            Debug.LogWarning("No EnvironmentPanelPlacement found. Memory Game will not start waiting for wall snap.");
        }
    }

    void Update()
    {
        // Testing toggles - moved outside of _gameReady to allow testing in simulator
        // Also added keyboard shortcuts (W for Win, L for Loss, S for Start) for easier use in simulator
        bool winPressed = Keyboard.current != null && Keyboard.current.wKey.wasPressedThisFrame;
        bool lossPressed = Keyboard.current != null && Keyboard.current.lKey.wasPressedThisFrame;
        bool startPressed = Keyboard.current != null && Keyboard.current.sKey.wasPressedThisFrame;

        if (forceWin || winPressed)
        {
            forceWin = false;
            StartCoroutine(WinRoutine());
            return;
        }
        if (forceFail || lossPressed)
        {
            forceFail = false;
            HandleFailure();
            return;
        }
        if (forceStart || startPressed)
        {
            forceStart = false;
            HandleFirstWallSnap();
            return;
        }

        if (_gameReady && !_isGameOver)
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0)
            {
                _timer = 0;
                HandleFailure();
            }
            
            if (_gameUI != null)
            {
                _gameUI.SetText("You have: " + Mathf.CeilToInt(_timer) + " seconds remaining to complete the puzzle");
            }
        }
    }

    private void ShuffleCards()
    {
        if (cardsList == null || cardsList.Count == 0) return;

        // Collect all current local positions
        List<Vector3> positions = new List<Vector3>();
        foreach (MemoryCards card in cardsList)
        {
            positions.Add(card.transform.localPosition);
        }

        // Shuffle the positions list
        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 temp = positions[i];
            int randomIndex = UnityEngine.Random.Range(i, positions.Count);
            positions[i] = positions[randomIndex];
            positions[randomIndex] = temp;
        }

        // Assign shuffled positions back to cards
        for (int i = 0; i < cardsList.Count; i++)
        {
            cardsList[i].transform.localPosition = positions[i];
        }
        
        Debug.Log("Cards shuffled.");
    }

    private void PlaySound(AudioClip clip)
    {
if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void HandleFirstWallSnap()
    {
        if (_panelPlacement != null)
            _panelPlacement.OnFirstWallSnap -= HandleFirstWallSnap; // unsubscribe — one-shot

        if (_isGameOver) return;

        ShuffleCards();
        PlaySound(bubblePopSound); // Play bubble pop when snapped
        StartCoroutine(InitialDelay());
    }

    public void CardSelected(MemoryCards selectedCard)
    {
        if (!_gameReady || checkingCards || selectedCard == null || _isGameOver)
        {
            return;
        }

        selectedCard.FlipUp();
        matchedCards.Add(selectedCard);

        if (matchedCards.Count == 2)
        {
            StartCoroutine(CheckSelectedCards());
        }
    }

    private IEnumerator InitialDelay()
    {
        int countdown = delayTime;
        while (countdown > 0)
        {
            if (_gameUI != null)
            {
                _gameUI.SetText("Well done! Beginning game in " + countdown);
            }
            PlaySound(countdownSound);
            yield return new WaitForSeconds(1f);
            countdown--;
        }
        
        if (_gameUI != null)
        {
            _gameUI.SetText(""); // Clear text or hide UI
        }
        
        yield return StartCoroutine(StartingFlip());
    }
    
    private IEnumerator StartingFlip()
    {
        foreach (MemoryCards card in cardsList)
        {
            card.myAnimator.SetTrigger("ReadyToFlipUp");
        }

        yield return new WaitForSeconds(2f);

        foreach (MemoryCards card in cardsList)
        {
            card.myAnimator.SetTrigger("ReadyToFlipDown");
        }

        firstFlipDone = true;
        _gameReady = true; // only now can players interact
    }

    private IEnumerator CheckSelectedCards()
    {
        checkingCards = true;
        
        yield return new WaitForSeconds(2f);

        if (_isGameOver) yield break;

        //declare these to shorten things so i dont get confused
        MemoryCards firstCard = matchedCards[0];
        MemoryCards secondCard = matchedCards[1];

        if (firstCard.CheckIfMatches(secondCard))
        {
            firstCard.Match();
            secondCard.Match();

            PlaySound(cardsMatchedSound);

            totalMatchedCards += 2;

            if (totalMatchedCards >= _initialCardCount)
            {
                Debug.Log("all cards matched");
                StartCoroutine(WinRoutine());
            }
        }
        else
        {
            firstCard.FlipDown();
            secondCard.FlipDown();
        }

        matchedCards.Clear();
        checkingCards = false;
    }

    private IEnumerator WinRoutine()
    {
        if (_isGameOver) yield break;
        _isGameOver = true;
        _gameReady = false;

        yield return new WaitForSeconds(1f);
        PlaySound(puzzleWinSound);

        if (successPanel != null)
        {
            PositionPanelInFront(successPanel);
            successPanel.SetActive(true);
        }
    }

    private void HandleFailure()
    {
        if (_isGameOver) return;
        _isGameOver = true;
        _gameReady = false;

        if (failPanel != null)
        {
            PositionPanelInFront(failPanel);
            failPanel.SetActive(true);
        }
        
        if (_gameUI != null)
        {
            _gameUI.SetText("Time is up! Puzzle failed.");
        }
    }

    private void PositionPanelInFront(GameObject panel, float distance = 1.2f)
    {
        if (panel == null || _cameraTransform == null) return;

        Vector3 camPos = _cameraTransform.position;
        Vector3 fwd = _cameraTransform.forward; 
        fwd.y = 0f;
        if (fwd == Vector3.zero) fwd = Vector3.forward; 
        else fwd.Normalize();

        Vector3 panelPos = camPos + fwd * distance;
        panelPos.y = camPos.y; // Keep it at eye level

        Vector3 look = camPos - panelPos; 
        look.y = 0f;
        Quaternion panelRot = Quaternion.identity;
        if (look != Vector3.zero)
            panelRot = Quaternion.LookRotation(-look); 

        panel.transform.SetPositionAndRotation(panelPos, panelRot);
    }

    
}
