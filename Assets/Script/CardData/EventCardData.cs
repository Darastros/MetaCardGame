using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card/Event")]
public class EventCardData : CardData
{
    public List<EffectData> eventEffects;

    public override void Resolve()
    {
        foreach (EffectData effect in eventEffects)
        {
            effect.ApplyEffect();
        }
    }
}
