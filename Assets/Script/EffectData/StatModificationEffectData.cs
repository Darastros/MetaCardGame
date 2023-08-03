using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Effect/StatModification/default")]
public class StatModificationEffect : EffectData
{
    public GameManager.PlayerStat affectedStat;
    public int value = 0;
    public int duration = -1;
}
