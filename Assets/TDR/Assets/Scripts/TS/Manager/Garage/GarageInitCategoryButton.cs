using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class GarageInitCategoryButton : MonoBehaviour
    {
        public int category = -1;
        public CurrentText currentText;
        public VehicleGlobalData vehicleData;


        void Start()
        {
            if (currentText && category != -1)
            {
                int listID = DataRef.instance.vehicleGlobalData.VehicleCategoryParamsList[category].ListID;
                int entryID = DataRef.instance.vehicleGlobalData.VehicleCategoryParamsList[category].EntryID;
               
                currentText.tab = listID;
                currentText._Entry = entryID;
                string txt = LanguageManager.instance.String_ReturnText(listID, entryID);
                currentText.DisplayTextComponent(currentText.gameObject, txt);
            }
        }


    }

}
