using System;
using System.Text;
using UnityEngine;

// A Feature contains one or more Effects. An Effect uses EffectData for backing
//   variables, and contains a SpellEffect.

public enum EffectRange { Self, Touch, Target }

// Backing variables, contains only jsonable fields, including one SpellEffectData
[System.Serializable]
public class EffectData : IJsonable
{
    public string FileName => "Effects.json";
    public int minMag = 0;
    public int maxMag = 0;
    public int duration = 0;
    public int area = 0;
    public EffectRange effectRange = EffectRange.Self;
    public int qualifier = default; // interpreted as Enum, but int is jsonable
    public SpellEffectData spellEffectData = null;

    public EffectData() { }

    public EffectData(EffectData original)
    {
        minMag = original.minMag;
        maxMag = original.maxMag;
        duration = original.duration;
        area = original.area;
        effectRange = original.effectRange;
        qualifier = original.qualifier;
        spellEffectData = original.spellEffectData;
    }
}

// Stores the values of the effect. Which values are used is determined by SpellEffect.
//   Effects belong to a feature. SpellEffects belong to Effects.
public class Effect : IComparable<Effect>
{
    // Owning feature, must be set via Constructor
    Feature feature = null;

    // Jsonable backing data
    EffectData effData = new EffectData();

    // Set by dropdown selection, initially null
    // retrieved as SpellEffectData in GetJson()
    public SpellEffect SpellEffect { get; set; } = null;

    public bool IsClamped { get; set; } = true;

    // Public Properties. Many of these are modified versions of the backing variables
    //   in effData or in SpellEffect.
    public string QualifiedName {
        get
        {
            if (HasQualifier && Qualifier is AttrName attrName)
            {
                string se = SpellEffect;  // SpellEffect has implicit ToString() => Name
                string attr = attrName.ToString(); 
                return se.Substring(0, se.Length - "Attribute".Length) + attr;
            }

            if (HasQualifier && Qualifier is SkillName skillName)
            {
                string se = SpellEffect;
                string skill = skillName.ToString();
                return se.Substring(0, se.Length - "Skill".Length) + skill;
            }

            return this.SpellEffect;
        }
    }
    
    public bool HasMagnitude => SpellEffect != null && SpellEffect.HasMagnitude;

    public int MinMagnitude {
        get => HasMagnitude ? effData.minMag : 0;
        set
        {
            //effData.minMag = Mathf.Clamp(value, 0, 500);
            effData.minMag = value; // clamp provided by slider
            if (effData.maxMag < effData.minMag)
                effData.maxMag = effData.minMag;
        }
    }

    public int MaxMagnitude
    {
        get => HasMagnitude ? effData.maxMag : 0;
        set
        {
            //effData.maxMag = Mathf.Clamp(value, 0, 500);
            effData.maxMag = value; // clamp provided by slider
            if (effData.maxMag < effData.minMag)
                effData.minMag = effData.maxMag;
        }
    }


    public bool HasDuration => SpellEffect != null 
        && SpellEffect.HasDuration
        && !feature.IsConstant;
    public int Duration
    {
        get => HasDuration ? effData.duration : 0;
        //set => effData.duration = Mathf.Clamp(value, 0, 300);
        set => effData.duration = value; // clamp provided by slider
    }

    public bool HasArea => SpellEffect != null && EffectRange != EffectRange.Self;
    public int Area
    {
        get => HasArea ? effData.area : 0;
        //set => effData.area = Mathf.Clamp(value, 0, 50);
        set => effData.area = value; // clamp provided by slider
    }

    public bool HasRange => SpellEffect != null && feature.FType != FType.Ability;
    public EffectRange EffectRange 
    {
        get => effData.effectRange;
        set 
        {
            if (!IsConstant)
                effData.effectRange = value;
            else
                effData.effectRange = EffectRange.Self;
        } 
    }
    
    public bool IsConstant => feature.IsConstant;

