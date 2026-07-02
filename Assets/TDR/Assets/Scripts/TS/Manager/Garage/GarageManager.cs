//Description: GarageManager. Attached to GarageManager in the Main Menu scene
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Globalization;
using UnityEngine.UI;

namespace TS.Generics
{
    public class GarageManager : MonoBehaviour
    {
        public static GarageManager instance = null;                    //Static instance allows to be accessed by any other script.
        public bool                 b_IsInitGarageInProcess = false;
        public int                  garagePageID = 13;

        public bool                 bActionAvailable;
        public bool displayVehiclecleThumbnailInProgress;
        public UnityEvent           newVechicleEvent;                   // List of events use when a new vehicle is loaded
        public UnityEvent           instantiateNewVehicleEvent;         // Manage the vehicle instantiation

        public CurrentText          HowManyVehicleAvailable;
        public CurrentText          selectedCategory;
        public CurrentText          VehicleName;
        public CurrentText          VehiclePrice;
        public CurrentText          txtOwn;
        public int                  listTextBuy = 0;
        public int                  idTextBuy = 59;
        public int                  listTextOwn = 0;
        public int                  idTextOwn = 113;

        public GameObject           grpBuyInfo;
        public CurrentText          txtBuyInfo;
        public int                  listTextNotEnoughCredits = 0;
        public int                  idTextBuyNotEnoughCredits = 79;
        public int                  listTextAlredyBought = 0;
        public int                  idTextAlredyBought = 114;
        public bool                 customizationAvailable = false;
        public int                  listTextCustomize = 0;
        public int                  idTextCustomize = 187;


        public GameObject           objLock;
        public GameObject           grpPrice;

        //-> Use to set up the transition (duration and curve)
        public float                duration = 1;                       // the transition duration
        public AnimationCurve       animSpeedCurve;
        [HideInInspector]
        public GarageTagPivot       garageTagPivot;
        [HideInInspector]
        public Transform            vehicleInstantiatePosition;

        public GameObject           objLoadVehicle;

        public int                  direction;


        public int                  CurrentCategory = 0;
        public GameObject           objScrollViewCategory;
        int lastVehicleID = 0;

        public bool EnableIsKiniematicWhenNewVehicleSpawned = true;

        public float waitDurationAfterVehicleSpawn = .7f;

        public Image thumbnailCar;

        void Awake()
        {
            #region
            //Check if instance already exists
            if (instance == null)
                instance = this;

            //If instance already exists and it's not this:
            else if (instance != this)
                Destroy(gameObject);
            #endregion
        }

        //-> Init the garage (Call by the GarageManagerAssistant)
        public IEnumerator OpenGarageRoutine()
        {
            #region
            InfoPlayerTS.instance.b_IsAvailableToDoSomething = false;

            //-> Open the garage page
            PageIn currentMenu = CanvasMainMenuManager.instance.listMenu[garagePageID].transform.parent.GetComponent<PageIn>();
            currentMenu.DisplayNewPage(garagePageID);
            InfoPlayerTS.instance.b_IsAvailableToDoSomething = true;
            bActionAvailable = true;

            DisplayVehicleCategory();


            yield return null;
            #endregion
        }

        //-> Call by Page Init (garage page) | Call when Button_PreviousVehicle or Button_NextVehicle are pressed
        public void DisplayNewVehicle(int Direction)
        {
            #region 
            if (bActionAvailable)
            {
                StopAllCoroutines();
                StartCoroutine(DisplayNextVehicleRoutine(Direction));
            }
                
            #endregion
        }

