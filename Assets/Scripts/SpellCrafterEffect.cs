using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// SpellCrafterEffect contains and provides a UI for an Effect
public class SpellCrafterEffect : MonoBehaviour
{
    Effect effect;

    SpellCrafterMonitor monitor;

    [SerializeField]
    TMP_InputField effectNameInput = null;

    [SerializeField]
    TMP_Dropdown spellEffectDropdown = null,
        qualifierDropdown = null,
        rangeDropdown = null;

    [SerializeField]
    TMP_InputField minField = null,
        maxField = null,
        durationField = null,
        areaField = null;

    [SerializeField]
    Slider minSlider = null,
        maxSlider = null,
        durationSlider = null,
        areaSlider = null;

    [SerializeField]
    TMP_Text printoutText = null;


    GameObject qualifierObject,
        magObject,
        durationObject,
        rangeObject,
        areaObject;

    
    // no Start() method, to ensure that Monitor.Start() executes first.

    public void SetEffect(Feature feature)
    {
        // get reference to controlling parent
        monitor = GetComponentInParent<SpellCrafterMonitor>();

        // grab references to container UI components for enable/disable
        qualifierObject =
            qualifierDropdown.gameObject.transform.parent.gameObject;
        magObject =
            minSlider.gameObject.transform.parent.transform.parent.gameObject;
        durationObject =
            durationSlider.gameObject.transform.parent.transform.parent.gameObject;
        rangeObject =
            rangeDropdown.gameObject.transform.parent.gameObject;
        areaObject =
            areaSlider.gameObject.transform.parent.gameObject;

        // adjust slider maximums
        foreach (var combo in magObject.GetComponentsInChildren<SliderFieldCombo>())
            combo.SetSliderMax(Constants.Effect.MaxMagnitude);
        durationObject.GetComponentInChildren<SliderFieldCombo>()
            .SetSliderMax(Constants.Effect.MaxDuration);
        areaObject.GetComponentInChildren<SliderFieldCombo>()
            .SetSliderMax(Constants.Effect.MaxArea);

        // load spellEffectsDropdown
        spellEffectDropdown.ClearOptions();
        spellEffectDropdown.AddOptions(new List<string> { "none" });
        spellEffectDropdown.AddOptions(
            Data.SpellEffects.Keys.OrderBy(x => x.ToString()).ToList());

        // connect sliders' listeners to their Effect values
        minSlider.onValueChanged.AddListener((float x) => effect.MinMagnitude = (int)x);
        maxSlider.onValueChanged.AddListener((float x) => effect.MaxMagnitude = (int)x);
        durationSlider.onValueChanged.AddListener((float x) => effect.Duration = (int)x);
        areaSlider.onValueChanged.AddListener((float x) => effect.Area = (int)x);
        qualifierDropdown.onValueChanged.AddListener(
            (int x) => effect.Qualifier = IntToQualifier(x));

        // more listeners so that the sliders respond to one another (Min/Max)
        //   and the printout
        minSlider.onValueChanged.AddListener(delegate { RefreshAllValues(); });
        maxSlider.onValueChanged.AddListener(delegate { RefreshAllValues(); });
        durationSlider.onValueChanged.AddListener(delegate { RefreshAllValues(); });
        areaSlider.onValueChanged.AddListener(delegate { RefreshAllValues(); });
        qualifierDropdown.onValueChanged.AddListener(delegate { RefreshEffect(); });

        // connect effect and qualifier dropdowns to the monitor's Refresh() routine
        spellEffectDropdown.onValueChanged.AddListener(delegate { monitor.Refresh(); });
        qualifierDropdown.onValueChanged.AddListener(delegate { monitor.Refresh(); });

        // listeners for connecting the spelleffect dropdown to the effectname inputfield
        effectNameInput.onSelect.AddListener(delegate { spellEffectDropdown.Show(); });
        effectNameInput.onValueChanged.AddListener(
            (string x) => PopulateSpellEffectsDropdown(x));
        spellEffectDropdown.onValueChanged.AddListener(
            delegate { effectNameInput.text = spellEffectDropdown.captionText.text; });

        effect = new Effect(feature);
        feature.Effects.Add(effect);
        
    }

