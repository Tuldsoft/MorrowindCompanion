using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// Containers of objects, filled by Initialization. Features additionally
//   created via the SpellCrafter.

public static class Data
{
    public static Dictionary<AttrName, Attribute> Attributes { get; set; }
        = new Dictionary<AttrName, Attribute>();

    public static Dictionary<SkillName, Skill> Skills { get; set; }
        = new Dictionary<SkillName, Skill>();

    public static Dictionary<RaceName, Race> Races { get; set; }
        = new Dictionary<RaceName, Race>();

    public static Dictionary<SignName, Sign> Signs { get; set; }
        = new Dictionary<SignName, Sign>();

    public static Dictionary<string, MWClass> Classes { get; set; }
        = new Dictionary<string, MWClass>();

    public static List<string> UserClassKeys { get; set; }
        = new List<string>();

    public static Dictionary<string, SpellEffect> SpellEffects { get; set; }
            = new Dictionary<string, SpellEffect>();

    public static List<Character.CharBasicData> UserCharacters { get; set; }
        = new List<Character.CharBasicData>();

    public static class Sprites
    {
        public static Dictionary<AttrName, Sprite> AttrSprites { get; set; }
            = new Dictionary<AttrName, Sprite>();

        public static Dictionary<SkillName, Sprite> SkillSprites { get; set; }
            = new Dictionary<SkillName, Sprite>();

        public static Dictionary<string, Sprite> ClassSprites { get; set; }
            = new Dictionary<string, Sprite>();
        
        public static Dictionary<SignName, Sprite> SignSprites { get; set; }
            = new Dictionary<SignName, Sprite>();

        public static Dictionary<string, Sprite> SpellEffectSprites { get; set; }
            = new Dictionary<string, Sprite>();
    }

    public static class Prefabs
    {
        public static GameObject EffectTemplate { get; set; } = null;
        public static GameObject ViewFeaturesMenu { get; set; } = null;
        public static GameObject ViewFeaturesPanel { get; set; } = null;
        public static GameObject ManageClassesMenu { get; set; } = null;
        public static GameObject SaveLoadCharMenu { get; set; } = null;
    }

    // Index-matched lists, private set to control add/remove
    public static List<Feature> Features { get; private set; }
        = new List<Feature>();
    public static List<FeatureData> FeatureDatas { get; private set; }
        = new List<FeatureData>(); // yes, I know Data is already plural.



    // Methods for Features. Used by SpellCrafter, not by Initializer.
    public static void AddFeature(Feature feature, bool writeJson = true)
    {
        // Feature has an embedded FeatureData. FeatureData needs storage info
        //   prior to this method, otherwise defaults are assumed.

        // Must have valid, unique name, and legitimate effects
        if (!feature.IsValid) return;

        // Prep feature for json
        var fData = (FeatureData)feature.GetJsonable();

        // Add Feature to Data.Features
        Features.Add(feature);

        // Add FeatureData to list
        FeatureDatas.Add(fData);

        // Add to Data.Races or Data.Signs
        if (fData.fFile == FeatureFile.Racial)
            Races[(RaceName)fData.qualifier].Features.Add(feature);
        else if (fData.fFile == FeatureFile.Signed)
            Signs[(SignName)fData.qualifier].Features.Add(feature);

        // Rewrite the Json of all features stored in Data
        if (writeJson)
            FileUtil.WriteJson(FeatureDatas, fData.FileName);

    }

    public static Feature GetSpell(string name)
        => Features.Find(x => x.Name == name && x.FFile == FeatureFile.Spells);

       
}
