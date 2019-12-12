using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardMovement : MonoBehaviour
{
    private GameObject gameManager;
    private CardManager cardManager;
    private CardProperties cardProperties;

    private float[] slotsPosition;

    public void Awake()
    {
        gameManager = GameObject.Find("GameManager");
        cardManager = gameManager.GetComponent<CardManager>();
        cardProperties = gameObject.GetComponent<CardProperties>();
        
        slotsPosition = cardManager.cardSlotsPos;
    }


    public void AdjustAreaCards(CardProperties currCard, int originalIndex, bool added)
    {
        /*
         * Adjust card one index left or right, depending on index of input card,
         * and whether said input card was added or removed from the area.
         */

        int newIndex = cardProperties.index;
        
        if (added)
        {
            if (currCard.index > cardProperties.index)
                newIndex = cardProperties.index - 1;

            else if (currCard.index < cardProperties.index)
                newIndex = cardProperties.index + 1;
        }
        else
        {
            if (originalIndex > cardProperties.index)
            {
                newIndex = cardProperties.index + 1;
            }
            else if (originalIndex < cardProperties.index)
            {
                newIndex = cardProperties.index - 1;
            }
        }

        cardProperties.targetIndex = newIndex;
        StartCoroutine(MoveCard(new Vector3(slotsPosition[newIndex], transform.position.y), cardManager.transitionTime));
        
        cardManager.UpdateSlotStatus(cardProperties);
    }

    public IEnumerator MoveCard(Vector3 targetPos, float duration)
    {
        float progress = 0;
        while (progress <= 1)
        {
            progress += (Time.deltaTime / duration);

            transform.position = Vector3.Lerp(transform.position, targetPos, progress);

            yield return null;
        }
    }


}
