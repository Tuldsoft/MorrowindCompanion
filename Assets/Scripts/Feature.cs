using System;                        // for Enum
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public enum FType { Ability, Power, Spell }
public enum FeatureFile { Spells, Racial, Signed, Misc }

[System.Serializable]
public class FeatureData : IJsonable
{
    // IJsonable implementation. Signals that only jsonable fields are included.
    public string FileName => "Features.json";

    // Serializable backing fields
    // Used by Data.AddFeature()
    
    // Metadata on Feature for adding to Races or Signs
    public FeatureFile fFile = FeatureFile.Spells;
    public int qualifier = 0; // either RaceName or SignName. Enum is not jsonable.
        
    public string name = string.Empty;
    public FType fType = FType.Spell;
    public int cost = 0;
    public bool isConstant = false;

    // Serializable list for json, defined by Feature.PackEffectsData()
    public List<EffectData> effDataList = null;

    // A list of Effects is maintained by Feature, and is not backed here.
}

public class Feature : IComparable<Feature>, IEquatable<Feature>
{
    // backing data, info for storage
    FeatureData fData = new FeatureData();
    
    // Non-serialized field for effects.
    // Converted and stored as fData.effData when GetJsonable() is called
    List<Effect> effects = new List<Effect>();

    // Public Properties
    public FeatureFile FFile { get => fData.fFile; set => fData.fFile = value; }
    public Enum Qualifier {
        get => IntToQualifier(fData.qualifier);
        set => fData.qualifier = QualifierToInt(value);
    }

    public string Name { get => fData.name; set => fData.name = value; }
    public string SystemName
    {
        get
        {
            StringBuilder output = new StringBuilder(Name);
            output.Append("_" + fData.fFile.ToString());
            if (Qualifier != default)
                output.Append("_" + Qualifier.ToString());
            return output.ToString();
        }
    }

    public FType FType { get => fData.fType; set => fData.fType = value; }
    
    // Cost in magicka, or null if power or ability
    //   Formula is not yet working correctly.
    public int Cost { 
        get => FType == FType.Spell ? fData.cost : 0; 
        set => fData.cost = value; 
    } 

    public bool IsConstant { 
        get => fData.isConstant || FType == FType.Ability; 
        set => fData.isConstant = value; 
    }
    
    // Whether or not all necessary components have been specified (Name, Effects, etc.)
    public bool IsValid => Validate();

    public List<Effect> Effects { get => effects; set => effects = value; }

    // Read Effects to determine magickaBonus, resistances, attrBonus
    public float MagickaMultiplier => GetMagickaMultiplier();
    public Dictionary<ResistType, int> Resistances => GetResistances();
    public Dictionary<AttrName, int> AttrBonus => GetAttrBonus();


    // Constructors
    public Feature() { }
    public Feature(FeatureData data)
    {
        fData = data;
        UnpackEffectsData();
    }

    // methods
    // searches Effects for MagickaMultiplier, returns their sum
    float GetMagickaMultiplier()
    {
        /*float output = 0;
        if (FType == FType.Ability)
            foreach (Effect effect in Effects)
                if (effect.SpellEffect.Name == "Fortify Maximum Magicka")
                    output += ((float)effect.MinMagnitude) / 10;
        return output;*/

        if (FType == FType.Ability)
            return Effects
                .Where(x => x.SpellEffect.Name == "Fortify Maximum Magicka")
                .Sum(x => ((float)x.MinMagnitude) / 10);
        else
            return 0;
    }

    // Combines MagickaMultiplier in a List<Features>, used by Race and Sign
    public static float GetMagickaMultiplier(List<Feature> features)
        => features.Sum(x => x.MagickaMultiplier);


    // builds a Dict of resistances, based on Effects
    Dictionary<ResistType, int> GetResistances()
    {
        Dictionary<ResistType, int> output = new Dictionary<ResistType, int>();
        if (FType == FType.Ability)
        {
            int sign;
            foreach (var effect in Effects)
            {
                string sEffect = effect.SpellEffect.Name;

                if (sEffect.Contains("Weakness"))
                    sign = -1;
                else if (sEffect.Contains("Resist"))
                    sign = 1;
                else
                    continue;

                int resistInt;
                if (sEffect.Contains("Fire"))
                    resistInt = (int)ResistType.Fire;
                else if (sEffect.Contains("Frost"))
                    resistInt = (int)ResistType.Frost;
                else if (sEffect.Contains("Shock"))
                    resistInt = (int)ResistType.Shock;
                else if (sEffect.Contains("Common Disease"))
                    resistInt = (int)ResistType.CommonDisease;
                else if (sEffect.Contains("Magicka"))
                    resistInt = (int)ResistType.Magicka;
                else if (sEffect.Contains("Poison"))
                    resistInt = (int)ResistType.Poison;
                else
                    continue;

                output.Add((ResistType)resistInt, sign * effect.MinMagnitude);
            }
        }
        return output;
    }

