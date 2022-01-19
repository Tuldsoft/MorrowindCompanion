using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Attached to manage classes menu prefab
public class ManageClassesMonitor : MonoBehaviour
{
    [SerializeField]
    GameObject contentObj = null, panelTemplate = null;

    public void SetMonitor(Action refresh) => PopulateListView(refresh);


    void PopulateListView(Action refresh)
    {
        foreach (var classKey in Data.UserClassKeys)
        {
            GameObject newPanel = Instantiate(panelTemplate, contentObj.transform);
            newPanel.GetComponent<ManageClassesPanel>().SetPanel(classKey, refresh);
        }
        panelTemplate.SetActive(false);
    }


    // attached via inspector to backing layer
    public void CloseWindow()
    {
        Destroy(this.gameObject); 
    }

}
