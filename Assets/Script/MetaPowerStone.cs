using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class MetaPowerStone : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject particleSpread;
    public GameObject particleGlow;
    public GameObject particleBurst;

    public GameObject descriptionText;
    public GameObject interactionText;

    public int magicCost = 3;
    public GameManager.MetaPower powerType;

    public bool isBeingUsed = false;
    public bool isInteractible = true;
    public bool isDisplayingInterraction = true;

    private void Awake()
    {
        interactionText.GetComponent<TMP_Text>().color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
    }

    public void Activate()
    {
        GameManager.Instance.AddToStat(GameManager.PlayerStat.MAGIC, -magicCost);
        isBeingUsed = true;
        UpdateDisplay();
    }

    public void Deactivate()
    {
        isBeingUsed = false;
        UpdateDisplay();
    }

    public void NotifyMagicAmountChanged()
    {
        if(GameManager.Instance.GetStatCurrentValue(GameManager.PlayerStat.MAGIC) >= magicCost)
            particleBurst.GetComponent<ParticleSystem>().Play();
        UpdateDisplay();
    }

    public void NotifyMetaPowerStart(GameManager.MetaPower power)
    {
        if (power == powerType)
        {
            isInteractible = true;
            interactionText.GetComponent<TMP_Text>().text = "Confirm";
            interactionText.GetComponent<TMP_Text>().fontSize = 32;
        }
        else
        {
            isInteractible = false;
        }
        UpdateDisplay();
    }

    public void NotifyMetaPowerStop()
    {
        isInteractible = true;
        interactionText.GetComponent<TMP_Text>().text = "cost " + magicCost.ToString() + " magic";
        interactionText.GetComponent<TMP_Text>().fontSize = 16;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (isInteractible && GameManager.Instance.GetStatCurrentValue(GameManager.PlayerStat.MAGIC) >= magicCost)
        {
            ParticleSystem.EmissionModule particleSpreadEmission = particleSpread.GetComponent<ParticleSystem>().emission;
            ParticleSystem.EmissionModule particleGlowEmission = particleGlow.GetComponent<ParticleSystem>().emission;
            particleSpreadEmission.enabled = true;
            particleGlowEmission.enabled = true;
            Material scryStoneMat = GetComponent<MeshRenderer>().material;
            scryStoneMat.color = new Color(1.0f, 1.0f, 1.0f);
            descriptionText.GetComponent<TMP_Text>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }
        else
        {
            ParticleSystem.EmissionModule particleSpreadEmission = particleSpread.GetComponent<ParticleSystem>().emission;
            ParticleSystem.EmissionModule particleGlowEmission = particleGlow.GetComponent<ParticleSystem>().emission;
            particleSpreadEmission.enabled = false;
            particleGlowEmission.enabled = false;
            Material scryStoneMat = GetComponent<MeshRenderer>().material;
            scryStoneMat.color = new Color(0.5f, 0.5f, 0.5f);
            descriptionText.GetComponent<TMP_Text>().color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
        }
        if (isBeingUsed)
        {
            descriptionText.GetComponent<TMP_Text>().color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        }
        if (isDisplayingInterraction)
        {
            interactionText.GetComponent<TMP_Text>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            if(!isBeingUsed && GameManager.Instance.GetStatCurrentValue(GameManager.PlayerStat.MAGIC) < magicCost)
                interactionText.GetComponent<TMP_Text>().color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
            if(!isInteractible)
                interactionText.GetComponent<TMP_Text>().color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        }
        else
        {
            if(isBeingUsed)
                interactionText.GetComponent<TMP_Text>().color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
            else
                interactionText.GetComponent<TMP_Text>().color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Instance.OnMetaPowerStoneClicked(GameManager.Instance.playerMagic >= magicCost, powerType);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isDisplayingInterraction = true;
        UpdateDisplay();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isDisplayingInterraction = false;
        UpdateDisplay();
    }
}
