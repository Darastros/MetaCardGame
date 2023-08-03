using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card/default")]
public class CardData : ScriptableObject
{
    public string cardName;
    public Material cardImage;
    public string cardText;

    public virtual void Resolve() {}
}