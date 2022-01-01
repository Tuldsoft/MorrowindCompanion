using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Gender { none = 0, Male = 1, Female = 2 }
public enum RaceName 
{ 
    none = 0, 
    Altmer = 1, 
    Argonian = 2, 
    Bosmer = 3, 
    Breton = 4, 
    Dunmer = 5, 
    Imperial = 6, 
    Khajiit = 7, 
    Nord = 8, 
    Orc = 9, 
    Redguard = 10
}

public enum ResistType
{
    Magicka = 0,
    Fire = 1,
    Frost = 2,
    Shock = 3,
    Poison = 4,
    CommonDisease = 5
}

public class Race : ISaveObject
{
    string ISaveObject.FileName { get; } = "Races.json";

    // private backing fields (Enum.TryParse(.. out field))
    private RaceName name; // used as out parameter in TryParse, cannot use auto-properties
    private Dictionary<(Gender, AttrName), int> attrValues
        = new Dictionary<(Gender, AttrName), int>();
    private Dictionary<SkillName, int> skillBonus
        = new Dictionary<SkillName, int>();
    //private float magickaMultiplier; // Feature: Fortify Maximum Magicka
    // 0 Magicka, 1 Fire, 2, Frost, 3 Shock, 4 Poison, 5 Common Disease
    //private int[] resistances = new int[6];
    private Dictionary<Gender, float> height
        = new Dictionary<Gender, float>();
    private Dictionary<Gender, float> weight
        = new Dictionary<Gender, float>();


    // public properties, get only
    public RaceName Name => name;
    public Dictionary<(Gender, AttrName), int> AttrValues => attrValues; 
    public Dictionary<SkillName, int> SkillBonus => skillBonus; 
    
    //public float MagickaMultiplier => magickaMultiplier; 
    //public int[] Resistances => resistances;
    public Dictionary<Gender, float> Height => height; 
    public Dictionary<Gender, float> Weight => weight;

    // Non-serialized Features, populated from Data.AddFeature()
    public List<Feature> Features { get; set; } = new List<Feature>();

    // Feature-derived properties
    public float MagickaMultiplier => Feature.GetMagickaMultiplier(Features);
    public Dictionary<ResistType, int> Resistances => Feature.GetResistances(Features);


    // Methods
    public int GetSkillBonus(SkillName skill) => SkillBonus.ContainsKey(skill) ? SkillBonus[skill] : 0;


    // sets string values from CSV to fields, and registers it in Data
    // returns false if fails
    public bool SetValues(string[] values)
    {
        // Check length
        if (values.Length != 55) return false;

        // name = values[0]       
        if (!Enum.TryParse(values[0], out name)) return false;

        // male attr = values[1-8]
        // female attr = values[9 to 16]
        for (int gender = 1; gender < 3; gender++)
        {
            for (int attr = 1; attr <= Constants.AttrCount; attr++)
            {
                if (!int.TryParse(values[((gender - 1) * Constants.AttrCount) + attr],
                    out int value)) return false;

                attrValues.Add(((Gender)gender, (AttrName)attr), value);
            }
        }

        // skills = values[17 to 43]
        for (int skill = 1; skill <= Constants.SkillCount; skill++)
        {
            if (!int.TryParse(values[17 + skill - 1],
                    out int value))
            {
                if (String.IsNullOrEmpty(values[17 + skill - 1]))
                    value = 0;
                else
                    return false;
            }

            if (value != 0)
                skillBonus.Add((SkillName)skill, value);
        }

        // 44 magicka multiplier, 45 to 50 Resistances[6]
        /*if (!float.TryParse(values[44], out magickaMultiplier))
        {
            if (String.IsNullOrEmpty(values[44]))
                magickaMultiplier = 0;
            else
                return false;
        }*/

        /*for (int i = 45; i < (45 + 6); i++)
            if (!int.TryParse(values[i], out resistances[i - 45]))
            {
                if (String.IsNullOrEmpty(values[i]))
                    resistances[i - 45] = 0;
                else
                    return false;
            }*/

        // 51 to 52 Height, 53 to 54 Weight
        {
            if (!float.TryParse(values[51], out float value)) return false;
            height.Add(Gender.Male, value);

            if (!float.TryParse(values[52], out value)) return false;
            height.Add(Gender.Female, value);

            if (!float.TryParse(values[53], out value)) return false;
            weight.Add(Gender.Male, value);

            if (!float.TryParse(values[54], out value)) return false;
            weight.Add(Gender.Female, value);
        }

        Data.Races.Add(name, this);
        return true;
    }

