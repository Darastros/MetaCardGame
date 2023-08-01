using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card/Object")]
public class ObjectCardData : CardData
{
    public int lifeAdd = 0;
    public int strengthAdd = 0;
    public int strengthSet = 0;
    public int coinAdd = 0;
    public int duration = -1;
}
