using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RaceBox : MonoBehaviour
{
    [SerializeField]
    TMP_Dropdown raceDropdown = null;

    [SerializeField]
    AttrBox attrBox = null;

    [SerializeField]
    SkillBox skillBox = null;

    [SerializeField]
    FeatureBox featureBox = null;

    [SerializeField]
    TMP_Text errorMessage = null;

    Character character;

    public void PopulateRaceBox(Character character)
    {
        this.character = character;
        raceDropdown.ClearOptions();
        raceDropdown.AddOptions(new List<string> { RaceName.none.ToString() });
        raceDropdown.AddOptions(Data.Races.Keys.Select(x => x.ToString()).ToList());

        int AttrGetter(AttrName attrName, bool ignoreValue)
            => Data.Races[character.Race.Name].GetAttrValue(attrName, character.Gender);

        attrBox.PopulateAttrBox(
            new AttrBox.AttrGetter(AttrGetter));

        // skillBox populates on Refresh
        // featureBox populates on Refresh
        
        
    }


    public void RefreshRaceBox()
    {
        int value = raceDropdown.value;
        RaceName race = (RaceName)value;

        if (value == (int)RaceName.none
            || character.Gender == Gender.none)
        {
            character.SetRace(RaceName.none);
            if (character.Gender == Gender.none)
            {
                errorMessage.text = "Specify Gender";
                errorMessage.gameObject.SetActive(true);
            }
            else
            {
                errorMessage.text = "";
                errorMessage.gameObject.SetActive(false);
            }
            
            attrBox.gameObject.SetActive(false);
            skillBox.gameObject.SetActive(false);
            featureBox.gameObject.SetActive(false);
        }
        else
        {
            character.SetRace(race);

            errorMessage.gameObject.SetActive(false);

            attrBox.gameObject.SetActive(true);
            attrBox.RefreshAttrBox();

            skillBox.gameObject.SetActive(true); 
            skillBox.PopulateRaceSkillBox(character.Race.SkillBonus);
            skillBox.RefreshSkillBox();

            featureBox.RefreshRaceFeatureBox(character);
            featureBox.gameObject.SetActive(true);

            errorMessage.text = "";
                //= Data.Races[race].GetAttrAsString(character.Gender);*/
            /*raceDetailsText2.text
                = Data.Races[race].GetSkillBonusAsString();
            raceDetailsText3.text
                = Data.Races[race].GetOtherFeaturesAsString(character.Gender);*/
        }
        
    }

}