        //-> direction 0: Init menu | 1: next vehicle | -1: Previous vehicle
        IEnumerator DisplayNextVehicleRoutine(int _direction, float waitBeforeLoadingNewVehicle = 1)
        {
            #region 
            displayVehiclecleThumbnailInProgress = false;
            direction = _direction;
           
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;

            VehicleGlobalData vehicleData = DataRef.instance.vehicleGlobalData;
            InfoVehicle infoVehicle = InfoVehicle.instance;

            //-> Find the vehicle to display. Check if bShow in vehicle parameters is set to True.
            bool bNewEntry = false;
            while (bNewEntry != true)
            {
                //-> Use list Order
                if (!vehicleData.OrderUsingCustomList)
                {
                    infoVehicle.currentVehicleDisplayedInTheGarage += direction + InfoVehicle.instance.vehicleParametersInGameList.Count + InfoVehicle.instance.vehicleParametersInGameList.Count;
                    infoVehicle.currentVehicleDisplayedInTheGarage %= InfoVehicle.instance.vehicleParametersInGameList.Count;

                    if (InfoVehicle.instance.vehicleParametersInGameList[infoVehicle.currentVehicleDisplayedInTheGarage].bShow &&
                        (CurrentCategory == InfoVehicle.instance.vehicleParametersInGameList[infoVehicle.currentVehicleDisplayedInTheGarage].vehicleCategory
                         ||
                         CurrentCategory == -1)) bNewEntry = true;

                }
                //-> Use Custom Order
                else
                {

                    //Debug.Log("CurrentCategory: " + CurrentCategory);
                    infoVehicle.currentVehicleDisplayedInTheGarage += direction + vehicleData.customList.Count + vehicleData.customList.Count;
                    infoVehicle.currentVehicleDisplayedInTheGarage %= vehicleData.customList.Count;

                    if (InfoVehicle.instance.vehicleParametersInGameList[vehicleData.customList[infoVehicle.currentVehicleDisplayedInTheGarage]].bShow &&
                        (CurrentCategory == InfoVehicle.instance.vehicleParametersInGameList[vehicleData.customList[infoVehicle.currentVehicleDisplayedInTheGarage]].vehicleCategory
                         ||
                         CurrentCategory == -1)) bNewEntry = true;
                }

                yield return null;
            }

            //-> Use list Order
            int currentVehicle = infoVehicle.currentVehicleDisplayedInTheGarage;
            //-> Use Custom Order
            if (vehicleData.OrderUsingCustomList)
                currentVehicle = vehicleData.customList[infoVehicle.currentVehicleDisplayedInTheGarage];


            //thumbnailCar.DisplayVehicleSprite(CurrentCategory,currentVehicle);

            //-> Display the new vehicle info in UI
            UpdateVehicleInfo(currentVehicle, vehicleData);

            displayVehiclecleThumbnailInProgress = true;

            yield return new WaitForSeconds(waitBeforeLoadingNewVehicle);

            displayVehiclecleThumbnailInProgress = false;

            bActionAvailable = false;
            instantiateNewVehicleEvent?.Invoke();

            yield return null; 
            #endregion
        }

        //-> Display vehicle info 
        void UpdateVehicleInfo(int currentVehicle, VehicleGlobalData vehicleData)
        {
            #region 
            newVechicleEvent?.Invoke();
            //-> Display Vehicle name
            VehicleName.DisplayTextComponent(VehicleName.gameObject, InfoVehicle.instance.vehicleParametersInGameList[currentVehicle].name, false);

            int howManyVehicle = 0;
            int posCurrentCarInListWithOffset = 0;
            //-> Use list Order
            if (!vehicleData.OrderUsingCustomList)
            {
                for (var i = 0; i < InfoVehicle.instance.vehicleParametersInGameList.Count; i++)
                    if (InfoVehicle.instance.vehicleParametersInGameList[i].bShow
                         && (CurrentCategory == InfoVehicle.instance.vehicleParametersInGameList[i].vehicleCategory ||
                         CurrentCategory == -1))
                        howManyVehicle++;

                for (var i = 0; i < InfoVehicle.instance.vehicleParametersInGameList.Count; i++)
                {
                    if (i == currentVehicle)
                    {
                        break;
                    }
                      

                    if (InfoVehicle.instance.vehicleParametersInGameList[i].bShow
                         && (CurrentCategory == InfoVehicle.instance.vehicleParametersInGameList[i].vehicleCategory ||
                         CurrentCategory == -1))
                        posCurrentCarInListWithOffset++;
                }
            }
            //-> Use Custom Order
            else
            {
                for (var i = 0; i < vehicleData.customList.Count; i++)
                    if (InfoVehicle.instance.vehicleParametersInGameList[vehicleData.customList[i]].bShow
                         && (CurrentCategory == InfoVehicle.instance.vehicleParametersInGameList[vehicleData.customList[i]].vehicleCategory ||
                         CurrentCategory == -1))
                        howManyVehicle++;

                for (var i = 0; i < vehicleData.customList.Count; i++)
                {
                    if (vehicleData.customList[i] == currentVehicle)
                    {
                        break;
                    }
                       

                    if (InfoVehicle.instance.vehicleParametersInGameList[vehicleData.customList[i]].bShow
                         && (CurrentCategory == InfoVehicle.instance.vehicleParametersInGameList[vehicleData.customList[i]].vehicleCategory ||
                         CurrentCategory == -1))
                        posCurrentCarInListWithOffset++;
                }
            }

            //-> Display the number of vehicle available
            string newTxt = (posCurrentCarInListWithOffset + 1) + "/" + howManyVehicle;
            HowManyVehicleAvailable.DisplayTextComponent(VehicleName.gameObject, newTxt, false);

            // Display Vehicle Sprite
            if(InfoVehicle.instance.vehicleParametersInGameList[currentVehicle].img)
            thumbnailCar.sprite = InfoVehicle.instance.vehicleParametersInGameList[currentVehicle].img;

            //-> Display cost
            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = " ";     // Replace , with blank space
            nfi.NumberGroupSizes = new int[] { 3 }; // 1000

            string formatedCoins = InfoVehicle.instance.vehicleParametersInGameList[currentVehicle].cost.ToString("#,0", nfi);
            VehiclePrice.DisplayTextComponent(VehicleName.gameObject, formatedCoins, false);


            //-> Display lock
            if (!customizationAvailable && InfoVehicle.instance.vehicleParametersInGameList[currentVehicle].isUnlocked)
            {
                grpPrice.SetActive(false);
                objLock.SetActive(false);
                txtOwn.NewTextManageByScript(new List<TextEntry>(1) { new TextEntry(listTextOwn, idTextOwn) }, false);
            }
            else if (customizationAvailable && InfoVehicle.instance.vehicleParametersInGameList[currentVehicle].isUnlocked)
            {
                grpPrice.SetActive(false);
                objLock.SetActive(false);
                txtOwn.NewTextManageByScript(new List<TextEntry>(1) { new TextEntry(listTextCustomize, idTextCustomize) }, false);
            }
            else
            {
                txtOwn.NewTextManageByScript(new List<TextEntry>(1) { new TextEntry(listTextBuy, idTextBuy) }, false);
                objLock.SetActive(true);
                grpPrice.SetActive(true);
            }

           
            #endregion
        }

