using UnityEngine;
using UnityEngine.UI;
using TMPro;

/*
* @Arthur Lacour
* @TextSliderUpdater.cs
* @15/03/2018
* @Description:
*   - Fonction public qui actualise un Text en fonction d'un slider.
*       (Hard codé pour la Metallurgy !)
* 
* @Condition:
*   - S'accroche sur un Slider.
*   - Avoir un text en parallèle.
*/

public class TextSliderUpdater : MonoBehaviour {

    [SerializeField] TextMeshProUGUI text;
    [SerializeField] Slider slider;

    public void UpdateText()
    {
        UIManager.Instance.MetalRef.numOrder = (int)slider.value;
        text.text = slider.value.ToString();
    }
}
