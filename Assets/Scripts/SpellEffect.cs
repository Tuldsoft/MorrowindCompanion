using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum SpellSchool { none, Alteration, Conjuration, Destruction, 
    Illusion, Mysticism, Restoration }

//public enum ItemGroup { none, Weapon, Armor }

//public enum Weapon { none, BattleAxe, Dagger, Longbow, Longsword, Mace, Spear }

//public enum Armor { none, Boots, Cuirass, LeftGauntlet, RightGauntlet, Helm, Shield }

public enum RangeRestriction { none, SelfOnly, TouchTargetOnly }

// Serializable backing data. Only exposed through SpellEffect.GetJsonable()
[System.Serializable]
public class SpellEffectData : IJsonable
{
    public string FileName => "SpellEffect.json";

    public string name = "Default SpellEffect Name";
    public SpellSchool school = SpellSchool.Alteration;
    public string description = "Default SpellEffect Description";
    public float baseCost = 0f;
    public bool hasMagnitude = true;
    public bool hasDuration = true;
    public RangeRestriction restriction = RangeRestriction.none;
    public bool hasQualifier = false;
}

// An Effect contains a SpellEffect, which determines which of the
//   Effect's properties can be set.
public class SpellEffect : ISaveObject
{
    // private backing data
    SpellEffectData seData = new SpellEffectData();

    public string Name => seData.name;
    public string SystemName => seData.name.Replace(' ', '_');
    public SpellSchool School => seData.school;
    public string Description => seData.description;
    public float BaseCost => seData.baseCost;
    public bool HasMagnitude => seData.hasMagnitude;
    public bool HasDuration => seData.hasDuration;
    public RangeRestriction Restriction => seData.restriction;
    public bool HasQualifier => seData.hasQualifier;

    // Expose interior data for making Json
    public IJsonable GetJsonable() => seData;

    public SpellEffect() { }
    public SpellEffect(SpellEffectData data) => seData = data;

    // Initialize to load info from CSV
    static bool isInitialized = false;
    public static void Initialize()
    {
        if (isInitialized) return;
        isInitialized = true;

        if (!FileUtil.ReadCSV<SpellEffect>(Constants.FileName.SpellEffects))
        {
            Debug.LogError("Error while reading " + Constants.FileName.SpellEffects);
            Data.SpellEffects.Clear();
            Data.SpellEffects.Add("Default Name", new SpellEffect());
        }

    }

    // ISaveObject Implementation
    public string FileName => "SpellEffects.json";

    public bool SetValues(string[] values)
    {
        if (values.Length != 8) return false;

        // 0 name
        if (String.IsNullOrEmpty(values[0])
            || String.IsNullOrWhiteSpace(values[0]))
            return false;
        else
            seData.name = values[0];
        
        // 1 school
        if (!Enum.TryParse(values[1], out seData.school)) return false;

        // 2 description
        if (String.IsNullOrEmpty(values[2])
            || String.IsNullOrWhiteSpace(values[2]))
            return false;
        else
            seData.description = values[2];

        if (!float.TryParse(values[3], out seData.baseCost)) return false;
        if (!bool.TryParse(values[4], out seData.hasMagnitude)) return false;
        if (!bool.TryParse(values[5], out seData.hasDuration)) return false;
        if (!Enum.TryParse(values[6], out seData.restriction)) return false;
        if (!bool.TryParse(values[7], out seData.hasQualifier)) return false;

        Data.SpellEffects.Add(Name, this);

        return true;
    }

    public bool ValidateHeadings(string[] values)
    {
        // Name, School, Description, BaseCost, HasMagnitude, HasDuration, Restriction
        /*return values.SequenceEqual(new string[]
            { "Name", "School", "Description", "BaseCost", "HasMagnitude", "HasDuration","Restriction", "HasQualifier" });*/

        string[] test = new string[]
        { "Name", "School", "Description", "BaseCost", "HasMagnitude",
            "HasDuration","Restriction", "HasQualifier" };

        if (values.Length != test.Length)
        {
            Debug.LogError("SpellEffects header length does not match");
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

    public override string ToString() => Name;

    public static implicit operator string(SpellEffect rhs) => rhs.ToString();
}

public static class SpellEffectBuilder
{

}
