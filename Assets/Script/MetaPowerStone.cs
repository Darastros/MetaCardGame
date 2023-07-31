using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MetaPowerStone : MonoBehaviour, IPointerClickHandler
{
    public GameObject particleSpread;
    public GameObject particleGlow;
    public GameObject particleBurst;

    public int magicNeededToActivate = 3;
    public GameManager.MetaPower powerType;

    public void Activate()
    {
        GameManager.Instance.AddMagic(-magicNeededToActivate);
    }

    public void NotifyMagicAmountChanged(int currentMagic)
    {
        if(currentMagic >= magicNeededToActivate)
        {
            ParticleSystem.EmissionModule particleSpreadEmission = particleSpread.GetComponent<ParticleSystem>().emission;
            ParticleSystem.EmissionModule particleGlowEmission = particleGlow.GetComponent<ParticleSystem>().emission;
            particleSpreadEmission.enabled = true;
            particleGlowEmission.enabled = true;
            particleBurst.GetComponent<ParticleSystem>().Play();
            Material scryStoneMat = GetComponent<MeshRenderer>().material;
            scryStoneMat.color = new Color(1, 1, 1);
        }
        else
        {
            ParticleSystem.EmissionModule particleSpreadEmission = particleSpread.GetComponent<ParticleSystem>().emission;
            ParticleSystem.EmissionModule particleGlowEmission = particleGlow.GetComponent<ParticleSystem>().emission;
            particleSpreadEmission.enabled = false;
            particleGlowEmission.enabled = false;
            Material scryStoneMat = GetComponent<MeshRenderer>().material;
            scryStoneMat.color = new Color(0.5f, 0.5f, 0.5f);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Instance.OnMetaPowerStoneClicked(GameManager.Instance.playerMagic >= magicNeededToActivate, powerType);
    }
}
