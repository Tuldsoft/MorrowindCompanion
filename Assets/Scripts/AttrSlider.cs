using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AttrSlider : MonoBehaviour
{

    //[SerializeField]
    //GameObject fillObject = null; 
    
    // TODO: Fill area can change to a percentage of full health, but this is a character
    //   generator, and so we only ever have 100% health.

    [SerializeField]
    TMP_Text valueText = null;

    public void SetValue(float value, float maxValue = 0)
    {
        if (maxValue == 0) 
            maxValue = value;

        valueText.text = (int)value + " / " + (int)maxValue;

        /*Debug.Log("Width of fillarea is "
            + fillObject.transform.parent.GetComponent<RectTransform>().sizeDelta.x);
        Debug.Log("Width of fill is " 
            + fillObject.GetComponent<RectTransform>().sizeDelta.x);*/
    }


}
