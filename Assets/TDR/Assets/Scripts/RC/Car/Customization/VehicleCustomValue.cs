// Description: Attached inside a vehicle to a customization part.
// Allows to setup performance value and price.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TS.Generics
{
    public class VehicleCustomValue : MonoBehaviour, IVehicleCustomization
    {
        public int              CurrentSelection = 0;
        int currentSelectedSection = 0;
        int currentVehicleIDCategory = 0;

        [System.Serializable]
        public class Params
        {
            public float        percentage = 0;
            public int          Price = 500;
        }

        public List<Params>     ValueList = new List<Params>();

        public UnityEvent       ActionInitCustomPerformance;

        public string           Txt = "Test 01";

        public void CustomizationProcess()
        {
            #region 
            //Debug.Log("CustomizationProcess");  
            #endregion
        }

        public int GetCurrentSelection()
        {
            #region 
            return CurrentSelection; 
            #endregion
        }

        public void SetCurrentSelection(int value)
        {
            #region 
            CurrentSelection = value; 
            #endregion
        }

        public Sprite GetSprite()
        {
            #region 
            return null; 
            #endregion
        }

        public List<int> GetPrice()
        {
            #region 
            List<int> priceList = new List<int>();
            for (var i = 0; i < ValueList.Count; i++)
                priceList.Add(ValueList[i].Price);

            return priceList; 
            #endregion
        }
        public int GetItemsNumber()
        {
            #region 
            return ValueList.Count; 
            #endregion
        }

        public void InitCustomPerformance()
        {
            #region 
            if (ActionInitCustomPerformance != null)
                ActionInitCustomPerformance.Invoke(); 
            #endregion
        }

        public void TxtTest()
        {
            #region 
            Debug.Log(Txt); 
            #endregion
        }

        public void UpdateCarControllerMaxSpeed()
        {
            #region 
            CarController carController = transform.parent.parent.GetComponent<VehiclePrefabInit>().vehicleInfo.GetComponent<CarController>();

            float percentage = 0;

            for (var i = 0; i < CurrentSelection; i++)
                percentage += ValueList[i].percentage;

            carController.VehicleCustomizationUpdateMaxSpeed(percentage); 
            #endregion
        }

        public void UpdateCarControllerAcceleration()
        {
            #region 
            CarController carController = transform.parent.parent.GetComponent<VehiclePrefabInit>().vehicleInfo.GetComponent<CarController>();

            float percentage = 0;

            for (var i = 0; i < CurrentSelection; i++)
                percentage += ValueList[i].percentage;

            carController.VehicleCustomizationUpdateAcceleration(percentage); 
            #endregion
        }

        public void UpdateCarControllerBrakeForce()
        {
            #region 
            CarController carController = transform.parent.parent.GetComponent<VehiclePrefabInit>().vehicleInfo.GetComponent<CarController>();

            float percentage = 0;

            for (var i = 0; i < CurrentSelection; i++)
                percentage += ValueList[i].percentage;

            carController.VehicleCustomizationUpdateBrakeForce(percentage); 
            #endregion
        }

        public void SetIndexSection(int indexSection)
        {
            currentSelectedSection = indexSection;
        }

        public void SetVehicleIDCategory(int vehicleIDCategory)
        {
            currentVehicleIDCategory = vehicleIDCategory;
        }
    }

}
