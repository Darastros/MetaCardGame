using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Effect/StatModification/SetStat")]
public class SetStatEffectData : StatModificationEffect
{
    public override void ApplyEffect()
    {
        if(duration > 0)
        {
            GameManager.Instance.ApplyStatModifier(affectedStat, true, value, duration);
        }
        else
        {
            GameManager.Instance.SetStatToValue(affectedStat, value);
        }
    }
}