        //-> Feedback Not enough credit | Already bought
        public void BuyButtonFeedback(int state)
        {
            #region 
            StopAllCoroutines();
            StartCoroutine(BuyButtonFeedbackRoutine(state)); 
            #endregion
        }

        IEnumerator BuyButtonFeedbackRoutine(int state)
        {
            #region 
           // Debug.Log("State: " + state);
            //-> Not Enough Credits
            if (state == 0)
            { txtBuyInfo.NewTextWithSpecificID(idTextBuyNotEnoughCredits, listTextNotEnoughCredits); }
            //-> ALready Bought
            else if (!customizationAvailable)
            { txtBuyInfo.NewTextWithSpecificID(idTextAlredyBought, listTextAlredyBought); }

            if (state == 0 || !customizationAvailable)
            {
                grpBuyInfo.SetActive(true);

                for (var i = 0; i < 5; i++)
                {
                    float t = 0;
                    float durationB = .25f;

                    while (t < durationB)
                    {
                        t += Time.deltaTime;
                        yield return null;
                    }
                    grpBuyInfo.SetActive(!grpBuyInfo.activeSelf);
                }
            }


            yield return null; 
            #endregion
        }

        public void UnlockVehicle()
        {
            #region 
            VehicleGlobalData vehicleData = DataRef.instance.vehicleGlobalData;
            InfoVehicle infoVehicle = InfoVehicle.instance;

            //-> Use list Order
            int currentVehicle = infoVehicle.currentVehicleDisplayedInTheGarage;
            //-> Use Custom Order
            if (vehicleData.OrderUsingCustomList)
                currentVehicle = vehicleData.customList[infoVehicle.currentVehicleDisplayedInTheGarage];

            InfoCoins.instance.UpdateCoins(-InfoVehicle.instance.vehicleParametersInGameList[currentVehicle].cost);
            InfoVehicle.instance.vehicleParametersInGameList[currentVehicle].isUnlocked = true;

            grpPrice.SetActive(false);
            objLock.SetActive(false);

            if (!customizationAvailable)
                txtOwn.NewTextManageByScript(new List<TextEntry>(1) { new TextEntry(listTextOwn, idTextOwn) }, false);
            else
                txtOwn.NewTextManageByScript(new List<TextEntry>(1) { new TextEntry(listTextCustomize, idTextCustomize) }, false);

            LoadSavePlayerProgession.instance.SavePlayerProgression(); 
            #endregion
        }

