using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public enum SkillName
{
    none = 0,
    Acrobatics = 1,
    Alchemy = 2,
    Alteration = 3,
    Armorer = 4,
    Athletics = 5,
    Axe = 6,
    Block = 7,
    BluntWeapon = 8,
    Conjuration = 9,
    Destruction = 10,
    Enchant = 11,
    HandToHand = 12,
    HeavyArmor = 13,
    Illusion = 14,
    LightArmor = 15,
    LongBlade = 16,
    Marksman = 17,
    MediumArmor = 18,
    Mercantile = 19,
    Mysticism = 20,
    Restoration = 21,
    Security = 22,
    ShortBlade = 23,
    Sneak = 24,
    Spear = 25,
    Speechcraft = 26,
    Unarmored = 27
}

public enum SpecName { Combat, Magic, Stealth }

[System.Serializable]
public class Skill : ISaveObject
{
    string ISaveObject.FileName { get; } = "Skills.json";

    public SkillName name;
    public string displayName;
    public SpecName specialization;
    public AttrName attribute;

    // sets string values from CSV to fields, and registers it in Data
    // returns false if fails
    public bool SetValues(string[] values)
    {
        if (values.Length != 4) return false;

        if (!Enum.TryParse(values[0], out name)) return false;
        displayName = values[1];
        if (!Enum.TryParse(values[2], out specialization)) return false;
        if (!Enum.TryParse(values[3], out attribute)) return false;

        Data.Skills.Add(name, this);
        return true;
    }

    // validates the headings, returns false if fails
    public bool ValidateHeadings(string[] values)
    {
        return values.SequenceEqual(new string[]
            { "Name", "DisplayName", "Specialization", "Attribute" });
    }
        
    
    // static members


    static bool isInitialized = false;
    
    // populates Data from a CSV, called by Initializer upon scene awake
    // if CSV load fails, loads a couple sample Skills
    public static void Initialize()
    {
        if (isInitialized) return;
        isInitialized = true;
        
        if (!FileUtil.ReadCSV<Skill>(Constants.FileName.Skills))
        {
            Debug.LogError("Error while reading csv.");
            Data.Skills.Clear();
            Data.Skills.Add(SkillName.Alchemy,
                new Skill
                {
                    name = SkillName.Alchemy,
                    displayName = "Alchemy",
                    specialization = SpecName.Magic,
                    attribute = AttrName.Intelligence
                });

            Data.Skills.Add(SkillName.Block,
                new Skill
                {
                    name = SkillName.Block,
                    displayName = "Block",
                    specialization = SpecName.Combat,
                    attribute = AttrName.Agility
                });
        }
    }

}
