using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeatureBox : MonoBehaviour
{
    [SerializeField]
    GameObject labelTemplate = null, featureTemplate = null;

    [SerializeField]
    Transform contentTransform = null; 

    List<GameObject> templates => new List<GameObject> { 
        labelTemplate, featureTemplate};

    private void Start()
    {
        templates.ForEach(x => x.SetActive(false));
    }

    void ClearFeatureBox()
    {
        for(int i = contentTransform.childCount - 1; i >= 0; i--)
        {
            GameObject gObj = contentTransform.GetChild(i).gameObject;
            if (!templates.Contains(gObj))
                Destroy(gObj);
        }
    }

    public void RefreshFeatureBox(Character character)
    {
        //contentTransform = gameObject.transform.Find("Contents").transform;
        
        ClearFeatureBox();
        PopulateFeatureBox(character);
    }

    // Instantiation and looping

    void CreateLabel(string label) =>
        Instantiate(labelTemplate, contentTransform)
                .GetComponent<FeatureBoxObj>()
                .SetBoxObj(label);

    void CreateEffect(Effect effect) =>
        Instantiate(featureTemplate, contentTransform)
                    .GetComponent<FeatureBoxObj>()
                    .SetBoxObj(effect);

    void CreateEffects(List<Effect> effects, string label = "")
    {
        if (!(string.IsNullOrEmpty(label)))
            CreateLabel(label);
        effects.ForEach(x => CreateEffect(x));
    }

    void CreateFeatureWithEffects(Feature feature, bool includeType = false) 
    {
        Instantiate(featureTemplate, contentTransform)
                .GetComponent<FeatureBoxObj>()
                .SetBoxObj(feature, includeType, true);

        foreach (var effect in feature.Effects)
            Instantiate(featureTemplate, contentTransform)
                .GetComponent<FeatureBoxObj>()
                .SetBoxObj(effect);
    }

    void CreateFeaturesWithEffects(List<Feature> features, bool includeType = false, string label = "")
    {
        if (!(string.IsNullOrEmpty(label)))
            CreateLabel(label);
        features.ForEach(x => CreateFeatureWithEffects(x, includeType));
    }

    // Clears, populates, and refreshes the FeatureBox in RaceBox
    public void RefreshRaceFeatureBox(Character character)
    {
        ClearFeatureBox();
        
        Race race = character.Race;
        Gender gender = character.Gender;
        List<Feature> features = new List<Feature>(character.Race.Features);
        
        if (!features.Any()) return;
        features.Sort();

        // Height & Weight
        CreateLabel($"Height: {race.Height[gender]}, Weight: {race.Weight[gender]}");

        // Active Effects
        List<Feature> subList = new List<Feature>(
            features.Where(x => x.FType == FType.Ability));
        if (subList.Any())
        {
            CreateLabel("Active Effects");
            CreateEffects(Feature.ExtractEffects(subList));
        }

        // Powers
        subList = new List<Feature>(
            features.Where(x => x.FType == FType.Power));

        if (subList.Any())
            CreateFeaturesWithEffects(subList, false, "Powers");

        // Spells
        subList = new List<Feature>(
            features.Where(x => x.FType == FType.Spell));
        if (subList.Any())
            CreateFeaturesWithEffects(subList, false, "Spells");
    }

    // Clears then populates and refreshes the FeatureBox in SignBox. Label each with name.
    public void RefreshSignFeatureBox(Character character)
    {
        ClearFeatureBox();

        if (character.Sign == null) return;

        List<Feature> features = character.Sign.Features;

        if (features.Any())
            CreateFeaturesWithEffects(features, true);
            
    }


    // Main FeatureBox, composite of Race, Class, Sign, Skillspells
    void PopulateFeatureBox(Character character)
    {
        // Active Effects
        var activeEffects = character.ActiveEffects;
        if (activeEffects.Any())
            CreateEffects(activeEffects, "Active Effects");
        
        // Features
        List<Feature> features = new List<Feature>();
        if (character.Race != null && character.Gender != Gender.none)
            features.AddRange(character.Race.Features);
        if (character.Sign != null)
            features.AddRange(character.Sign.Features);
        features.Sort();

        // Powers (Race & Sign)
        List<Feature> subList = new List<Feature>( 
            features.Where(x => x.FType == FType.Power) );

        if (subList.Any())
            CreateFeaturesWithEffects(subList, false, "Powers");

        // Spells (Race & Sign)
        subList = new List<Feature>(
            features.Where(x => x.FType == FType.Spell) );

        // Spells (Skill-based)
        TryAddSpellToList("Water Walking", character, 
            SkillName.Alteration, 25, ref subList);
        TryAddSpellToList("Shield", character,
            SkillName.Alteration, 30, ref subList);
        
        TryAddSpellToList("Bound Dagger", character,
            SkillName.Conjuration, 25, ref subList);
        TryAddSpellToList("Summon Ancestral Ghost", character,
            SkillName.Conjuration, 30, ref subList);
        
        TryAddSpellToList("Fire Bite", character,
            SkillName.Destruction, 25, ref subList);
        
        TryAddSpellToList("Chameleon", character,
            SkillName.Illusion, 30, ref subList);
        TryAddSpellToList("Sanctuary", character,
            SkillName.Illusion, 30, ref subList);

        TryAddSpellToList("Detect Creature", character,
            SkillName.Mysticism, 30, ref subList);

        TryAddSpellToList("Hearth Heal", character,
            SkillName.Restoration, 25, ref subList);

        if (subList.Any())
        {
            subList.Sort();
            CreateFeaturesWithEffects(subList, false, "Spells");
        }

    }

    void TryAddSpellToList(string spell, Character character, SkillName skill,
        int minValue, ref List<Feature> subList)
    {
        if (character.GetSkillValue(skill) >= minValue)
        {
            Feature feature = Data.GetSpell(spell);
            if (feature == null)
                Debug.Log(spell + " spell not found.");
            else
                subList.Add(feature);
        }
    }

}
