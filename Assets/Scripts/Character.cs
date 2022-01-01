using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A player character. For now, a composite of name, gender, race, class, and sign.
// TODO: Implement a system for tracking level-ups
public class Character
{
    // Store only keys in the character. Use keys to redirect to infor in Data.
    string name = "";
    Gender gender = Gender.none;
    RaceName race = RaceName.none;
    string mwClassKey = "";
    SignName sign = SignName.none;

    // Properties
    public string Name { get => name; set => name = value; }
    public Gender Gender { get => gender; set => gender = value; }

    // Attributes
    public int Strength => GetAttrValue(AttrName.Strength) ;
    public int Intelligence => GetAttrValue(AttrName.Intelligence);
    public int Willpower => GetAttrValue(AttrName.Willpower);
    public int Agility => GetAttrValue(AttrName.Agility);
    public int Speed => GetAttrValue(AttrName.Speed);
    public int Endurance => GetAttrValue(AttrName.Endurance);
    public int Personality => GetAttrValue(AttrName.Personality);
    public int Luck => GetAttrValue(AttrName.Luck);



    // Derived Attributes
    // initial max health ignores +25 Endurance from The Lady sign
    // max health increased by 10% of Endurance at level up, incluing The Lady sign
    float MaxHealth_Initial => (Strength + GetAttrValue(AttrName.Endurance, true)) / 2;
    public float MaxHealth => MaxHealth_Initial; // plus health from level ups

    // magicka = Int x (1 + Racial modifier + Birthsign modifier) + Fortify Magicka
    public float MaxMagicka => Intelligence * MagickaMultiplier;

    // Fatigue = Str + Wil + Agi + End
    public int MaxFatigue => Strength + Willpower + Agility + Endurance;


    // Compiles constant ability features into a list of active effects
    private List<Feature> activeFeatures => GetActiveFeatures();
    public List<Effect> ActiveEffects => Feature.ExtractEffects(activeFeatures);


    // shortcut getters point to the instance stored in Data, using character keys
    public Race Race => race == RaceName.none ? null : Data.Races[race];
    public MWClass MWClass => (string.IsNullOrEmpty(mwClassKey) || mwClassKey == "none") ? null 
                : Data.Classes[mwClassKey];
    public Sign Sign => sign == SignName.none ? null : Data.Signs[sign];

    public bool IsValid => gender != Gender.none && Race != null 
        && MWClass != null && Sign != null;

    public float MagickaMultiplier => 1 + (Race == null ? 0 : Race.MagickaMultiplier)
        + (Sign == null ? 0 : Sign.MagickaMultiplier);


    // Methods

    // Custom setters save only keys, used in referencing Data, used by MainMonitor.Refresh()
    public void SetRace(RaceName race) => this.race = race;
    public void SetClass(string mwClassKey) => this.mwClassKey = mwClassKey;
    public void SetSign(SignName sign) => this.sign = sign;


    // Getters for compiled values
    public int GetAttrValue(AttrName attr, bool ignoreSign = false)
    {
        // if (!IsValid) return 0;
        if (attr == AttrName.none) return 0;

        int value = 0;

        // add value from race
        if (Race != null) 
            value += Race.AttrValues[(gender, attr)];
        
        // add bonus from class
        if (MWClass != null && MWClass.KeyAttributes[0] == attr)
            value += 10;
        else if (MWClass != null && MWClass.KeyAttributes[1] == attr)
            value += 10;

        // add bonus from sign
        if (Sign != null && ignoreSign == false)
            value += Sign.GetAttrBonus(attr);

        return value;
    }

    public int GetSkillValue(SkillName skill)
    {
        //if (!IsValid) return 0;
        if (skill == SkillName.none) return 0;

        int value = 0;

        // add value from class
        if (MWClass != null)
            value = MWClass.GetSkillValue(skill);
        
        // add bonuses from race
        if (Race != null)
            value += Race.GetSkillBonus(skill);

        return value;
    }

    List<Feature> GetActiveFeatures()
    {
        List<Feature> features = new List<Feature>();

        // add from Race
        if (Race != null)
            foreach (var feature in Data.Races[Race.Name].Features)
                if (feature.FType == FType.Ability)
                    features.Add(feature);

        // add from Sign
        if (Sign != null)
            foreach (var feature in Data.Signs[Sign.Name].Features)
                if (feature.FType == FType.Ability)
                    features.Add(feature);

        features.Sort();

        return features;
    }

    // returns a dictionary with its contents added in the correct order
    public Dictionary<SkillName, int> GetSkills()
    {
        var output = new Dictionary<SkillName, int>();

        if (MWClass == null)
        {
            // fill with alphabetical skills
            for (int i = 0; i < Constants.SkillCount; i++)
            {
                SkillName skill = (SkillName)(i + 1);
                output.Add(skill, GetSkillValue(skill));
            }
        }
        else
        {
            // set Maj/Min/Misc skills
            for (int i = 0; i < Constants.SkillCount; i++)
            {
                SkillName skill = MWClass.Skills[i];
                output.Add(skill, GetSkillValue(skill));
            }
        }

        return output;
    }
}
