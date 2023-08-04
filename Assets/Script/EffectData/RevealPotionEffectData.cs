using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Effect/Reveal potion")]
public class RevealPotionEffectData : EffectData
{
    public override void ApplyEffect()
    {
        GameManager.Instance.haveGrimoire = true;
    }
}
