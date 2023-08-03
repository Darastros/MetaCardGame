using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card/Object")]
public class ObjectCardData : CardData
{
    public List<EffectData> objectEffects;

    public override void Resolve()
    {
        foreach (EffectData effect in objectEffects)
        {
            effect.ApplyEffect();
        }
    }
}
