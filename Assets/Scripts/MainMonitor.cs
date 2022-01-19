using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMonitor : MonoBehaviour
{
    static MainMonitor mainMonitor = null;

    Character activeCharacter = new Character();
    Character GetActiveCharacter() => activeCharacter;
    void SetActiveCharacter(Character.CharBasicData charData) =>
        activeCharacter.UnpackData(charData);

    [SerializeField]
    TMP_InputField nameInput = null;

    [SerializeField]
    TMP_Text outputText = null;

    [SerializeField]
    TMP_Dropdown genderDropdown = null;

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

    [SerializeField]
    Button resetButton = null, 
        saveCharButton = null, loadCharButton = null,
        saveClassButton = null, manageClassButton = null,
        levelTrackerButton = null, factionButton = null, spellCraftButton = null;

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

        nameInput.onEndEdit.AddListener(
            delegate (string x) {
                activeCharacter.SetName(x);
                Refresh(); });

        genderDropdown.onValueChanged.AddListener(
            delegate (int value) { 
                activeCharacter.SetGender((Gender)value); 
                Refresh(); });

        resetButton.onClick.AddListener(delegate ()
        {
            Reset_Click();
            Refresh();
        });

        saveCharButton.onClick.AddListener(SaveChar_Click);
        loadCharButton.onClick.AddListener(LoadChar_Click);
        saveClassButton.onClick.AddListener(SaveClass_Click);
        manageClassButton.onClick.AddListener(ManageClasses_Click);
        //level tracker
        //faction viewer
        //spell crafter

        PopulateRaceBox();
        PopulateClassBox();
        PopulateSignBox();
        PopulateAttrBox();
        PopulateSkillBox();

        Refresh();

        //TestArea();

    }

    // Updates all boxes with new selection
    public void Refresh()
    {
        if (isRefreshing) return;
        isRefreshing = true;

        nameInput.SetTextWithoutNotify(activeCharacter.Name);
        genderDropdown.SetValueWithoutNotify((int)activeCharacter.Gender);

        //activeCharacter.Gender = (Gender)genderDropdown.value;
        RefreshRaceBox();
        RefreshClassBox();
        RefreshSignBox();
        RefreshAttrBox();
        RefreshSkillBox();
        RefreshDerivedAttrBox(); // TODO: Remove DerivedAttrBox from MainMonitor
        RefreshFeatureBox();

        ActivateButtons();
        RefreshOutput(); // for debugging

        isRefreshing = false;
    }

    // RaceBox
    void PopulateRaceBox() => raceBox.PopulateRaceBox(activeCharacter, Refresh);
    void RefreshRaceBox() => raceBox.RefreshRaceBox();

    // SignBox
    void PopulateSignBox() => signBox.PopulateSignBox(activeCharacter, Refresh);
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
    void PopulateClassBox() => classBox.PopulateClassBox(activeCharacter, Refresh);
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

    void ActivateButtons()
    {
        resetButton.interactable = activeCharacter.IsResetable;
        saveCharButton.interactable = activeCharacter.IsSaveable;
        loadCharButton.interactable = Data.UserCharacters.Any();

        saveClassButton.interactable = classBox.IsSaveable;
        manageClassButton.interactable = Data.UserClassKeys.Any();

        levelTrackerButton.interactable = false;
        factionButton.interactable = false;
        spellCraftButton.interactable = false;
    }

    void Reset_Click() => activeCharacter.Reset();

    void SaveChar_Click()
    {
        GameObject gObj = Instantiate(Data.Prefabs.SaveLoadCharMenu, gameObject.transform);
        gObj.GetComponent<SaveLoadCharMonitor>()
            .SetSaveMonitor(GetActiveCharacter, SetActiveCharacter, Refresh);
    }

    void LoadChar_Click()
    {
        GameObject gObj = Instantiate(Data.Prefabs.SaveLoadCharMenu, gameObject.transform);
        gObj.GetComponent<SaveLoadCharMonitor>().SetLoadMonitor(SetActiveCharacter, Refresh);
    }


    void SaveClass_Click()
    {
        // should only be interactable if saveable, but double check anyway
        
        MWClass mwClass = activeCharacter.MWClass;
        if (!classBox.IsSaveable) return;

        // Attempt to convert to standard by checking name
        if (!MWClass.ConvertToStandard(ref mwClass)) return;

        /*FileUtil.WriteJson<MWClass.MWClassData>(
            new List<MWClass.MWClassData> { mwClass.PackData() }, mwClass.KeyName + ".json");*/

        Data.Classes.Remove(Constants.Class.CustomKey);
        FileUtil.SaveBSOToFile<MWClass.MWClassData>(mwClass.PackData());
        
        activeCharacter.SetClass(mwClass.KeyName);
        Refresh();
    }

    void ManageClasses_Click()
    {
        GameObject gObj = Instantiate(Data.Prefabs.ManageClassesMenu, gameObject.transform);
        gObj.GetComponent<ManageClassesMonitor>().SetMonitor(Refresh);
        //gObj.GetComponent<ManageClassesMonitor>().mainMonitor = this;
    }

    /*void TestArea()
    {
        var saveObjects = FileUtil.LoadBSOsFromFiles<MWClass.MWClassData>(
            Constants.Class.FileExtension);

        Debug.Log(saveObjects.Count + " class objects loaded successfully");

        foreach (var saveObject in saveObjects)
        {
            MWClass mwClass = new MWClass();
            mwClass.UnpackData(saveObject);
            Debug.Log(mwClass + " loaded.");
        }

    }*/

}
