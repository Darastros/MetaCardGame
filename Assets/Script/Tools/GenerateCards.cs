#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;


public class GenerateCards : Editor
{
    static readonly string deckPath = "Assets/Deck/";
    static readonly string csvPath = deckPath + "CSV/";
    static readonly string cardPath = deckPath + "Cards/";
    static readonly string effectPath = deckPath + "Effects/";
    
    static readonly string creaturesPath = cardPath + "Creatures/";
    static readonly string objectsPath = cardPath + "Objects/";
    static readonly string eventsPath = cardPath + "Events/";
    
    static readonly string deckDataPath = deckPath + "official.asset";
    static readonly List<string> regionsName = new (){"Plain", "PlainToForest", "Forest", "Mountain", "Volcano"};
    
    [MenuItem("Editor/GenerateCards", false, -1)]
    public static void Generate()
    {
        DeckData deck = (DeckData)AssetDatabase.LoadAssetAtPath(deckDataPath, typeof(DeckData));
        List<CardGroup> regions = deck.groups;
        
        foreach (var regionName in regionsName)
        {
            var region = regions.Find(x => x.name == regionName + "Group");
            foreach (var cardPool in region.cardPools)
            {
                var path = AssetDatabase.GetAssetPath(cardPool);
                AssetDatabase.DeleteAsset(path);
            }

            region.cardPools = new List<CardPool>();
            EditorUtility.SetDirty(region);
        }
        
        var effects = new DirectoryInfo(effectPath);
        var filesEffects = effects.GetFiles();
        foreach (var effect in filesEffects)
        {
            AssetDatabase.DeleteAsset(effectPath + effect.Name);
        }

        var info = new DirectoryInfo(csvPath);
        var filesInfo = info.GetFiles();
        
        int nbPool = 0;
        foreach (var file in filesInfo)
        {
            if (file.Extension != ".tsv") continue;
            //Debug.Log(file.FullName);
            
            string fileData = File.ReadAllText(file.FullName);
            string[] lines = fileData.Split("\n"[0]);
            
            int Y = 0;
            while (Y < lines.Length)
            {
                List<List<string>> cardsData = new List<List<string>>();
                int i = 0;
                for (int y = Y; y < lines.Length; ++y)
                {
                    string[] lineElements = lines[y].Trim().Split("\t");
                    
                    if (lineElements[0] == "IGNORE" || lineElements[0] == "END") break;
                    if (lineElements[0] == "CUT") return;
                    cardsData.Add(new List<string>());
                    
                    for (int x = 0; x < lineElements.Length; ++x)
                    {
                        cardsData[i].Add(lineElements[x]);
                    }
                    ++i;
                }
                if(cardsData.Count == 0)
                {
                    ++Y;
                    continue;
                }

                ++nbPool;

                var pools = new List<WeightedCardPoolWithRules>();
                for(int regionId = 0; regionId < regionsName.Count; regionId++)
                {
                    var pool = CreateInstance<WeightedCardPoolWithRules>();
                    pool.cards = new List<CardData>();
                    pool.weightList = new List<float>();
                    pools.Add(pool);
                }

                CardData card = null;
                int numberOfEffect = 0;
                foreach (List<string> cardData in cardsData.Skip(2))
                {
                    if (cardData[0] == "\\")
                    {
                        ++numberOfEffect;
                        AddEffect(card, cardsData[1], cardData, numberOfEffect);
                        continue;
                    }

                    numberOfEffect = 0;
                    card = CreateCard(cardsData[1], cardData);
                    if (card)
                    {
                        for(int regionId = 0; regionId < regionsName.Count; regionId++)
                        {
                            int regionIndex = cardsData[1].FindIndex( x => x == regionsName[regionId]);
                            if (regionIndex < cardData.Count && regionIndex >= 0)
                            {
                                if (float.TryParse(cardData[regionIndex], out var value) && value > 0.0f)
                                {
                                    pools[regionId].cards.Add(card);
                                    pools[regionId].weightList.Add(value);
                                }
                            }
                        }
                    }
                }

                for(int regionId = 0; regionId < regionsName.Count; regionId++)
                {
                    if (pools[regionId].cards.Count == 0) continue;
                    
                    int dataIndex = cardsData[1].FindIndex( x => x == regionsName[regionId]);
                    
                    if (dataIndex < cardsData[0].Count && dataIndex >= 0)
                    {
                        string poolDataStr = cardsData[0][dataIndex];
                        string[] poolData = poolDataStr.Trim('(').Trim(')').Split(' ');
                        pools[regionId].amountOfCardsfromPool = int.Parse(poolData[0]);
                        pools[regionId].minimumOfEachCard = int.Parse(poolData[1]);
                        pools[regionId].maximumOfEachCard = int.Parse(poolData[2]);
                    }

                    regions[regionId].cardPools.Add(pools[regionId]);
                    AssetDatabase.CreateAsset(pools[regionId], deckPath + regionsName[regionId]  + "/" + regionsName[regionId] + "_CardPool_" + nbPool + ".asset");
                    EditorUtility.SetDirty(regions[regionId]);
                }

                Y += cardsData.Count + 1;
                
            }
        }
    }

