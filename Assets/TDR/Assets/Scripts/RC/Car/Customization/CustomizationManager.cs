// Description: Attached to CustomizationManager. 
// Manage the UI page that allows to customize vehicle.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TS.Generics
{
    public class CustomizationManager : MonoBehaviour
    {
        public bool seeInspector = true;
        [HideInInspector]
        public VehicleCustomization     Vehicle;
        public int                      CurrentSelectedParam = 0;
        public int                      currentItem = 0;

        public bool                     isCategorySelected = false;
        public string                   currentCategoryName = "";
        public int                      TSInputKeyBack = 0;
        [HideInInspector]
        public bool                     isBackButtonAllowed = true;

        [System.Serializable]
        public class TabParams
        {
            public string           Name = "";
            public CanvasGroup      CanvasGrp;
            public List<Transform>  objList = new List<Transform>();
        }
        public List<TabParams>          TabsList = new List<TabParams>();


        [System.Serializable]
        public class CategoryParams
        {
            public string       name = "";
            public GameObject   objButton;
        }

        public List<CategoryParams>     CategoryButtonList = new List<CategoryParams>();

        public OrbitalCam               orbitalCam;

        public GameObject               grpCategory;
        public GameObject               txtPerformance;
        public GameObject               txtKit;

        public List<GameObject>         customObjList = new List<GameObject>();

        public enum NewCat { Model, Value, Livery};
        public NewCat category = new NewCat();

        public string categoryName = "";

        public List<GameObject> categoryRef = new List<GameObject>();

        public void DisplayCustomPartName(int index)
        {
            #region
            Debug.Log(  InfoVehicleCustomization.instance.CustomizationVehicleList[0].Vehicle[index].Name);
            #endregion
        }

        public bool initCustomizationTabs()
        {
            #region
            Vehicle = FindFirstObjectByType<VehicleCustomization>();

            foreach (TabParams tab in TabsList)
            {
                tab.CanvasGrp.alpha = 0;
                tab.CanvasGrp.gameObject.SetActive(false);
            }
               

            InitCategoryButtons();

            return true;
            #endregion
        }

        public bool CheckCoins(int customType)
        {
            #region
            // Find Current Car
            int carID = InfoVehicle.instance.currentVehicleDisplayedInTheGarage;
            InfoVehicleCustomization.Section carParams = InfoVehicleCustomization.instance.CustomizationVehicleList[carID].Vehicle[CurrentSelectedParam];

            int currentSelection = carParams.CurrentSelection;
            int price = 0;
            if(customType == 0)
                price = Vehicle.CustomSectionList[CurrentSelectedParam].ObjSection.GetComponent<VehicleCustomValue>().ValueList[currentSelection].Price;
            if (customType == 1)
                price = Vehicle.CustomSectionList[CurrentSelectedParam].ObjSection.GetComponent<VehicleCustomModels>().ModelsList[currentSelection].Price;
            if (customType == 2)
                price = Vehicle.CustomSectionList[CurrentSelectedParam].ObjSection.GetComponent<VehicleCustomColor>().ColorList[currentSelection].Price;
            if (customType == 3)
                price = Vehicle.CustomSectionList[CurrentSelectedParam].ObjSection.GetComponent<VehicleCustomLivery>().liveryList[currentSelection].Price;


            // Vehicle is not already unlocked
            if (InfoCoins.instance.currentPlayerCoins >= price)
                return true;

            return false;
            #endregion
        }

        public void DisplayAValue(string _name)
        {
            #region
            int index = 0;
            for (var i = 0; i < Vehicle.CustomSectionList.Count; i++)
            {
                if (Vehicle.CustomSectionList[i].Name == _name)
                {
                    index = i;
                    break;
                }
            }


            ResetModelOrColor(CurrentSelectedParam);

            CurrentSelectedParam = index;

            for (var i = 0; i < TabsList.Count; i++)
            {
                if (i == 0)
                    TabsList[i].CanvasGrp.gameObject.SetActive(true);
                else
                    TabsList[i].CanvasGrp.gameObject.SetActive(false);
            }

            for (var i =0;i< TabsList.Count; i++)
            {
                if(i == 0)
                {
                    UpdateValue(index,i);
                    TabsList[i].CanvasGrp.alpha = 1;
                }
                else
                    TabsList[i].CanvasGrp.alpha = 0;
            }

            // Find Current Car
            int carID = InfoVehicle.instance.currentVehicleDisplayedInTheGarage;
            InfoVehicleCustomization.Section carParams = InfoVehicleCustomization.instance.CustomizationVehicleList[carID].Vehicle[CurrentSelectedParam];
            if (!carParams.itemList[carParams.itemList.Count-1].IsUnlocked )
                TS_EventSystem.instance.eventSystem.SetSelectedGameObject(TabsList[0].objList[7].gameObject);

            isCategorySelected = true;
            currentCategoryName = _name;

            //Debug.Log(InfoVehicleCustomization.instance.CustomizationVehicleList[0].Vehicle[index].Name);
            #endregion
        }

        void UpdateValue(int index,int i)
        {
            #region
            // Find Current Car
            int carID = InfoVehicle.instance.currentVehicleDisplayedInTheGarage;
            InfoVehicleCustomization.Section carParams = InfoVehicleCustomization.instance.CustomizationVehicleList[carID].Vehicle[index];

            int currentSelection = carParams.CurrentSelection;

            int textList = Vehicle.CustomSectionList[index].TextList;
            int textID = Vehicle.CustomSectionList[index].TextID;

            float percentage = Vehicle.CustomSectionList[index].ObjSection.GetComponent<VehicleCustomValue>().ValueList[currentSelection].percentage;
            int price = Vehicle.CustomSectionList[index].ObjSection.GetComponent<VehicleCustomValue>().ValueList[currentSelection].Price;

            // Update UI info
            TabsList[i].objList[0].GetComponent<CurrentText>().NewTextWithSpecificID(textID, textList);
            TabsList[i].objList[1].GetComponent<CurrentText>().DisplayTextComponent(TabsList[i].objList[1].gameObject, "+" + percentage + "%");
            string txtPrice = LanguageManager.instance.String_ReturnText(0, 182);
            TabsList[i].objList[2].GetComponent<CurrentText>().DisplayTextComponent(TabsList[i].objList[2].gameObject, txtPrice + " " + price);

            int numberOfimprovement = Vehicle.CustomSectionList[index].ObjSection.GetComponent<VehicleCustomValue>().ValueList.Count;
            TabsList[i].objList[3].GetComponent<CurrentText>().DisplayTextComponent(TabsList[i].objList[3].gameObject, currentSelection + "/" + numberOfimprovement);

            float gaugeSize = (TabsList[i].objList[4].parent.GetComponent<RectTransform>().rect.width / numberOfimprovement) * currentSelection;
            TabsList[i].objList[4].GetComponent<RectTransform>().sizeDelta = new Vector2(gaugeSize, TabsList[i].objList[4].GetComponent<RectTransform>().rect.height);

            int howManyItems = carParams.itemList.Count;
            if (carParams.itemList[howManyItems-1].IsUnlocked)
            {
                TabsList[i].objList[1].gameObject.SetActive(false);     // Upgrade info
                TabsList[i].objList[2].gameObject.SetActive(false);     // Price
                TabsList[i].objList[3].gameObject.SetActive(false);     // x/x
                TabsList[i].objList[6].GetComponent<CurrentText>().NewTextWithSpecificID(194, 0);    // All update aquired
                TabsList[i].objList[6].gameObject.SetActive(true);
                TabsList[i].objList[5].gameObject.SetActive(false);     // Button Buy
                TabsList[i].objList[4].GetComponent<RectTransform>().sizeDelta = TabsList[i].objList[4].transform.parent.GetComponent<RectTransform>().sizeDelta;
            }
            else
            {
                TabsList[i].objList[1].gameObject.SetActive(true);     // Upgrade info
                TabsList[i].objList[2].gameObject.SetActive(true);     // Price
                TabsList[i].objList[3].gameObject.SetActive(true);     // x/x
                TabsList[i].objList[6].gameObject.SetActive(false);    // All update aquired
                TabsList[i].objList[5].gameObject.SetActive(true);     // Button Buy
            }
            #endregion
        }
       
        public void DisplayAModel(string _name)
        {
            #region
            int index = 0;
            for (var i = 0; i < Vehicle.CustomSectionList.Count; i++)
            {
                if(Vehicle.CustomSectionList[i].Name == _name)
                {
                    index = i;
                    break;
                }
            }

            

            ResetModelOrColor(CurrentSelectedParam);

            CurrentSelectedParam = index;

            for (var i = 0; i < TabsList.Count; i++)
            {
                if (i == 1)
                    TabsList[i].CanvasGrp.gameObject.SetActive(true);
                else
                    TabsList[i].CanvasGrp.gameObject.SetActive(false);
            }

            for (var i = 0; i < TabsList.Count; i++)
            {
                if (i == 1)
                    TabsList[i].CanvasGrp.alpha = 1;
                else
                {
                    UpdateModel(index);
                    TabsList[i].CanvasGrp.alpha = 0;
                }   
            }

            TS_EventSystem.instance.eventSystem.SetSelectedGameObject(TabsList[1].objList[5].gameObject);

            isCategorySelected = true;
            currentCategoryName = _name;
            //Debug.Log(InfoVehicleCustomization.instance.CustomizationVehicleList[0].Vehicle[index].Name);
            #endregion
        }

        public void UpdateModel(int index, int dir = 0)
        {
            #region
            int howManyItems = Vehicle.CustomSectionList[index].ObjSection.GetComponent<VehicleCustomModels>().ModelsList.Count;
          
            // Find Current Car
            int carID = InfoVehicle.instance.currentVehicleDisplayedInTheGarage;
            InfoVehicleCustomization.Section carParams = InfoVehicleCustomization.instance.CustomizationVehicleList[carID].Vehicle[index];

            currentItem = carParams.CurrentSelection;

            currentItem += dir + 2 * howManyItems;
            currentItem %= howManyItems;
            Vehicle.CustomSectionList[index].ObjSection.GetComponent<VehicleCustomModels>().CurrentSelection = currentItem;

            carParams.CurrentSelection = currentItem;

            if (carParams.itemList[carParams.CurrentSelection].IsUnlocked)
                carParams.TemporarySelection = currentItem;

            Vehicle.CustomSectionList[index].ObjSection.GetComponent<VehicleCustomModels>().CustomizationProcess();


            // Update UI Info
            Sprite sprite = Vehicle.CustomSectionList[index].ObjSection.GetComponent<VehicleCustomModels>().GetSprite();
            if (sprite)
                TabsList[1].objList[2].GetComponent<Image>().sprite = sprite;    
            else
                TabsList[1].objList[2].GetComponent<Image>().sprite = null;

            int textList = Vehicle.CustomSectionList[index].TextList;
            int textID = Vehicle.CustomSectionList[index].TextID;
            TabsList[1].objList[0].GetComponent<CurrentText>().NewTextWithSpecificID(textID, textList);

            int price = Vehicle.CustomSectionList[index].ObjSection.GetComponent<VehicleCustomModels>().ModelsList[currentItem].Price;

            string txtPrice = LanguageManager.instance.String_ReturnText(0, 182);
            TabsList[1].objList[4].GetComponent<CurrentText>().DisplayTextComponent(TabsList[1].objList[4].gameObject, txtPrice + " " + price);

            TabsList[1].objList[6].GetComponent<CurrentText>().DisplayTextComponent(TabsList[1].objList[6].gameObject, (currentItem+1).ToString() + "/" + howManyItems);

            if (carParams.itemList[currentItem].IsUnlocked)
            {
                TabsList[1].objList[3].gameObject.SetActive(false);
                TabsList[1].objList[4].gameObject.SetActive(false);
            }
            else
            {
                TabsList[1].objList[3].gameObject.SetActive(true);
                TabsList[1].objList[4].gameObject.SetActive(true);
            }
            #endregion
        }

        public void ButtonNewModel(int dir)
        {
            #region
            UpdateModel(CurrentSelectedParam, dir);
            #endregion
        }

        void ResetModelOrColor(int index)
        {
            #region
            // Find Current Car
            int carID = InfoVehicle.instance.currentVehicleDisplayedInTheGarage;
            InfoVehicleCustomization.Section carParams = InfoVehicleCustomization.instance.CustomizationVehicleList[carID].Vehicle[index];

            if(carParams.CurrentSelection != carParams.TemporarySelection &&
                carParams.TemporarySelection != -1)
            {
                carParams.CurrentSelection = carParams.TemporarySelection;
                if (Vehicle.CustomSectionList[index].ObjSection.GetComponent<VehicleCustomModels>())
                {
                    Vehicle.CustomSectionList[index].ObjSection.GetComponent<VehicleCustomModels>().CurrentSelection = carParams.CurrentSelection;
                    Vehicle.CustomSectionList[index].ObjSection.GetComponent<VehicleCustomModels>().CustomizationProcess();
                }
                if (Vehicle.CustomSectionList[index].ObjSection.GetComponent<VehicleCustomColor>())
                {
                    Vehicle.CustomSectionList[index].ObjSection.GetComponent<VehicleCustomColor>().CurrentSelection = carParams.CurrentSelection;
                    Vehicle.CustomSectionList[index].ObjSection.GetComponent<VehicleCustomColor>().CustomizationProcess();
                }
                if (Vehicle.CustomSectionList[index].ObjSection.GetComponent<VehicleCustomLivery>())
                {
                    Vehicle.CustomSectionList[index].ObjSection.GetComponent<VehicleCustomLivery>().CurrentSelection = carParams.CurrentSelection;
                    Vehicle.CustomSectionList[index].ObjSection.GetComponent<VehicleCustomLivery>().CustomizationProcess();
                }
            }
            #endregion
        }

        public void BuyValueItem()
        {
            #region
            // Find Current Car
            int carID = InfoVehicle.instance.currentVehicleDisplayedInTheGarage;
            InfoVehicleCustomization.Section carParams = InfoVehicleCustomization.instance.CustomizationVehicleList[carID].Vehicle[CurrentSelectedParam];

            carParams.itemList[carParams.CurrentSelection].IsUnlocked = true;

            UpdateCoins(0, carParams.CurrentSelection);


            carParams.CurrentSelection ++;

            if (carParams.CurrentSelection == carParams.itemList.Count)
                TS_EventSystem.instance.eventSystem.SetSelectedGameObject(TabsList[0].objList[8].gameObject);

            carParams.CurrentSelection = Mathf.Clamp(carParams.CurrentSelection,0,carParams.itemList.Count-1);

            UpdateValue(CurrentSelectedParam, 0);

           

            #endregion
        }

        public void BuyModelItem()
        {
            #region
            // Find Current Car
            int carID = InfoVehicle.instance.currentVehicleDisplayedInTheGarage;
            InfoVehicleCustomization.Section carParams = InfoVehicleCustomization.instance.CustomizationVehicleList[carID].Vehicle[CurrentSelectedParam];

            carParams.itemList[carParams.CurrentSelection].IsUnlocked = true;

            UpdateCoins(1, carParams.CurrentSelection);

            UpdateModel(CurrentSelectedParam, 0);
            TS_EventSystem.instance.eventSystem.SetSelectedGameObject(TabsList[1].objList[5].gameObject);

            #endregion
        }

        public void AutoSave()
        {
            #region
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;
            LoadSavePlayerProgession.instance.SavePlayerProgression();
            StartCoroutine(AutoSaveRoutine());
          
            #endregion
        }

        void InitCategoryButtons()
        {
            #region Show or Hide Customization Category buttons. Show if the category contains Item.

            if (InfoVehicleCustomization.instance.isInitDone)
            {

                int carID = InfoVehicle.instance.currentVehicleDisplayedInTheGarage;
                InfoVehicleCustomization.CustomizationParams carParams = InfoVehicleCustomization.instance.CustomizationVehicleList[carID];

                for (var i = 0; i < CategoryButtonList.Count; i++)
                {
                    bool fund = false;
                    for (var j = 0; j < carParams.Vehicle.Count; j++)
                    {
                        if (carParams.Vehicle[j].Name == CategoryButtonList[i].name)
                        {
                            if (carParams.Vehicle[j].itemList.Count > 0)
                            {
                                CategoryButtonList[i].objButton.SetActive(true);
                                fund = true;
                                break;
                            }

                        }

                    }
                    if (!fund)
                        CategoryButtonList[i].objButton.SetActive(false);
                }



                int howManyChildrenInGrpCat = grpCategory.transform.childCount;
                // find kit position in the list
                int txtKitChildID = 0;
                for (var i = howManyChildrenInGrpCat - 1; i >= 0; i--)
                {
                    if (grpCategory.transform.GetChild(i).gameObject == txtKit)
                    {
                        txtKitChildID = i;
                        break;
                    }
                }

                // Debug.Log(txtKitChildID);

                // find if a performance button is available
                bool kitFund = false;
                for (var i = howManyChildrenInGrpCat - 1; i > txtKitChildID; i--)
                {
                    if (grpCategory.transform.GetChild(i).gameObject.activeSelf)
                    {
                        //Debug.Log("perf: " + grpCategory.transform.GetChild(i).name);
                        kitFund = true;
                        break;
                    }
                }

                // find if a kit button is available

                bool perfFund = false;
                for (var i = txtKitChildID - 1; i > 0; i--)
                {
                    if (grpCategory.transform.GetChild(i).gameObject.activeSelf)
                    {
                        //Debug.Log("Kit: " + grpCategory.transform.GetChild(i).name);
                        perfFund = true;
                        break;
                    }
                }


                // Debug.Log("perfFund: " + perfFund + " | kitFund: " + kitFund);

                if (!kitFund) txtKit.SetActive(false);
                else txtKit.SetActive(true);

                if (!perfFund) txtPerformance.SetActive(false);
                else txtPerformance.SetActive(true);
            }



            #endregion
        }

        public void DisplayAColor(string _name)
        {
            #region
            int index = 0;
            for (var i = 0; i < Vehicle.CustomSectionList.Count; i++)
            {
                if (Vehicle.CustomSectionList[i].Name == _name)
                {
                    index = i;
                    break;
                }
            }

            ResetModelOrColor(CurrentSelectedParam);

            CurrentSelectedParam = index;

            for (var i = 0; i < TabsList.Count; i++)
            {
                if (i == 2)
                    TabsList[i].CanvasGrp.gameObject.SetActive(true);
                else
                    TabsList[i].CanvasGrp.gameObject.SetActive(false);
            }

            for (var i = 0; i < TabsList.Count; i++)
            {
                if (i == 2)
                {
                    UpdateColorPanel(index);
                    TabsList[i].CanvasGrp.alpha = 1;
                }
                else
                {
                    TabsList[i].CanvasGrp.alpha = 0;
                }
            }

            isCategorySelected = true;
            currentCategoryName = _name;
            //Debug.Log(InfoVehicleCustomization.instance.CustomizationVehicleList[0].Vehicle[index].Name);
            #endregion
        }

        public void UpdateColorPanel(int index)
        {
            #region
            VehicleCustomColor vehicleCustomColor = Vehicle.CustomSectionList[index].ObjSection.GetComponent<VehicleCustomColor>();

            List<Color> colorList = new List<Color>();

            for(var i = 0; i < vehicleCustomColor.ColorList.Count; i++)
                colorList.Add(vehicleCustomColor.ColorList[i].MatColor);

            int carID = InfoVehicle.instance.currentVehicleDisplayedInTheGarage;
            InfoVehicleCustomization.Section carParams = InfoVehicleCustomization.instance.CustomizationVehicleList[carID].Vehicle[CurrentSelectedParam];

            // Update UI Info
            TabsList[2].objList[0].GetComponent<ColorPanel>().InitColorPanel(colorList, carParams.CurrentSelection);
            UpdateColor(carParams.CurrentSelection);
            #endregion
        }

        public void UpdateColor(int colorIndex)
        {
            #region
            CustomizationManager customizationManager = FindFirstObjectByType<CustomizationManager>();

            int CurrentSelectedParam = customizationManager.CurrentSelectedParam;

            // Find Current Car
            int carID = InfoVehicle.instance.currentVehicleDisplayedInTheGarage;
            InfoVehicleCustomization.Section carParams = InfoVehicleCustomization.instance.CustomizationVehicleList[carID].Vehicle[CurrentSelectedParam];

            currentItem = colorIndex;

            customizationManager.Vehicle.CustomSectionList[CurrentSelectedParam].ObjSection.GetComponent<VehicleCustomColor>().CurrentSelection = currentItem;

            carParams.CurrentSelection = currentItem;

            // Remember if the current item is unlocked by the player
            if (carParams.itemList[carParams.CurrentSelection].IsUnlocked)
                carParams.TemporarySelection = currentItem;

            customizationManager.Vehicle.CustomSectionList[CurrentSelectedParam].ObjSection.GetComponent<VehicleCustomColor>().CustomizationProcess();

            // Update Ui Info
            int textList = Vehicle.CustomSectionList[CurrentSelectedParam].TextList;
            int textID = Vehicle.CustomSectionList[CurrentSelectedParam].TextID;
            TabsList[2].objList[1].GetComponent<CurrentText>().NewTextWithSpecificID(textID, textList);

            int price = customizationManager.Vehicle.CustomSectionList[CurrentSelectedParam].ObjSection.GetComponent<VehicleCustomColor>().ColorList[currentItem].Price;

            string txtPrice = LanguageManager.instance.String_ReturnText(0, 182);
            customizationManager.TabsList[2].objList[2].GetComponent<CurrentText>().DisplayTextComponent(customizationManager.TabsList[2].objList[2].gameObject, txtPrice + " " + price);

            if (carParams.itemList[currentItem].IsUnlocked)
            {
                customizationManager.TabsList[2].objList[2].gameObject.SetActive(false);
                customizationManager.TabsList[2].objList[3].gameObject.SetActive(false);
            }
            else
            {
                customizationManager.TabsList[2].objList[2].gameObject.SetActive(true);
                customizationManager.TabsList[2].objList[3].gameObject.SetActive(true);
            }
            #endregion
        }

        public void BuyColorItem()
        {
            #region
            // Find Current Car
            int carID = InfoVehicle.instance.currentVehicleDisplayedInTheGarage;
            InfoVehicleCustomization.Section carParams = InfoVehicleCustomization.instance.CustomizationVehicleList[carID].Vehicle[CurrentSelectedParam];

            carParams.itemList[carParams.CurrentSelection].IsUnlocked = true;
            UpdateCoins(2, carParams.CurrentSelection);

            UpdateColor(carParams.CurrentSelection);

            ColorSlot[] allSlot = FindObjectsByType<ColorSlot>(FindObjectsSortMode.None);
            //Debug.Log("How many slot: " + allSlot.Length);
            foreach (ColorSlot slot in allSlot)
            {
                if (slot.Index == currentItem)
                {
                    TS_EventSystem.instance.eventSystem.SetSelectedGameObject(slot.gameObject);
                    break;
                }
            }

            #endregion
        }

        public void DisplayALivery(string _name)
        {
            #region
            int index = 0;
            for (var i = 0; i < Vehicle.CustomSectionList.Count; i++)
            {
                if (Vehicle.CustomSectionList[i].Name == _name)
                {
                    index = i;
                    break;
                }
            }



            ResetModelOrColor(CurrentSelectedParam);

            CurrentSelectedParam = index;

            for (var i = 0; i < TabsList.Count; i++)
            {
                if (i == 3)
                    TabsList[i].CanvasGrp.gameObject.SetActive(true);
                else
                    TabsList[i].CanvasGrp.gameObject.SetActive(false);
            }

            for (var i = 0; i < TabsList.Count; i++)
            {
                if (i == 3)
                    TabsList[i].CanvasGrp.alpha = 1;
                else
                {
                    UpdateLivery(index);
                    TabsList[i].CanvasGrp.alpha = 0;
                }
            }

            TS_EventSystem.instance.eventSystem.SetSelectedGameObject(TabsList[3].objList[5].gameObject);

            isCategorySelected = true;
            currentCategoryName = _name;
            //Debug.Log(InfoVehicleCustomization.instance.CustomizationVehicleList[0].Vehicle[index].Name);
            #endregion
        }

        public void DisplayNewLivery(int dir)
        {
            #region
            UpdateLivery(CurrentSelectedParam, dir);
            #endregion
        }

      
        public void UpdateLivery(int index, int dir = 0)
        {
            #region
            int howManyItems = Vehicle.CustomSectionList[index].ObjSection.GetComponent<VehicleCustomLivery>().liveryList.Count;

            // Find Current Car
            int carID = InfoVehicle.instance.currentVehicleDisplayedInTheGarage;
            InfoVehicleCustomization.Section carParams = InfoVehicleCustomization.instance.CustomizationVehicleList[carID].Vehicle[index];

            currentItem = carParams.CurrentSelection;

            currentItem += dir + 2 * howManyItems;
            currentItem %= howManyItems;
            Vehicle.CustomSectionList[index].ObjSection.GetComponent<VehicleCustomLivery>().CurrentSelection = currentItem;

            carParams.CurrentSelection = currentItem;

            if (carParams.itemList[carParams.CurrentSelection].IsUnlocked)
                carParams.TemporarySelection = currentItem;

            Vehicle.CustomSectionList[index].ObjSection.GetComponent<VehicleCustomLivery>().CustomizationProcess();


            // Update UI Info
            Sprite sprite = Vehicle.CustomSectionList[index].ObjSection.GetComponent<VehicleCustomLivery>().GetSprite();
            if (sprite)
                TabsList[3].objList[2].GetComponent<Image>().sprite = sprite;
            else
                TabsList[3].objList[2].GetComponent<Image>().sprite = null;

            int textList = Vehicle.CustomSectionList[index].TextList;
            int textID = Vehicle.CustomSectionList[index].TextID;
            TabsList[3].objList[0].GetComponent<CurrentText>().NewTextWithSpecificID(textID, textList);

            int price = Vehicle.CustomSectionList[index].ObjSection.GetComponent<VehicleCustomLivery>().liveryList[currentItem].Price;

            string txtPrice = LanguageManager.instance.String_ReturnText(0, 182);
            TabsList[3].objList[4].GetComponent<CurrentText>().DisplayTextComponent(TabsList[3].objList[4].gameObject, txtPrice + " " + price);

            TabsList[3].objList[6].GetComponent<CurrentText>().DisplayTextComponent(TabsList[3].objList[6].gameObject, (currentItem+1).ToString() + "/" + howManyItems);

            if (carParams.itemList[currentItem].IsUnlocked)
            {
                TabsList[3].objList[3].gameObject.SetActive(false);
                TabsList[3].objList[4].gameObject.SetActive(false);
            }
            else
            {
                TabsList[3].objList[3].gameObject.SetActive(true);
                TabsList[3].objList[4].gameObject.SetActive(true);
            }



            #endregion
        }

        public void BuyLiveryItem()
        {
            #region
            // Find Current Car
            int carID = InfoVehicle.instance.currentVehicleDisplayedInTheGarage;
            InfoVehicleCustomization.Section carParams = InfoVehicleCustomization.instance.CustomizationVehicleList[carID].Vehicle[CurrentSelectedParam];

            carParams.itemList[carParams.CurrentSelection].IsUnlocked = true;
            UpdateCoins(3, carParams.CurrentSelection);
            UpdateLivery(3, 0);


            TS_EventSystem.instance.eventSystem.SetSelectedGameObject(TabsList[3].objList[5].gameObject);

            #endregion
        }

        public void PageOutResetColorOrModel()
        {
            #region
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;
            ResetModelOrColor(CurrentSelectedParam);


            foreach (TabParams tab in TabsList)
            {
                tab.CanvasGrp.alpha = 0;
                tab.CanvasGrp.gameObject.SetActive(false);
            }

            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false;
            #endregion
        }

        public void NewOrbitalPosition(string _name)
        {
            #region MyRegion
            int index = 0;
            for (var i = 0; i < Vehicle.CustomSectionList.Count; i++)
            {
                if (Vehicle.CustomSectionList[i].Name == _name)
                {
                    index = i;
                    break;
                }
            }

            int carID = InfoVehicle.instance.currentVehicleDisplayedInTheGarage;
            InfoVehicleCustomization.CustomizationParams carParams = InfoVehicleCustomization.instance.CustomizationVehicleList[carID];

            if (carParams.Vehicle[index].orbitalState == OrbitalState.NewPosition)
                orbitalCam.NewCamVectorThreePosition(carParams.Vehicle[index].newCamPosition); 
            #endregion
        }

        void OnEnable()
        {
            #region
            InfoInputs.instance.ListOfInputsForEachPlayer[0].listOfButtons[TSInputKeyBack].OnGetKeyReceived += OnGetKeyBackAction;
            #endregion
        }
       void OnDisable()
        {
            #region
            InfoInputs.instance.ListOfInputsForEachPlayer[0].listOfButtons[TSInputKeyBack].OnGetKeyReceived -= OnGetKeyBackAction;
            #endregion
        }


        void OnGetKeyBackAction()
        {
            #region
            if (isCategorySelected)
            {
                StopAllCoroutines();
                StartCoroutine(BackButtonAllowedRoutine());

                isCategorySelected = false;

                ResetModelOrColor(CurrentSelectedParam);

                for (var i = 0; i < TabsList.Count; i++)
                    TabsList[i].CanvasGrp.gameObject.SetActive(false);

                for (var i = 0; i < CategoryButtonList.Count; i++)
                {
                    if(CategoryButtonList[i].name == currentCategoryName)
                        TS_EventSystem.instance.eventSystem.SetSelectedGameObject(CategoryButtonList[i].objButton.transform.GetChild(0).GetChild(1).gameObject);
                }

                currentCategoryName = "";
            } 
            #endregion
        }

        IEnumerator BackButtonAllowedRoutine()
        {
            #region 
            isBackButtonAllowed = false;
            yield return new WaitForSeconds(.2f);
            isBackButtonAllowed = true;
            yield return null; 
            #endregion
        }

        public void UpdateCoins(int customType,int currentSelection)
        {
            #region
            // Find Current Car
            int carID = InfoVehicle.instance.currentVehicleDisplayedInTheGarage;
            InfoVehicleCustomization.Section carParams = InfoVehicleCustomization.instance.CustomizationVehicleList[carID].Vehicle[CurrentSelectedParam];

            //int currentSelection = carParams.CurrentSelection;
            int price = 0;
            if (customType == 0)
                price = Vehicle.CustomSectionList[CurrentSelectedParam].ObjSection.GetComponent<VehicleCustomValue>().ValueList[currentSelection].Price;
            if (customType == 1)
                price = Vehicle.CustomSectionList[CurrentSelectedParam].ObjSection.GetComponent<VehicleCustomModels>().ModelsList[currentSelection].Price;
            if (customType == 2)
                price = Vehicle.CustomSectionList[CurrentSelectedParam].ObjSection.GetComponent<VehicleCustomColor>().ColorList[currentSelection].Price;
            if (customType == 3)
                price = Vehicle.CustomSectionList[CurrentSelectedParam].ObjSection.GetComponent<VehicleCustomLivery>().liveryList[currentSelection].Price;


            InfoCoins.instance.UpdateCoins(-price);
            #endregion
        }

        IEnumerator AutoSaveRoutine()
        {
            #region 

            yield return new WaitUntil(()=>!CanvasAutoSave.instance.transform.GetChild(0).gameObject.activeInHierarchy);

            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false;
            yield return null;
            #endregion
        }
    }
}
