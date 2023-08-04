using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Deck : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public class CardGroupInstance
    {
        public List<CardInstance> cards;

        public CardGroupInstance(CardGroup data)
        {
            cards = data.GetCards();
        }

        public void Shuffle()
        {
            List<CardInstance> newCardInstances = new List<CardInstance>();
            int groupSize = cards.Count;
            for (int i = 0; i < groupSize; i++)
            {
                int randomIndex = Random.Range(0, cards.Count);
                newCardInstances.Add(cards[randomIndex]);
                cards.RemoveAt(randomIndex);
            }
            cards = newCardInstances;
        }

        public CardInstance GetTopCard()
        {
            CardInstance topCard;
            if (cards.Count > 0)
            {
                topCard = cards[0];
                cards.RemoveAt(0);
            }
            else
            {
                Debug.Log("group is empty");
                topCard = null;
            }
            return topCard;
        }

        public List<CardInstance> GetTopCards(uint amount)
        {
            List<CardInstance> topCards = new List<CardInstance>();
            for (int i = 0; i < amount; i++)
            {
                if (cards.Count > 0)
                {
                    topCards.Add(cards[0]);
                    cards.RemoveAt(0);
                }
                else
                {
                    Debug.Log("group is empty");
                    break;
                }
            }
            return topCards;
        }

        public void PutCardOnTopOfGroup(CardInstance card)
        {
            cards.Insert(0, card);
        }

        public void PutCardsOnTopOfGroup(List<CardInstance> cardsInDrawOrder)
        {
            cards.InsertRange(0, cardsInDrawOrder);
        }

        public void ShuffleCardInGoup(CardInstance card, int beginRange = 0, int endRange = -1)
        {
            if (cards.Count == 0)
            {
                cards.Add(card);
            }
            else
            {
                endRange = endRange == -1 ? cards.Count : endRange;
                int randomIndex = Random.Range(beginRange, endRange + 1);
                randomIndex = Mathf.Clamp(randomIndex, 0, cards.Count);
                InsertCardInGroup(card, randomIndex);
            }
        }

        public void InsertCardInGroup(CardInstance card, int index)
        {
            cards.Insert(index, card);
        }

        public void DebugDraw()
        {
            foreach (CardInstance card in cards)
            {
                Debug.Log(card.dataInstance.cardName);
            }
        }
    }

    public DeckData deckData;
    public List<CardGroupInstance> deck;

    private Vector3 initScale;
    private Vector3 initPos;
    private int initSize;

    private void Awake()
    {
        deck = new List<CardGroupInstance>();
        CreateDeckFromData();
        Shuffle();
        
        foreach(CardGroupInstance cardGroup in deck)
        {
            Debug.Log("----New region----");
            foreach (var card in cardGroup.cards)
            {
                Debug.Log(card.dataInstance.cardName);
            }
        }
        initScale = transform.localScale;
        initPos = transform.position;
        int cardCount = 0;
        foreach (CardGroupInstance cardGroup in deck) { cardCount += cardGroup.cards.Count; }
        initSize = cardCount;
    }

    private void CreateDeckFromData()
    {
        foreach (CardGroup cardGroupData in deckData.groups)
        {
            deck.Add(new CardGroupInstance(cardGroupData));
        }
    }

    public CardInstance GetTopCard()
    {
        CardInstance topCard;
        if(deck.Count > 0)
        {
            CardGroupInstance topGroup = deck[0];
            topCard = topGroup.GetTopCard();

            if (topCard == null)
            {
                deck.RemoveAt(0);
                return GetTopCard();
            }
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
        if (amount > 0)
        {
            if (deck.Count > 0)
            {
                CardGroupInstance topGroup = deck[0];
                topCards = topGroup.GetTopCards(amount);
                if (topCards.Count <= 0)
                {
                    deck.RemoveAt(0);
                    return GetTopCards(amount);
                }
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
        foreach(CardGroupInstance cardGroup in deck)
        {
            cardGroup.Shuffle();
        }
    }

    //probably debug only
    public void ReShuffle()
    {
        deck.Clear();
        CreateDeckFromData();
        Shuffle();
        UpdateDeckScale();
        
        foreach(CardGroupInstance cardGroup in deck)
        {
            Debug.Log("----New region----");
            foreach (var card in cardGroup.cards)
            {
                Debug.Log(card.dataInstance.cardName);
            }
        }
    }

    public void PutCardOnTopOfDeck(CardInstance card)
    {
        if (deck.Count > 0)
        {
            deck[0].PutCardOnTopOfGroup(card);
        }
    }

    public void PutCardsOnTopOfDeck(List<CardInstance> cardsInDrawOrder)
    {
        if (deck.Count > 0)
        {
            deck[0].PutCardsOnTopOfGroup(cardsInDrawOrder);
        }
    }

    public void ShuffleCardInDeck(CardInstance card, int beginRange = 0, int endRange = -1)
    {
        if(deck.Count > 0)
        {
            int cardCount = 0;
            foreach (CardGroupInstance cardGroup in deck) { cardCount += cardGroup.cards.Count; }

            endRange = endRange == -1 ? cardCount : endRange;
            int randomIndex = Random.Range(beginRange, endRange + 1);
            randomIndex = Mathf.Clamp(randomIndex, 0, cardCount);

            foreach(CardGroupInstance cardGroup in deck)
            {
                if (randomIndex <= cardGroup.cards.Count)
                {
                    cardGroup.InsertCardInGroup(card, randomIndex);
                    break;
                }
                randomIndex -= cardGroup.cards.Count;
            }
            UpdateDeckScale();
        }
    }

    public void ShuffleCardInGroup(CardInstance card, int groupIndex, int beginRange = 0, int endRange = -1)
    {
        if(deck.Count > 0)
        {
            groupIndex = groupIndex == -1 ? deck.Count - 1 : groupIndex;
            groupIndex = Mathf.Clamp(groupIndex, 0, deck.Count - 1);
            deck[groupIndex].ShuffleCardInGoup(card, beginRange, endRange);
            UpdateDeckScale();
        }
    }

    private void UpdateDeckScale()
    {
        int cardCount = 0;
        foreach(CardGroupInstance cardGroup in deck) { cardCount += cardGroup.cards.Count; }

        if(cardCount > 0)
        {
            transform.localScale = new Vector3(initScale.x, initScale.y / initSize * cardCount, initScale.z);
            transform.position = new Vector3(initPos.x, initPos.y - ((initSize - cardCount) *  0.5f / initSize * initScale.y), initPos.z);
            GetComponent<MeshRenderer>().enabled = true;
        }
        else
        {
            GetComponent<MeshRenderer>().enabled = false;
        }
    }

    public void DebugDraw()
    {
        foreach(CardGroupInstance group in deck)
        {
            group.DebugDraw();
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
