//Desciption: ButtonFxManager.cs. Methods to change Text size
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;


namespace TS.Generics
{
    public class ButtonFxManager : MonoBehaviour
    { 
        public UnityEvent       OnPointerEnterEvents;
        public UnityEvent       OnPointerExitEvents;

        public void TypoSize0(bool b_Enable = true)
        {
            #region
            if (b_Enable) transform.GetChild(0).GetComponent<Text>().fontSize = 20; 
            #endregion
        }

        public void TypoSize1(bool b_Enable = true)
        {
            #region 
            if (b_Enable) transform.GetChild(0).GetComponent<Text>().fontSize = 14; 
            #endregion
        }

    }

}
