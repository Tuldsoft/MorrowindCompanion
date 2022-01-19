using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A player character. For now, a composite of name, gender, race, class, and sign.
// TODO: Implement a system for tracking level-ups
public class Character
{
    // Fields
    string name = "";
    Gender gender = Gender.none;
    RaceName race = RaceName.none;
    string mwClassKey = "";
    SignName sign = SignName.none;
    DateTime saveTime = DateTime.MinValue;
    int saveSlot = int.MinValue;

    // Properties
    public DateTime SaveTime => saveTime;
    public int SaveSlot => saveSlot;
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



    // Features
    // Compiles constant ability features into a list of active effects
    private List<Feature> activeFeatures => GetActiveFeatures();
    public List<Effect> ActiveEffects => Feature.ExtractEffects(activeFeatures);

    // Race
    public Race Race => race == RaceName.none ? null : Data.Races[race];
    public RaceName RaceName => race;

    // Class
    public MWClass MWClass
    {
        get
        {
            if (string.IsNullOrEmpty(mwClassKey)
                || mwClassKey == Constants.Class.NoneKey
                || !Data.Classes.ContainsKey(mwClassKey))
            {
                SetClass(Constants.Class.NoneKey);
                return null;
            }
            else 
                return Data.Classes[mwClassKey];
        }
    }

    // Sign
    public Sign Sign => sign == SignName.none ? null : Data.Signs[sign];
    public SignName SignName => sign;


    // Other Properties (getters)
    public bool IsValid => gender != Gender.none && Race != null 
        && MWClass != null && Sign != null;

    public float MagickaMultiplier => 1 + (Race == null ? 0 : Race.MagickaMultiplier)
        + (Sign == null ? 0 : Sign.MagickaMultiplier);

    public bool IsResetable =>
            (!string.IsNullOrEmpty(Name)
            || Gender != Gender.none
            || Race != null
            || MWClass != null
            || Sign != null) ? true : false;

    public bool IsSaveable => (!string.IsNullOrEmpty(Name));

    // Methods 

    // Custom setters save only keys, used in referencing Data, used by MainMonitor.Refresh()
    public void SetName(string name) => this.name = name;
    public void SetGender(Gender gender) => this.gender = gender;
    public void SetRace(RaceName race) => this.race = race;
    public void SetClass(string mwClassKey) => this.mwClassKey = mwClassKey;
    public void SetSign(SignName sign) => this.sign = sign;


    // Getter Methods for compiled values
    public int GetAttrValue(AttrName attr, bool ignoreSign = false)
    {
        // if (!IsValid) return 0;
        if (attr == AttrName.none) return 0;

        int value = 0;

        // add value from race
        if (Race != null && Gender != Gender.none) 
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
        if (Race != null && Gender != Gender.none)
            value += Race.GetSkillBonus(skill);

        return value;
    }

    List<Feature> GetActiveFeatures()
    {
        List<Feature> features = new List<Feature>();

        // add from Race
        if (Race != null && Gender != Gender.none)
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

    // Methods

    public void Reset()
    {
        SetName(string.Empty);
        SetGender(Gender.none);
        SetClass(string.Empty);
        SetRace(RaceName.none);
        SetSign(SignName.none);
    }

    // Static initializer, for loading user-defined characters from files
    static bool isInitialized = false;
    public static void Initialize()
    {
        if (isInitialized) return;
        isInitialized = true;

        // read user-created files
        var saveObjects = FileUtil.LoadBSOsFromFiles<Character.CharBasicData>(
            Constants.Character.FileExtension);

        foreach (var saveObject in saveObjects)
        {
            Data.UserCharacters.Add(saveObject);

            Character character = new Character();
            character.UnpackData(saveObject);
            Debug.Log(character.name + " loaded.");
        }
    }


    // A serializable data file containing only character fields. 
    [System.Serializable]
    public class CharBasicData : IJsonable, IBinarySaveObject, IEquatable<CharBasicData>
    {
        // Marks as IJsonable, but is not used.
        string IJsonable.FileName { get; } = "SomeChar.json";

        // Implements IBinarySaveObject template
        string IBinarySaveObject.GetFileName() => 
            saveSlot + " " + name + Constants.Character.FileExtension;

        // Implements IEquatable (for removing from list)
        public bool Equals(CharBasicData other) => other.saveSlot.Equals(saveSlot);


        public string name;
        public Gender gender;
        public string mwClassKey;
        public RaceName race;
        public SignName sign;
        public DateTime saveTime;
        public int saveSlot = 1;



        /*IBinarySaveObject IBinarySaveObject.PackData(object container)
        {
            if (!(container is Character character))
                return null;
            
            return new CharBasicData
            {
                name = character.name,
                gender = character.gender,
                mwClassKey = character.mwClassKey,
                race = character.race,
                sign = character.sign,
                saveTime = DateTime.Now
            };
        }*/

        /*void IBinarySaveObject.UnpackData(IBinarySaveObject so,
            ref object container)
        {
            if (!(so is CharBasicData data)
                || !(container is Character character)) return;
            
            character.name = data.name;
            character.gender = data.gender;
            character.mwClassKey = data.mwClassKey;
            character.race = data.race;
            character.sign = data.sign;
            character.saveTime = data.saveTime;
        }*/
    }

    public CharBasicData PackData(int slot)
    {
        CharBasicData data = new CharBasicData
        {
            name = name,
            gender = gender,
            mwClassKey = mwClassKey,
            race = race,
            sign = sign,
            saveTime = DateTime.Now,
            saveSlot = slot
        };
        this.saveSlot = data.saveSlot;
        this.saveTime = data.saveTime;
        return data;
    }

    public void UnpackData(CharBasicData data)
    {
        name = data.name;
        gender = data.gender;
        mwClassKey = data.mwClassKey;
        race = data.race;
        sign = data.sign;
        saveTime = data.saveTime;
        saveSlot = data.saveSlot;
    }

}