    private static CardData CreateCard(List<string> _pattern, List<string> _cardData)
    {
        string mergedData = "";
        
        foreach (string data in _cardData)
        {
            mergedData += data + " ";
        }
        
        Debug.Log(mergedData);
        
        int typeIndex = _pattern.FindIndex( x => x == "Type");
        if (typeIndex >= _cardData.Count || typeIndex < 0) return null;
        switch (_cardData[typeIndex])
        {
            case "Creature" :
                return CreateCreature(_pattern, _cardData);
            case "Object" :
                return CreateObject(_pattern, _cardData);
            case "Event" :
                return CreateEvent(_pattern, _cardData);
        }

        return null;
    }
    
    private static CardData CreateCreature(List<string> _pattern, List<string> _cardData)
    {
        int nameIndex = _pattern.FindIndex( x => x == "Name");
        if (nameIndex < _cardData.Count)
        {
            MonsterCardData monsterCard;
            bool exist = FindOrCreateCard(_pattern, _cardData, out monsterCard);
            monsterCard.effectsOnWinBattle = new List<EffectData>();
            monsterCard.effectsOnDeafeated = new List<EffectData>();
            monsterCard.effectsOnRevealed = new List<EffectData>();
            
            int strengthIndex = _pattern.FindIndex( x => x == "Strength");
            if (strengthIndex < _cardData.Count) monsterCard.strength = int.Parse(_cardData[strengthIndex]);

            var effectOnWin = CreateEffect(monsterCard.cardName, "Defeat Effect", "Defeat Value", _pattern, _cardData, -1, 0);
            if(effectOnWin) monsterCard.effectsOnWinBattle.Add(effectOnWin);

            var effectOnDefeat = CreateEffect(monsterCard.cardName, "Win Effect", "Win Value", _pattern, _cardData, -1, 1);
            if(effectOnDefeat) monsterCard.effectsOnDeafeated.Add(effectOnDefeat);
            
            var effectOnRevealed = CreateEffect(monsterCard.cardName, "Reveal Effect", "Reveal Value",_pattern, _cardData, 1, 2);
            if(effectOnRevealed) monsterCard.effectsOnRevealed.Add(effectOnRevealed);
            
            if(!exist) AssetDatabase.CreateAsset(monsterCard, creaturesPath + monsterCard.cardName.Replace(' ', '_') + ".asset");
            else EditorUtility.SetDirty(monsterCard);

            return monsterCard;
        }

        return null;
    }
    
