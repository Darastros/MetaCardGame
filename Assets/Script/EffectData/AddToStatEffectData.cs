using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Effect/StatModification/AddToStat")]
public class AddToStatEffectData : StatModificationEffect
{
    public override void ApplyEffect()
    {
        if (duration > 0)
        {
            GameManager.Instance.ApplyStatModifier(affectedStat, false, value, duration);
        }
        else
        {
            GameManager.Instance.AddToStat(affectedStat, value);
        }
    }
}
