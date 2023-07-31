using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DeckData/Deck")]
public class DeckData : ScriptableObject
{
    public List<CardGroup> groups;
}