    // Generally called by SpellCrafterMonitor, and by changing SpellEffect.
    // Adjusts the on/off state of components. The value of components is
    //   set by RefreshAllValues(), which is called at the end.
    public void RefreshEffect()
    {
        string spellEffectStr = spellEffectDropdown.captionText.text;

        if (!Data.SpellEffects.ContainsKey(spellEffectStr) ) // eg. "none"
        {
            qualifierObject.SetActive(false);
            magObject.SetActive(false);
            durationObject.SetActive(false);
            rangeObject.SetActive(false);
            areaObject.SetActive(false);

            effect.SpellEffect = null;
        }
        else
        {
            
            SpellEffect spellEffect = Data.SpellEffects[spellEffectStr];

            bool isNewSpellEffect = effect.SpellEffect == null
                || spellEffectStr != effect.SpellEffect.ToString();

            // only bother turning things on or off if it is a new SpellEffect    
            if (isNewSpellEffect)
            {
                effect.SpellEffect = spellEffect;

                qualifierObject.SetActive(spellEffect.HasQualifier);
                PopulateQualifiers(spellEffect);

                magObject.SetActive(spellEffect.HasMagnitude);
            }

            // based partially on feature properties
            durationObject.SetActive( effect.HasDuration);

            rangeObject.SetActive(effect.HasRange);
            if (isNewSpellEffect)
                PopulateRanges(spellEffect);

            // display area based on current EffectRange setting
            Enum.TryParse(rangeDropdown.captionText.text, out EffectRange effectRange);
            effect.EffectRange = effectRange;
            areaObject.SetActive(effect.HasRange
                && effectRange != EffectRange.Self);
        }

        RefreshAllValues();
    }

    // Populate the dropdown, filtering based on a string
    void PopulateSpellEffectsDropdown(string s)
    {
        var masterList = Data.SpellEffects.Keys.OrderBy(x => x.ToString()).ToList();

        if (masterList.Contains(s) || s == "none") return;

        spellEffectDropdown.ClearOptions();
        spellEffectDropdown.AddOptions(new List<string> { "none" });

        if (string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s))
        {
            spellEffectDropdown.AddOptions(masterList);
        }
        else
        {
            spellEffectDropdown.AddOptions(
            Data.SpellEffects.Keys
            .Where(x => x.ToString().IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0)
            .OrderBy(x => x.ToString())
            .ToList()
            );
        }
        
        spellEffectDropdown.enabled = false;
        spellEffectDropdown.enabled = true;
        spellEffectDropdown.Show();
        effectNameInput.ActivateInputField();

    }

    // Fill the Qualifiers dropdown with either AttrName or SkillName
    void PopulateQualifiers(SpellEffect spellEffect)
    {
        if (!spellEffect.HasQualifier) return;
        qualifierDropdown.ClearOptions();

        // get type of qualifier from SpellEffect.Name
        string name = spellEffect.Name;
        if (name.Substring(name.Length - 9) == "Attribute")
        {
            qualifierDropdown.AddOptions(
                    Enum.GetNames(typeof(AttrName))
                    .Select(x => x.ToString())
                    .ToList());
        }
        else if (name.Substring(name.Length - 5) == "Skill")
        {
            qualifierDropdown.AddOptions(
                    Enum.GetNames(typeof(SkillName))
                    .Select(x => x.ToString())
                    .ToList());
        }
    }

    // Fills the RangeDropdown based on the restrictions of the SpellEffect
    void PopulateRanges(SpellEffect spellEffect)
    {
        if (!effect.HasRange) return;

        rangeDropdown.ClearOptions();
        switch (spellEffect.Restriction)
        {
            case RangeRestriction.none:
                rangeDropdown.AddOptions( 
                    Enum.GetNames(typeof(EffectRange))
                    .Select(x => x.ToString())
                    .ToList());
                break;
            case RangeRestriction.SelfOnly:
                rangeDropdown.AddOptions(
                    new List<string> { EffectRange.Self.ToString() });
                break;
            case RangeRestriction.TouchTargetOnly:
                rangeDropdown.AddOptions(
                    new List<string> {
                        EffectRange.Touch.ToString(),
                        EffectRange.Target.ToString()
                    });
                break;
            default:
                rangeDropdown.AddOptions(
                    Enum.GetNames(typeof(EffectRange))
                    .Select(x => x.ToString())
                    .ToList());
                break;
        }
    }

    // updates UI elements based on current values of effect
    void RefreshAllValues()
    {
        if (effect.HasMagnitude)
        {
            minSlider.value = effect.MinMagnitude;
            minField.text = effect.MinMagnitude.ToString();
            maxSlider.value = effect.MaxMagnitude;
            maxField.text = effect.MaxMagnitude.ToString();
        }
        if (effect.HasDuration)
        {
            durationSlider.value = effect.Duration;
            durationField.text = effect.Duration.ToString();
        }
        if (effect.HasArea)
        {
            areaSlider.value = effect.Area;
            areaField.text = effect.Area.ToString();
        }

        monitor.CalculateCost();
        printoutText.text = effect; // implicit conversion to string
    }
    
    Enum IntToQualifier(int value)
    {
        string seName = effect.SpellEffect.Name;
        if (seName.Substring(seName.Length - 9) == "Attribute")
            return (AttrName)value;
        if (seName.Substring(seName.Length - 5) == "Skill")
            return (SkillName)value;

        return default;
    }

    
}
