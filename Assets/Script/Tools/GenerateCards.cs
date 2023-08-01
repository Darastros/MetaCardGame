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
    
    static readonly string plainPath = deckPath + "Plain/";
    static readonly string forestPath = deckPath + "Forest/";
    static readonly string mountainPath = deckPath + "Mountain/";
    
    static readonly string creaturesPath = cardPath + "Creatures/";
    static readonly string objectsPath = cardPath + "Objects/";
    static readonly string eventsPath = cardPath + "Events/";
    
    [MenuItem("Editor/GenerateCards", false, -1)]
    public static void Generate()
    {
        var info = new DirectoryInfo(csvPath);
        var filesInfo = info.GetFiles();

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
                    
                    if (lineElements[0] == "IGNORE") break;
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
                foreach (List<string> cardData in cardsData.Skip(1))
                {
                    CreateCard(cardsData[0], cardData);
                }
                
                Y += cardsData.Count + 1;
                
            }
        }
    }

    private static void CreateCard(List<string> _pattern, List<string> _cardData)
    {
        string mergedPattern = "";
        string mergedData = "";
        
        foreach (string data in _pattern)
        {
            mergedPattern += data + " ";
        }
        
        foreach (string data in _cardData)
        {
            mergedData += data + " ";
        }
        
        Debug.Log(mergedPattern);
        Debug.Log(mergedData);
        
        int typeIndex = _pattern.FindIndex( x => x == "Type");
        if (typeIndex >= _cardData.Count || typeIndex < 0) return;
        switch (_cardData[typeIndex])
        {
            case "Creature" :
                CreateCreature(_pattern, _cardData);
                break;
            case "Object" :
                CreateObject(_pattern, _cardData);
                break;
            case "Event" :
                CreateEvent(_pattern, _cardData);
                break;
        }
    }
    
    private static void CreateCreature(List<string> _pattern, List<string> _cardData)
    {
        int nameIndex = _pattern.FindIndex( x => x == "Name");
        if (nameIndex < _cardData.Count)
        {
            MonsterCardData monsterCard;
            bool exist = FindOrCreateCard(_pattern, _cardData, out monsterCard);
            
            int strengthIndex = _pattern.FindIndex( x => x == "Strength");
            if (strengthIndex < _cardData.Count) monsterCard.strength = int.Parse(_cardData[strengthIndex]);
            
            int damageIndex = _pattern.FindIndex( x => x == "Damage");
            if (damageIndex < _cardData.Count) monsterCard.damage = int.Parse(_cardData[damageIndex]);
            
            if(!exist) AssetDatabase.CreateAsset(monsterCard, creaturesPath + monsterCard.cardName + ".asset");
            else EditorUtility.SetDirty(monsterCard);
        }
    }
    
    private static void CreateObject(List<string> _pattern, List<string> _cardData)
    {
        int nameIndex = _pattern.FindIndex( x => x == "Name");
        if (nameIndex < _cardData.Count)
        {
            ObjectCardData objectCard;
            bool exist = FindOrCreateCard(_pattern, _cardData, out objectCard);
            
            int value = 0;
            int strengthIndex = _pattern.FindIndex( x => x == "Value");
            if (strengthIndex < _cardData.Count) int.TryParse(_cardData[strengthIndex], out value);
            
            int effectIndex = _pattern.FindIndex( x => x == "Effect");
            if (effectIndex < _cardData.Count)
            {
                switch (_cardData[effectIndex])
                {
                    case "Add strength" :
                        objectCard.strengthAdd = value;
                        break;
                    case "Remove strength" :
                        objectCard.strengthAdd = -value;
                        break;
                    case "Heal" :
                        objectCard.lifeAdd = value;
                        break;
                    case "Inflict damage" :
                        objectCard.lifeAdd = -value;
                        break;
                    case "Give gold" :
                        objectCard.coinAdd = value;
                        break;
                    case "Steal gold" :
                        objectCard.coinAdd = -value;
                        break;
                    case "Set strength" :
                        objectCard.strengthSet = value;
                        break;
                }
            }
            
            int durationIndex = _pattern.FindIndex( x => x == "Duration");
            if (durationIndex < _cardData.Count)
            {
                if (int.TryParse(_cardData[durationIndex], out var duration)) objectCard.duration = duration;
                else objectCard.duration = -1;
            }

            if(!exist) AssetDatabase.CreateAsset(objectCard, objectsPath + objectCard.cardName + ".asset");
            else EditorUtility.SetDirty(objectCard);
        }
    }

    private static void CreateEvent(List<string> _pattern, List<string> _cardData)
    {
        int nameIndex = _pattern.FindIndex( x => x == "Name");
        if (nameIndex < _cardData.Count)
        {
            EventCardData eventCard;
            bool exist = FindOrCreateCard(_pattern, _cardData, out eventCard);
            
            int value = 0;
            int strengthIndex = _pattern.FindIndex( x => x == "Value");
            if (strengthIndex < _cardData.Count) int.TryParse(_cardData[strengthIndex], out value);
            
            int effectIndex = _pattern.FindIndex( x => x == "Effect");
            if (effectIndex < _cardData.Count)
            {
                switch (_cardData[effectIndex])
                {
                    case "Add strength" :
                        eventCard.strengthAdd = value;
                        break;
                    case "Remove strength" :
                        eventCard.strengthAdd = -value;
                        break;
                    case "Heal" :
                        eventCard.lifeAdd = value;
                        break;
                    case "Inflict damage" :
                        eventCard.lifeAdd = -value;
                        break;
                    case "Give gold" :
                        eventCard.coinAdd = value;
                        break;
                    case "Steal gold" :
                        eventCard.coinAdd = -value;
                        break;
                    case "Set strength" :
                        eventCard.strengthSet = value;
                        break;
                }
            }
            
            int durationIndex = _pattern.FindIndex( x => x == "Duration");
            if (durationIndex < _cardData.Count)
            {
                if (int.TryParse(_cardData[durationIndex], out var duration)) eventCard.duration = duration;
                else eventCard.duration = -1;
            }

            if(!exist) AssetDatabase.CreateAsset(eventCard, eventsPath + eventCard.cardName + ".asset");
            else EditorUtility.SetDirty(eventCard);
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
}
#endif