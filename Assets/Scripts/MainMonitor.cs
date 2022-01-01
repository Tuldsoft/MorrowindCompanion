using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MainMonitor : MonoBehaviour
{
    static MainMonitor mainMonitor = null;

    Character activeCharacter = new Character();

    [SerializeField]
    TMP_Text signDetailsText = null,
        outputText = null;

    [SerializeField]
    TMP_Dropdown genderDropdown = null,
        signDropdown = null;

    [SerializeField]
    RaceBox raceBox = null;

    [SerializeField]
    ClassBox classBox = null;

    [SerializeField]
    SignBox signBox = null;

    [SerializeField]
    AttrBox attrBox = null;

    [SerializeField]
    SkillBox skillBox = null;

    [SerializeField]
    FeatureBox featureBox = null;

    [SerializeField]
    AttrSlider healthSlider = null, magickaSlider = null, fatigueSlider = null;

    bool isRefreshing = false;

    private void Awake()
    {
        Initializer.Initialize();

        if (mainMonitor == null)
            mainMonitor = this;
    }

    // Start is called before the first frame update
    // Setup all boxes with objects
    void Start()
    {
        /*foreach (KeyValuePair<RaceName, Race> pair in Data.Races)
            Debug.Log(pair.Key.ToString() + " registered successfully");*/
        /*foreach (KeyValuePair<string, MWClass> pair in Data.Classes)
            Debug.Log(pair.Key + " registered successfully");*/

        PopulateRaceBox();
        PopulateClassBox();
        PopulateSignBox();
        PopulateAttrBox();
        PopulateSkillBox();

        Refresh();

    }

    // Updates all boxes with new selection
    public void Refresh()
    {
        if (isRefreshing) return;
        isRefreshing = true;

        activeCharacter.Gender = (Gender)genderDropdown.value;
        RefreshRaceBox();
        RefreshClassBox();
        RefreshSignBox();
        RefreshAttrBox();
        RefreshSkillBox();
        RefreshDerivedAttrBox(); // TODO: Remove DerivedAttrBox from MainMonitor
        RefreshFeatureBox();

        RefreshOutput(); // for debugging

        isRefreshing = false;
    }

    // RaceBox
    void PopulateRaceBox() => raceBox.PopulateRaceBox(activeCharacter);
    void RefreshRaceBox() => raceBox.RefreshRaceBox();

    // SignBox
    void PopulateSignBox() => signBox.PopulateSignBox(activeCharacter);
    void RefreshSignBox() => signBox.RefreshSignBox();


    // Debugging only
    private void RefreshOutput()
    {
        if (!activeCharacter.IsValid)
        {
            outputText.text = string.Empty;
            return;
        }

        StringBuilder sb = new StringBuilder();

        for (int i = 1; i <= Constants.AttrCount; i++)
            sb.AppendLine(((AttrName)i).ToString() + ": " 
                + activeCharacter.GetAttrValue((AttrName)i));
        sb.AppendLine();

        sb.AppendLine("Major Skills");
        for (int i = 0; i < 5; i++)
        {
            SkillName skill = activeCharacter.MWClass.Skills[i];
            sb.AppendLine(Data.Skills[skill].displayName + ": "
                + activeCharacter.GetSkillValue(skill));
        }
        sb.AppendLine();

        sb.AppendLine("Minor Skills");
        for (int i = 5; i < 10; i++)
        {
            SkillName skill = activeCharacter.MWClass.Skills[i];
            sb.AppendLine(Data.Skills[skill].displayName + ": "
                + activeCharacter.GetSkillValue(skill));
        }
        sb.AppendLine();

        sb.AppendLine("Misc Skills");
        for (int i = 10; i < 27; i++)
        {
            SkillName skill = activeCharacter.MWClass.Skills[i];
            sb.AppendLine(Data.Skills[skill].displayName + ": "
                + activeCharacter.GetSkillValue(skill));
        }
        sb.AppendLine();
        
        sb.AppendLine("Active Effects");
        foreach (Effect effect in activeCharacter.ActiveEffects)
            sb.AppendLine(effect.QualifiedName + " "
                + effect.MagToString());

        outputText.text = sb.ToString();
    }

    // AttrBox
    void PopulateAttrBox() => attrBox.PopulateAttrBox(
        new AttrBox.AttrGetter(activeCharacter.GetAttrValue));
    void RefreshAttrBox() => attrBox.RefreshAttrBox();

    // SkillBox (main page)
    void PopulateSkillBox() => skillBox.PopulateSkillBox(activeCharacter.GetSkills());
    void RefreshSkillBox() => skillBox.RefreshSkillBox(activeCharacter.GetSkills());

    // ClassBox
    void PopulateClassBox() => classBox.PopulateClassBox(activeCharacter);
    void RefreshClassBox() => classBox.RefreshClassBox();

    // DerivedAttrBox (Health, Magicka, etc)
    void RefreshDerivedAttrBox()
    {
        healthSlider.SetValue(activeCharacter.MaxHealth);
        magickaSlider.SetValue(activeCharacter.MaxMagicka);
        fatigueSlider.SetValue(activeCharacter.MaxFatigue);
    }

    // FeatureBox
    void RefreshFeatureBox() => featureBox.RefreshFeatureBox(activeCharacter);
}
