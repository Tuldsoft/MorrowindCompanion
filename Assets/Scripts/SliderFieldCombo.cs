using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderFieldCombo : MonoBehaviour
{
    [SerializeField]
    TMP_InputField inputField = null;

    [SerializeField]
    Slider slider = null;

    [SerializeField]
    Slider linkedSlider = null;

    float originalMax;

    public void SetSliderMax(int max)
    {
        slider.maxValue = max;
        originalMax = max;
    }

    public void OnSliderValueChange(float value)
    {
        inputField.text = value.ToString();
    }

    public void OnFieldValueChange(string value)
    {
        if (int.TryParse(value, out int intValue))
        {
            if (intValue > slider.value)
            {
                slider.interactable = false;
                slider.maxValue = intValue;
                if (linkedSlider != null)
                {
                    linkedSlider.interactable = false; 
                    linkedSlider.maxValue = intValue;
                }
            }
            if (intValue < originalMax)
            {
                slider.interactable = true;
                slider.maxValue = originalMax;
                if (linkedSlider != null)
                {
                    linkedSlider.interactable = true;
                    linkedSlider.maxValue = originalMax;
                }
            }
            slider.value = Mathf.Clamp(intValue, slider.minValue, slider.maxValue);
        }
        else
            inputField.text = slider.value.ToString();
    }

}
