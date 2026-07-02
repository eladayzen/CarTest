// Description: Attached to InfoVehicles. Load and save info about vehicles customization.
// Contains info about vehicles customization. Know if an item is unlocked.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class InfoVehicleCustomization : MonoBehaviour
    {
        public static InfoVehicleCustomization  instance = null;

        public bool                             AutoInit = false;
        public bool                             isInitDone = false;

        [System.Serializable]
        public class Item
        {
            public bool         IsUnlocked;

            public Item(bool c)
            {
                IsUnlocked = c;
            }
        }

        [System.Serializable]
        public class Section
        {
            public string       Name = "";
            public int          TextList = 0;
            public int          TextID = 0;

            public OrbitalState orbitalState = OrbitalState.KeepPosition;
            public Vector3      newCamPosition;


            public int          CurrentSelection;
            public int          TemporarySelection = -1;
            public List<Item>   itemList = new List<Item>();
        }

        [System.Serializable]
        public class CustomizationParams
        {
            public string       Name = "";
            public List<Section>Vehicle = new List<Section>();
        }

        public List<CustomizationParams>        CustomizationVehicleList = new List<CustomizationParams>();

        void Awake()
        {
            #region Create only one instance of the gameObject in the Hierarchy
            if (instance == null)
                instance = this;
            else if (instance != this)
                Destroy(gameObject);
            #endregion
        }


        void Start()
        {
            #region
            if(AutoInit)
                StartCoroutine( InitRoutine());
            #endregion
        }

        public void Init(string sData)
        {
            #region
            if(InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode != 5)
                StartCoroutine(InitRoutine(sData));
            #endregion
        }

        IEnumerator InitRoutine(string sData = "")
        {
            #region
            isInitDone = false;
            CustomizationVehicleList.Clear();
            // Create vehicle List
            for (var i = 0; i < DataRef.instance.vehicleGlobalData.carParametersList.Count; i++)
            {
                CustomizationVehicleList.Add(new CustomizationParams());
            }

            // Find the name of the car
            for (var i = 0; i < DataRef.instance.vehicleGlobalData.carParametersList.Count; i++)
            {
                CustomizationVehicleList[i].Name = DataRef.instance.vehicleGlobalData.carParametersList[i].name;
            }

            // Create Customization Parameters
            for (var i = 0; i < DataRef.instance.vehicleGlobalData.carParametersList.Count; i++)
            {
                GameObject vehicle = DataRef.instance.vehicleGlobalData.carParametersList[i].Prefab;
                if (vehicle)
                {
                    VehicleCustomization vehicleCustom = vehicle.GetComponentInChildren<VehicleCustomization>();

                    for (var j = 0; j < vehicleCustom.CustomSectionList.Count; j++)
                    {
                        CustomizationVehicleList[i].Vehicle.Add(new Section());
                    }
                }
            }
            // Get the default parameters
            for (var i = 0; i < DataRef.instance.vehicleGlobalData.carParametersList.Count; i++)
            {
                GameObject vehicle = DataRef.instance.vehicleGlobalData.carParametersList[i].Prefab;
                if (vehicle)
                {
                    VehicleCustomization vehicleCustom = vehicle.GetComponentInChildren<VehicleCustomization>();

                    for (var j = 0; j < vehicleCustom.CustomSectionList.Count; j++)
                    {
                        CustomizationVehicleList[i].Vehicle[j].Name = vehicleCustom.CustomSectionList[j].Name;

                        CustomizationVehicleList[i].Vehicle[j].TextList = vehicleCustom.CustomSectionList[j].TextList;
                        CustomizationVehicleList[i].Vehicle[j].TextID = vehicleCustom.CustomSectionList[j].TextID;

                        CustomizationVehicleList[i].Vehicle[j].orbitalState = vehicleCustom.CustomSectionList[j].orbitalState;
                        CustomizationVehicleList[i].Vehicle[j].newCamPosition = vehicleCustom.CustomSectionList[j].newCamPosition;


                        IVehicleCustomization selec = vehicleCustom.CustomSectionList[j].ObjSection.GetComponent<IVehicleCustomization>();
                        // Get current selection
                        CustomizationVehicleList[i].Vehicle[j].CurrentSelection = selec.GetCurrentSelection();


                        // Get Items Unlock state
                        List<int> priceList = selec.GetPrice();
                       // Debug.Log("priceList: " + priceList.Count);
                        for (var k = 0; k < priceList.Count; k++)
                        {
                            int price = priceList[k];
                            bool isUnlocked = true ? price == 0 : false;
                            //Debug.Log(k + " |price: " + price);

                            CustomizationVehicleList[i].Vehicle[j].itemList.Add(new Item(isUnlocked/*,price*/));
                        }
                    }

                }
            }


            //yield return new WaitForSeconds(2);
            if (sData != "")
                yield return new WaitUntil(() => LoadData(sData) == true);

            isInitDone = true;
            yield return null;
            #endregion
        }

        bool LoadData(string sData)
        {
            #region
            string[] codes = sData.Split('_');
            int counter = 0;

            int howManyVehicle = int.Parse(codes[0]);
            counter++;

            //Debug.Log("howManyVehicle: " + howManyVehicle);

            for (var i = 0; i < howManyVehicle; i++)
            {

                 //if (i < codes.Length)
                 //    break;

                CustomizationParams vehicleParams = CustomizationVehicleList[i];
                int howManyParams = int.Parse(codes[counter]);  // 1
                counter++;

                for (var j = 0; j < howManyParams; j++)
                {
                    int howManyitems = int.Parse(codes[counter]);  // 2
                    counter++;

                    int currentSelection = int.Parse(codes[counter]);
                    vehicleParams.Vehicle[j].CurrentSelection = currentSelection;
                    if(vehicleParams.Vehicle[j].TemporarySelection != -1)
                        vehicleParams.Vehicle[j].TemporarySelection = currentSelection;
                    counter++;

                    for (var k = 0; k < howManyitems; k++)
                    {
                        vehicleParams.Vehicle[j].itemList[k].IsUnlocked = TrueFalse(codes[counter]);
                        counter++;
                    }
                }
            }

            return true;
            #endregion
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