    private static CardData CreateObject(List<string> _pattern, List<string> _cardData)
    {
        int nameIndex = _pattern.FindIndex( x => x == "Name");
        if (nameIndex < _cardData.Count)
        {
            ObjectCardData objectCard;
            bool exist = FindOrCreateCard(_pattern, _cardData, out objectCard);
            objectCard.objectEffects = new List<EffectData>();
            
            int effectIndex = _pattern.FindIndex( x => x == "Effect");
            if (effectIndex < _cardData.Count)
            {
                var effect = CreateEffect(objectCard.cardName, _pattern, _cardData, 0);
                if(effect) objectCard.objectEffects.Add(effect);
            }

            objectCard.isPotion = objectCard.cardName.ToLower().Contains("potion");

            if(!exist) AssetDatabase.CreateAsset(objectCard, objectsPath + objectCard.cardName.Replace(' ', '_') + ".asset");
            else EditorUtility.SetDirty(objectCard);
            return objectCard;
        }

        return null;
    }
    
    private static CardData CreateEvent(List<string> _pattern, List<string> _cardData)
    {
        int nameIndex = _pattern.FindIndex( x => x == "Name");
        if (nameIndex < _cardData.Count)
        {
            EventCardData eventCard;
            bool exist = FindOrCreateCard(_pattern, _cardData, out eventCard);
            eventCard.eventEffects = new List<EffectData>();
            
            int value = 0;
            int strengthIndex = _pattern.FindIndex( x => x == "Value");
            if (strengthIndex < _cardData.Count) int.TryParse(_cardData[strengthIndex], out value);
            
            int effectIndex = _pattern.FindIndex( x => x == "Effect");
            if (effectIndex < _cardData.Count)
            {
                eventCard.eventEffects.Add(CreateEffect(eventCard.cardName, _pattern, _cardData, 0));
            }

            if(!exist) AssetDatabase.CreateAsset(eventCard, eventsPath + eventCard.cardName.Replace(' ', '_') + ".asset");
            else EditorUtility.SetDirty(eventCard);
            return eventCard;
        }

        return null;
    }

    private static void AddEffect(CardData _card, List<string> _pattern, List<string> _cardData, int _numberOfEfect)
    {
        if (!_card) return;

        int effectIndex = _pattern.FindIndex(x => x == "Effect");

        if (effectIndex < _cardData.Count)
        {
            if (_card is EventCardData)
            {
                ((EventCardData)_card).eventEffects.Add(CreateEffect(_cardData[effectIndex], _pattern, _cardData, _numberOfEfect));
            }

            else if (_card is ObjectCardData)
            {
                ((ObjectCardData)_card).objectEffects.Add(CreateEffect(_cardData[effectIndex], _pattern, _cardData, _numberOfEfect));
            }
        }
    }

    private static void FillGenericCardData(CardData _card, List<string> _pattern, List<string> _itemData)
    {
        int nameIndex = _pattern.FindIndex( x => x == "Name");
        if (nameIndex < _itemData.Count) 
            _card.cardName = _itemData[nameIndex];
        
        int descriptionIndex = _pattern.FindIndex( x => x == "Description");
        if (descriptionIndex < _itemData.Count) 
            _card.cardText = _itemData[descriptionIndex];
    }
    
    
    public static T[] GetAllInstances<T>() where T : ScriptableObject
    {
        string[] guids = AssetDatabase.FindAssets("t:"+ typeof(T).Name);
        T[] a = new T[guids.Length];
        for(int i =0;i<guids.Length;i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            a[i] = AssetDatabase.LoadAssetAtPath<T>(path);
        }
        return a;
    }

    private static bool FindOrCreateCard<T>(List<string> _pattern, List<string> _cardData, out T _card) where T : CardData 
    {
        int nameIndex = _pattern.FindIndex( x => x == "Name");
        string name = _cardData[nameIndex];
        var cards = GetAllInstances<T>().ToList();
        _card = cards.Find(x => x.cardName == name);

        bool exist = _card != null;
        if (!exist)
        {
            _card = CreateInstance<T>();
        }

        FillGenericCardData(_card, _pattern, _cardData);
        return exist;
    }

