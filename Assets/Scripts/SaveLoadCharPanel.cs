using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class SaveLoadCharPanel : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    TMP_Text slotText = null, nameText = null, dateText = null, descriptionText = null;

    [SerializeField]
    Button panelButton = null, deleteButton = null;

    int GetNextSlot => Data.UserCharacters.Any() ? 
        Data.UserCharacters.Select(x => x.saveSlot).Max() + 1
        : 1;


    void SetPanel(Character.CharBasicData charData)
    {
        Character character = new Character();
        character.UnpackData(charData);

        deleteButton.onClick.AddListener(
            delegate () { DeleteCharacter(character); });

        slotText.text = "Slot " + character.SaveSlot.ToString();
        nameText.text = character.Name;
        dateText.text = character.SaveTime.ToString();
        MWClass mwClass = character.MWClass;
        descriptionText.text = mwClass == null ? string.Empty : mwClass.DisplayName;

    }

    public void SetNewSlotPanel(Func<Character> getActiveChar,
        Action<Character.CharBasicData> setActiveChar, Action listRefresh)
    {
        panelButton.onClick.RemoveAllListeners();
        panelButton.onClick.AddListener( delegate () 
        { 
            SaveCharacter(getActiveChar(), GetNextSlot, setActiveChar);
            listRefresh();
        });
        // text fields preset in template
    }

    /*if (mode == SaveLoadMenuMode.Save)
                newPanel.SetSavePanel(character, getChar, PopulateListView);
            else
                newPanel.SetLoadPanel(character, setChar, refresh, CloseWindow);
        }

        if (mode == SaveLoadMenuMode.Save)
            newSavePanel.GetComponent<SaveLoadCharPanel>()
                .SetNewSlotPanel(getChar, PopulateListView);*/


    public void SetSavePanel(Character.CharBasicData charData, Func<Character> getActiveChar,
        Action<Character.CharBasicData> setActiveChar, Action localRefresh)
    {
        panelButton.onClick.AddListener(
            delegate() 
            { 
                SaveCharacter(getActiveChar(), charData.saveSlot, setActiveChar);
                localRefresh();
            });
        SetPanel(charData);
    }

    public void SetLoadPanel(Character.CharBasicData charData, 
        Action<Character.CharBasicData> setChar, 
        Action closeWindow)
    {
        panelButton.onClick.AddListener(delegate()
        {
            setChar(charData);
            closeWindow();
        });

        SetPanel(charData);
    }

    void SaveCharacter(Character saveChar, int saveSlot, 
        Action<Character.CharBasicData> setChar)
    {
        Character.CharBasicData charData = saveChar.PackData(saveSlot);
        FileUtil.SaveBSOToFile<Character.CharBasicData>(charData);
        
        Data.UserCharacters.Remove(charData);
        Data.UserCharacters.Add(charData);

        setChar(charData);
    }

    void DeleteCharacter(Character character)
    {
        IBinarySaveObject so = character.PackData(character.SaveSlot);
        string fileName = so.GetFileName();

        // delete file
        FileUtil.DeleteFile(fileName);

        // delete from data
        Data.UserCharacters.Remove((Character.CharBasicData)so);

        // delete panel
        Destroy(gameObject);
    }
}
