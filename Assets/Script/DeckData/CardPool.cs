using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DeckData/CardPool")]
public class CardPool : ScriptableObject
{
    public List<CardData> cards;

    public virtual List<CardInstance> GetCards()
    {
        List<CardInstance> cardInstances = new List<CardInstance>();
        foreach(CardData card in cards)
        {
            CardInstance newCardInstance = new CardInstance(card);
            cardInstances.Add(newCardInstance);
        }

        return cardInstances;
    }
}
