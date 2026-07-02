// Description: InfoVehicle: Access from anywhere info about vehicles (unlocked vehicles, P1 P2 selected vehicle ID, Vehicles parameters).
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace TS.Generics
{
    public class InfoVehicle : MonoBehaviour
    {
        public static InfoVehicle       instance = null;
        public List<bool>               listVehicleUnlockState  = new List<bool>();     // Remember if a vehicle is unlocked is unlocked.
        public int                      currentVehicleDisplayedInTheGarage = 0;
        public List<int>                listSelectedVehicles    = new List<int>();      // Remember selected vehicles.

        public List<int>                listSelectedVehiclesInCategory = new List<int>();      // Remember last selected vehicles in a category.

        public List<VehicleGlobalData.CarParameters> vehicleParametersInGameList = new List<VehicleGlobalData.CarParameters>();

        [System.Serializable]
        public class VehicleListByCategory
        {
            public List<int> vehicleList = new List<int>();
        }

        public List<VehicleListByCategory> vehicleListByCategory = new List<VehicleListByCategory>();


            void Awake()
        {
            #region Create ony one instance of the gameObject in the Hierarchy
            //Check if instance already exists
            if (instance == null)
                //if not, set instance to this
                instance = this;
            else if (instance != this)
                Destroy(gameObject);
            #endregion
        }

        public void Init(string sData)
        {
            #region 
            //-> Init vehicleParametersInGameList
            for (var i = 0; i < DataRef.instance.vehicleGlobalData.carParametersList.Count; i++)
                vehicleParametersInGameList.Add(new VehicleGlobalData.CarParameters(DataRef.instance.vehicleGlobalData.carParametersList[i]));

            for (var i = 0; i < DataRef.instance.vehicleGlobalData.VehicleCategoryParamsList.Count; i++)
            {
                listSelectedVehiclesInCategory.Add(0);
                vehicleListByCategory.Add(new VehicleListByCategory());
            }
                

            // Add one more for All vehicles category
            listSelectedVehiclesInCategory.Add(0);
            vehicleListByCategory.Add(new VehicleListByCategory());

            //Debug.Log("ini: ''" + sData + "''");
            if (sData == "")
            {

            }
            else
            {
                string[] codes = sData.Split('_');
                int counter = 0;
                int howManyEntries = int.Parse(codes[counter]);
                counter++;
                //-> Update unlock State
                for (var i = 0; i < howManyEntries; i++)
                {
                    //Debug.Log(i + ": " + codes[counter]);
                    vehicleParametersInGameList[i].isUnlocked = TrueFalse(codes[counter]);
                    counter++;
                }

                howManyEntries = int.Parse(codes[counter]);

                counter++;
                //Debug.Log("howManyEntries: " + howManyEntries + " | Counter: " + counter + " | codes.Lenght: " + codes.Length);
                //-> Update show in vehicle selection.
                for (var i = 0; i < howManyEntries; i++)
                {
                    Debug.Log(i + ": " + codes[counter]);
                    vehicleParametersInGameList[i].bShow = TrueFalse(codes[counter]);
                    counter++;
                }
            }


            StartCoroutine(VehicleListByCategoryRoutine());
          

            #endregion
        }

        IEnumerator VehicleListByCategoryRoutine()
        {
            int howManyCategory = DataRef.instance.vehicleGlobalData.VehicleCategoryParamsList.Count;
            yield return new WaitUntil (()=> vehicleListByCategory.Count == howManyCategory+1);

            //yield return new WaitForSeconds(5);
            //-> Create vehicle List by Category
            for (var i = 0; i < vehicleParametersInGameList.Count; i++)
            {
                int vehicleCategory = vehicleParametersInGameList[i].vehicleCategory;
                vehicleListByCategory[vehicleCategory].vehicleList.Add(i);
                vehicleListByCategory[vehicleListByCategory.Count - 1].vehicleList.Add(i);
            }

            yield return null;
        }

        bool TrueFalse(string value)
        {
            #region 
            if (value == "T") return true;
            else return false; 
            #endregion
        }
    }
}

