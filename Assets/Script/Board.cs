using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public void RevealCard()
    {
        GameManager.Instance.RevealCard();
    }
    
    public void WinBattleApplyEffect()
    {
        GameManager.Instance.WinBattleApplyEffect();
    }
    
    public void LooseBattleApplyEffect()
    {
        GameManager.Instance.LooseBattleApplyEffect();
    }
    
    public void DestroyCard()
    {
        GameManager.Instance.DestroyCard();
    }

    public void ShuffleCard()
    {
        GameManager.Instance.ShuffleCard();
    }

    public void PlaySound(string soundKey)
    {
        var sound = FMODUnity.RuntimeManager.CreateInstance(soundKey);
        sound.start();
    }
}