    // Qualifier can be either AttrName or SkillName. Uses helper method.
    public bool HasQualifier => SpellEffect.HasQualifier;
    public Enum Qualifier { 
        get => HasQualifier && SpellEffect != null ? 
            ConvertIntToQualifier(effData.qualifier) : default;
        set => effData.qualifier = ConvertQualifierToInt(value); }

    public float BaseCost => SpellEffect != null ? SpellEffect.BaseCost : 0;


    // Constructor, only sets feature. 
    public Effect(Feature feat) => this.feature = feat;

    // Constructor, sets feature and effData, unpacks spellEffectData
    public Effect(Feature feat, EffectData data)
    {
        feature = feat;
        effData = new EffectData(data);
        SpellEffect = new SpellEffect(data.spellEffectData);
    }

    // Methods
    // Expose backing data for json creation
    public IJsonable GetJsonable()
    {
        // for debugging only, create a default SpellEffect
        if (SpellEffect == null)
            SpellEffect = new SpellEffect();
        effData.spellEffectData = (SpellEffectData)SpellEffect.GetJsonable();
        return effData;
    }

    // Repackages a jsonable int into the correct Enum
    Enum ConvertIntToQualifier(int number)
    {
        string name = SpellEffect.ToString();
        if (name.Substring(name.Length - "Attribute".Length) == "Attribute")
            return (AttrName)number;
        if (name.Substring(name.Length - "Skill".Length) == "Skill")
            return (SkillName)number;
        return default;
    }

    // Repackages an Enum into a jsonable int
    int ConvertQualifierToInt(Enum qualifier)
    {
        if (qualifier is AttrName attr) return (int)attr;
        if (qualifier is SkillName skill) return (int)skill;
        return 0;
    }

    // Converts Magnitude to a readable format
    public string MagToString()
    {
        if (!HasMagnitude)
            return String.Empty;

        // Special case
        if (SpellEffect.Name.Equals("Fortify Maximum Magicka"))
            return (((float)MinMagnitude / 10)).ToString() + "x INT";

        string value;
        if (MinMagnitude == MaxMagnitude)
            value = MinMagnitude.ToString();
        else
            value = MinMagnitude.ToString() + " to " + MaxMagnitude.ToString();

        string suffix;
        // More Special cases
        if (SpellEffect.Name.Contains("Command")
            && SpellEffect.School == SpellSchool.Conjuration)
            suffix = " levels"; // M levels
        else if (SpellEffect.Name.Contains("Weakness")
            || SpellEffect.Name.Equals("Blind")
            || SpellEffect.Name.Equals("Chameleon")
            || SpellEffect.Name.Equals("Dispel")
            || SpellEffect.Name.Equals("Reflect")
            || SpellEffect.Name.Contains("Resist"))
            suffix = "%"; // M%
        else if (SpellEffect.Name.Contains("Detect")
            || SpellEffect.Name.Contains("Telekinesis"))
            suffix = " feet"; // M feet
        else
            suffix = " pts";

        return value + suffix;
    }

    // Creates a "printout" of what the effect does. Also used for implicit conversion.
    public override string ToString()
    {
        if (SpellEffect == null) return String.Empty;

        StringBuilder output = new StringBuilder();
        output.Append(QualifiedName); // name is qualified as needed
        
        if (HasMagnitude)
            output.Append(" " + MagToString());

        if (HasDuration && Duration > 1)
            output.Append(" for " + Duration.ToString() + " secs");

        if (HasArea && Area > 1)
            output.Append(" in " + Area.ToString() + "ft");

        if (!IsConstant)
            output.Append(" on " + EffectRange.ToString());

        return output.ToString();
    }

    public static implicit operator string(Effect rhs) => rhs.ToString();

    // IComparable implementation. Compare String representations
    public int CompareTo(Effect otherEffect)
        => this.ToString().CompareTo(otherEffect.ToString());

}