        public void InstantiateNewVehicle()
        {
            #region 
            StartCoroutine(InstantiateNewVehicleRoutine()); 
            #endregion
        }

        IEnumerator InstantiateNewVehicleRoutine()
        {
            #region 
            GarageManager gm = GarageManager.instance;
            VehicleGlobalData vehicleData = DataRef.instance.vehicleGlobalData;
            InfoVehicle infoVehicle = InfoVehicle.instance;

            //-> Use list Order
            int currentVehicle = infoVehicle.currentVehicleDisplayedInTheGarage;
            //-> Use Custom Order
            if (vehicleData.OrderUsingCustomList)
                currentVehicle = vehicleData.customList[infoVehicle.currentVehicleDisplayedInTheGarage];

            //-> Check if vehicle is already displayed
            GameObject currentObjVehicle = null;
            if (gm.garageTagPivot && gm.garageTagPivot.transform.childCount > 0)
            { currentObjVehicle = gm.garageTagPivot.transform.GetChild(0).gameObject; }

            if (!gm.garageTagPivot)
            {
                GarageTagPivot[] objs = FindObjectsByType<GarageTagPivot>(FindObjectsSortMode.None);

                for (var i = 0; i < objs.Length; i++)
                {
                    if (objs[i].ID == 0)
                    {
                        gm.garageTagPivot = objs[i];
                    }

                    if (objs[i].ID == 3)
                    {
                        gm.vehicleInstantiatePosition = objs[i].transform;
                    }
                }
            }

            //-> Set the Ref transform depending direction
            Transform startParent = gm.garageTagPivot.backPos;
            Transform endParent = gm.garageTagPivot.frontPos;
            if (gm.direction == -1)
            {
                startParent = gm.garageTagPivot.frontPos;
                endParent = gm.garageTagPivot.backPos;
            }

            //-> instantiate new Vehicle
            if (currentObjVehicle && gm.direction != 0 ||
                !currentObjVehicle && gm.direction == 0 ||
                lastVehicleID != currentVehicle)
            {

                if (currentObjVehicle)
                {
                    if (EnableIsKiniematicWhenNewVehicleSpawned)
                        currentObjVehicle.transform.GetChild(0).GetComponent<Rigidbody>().isKinematic = true;
                    currentObjVehicle.transform.position = endParent.position;
                }


                if (objLoadVehicle) objLoadVehicle.SetActive(true);

                //-> Instantiate the new vehicle
                GameObject newVehicle = Instantiate(InfoVehicle.instance.vehicleParametersInGameList[currentVehicle].Prefab, gm.vehicleInstantiatePosition);      // garageTagPivot.transform);
                float currentGameTime = Time.time;


                float t = 0;
                while (t < gm.duration)
                {
                    t += Time.deltaTime;
                    yield return null;
                }

                yield return new WaitUntil(() => newVehicle.GetComponent<VehiclePrefabInit>().bInitVehicleInfo(1));  // Init for Main menu


                newVehicle.transform.SetParent(startParent);
                newVehicle.transform.position = startParent.position;
                newVehicle.transform.localRotation = Quaternion.identity;

                newVehicle.transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
                CarController car = newVehicle.transform.GetChild(0).GetComponent<CarController>();
                for (var i = 0; i < car.wheelsList.Count; i++)
                    car.wheelsList[i].wheelAxisX.transform.GetChild(0).gameObject.SetActive(false);



                newVehicle.transform.position = gm.garageTagPivot.transform.position;

                yield return new WaitUntil(() => newVehicle.transform.position == gm.garageTagPivot.transform.position);

                yield return new WaitForSeconds(.1f);

                newVehicle.transform.GetChild(0).GetComponent<Rigidbody>().isKinematic = false;


                InitOrbitalCameras(newVehicle);

                yield return new WaitForSeconds(waitDurationAfterVehicleSpawn);

                if (objLoadVehicle) objLoadVehicle.SetActive(false);


                newVehicle.transform.GetChild(0).GetChild(1).gameObject.SetActive(true);
                for (var i = 0; i < car.wheelsList.Count; i++)
                    car.wheelsList[i].wheelAxisX.transform.GetChild(0).gameObject.SetActive(true);


                yield return new WaitUntil(() => newVehicle.GetComponent<VehiclePrefabInit>().b_InitDone);

                //-> Delete the old vehicle 
                if (gm.garageTagPivot.transform.childCount > 0)
                { if (currentObjVehicle) Destroy(currentObjVehicle); }

                newVehicle.transform.SetParent(gm.garageTagPivot.transform);
                newVehicle.transform.localRotation = Quaternion.identity;

                if (EnableIsKiniematicWhenNewVehicleSpawned)
                {

                    newVehicle.transform.GetChild(0).GetComponent<Rigidbody>().constraints =
                       RigidbodyConstraints.FreezePositionX |
                       RigidbodyConstraints.FreezePositionZ |
                       RigidbodyConstraints.FreezeRotationY;
                    newVehicle.transform.GetChild(0).GetComponent<Rigidbody>().isKinematic = false;
                }

                lastVehicleID = currentVehicle;
            }


            //-> IMPORTANT: Next 2 lines closed the process. Always add those 2 lines.
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false;
            gm.bActionAvailable = true;

            yield return null; 
            #endregion
        }

