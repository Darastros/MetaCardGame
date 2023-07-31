using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DeckData/WeightedCardPoolWithRules")]
public class WeightedCardPoolWithRules : CardPoolWithRules
{
    public List<float> weightList;

    public override int SelectCardInList(List<int> amountAlreadySelectedPerCard)
    {
        List<int> indexToSelectFrom = new List<int>();

        for (int i = 0; i < amountAlreadySelectedPerCard.Count; i++)
        {
            if (amountAlreadySelectedPerCard[i] < maximumOfEachCard)
                indexToSelectFrom.Add(i);
        }

        if (indexToSelectFrom.Count <= 0)
            return -1;
        else
        {
            float totalWeight = 0;
            foreach(int index in indexToSelectFrom) { totalWeight += weightList[index]; }

            float randomSelect = Random.Range(0.0f, totalWeight);

            float weightSum = 0;
            foreach(int index in indexToSelectFrom)
            {
                weightSum += weightList[index];
                if (weightSum >= randomSelect)
                    return index;
            }
            return 0;
        }
    }
}
