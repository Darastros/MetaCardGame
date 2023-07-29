using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class MonsterCard : Card
{
    public GameObject strengthAttributeText;

    public override void ApplyCardData(CardInstance data)
    {
        base.ApplyCardData(data);
        MonsterCardData cardData = (MonsterCardData)data.dataInstance;
        strengthAttributeText.GetComponent<TMP_Text>().text = cardData.strength.ToString();
    }

    public override void Resolve()
    {
        GameManager.Instance.StartBattleAgainstMonster();
    }
}
