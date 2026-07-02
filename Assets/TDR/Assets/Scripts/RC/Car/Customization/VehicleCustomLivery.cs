// Description: Attached inside a vehicle to a customization part.
// Allows to setup car livery and price for vehicle custom part.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TS.Generics
{
    public class VehicleCustomLivery : MonoBehaviour, IVehicleCustomization
    {
        public int CurrentSelection = 0;
        int currentSelectedSection = 0;
        int currentVehicleIDCategory = 0;

        [System.Serializable]
        public class Params
        {

            //public Color MatColor = Color.white;
            //public Texture2D liveryMask;
            public Texture2D liveryTexture;
            public int Price = 500;
            public Sprite Img;
        }

        public List<Params> liveryList = new List<Params>();


        [System.Serializable]
        public class Part
        {
            public Transform obj;
            public int matID = 0;
        }

        public List<Part> ObjList = new List<Part>();

        public UnityEvent ActionCustomizationProcess;

        public enum AICustomType { Default, Random, Modulo, CustomList };
        public AICustomType aiCustomType = AICustomType.Default;
        public List<int> aiCustomList = new List<int>();

        public void CustomizationProcess()
        {
            #region 
            //Debug.Log(Txt);
            if (ActionCustomizationProcess != null)
                ActionCustomizationProcess.Invoke();
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
            return liveryList[CurrentSelection].Img;
            #endregion
        }

        public List<int> GetPrice()
        {
            #region 
            List<int> priceList = new List<int>();
            for (var i = 0; i < liveryList.Count; i++)
                priceList.Add(liveryList[i].Price);

            return priceList;
            #endregion
        }
        public int GetItemsNumber()
        {
            #region 
            return liveryList.Count;
            #endregion
        }
        public void InitCustomPerformance()
        {
            #region 
            // Nothing to do for Livery
            #endregion
        }

        public void TxtTest()
        {
            #region 
            // Debug.Log(Txt); 
            #endregion
        }

        public void ChangeTheMaterialColor(int materialIndex)
        {
            #region 
            // Nothing to do for Livery
            #endregion
        }

        public void AICustomization(int i)
        {
            #region 
            int index = 0;
            switch (aiCustomType)
            {
                case AICustomType.Default:
                    index = 0;
                    break;
                case AICustomType.Random:
                    index = UnityEngine.Random.Range(0, liveryList.Count);
                    break;
                case AICustomType.Modulo:
                    index = CurrentSelection;
                    index++;
                    index *= -1;
                    var customizationVehicleList = InfoVehicleCustomization.instance.CustomizationVehicleList;
                    int offset = customizationVehicleList[currentVehicleIDCategory].Vehicle[currentSelectedSection].CurrentSelection;
                    index += offset;
                    index %= liveryList.Count;

                    /*if(index == offset)
                    {
                        List<int> liveryTempList = new List<int>();

                        for(var j = 0;j< liveryList.Count;j++)
                        {
                            if (j != offset)
                                liveryTempList.Add(j);
                        }

                        var rand = UnityEngine.Random.Range(0, liveryTempList.Count);
                        index = liveryTempList[rand];
                    }*/


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
            ApplyNewLivery(index,i);
            #endregion
        }


        public void ChangeCarPaintingMaskAndLivery()
        {
            #region 
          
            for (var i = 0; i < ObjList.Count; i++)
            {
                if (ObjList[i].obj != null)
                {
                    //Debug.Log("Change Livery");
                    if (CurrentSelection < 0)
                        AICustomization(i);
                    else
                        ApplyNewLivery(CurrentSelection,i);
                }
            }
            #endregion
        }

        public void ApplyNewLivery(int liveryIndex,int i)
        {
            /*if (liveryList[liveryIndex].liveryMask != null)
                ObjList[0].GetComponent<Renderer>().materials[materialIndex].SetTexture("_LiveryMask", liveryList[liveryIndex].liveryMask);
            else
                ObjList[0].GetComponent<Renderer>().materials[materialIndex].SetTexture("_LiveryMask", null);
            */
            if (liveryList[liveryIndex].liveryTexture != null)
                ObjList[i].obj.GetComponent<Renderer>().materials[ObjList[i].matID].mainTexture = liveryList[liveryIndex].liveryTexture;// SetTexture("_LiveryTexture", liveryList[liveryIndex].liveryTexture);
            else
                ObjList[i].obj.GetComponent<Renderer>().materials[ObjList[i].matID].mainTexture = null;//.SetTexture("_LiveryTexture", null);
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

