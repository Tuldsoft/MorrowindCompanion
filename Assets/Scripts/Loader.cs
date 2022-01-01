using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Caches references to resources into Data
public static class Loader
{
    static bool isInitialized = false;

    public static void Initialize()
    {
        if (isInitialized) return;
        isInitialized = true;

        LoadSprites();
        LoadPrefabs();
    }

    static void LoadSprites()
    {
        for (int i = 1; i <= Constants.AttrCount; i++)
        {
            AttrName attr = (AttrName)i;
            string path = @"Icons\MW-icon-attribute-" + attr.ToString();
            Sprite sprite = Resources.Load<Sprite>(path);
            if (sprite == null)
                Debug.LogError("error loading " + attr.ToString());
            Data.Sprites.AttrSprites.Add(attr, sprite);
        }

        for (int i=1; i <= Constants.SkillCount; i++)
        {
            SkillName skill = (SkillName)i;
            string path = @"Icons\MW-icon-skill-" + skill.ToString();
            Sprite sprite = Resources.Load<Sprite>(path);
            if (sprite == null)
                Debug.LogError("error loading " + skill.ToString());
            Data.Sprites.SkillSprites.Add(skill, sprite);
        }

        foreach (string mwClass in Data.Classes.Keys)
        {
            string path = @"Icons\MW-class-" + mwClass;
            Sprite sprite = Resources.Load<Sprite>(path);
            if (sprite == null)
                Debug.LogError("error loading " + mwClass);
            Data.Sprites.ClassSprites.Add(mwClass, sprite);
        }

        for (int i = 1; i <= Constants.SignCount; i++)
        {
            SignName sign = (SignName)i;
            string path = @"Icons\MW-birthsign-" + sign.ToString();
            Sprite sprite = Resources.Load<Sprite>(path);
            if (sprite == null)
                Debug.LogError("error loading " + sign.ToString());
            Data.Sprites.SignSprites.Add(sign, sprite);
        }

        foreach (var spellEffect in Data.SpellEffects.Values)
        {
            string path = @"Icons\MW-icon-effect-" + spellEffect.SystemName;
            Sprite sprite = Resources.Load<Sprite>(path);
            if (sprite == null)
                Debug.LogError("error loading " + spellEffect.SystemName);
            Data.Sprites.SpellEffectSprites.Add(spellEffect.Name, sprite);
        }

        //Debug.Log(Data.Sprites.SpellEffectSprites.Count + " SE sprites loaded");
    }

    static void LoadPrefabs()
    {
        Data.Prefabs.EffectTemplate = Resources.Load<GameObject>(
            @"Prefabs\EffectTemplate");
        Data.Prefabs.ViewFeaturesMenu = Resources.Load<GameObject>(
            @"Prefabs\ViewFeaturesMenu");
        Data.Prefabs.ViewFeaturesPanel = Resources.Load<GameObject>(
            @"Prefabs\ViewFeaturesPanel");
    }

}
