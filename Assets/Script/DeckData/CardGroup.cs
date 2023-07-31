using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DeckData/CardGroup")]
public class CardGroup : ScriptableObject
{
    public List<CardPool> cardPools;

    public List<CardInstance> GetCards()
    {
        List<CardInstance> cards = new List<CardInstance>();
        foreach(CardPool cardPool in cardPools)
        {
            List<CardInstance> cardsFromPool = cardPool.GetCards();
            foreach(CardInstance card in cardsFromPool)
            {
                cards.Add(card);
            }
        }
        return cards;
    }
}
