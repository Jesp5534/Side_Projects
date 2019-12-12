using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: Make into hearthstone esq. card slots, w. automatic rearrangement.

public enum CardLocation { hand, board, deck, none }; //potentially more to be added?

public class CardManager : MonoBehaviour
{
    public float spacing;
    public int maxCards;
    public int maxSlots;
    public float transitionTime;
    public float[] cardSlotsPos;
    public GameObject cardSlotPrefab;
    public GameObject cardPrefab;
    public int[] boardSlotStatus;
    public int[] handSlotStatus;
    public GameObject[] boardSlotContainer;
    public GameObject[] handSlotContainer;

    private GameObject gamemanager;
    private GameObject boardArea;
    private GameObject handArea;
    private GameObject boardCards;
    private GameObject handCards;
    [SerializeField] private readonly int boardCardsNum;
    [SerializeField] private int handCardsNum;

    private void Start()
    {
        maxSlots = maxCards * 2 - 1;
        boardSlotStatus = new int[maxSlots];
        handSlotStatus = new int[maxSlots];
        gamemanager = GameObject.Find("GameManager");
        boardArea = GameObject.Find("PlayArea");
        handArea = GameObject.Find("HandArea");
        boardCards = boardArea.transform.Find("CardCache").gameObject;
        handCards = handArea.transform.Find("CardCache").gameObject;        
        InputManager inputManager = gamemanager.GetComponent<InputManager>();
        inputManager.ReleasedCard += OnReleasedCard;

        InstantianteCardSlots(maxSlots);
        StartCoroutine(InstantiateDummyCards());
    }


    private void OnReleasedCard(CardProperties info)
    {
        /*Call different functions, depending on targetindex availablity and targetlocation*/

        //Check if num of cards are equal to max cards. Then we have to force a swap.
        
        if (false)
        {
            
        }
        else
        {
            ArrangeCards(info);
        }
    }


    private void InstantianteCardSlots(int maxSlots)
    {
        //Instantiate cardslots 
        GameObject currSlot;
        for (int i = 0; i < maxSlots; i++)
        {
            currSlot = Instantiate(cardSlotPrefab, GameObject.Find("BoardSlotCache").transform);
            currSlot.tag = ("BoardSlot");

            currSlot = Instantiate(cardSlotPrefab, GameObject.Find("HandSlotCache").transform);
            currSlot.tag = ("HandSlot");
        }

        //Set cardslot positions forr the area
        boardSlotContainer = GameObject.FindGameObjectsWithTag("BoardSlot");
        handSlotContainer = GameObject.FindGameObjectsWithTag("HandSlot");

        cardSlotsPos = CalcSlotPositions(maxSlots, spacing);

        for (int i = 0; i < cardSlotsPos.Length; i++)
        {
            boardSlotContainer[i].transform.position = new Vector3(cardSlotsPos[i], boardArea.transform.position.y);
            handSlotContainer[i].transform.position = new Vector3(cardSlotsPos[i], handArea.transform.position.y);
        }
    }


    private float[] CalcSlotPositions(int num, float spacing)
    {
        /*** Calculate spawn position for the slots the cards can be at ***/

        List<float> returnList = new List<float>();

        for (int i = 0; i < num; i++)
        {
            returnList.Add((spacing * i) - (spacing * num / 2 - 0.5f)); //centers middle card at 0
        }

        return returnList.ToArray();
    }


    private IEnumerator InstantiateDummyCards()
    {
        /*** Test function to simulate having cards drawn ***/

        for (int i = 0; i < 6; i++)
        {
            if (handCardsNum < maxCards)
            {
                GameObject card = Instantiate(cardPrefab, handCards.transform);
                CardProperties currCard = card.GetComponent<CardProperties>();

                card.name = "Card " + (i + 1);
                currCard.location = CardLocation.deck;
                currCard.index = 0;
                currCard.targetLocation = CardLocation.hand;
                currCard.targetIndex = maxSlots - 1; //we want to fill out the hand from the right, so we draw to the rightmost slot.

                handCardsNum++;
                ArrangeCards(currCard);

                yield return new WaitForSeconds(0.5f);
            }            
        }
    }


    private void ArrangeCards(CardProperties currCard)
    {
        GameObject targetArea;
        int originalIndex = currCard.index;

        switch (currCard.targetLocation)
        {
            case CardLocation.hand:
                targetArea = handArea;
                break;
            case CardLocation.board:
                targetArea = boardArea;
                break;
            default:
                targetArea = null;
                break;
        }

        CardMovement cardMovement = currCard.gameObject.GetComponent<CardMovement>();
        
        currCard.targetIndex = CalcNewPosition(currCard); 
        StartCoroutine(cardMovement.MoveCard(new Vector3(cardSlotsPos[currCard.targetIndex], targetArea.transform.position.y), transitionTime));

        UpdateSlotStatus(currCard);
        RearrangeAreas(currCard, originalIndex);
    }


