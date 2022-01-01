using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SignName
{
    none,
    Warrior,
    Mage,
    Thief,
    Serpent,
    Lady,
    Steed,
    Lord,
    Apprentice,
    Atronach,
    Ritual,
    Lover,
    Shadow,
    Tower
}
public class Sign : ISaveObject
{
    string ISaveObject.FileName { get; } = "Signs.json";

    // backing fields, because properties cannot be used as out parameters
    SignName name = SignName.none;
    string subName = "";
    string description = "";

    // public properties
    public SignName Name => name;
    public string DisplayName { get => "The " + Name.ToString(); }
    public string SubName => subName;
    public string Description => description;

    // Features-based properties
    public float MagickaMultiplier => Feature.GetMagickaMultiplier(Features);
    public Dictionary<ResistType, int> Resistances => Feature.GetResistances(Features);
    public Dictionary<AttrName, int> AttrBonus => Feature.GetAttrBonus(Features);

    
    // Populated by Data.AddFeature()
    public List<Feature> Features { get; set; } = new List<Feature>();

    // Getter for values
    public int GetResistance(ResistType resist) 
        => Resistances.ContainsKey(resist) ? Resistances[resist] : 0;
    public int GetAttrBonus(AttrName attr) 
        => AttrBonus.ContainsKey(attr) ? AttrBonus[attr] : 0;



    /*// Getter Methods
    Dictionary<ResistType, int> GetResistances()
    {
        var output = new Dictionary<ResistType, int>();
        foreach (Feature feature in Features)
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

    Dictionary<AttrName, int> GetAttrBonus()
    {
        var output = new Dictionary<AttrName, int>();
        foreach (Feature feature in Features)
            if (feature.FType == FType.Ability)
            {
                foreach (var pair in feature.AttrBonus)
                    if (output.ContainsKey(pair.Key))
                        output[pair.Key] += pair.Value;
                    else
                        output.Add(pair.Key, pair.Value);
            }
        return output;
    }*/


    // Static initialization, load non-feature data from a csv
    static bool isInitialized = false;
    public static void Initialize()
    {
        if (isInitialized) return;
        isInitialized = true;


        // read CSV, if returns false, then create a single default race
        if (!FileUtil.ReadCSV<Sign>(Constants.FileName.Signs))
        {
            Debug.LogError("Error while reading csv.");
            Data.Signs.Clear();
            Sign sign = new Sign();
            Data.Signs.Add(sign.Name, sign);
        }

        /*foreach (SignName sign in Enum.GetValues(typeof(SignName)))
        {
            if (sign != SignName.none)
            {
                Data.Signs.Add(sign, new Sign());
            }
        }*/
    }


    // ISaveObject implementation
    // sets string values from CSV to fields, and registers it in Data
    // returns false if fails
    public bool SetValues(string[] values)
    {
        // Check length
        if (values.Length != 3) return false;

        // 0 name, 1 subname, 2 description
        if (!Enum.TryParse(values[0], out name)) return false;
        subName = values[1];
        description = values[2];

        Data.Signs.Add(name, this);
        return true;
    }

    // validates the headings, returns false if fails
    public bool ValidateHeadings(string[] values)
    {
        string[] test = new string[]{ 
            "Name", "SubName", "Description" };
        if (values.Length != test.Length)
        {
            Debug.LogError("Signs header length does not match.");
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

    public string PrintSign()
    {
        StringBuilder sb = new StringBuilder();

        foreach (var feature in Features)
            sb.Append("(" + feature.FType.ToString() + ") " + feature.Name
                + ": " + feature.PrintEffects() + "\r\n");

        return sb.ToString();
    }

    public override string ToString() => DisplayName;

    public static implicit operator string(Sign rhs) => rhs.DisplayName;

}
