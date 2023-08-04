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
    static readonly List<string> regionsName = new (){"Plain", "Forest", "Mountain", "Volcano"};
    
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
            if (file.Extension != ".csv") continue;
            Debug.Log(file.FullName);
            
            string fileData = File.ReadAllText(file.FullName);
            string[] lines = fileData.Split("\n"[0]);
            
            int Y = 0;
            while (Y < lines.Length)
            {
                List<List<string>> cardsData = new List<List<string>>();
                int i = 0;
                for (int y = Y; y < lines.Length; ++y)
                {
                    string[] lineElements = lines[y].Trim().Split(",");
                    
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
                    pool.amountOfCardsfromPool = 20;
                    pool.minimumOfEachCard = 1;
                    pool.maximumOfEachCard = 3;
                    pools.Add(pool);
                }

                foreach (List<string> cardData in cardsData.Skip(1))
                {
                    var card = CreateCard(cardsData[0], cardData);
                    if (card)
                    {
                        for(int regionId = 0; regionId < regionsName.Count; regionId++)
                        {
                            int regionIndex = cardsData[0].FindIndex( x => x == regionsName[regionId]);
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
            
            int strengthIndex = _pattern.FindIndex( x => x == "Strength");
            if (strengthIndex < _cardData.Count) monsterCard.strength = int.Parse(_cardData[strengthIndex]);
            
            
            var effectOnWinBattle = CreateInstance<AddToStatEffectData>();
            effectOnWinBattle.affectedStat = GameManager.PlayerStat.LIFE;
            int damageIndex = _pattern.FindIndex( x => x == "Damage");
            if (damageIndex < _cardData.Count) effectOnWinBattle.value = -int.Parse(_cardData[damageIndex]);
            AssetDatabase.CreateAsset(effectOnWinBattle, effectPath + monsterCard.cardName.Replace(' ', '_') + "_Win_Effect.asset");
            monsterCard.effectsOnWinBattle.Add(effectOnWinBattle);
            
            var effectOnDefeated = CreateInstance<AddToStatEffectData>();
            effectOnDefeated.affectedStat = GameManager.PlayerStat.MAGIC;
            effectOnDefeated.value = 1;
            AssetDatabase.CreateAsset(effectOnDefeated, effectPath + monsterCard.cardName.Replace(' ', '_') + "_Defeat_Effect.asset");
            monsterCard.effectsOnDeafeated.Add(effectOnDefeated);
            
            
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
                objectCard.objectEffects.Add(CreateEffect(objectCard.cardName,_cardData[effectIndex], _pattern, _cardData));
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
                eventCard.eventEffects.Add(CreateEffect(eventCard.cardName, _cardData[effectIndex], _pattern, _cardData));
            }

            if(!exist) AssetDatabase.CreateAsset(eventCard, eventsPath + eventCard.cardName.Replace(' ', '_') + ".asset");
            else EditorUtility.SetDirty(eventCard);
            return eventCard;
        }

        return null;
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

    private static EffectData CreateEffect(string _cardName, string _effectType, List<string> _pattern, List<string> _cardData)
    {
        int value = 0;
        int valueIndex = _pattern.FindIndex( x => x == "Value");
        if (valueIndex < _cardData.Count) int.TryParse(_cardData[valueIndex], out value);

        EffectData effect = null;
        
        switch (_effectType)
        {
            case "Add strength":
                var effectAddStrength = CreateInstance<AddToStatEffectData>();
                effectAddStrength.affectedStat = GameManager.PlayerStat.STRENGTH;
                effectAddStrength.value = value;
                effect = effectAddStrength;
                break;
            case "Remove strength":
                var effectRemoveStrength = CreateInstance<AddToStatEffectData>();
                effectRemoveStrength.affectedStat = GameManager.PlayerStat.STRENGTH;
                effectRemoveStrength.value = -value;
                effect = effectRemoveStrength;
                break;
            case "Heal":
                var effectHeal = CreateInstance<AddToStatEffectData>();
                effectHeal.affectedStat = GameManager.PlayerStat.LIFE;
                effectHeal.value = value;
                effect = effectHeal;
                break;
            case "Inflict damage":
                var effectDamage = CreateInstance<AddToStatEffectData>();
                effectDamage.affectedStat = GameManager.PlayerStat.LIFE;
                effectDamage.value = -value;
                effect = effectDamage;
                break;
            case "Give gold":
                var effectGiveGold = CreateInstance<AddToStatEffectData>();
                effectGiveGold.affectedStat = GameManager.PlayerStat.COIN;
                effectGiveGold.value = value;
                effect = effectGiveGold;
                
                break;
            case "Steal gold":
                var effectStealGold = CreateInstance<AddToStatEffectData>();
                effectStealGold.affectedStat = GameManager.PlayerStat.COIN;
                effectStealGold.value = -value;
                effect = effectStealGold;
                
                break;
            case "Set strength":
                var effectSetStrength = CreateInstance<SetStatEffectData>();
                effectSetStrength.affectedStat = GameManager.PlayerStat.STRENGTH;
                effectSetStrength.value = value;
                
                int duration = 0;
                int durationIndex = _pattern.FindIndex( x => x == "Duration");
                if (durationIndex < _cardData.Count) int.TryParse(_cardData[durationIndex], out duration);
                effectSetStrength.duration = duration;
                
                effect = effectSetStrength;
                
                break;
            case "Reveal potion effect":
                var effectRevealPotion = CreateInstance<RevealPotionEffectData>();
                effect = effectRevealPotion;
                
                break;
        }

        if (effect)
        {
            AssetDatabase.CreateAsset(effect, effectPath + _cardName.Replace(' ', '_') + "_Effect.asset");
        }
        return effect;
    }

}
#endif