using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FeatureBoxObj : MonoBehaviour
{
    [SerializeField]
    TMP_Text nameText = null, valueText = null;

    [SerializeField]
    Image image = null;

    public void SetBoxObj(string display)
    {
        nameText.text = display;
        //valueText.gameObject.SetActive(false);
        //image.gameObject.SetActive(false);
        gameObject.SetActive(true);
    }

    public void SetBoxObj (Effect effect)
    {
        nameText.text = effect.QualifiedName;
        valueText.text = effect.MagToString();
        image.sprite = Data.Sprites.SpellEffectSprites[effect.SpellEffect.Name];
        gameObject.SetActive(true);
    }

    public void SetBoxObj(Feature feature, bool includeType = false, bool isHeader = false)
    {
        if (includeType) // used in SignBox so that a FeatureObj can double as a label.
            nameText.text = feature.FType.ToString() + ": " + feature.Name;
        else              // used in FeatureBox and Race.FeatureBox
            nameText.text = feature.Name;

        if (isHeader)
        {
            //nameText.alignment = TextAlignmentOptions.BottomLeft;
            //nameText.enableAutoSizing = false;
            //nameText.fontSize = valueText.fontSize;
            Image image = gameObject.AddComponent<Image>();
            image.color = new Color ( 1f, 1f, 1f, 0.05f );

            RectTransform boxObjRT = gameObject.GetComponent<RectTransform>();
            boxObjRT.sizeDelta = new Vector2(boxObjRT.sizeDelta.x, boxObjRT.sizeDelta.y + 3);
        }

        if (feature.FType == FType.Spell)
            valueText.text = "Cost: " + feature.Cost.ToString();
        else
            valueText.gameObject.SetActive(false);

        

        image.gameObject.SetActive(false);

        // move text portion over the inactive image
        RectTransform nameRT = nameText.gameObject.GetComponent<RectTransform>();
        nameRT.offsetMin = new Vector2(0, nameRT.offsetMin.y);

        gameObject.SetActive(true);
    }
}