        //-> Change the category of vehicles displayed in the garage
        public void DisplayNewVehicleCategory(int NewCategory)
        {
            #region 
            if (bActionAvailable && !displayVehiclecleThumbnailInProgress)
            {
                InfoVehicle infoVehicle = InfoVehicle.instance;
                if (CurrentCategory == -1)
                    infoVehicle.listSelectedVehiclesInCategory[infoVehicle.listSelectedVehiclesInCategory.Count - 1] = infoVehicle.currentVehicleDisplayedInTheGarage;
                else
                    infoVehicle.listSelectedVehiclesInCategory[CurrentCategory] = infoVehicle.currentVehicleDisplayedInTheGarage;

                CurrentCategory = NewCategory;

                if (NewCategory == -1)
                {
                    // infoVehicle.currentVehicleDisplayedInTheGarage = -1;
                    int newSelectedVehicle = infoVehicle.listSelectedVehiclesInCategory[infoVehicle.listSelectedVehiclesInCategory.Count - 1] - 1 + InfoVehicle.instance.vehicleParametersInGameList.Count * 2;
                    newSelectedVehicle %= InfoVehicle.instance.vehicleParametersInGameList.Count;
                    infoVehicle.currentVehicleDisplayedInTheGarage = newSelectedVehicle;
                }
                else
                {
                    // Debug.Log("CurrentCategory: " + CurrentCategory);
                    int newSelectedVehicle = infoVehicle.listSelectedVehiclesInCategory[CurrentCategory] - 1 + InfoVehicle.instance.vehicleParametersInGameList.Count * 2;
                    newSelectedVehicle %= InfoVehicle.instance.vehicleParametersInGameList.Count;
                    infoVehicle.currentVehicleDisplayedInTheGarage = newSelectedVehicle;
                }

                DisplayVehicleCategory();


                StartCoroutine(DisplayNextVehicleRoutine(1,0));
            } 
            #endregion
        }

        void InitOrbitalCameras(GameObject vehicle)
        {
            #region 
            OrbitalCam[] allOrbitalCam = FindObjectsByType<OrbitalCam>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (OrbitalCam orbitalCam in allOrbitalCam)
            {
                VehiclePrefabInit vehiclePrefabInit = vehicle.GetComponent<VehiclePrefabInit>();

                orbitalCam.InitVariableDependingVehicle(
                    vehiclePrefabInit.garageZoomMinScrollOverride,
                    vehiclePrefabInit.garageZoomMaxScrollOverride,
                    vehiclePrefabInit.orbitalSpecialPosOverride);
            } 
            #endregion
        }

        void DisplayVehicleCategory()
        {
            #region 
            string txt = "";
            if (CurrentCategory == -1)
            {
                txt = LanguageManager.instance.String_ReturnText(0, 198);
            }
            else
            {
                int entryID = DataRef.instance.vehicleGlobalData.VehicleCategoryParamsList[CurrentCategory].EntryID;
                int listID = DataRef.instance.vehicleGlobalData.VehicleCategoryParamsList[CurrentCategory].ListID;
                txt = LanguageManager.instance.String_ReturnText(listID, entryID);
            }

            if (selectedCategory) selectedCategory.DisplayTextComponent(selectedCategory.gameObject, txt); 
            #endregion
        }

        public void DisplayNewVehicleCategoryWithChildID(GarageInitCategoryButton obj)
        {
            int id = obj.category;

            DisplayNewVehicleCategory(id);
        }
    }
}
