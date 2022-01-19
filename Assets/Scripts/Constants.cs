using System;

// Global constants to protect against typos
public class Constants
{
    // Constants
    public const int MaxNumLevels = 100;
    public const int MaxSkillLevel = 100;
    public const int MaxAttrLevel = 100;
    
    public class FileName
    {
        public const string Skills = "Skills.csv";
        //public const string Attributes = "Attributes.csv";
        public const string Races = "Races.csv";
        public const string Signs = "Signs.csv";
        public const string Classes = "Classes.csv";
        public const string SpellEffects = "SpellEffects.csv";
        public const string Features = "Features.json";

    }

    public static int AttrCount => Enum.GetValues(typeof(AttrName)).Length - 1;
    public static int SkillCount => Enum.GetValues(typeof(SkillName)).Length - 1;
    public static int SignCount => Enum.GetValues(typeof(SignName)).Length - 1;

    public class Effect
    {
        public const int MaxMagnitude = 500;
        public const int MaxDuration = 300;
        public const int MaxArea = 50;
    }

    public class Character
    {
        public const string FileExtension = ".chr";
    }


    public class Class
    {
        public const string CustomKey = "custom";
        public const string NoneKey = "none";
        public const string FileExtension = ".cls";
    }

}
