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

    public virtual void ApplyCardData(CardInstance data)
    {
        this.data = data;
        nameText.GetComponent<TMP_Text>().text = data.dataInstance.cardName;
        descriptionText.GetComponent<TMP_Text>().text = data.dataInstance.cardText;
        if (data.dataInstance.cardImage)
            GetComponent<MeshRenderer>().material = data.dataInstance.cardImage;
    }

    public virtual void Resolve()
    {
        Destroy(gameObject);
    }

    ////////////Interaction Implementation////////////////
    public void OnPointerDown(PointerEventData eventData)
    {
        interactionHandler.OnCardPressed(gameObject);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        interactionHandler.OnCardReleased(gameObject);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        interactionHandler.OnCardClicked(gameObject);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        interactionHandler.OnCardEntered(gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        interactionHandler.OnCardExited(gameObject);
    }

    ////////////////////////////////////////////////////////////////
}
