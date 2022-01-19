using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveLoadCharMonitor : MonoBehaviour
{
    public enum SaveLoadMenuMode { Save, Load };

    [SerializeField]
    GameObject contentObj = null, newSavePanel = null, panelTemplate = null;

    List<GameObject> templates => new List<GameObject> { newSavePanel, panelTemplate };

    SaveLoadMenuMode mode;                     // menu mode
    Action refresh;                            // either local or monitor refresh
    Action<Character.CharBasicData> setChar;   // for Save and Load
    Func<Character> getChar;                   // only needed for Save

    /*int GetNextSlot =>
        Data.UserCharacters.Select(x => x.SaveSlot).Max() + 1;*/

    public void SetSaveMonitor(Func<Character> getChar, 
        Action<Character.CharBasicData> setChar, Action refresh)
    {
        this.mode = SaveLoadMenuMode.Save;
        this.refresh = refresh;
        this.getChar = getChar;
        this.setChar = setChar;

        PopulateListView();
    }

    public void SetLoadMonitor(Action<Character.CharBasicData> setChar, Action refresh)
    {
        this.mode = SaveLoadMenuMode.Load;
        this.refresh = refresh;
        this.setChar = setChar;

        PopulateListView();
    }

    void PopulateListView()
    {
        ClearListView();
        templates.ForEach(x => x.gameObject.SetActive(true));

        if (Data.UserCharacters.Any())
        {
            var characters = Data.UserCharacters
                .OrderByDescending(character => character.saveTime);

            foreach (var character in characters)
            {
                SaveLoadCharPanel newPanel = Instantiate(panelTemplate, contentObj.transform)
                    .GetComponent<SaveLoadCharPanel>();

                if (mode == SaveLoadMenuMode.Save)
                    newPanel.SetSavePanel(character, getChar, setChar, PopulateListView);
                else
                    newPanel.SetLoadPanel(character, setChar, CloseWindow);
            }

        }

        if (mode == SaveLoadMenuMode.Save)
            newSavePanel.GetComponent<SaveLoadCharPanel>()
                .SetNewSlotPanel(getChar, setChar, PopulateListView);
        else
            newSavePanel.SetActive(false);

        panelTemplate.SetActive(false);
        //templates.ForEach(x => x.gameObject.SetActive(false));
    }


    void ClearListView() =>
        contentObj.transform
            .GetComponentsInChildren<SaveLoadCharPanel>()
            .Select(x => x.gameObject)
            .Where(x => !templates.Contains(x))
            .ToList()
            .ForEach( x => Destroy(x));
        

    // attached via inspector to backing layer
    public void CloseWindow()
    {
        refresh();
        Destroy(this.gameObject);
    }

}

