using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card/Event")]
public class EventCardData : CardData
{
    public int lifeAdd = 0;
    public int strengthAdd = 0;
    public int strengthSet = 0;
    public int coinAdd = 0;
    public int duration = -1;
}
