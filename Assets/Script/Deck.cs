using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Deck : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public List<CardData> deckData = new List<CardData>();
    public List<CardInstance> deck = new List<CardInstance>();

    private Vector3 initScale;
    private Vector3 initPos;
    private int initSize;

    private void Awake()
    {
        CreateDeckFromData();
        Shuffle();
        initScale = transform.localScale;
        initPos = transform.position;
        initSize = deck.Count;
    }

    private void CreateDeckFromData()
    {
        foreach (CardData card in deckData)
        {
            deck.Add(new CardInstance(card));
        }
    }

    public CardInstance GetTopCard()
    {
        CardInstance topCard;
        if(deck.Count > 0)
        {
            topCard = deck[0];
            deck.RemoveAt(0);
        }
        else
        {
            Debug.Log("deck is empty");
            topCard = null;
        }
        UpdateDeckScale();
        return topCard;
    }

    public List<CardInstance> GetTopCards(uint amount)
    {
        List<CardInstance> topCards = new List<CardInstance>();
        for(int i = 0; i < amount; i++)
        {
            if (deck.Count > 0)
            {
                topCards.Add(deck[0]);
                deck.RemoveAt(0);
            }
            else
            {
                Debug.Log("deck is empty");
            }
        }
        UpdateDeckScale();
        return topCards;
    }

    public void Shuffle()
    {
        List<CardInstance> newDeck = new List<CardInstance>();
        int deckSize = deck.Count;
        for(int i = 0; i < deckSize; i++)
        {
            int randomIndex = Random.Range(0, deck.Count);
            newDeck.Add(deck[randomIndex]);
            deck.RemoveAt(randomIndex);
        }
        deck = newDeck;
    }

    //probably debug only
    public void ReShuffle()
    {
        deck.Clear();
        CreateDeckFromData();
        Shuffle();
        UpdateDeckScale();
    }

    public void ShuffleCardInDeck(CardInstance card, int beginRange = 0, int endRange = -1)
    {
        if(deck.Count == 0)
        {
            deck.Add(card);
        }
        else
        {
            endRange = endRange == -1 ? deck.Count : endRange;
            int randomIndex = Random.Range(beginRange, endRange + 1);
            if(randomIndex == deck.Count)
            {
                deck.Add(card);
                UpdateDeckScale();
                return;
            }

            List<CardInstance> newDeck = new List<CardInstance>();
            int i = 0;
            while (i < deck.Count)
            {
                if(i == randomIndex)
                    newDeck.Add(card);

                newDeck.Add(deck[i]);
                i++;
            }

            deck = newDeck;
        }
        UpdateDeckScale();
    }

    private void UpdateDeckScale()
    {
        if(deck.Count > 0)
        {
            transform.localScale = new Vector3(initScale.x, initScale.y / initSize * deck.Count, initScale.z);
            transform.position = new Vector3(initPos.x, initPos.y - ((initSize - deck.Count) *  0.5f / initSize * initScale.y), initPos.z);
            GetComponent<MeshRenderer>().enabled = true;
        }
        else
        {
            GetComponent<MeshRenderer>().enabled = false;
        }
    }

    ////////////Interaction Implementation////////////////
    public void OnPointerDown(PointerEventData eventData)
    {
        //empty
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //empty
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            GameManager.Instance.OnDeckClicked(deck.Count);
        else if (eventData.button == PointerEventData.InputButton.Right)
            GameManager.Instance.OnDeckRightClicked();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //empty
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //empty
    }

    ////////////////////////////////////////////////////////////////


}
