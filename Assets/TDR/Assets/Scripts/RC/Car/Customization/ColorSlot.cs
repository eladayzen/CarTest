// Description: Attached to UI button to change the vehicle customization color part. 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TS.Generics
{
    public class ColorSlot : MonoBehaviour
    {
        public int      Index = 0;
        public Image    Img;

        public void ChangeColor()
        {
            #region
            //Debug.Log("Change the Color");
            CustomizationManager customizationManager = FindFirstObjectByType<CustomizationManager>();
            customizationManager.UpdateColor(Index);
            #endregion
        }
    }

}
