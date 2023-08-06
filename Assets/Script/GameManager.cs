using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using TMPro;
using FMODUnity;
using Plane = UnityEngine.Plane;
using Vector3 = UnityEngine.Vector3;

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
        //DontDestroyOnLoad(this.gameObject);
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
        DISCARD,
        SCRY,
        SKIP
    }
    private MetaPower currentlyUsedPower;

    public GameObject discardStone;
    public GameObject scryStone;
    public GameObject skipStone;

    public int cardsSeenInScry = 5;
    public int cardsSeenInDiscard = 3;
    ///////////////////////
    public Animator animator;
    public CardData mysteriousPotionData;
    public GameObject cardPrefab;
    public GameObject objectCardPrefab;
    public GameObject monsterCardPrefab;
    public GameObject eventCardPrefab;
    public GameObject currentCard;
    public Transform cardTransform;

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

    public GameObject notePadInteractionText;
    public GameObject discardInteractionText;
    public GameObject notePadHighlight;
    public GameObject discardHighlight;

    private GameObject selectedCard;
    public Vector3 selectedCardTranslation;


    //gameState
    bool canRevealNextCard = true;
    public bool haveGrimoire = false;

    public GameObject lifeText;
    public GameObject strengthText;
    public GameObject coinText;

    public GameObject magicText;
    public GameObject deathIcon;

    public enum PlayerStat
    {
        LIFE,
        STRENGTH,
        COIN,
        MAGIC
    }

    [Header("Life")]
    public int playerStartingLife = 3;
    public int playerMaxLife = 10;
    public TextMeshProUGUI addLifeText;
    [Header("Strength")]
    public int playerStartingStrength = 0;
    public int playerMinStrength = 3;
    public TextMeshProUGUI addStrengthText;
    [Header("Coin")]
    public int playerStartingCoin = 5;
    [Header("Magic")]
    public int playerStartingMagic = 3;
    public int playerMaxMagic = 10;
    public TextMeshProUGUI addMagicText;

    //setModifiers
    private class ActiveStatModifier
    {
        public bool isSet = false;
        public int value = 0;
        public int remaningDuration = 0;
    }

    private ActiveStatModifier lifeModifier;
    private ActiveStatModifier strengthModifier;

    public int playerLife { get; private set; }
    public int playerStrength { get; private set; }
    public int playerCoin { get; private set; }
    public int playerMagic { get; private set; }

    private void InitGame()
    {
        playerLife = 0 ;
        playerStrength = 0;
        playerCoin = 0;
        playerMagic = 0;
        AddLife(playerStartingLife, true);
        AddStrength(playerStartingStrength, true);
        AddCoin(playerStartingCoin);
        AddMagic(playerStartingMagic, true);
        lifeModifier = null;
        strengthModifier = null;
        UpdateStatUI();
        canRevealNextCard = true;
        currentGameState = GameState.MAIN;
        holdCardPlane = new Plane(Vector3.up, -holdCardPlaneHeight);
        var startSound = FMODUnity.RuntimeManager.CreateInstance("event:/sfx/sfx_fight/sfx_fight_start");
        startSound.start();
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
        else if (data is EventCardData)
        {
            cardObject = Instantiate(eventCardPrefab);
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
                CardInstance revealedCard = deck.GetComponent<Deck>().DrawTopCard();
                if(revealedCard != null)
                {
                    animator.SetTrigger("RevealedCard");
                    var revealCard = FMODUnity.RuntimeManager.CreateInstance("event:/sfx/sfx_card/sfx_card_take");
                    revealCard.start();
                    currentCard = CreateCardObject(revealedCard);
                    currentCard.transform.parent = cardTransform;
                    currentCard.transform.localPosition = Vector3.zero;
                    Card card = currentCard.GetComponent<Card>();
                    card.interactionHandler = this;
                    if (card.data.dataInstance is MonsterCardData)
                        currentCard.GetComponent<MonsterCard>().OnRevealed();
                    canRevealNextCard = false;
                    DecreaseStatModifiersDuration();
                }
            }
        }
    }

    public void OnMetaPowerStoneClicked(bool isActivable, MetaPower powerType)
    {
        if (isActivable && currentGameState == GameState.MAIN)
        {
            switch(powerType)
            {
                case MetaPower.DISCARD:
                    if (!StartMetaPowerDiscard(cardsSeenInDiscard))
                        return;
                    currentGameState = GameState.METAPOWER;
                    sceneVFX.GetComponent<SceneVFX>().SwitchMode(SceneVFX.Mode.SCRY);
                    currentlyUsedPower = MetaPower.DISCARD;
                    discardStone.GetComponent<MetaPowerStone>().Activate();
                    NotifyStonePowerStart(MetaPower.DISCARD);
                    break;

                case MetaPower.SCRY:
                    if (!StartMetaPowerScry(cardsSeenInScry))
                        return;
                    currentGameState = GameState.METAPOWER;
                    sceneVFX.GetComponent<SceneVFX>().SwitchMode(SceneVFX.Mode.SCRY);
                    currentlyUsedPower = MetaPower.SCRY;
                    scryStone.GetComponent<MetaPowerStone>().Activate();
                    NotifyStonePowerStart(MetaPower.SCRY);
                    break;

                case MetaPower.SKIP:
                    if (!ExecuteMetaPowerSkip())
                        return;
                    skipStone.GetComponent<MetaPowerStone>().Activate();
                    skipStone.GetComponent<MetaPowerStone>().Deactivate();
                    break;
            }

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

            NotifyStonePowerStop();
        }
    }

    private void NotifyStonePowerStart(GameManager.MetaPower power)
    {
        discardStone.GetComponent<MetaPowerStone>().NotifyMetaPowerStart(power);
        scryStone.GetComponent<MetaPowerStone>().NotifyMetaPowerStart(power);
        skipStone.GetComponent<MetaPowerStone>().NotifyMetaPowerStart(power);
    }

    private void NotifyStonePowerStop()
    {
        discardStone.GetComponent<MetaPowerStone>().NotifyMetaPowerStop();
        scryStone.GetComponent<MetaPowerStone>().NotifyMetaPowerStop();
        skipStone.GetComponent<MetaPowerStone>().NotifyMetaPowerStop();
    }

    public bool StartMetaPowerDiscard(int cardsSeen)
    {
        if (deck.GetComponent<Deck>().deck.Count > 0)
        {
            metaPowerCardList = Instantiate(cardListPrefab);
            InteractibleCardList cardListComponent = metaPowerCardList.GetComponent<InteractibleCardList>();
            cardListComponent.instructionText.GetComponent<TMP_Text>().text = "Select a path, avoid the rest";
            List<CardInstance> topCards = deck.GetComponent<Deck>().LookAtTopCards(cardsSeen);
            foreach (CardInstance card in topCards)
            {
                GameObject cardObject = CreateCardObject(card);
                cardObject.GetComponent<Card>().Activate();
                Vector3 prefabScale = cardPrefab.transform.localScale;
                cardObject.transform.localScale = new Vector3(prefabScale.x * cardListScaleMultiplier, prefabScale.y * cardListScaleMultiplier, prefabScale.z);
                cardListComponent.AddCardToList(cardObject);
            }
            selectedCard = cardListComponent.cardList[0];
            return true;
        }
        return false;
    }

    public void StopMetaPowerDiscard()
    {
        List<GameObject> cardList = metaPowerCardList.GetComponent<InteractibleCardList>().cardList;
        for (int i = 0; i < cardList.Count; i++)
        {
            if (cardList[i] == selectedCard)
                deck.GetComponent<Deck>().KeepOneCardFromTopCards(i, metaPowerCardList.GetComponent<InteractibleCardList>().cardList.Count);
            
            if(cardList[i].GetComponent<Card>().data.dataInstance.cardName =="Dragon") OnPetitFilou();
            Destroy(cardList[i]);
        }

        Destroy(metaPowerCardList);
        discardStone.GetComponent<MetaPowerStone>().Deactivate();
    }

    public bool StartMetaPowerScry(int cardsSeen)
    {
        if (deck.GetComponent<Deck>().deck.Count > 0)
        {
            metaPowerCardList = Instantiate(cardListPrefab);
            InteractibleCardList cardListComponent = metaPowerCardList.GetComponent<InteractibleCardList>();
            cardListComponent.instructionText.GetComponent<TMP_Text>().text = "Order the events of your fate";
            List<CardInstance> topCards = deck.GetComponent<Deck>().LookAtTopCards(cardsSeen);
            foreach (CardInstance card in topCards)
            {
                GameObject cardObject = CreateCardObject(card);
                cardObject.GetComponent<Card>().Activate();
                Vector3 prefabScale = cardPrefab.transform.localScale;
                cardObject.transform.localScale = new Vector3(prefabScale.x * cardListScaleMultiplier, prefabScale.y * cardListScaleMultiplier, prefabScale.z);
                cardListComponent.AddCardToList(cardObject);
            }
            return true;
        }
        return false;
    }

    public void StopMetaPowerScry()
    {
        List<CardInstance> cardsToPutBackOnTop = new List<CardInstance>();
        InteractibleCardList cardListComponent = metaPowerCardList.GetComponent<InteractibleCardList>();
        for (int i = 0; i < cardListComponent.cardList.Count; i++)
        {
            GameObject card = cardListComponent.cardList[i];
            cardsToPutBackOnTop.Add(card.GetComponent<Card>().data);
            Destroy(card);
        }
        deck.GetComponent<Deck>().PutBackCardsOnTopOfDeck(cardsToPutBackOnTop);
        Destroy(metaPowerCardList);
        scryStone.GetComponent<MetaPowerStone>().Deactivate();
    }

    public bool ExecuteMetaPowerSkip()
    {
        if(currentCard)
        {
            //deck.GetComponent<Deck>().ShuffleCardInGroup(currentCard.GetComponent<Card>().data, 0);
            //Destroy(currentCard);
            //canRevealNextCard = true;
            animator.SetTrigger("Shuffle");
            currentGameState = GameState.NOINTERACTION;
            return true;
        }
        return false;
    }

    public void OnDeckRightClicked()
    {
        var playerIntro = FMODUnity.RuntimeManager.CreateInstance("event:/sfx/sfx_back_card_return/sfx_back_card_attack_impact");
        playerIntro.start();
        //RestartGame();
    }

    public int GetStatCurrentValue(PlayerStat stat)
    {
        int statValue = 0;
        switch (stat)
        {
            case PlayerStat.LIFE:
                if (lifeModifier != null)
                {
                    if (lifeModifier.isSet)
                        statValue = lifeModifier.value;
                    else
                        statValue = playerLife + lifeModifier.value;
                }
                else
                    statValue = playerLife;
                break;
            case PlayerStat.STRENGTH:
                if (strengthModifier != null)
                {
                    if (strengthModifier.isSet)
                        statValue = strengthModifier.value;
                    else
                        statValue = playerStrength + strengthModifier.value;
                }
                else
                    statValue = playerStrength;
                break;
            case PlayerStat.COIN:
                statValue = playerCoin;
                break;
            case PlayerStat.MAGIC:
                statValue = playerMagic;
                break;
        }
        return statValue;
    }

    public void AddToStat(PlayerStat stat, int value)
    {
        switch(stat)
        {
            case PlayerStat.LIFE:
                AddLife(value);
                break;
            case PlayerStat.STRENGTH:
                AddStrength(value);
                break;
            case PlayerStat.COIN:
                AddCoin(value);
                break;
            case PlayerStat.MAGIC:
                AddMagic(value);
                break;
        }
        UpdateStatUI();
    }

    public void SetStatToValue(PlayerStat stat, int value)
    {
        switch (stat)
        {
            case PlayerStat.LIFE:
                AddLife(value - playerLife);
                break;
            case PlayerStat.STRENGTH:
                AddStrength(value - playerStrength);
                break;
            case PlayerStat.COIN:
                AddCoin(value - playerCoin);
                break;
            case PlayerStat.MAGIC:
                AddMagic(value - playerMagic);
                break;
        }
        UpdateStatUI();
    }

    public void ApplyStatModifier(PlayerStat stat, bool isSet, int value, int duration)
    {
        switch (stat)
        {
            case PlayerStat.LIFE:
                ApplyLifeModifier(isSet, value, duration + 1); //hack to compensate for immediate loss of one turn
                break;
            case PlayerStat.STRENGTH:
                ApplyStrengthModifier(isSet, value, duration + 1); //same
                break;
            case PlayerStat.COIN:
                break;
            case PlayerStat.MAGIC:
                break;
        }
        UpdateStatUI();
    }

    public void AddLife(int value, bool ignoreAnim = false)
    {
        playerLife += value;
        if (!ignoreAnim)
        {
            addLifeText.text = value < 0 ? ""+value : "+"+value;
            animator.SetTrigger("AddLife");
        }
        
        playerLife = Mathf.Clamp(playerLife, 0, playerMaxLife);
        if (playerLife <= 0)
        {
            playerLife = 0;
            //deathIcon.SetActive(true);
            //deathIcon.GetComponent<BasicAnimation>().PlayAnimation();
            OnLoose();
            //Invoke("RestartGame", deathIcon.GetComponent<BasicAnimation>().duration);
            currentGameState = GameState.NOINTERACTION;
        }
    }

    public void AddStrength(int value, bool ignoreAnim = false)
    {
        playerStrength += value;
        if (!ignoreAnim)
        {
            addStrengthText.text = value < 0 ? ""+value : "+"+value;
            animator.SetTrigger("AddStrength");
        }

        playerStrength = Mathf.Max(playerStrength, playerMinStrength);
        if (playerStrength < 0) playerStrength = 0;
    }

    public void AddCoin(int value)
    {
        playerCoin += value;
        if (playerCoin < 0) playerCoin = 0;
    }

    public void AddMagic(int value, bool ignoreAnim = false)
    {
        playerMagic += value;
        if (!ignoreAnim)
        {
            addMagicText.text = value < 0 ? ""+value : "+"+value;
            animator.SetTrigger("AddMagic");
        }
        playerMagic = Mathf.Clamp(playerMagic, 0, playerMaxMagic);

        discardStone.GetComponent<MetaPowerStone>().NotifyMagicAmountChanged();
        scryStone.GetComponent<MetaPowerStone>().NotifyMagicAmountChanged();
        skipStone.GetComponent<MetaPowerStone>().NotifyMagicAmountChanged();
    }

    public void ApplyLifeModifier(bool isSet, int value, int duration)
    {
        ActiveStatModifier statModifier = new ActiveStatModifier();
        statModifier.isSet = isSet;
        statModifier.value = value;
        statModifier.remaningDuration = duration;
        lifeModifier = statModifier;
    }

    public void ApplyStrengthModifier(bool isSet, int value, int duration)
    {
        ActiveStatModifier statModifier = new ActiveStatModifier();
        statModifier.isSet = isSet;
        statModifier.value = value;
        statModifier.remaningDuration = duration;
        strengthModifier = statModifier;
    }

    public void OnCartHoldStart(GameObject card)
    {
        holdedCard = card;
        if (currentGameState == GameState.METAPOWER)
        {
            Vector3 prefabScale = cardPrefab.transform.localScale;
            card.transform.localScale = new Vector3(prefabScale.x * holdCardScaleMultiplier, prefabScale.y * holdCardScaleMultiplier, prefabScale.z);
        }
        else if(currentGameState == GameState.MAIN)
        {
            notePadInteractionText.SetActive(true);
            discardInteractionText.SetActive(true);
            notePadHighlight.SetActive(true);
            discardHighlight.SetActive(true);
        }
    }

    public void OnCartHoldStop(GameObject card)
    {
        holdedCard = null;

        if (currentGameState == GameState.METAPOWER)
        {
            Vector3 prefabScale = cardPrefab.transform.localScale;
            card.transform.localScale = new Vector3(prefabScale.x * cardListScaleMultiplier, prefabScale.y * cardListScaleMultiplier, prefabScale.z);
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
        else if(currentGameState == GameState.MAIN)
        {
            Vector3 targetPosition = new Vector3();
            Plane tablePlane = new Plane(Vector3.up, 0);
            float distance;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (tablePlane.Raycast(ray, out distance))
            {
                targetPosition = ray.GetPoint(distance);
            }

            if(targetPosition.x > 5.5f)
            {
                Destroy(currentCard);
                var sound = FMODUnity.RuntimeManager.CreateInstance("event:/sfx/sfx_int/sfx_int_item_leave");
                sound.start();
                canRevealNextCard = true;
            }
            else if(targetPosition.x < -5.7f)
            {
                if(currentCard.GetComponent<Card>().data.dataInstance.cardName.ToLower().Contains("potion"))
                {
                    var sound = FMODUnity.RuntimeManager.CreateInstance("event:/sfx/sfx_int/sfx_int_potion_drink");
                    sound.start();
                }
                else
                {
                    var sound = FMODUnity.RuntimeManager.CreateInstance("event:/sfx/sfx_int/sfx_int_item_pickup");
                    sound.start();
                }
                currentCard.GetComponent<Card>().Resolve();
                canRevealNextCard = true;
            }
            else
            {
                currentCard.GetComponent<Card>().GoTo(currentCard.transform.parent.position, false);
            }

            notePadInteractionText.SetActive(false);
            discardInteractionText.SetActive(false);
            notePadHighlight.SetActive(false);
            discardHighlight.SetActive(false);
        }
    }

    private void SelectCardInList(GameObject card)
    {
        metaPowerCardList.GetComponent<InteractibleCardList>().MoveCardsToDefaultPosition();
        selectedCard = card;
        selectedCard.GetComponent<Card>().GoTo(selectedCard.transform.position + selectedCardTranslation);
    }

    public void StartBattleAgainstMonster()
    {

        currentGameState = GameState.NOINTERACTION;
        MonsterCardData monsterData = (MonsterCardData)currentCard.GetComponent<Card>().data.dataInstance;
        if(monsterData.strength > GetStatCurrentValue(PlayerStat.STRENGTH) + 5)
        {
            animator.SetTrigger("LooseBattle");
        }
        else if(monsterData.strength + 5 <= GetStatCurrentValue(PlayerStat.STRENGTH))
        {
            animator.SetTrigger("WinBattle");
        }
        else RollDice();
    }

    private void CompleteBattle()
    {
        MonsterCardData monsterData = (MonsterCardData)currentCard.GetComponent<Card>().data.dataInstance;
        
        if(monsterData.strength + redDiceResult > GetStatCurrentValue(PlayerStat.STRENGTH) + whiteDiceResult)
        {
            animator.SetTrigger("LooseBattle");
        }
        else
        {
            animator.SetTrigger("WinBattle");
        }
    }

    private void RollDice()
    {
        var sound = FMODUnity.RuntimeManager.CreateInstance("event:/sfx/sfx_dice/sfx_dice_roll_launch");
        sound.start();
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
            strengthText.GetComponent<TMP_Text>().text = (GetStatCurrentValue(PlayerStat.STRENGTH) + result).ToString();
        }
        else if(dice == redDice)
        {
            redDiceResult = result;
            currentCard.GetComponent<MonsterCard>().strengthAttributeText.GetComponent<TMP_Text>().text = (((MonsterCardData)currentCard.GetComponent<Card>().data.dataInstance).strength + result).ToString();
        }

        if (whiteDiceResult > 0 && redDiceResult > 0)
            CompleteBattle();
    }

    private void FixedUpdate()
    {
        Plane planeUsed = new Plane(Vector3.up, -2.5f);
        if (currentGameState == GameState.METAPOWER)
            planeUsed = holdCardPlane;
        if(holdedCard)
        {
            Vector3 targetPosition = new Vector3();
   
            float distance;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (planeUsed.Raycast(ray, out distance))
            {
                targetPosition = ray.GetPoint(distance);
            }
            holdedCard.GetComponent<Card>().GoTo(targetPosition);

            //TEMP TRESSS MOCHE
            if (targetPosition.x > 6.5f)
            {
                notePadInteractionText.transform.localScale = new Vector3(1.6f, 2.1f, 1);
                discardInteractionText.transform.localScale = new Vector3(3.5f, 4.5f, 1);

            }
            else if (targetPosition.x < -6.7f)
            {
                notePadInteractionText.transform.localScale = new Vector3(2.8f, 3.7f, 1);
                discardInteractionText.transform.localScale = new Vector3(2, 2.5f, 1);
            }
            else
            {
                notePadInteractionText.transform.localScale = new Vector3(1.6f, 2.1f, 1);
                discardInteractionText.transform.localScale = new Vector3(2, 2.5f, 1);
            }
            //
        }
    }

    private void UpdateStatUI()
    {
        lifeText.GetComponent<TMP_Text>().text = GetStatCurrentValue(PlayerStat.LIFE) + "/" + playerMaxLife;
        strengthText.GetComponent<TMP_Text>().text = GetStatCurrentValue(PlayerStat.STRENGTH).ToString();
        coinText.GetComponent<TMP_Text>().text = GetStatCurrentValue(PlayerStat.COIN).ToString();
        magicText.GetComponent<TMP_Text>().text = GetStatCurrentValue(PlayerStat.MAGIC) + "/" + playerMaxMagic;
    }

    private void DecreaseStatModifiersDuration()
    {
        if(lifeModifier != null)
        {
            lifeModifier.remaningDuration -= 1;
            if (lifeModifier.remaningDuration <= 0)
                lifeModifier = null;
        }
        if (strengthModifier != null)
        {
            strengthModifier.remaningDuration -= 1;
            if (strengthModifier.remaningDuration <= 0)
                strengthModifier = null;
        }
        UpdateStatUI();
    }

    //////////////////// HANDLE CARD INTERACTION///////////////////////
    public void OnCardPressed(GameObject card)
    {
        if (currentGameState == GameState.MAIN)
        {
            if (currentCard && currentCard.GetComponent<Card>() is ObjectCard)
            {
                OnCartHoldStart(card);
            }
        }
        if (currentGameState == GameState.METAPOWER && currentlyUsedPower == MetaPower.SCRY)
        {
            if (metaPowerCardList.GetComponent<InteractibleCardList>().IsCardInList(card))
                OnCartHoldStart(card);
        }
    }
    public void OnCardReleased(GameObject card)
    {
        if (currentGameState == GameState.MAIN)
        {
            if (card == holdedCard)
                OnCartHoldStop(card);
        }
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
            if(!(currentCard.GetComponent<Card>() is ObjectCard))
            {
                currentCard.GetComponent<Card>().Resolve();
                canRevealNextCard = true;
            }
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

    private void OnLoose()
    {
        animator.SetTrigger("OnLoose");
    }
    private void OnWin()
    {
        animator.SetTrigger("OnWin");
        
    }
    private void OnPetitFilou()
    {
        animator.SetTrigger("OnPetitFilou");
    }
    
    ///////////////////////////////////////////////////////////////////

    public void RevealCard()
    {
        currentCard.GetComponent<Card>().Activate();
    }
    public void WinBattleApplyEffect()
    {
        MonsterCardData monsterData = (MonsterCardData)currentCard.GetComponent<Card>().data.dataInstance;
        monsterData.OnDefeated();
        if(monsterData.cardName == "Dragon") OnWin();
    }

    public void LooseBattleApplyEffect()
    {
        MonsterCardData monsterData = (MonsterCardData)currentCard.GetComponent<Card>().data.dataInstance;
        monsterData.OnWinBattle();
    }
    public void ShuffleCard()
    {
        deck.GetComponent<Deck>().ShuffleCardInGroup(currentCard.GetComponent<Card>().data, 0);
        Destroy(currentCard);
        currentGameState = GameState.MAIN;
        canRevealNextCard = true;
    }

    public void DestroyCard()
    {
        var revealCard = FMODUnity.RuntimeManager.CreateInstance("event:/sfx/sfx_card/sfx_card_destroy");
        revealCard.start();
        Destroy(currentCard);
        currentGameState = GameState.MAIN;
    }

}
