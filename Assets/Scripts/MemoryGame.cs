using System;
using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKitSamples.EnvironmentPanelPlacement;
using UnityEngine;

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
    [SerializeField] private MemoryGameUI _gameUI;
    [SerializeField] private EnvironmentPanelPlacement _panelPlacement;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip bubblePopSound;
    [SerializeField] private AudioClip cardsMatchedSound;
    [SerializeField] private AudioClip countdownSound;
    [SerializeField] private AudioClip puzzleWinSound;

    private int _initialCardCount;

    void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        if (_panelPlacement == null) _panelPlacement = GetComponentInParent<EnvironmentPanelPlacement>();
        if (_gameUI == null) _gameUI = GetComponentInChildren<MemoryGameUI>();

        _initialCardCount = cardsList.Count;

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

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void HandleFirstWallSnap()
    {
        _panelPlacement.OnFirstWallSnap -= HandleFirstWallSnap; // unsubscribe — one-shot
        PlaySound(bubblePopSound); // Play bubble pop when snapped
        StartCoroutine(InitialDelay());
    }

    public void CardSelected(MemoryCards selectedCard)
    {
        if (!_gameReady || checkingCards || selectedCard == null)
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
        yield return new WaitForSeconds(1f);
        PlaySound(puzzleWinSound);
    }

    
}
