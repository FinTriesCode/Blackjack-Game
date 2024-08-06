using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
//using UnityEngine.InputSystem;
using UnityEngine.Events;
using System;
using UnityEditor;
//using Language.Lua;
using Random = UnityEngine.Random;

public class BlackJackGameLogic : MonoBehaviour
{
    //array created so that the table slots can be dragged in -> in-editor
    [SerializeField]
    Transform[] cardTableSlots;

    [SerializeField]
    Transform[] AICardTableSlots;

    [SerializeField]
    Transform[] pokerChipTableSlots;

    //array of entire deck
    [SerializeField]
    Card[] cards;

    //array of cards in player's hand
    //array of poker chips placed
    List<PlayCard> playerCardsHeld = new();
    List<PlayCard> AICardsHeld = new();
    List<GameObject> pokerChipsList = new();

    //reference to the in-engine card prefab(s) (used to update the mesh and instatiate)
    [SerializeField]
    PlayCard cardPrefab;
    [SerializeField]
    GameObject pokerChip;

    //player wallet
    public PCPlayerFunds playerWallet;

    [Header("Events")]
    [SerializeField] private UnityEvent lackingFunds;
    [SerializeField] private UnityEvent playerLose;
    [SerializeField] private UnityEvent playerWin;
 

    //Text
    [SerializeField]
    TMP_Text WinLoseText;

    [SerializeField]
    TMP_Text YesNoText;

    [SerializeField]
    TMP_Text playInstructions;

    [SerializeField]
    TMP_Text playerBalance;

    [SerializeField]
    TMP_Text outOfFundsText;


    //misc variables
    int takenSlots = 0; //for cards
    int AITakenSlots = 0; //for cards
    int takenPokerChipSlots = 0; //for chips

    [Header("Booleans")]
    [SerializeField] private bool gameActive = false;
    [SerializeField] private bool hasFolded = false;
    [SerializeField] private bool AIFold = false;
    [SerializeField] private bool hasGameEnded = true;
    [SerializeField] private bool balanceUpdated = false;
    [SerializeField] private bool hasMadeReplayDecision = false;

    private void Start()
    {
        StartGameLoop();
    }

    private void Update()
    {
        if (gameActive) GameplayLoop();

        if(!gameActive && balanceUpdated && !hasMadeReplayDecision)
        {
            PromtToPlayAgain();
        }
    }

    //function to give the player 2 starting cards
    void DealStartingCards()
    {
        //2 starting cards
        for (int i = 0; i < 2; i++)
        {
            //get random card index (card from deck)
            int randInt = UnityEngine.Random.Range(0, cards.Length);

            //apply random number to draw a random card from the deck
            //and instantiate it and the position of the available card slots
            Card randomCard = FindNonPlayedCard(randInt);
            PlayCard chosenCard = Instantiate(cardPrefab, cardTableSlots[i].position, cardTableSlots[i].rotation);

            //set the chosen card using the SetCard funciton - allowing for in-engine updating (actual visual playing of card(s))
            chosenCard.SetCard(randomCard);

            //add card to players hand array
            playerCardsHeld.Add(chosenCard);

            takenSlots++;
        }
    }

    void AIDealStartingCards()
    {
        //2 starting cards
        for (int i = 0; i < 2; i++)
        {
            //get random card index (card from deck)
            int randInt = UnityEngine.Random.Range(0, cards.Length);

            //apply random number to draw a random card from the deck
            //and instantiate it and the position of the available card slots
            Card randomCard = FindNonPlayedCard(randInt);
            PlayCard chosenCard = Instantiate(cardPrefab, AICardTableSlots[i].position, AICardTableSlots[i].rotation);

            //set the chosen card using the SetCard funciton - allowing for in-engine updating (actual visual playing of card(s))
            chosenCard.SetCard(randomCard);

            //add card to players hand array
            AICardsHeld.Add(chosenCard);

            AITakenSlots++;
        }
    }

