using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour, ICardInteractionHandler
{
    /////////////SINGLETON IMPLEMENTATION/////////////////
    private static GameManager instance = null;
    public static GameManager Instance => instance;
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(this.gameObject);
        InitGame();
    }
    ///////////////////////////////////////////////////////

    public enum GameState
    {
        MAIN,
        NOINTERACTION,
        METAPOWER
    }

    private GameState currentGameState = GameState.MAIN;

    public GameObject sceneVFX;

    public GameObject deck;

    /////////Meta Powers/////
    public enum MetaPower
    {
        NONE,
        REROLL,
        DISCARD,
        SCRY,
        SKIP
    }
    private MetaPower currentlyUsedPower;

    public GameObject rerollStone;
    public GameObject discardStone;
    public GameObject scryStone;
    public GameObject skipStone;

    public uint cardsSeenInScry = 5;
    public uint cardsSeenInDiscard = 3;
    ///////////////////////

    public GameObject cardPrefab;
    public GameObject objectCardPrefab;
    public GameObject monsterCardPrefab;
    public GameObject currentCard;

    public GameObject whiteDicePrefab;
    public GameObject redDicePrefab;
    public GameObject whiteDice;
    public GameObject redDice;
    private int whiteDiceResult = -1;
    private int redDiceResult = -1;

    public GameObject cardListPrefab;
    public GameObject metaPowerCardList;

    public GameObject holdedCard;
    public int holdCardPlaneHeight = 5;
    public float holdCardScaleMultiplier = 0.8f;
    public float cardListScaleMultiplier = 0.5f;
    private Plane holdCardPlane;

    private GameObject selectedCard;
    public Vector3 selectedCardTranslation;


    //gameState
    bool canRevealNextCard = true;

    public GameObject lifeText;
    public GameObject strengthText;
    public GameObject coinText;

    public GameObject magicText;
    public GameObject deathIcon;

    public int playerStartingLife = 3;
    public int playerStartingStrength = 0;
    public int playerStartingCoin = 5;

    public int playerLife { get; private set; }
    public int playerStrength { get; private set; }
    public int playerCoin { get; private set; }

    public int playerStartingMagic = 3;
    public int playerMagic { get; private set; }

    private void InitGame()
    {
        playerLife = 0 ;
        playerStrength = 0;
        playerCoin = 0;
        playerMagic = 0;
        AddLife(playerStartingLife);
        AddStrength(playerStartingStrength);
        AddCoin(playerStartingCoin);
        AddMagic(playerStartingMagic);
        lifeText.GetComponent<TMP_Text>().text = playerLife.ToString();
        strengthText.GetComponent<TMP_Text>().text = playerStrength.ToString();
        canRevealNextCard = true;
        currentGameState = GameState.MAIN;
        holdCardPlane = new Plane(Vector3.up, -holdCardPlaneHeight);
    }
    public void RestartGame()
    {
        if (currentCard)
            Destroy(currentCard);
        deck.GetComponent<Deck>().ReShuffle();
        InitGame();
    }


    public GameObject CreateCardObject(CardInstance cardInstance)
    {
        CardData data = cardInstance.dataInstance;
        GameObject cardObject;
        if (data is ObjectCardData)
        {
            cardObject = Instantiate(objectCardPrefab);
        }
        else if(data is MonsterCardData)
        {
            cardObject = Instantiate(monsterCardPrefab);
        }
        else
        {
            cardObject = Instantiate(cardPrefab);
        }

        cardObject.GetComponent<Card>().ApplyCardData(cardInstance);
        return cardObject;
    }

    public void OnDeckClicked(int deckSize)
    {
        if(canRevealNextCard && currentGameState == GameState.MAIN)
        {
            if(deckSize > 0)
            {
                CardInstance revealedCard = deck.GetComponent<Deck>().GetTopCard();
                currentCard = CreateCardObject(revealedCard);
                currentCard.GetComponent<Card>().interactionHandler = this;
                canRevealNextCard = false;
            }
        }
    }

    public void OnMetaPowerStoneClicked(bool isActivable, MetaPower powerType)
    {
        if (isActivable && currentGameState == GameState.MAIN)
        {
            switch(powerType)
            {
                case MetaPower.REROLL:
                    break;

                case MetaPower.DISCARD:
                    if (!StartMetaPowerDiscard(cardsSeenInDiscard))
                        return;
                    currentGameState = GameState.METAPOWER;
                    sceneVFX.GetComponent<SceneVFX>().SwitchMode(SceneVFX.Mode.SCRY);
                    currentlyUsedPower = MetaPower.DISCARD;
                    break;

                case MetaPower.SCRY:
                    if (!StartMetaPowerScry(cardsSeenInScry))
                        return;
                    currentGameState = GameState.METAPOWER;
                    sceneVFX.GetComponent<SceneVFX>().SwitchMode(SceneVFX.Mode.SCRY);
                    currentlyUsedPower = MetaPower.SCRY;
                    break;

                case MetaPower.SKIP:
                    if (!ExecuteMetaPowerSkip())
                        return;
                    break;
            }

            scryStone.GetComponent<MetaPowerStone>().Activate();
        }
        else if (currentGameState == GameState.METAPOWER && powerType == currentlyUsedPower)
        {
            currentGameState = GameState.MAIN;
            sceneVFX.GetComponent<SceneVFX>().SwitchMode(SceneVFX.Mode.CANDLE);

            if(powerType == MetaPower.SCRY)
            {
                StopMetaPowerScry();
            }
            else if (powerType == MetaPower.DISCARD)
            {
                StopMetaPowerDiscard();
            }
        }
    }

    public bool StartMetaPowerDiscard(uint cardsSeen)
    {
        if (deck.GetComponent<Deck>().deck.Count > 0)
        {
            metaPowerCardList = Instantiate(cardListPrefab);
            InteractibleCardList cardListComponent = metaPowerCardList.GetComponent<InteractibleCardList>();
            List<CardInstance> topCards = deck.GetComponent<Deck>().GetTopCards(cardsSeen);
            foreach (CardInstance card in topCards)
            {
                GameObject cardObject = CreateCardObject(card);
                Vector3 prefabScale = cardPrefab.transform.localScale;
                cardObject.transform.localScale = new Vector3(prefabScale.x * cardListScaleMultiplier, prefabScale.y, prefabScale.z * cardListScaleMultiplier);
                cardListComponent.AddCardToList(cardObject);
            }
            selectedCard = cardListComponent.cardList[0];
            return true;
        }
        return false;
    }

    public void StopMetaPowerDiscard()
    {
        foreach (GameObject card in metaPowerCardList.GetComponent<InteractibleCardList>().cardList)
        {
            if (card == selectedCard)
                deck.GetComponent<Deck>().ShuffleCardInDeck(card.GetComponent<Card>().data, 0, 0);
            Destroy(card);
        }

        Destroy(metaPowerCardList);
    }

    public bool StartMetaPowerScry(uint cardsSeen)
    {
        if (deck.GetComponent<Deck>().deck.Count > 0)
        {
            metaPowerCardList = Instantiate(cardListPrefab);
            InteractibleCardList cardListComponent = metaPowerCardList.GetComponent<InteractibleCardList>();
            List<CardInstance> topCards = deck.GetComponent<Deck>().GetTopCards(cardsSeen);
            foreach (CardInstance card in topCards)
            {
                GameObject cardObject = CreateCardObject(card);
                Vector3 prefabScale = cardPrefab.transform.localScale;
                cardObject.transform.localScale = new Vector3(prefabScale.x * cardListScaleMultiplier, prefabScale.y, prefabScale.z * cardListScaleMultiplier);
                cardListComponent.AddCardToList(cardObject);
            }
            return true;
        }
        return false;
    }

    public void StopMetaPowerScry()
    {
        for (int i = metaPowerCardList.GetComponent<InteractibleCardList>().cardList.Count - 1; i >= 0; i--)
        {
            GameObject card = metaPowerCardList.GetComponent<InteractibleCardList>().cardList[i];
            deck.GetComponent<Deck>().ShuffleCardInDeck(card.GetComponent<Card>().data, 0, 0);
            Destroy(card);
        }

        Destroy(metaPowerCardList);
    }

    public bool ExecuteMetaPowerSkip()
    {
        if(currentCard)
        {
            deck.GetComponent<Deck>().ShuffleCardInDeck(currentCard.GetComponent<Card>().data);
            Destroy(currentCard);
            canRevealNextCard = true;
            OnDeckClicked(deck.GetComponent<Deck>().deck.Count);
            return true;
        }
        return false;
    }

    public void OnDeckRightClicked()
    {
        RestartGame();
    }

    public void AddCardToDeck(CardInstance card)
    {
        deck.GetComponent<Deck>().ShuffleCardInDeck(card);
    }

    public void AddLife(int amount)
    {
        playerLife += amount;
        lifeText.GetComponent<TMP_Text>().text = playerLife.ToString();
        if (playerLife <= 0)
        {
            playerLife = 0;
            deathIcon.SetActive(true);
            deathIcon.GetComponent<BasicAnimation>().PlayAnimation();
            Invoke("RestartGame", deathIcon.GetComponent<BasicAnimation>().duration);
            currentGameState = GameState.NOINTERACTION;
        }
    }

    public void AddStrength(int amount)
    {
        playerStrength += amount;
        if (playerStrength < 0) playerStrength = 0;
        strengthText.GetComponent<TMP_Text>().text = playerStrength.ToString();
    }

    public void AddCoin(int amount)
    {
        playerCoin += amount;
        if (playerCoin < 0) playerCoin = 0;
        coinText.GetComponent<TMP_Text>().text = playerCoin.ToString();
    }

    public void AddMagic(int amount)
    {
        playerMagic += amount;
        if (playerMagic < 0) playerMagic = 0;
        magicText.GetComponent<TMP_Text>().text = playerMagic.ToString();

        rerollStone.GetComponent<MetaPowerStone>().NotifyMagicAmountChanged(playerMagic);
        discardStone.GetComponent<MetaPowerStone>().NotifyMagicAmountChanged(playerMagic);
        scryStone.GetComponent<MetaPowerStone>().NotifyMagicAmountChanged(playerMagic);
        skipStone.GetComponent<MetaPowerStone>().NotifyMagicAmountChanged(playerMagic);
    }

    public void OnCartHoldStart(GameObject card)
    {
        holdedCard = card;
        Vector3 prefabScale = cardPrefab.transform.localScale;
        card.transform.localScale = new Vector3(prefabScale.x * holdCardScaleMultiplier, prefabScale.y, prefabScale.z * holdCardScaleMultiplier);
    }

    public void OnCartHoldStop(GameObject card)
    {
        holdedCard = null;
        Vector3 prefabScale = cardPrefab.transform.localScale;
        card.transform.localScale = new Vector3(prefabScale.x * cardListScaleMultiplier, prefabScale.y, prefabScale.z * cardListScaleMultiplier);

        Vector3 targetPosition = new Vector3();
        Plane cardListPlane = new Plane(Vector3.up, -metaPowerCardList.transform.position.y);
        float distance;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (cardListPlane.Raycast(ray, out distance))
        {
            targetPosition = ray.GetPoint(distance);
        }
        metaPowerCardList.GetComponent<InteractibleCardList>().ReleaseCardAtPos(targetPosition, card);
    }

    private void SelectCardInList(GameObject card)
    {
        if(card != selectedCard)
        {
            metaPowerCardList.GetComponent<InteractibleCardList>().MoveCardsToDefaultPosition();
            selectedCard = card;
            selectedCard.transform.position = selectedCard.transform.position + selectedCardTranslation;
        }
    }

    public void StartBattleAgainstMonster()
    {
        currentGameState = GameState.NOINTERACTION;
        RollDice();
    }

    private void CompleteBattle()
    {
        MonsterCardData monsterData = (MonsterCardData)currentCard.GetComponent<Card>().data.dataInstance;
        if(monsterData.strength + redDiceResult > playerStrength + whiteDiceResult)
        {
            deck.GetComponent<Deck>().ShuffleCardInDeck(currentCard.GetComponent<Card>().data);
            AddLife(-1);
        }
        Destroy(currentCard);
        Destroy(whiteDice);
        Destroy(redDice);
        currentGameState = GameState.MAIN;
    }

    private void RollDice()
    {
        whiteDiceResult = -1;
        redDiceResult = -1;

        if (whiteDice)
            Destroy(whiteDice);

        if (redDice)
            Destroy(redDice);

        whiteDice = Instantiate(whiteDicePrefab);
        redDice = Instantiate(redDicePrefab);
        whiteDice.GetComponent<DiceRolling>().RollTheDice();
        redDice.GetComponent<DiceRolling>().RollTheDice();
    }
    public void OnDiceResult(GameObject dice, int result)
    {
        if(dice == whiteDice)
        {
            whiteDiceResult = result;
        }
        else if(dice == redDice)
        {
            redDiceResult = result;
        }

        if(whiteDiceResult > 0 && redDiceResult > 0)
            CompleteBattle();
    }

    private void Update()
    {
        if(holdedCard)
        {
            Vector3 targetPosition = new Vector3();
   
            float distance;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (holdCardPlane.Raycast(ray, out distance))
            {
                targetPosition = ray.GetPoint(distance);
            }
            holdedCard.transform.position = targetPosition;
        }
    }

    //////////////////// HANDLE CARD INTERACTION///////////////////////
    public void OnCardPressed(GameObject card)
    {
        if(currentGameState == GameState.METAPOWER && currentlyUsedPower == MetaPower.SCRY)
        {
            if(metaPowerCardList.GetComponent<InteractibleCardList>().IsCardInList(card))
                OnCartHoldStart(card);
        }
    }
    public void OnCardReleased(GameObject card)
    {
        if (currentGameState == GameState.METAPOWER && currentlyUsedPower == MetaPower.SCRY)
        {
            if(card == holdedCard)
                OnCartHoldStop(card);
        }
    }
    public void OnCardClicked(GameObject card)
    {
        if (currentGameState == GameState.MAIN)
        {
            currentCard.GetComponent<Card>().Resolve();
            canRevealNextCard = true;
        }
        else if (currentGameState == GameState.METAPOWER && currentlyUsedPower == MetaPower.DISCARD)
        {
            SelectCardInList(card);
        }
    }
    public void OnCardEntered(GameObject card)
    {

    }
    public void OnCardExited(GameObject card)
    {

    }
    ///////////////////////////////////////////////////////////////////

}