    public int CalcNewPosition(CardProperties currCard)
    {
        /*
         * Calculates the position for the input card in a given area,
         * based on what other slots are filled in said area.
         */
         
        int[] currAreaStatus;

        switch (currCard.targetLocation)
        {
            case CardLocation.hand:
                currAreaStatus = handSlotStatus;
                break;
            case CardLocation.board:
                currAreaStatus = boardSlotStatus;
                break;
            default:
                currAreaStatus = null;
                break;
        }

        int slotsToTheLeft = 0;
        int slotsToTheRight = 0;

        for (int i = 0; i < currCard.targetIndex; i++)
        {
            if (currAreaStatus[i] == 1)
            {
                slotsToTheLeft++;
            }
        }

        for (int i = currCard.targetIndex + 1; i < currAreaStatus.Length; i++)
        {
            if (currAreaStatus[i] == 1)
            {
                slotsToTheRight++;
            }
        }

        return maxSlots / 2 + (slotsToTheLeft - slotsToTheRight);
    }


    public void UpdateSlotStatus(CardProperties currCard)
    {
        /*
         * Updates what slots are filled in a given area.
         */

        GameObject newCardCache;

        switch (currCard.targetLocation)
        {
            case CardLocation.hand:
                handSlotStatus[currCard.targetIndex] = 1;
                newCardCache = handCards;
                break;
            case CardLocation.board:
                boardSlotStatus[currCard.targetIndex] = 1;
                newCardCache = boardCards;
                break;
            default:
                newCardCache = null;
                break;
        }

        switch (currCard.location)
        {
            case CardLocation.hand:
                handSlotStatus[currCard.index] = 0;
                break;
            case CardLocation.board:
                boardSlotStatus[currCard.index] = 0;
                break;
            default:
                break;
        }

        currCard.index = currCard.targetIndex;
        currCard.targetIndex = 0;
        currCard.transform.parent = newCardCache.transform;
    }


    public void RearrangeAreas(CardProperties currCard, int originalIndex)
    {
        /*
         * Rearranges the cards in the current area, and the target area,
         * to adjust them to accomodate the newly added/removed card.
         */

        List<Transform> newAreaCards = new List<Transform>();
        List<Transform> oldAreaCards = new List<Transform>();
        bool drawnFromDeck = false;
        
        switch (currCard.targetLocation)
        {
            case CardLocation.hand:
                newAreaCards = handCards.GetComponentsInChildren<Transform>().ToList();
                break;
            case CardLocation.board:
                newAreaCards = boardCards.GetComponentsInChildren<Transform>().ToList();
                break;
            default:
                newAreaCards = null;
                break;
        }

        for (int i = 1; i < newAreaCards.Count; i++) //starts at index 1, as index 
        {
            //dont need to move the card we just placed
            if (newAreaCards[i].gameObject != currCard.gameObject)
            {
                newAreaCards[i].GetComponent<CardMovement>().AdjustAreaCards(currCard, originalIndex, true);
            }
        }

        switch (currCard.location)
        {
            case CardLocation.hand:
                oldAreaCards = handCards.GetComponentsInChildren<Transform>().ToList();
                break;
            case CardLocation.board:
                oldAreaCards = boardCards.GetComponentsInChildren<Transform>().ToList();
                break;
            default:
                oldAreaCards = null;
                drawnFromDeck = true;
                break;
        }


        if (!drawnFromDeck)
        {
            for (int i = 1; i < oldAreaCards.Count; i++)
            {
                oldAreaCards[i].GetComponent<CardMovement>().AdjustAreaCards(currCard, originalIndex, false);
            }
        }

        currCard.location = currCard.targetLocation;
    }


    /** OLD DUMB CODE**/
         
    // Wierd and slower way to calc target position for a card, than the one in cardmovement
    private int CalcCardPosition(CardInfo currCard)
    {
        int[] currSlotStatus = (currCard.targetLocation == CardLocation.hand) ? handSlotStatus : boardSlotStatus;

        List<int> occupiedSlots = GetFilledSlots(currSlotStatus);

        //find closest index to put card.
        //Also checks if  said index has a neighbor. We do not want cards to float nilly willy.
        int searchRange = 1;
        int higherSearchIndex;
        int lowerSearchIndex;


        while (searchRange <= maxSlots / 2)
        {
            higherSearchIndex = currCard.targetIndex + searchRange;
            lowerSearchIndex = currCard.targetIndex - searchRange;

            if (higherSearchIndex < maxSlots)
            {
                if (occupiedSlots.Contains(higherSearchIndex))
                {
                    return (higherSearchIndex - 1);
                }
            }

            if (lowerSearchIndex > 0)
            {
                if (occupiedSlots.Contains(lowerSearchIndex))
                {
                    return (lowerSearchIndex + 1);
                }
            }

            searchRange++;
        }

        return maxSlots / 2; //Middle value
    }


    //A helper function to find removed function above
    private List<int> GetFilledSlots(int[] locationStatus)
    {
        List<int> occupiedSlots = new List<int>();

        for (int i = 0; i < locationStatus.Length; i++)
        {
            if (locationStatus[i] == 1)
            {
                occupiedSlots.Add(i);
            }
        }

        return occupiedSlots;
    }


    // Tried putting all variables of a scripts into a struct... Wasn't a good idea apparently. Now i just pass the script.
    private struct CardInfo
    {
        public GameObject gameObject;
        public CardLocation location;
        public CardLocation targetLocation;
        public int index;
        public int targetIndex;
        public int id;

        public CardInfo(GameObject cardGameObject, CardLocation cardLocation, CardLocation cardTargetLocation, int cardIndex, int cardTargetIndex, int cardID)
        {
            gameObject = cardGameObject;
            location = cardLocation;
            targetLocation = cardTargetLocation;
            index = cardIndex;
            targetIndex = cardTargetIndex;
            id = cardID;
        }
    }

}
