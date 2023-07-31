using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardInstance
{
    static private uint nextCardID = 0;

    public CardData dataInstance;
    public uint cardID;
        
    public CardInstance(CardData data)
    {
        cardID = nextCardID;
        nextCardID++;
        dataInstance = Object.Instantiate(data);
    }
}
