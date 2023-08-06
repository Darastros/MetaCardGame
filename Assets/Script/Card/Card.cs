using System;
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

    private Vector3 targetPosition;
    private bool gotoPosition = false;
    private bool isAnimated = false;

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

    public void GoTo(Vector3 position, bool animated = true)
    {
        targetPosition = position;
        gotoPosition = true;
        isAnimated = animated;
    }

    private void FixedUpdate()
    {
        if (!gotoPosition) return;
        
        Vector3 desiredPosition = Vector3.Lerp(transform.position, targetPosition, 0.2f);
        Vector3 velocity = (desiredPosition - transform.position)/Time.deltaTime;
            
        var cardAnimator = GetComponent<Animator>();

        cardAnimator.SetFloat("vertical", Mathf.Lerp(cardAnimator.GetFloat("vertical"), isAnimated ? -velocity.z / 10.0f : 0.0f, 0.2f));
        cardAnimator.SetFloat("horizontal", Mathf.Lerp(cardAnimator.GetFloat("horizontal"), isAnimated ? velocity.x / 10.0f : 0.0f, 0.2f));
        if ((targetPosition - desiredPosition).magnitude < 0.01f)
        {
            gotoPosition = false;
            transform.position = targetPosition;
            cardAnimator.SetFloat("vertical", 0.0f);
            cardAnimator.SetFloat("horizontal", 0.0f);
        }
        else transform.position = desiredPosition;
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
