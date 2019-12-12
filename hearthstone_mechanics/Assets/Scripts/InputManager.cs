using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public delegate void ReleasedCardEventHandler(CardProperties info);
    public event ReleasedCardEventHandler ReleasedCard;

    private CardProperties cardInfo;
    private CardManager cardManager;
    private Vector3 startPosition;
    private bool dragging;
    private RaycastHit[] hits;
    private Ray inputRay;


    private void Start()
    {
        cardManager = GameObject.Find("GameManager").GetComponent<CardManager>();
        dragging = false;
        startPosition = new Vector3(0, 0, 0);
    }


    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !dragging) /***CLICKING CARD***/
        {
            inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), 100);
            
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider.CompareTag("Card")) //issues if multiple cards are stacked ontop of eachother?
                {
                    cardInfo = hits[i].collider.gameObject.GetComponent<CardProperties>();

                    startPosition = cardInfo.gameObject.transform.position;
                    dragging = true;                    
                    break;
                }
            }
        }
        else if (dragging) /***HOLDING CARD***/
        {
            if (cardInfo.location == CardLocation.hand || true /*true for testing*/) //only if card is not a 'spell' - otherwise new condition
            {
                cardInfo.gameObject.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0, 0, 10));
            }
            else if (cardInfo.location == CardLocation.board)
            {
                //some graphic to indicate where you are aiming with 'creature'
            }
        }

        if (Input.GetMouseButtonUp(0) && dragging) /***RELEASING HELD CARD***/
        {
            dragging = false;

            inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), 100);

            for (int i = 0; i < hits.Length; i++) /***LOOPS THROUGH OBJECTS TO FIND AREA***/
            {
                if (hits[i].collider.gameObject.CompareTag("Area"))
                {
                    //check for other areas, call different functions depending
                    cardInfo.targetLocation = (hits[i].collider.gameObject.name == "PlayArea") ? CardLocation.board : CardLocation.hand;
                    break;
                }
                cardInfo.targetLocation = CardLocation.none;
            }

            if (cardInfo.targetLocation == CardLocation.none) /***RELEASED CARD OUTSIDE ANY AREA***/
            {
                cardInfo.gameObject.transform.position = startPosition;
            }
            else /***HIT AN AREA***/
            {
                bool aimedAtOwnArea = (cardInfo.targetLocation == cardInfo.location);

                if (aimedAtOwnArea)
                {
                    Debug.Log("Swap!");
                }
                
                cardInfo.targetIndex = CalcTargetIndex(cardInfo);
                OnReleasedCard(cardInfo);
            }

        }
    }


    private int CalcTargetIndex(CardProperties currCard)
    {
        GameObject[] targetSlotsPosition = new GameObject[cardManager.maxSlots];
        int[] targetSlotsStatus = new int[cardManager.maxSlots];

        int closestIndex = 0;

        switch (cardInfo.targetLocation)
        {
            case CardLocation.hand:
                targetSlotsPosition = cardManager.handSlotContainer;
                targetSlotsStatus = cardManager.handSlotStatus;
                break;
            case CardLocation.board:
                targetSlotsPosition = cardManager.boardSlotContainer;
                targetSlotsStatus = cardManager.boardSlotStatus;
                break;
        }

        for (int i = 0; i < targetSlotsPosition.Length; i++)
        {
            if (Vector3.Distance(targetSlotsPosition[i].transform.position, cardInfo.gameObject.transform.position) <
                Vector3.Distance(targetSlotsPosition[closestIndex].transform.position, cardInfo.gameObject.transform.position)
                && targetSlotsStatus[i] == 0)
            {
                closestIndex = i;
            }
        }
        return closestIndex;
    }


    protected virtual void OnReleasedCard(CardProperties info)
    {
        ReleasedCard?.Invoke(info);
    }
}
