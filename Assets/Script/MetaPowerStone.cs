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

    public GameObject stoneMesh;

    public int magicCost = 3;
    public GameManager.MetaPower powerType;

    public bool isBeingUsed = false;
    public bool isInteractible = true;
    public bool isDisplayingInterraction = false;

    public Vector3 positionWhenUsed;

    private Vector3 initPosition;

    private void Awake()
    {
        interactionText.GetComponent<TMP_Text>().color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        initPosition = transform.position;
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
            transform.position = positionWhenUsed;
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
        transform.position = initPosition;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (isInteractible && (GameManager.Instance.GetStatCurrentValue(GameManager.PlayerStat.MAGIC) >= magicCost || isBeingUsed))
        {
            ParticleSystem.EmissionModule particleSpreadEmission = particleSpread.GetComponent<ParticleSystem>().emission;
            ParticleSystem.EmissionModule particleGlowEmission = particleGlow.GetComponent<ParticleSystem>().emission;
            particleSpreadEmission.enabled = true;
            particleGlowEmission.enabled = true;
            Material scryStoneMat = stoneMesh.GetComponent<MeshRenderer>().material;
            scryStoneMat.color = new Color(1.0f, 1.0f, 1.0f);
            descriptionText.GetComponent<TMP_Text>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }
        else
        {
            ParticleSystem.EmissionModule particleSpreadEmission = particleSpread.GetComponent<ParticleSystem>().emission;
            ParticleSystem.EmissionModule particleGlowEmission = particleGlow.GetComponent<ParticleSystem>().emission;
            particleSpreadEmission.enabled = false;
            particleGlowEmission.enabled = false;
            Material scryStoneMat = stoneMesh.GetComponent<MeshRenderer>().material;
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
                interactionText.GetComponent<TMP_Text>().color = new Color(0.7f, 0.7f, 0.7f, 1.0f);
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
