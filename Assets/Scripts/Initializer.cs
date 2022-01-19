using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Called by evey scene monitor. Includes safeguards to only be called once.
//   Redirects to statically initialize other classes.
public static class Initializer 
{
    static bool isInitialized = false;

    public static void Initialize()
    {
        if (isInitialized) return;
        isInitialized = true;

        Skill.Initialize();       // populates Data.Skills from csv
        Race.Initialize();        // populates Data.Races from csv
        MWClass.Initialize();     // populates Data.Classes from csv
                                  //   and Data.UserClasses from serial files
        Sign.Initialize();        // populates Data.Signs from csv
        SpellEffect.Initialize(); // populates Data.SpellEffects from csv
        Feature.Initialize();     // populates Data.Features from json
        Loader.Initialize();      // populates Data.Sprites and Data.Prefabs
        Character.Initialize();   // populates Data.UserCharacters from serial files
    }

}
