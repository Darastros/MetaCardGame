using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractibleCardList : MonoBehaviour, ICardInteractionHandler
{
    public List<GameObject> cardList;
    public float maxInterval = 3;
    public float maxSize = 15;

    public void AddCardToList(GameObject card)
    {
        cardList.Add(card);
        card.GetComponent<Card>().interactionHandler = this;
        DisplayCards();
    }

    public GameObject RemoveCardFromList(uint cardID)
    {
        for (int i = 0; i < cardList.Count; i++)
        {
            GameObject card = cardList[i];
            if (card.GetComponent<Card>().data.cardID == cardID)
            {
                cardList.RemoveAt(i);
                return card;
            }
        }
        return null;
    }

    public void ReleaseCardAtPos(Vector3 pos, GameObject card)
    {
        int intervalToInsert = 0;
        int intervalCount = cardList.Count - 1;
        float interval = 0;
        if (intervalCount > 0)
        {
            interval = Mathf.Min(maxInterval, maxSize / intervalCount);
        }
        float relativeXPos = pos.x - (gameObject.transform.position.x - interval * intervalCount * 0.5f);
        if(relativeXPos > 0)
            intervalToInsert = Mathf.FloorToInt(relativeXPos / interval) + 1;

        List<GameObject> newCardList = new List<GameObject>();
        for (int i = 0; i < cardList.Count; i++)
        {
            if (i == intervalToInsert)
                newCardList.Add(card);

            if(cardList[i] != card)
                newCardList.Add(cardList[i]);
        }
        if(intervalToInsert >= cardList.Count)
        {
            newCardList.Add(card);
        }
        cardList = newCardList;
        DisplayCards();
    }

    public void DisplayCards()
    {
        int intervalCount = cardList.Count - 1;
        float interval = 0;
        if(intervalCount > 0)
        {
            interval = Mathf.Min(maxInterval, maxSize / intervalCount);
        }
        float cumulatedIntervals = interval * intervalCount;
        Vector3 startPosition = gameObject.transform.position - new Vector3(cumulatedIntervals * 0.5f, 0, 0);
        for(int i = 0; i < cardList.Count; i++)
        {
            cardList[i].transform.position = startPosition + new Vector3(i * interval, -i * 0.025f, 0);
        }
    }

    //////////////////// HANDLE CARD INTERACTION///////////////////////
    public void OnCardPressed(GameObject card)
    {
        GameManager.Instance.OnCardPressed(card);
    }
    public void OnCardReleased(GameObject card)
    {
        GameManager.Instance.OnCardReleased(card);
    }
    public void OnCardClicked(GameObject card)
    {
        GameManager.Instance.OnCardClicked(card); 
    }
    public void OnCardEntered(GameObject card)
    {

    }
    public void OnCardExited(GameObject card)
    {

    }
    ///////////////////////////////////////////////////////////////////

}