    //funciton to pick up card
    void PickUpCard()
    {
        Debug.Log("Called");
        //do a check for a key press, then update using update function.
        if (takenSlots < 6)
        {
            int randInt = UnityEngine.Random.Range(0, cards.Length);

            Card randomCard = FindNonPlayedCard(randInt);

            PlayCard chosenCard = Instantiate(cardPrefab, cardTableSlots[takenSlots].position, cardTableSlots[takenSlots].rotation);

            chosenCard.SetCard(randomCard);

            //add card to players hand array
            playerCardsHeld.Add(chosenCard);

            takenSlots++;
        }
    }

    void AIPickUpCard()
    {
        //do a check for a key press, then update using update function.
        if (AITakenSlots < 6)
        {
            int randInt = UnityEngine.Random.Range(0, cards.Length);

            Card randomCard = FindNonPlayedCard(randInt);
            PlayCard chosenCard = Instantiate(cardPrefab, AICardTableSlots[AITakenSlots].position, AICardTableSlots[AITakenSlots].rotation);

            chosenCard.SetCard(randomCard);

            //add card to players hand array
            AICardsHeld.Add(chosenCard);

            AITakenSlots++;
        }
    }

    Card FindNonPlayedCard(int cardIndex)
    {
        bool found = false;
        cardIndex %= cards.Length; //wrap the array, so never goes out of range

        foreach (var c in playerCardsHeld)
        {
            if (c.GetCard() == cards[cardIndex])
            {
                found = true;
                break;
            }
        }

        if (found)
        {
            return FindNonPlayedCard(cardIndex + 1);
        }

        return cards[cardIndex];
    }

    //funciton to place poker chip on table
    //can only place one currently
    void PlacePokerChip()
    {
        if (takenPokerChipSlots < 3)
        {
            GameObject pokerChipTemp = Instantiate(pokerChip, pokerChipTableSlots[takenPokerChipSlots].position, pokerChipTableSlots[takenPokerChipSlots].rotation);
            pokerChipsList.Add(pokerChipTemp);

            takenPokerChipSlots++;
        }
    }

    //function to destroy poker chip(s)
    void DestroyPokerChips()
    {
        for (int i = 0; i < pokerChipsList.Count; i++)
        {
            Destroy(pokerChipsList[i]);

            takenPokerChipSlots--;
        }

        pokerChipsList.Clear();
        takenPokerChipSlots = 0;
    }

    //function to check the sum of the player's current hand
    int CheckPlayerCardsSum()
    {
        //sum var
        int handSum = 0;

        //loop through all cards and add them to the sum
        foreach (var c in playerCardsHeld)
        {
            //get the value of the held card and add this ot the hand sum
            int currentCard = (int)c.card.cardNum;
            handSum += currentCard;
        }

        //return the sum
        return handSum;
    }

    int CheckAICardsSum()
    {
        //sum var
        int handSum = 0;

        //loop through all cards and add them to the sum
        foreach (var c in AICardsHeld)
        {
            //get the value of the held card and add this ot the hand sum
            int currentCard = (int)c.card.cardNum;
            handSum += currentCard;
        }

        //return the sum
        return handSum;
    }

    //flag variable update functions
    public void EnableGame() { gameActive = true; }

    public void DisableGame() { gameActive = false; }

    //gameplay loop funcitons
    public void ResetGameplayLoop()
    {
        //reset game info and data

        //empty all variables/data

        //reset cards by empting list and resetting takenSlots

        //destory cards on table
        ClearTable();

        //reset data structures
        takenSlots = 0;
        AITakenSlots = 0;
        hasFolded = false;
        AIFold = false;
        WinLoseText.text = "";
        YesNoText.text = "";
        playInstructions.text = "";

        string balance = $"Your Balance: {playerWallet.currentBalance}";
        playerBalance.text = balance;

        balanceUpdated = false;
        gameActive = true;
        hasGameEnded = false;
        hasMadeReplayDecision = false;

        //deal starting cards
        DealStartingCards();
        AIDealStartingCards();
        PlacePokerChip();
        GameplayLoop();
    }

