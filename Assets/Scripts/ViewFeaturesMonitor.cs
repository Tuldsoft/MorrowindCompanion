using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Attach to menu prefab. Contains button operation.
//   ScrollView management handled by ViewFeaturesListMonitor
public class ViewFeaturesMonitor : MonoBehaviour
{
    
    [SerializeField]
    Button spellsButton = null, racialButton = null, signedButton = null, miscButton = null;

    [SerializeField]
    ViewFeaturesListMonitor listMonitor = null;


    FeatureFile fFile = FeatureFile.Spells;

    IEnumerable<Feature> Features { get => GetFeatures(fFile); }


    // Start is called before the first frame update
    void Start()
    {
        spellsButton.onClick.AddListener(
            delegate { FFileButtonClick(FeatureFile.Spells); });
        racialButton.onClick.AddListener(
            delegate { FFileButtonClick(FeatureFile.Racial); });
        signedButton.onClick.AddListener(
            delegate { FFileButtonClick(FeatureFile.Signed); });
        miscButton.onClick.AddListener(
            delegate { FFileButtonClick(FeatureFile.Misc); });

        FFileButtonClick(FeatureFile.Spells);
    }


    void FFileButtonClick(FeatureFile fFile)
    {
        this.fFile = fFile;
        listMonitor.PopulateList(Features);
    }

    static IEnumerable<Feature> GetFeatures (FeatureFile file) 
        => Data.Features
        .Where(
            (feature) => ((FeatureData)feature.GetJsonable()).fFile == file)
        .OrderBy(
            (feature) => ((FeatureData)feature.GetJsonable()).qualifier)
        .ThenBy(
            (feature) => feature.Name);

    // attached via inspector to backing layer
    public void CloseWindow() => Destroy(this.gameObject);

}
