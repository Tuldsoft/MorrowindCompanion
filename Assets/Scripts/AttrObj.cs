using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AttrObj : MonoBehaviour
{
    [SerializeField]
    TMP_Text nameText = null, valueText = null;

    [SerializeField]
    Image attrImage = null;

    AttrName attrName = AttrName.none;

    int value = 0;


    public void SetAttr(AttrName attr = AttrName.none, int value = 0, bool isBonus = false)
    {
        attrName = attr;
        this.value = value;
        Refresh(isBonus);
    }
    
    void Refresh(bool isBonus = false)
    {
        nameText.text = attrName.ToString();
        string modifier = string.Empty;
        if (isBonus && value != 0)
            modifier = value < 0 ? "-" : "+";
        valueText.text = modifier + value.ToString();
        attrImage.sprite = Data.Sprites.AttrSprites[attrName];
    }

}