    void GameplayLoop()
    {
        int handSum = CheckPlayerCardsSum();
        int AIHandSum = CheckAICardsSum();
        bool WinLoseCheck = string.IsNullOrEmpty(WinLoseText.text); //if there is text, become true, else false

        string playAgainPromtText = "Press 'y' to play again\nPress 'n' to quit";

        playInstructions.text = "Press 'E' to pick up card\r\nPress 'F' to fold\r\nPress 'ESC' to quit";


        string balance = $"Your Balance: {playerWallet.currentBalance}";
        playerBalance.text = balance;

        //check card's total value and then play turn
        if (handSum <= 21)
        {
            //pick up or fold?
            if (Input.GetKeyDown(KeyCode.E) && WinLoseCheck)
            {
                //player pickup card
                PickUpCard();
                AIDesicionMaking();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }

            StartCoroutine(Fold());
        }

        if(!hasGameEnded)
        {
            StartCoroutine(Over21Check());
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        
    }

    IEnumerator Over21Check()
    {
        int handSum = CheckPlayerCardsSum();
        string playAgainPromtText = "Press 'y' to play again\nPress 'n' to quit";

        if (handSum > 21 && !balanceUpdated)
        {
            //Stop Game
            gameActive = false;
            hasGameEnded = true;
            
            //lose money 
            balanceUpdated = true;
            playerLose.Invoke();
            Debug.Log("You Lose!");
            WinLoseText.text = "You Lose!";
            YesNoText.text = playAgainPromtText;
            DestroyPokerChips();
        }

        yield return StartCoroutine(Wait(1.0f));
    }

    IEnumerator Fold()
    {
        int handSum = CheckPlayerCardsSum();
        int AIHandSum = CheckAICardsSum();
        bool WinLoseCheck = string.IsNullOrEmpty(WinLoseText.text); //if there is text, become true, else false

        string playAgainPromtText = "Press 'y' to play again\nPress 'n' to quit";

        //fold hand and check for win/loss
        if (Input.GetKeyDown(KeyCode.F) && WinLoseCheck && gameActive)
        {
            hasFolded = true;
            gameActive = false;

            Debug.Log("You Fold!");

            if (playerWallet.currentBalance <= 0 || playerWallet.currentBalance < 100)
            {
                lackingFunds.Invoke();
                Debug.Log("You do not have enough funds to continue.");
                yield return StartCoroutine(Wait(1.0f)); // Exit the coroutine early since the player cannot afford to continue

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Application.Quit();
                }
            }

            if (handSum > 21 || AIHandSum > handSum)
            {
                if(!balanceUpdated)
                {   
                    gameActive = false;
                    //lose money
                    balanceUpdated = true;
                    playerLose.Invoke();
                    Debug.Log("You Lose!");
                    WinLoseText.text = "You Lose!";
                    YesNoText.text = playAgainPromtText;
                    playInstructions.text = "";

                    string balance = $"Your Balance: {playerWallet.currentBalance}";
                    playerBalance.text = balance;

                    hasGameEnded = true;

                    //call reset function
                    DestroyPokerChips();
                    PlayerReplayOption();
                }
                else
                {
                    Debug.Log("Tried Folding but balance already updated");
                }
            }
            else if (handSum <= 21 && handSum > AIHandSum)
            {
                if(!balanceUpdated)
                {
                    gameActive = false;
                    //win money
                    balanceUpdated = true;
                    playerWin.Invoke();
                    Debug.Log("You Win!");
                    WinLoseText.text = "You Win!";
                    YesNoText.text = playAgainPromtText;
                    playInstructions.text = "";

                    string balance = $"Your Balance: {playerWallet.currentBalance}";
                    playerBalance.text = balance;

                    hasGameEnded = true;

                    DestroyPokerChips();
                    PlayerReplayOption();

                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        Application.Quit();
                    }
                }


            }
            else //draw
            {
                gameActive = false;
                balanceUpdated = true;
                Debug.Log("You Draw!");
                WinLoseText.text = "You Draw!";
                YesNoText.text = playAgainPromtText;
                playInstructions.text = "";

                string balance = $"Your Balance: {playerWallet.currentBalance}";
                playerBalance.text = balance;

                hasGameEnded = true;

                DestroyPokerChips();
                PlayerReplayOption();

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Application.Quit();
                }

            }
        }

        if (playerWallet.currentBalance <= 0 || playerWallet.currentBalance < 100)
        {
            lackingFunds.Invoke();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }

        yield return StartCoroutine(Wait(1.0f));
    }

    private void PlayerReplayOption()
    {
        int handSum = CheckPlayerCardsSum();

        if (handSum <= 21 && hasFolded)
        {
            //ask if player wants to play again
            DestroyPokerChips();
            Debug.Log("Do you want to play again (Y/N)?");
        }
    }

    IEnumerator Wait(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }

    public void ClearTable()
    {
        for (int i = 0; i < playerCardsHeld.Count; i++)
        {
            Destroy(playerCardsHeld[i].gameObject);
        }
        playerCardsHeld.Clear();

        for (int i = 0; i < AICardsHeld.Count; i++)
        {
            Destroy(AICardsHeld[i].gameObject);
        }
        AICardsHeld.Clear();
    }

    public void AIDesicionMaking()
    {
        //AI's turn
        int AIHandSum = CheckAICardsSum();

        //add weighing to add AI 'decision making', allowing for a chance to fail
        int AICardPickupWeighing = Random.Range(1, 100);

        //use weighing for more human-like decision making
        if (!AIFold)
        {
            if (AIHandSum <= 10)
            {
                AIPickUpCard();
            }
            else if (AIHandSum <= 15)
            {
                if (AICardPickupWeighing < 50)
                {
                    AIPickUpCard();
                }
            }
            else if (AIHandSum <= 18)
            {
                if (AICardPickupWeighing <= 35)
                {
                    AIPickUpCard();
                }
            }
            else if (AIHandSum <= 20)
            {
                if (AICardPickupWeighing <= 8)
                {
                    AIPickUpCard();
                }
            }
            else
            {
                AIFold = true;
            }
        }
    }

    private void PromtToPlayAgain()
    {
        //ask if player wants to play again
        Debug.Log("Do you want to play again (Y/N)?");
        if (Input.GetKeyDown(KeyCode.Y))
        {
            hasMadeReplayDecision = true;

            Debug.Log("You have chosen to play again.");
            WinLoseText.text = "";
            YesNoText.text = "";
            playInstructions.text = "Press 'E' to pick up card\r\nPress 'F' to fold\r\nPress 'ESC' to quit";

            string balance = $"Your Balance: {playerWallet.currentBalance}";
            playerBalance.text = balance;

            if (playerWallet.currentBalance >= 100)
            {
                //Player has enough funds and is continuiing
                ResetGameplayLoop();
            }
            else
            {
                Debug.Log("You cannot play due to a lack of funds.");
                ClearTable();
            }
        }
    
        if (Input.GetKeyDown(KeyCode.N))
        {   
            hasMadeReplayDecision = true;

            Debug.Log("Player Stop Playing and Quit");
            WinLoseText.text = "";
            YesNoText.text = "";
            playInstructions.text = "";
            playerBalance.text = "";
            Application.Quit();

        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (playerWallet.currentBalance < 100)
        {
            lackingFunds.Invoke();
            WinLoseText.text = "";

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }
    }

    public void StartGameLoop()
    {
        Debug.Log("Start Game Loop Called Once");

        playerWallet = FindObjectOfType<PCPlayerFunds>();

        //check player balance
        if (playerWallet.currentBalance >= 100)
        {
            //This is starting the game as the player has enough funds and has inputted to play the game
            Debug.Log("Has Funds and Setting Game Active to True");
            gameActive = true;
            hasGameEnded = false;

            //deal starting cards
            DealStartingCards();
            AIDealStartingCards();
            PlacePokerChip();
            GameplayLoop();
        }
        else
        {
            lackingFunds.Invoke();
            Debug.Log("You dont have the money to play.");
            gameActive = false;
        }
    }

    public void NoMoreFunds()
    {
        outOfFundsText.text = "You are out of funds!\r\nYou have lost.";
        YesNoText.text = "Press 'ESC' to exit the game\r\nBetter luck next time.";

        playerBalance.text = "";
    }    
}