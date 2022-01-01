using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SpellCrafterMonitor : MonoBehaviour
{

    public Feature Feature { get; private set; } = new Feature();

    [SerializeField]
    EventSystem eventSystem = null; // to simulate clicks

    [SerializeField]
    TMP_InputField featureNameInput = null,
        costOverrideInput = null;
    
    [SerializeField]
    TMP_Dropdown fTypeDropdown = null,
        dataFileDropdown = null,
        addToDropdown = null;

    [SerializeField]
    TMP_Text calcCostText = null;

    // castWhenText currently not used in this class

    [SerializeField]
    GameObject effectsContent = null;
    
    GameObject addToObject,
        costObject;
    GameObject effectTemplate => Data.Prefabs.EffectTemplate;
    GameObject viewFeaturesMenu => Data.Prefabs.ViewFeaturesMenu;

    [SerializeField]
    Button saveFeatureButton = null,
        castWhenButton = null;

    // not used in this class: reserAllButton, addEffectButton, resetEffectsButton

    List<SpellCrafterEffect> crafterEffects = new List<SpellCrafterEffect>();

    bool isRefreshing = false;

    private void Awake()
    {
        Initializer.Initialize();
    }

    private void Start()
    {
        
        addToObject = 
            addToDropdown.gameObject.transform.parent.gameObject;
        costObject 
            = calcCostText.gameObject.transform.parent.gameObject;

        featureNameInput.onEndEdit.AddListener(((string x) 
            => Feature.Name = x));
        featureNameInput.onEndEdit.AddListener((string x) => Refresh());

        fTypeDropdown.onValueChanged.AddListener(
            (int x) => FTypeSelected(x));
        fTypeDropdown.onValueChanged.AddListener((int x) => Refresh());

        dataFileDropdown.onValueChanged.AddListener(
            (int x) => DataFileSelected(x));
        dataFileDropdown.onValueChanged.AddListener((int x) => Refresh());

        addToDropdown.onValueChanged.AddListener(
            (int x) => AddToSelected(x));
        addToDropdown.onValueChanged.AddListener((int x) => Refresh());

        costOverrideInput.onEndEdit.AddListener(
            (string x) => {
                int.TryParse(x, out int cost);
                Feature.Cost = cost;
            });

        ResetAll();
        
        /*foreach (Feature feature in Data.Features)
        {
            Debug.Log(feature.Name + " registered in Data");
            foreach (Effect effect in feature.Effects)
                Debug.Log(effect);
        }*/
    }

    // Originally intended to refresh all UI components, now just handles
    //   whether the save button is activated and UpdateEffects()
    // Called when any Feature UI Element is adjusted.
    public void Refresh()
    {
        if (isRefreshing) return; // guard against accidental recursion
        isRefreshing = true;

        saveFeatureButton.interactable = 
            !string.IsNullOrEmpty(featureNameInput.text)
            && (!addToDropdown.gameObject.activeInHierarchy
                    || (addToDropdown.gameObject.activeInHierarchy 
                          && addToDropdown.value != 0))
            && (Feature.IsValid);

        UpdateEffects();

        isRefreshing = false;
    }

    // Activates and populates addToDropdown
    void UpdateAddTo()
    {
        if (dataFileDropdown.captionText.text == "Racial")
        {
            // only reset if not currently populated with RaceName
            if (addToDropdown.options.Count <= 1 
                || addToDropdown.options[1].text != ((RaceName)1).ToString() )
            {
                addToDropdown.ClearOptions();
                addToDropdown.AddOptions(new List<string> { "none" });
                addToDropdown.AddOptions(
                    Data.Races.Keys.Select(x => x.ToString()).ToList());
                addToObject.gameObject.SetActive(true);
                addToDropdown.value = 0;
            }
        }
        else if (dataFileDropdown.captionText.text == "Signed")
        {
            // only reset if not currently populated with SignName
            if (addToDropdown.options.Count <= 1
                || addToDropdown.options[1].text != ((SignName)1).ToString())
            {
                addToDropdown.ClearOptions();
                addToDropdown.AddOptions(new List<string> { "none" });
                addToDropdown.AddOptions(
                    Data.Signs.Keys.Select(x => x.ToString()).ToList());
                addToObject.gameObject.SetActive(true);
                addToDropdown.value = 0;
            }
        }
        else
        {
            addToDropdown.ClearOptions();
            addToDropdown.AddOptions(new List<string> { "none" });
            addToDropdown.value = 0;
            addToObject.gameObject.SetActive(false);
        }
    }

    // Triggers from fTypeDropdown.OnValueChanged and ResetAll()
    void FTypeSelected(int fTypeInt)
    {
        // Abilities are constant effects for a race or sign
        FType fType = (FType)fTypeInt;

        costObject.SetActive(fType == FType.Spell);
        castWhenButton.gameObject.SetActive(fType != FType.Ability);
        castWhenButton.interactable = fType == FType.Spell;
        this.Feature.FType = fType;
        this.Feature.IsConstant = fType == FType.Ability;
        
        if (fType == FType.Spell)
            CalculateCost();
    }

    // Triggers from dataFileDropdown.onValueChanged, and ResetAll()
    void DataFileSelected(int fFileInt)
    {
        Feature.FFile = (FeatureFile)fFileInt;
        UpdateAddTo();
    }

    // Triggers from addToDropdown.onValueChanged, and ResetAll()
    void AddToSelected(int qualifierInt) 
        => Feature.Qualifier = Feature.IntToQualifier(qualifierInt);

    // Called by Refresh()
    void UpdateEffects()
    {
        foreach (var effect in crafterEffects)
        {
            effect.RefreshEffect();
        }
    }

    // Called by SpellCrafterEffect.RefreshAllValues();
    public void CalculateCost()
    {
        // ( [ Min Magnitude + Max Magnitude ] * [ Duration + 1 ] + Area ) * Base Cost / 40
        // no mag = mags of 1
        // Self = area of 1
        // Lowest possible value on a slider is 1 (despite displaying 0)
        // Target spells multiply by 1.5
        // round down to nearest int
        float totalCost = 0f;

        foreach (Effect effect in Feature.Effects)
        {
            
            // MinMagnitude returns 0 if !HasMagnitude
            int mag = Mathf.Max(effect.MinMagnitude, 1)
                + Mathf.Max(effect.MaxMagnitude, 1);

            // Duration returns 0 if !HasDuration
            int dur = Mathf.Max(effect.Duration, 1) + 1;

            // Area returns 0 if !HasArea or EffectRange.Self
            int area = Mathf.Max(effect.Area, 1);

            float targetMultiplier = effect.EffectRange == EffectRange.Target ? 1.5f: 1f;

            totalCost += ((mag * dur) + area) * effect.BaseCost / 40 * targetMultiplier;
        }

        calcCostText.text = ((int) totalCost).ToString();
    }

    // Clears any effects present, loads a single blank effect
    public void ClearEffects()
    {
        Feature.Effects.Clear();
        crafterEffects.Clear();

        for (int i = 0; i < effectsContent.transform.childCount; i++)
        {
            Destroy(effectsContent.transform.GetChild(i).gameObject);
        }

        AddEffect();
    }

    // Attached to the Add Effect button
    public void AddEffect()
    {
        GameObject newEffectTemplate = Instantiate(
            effectTemplate, effectsContent.transform);

        SpellCrafterEffect crafterEffect =
            newEffectTemplate.GetComponentInChildren<SpellCrafterEffect>();

        crafterEffect.SetEffect(Feature);  // also adds effect to Feature
        crafterEffects.Add(crafterEffect);

        Refresh();
    }

    // full reset, activated by Reset All button or Save Feature button
    public void ResetAll()
    {
        Feature = new Feature();

        featureNameInput.text = string.Empty;
        FTypeSelected(fTypeDropdown.value);         // simulate selection of FType 
        DataFileSelected(dataFileDropdown.value);   // simulate selection of Data File
        AddToSelected(addToDropdown.value);         // simulate selection of Add To

        costOverrideInput.text = string.Empty;

        ClearEffects();
        Refresh();

        eventSystem.SetSelectedGameObject(featureNameInput.gameObject);
    }

    public void SaveFeature()
    {
        // store the feature in Data and write a new json
        Data.AddFeature(Feature);

        // Clear this feature so the next one doesn't write over what we just did
        ResetAll();
    }

    public void ViewFeatures()
    {
        Instantiate(viewFeaturesMenu, gameObject.transform);
    }


}