    // validates the headings, returns false if fails
    public bool ValidateHeadings(string[] values)
    {
        string[] test = new string[]
        {
            "Name",                                                   // 0
                "STR", "INT", "WIL", "AGI", "SPD", "END", "PER", "LUK", // 1 to 8
                "STR", "INT", "WIL", "AGI", "SPD", "END", "PER", "LUK", // 9 to 16
                "Acrobatics", "Alchemy", "Alteration",                  // 17 to 43
                "Armorer", "Athletics", "Axe",
                "Block", "BluntWeapon", "Conjuration",
                "Destruction", "Enchant", "HandToHand",
                "HeavyArmor", "Illusion", "LightArmor",
                "LongBlade", "Marksman", "MediumArmor",
                "Mercantile", "Mysticism", "Restoration",
                "Security", "ShortBlade", "Sneak",
                "Spear","Speechcraft", "Unarmored",
                "MagickaMultiplier",                                    // 44
                "MagickaResist", "FireResist", "FrostResist",           // 45 - 49
                "ShockResist", "PoisonResist", "CommonDiseaseResist",
                "HeightM", "HeightF", "WeightM", "WeightF"
        };
        if (values.Length != test.Length)
        {
            Debug.LogError("Race header length does not match");
            return false;
        }

        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] != test[i])
            {
                Debug.LogError("Error matching " + values[i] + " to " + test[i]);
                return false;
            }
        }

        return true;
    }

    // Converts a Race to a string listing of details
    public string GetDetails(Gender gender)
    {
        StringBuilder output = new StringBuilder();

        output.Append(GetAttrAsString(gender));
        output.AppendLine();
        output.Append(GetSkillBonusAsString());
        output.AppendLine();
        output.Append(GetOtherFeaturesAsString(gender));

        return output.ToString();
    }

    public int GetAttrValue(AttrName attrName, Gender gender) => AttrValues[(gender, attrName)];

    public string GetAttrAsString(Gender gender)
    {
        StringBuilder output = new StringBuilder();
        foreach (var pair in AttrValues.Where(x => x.Key.Item1 == gender))
        {
            output.AppendLine($"{pair.Key.Item2}, {pair.Value}");
        }

        return output.ToString();
    }

    public string GetSkillBonusAsString()
    {
        StringBuilder output = new StringBuilder();
        foreach (var pair in SkillBonus)
        {
            output.AppendLine($"{Data.Skills[pair.Key].displayName}, {pair.Value}");
        }
        return output.ToString();
    }

    public string GetOtherFeaturesAsString(Gender gender)
    {
        StringBuilder output = new StringBuilder();
        // Resistances
        /*foreach (var pair in Resistances)
        {
            if (pair.Value > 0)
                output.AppendLine($"Resist {pair.Key}: {pair.Value}");
            else
                output.AppendLine($"Weakness to {pair.Key}: {pair.Value}");
        }*/

        // Other
        //output.AppendLine($"Magicka Multiplier: {MagickaMultiplier}");
        output.AppendLine($"Height: {Height[gender]}, Weight: {Weight[gender]}");

        // Features (spells, powers, etc.)
        foreach (var feature in Features)
        {
            if (feature.FType != FType.Ability)
                output.Append("(" + feature.FType.ToString() + ") " + feature.Name + ": ");
            output.Append(feature.PrintEffects() + "\r\n");
        }

        return output.ToString();
    }

    static bool isInitialized = false;

    // populates Data from a CSV, called by Initializer upon scene awake
    public static void Initialize()
    {
        if (isInitialized) return;
        isInitialized = true;

        // read CSV, if returns false, then create a single default race
        if (!FileUtil.ReadCSV<Race>(Constants.FileName.Races))
        {
            Debug.LogError("Error while reading csv.");
            Data.Races.Clear();
            Race race = DefaultNew();
            Data.Races.Add(race.name, race);
        }
    }

    // creates and returns the Imperial race as a default
    public static Race DefaultNew()
    {
        return new Race
        {
            name = RaceName.Imperial,
            attrValues = new Dictionary<(Gender, AttrName), int>()
            {
                { (Gender.Male, AttrName.Strength), 40 },
                { (Gender.Male, AttrName.Intelligence), 40 },
                { (Gender.Male, AttrName.Willpower), 30 },
                { (Gender.Male, AttrName.Agility), 30 },
                { (Gender.Male, AttrName.Speed), 40 },
                { (Gender.Male, AttrName.Endurance), 40 },
                { (Gender.Male, AttrName.Personality), 50 },
                { (Gender.Male, AttrName.Luck), 40 },
                { (Gender.Female, AttrName.Strength), 40 },
                { (Gender.Female, AttrName.Intelligence), 40 },
                { (Gender.Female, AttrName.Willpower), 40 },
                { (Gender.Female, AttrName.Agility), 30 },
                { (Gender.Female, AttrName.Speed), 30 },
                { (Gender.Female, AttrName.Endurance), 40 },
                { (Gender.Female, AttrName.Personality), 50 },
                { (Gender.Female, AttrName.Luck), 40 }
            },
            skillBonus = new Dictionary<SkillName, int>()
            {
                { SkillName.BluntWeapon, 5 },
                { SkillName.HandToHand, 5 },
                { SkillName.LightArmor, 5 },
                { SkillName.LongBlade, 10 },
                { SkillName.Mercantile, 10 },
                { SkillName.Speechcraft, 10 }
            },
            //magickaMultiplier = 0,
            //resistances = new int[6],
            height = new Dictionary<Gender, float>()
            {
                { Gender.Male, 1.0f },
                { Gender.Female, 1.0f }
            },
            weight = new Dictionary<Gender, float>()
            {
                {Gender.Male, 1.25f },
                {Gender.Female, 0.95f }
            }
        };
    }



}


