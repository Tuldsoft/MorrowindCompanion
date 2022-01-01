using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ViewFeaturesPanel : MonoBehaviour
{
    //[SerializeField]
    //Button deleteButton = null;

    [SerializeField]
    TMP_Text qualifierText = null,
        nameText = null,
        effectsText = null;

    Feature feature;



    public void SetPanel(Feature f)
    {
        this.feature = f;

        qualifierText.text = feature.Qualifier == default ? string.Empty
            : feature.Qualifier.ToString() + "\r\n";
        qualifierText.text += feature.FType.ToString();

        nameText.text = feature.Name;
        effectsText.text = feature.PrintEffects();
    }

    public void DeleteClick()
    {
        // Remove from race or sign
        if (feature.FFile == FeatureFile.Racial)
            Data.Races[(RaceName)feature.Qualifier].Features.Remove(feature);
        if (feature.FFile == FeatureFile.Signed)
            Data.Signs[(SignName)feature.Qualifier].Features.Remove(feature);

        FeatureData fData = (FeatureData)feature.GetJsonable();

        // Remove from Data.Features and Data.FeatureDatas
        Data.FeatureDatas.Remove(fData);
        Data.Features.Remove(feature);

        // Rewrite the json
        FileUtil.WriteJson(Data.FeatureDatas, fData.FileName);

        Destroy(gameObject);
    }


}