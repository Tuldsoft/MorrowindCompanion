using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class SignBox : MonoBehaviour
{
    [SerializeField]
    TMP_Dropdown signDropdown = null;

    [SerializeField]
    FeatureBox featureBox = null;

    Character character;

    public void PopulateSignBox(Character character)
    {
        this.character = character;

        signDropdown.ClearOptions();
        signDropdown.AddOptions(new List<string> { SignName.none.ToString() });
        signDropdown.AddOptions(Data.Signs.Keys
            .Select(x => Data.Signs[x].DisplayName).ToList());
    }

    public void RefreshSignBox()
    {
        SignName sign = (SignName)signDropdown.value;
        character.SetSign(sign);

        featureBox.RefreshSignFeatureBox(character);
    }


}
