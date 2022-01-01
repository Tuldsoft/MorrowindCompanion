using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillObj : MonoBehaviour
{
    [SerializeField]
    TMP_Text nameText = null, valueText = null;

    [SerializeField]
    Image skillImage = null;

    SkillName skillName = SkillName.none;

    int value = 0;


    public void SetSkill(SkillName skill = SkillName.none, int value = 0, bool isBonus = false)
    {
        skillName = skill;
        this.value = value;
        Refresh(isBonus);
    }

    void Refresh(bool isBonus = false)
    {
        nameText.text = Data.Skills[skillName].displayName;
        string modifier = string.Empty;
        if (isBonus && value != 0)
            modifier = value < 0 ? "-" : "+";
        valueText.text = modifier + value.ToString();
        skillImage.sprite = Data.Sprites.SkillSprites[skillName];
    }
}
