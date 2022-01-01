using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewFeaturesListMonitor : MonoBehaviour
{
    [SerializeField]
    GameObject contentObj = null;

    GameObject prefabPanelTemplate => Data.Prefabs.ViewFeaturesPanel;

    public void PopulateList(IEnumerable<Feature> features)
    {
        EmptyList();

        foreach(var feature in features)
        {
            AddToList(feature);
        }
    }

    void AddToList(Feature feature)
    {
        if (prefabPanelTemplate is null)
        {
            Debug.LogError("Attempting to create a panel from null");
            return;
        }

        ViewFeaturesPanel newPanel = Instantiate(prefabPanelTemplate, contentObj.transform)
            .GetComponent<ViewFeaturesPanel>();

        newPanel.SetPanel(feature);
    }

    // Empty the ScrollList
    void EmptyList()
    {
        for (int i = 0; i < contentObj.transform.childCount; i++)
        {
            Destroy(contentObj.transform.GetChild(i).gameObject);
        }
    }



}
