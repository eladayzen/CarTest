// Description: SceneInitAssistant: Attached to SceneInitManager object. Methods called to init the scene.
using System.Collections;
using UnityEngine;

//using UnityEngine.Rendering.Universal;

namespace TS.Generics
{
    public class SceneInitAssistant : MonoBehaviour
    {
        private int loadInProgress = 0;

        public bool b_InitDone;
        private bool b_InitInProgress;

        //public UniversalRenderPipelineAsset urpRenderer;

        public bool A_S101_IntroAlreadyLoaded()
        {
            IntroInfo.instance.introAlreadyLoaded = true;
            return true;
        }


        public bool A_S102_InitGameMode()
        {
            #region 
            //-> Play the coroutine Once
            if (!b_InitInProgress)
            {
                b_InitInProgress = true;
                b_InitDone = false;
                int selectedGameMode = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode;
                SceneStepsManager.instance.NextStep(selectedGameMode, 0);
                StartCoroutine(InitGameModeRoutine());
            }
            //-> Check if the coroutine is finished
            else if (b_InitDone)
                b_InitInProgress = false;

            return b_InitDone;
            #endregion
        }

        // Wait until the Game Mode initialization ended (script SceneStepManager.cs)
        IEnumerator InitGameModeRoutine()
        {
            #region
            b_InitDone = false;
            yield return new WaitUntil(() => SceneStepsManager.instance.b_IsSceneStepManagerRunning == false);
            Debug.Log("Init Game Mode Done");
            b_InitDone = true;

            yield return null;
            #endregion
        }

        public bool A_S103_InitAllTexts()
        {
            #region 
            return LanguageManager.instance.Bool_UpdateAllTexts();
            #endregion
        }

        public bool A_S104_DisplayMenuDependingGameMode()
        {
            #region
            //-> Play the coroutine Once
            if (loadInProgress == 0)
            {
                loadInProgress = 1;
                StartCoroutine(DisplayMenuDependingGameModeRoutine());
            }
            //-> Check if the coroutine is finished
            else if (loadInProgress == 2)
                loadInProgress = 3;

            if (loadInProgress == 3)
            {
                loadInProgress = 0;
                return true;
            }
            else
                return false;
            #endregion
        }

        //-> 
        public IEnumerator DisplayMenuDependingGameModeRoutine()
        {
            #region
            Debug.Log("Open Page Starts");
            SceneStepsManager.instance.NextStep(4, 0);  //  Main Menu step sequence
            yield return new WaitForEndOfFrame();
           
                 //yield return new WaitUntil(() => CanvasLoadingManager.instance.b_CanvasLoadingIsDisplayed == false);
            yield return new WaitUntil(() => InfoPlayerTS.instance.returnCheckState(0) == true);
            loadInProgress = 2;
            yield return null;
            #endregion
        }

        public bool A_S105_InstantiateAnAirplane(GameObject planePrefab)
        {
            #region
            GameObject newPlane = Instantiate(planePrefab);
            newPlane.GetComponent<VehiclePrefabInit>().bInitVehicleInfo(0);
            return true;
            #endregion
        }

        public bool AllowTransition()
        {
            #region 
            //-> Prevent to play a transition when the scene is launched
            SceneInitManager.instance.bAllowTransition = true;
            return true;
            #endregion
        }

        public bool UpdateCoin()
        {
            #region 
            InfoCoins.instance.UpdateCoins();
            return true;
            #endregion
        }

        public bool URPShadowDistanceHundred()
        {
            #region 
            //if(urpRenderer)urpRenderer.shadowDistance = 100;
            return true;
            #endregion
        }

        public bool URPShadowDistanceThousand()
        {
            #region 
            //if (urpRenderer) urpRenderer.shadowDistance = 1000;
            return true;
            #endregion
        }

        public bool URPShadowDistanceCustom(int value = 1000)
        {
            #region 
            //if (urpRenderer) urpRenderer.shadowDistance = value;
            return true;
            #endregion
        }

        public bool ModeModeFeedback()
        {
            #region 
            if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 5)
            {
                Debug.Log("INFO TS: Mode Five is enabled. It is automatically change to mode 0 (Arcade) to prevent issue.");
                InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode = 0;
            }

            return true;
            #endregion
        }

        public bool RemoveVehiclesOnHierarchy()
        {
            #region
            //-> Play the coroutine Once
            if (loadInProgress == 0)
            {
                loadInProgress = 1;
                StartCoroutine(RemoveVehiclesOnHierarchyRoutine());
            }
            //-> Check if the coroutine is finished
            else if (loadInProgress == 2)
                loadInProgress = 3;

            if (loadInProgress == 3)
            {
                loadInProgress = 0;
                return true;
            }
            else
                return false;
            #endregion
        }

        //-> 
        IEnumerator RemoveVehiclesOnHierarchyRoutine()
        {
            #region
            if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode != 5)
            {
                VehiclePrefabInit[] vehicles = FindObjectsByType<VehiclePrefabInit>(FindObjectsSortMode.None);
                Debug.Log("Remove " + vehicles.Length + " Vehicle");

                foreach (VehiclePrefabInit vehicle in vehicles)
                    Destroy(vehicle.gameObject);

                yield return new WaitUntil(() => HowManyVehicleInTheScene() == 0);

            }
            loadInProgress = 2;
            yield return null;
            #endregion
        }

        int HowManyVehicleInTheScene()
        {
            VehiclePrefabInit[] vehicles = FindObjectsByType<VehiclePrefabInit>(FindObjectsSortMode.None);
            return vehicles.Length;
        }

        public bool EnablePlayerInputs()
        {
            if (InfoPlayerTS.instance)
            {
                InfoPlayerTS.instance.b_IsAvailableToDoSomething = true;
            }
            return true;
        }
    }
}

