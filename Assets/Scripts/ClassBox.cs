using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ClassBox : MonoBehaviour
{
    //[SerializeField]
    //MainMonitor monitor = null;

    Action monitorRefresh;

    [SerializeField]
    TMP_Text specText = null;

    [SerializeField]
    GameObject majSkillTemplate = null, minSkillTemplate = null, miscSkillTemplate = null;

    [SerializeField]
    TMP_InputField classInput = null;

    [SerializeField]
    TMP_Dropdown classDd = null, specDd = null, keyAttrDd0 = null, keyAttrDd1 = null,
        majSkillDdTemplate = null, minSkillDdTemplate = null, miscSkillDdTemplate = null;

    [SerializeField]
    Button customButton = null, miscSkillButton = null;

    [SerializeField]
    Image customHighlight = null, miscSkillHighlight = null;

    [SerializeField]
    AttrObj[] keyAttrObjs = null;

    Character character = null;

    // for enabling/disabling
    List<GameObject>
        standardObjs = new List<GameObject>(),
        customObjs = new List<GameObject>();
    List<GameObject> allObjs => standardObjs.Concat(customObjs).ToList();
  

    // array of skill text objs, but not dropdowns
    SkillObj[] skillObjs = new SkillObj[Constants.SkillCount];

    // array of skill dropdowns
    TMP_Dropdown[] skillDds = new TMP_Dropdown[Constants.SkillCount];

    bool isStandardMode = true;


    // properties
    string mwClassKey => character.MWClass == null
        ? Constants.Class.NoneKey : character.MWClass.KeyName;
    /*(classDd.value == 1 && Data.Classes.ContainsKey(Constants.Class.CustomKey)) ?
        Constants.Class.CustomKey : classDd.captionText.text;
*/

    string GetKeyFromClassDd() =>
        (classDd.value == 1 && Data.Classes.ContainsKey(Constants.Class.CustomKey)) ?
            Constants.Class.CustomKey : classDd.captionText.text;

    // checks status for updating interactable of SaveClassButton
    public bool IsSaveable
    {
        get
        {
            MWClass mwClass = character.MWClass;
            if (mwClass == null || mwClass.isStandard)
                return false;
            else if (!isStandardMode)
                return false;
            else if (mwClass.KeyName != Constants.Class.CustomKey)
                return false;
            else if (!MWClass.ValidateNewName(mwClass))
                return false;
            else return true;
        }
    }


    // run once by main monitor to create objects in the box
    public void PopulateClassBox(Character character, Action refresh)
    {
        this.character = character;
        monitorRefresh = refresh;
        
        standardObjs.AddRange( new List<GameObject> { classDd.gameObject,
            specText.gameObject, keyAttrObjs[0].gameObject, keyAttrObjs[1].gameObject});

        customObjs.AddRange(new List<GameObject> { classInput.gameObject,
            specDd.gameObject, keyAttrDd0.gameObject, keyAttrDd1.gameObject});

        classInput.onEndEdit.AddListener(delegate (string x) {
            Data.Classes[Constants.Class.CustomKey].SetName(x);
            monitorRefresh(); });

        classDd.onValueChanged.AddListener(delegate {
            character.SetClass(GetKeyFromClassDd());
            monitorRefresh();
        });

        PopulateClassDd();
        // specDd and keyAttrDd are populated at start

        specDd.onValueChanged.AddListener(delegate (int x)
        {
            Data.Classes[Constants.Class.CustomKey].SetSpec((SpecName)x);
            monitorRefresh();
        });

        keyAttrDd0.onValueChanged.AddListener(delegate (int x)
        {
            if (keyAttrDd1.value == x)
                Data.Classes[Constants.Class.CustomKey].KeyAttrSwap();
            else
                Data.Classes[Constants.Class.CustomKey].SetKeyAttr(0, (AttrName)(x + 1));
            monitorRefresh();
        });

        keyAttrDd1.onValueChanged.AddListener(delegate (int x)
        {
            if (keyAttrDd0.value == x)
                Data.Classes[Constants.Class.CustomKey].KeyAttrSwap();
            else
                Data.Classes[Constants.Class.CustomKey].SetKeyAttr(0, (AttrName)(x + 1));
            monitorRefresh();
        });


        var skillNames = Data.Skills.Values.Select(x => x.displayName).ToList();
        majSkillDdTemplate.ClearOptions();
        majSkillDdTemplate.AddOptions(skillNames);
        minSkillDdTemplate.ClearOptions();
        minSkillDdTemplate.AddOptions(skillNames);
        miscSkillDdTemplate.ClearOptions();
        miscSkillDdTemplate.AddOptions(skillNames);

        // Major Skills
        for (int i = 0; i < 5; i++)
        {
            CreateStandardSkillObj(majSkillTemplate, i);
            CreateCustomSkillObj(majSkillDdTemplate.gameObject, i);
        }

        // Minor Skills
        for (int i = 5; i < 10; i++)
        {
            CreateStandardSkillObj(minSkillTemplate, i);
            CreateCustomSkillObj(minSkillDdTemplate.gameObject, i);
        }

        // Misc Skills
        for (int i = 10; i < Constants.SkillCount; i++)
        {
            CreateStandardSkillObj(miscSkillTemplate, i, false);
            CreateCustomSkillObj(miscSkillDdTemplate.gameObject, i,false);
        }

        Destroy(majSkillTemplate);
        Destroy(minSkillTemplate);
        Destroy(miscSkillTemplate);
        Destroy(majSkillDdTemplate.gameObject);
        Destroy(minSkillDdTemplate.gameObject);
        Destroy(miscSkillDdTemplate.gameObject);

    }

    // populated at start, repopulated when "Customize" is clicked
    void PopulateClassDd()
    {
        classDd.ClearOptions();
        classDd.AddOptions(new List<string> { "none" });               // 0 none
        if (Data.Classes.ContainsKey(Constants.Class.CustomKey))
            classDd.AddOptions(new List<string> { 
                Data.Classes[Constants.Class.CustomKey].DisplayName }); // 1 custom

        /*Debug.Log("Adding user classes: \n" + String.Join(",\n",
            Data.Classes.Values
            .Where(x => x.isStandard
                && Data.UserClassKeys.Contains(x.KeyName))
            .Select(x => x.DisplayName)
            .ToList()));*/

        // user-defined "standard" classes from files
        classDd.AddOptions(Data.Classes.Values
            .Where(x => x.isStandard
                && Data.UserClassKeys.Contains(x.KeyName))
            .Select(x => x.DisplayName)
            .OrderBy(x => x)
            .ToList());

        // in-game standard classes
        classDd.AddOptions(Data.Classes.Values
            .Where(x => x.isStandard
                && !Data.UserClassKeys.Contains(x.KeyName))
            .Select(x => x.DisplayName)
            .OrderBy(x => x)
            .ToList());

        // reselect
        if (mwClassKey == Constants.Class.NoneKey)
            classDd.SetValueWithoutNotify(0);
        else if (mwClassKey == Constants.Class.CustomKey)
            classDd.SetValueWithoutNotify(1);
            //classDd.value = 1;
        else
        {
            int index = classDd.options
                .Select(x => x.text)
                .ToList()
                .IndexOf(mwClassKey);
            if (index < 1)
                classDd.SetValueWithoutNotify(0);
                //classDd.value = 0;
            else
                classDd.SetValueWithoutNotify(index);
                //classDd.value = index;
        }
    }

    void CreateStandardSkillObj(GameObject template, int index, bool isMajMin = true)
    {
        GameObject gameObj = Instantiate(template, template.transform.parent);
        skillObjs[index] = gameObj.GetComponent<SkillObj>();

        PositionSkillObj(gameObj, index, isMajMin);
        standardObjs.Add(gameObj);
    }

    void CreateCustomSkillObj(GameObject template, int index, bool isMajMin = true)
    {
        GameObject gameObj = Instantiate(template, template.transform.parent);
        skillDds[index] = gameObj.GetComponent<TMP_Dropdown>();
        skillDds[index].onValueChanged.AddListener(
            (int x) => SkillDd_Click(index, x));
        
        PositionSkillObj(gameObj, index, isMajMin);
        customObjs.Add(gameObj);
    }

    void PositionSkillObj(GameObject gameObj, int index, bool isMajMin = true)
    {
        Vector2 position = gameObj.transform.localPosition;
        if (isMajMin)
            position.y -= 15 * (index % 5);
        else
            position.y -= 15 * (index - 10);

        gameObj.transform.localPosition = position;
    }

    void HideAll()
    {
        foreach (var gameObj in allObjs)
            gameObj.SetActive(false);

        classDd.gameObject.SetActive(true);
        customButton.interactable = false;
        customHighlight.gameObject.SetActive(false);
    }

    void Show(bool standard = true)
    {
        foreach (var gameObj in standardObjs)
            gameObj.SetActive(standard);

        foreach (var gameObj in customObjs)
            gameObj.SetActive(!standard);

        customButton.interactable = true;
        customHighlight.gameObject.SetActive(!isStandardMode);
        miscSkillButton.interactable = !isStandardMode;

        if (isStandardMode && miscSkillHighlight.gameObject.activeInHierarchy)
            MiscButton_Click();
    }

    public void RefreshClassBox()
    {
        MWClass mwClass = character.MWClass;

        PopulateClassDd();

        // if "none"
        if (mwClass == null)
            HideAll(); // everything off
        else
        {
            Show(isStandardMode);

            classInput.text = mwClass.DisplayName;

            // set spec and key attributes
            specText.text = mwClass.Specialization.ToString();
            keyAttrObjs[0].SetAttr(mwClass.KeyAttributes[0], 10, true);
            keyAttrObjs[1].SetAttr(mwClass.KeyAttributes[1], 10, true);

            specDd.SetValueWithoutNotify((int)mwClass.Specialization);
            keyAttrDd0.SetValueWithoutNotify((int)mwClass.KeyAttributes[0] - 1);
            keyAttrDd1.SetValueWithoutNotify((int)mwClass.KeyAttributes[1] - 1);

            
            // set Maj/Min/Misc skills
            for (int i = 0; i < Constants.SkillCount; i++)
            {
                SkillName skill = mwClass.Skills[i];
                skillObjs[i].SetSkill(skill, mwClass.GetSkillValue(skill), true);
                skillDds[i].SetValueWithoutNotify((int)character.MWClass.Skills[i] - 1);
            }
        }
    }

    public void Customize_Click()
    {
        //string originalKey = mwClassKey; // changes to "custom" if !standard
        //Debug.Log("switching from " + originalKey);
        isStandardMode = !isStandardMode;

        if (!isStandardMode)
        {
            // overwrite or add custom key
            Data.Classes[Constants.Class.CustomKey]
                = MWClass.CopyToCustom(character.MWClass);
            
            character.SetClass(Constants.Class.CustomKey);
            //PopulateClassDd(Constants.Class.CustomKey); // done in Refresh()
        }
        else
        {
            //PopulateClassDd(mwClassKey);  // done in Refresh()
        }

        // enable and disable things
        //Show(isStandardMode); //now handled in refresh()

        monitorRefresh();
    }

    // show or hide the Misc skills panel, only available for custom classes
    public void MiscButton_Click()
    {
        GameObject miscWindow = skillDds[10].gameObject.transform.parent.gameObject;
        miscWindow.SetActive(!miscWindow.activeInHierarchy);
        miscSkillHighlight.gameObject.SetActive(!miscSkillHighlight.gameObject.activeInHierarchy);
    }

    // listener assigned in PopulateClassBox(), provides new slot and new value
    void SkillDd_Click(int skillSlot, int ddValue)
    {
        if (isStandardMode) return;
        
        MWClass customClass = Data.Classes[Constants.Class.CustomKey];
        var oldSkills = customClass.Skills.ToList();
        int oldSkillSlot = oldSkills.IndexOf((SkillName)(ddValue + 1));

        //Debug.Log($"{(SkillName)(ddValue + 1)} found in slot {oldSkillSlot}");

        // only swap if the slots are different
        if (oldSkillSlot != skillSlot)
        {
            customClass.SkillSwap(oldSkillSlot, skillSlot);
            monitorRefresh();
        }
    }

    

}
