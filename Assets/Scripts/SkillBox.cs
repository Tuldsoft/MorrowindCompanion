using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;


public class SkillBox : MonoBehaviour
{
    [SerializeField]
    Transform contentTransform = null;

    [SerializeField]
    GameObject skillTemplate = null, labelTemplate = null;

    List<GameObject> templates => new List<GameObject> { skillTemplate, labelTemplate };

    List<SkillObj> skillObjs = new List<SkillObj>();

    Dictionary<SkillName, int> skillsDict;
    

    // run once, thereafter refresh the box
    public void PopulateSkillBox(Dictionary<SkillName, int> dict)
    {

        ClearSkillBox(); // disables templates
        skillsDict = dict;

        // CreateSkillObj only adds non-label objs to skillObjs

        // Major Skills
        CreateSkillObj(labelTemplate, "Major Skills");
        for (int i = 0; i < 5; i++)
            CreateSkillObj(skillTemplate);

        // Minor Skills
        CreateSkillObj(labelTemplate, "Minor Skills");
        for (int i = 5; i < 10; i++)
            CreateSkillObj(skillTemplate);

        // Misc Skills
        CreateSkillObj(labelTemplate, "Misc Skills");
        for (int i = 10; i < Constants.SkillCount; i++)
            CreateSkillObj(skillTemplate);

    }

    // run each time race is chosen
    public void PopulateRaceSkillBox(Dictionary<SkillName,int> dict)
    {
        ClearSkillBox();
        skillsDict = dict;

        foreach (var pair in skillsDict )
            CreateSkillObj(skillTemplate);

    }
    void ClearSkillBox()
    {
        if (contentTransform == null) { Debug.LogError("Content not found"); return; }
        foreach (GameObject template in templates)
            if (template != null)
                template.SetActive(false);
        
        /*templates
            .Where(x => x != null)
            .ToList()
            .ForEach(x => x.SetActive(false));*/

        // delete from last to first, ignoring templates
        for (int i = contentTransform.childCount - 1; i >= 0; i--)
        {
            GameObject child = contentTransform.GetChild(i).gameObject;
            if (!(templates.Contains(child)))
                Destroy(child);
        }
        
        skillObjs.Clear();
    }

    void CreateSkillObj(GameObject template, string label = "")
    {
        GameObject gameObj = Instantiate(template, template.transform.parent);

        if (string.IsNullOrEmpty(label))
            skillObjs.Add(gameObj.GetComponent<SkillObj>());
        else
            gameObj.GetComponent<TMP_Text>().text = label;
        gameObj.SetActive(true);
    }
    

    public void RefreshSkillBox(Dictionary<SkillName,int> dict = null)
    {
        if (dict != null)
            skillsDict = dict;
        
        if (skillsDict.Count != skillObjs.Count)
        {
            Debug.LogError("Skills Dict does not match Skills Objs.");
            return;
        }

        int iter = 0;
        foreach(var pair in skillsDict)
        {
            skillObjs[iter].SetSkill(pair.Key, pair.Value);
            iter++;
        }

    }

    
}
