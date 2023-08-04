using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DeckData/CardPoolWithRules")]
public class CardPoolWithRules : CardPool
{
    public int amountOfCardsfromPool;
    public int minimumOfEachCard;
    public int maximumOfEachCard;

    public override List<CardInstance> GetCards()
    {
        List<CardInstance> cardInstances = new List<CardInstance>();
        int amountOfSelectedCards = 0;
        for(int i = 0; i < minimumOfEachCard; i++)
        {
            foreach (CardData card in cards)
            {
                cardInstances.Add(new CardInstance(card));
                amountOfSelectedCards++;
                if (amountOfSelectedCards >= amountOfCardsfromPool)
                    return cardInstances;
            }
        }

        List<int> amountSelectedPerCard = new List<int>();
        for (int i = 0; i < cards.Count; i++) { amountSelectedPerCard.Add(minimumOfEachCard); }

        while (amountOfSelectedCards < amountOfCardsfromPool)
        {
            int selectedIndex = SelectCardInList(amountSelectedPerCard);
            if (selectedIndex < 0)
                break; 
            cardInstances.Add(new CardInstance(cards[selectedIndex]));
            amountSelectedPerCard[selectedIndex]++;
            amountOfSelectedCards++;
        }
        
        return cardInstances;
    }

    public virtual int SelectCardInList(List<int> amountAlreadySelectedPerCard)
    {
        List<int> indexToSelectFrom = new List<int>();

        for(int i = 0; i < amountAlreadySelectedPerCard.Count; i++)
        {
            if (amountAlreadySelectedPerCard[i] < maximumOfEachCard)
                indexToSelectFrom.Add(i);
        }

        if (indexToSelectFrom.Count > 0)
            return indexToSelectFrom[Random.Range(0, indexToSelectFrom.Count)];
        else
            return -1;
    }


}
