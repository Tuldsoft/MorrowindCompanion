using System;                       // Enum.GetNames()
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Stores info for a class, uses ISaveObject to load info from CSV into Data
public class MWClass : ISaveObject
{
    string ISaveObject.FileName { get; } = "MWClasses.json";

    // private backing fields
    private string name;
    private string description;
    private SpecName specialization;
    private AttrName[] keyAttributes = new AttrName[2];
    private SkillName[] skills
        = new SkillName[Constants.SkillCount];
    
    // public fields
    public bool isStandard = true;

    // public properties
    public string DisplayName => isStandard ? name : name + " (custom)";
    public string KeyName => isStandard ? name : Constants.Class.CustomKey;
    public string Description => description;
    public SpecName Specialization => specialization;
    public AttrName[] KeyAttributes => keyAttributes;
    public SkillName[] Skills => skills;

    // custom getters
    public int GetSkillValue (SkillName skill)
    {
        int value = 5;
        // Major Skill?
        for (int i = 0; i < 5; i++)
        {
            if (Skills[i] == skill)
            {
                value += 25;
                break;
            }
        }
        // Minor skill?
        if (value == 5)
            for(int i = 5; i < 10; i++)
            {
                if (Skills[i] == skill)
                {
                    value += 10;
                    break;
                }
            }
        // Class Specialization?
        if (Specialization == Data.Skills[skill].specialization)
            value += 5;

        return value;
    }


    public void SetName(string s) => name = (s == DisplayName ? name : s);
    public void SetSpec(SpecName spec) => specialization = spec;
    public void SetKeyAttr(int index, AttrName attr) => keyAttributes[index] = attr;

    // static Initialization, reads a CSV file
    static bool isInitialized = false;

    public static void Initialize()
    {
        if (isInitialized) return;
        isInitialized = true;

        // read CSV, if returns false, then create a single default race
        if (!FileUtil.ReadCSV<MWClass>(Constants.FileName.Classes))
        {
            Debug.LogError("Error while reading csv.");
            Data.Races.Clear();
            MWClass mwClass = DefaultNew();
            Data.Classes.Add(mwClass.KeyName, mwClass);
        }
    }


    // relocate these to Character
    /*protected Gender gender;
    protected RaceName race;
    protected SignName sign;

    protected int level;
    protected int[,] attrIncreases 
        = new int[Enum.GetNames(typeof (AttrName)).Length - 1, Constants.MaxNumLevels];
    protected int[,] skillIncreases 
        = new int[Enum.GetNames(typeof (SkillName)).Length - 1, Constants.MaxNumLevels];*/

    // ISaveObject implementation, for building MWClasses from CSV and storing in Data

    // sets string values from CSV to fields, and registers it in Data
    // returns false if fails
    public bool SetValues(string[] values)
    {
        // Check length
        if (values.Length != 32) return false;

        // 0 name
        // "" is not allowed for name
        if (String.IsNullOrEmpty(values[0])) 
            return false;
        else 
            name = values[0];

        // 1 description, 2 specialization
        description = values[1]; // "" permitted
        if (!Enum.TryParse(values[2], out specialization)) return false;

        // 3 to 4, keyAttributes
        if (!Enum.TryParse(values[3], out keyAttributes[0])) return false;
        if (!Enum.TryParse(values[4], out keyAttributes[1])) return false;

        // 5 to 14, major and minor skills, 6 to 31 misc skills
        for (int i = 5; i < 32; i++)
        {
            if (!Enum.TryParse(values[i], out skills[i - 5])) return false;
        }

        Data.Classes.Add(KeyName, this);
        return true;
    }

