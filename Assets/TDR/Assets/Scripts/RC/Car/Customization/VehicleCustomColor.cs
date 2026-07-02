// Description: Attached inside a vehicle to a customization part.
// Allows to setup color and price for vehicle custom part.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TS.Generics
{
    public class VehicleCustomColor : MonoBehaviour, IVehicleCustomization
    {
        public int                  CurrentSelection = 0;
        int currentSelectedSection = 0;
        int currentVehicleIDCategory = 0;

        [System.Serializable]
        public class Params
        {
            public Color    MatColor = Color.white;
            public int      Price = 500;
        }

        public List<Params>         ColorList = new List<Params>();

        [System.Serializable]
        public class Part
        {
            public Transform obj;
            public int matID = 0;
        }

        public List<Part> ObjList = new List<Part>();

        public UnityEvent           ActionCustomizationProcess;

        public enum AICustomType    { Default,Random ,Modulo, CustomList };
        public AICustomType         aiCustomType = AICustomType.Default;
        public List<int>            aiCustomList = new List<int>();

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
            return null; 
            #endregion
        }

        public List<int> GetPrice()
        {
            #region 
            List<int> priceList = new List<int>();
            for (var i = 0; i < ColorList.Count; i++)
                priceList.Add(ColorList[i].Price);

            return priceList; 
            #endregion
        }
        public int GetItemsNumber()
        {
            #region 
            return ColorList.Count; 
            #endregion
        }
        public void InitCustomPerformance()
        {
            #region 
            // if (ActionInitCustomPerformance != null)
            //    ActionInitCustomPerformance.Invoke(); 
            #endregion
        }

        public void TxtTest()
        {
            #region 
            // Debug.Log(Txt); 
            #endregion
        }

        public void ChangeTheMaterialColorAccessURPMainColor()
        {
            #region 
            for (var i = 0; i < ObjList.Count; i++)
            {
                if (ObjList[i].obj != null)
                {
                    if (CurrentSelection < 0)
                        AICustomizationAccessURPUnlitMainColorShader(i);
                    else
                        URPShaderApplyColor(i, CurrentSelection);
                }
            } 
            #endregion
        }

        public void URPShaderApplyColor(int i, int itemIndex)
        {
            #region
            ObjList[i].obj.GetComponent<Renderer>().materials[ObjList[i].matID].color = ColorList[itemIndex].MatColor;
            #endregion
        }


        public void AICustomizationAccessURPUnlitMainColorShader(int i)
        {
            #region 
            int index = returnItemIndex();
            URPShaderApplyColor(i, index);
            #endregion
        }

        public void ChangeCarPaintingColor01()
        {
            #region 
            for (var i = 0; i < ObjList.Count; i++)
            {
                if (ObjList[i] != null)
                {
                    if (CurrentSelection < 0)
                        AICustomizationCarPaintingColor01(i);
                    else
                        CarPaintingColor01ApplyColor(i, CurrentSelection);
                }
            }
            #endregion
        }

        public void AICustomizationCarPaintingColor01(int i)
        {
            #region 
            int index = returnItemIndex();
            CarPaintingColor01ApplyColor(i, index);
            #endregion
        }

        public void CarPaintingColor01ApplyColor(int i, int itemIndex)
        {
            #region
            ObjList[i].obj.GetComponent<Renderer>().materials[ObjList[i].matID].SetColor("_LiveryMainColor", ColorList[itemIndex].MatColor);
            #endregion
        }

        public void CustomMethodToChangeMainColor(int materialIndex)
        {
            #region 
            for (var i = 0; i < ObjList.Count; i++)
            {
                if (ObjList[0] != null)
                {
                    if (CurrentSelection < 0)
                        AICustomMethodToChangeMainColor(materialIndex);
                    else
                        CustomMethodApplyColor(materialIndex, CurrentSelection);
                }
            }
            #endregion
        }

        void AICustomMethodToChangeMainColor(int materialIndex)
        {
            #region 
            int index = returnItemIndex();
            CustomMethodApplyColor(materialIndex,index);
            #endregion
        }

        void CustomMethodApplyColor(int materialIndex,int itemIndex)
        {
            #region
            // Example to change the main color of a material in a standard urp shader 
            //ObjList[0].GetComponent<Renderer>().materials[materialIndex].color = ColorList[itemIndex].MatColor;

            // Access your material with:
            // ObjList[0].GetComponent<Renderer>().materials[materialIndex]

            // Access a car color in the list of color with
            // ColorList[itemIndex].MatColor


            // Write your custom code here to change the car body color. 
            #endregion
        }

        int returnItemIndex()
        {
            #region 
            int index = 0;
            switch (aiCustomType)
            {
                case AICustomType.Default:
                    index = 0;
                    break;
                case AICustomType.Random:
                    index = UnityEngine.Random.Range(0, ColorList.Count);
                    break;
                case AICustomType.Modulo:
                    index = CurrentSelection;
                    index++;
                    index *= -1;


                    var customizationVehicleList = InfoVehicleCustomization.instance.CustomizationVehicleList;
                    int offset = customizationVehicleList[currentVehicleIDCategory].Vehicle[currentSelectedSection].CurrentSelection;
                    index += offset;
                    index %= ColorList.Count;

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

            return index; 
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
