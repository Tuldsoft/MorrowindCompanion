using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManageClassesPanel : MonoBehaviour
{
    [SerializeField]
    TMP_Text nameText = null, descriptionText = null;

    [SerializeField]
    Button deleteButton = null;

    public void SetPanel(string classKey, Action refresh)
    {
        deleteButton.onClick.AddListener(
            delegate () { DeleteClass(classKey); refresh(); });

        nameText.text = Data.Classes[classKey].DisplayName;
        descriptionText.text = Data.Classes[classKey].GetFiveSkillsAsString(true);

    }


    void DeleteClass(string classKey)
    {
        // delete file
        FileUtil.DeleteFile(classKey + Constants.Class.FileExtension);

        // delete from data
        Data.Classes.Remove(classKey);
        Data.UserClassKeys.Remove(classKey);

        // delete panel
        Destroy(gameObject);

    }

}
