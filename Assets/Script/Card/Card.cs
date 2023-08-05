using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject nameText;
    public GameObject descriptionText;
    public CardInstance data;

    public ICardInteractionHandler interactionHandler;

    private bool active = false;

    public void Activate()
    {
        active = true;
    }
    public virtual void ApplyCardData(CardInstance data)
    {
        var manager = GameManager.Instance;
        this.data = data;
        if (manager.haveGrimoire || data.dataInstance is not ObjectCardData || !((ObjectCardData)data.dataInstance).isPotion)
        {
            nameText.GetComponent<TMP_Text>().text = data.dataInstance.cardName;
            descriptionText.GetComponent<TMP_Text>().text = data.dataInstance.cardText;
            if (data.dataInstance.cardImage)
                GetComponent<MeshRenderer>().material = data.dataInstance.cardImage;
        }
        else
        {
            nameText.GetComponent<TMP_Text>().text = manager.mysteriousPotionData.cardName;
            descriptionText.GetComponent<TMP_Text>().text = manager.mysteriousPotionData.cardText;
            if (data.dataInstance.cardImage)
                GetComponent<MeshRenderer>().material = manager.mysteriousPotionData.cardImage;
        }
    }

    public virtual void Resolve()
    {
        data.dataInstance.Resolve();
        Destroy(gameObject);
    }

    ////////////Interaction Implementation////////////////
    public void OnPointerDown(PointerEventData eventData)
    {
        if(active) interactionHandler.OnCardPressed(gameObject);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(active) interactionHandler.OnCardReleased(gameObject);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(active) interactionHandler.OnCardClicked(gameObject);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(active) interactionHandler.OnCardEntered(gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(active) interactionHandler.OnCardExited(gameObject);
    }

    ////////////////////////////////////////////////////////////////
}