    private static EffectData CreateEffect(string _cardName, List<string> _pattern, List<string> _cardData, int _numberOfEffect)
    {
        string effectType = "";
        int effectIndex = _pattern.FindIndex( x => x == "Effect");
        if (effectIndex < _cardData.Count) effectType = _cardData[effectIndex];
        
        int value = 0;
        int valueIndex = _pattern.FindIndex( x => x == "Value");
        if (valueIndex < _cardData.Count) int.TryParse(_cardData[valueIndex], out value);

        int duration = -1;
        int durationIndex = _pattern.FindIndex( x => x == "Duration");
        if (durationIndex < _cardData.Count) int.TryParse(_cardData[durationIndex], out duration);
        
        return CreateEffect(_cardName, effectType, value, duration, _numberOfEffect);
    }

    private static EffectData CreateEffect(string _cardName, string _effectName, string _valueName, List<string> _pattern, List<string> _cardData, int _duration, int _numberOfEffect)
    {
        string effectType = "";
        int effectIndex = _pattern.FindIndex( x => x == _effectName);
        if (effectIndex < _cardData.Count) effectType = _cardData[effectIndex];
        int value = 0;
        int valueIndex = _pattern.FindIndex( x => x == _valueName);
        if (valueIndex < _cardData.Count) int.TryParse(_cardData[valueIndex], out value);
        return CreateEffect(_cardName, effectType, value, _duration, _numberOfEffect);
    }
    private static EffectData CreateEffect(string _cardName, string _effectType, int _value, int _duration, int _numberOfEffect)
    {
        EffectData effect = null;
        
        switch (_effectType)
        {
            case "Add strength":
                var effectAddStrength = CreateInstance<AddToStatEffectData>();
                effectAddStrength.affectedStat = GameManager.PlayerStat.STRENGTH;
                effectAddStrength.value = _value;
                effect = effectAddStrength;
                break;
            case "Remove strength":
                var effectRemoveStrength = CreateInstance<AddToStatEffectData>();
                effectRemoveStrength.affectedStat = GameManager.PlayerStat.STRENGTH;
                effectRemoveStrength.value = -_value;
                effect = effectRemoveStrength;
                break;
            case "Heal":
                var effectHeal = CreateInstance<AddToStatEffectData>();
                effectHeal.affectedStat = GameManager.PlayerStat.LIFE;
                effectHeal.value = _value;
                effect = effectHeal;
                break;
            case "Inflict damage":
                var effectDamage = CreateInstance<AddToStatEffectData>();
                effectDamage.affectedStat = GameManager.PlayerStat.LIFE;
                effectDamage.value = -_value;
                effect = effectDamage;
                break;
            case "Give gold":
                var effectGiveGold = CreateInstance<AddToStatEffectData>();
                effectGiveGold.affectedStat = GameManager.PlayerStat.COIN;
                effectGiveGold.value = _value;
                effect = effectGiveGold;
                
                break;
            case "Steal gold":
                var effectStealGold = CreateInstance<AddToStatEffectData>();
                effectStealGold.affectedStat = GameManager.PlayerStat.COIN;
                effectStealGold.value = -_value;
                effect = effectStealGold;
                
                break;
            case "Set strength":
                var effectSetStrength = CreateInstance<SetStatEffectData>();
                effectSetStrength.affectedStat = GameManager.PlayerStat.STRENGTH;
                effectSetStrength.value = _value;
                effectSetStrength.duration = _duration;
                
                effect = effectSetStrength;
                
                break;
            case "Reveal potion effect":
                var effectRevealPotion = CreateInstance<RevealPotionEffectData>();
                effect = effectRevealPotion;
                
                break;
            case "Full mana":
                var effectFullMana = CreateInstance<SetStatEffectData>();
                effectFullMana.affectedStat = GameManager.PlayerStat.MAGIC;
                effectFullMana.value = 10;
                effect = effectFullMana;
                break;
            case "Add mana":
                var effectAddMana = CreateInstance<AddToStatEffectData>();
                effectAddMana.affectedStat = GameManager.PlayerStat.MAGIC;
                effectAddMana.value = _value;
                effect = effectAddMana;
                break;
        }

        if (effect)
        {
            AssetDatabase.CreateAsset(effect, effectPath + _cardName.Replace(' ', '_') + "_Effect" + _numberOfEffect + ".asset");
        }
        return effect;
    }

}
#endif