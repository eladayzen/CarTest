// Description: Attached inside a vehicle to a customization part.
// Allows to setup a 3D model and a price for vehicle custom part.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TS.Generics
{
    public class VehicleCustomModels : MonoBehaviour, IVehicleCustomization
    {
        public int                  CurrentSelection = 0;
        int currentSelectedSection = 0;
        int currentVehicleIDCategory = 0;

        public Vector3              prefabScale = Vector3.one;

        [System.Serializable]
        public class Params
        {
            public GameObject   Obj;
            public int          Price = 500;
            public Sprite       Img;
        }

        public List<Params>         ModelsList = new List<Params>();

        public List<Transform>      PivotsList   = new List<Transform>();

        public enum AICustomType    { Default, Random, Modulo, CustomList };
        public AICustomType         aiCustomType = AICustomType.Default;
        public List<int>            aiCustomList = new List<int>();

        public void CustomizationProcess()
        {
            #region 
            for (var i = 0; i < PivotsList.Count; i++)
            {
                List<GameObject> objsToDestroy = new List<GameObject>();
                for (var j = 0; j < PivotsList[i].childCount; j++)
                    objsToDestroy.Add(PivotsList[i].GetChild(j).gameObject);

                foreach (GameObject obj in objsToDestroy)
                    Destroy(obj);

                if (CurrentSelection < 0)
                    AICustomization(i);
                else if (CurrentSelection >= 0 && ModelsList[CurrentSelection] != null)
                {
                    GameObject obj = Instantiate(ModelsList[CurrentSelection].Obj, PivotsList[i].position, PivotsList[i].rotation, PivotsList[i]);
                    obj.transform.localScale = prefabScale;
                }
                   
            } 
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
            return ModelsList[CurrentSelection].Img; 
            #endregion
        }

        public List<int> GetPrice()
        {
            #region 
            List<int> priceList = new List<int>();
            for (var i = 0; i < ModelsList.Count; i++)
                priceList.Add(ModelsList[i].Price);

            return priceList; 
            #endregion
        }

        public int GetItemsNumber()
        {
            #region 
            return ModelsList.Count; 
            #endregion
        }
        public void InitCustomPerformance()
        {
            #region 
            //if (ActionInitCustomPerformance != null)
            //    ActionInitCustomPerformance.Invoke(); 
            #endregion
        }

        public void TxtTest()
        {
            #region
            //Debug.Log(Txt); 
            #endregion
        }

        public void AICustomization(int id)
        {
            #region 
            int index = 0;
            switch (aiCustomType)
            {
                case AICustomType.Default:
                    index = 0;
                    break;
                case AICustomType.Random:
                    index = UnityEngine.Random.Range(0, ModelsList.Count);
                    break;
                case AICustomType.Modulo:
                    index = CurrentSelection;
                    index++;
                    index *= -1;

                    var customizationVehicleList = InfoVehicleCustomization.instance.CustomizationVehicleList;
                    int offset = customizationVehicleList[currentVehicleIDCategory].Vehicle[currentSelectedSection].CurrentSelection;
                    index += offset;
                    index %= ModelsList.Count;

                    break;
                case AICustomType.CustomList:
                    index = CurrentSelection;
                    index++;
                    index *= -1;


                    if (aiCustomList.Count == 0)
                        index = 0;
                    else
                    {
                        index %= aiCustomList.Count;
                        index = aiCustomList[index];
                    }

                    break;
            }

            GameObject obj = Instantiate(ModelsList[index].Obj, PivotsList[id].position, PivotsList[id].rotation, PivotsList[id]);
            obj.transform.localScale = prefabScale;
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