    // validates the headings, returns false if fails
    public bool ValidateHeadings(string[] values)
    {
        string[] test = new string[]
        {
            "Name",             // 0
            "Description",      // 1
            "Specialization",   // 2
            "Key Attribute 1",  // 3 to 4
            "Key Attribute 2",
            "Skill 1",          // 5 to 14
            "Skill 2",
            "Skill 3",
            "Skill 4",
            "Skill 5",
            "Skill 6",
            "Skill 7",
            "Skill 8",
            "Skill 9",
            "Skill 10",
            "Skill 11",         // 15 to 31
            "Skill 12",
            "Skill 13",
            "Skill 14",
            "Skill 15",
            "Skill 16",
            "Skill 17",
            "Skill 18",
            "Skill 19",
            "Skill 20",
            "Skill 21",
            "Skill 22",
            "Skill 23",
            "Skill 24",
            "Skill 25",
            "Skill 26",
            "Skill 27"
        };

        if (values.Length != test.Length)
        {
            Debug.LogError("Class header length does not match");
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

    // singular default class in case csv file does not load
    static MWClass DefaultNew()
    {
        return new MWClass()
        {
            name = "Thief",
            description = "Thieves are pickpockets and pilferers. Unlike robbers, who kill and loot, thieves typically choose stealth and subterfuge over violence, and often entertain romantic notions of their charm and cleverness in their acquisitive activities",
            specialization = SpecName.Stealth,
            keyAttributes = new AttrName[2]
                {AttrName.Speed, AttrName.Agility },
            skills = new SkillName[27]
            {
                SkillName.Security,
                SkillName.Sneak,
                SkillName.Acrobatics,
                SkillName.LightArmor,
                SkillName.ShortBlade,
                SkillName.Marksman,
                SkillName.Speechcraft,
                SkillName.HandToHand,
                SkillName.Mercantile,
                SkillName.Athletics,
                SkillName.Block,
                SkillName.Armorer,
                SkillName.MediumArmor,
                SkillName.HeavyArmor,
                SkillName.BluntWeapon,
                SkillName.LongBlade,
                SkillName.Axe,
                SkillName.Spear,
                SkillName.Enchant,
                SkillName.Destruction,
                SkillName.Alteration,
                SkillName.Illusion,
                SkillName.Conjuration,
                SkillName.Mysticism,
                SkillName.Restoration,
                SkillName.Alchemy,
                SkillName.Unarmored
            }
        };
    }

    // copy a class into a custom class
    public static MWClass CopyToCustom(MWClass original)
    {
        MWClass newClass = new MWClass();

        newClass.name = original.name;
        newClass.description = original.Description;
        newClass.specialization = original.Specialization;
        newClass.keyAttributes = new AttrName[2]
            {original.KeyAttributes[0] , original.KeyAttributes[1] };
        for (int i = 0; i < newClass.skills.Length; i++)
            newClass.skills[i] = original.skills[i];
        newClass.isStandard = false;

        return newClass;
    }

    // non-static string helper functions
    public string GetMiscAsString()
    {
        StringBuilder output = new StringBuilder();

        output.AppendLine("Specialization:");
        output.AppendLine(Specialization.ToString());
        output.AppendLine();
        output.AppendLine("Key Attributes:");
        output.AppendLine(KeyAttributes[0].ToString());
        output.AppendLine(KeyAttributes[1].ToString());

        return output.ToString();
    }

    public string GetFiveSkillsAsString(bool major = true)
    {
        StringBuilder output = new StringBuilder();

        output.AppendLine(
            major ? "Major Skills:" : "Minor Skills:");
        int start = major ? 0 : 5;
        for (int i = start; i < start + 5; i++)
            output.AppendLine(Data.Skills[Skills[i]].displayName);

        return output.ToString();
    }

    public void KeyAttrSwap()
    {
        AttrName temp = keyAttributes[0];
        keyAttributes[0] = keyAttributes[1];
        keyAttributes[1] = temp;
    }

    public void SkillSwap(int first, int second)
    {
        SkillName temp = skills[first];
        skills[first] = skills[second];
        skills[second] = temp;
    }
}

/*public class MWClass : MWClassData
{
    // relocate these to Character
    *//*public Gender Gender => gender;
    public RaceName Race => race;
    public SignName Sign => sign;
    public int Level => level;
    public int[,] AttrIncreases => attrIncreases;
    public int[,] SkillIncreases => skillIncreases;*//*
}*/
