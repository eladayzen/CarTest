using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TS.Generics
{
    public class CanvasDamage : MonoBehaviour
    {
        public int                  PlayerID = 0;

        [System.Serializable]
        public class DamageParams
        {
            public string   Name;
            public Image    Im;
            public Text     Txt;
            public Gradient ColorGradient;
        }

        public List<DamageParams>   DamageList = new List<DamageParams>();


        public void UpdateValue(int index,int value)
        {
            #region 
            DamageList[index].Im.color = DamageList[index].ColorGradient.Evaluate(1 - value * .01f);
            DamageList[index].Txt.text = value.ToString(); 
            #endregion
        }
    }
}

