using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Deck : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public class CardGroupInstance
    {
        public string name;
        public bool isDicovered;
        public List<CardInstance> cards;

        public CardGroupInstance(CardGroup data)
        {
            name = data.groupName;
            cards = data.GetCards();
            isDicovered = false;
        }

        public CardGroupInstance()
        {
            name = "";
            cards = new List<CardInstance>();
            isDicovered = false;
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

        public CardInstance DrawTopCard()
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

        public List<CardInstance> LookAtTopCards(int amount)
        {
            int maxAmount = Mathf.Min(amount, cards.Count);
            List<CardInstance> topCards = cards.GetRange(0, maxAmount);
            return topCards;
        }

        public void PutCardOnTopOfGroup(CardInstance card)
        {
            cards.Insert(0, card);
        }

        public void PutBackCardsOnTopOfGroup(List<CardInstance> cardsInDrawOrder)
        {
            if(cardsInDrawOrder.Count <= cards.Count)
            {
                cards.RemoveRange(0, cardsInDrawOrder.Count);
                cards.InsertRange(0, cardsInDrawOrder);
            }
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

    public Material MonsterCardBack;
    public Material ObjectCardBack;
    public Material EventCardBack;
    public GameObject TopcardDisplayer;

    private void Awake()
    {
        deck = new List<CardGroupInstance>();
        CreateDeckFromData();
        Shuffle();
        DebugDraw();

        initScale = transform.localScale;
        initPos = transform.position;
        int cardCount = 0;
        foreach (CardGroupInstance cardGroup in deck) { cardCount += cardGroup.cards.Count; }
        initSize = cardCount;
        UpdateDeckVisual();
    }

    private void CreateDeckFromData()
    {
        foreach (CardGroup cardGroupData in deckData.groups)
        {
            deck.Add(new CardGroupInstance(cardGroupData));
        }
    }

    public CardInstance DrawTopCard()
    {
        CardInstance topCard;
        if(deck.Count > 0)
        {
            if (!deck[0].isDicovered)
            {
                GameManager.Instance.animator.SetInteger("Zone", deck.Count); //temp value is deck.count -> decremental
                Debug.Log(deck[0].name);
                deck[0].isDicovered = true;
            }

            CardGroupInstance topGroup = deck[0];
            topCard = topGroup.DrawTopCard();

            if (topCard == null)
            {
                deck.RemoveAt(0);
                return DrawTopCard();
            }
        }
        else
        {
            Debug.Log("deck is empty");
            topCard = null;
        }
        UpdateDeckVisual();
        return topCard;
    }

    public List<CardInstance> LookAtTopCards(int amount)
    {
        List<CardInstance> topCards = new List<CardInstance>();
        int i = 0;
        while (i < deck.Count && topCards.Count < amount)
        {
            CardGroupInstance groupToLookFrom = deck[i];
            topCards.AddRange(groupToLookFrom.LookAtTopCards(amount - topCards.Count));
            i++;
        }
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
        UpdateDeckVisual();
        DebugDraw();
    }

    public void PutCardOnTopOfDeck(CardInstance card)
    {
        if (deck.Count > 0)
        {
            deck[0].PutCardOnTopOfGroup(card);
        }
        else
        {
            deck.Add(new CardGroupInstance());
            deck[0].PutCardOnTopOfGroup(card);
        }
        UpdateDeckVisual();
    }

    public void PutBackCardsOnTopOfDeck(List<CardInstance> cardsInDrawOrder)
    {
        int amountOfCardsPutBack = 0;
        int i = 0;
        while (i < deck.Count && amountOfCardsPutBack < cardsInDrawOrder.Count)
        {
            CardGroupInstance groupToPutCardsTo = deck[i];
            int amountOfCardsToPutBack = Mathf.Min(cardsInDrawOrder.Count - amountOfCardsPutBack, groupToPutCardsTo.cards.Count);
            groupToPutCardsTo.PutBackCardsOnTopOfGroup(cardsInDrawOrder.GetRange(amountOfCardsPutBack, amountOfCardsToPutBack));
            amountOfCardsPutBack += amountOfCardsToPutBack;
            i++;
        }
        UpdateDeckVisual();
    }

    public void KeepOneCardFromTopCards(int indexCardToKeep, int amountOfCardsFromTop)
    {
        int currentIndex = indexCardToKeep;
        int amountOfCardsRemaining = amountOfCardsFromTop;
        int groupIndex = 0;
        while(amountOfCardsRemaining > 0 && groupIndex < deck.Count)
        {
            CardGroupInstance currentGroup = deck[groupIndex];
            if(currentGroup.cards.Count <= currentIndex)
            {
                deck.RemoveAt(0);
                currentIndex -= currentGroup.cards.Count;
                amountOfCardsRemaining -= currentGroup.cards.Count;
            }
            else if(currentIndex >= 0)
            {
                currentGroup.cards.RemoveRange(0, currentIndex);
                amountOfCardsRemaining -= currentIndex + 1;
                currentIndex = -1;
                int amountToRemoveAfterIndex = Mathf.Min(currentGroup.cards.Count - 1, amountOfCardsRemaining);
                currentGroup.cards.RemoveRange(1, amountToRemoveAfterIndex);
                amountOfCardsRemaining -= amountToRemoveAfterIndex;
                groupIndex++;
            }
            else
            {
                if(currentGroup.cards.Count <= amountOfCardsRemaining)
                {
                    deck.RemoveAt(groupIndex);
                    amountOfCardsRemaining -= currentGroup.cards.Count;
                }
                else
                {
                    currentGroup.cards.RemoveRange(0, amountOfCardsRemaining);
                    amountOfCardsRemaining = 0;
                }
            }
        }
        UpdateDeckVisual();
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
            UpdateDeckVisual();
        }
    }

    public void ShuffleCardInGroup(CardInstance card, int groupIndex, int beginRange = 0, int endRange = -1)
    {
        if(deck.Count > 0)
        {
            groupIndex = groupIndex == -1 ? deck.Count - 1 : groupIndex;
            groupIndex = Mathf.Clamp(groupIndex, 0, deck.Count - 1);
            deck[groupIndex].ShuffleCardInGoup(card, beginRange, endRange);
            UpdateDeckVisual();
        }
    }

    private void UpdateDeckVisual()
    {
        int cardCount = 0;
        foreach(CardGroupInstance cardGroup in deck) { cardCount += cardGroup.cards.Count; }

        if(cardCount > 0)
        {
            transform.localScale = new Vector3(initScale.x, initScale.y / initSize * cardCount, initScale.z);
            transform.position = new Vector3(initPos.x, initPos.y - ((initSize - cardCount) *  0.5f / initSize * initScale.y), initPos.z);
            GetComponent<MeshRenderer>().enabled = true;
            TopcardDisplayer.GetComponent<MeshRenderer>().enabled = true;
        }
        else
        {
            GetComponent<MeshRenderer>().enabled = false;
            TopcardDisplayer.GetComponent<MeshRenderer>().enabled = false;
        }
        //temp, à mettre ailleurs probablement
        CardData topCardData = null;
        if(deck.Count > 0)
        {
            if(deck[0].cards.Count > 0)
            {
                topCardData = deck[0].cards[0].dataInstance;
            }
            else if(deck.Count > 1 && deck[1].cards.Count > 0)
            {
                topCardData = deck[1].cards[0].dataInstance;
            }
        }
        if(topCardData != null)
        {
            if(topCardData is MonsterCardData)
                TopcardDisplayer.GetComponent<MeshRenderer>().material = MonsterCardBack;
            if (topCardData is ObjectCardData)
                TopcardDisplayer.GetComponent<MeshRenderer>().material = ObjectCardBack;
            if (topCardData is EventCardData)
                TopcardDisplayer.GetComponent<MeshRenderer>().material = EventCardBack;
        }
    }

    public void DebugDraw()
    {
        return;
        foreach (CardGroupInstance cardGroup in deck)
        {
            Debug.Log("----New region----");
            cardGroup.DebugDraw();
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