    // compiles all the resistances in a List<Feature>
    public static Dictionary<ResistType, int> GetResistances(List<Feature> features)
    {
        var output = new Dictionary<ResistType, int>();
        foreach (Feature feature in features)
            if (feature.FType == FType.Ability)
            {
                foreach (var pair in feature.Resistances)
                    if (output.ContainsKey(pair.Key))
                        output[pair.Key] += pair.Value;
                    else
                        output.Add(pair.Key, pair.Value);
            }
        return output;
    }

    // Compiles all the AttrBonus in Effects
    Dictionary<AttrName, int> GetAttrBonus()
    {
        var output = new Dictionary<AttrName, int>();

        if (FType == FType.Ability)
            foreach (Effect effect in Effects)
                if (effect.SpellEffect.Name == "Fortify Attribute")
                    output.Add((AttrName)effect.Qualifier, effect.MinMagnitude);

        return output;
    }

    // Compiles all the AttrBonus in a List<Feature>
    public static Dictionary<AttrName, int> GetAttrBonus(List<Feature> features)
    {
        var output = new Dictionary<AttrName, int>();
        foreach (Feature feature in features)
            if (feature.FType == FType.Ability)
            {
                foreach (var pair in feature.AttrBonus)
                    if (output.ContainsKey(pair.Key))
                        output[pair.Key] += pair.Value;
                    else
                        output.Add(pair.Key, pair.Value);
            }
        return output;
    }


    // Packs effects with their effData, and packages this as FeatureData
    public IJsonable GetJsonable()
    {
        PackEffectsData();
        return fData;
    }

    // Converts Feature.List<Effect> to FeatureData.List<EffectData>
    //   Used before writing to json.
    void PackEffectsData()
    {
        var effDataList = new List<EffectData>();

        // for debugging only
        if (Effects.Count == 0)
            Effects.Add(new Effect(this));

        foreach (Effect effect in Effects)
            effDataList.Add((EffectData)effect.GetJsonable());

        fData.effDataList = effDataList;
    }

    // Create Feature.List<Effect> from fData.List<EffectData>
    //   Used after reading from json.
    void UnpackEffectsData()
    {
        Effects.Clear();

        if (fData.effDataList == null
            || fData.effDataList.Count == 0) return;
            
        foreach (EffectData effectData in fData.effDataList)
        {
            Effects.Add(new Effect(this, effectData));
        }
    }

    // Returns false if a necessary piece of the feature is missing
    private bool Validate()
    {
        if (Effects == null) return false;
        if (Effects.Count < 1) return false;
        foreach (Effect effect in Effects)
        {
            if (effect.SpellEffect == null) return false;
            if (effect.HasQualifier && 
                (effect.Qualifier == default
                  || effect.Qualifier.ToString() == "none")) return false;
        }
        if (Data.Features.Contains(this)) return false;

        return true;
    }

    public string PrintEffects()
    {
        StringBuilder output = new StringBuilder();
        foreach (var effect in Effects)
            output.Append(effect + ". ");
        return output.ToString();
    }

    // Static initialization, called by Initializer.
    public static bool isInitialized = false;
    public static void Initialize()
    {
        if (isInitialized) return;
        isInitialized = true;

        // read features from json, save them into Data.Features, and copy
        //   the relevant ones into Data.Races, and Data.Signs

        var featureDataArray = FileUtil.ReadJson<FeatureData>(Constants.FileName.Features);
        foreach (var data in featureDataArray)
        {
            Feature feature = new Feature(data);
            Data.AddFeature(feature, false); // no need to write json during initialize
        }
    }

    public static List<Effect> ExtractEffects(List<Feature> features)
    {
        var effectsOutput = new List<Effect>();

        foreach (var feature in features)
        {
            // only for const features
            if (feature.FType != FType.Ability)
                continue;

            foreach (var effect in feature.effects)
            {

                if (effectsOutput.Select(x => x.QualifiedName).Contains(effect.QualifiedName))
                {
                    Effect existingEffect = effectsOutput.Find(
                        x => x.QualifiedName == effect.QualifiedName);

                    // combine effects
                    existingEffect.MaxMagnitude += effect.MaxMagnitude;
                    existingEffect.MinMagnitude += effect.MinMagnitude;
                }
                else
                {
                    // TODO: check here for weakness/resist match
                    // Dunmer with The Lord sign (weakness/resist Fire)
                    // Breton or Orc with The Apprentice (weakness/resist Magicka)

                    // add alterable copy to output
                    effectsOutput.Add(new Effect(
                        feature, (EffectData)effect.GetJsonable()));
                }
            }
        }

        return effectsOutput;
    }


    public Enum IntToQualifier(int value) 
        => FFile switch
        {
            FeatureFile.Racial => (RaceName)value,
            FeatureFile.Signed => (SignName)value,
            _ => default,
        };

    public int QualifierToInt(Enum value)
        => FFile switch
        {
            FeatureFile.Racial => (int)((RaceName)value),
            FeatureFile.Signed => (int)((SignName)value),
            _ => default,
        };

// IComparable implementation, for sorting
public int CompareTo(Feature other) => SystemName.CompareTo(other.SystemName);

    // IEquatable implementation, for List<Feature>.Contains()
    public bool Equals(Feature other) => SystemName.Equals(other.SystemName);
}
