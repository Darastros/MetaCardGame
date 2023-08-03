using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card/Monster")]
public class MonsterCardData : CardData
{
    public int strength = 0;
    public List<EffectData> effectsOnRevealed;
    public List<EffectData> effectsOnWinBattle;
    public List<EffectData> effectsOnDeafeated;

    public void OnRevealed()
    {
        foreach (EffectData effect in effectsOnRevealed)
        {
            effect.ApplyEffect();
        }
    }

    public void OnWinBattle()
    {
        foreach (EffectData effect in effectsOnWinBattle)
        {
            effect.ApplyEffect();
        }
    }

    public void OnDefeated()
    {
        foreach (EffectData effect in effectsOnDeafeated)
        {
            effect.ApplyEffect();
        }
    }
}
