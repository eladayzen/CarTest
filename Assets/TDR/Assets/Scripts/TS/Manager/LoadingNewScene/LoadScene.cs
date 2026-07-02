// Desciption: LoadScene: Allows to load a new scene (async)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

namespace TS.Generics
{
    public class LoadScene : MonoBehaviour
    {

        public static LoadScene     instance = null;
        
        public bool                 SeeInspector;
        [HideInInspector]
        public bool                 moreOption;
        [HideInInspector]
        public bool                 b_EverythingDoneBeforeLoading;
        [HideInInspector]
        public bool                 b_IsLoadingFinished = true;

        [HideInInspector]
        public bool                 helpBox = true;

        [HideInInspector]
        public int                  newScene = 0;            

        public List<EditorMethodsList_Pc.MethodsList> methodsList       // Create a list of Custom Methods that could be edit in the Inspector
       = new List<EditorMethodsList_Pc.MethodsList>();

        public CallMethods_Pc callMethods;                              // Access script taht allow to call public function in this script.

        [System.Serializable]
        public class C_MethodLists
        {
            public List<EditorMethodsList_Pc.MethodsList> methodsList       // Create a list of Custom Methods that could be edit in the Inspector
                = new List<EditorMethodsList_Pc.MethodsList>();
        }

        public List<C_MethodLists> multiMethodList = new List<C_MethodLists>();

        public bool initAutomaticallyNewScene = true;
        public UnityEvent initNewSceneEvent;

        public List<int> forceInitIfInTheList = new List<int>();   // If the current loaded scene is in the list: InitSceneRoutine() is called

        void Awake()
        {
            #region 
            //-> Check if instance already exists
            if (instance == null)
                instance = this;

            //-> If instance already exists and it's not this:
            else if (instance != this)
                Destroy(gameObject); 
            #endregion
        }

        public void LoadSceneWithSceneNumberAndSpecificCustomMethodList(int whichSceneToLoad, int whichMethodist = 0)
        {
            #region 
            if (b_IsLoadingFinished)
                StartCoroutine(LoadYourAsyncSceneWithSpecificCustomMethodList(whichSceneToLoad, whichMethodist)); 
            #endregion
        }

        public void LoadSceneWithSceneNameAndSpecificCustomMethodList(string sceneName = "", int whichMethodist = 0,int index = -1)
        {
            #region
            if (b_IsLoadingFinished)
                StartCoroutine(LoadYourAsyncSceneWithSpecificCustomMethodList(index, whichMethodist, sceneName)); 
            #endregion
        }

        // Load scene async
        IEnumerator LoadYourAsyncSceneWithSpecificCustomMethodList(int index, int whichMethodist,string sceneName = "")
        {
            #region 
            newScene = index;
            b_IsLoadingFinished = false;

            yield return new WaitForEndOfFrame();
            yield return new WaitUntil(() => !b_IsLoadingFinished);

            StartCoroutine(CallAllTheMethodsOneByOneWithSpecificCustomMethodList(whichMethodist));

            yield return new WaitUntil(() => b_EverythingDoneBeforeLoading == true);

            AsyncOperation asyncLoad;
            // Case: Use the name of the scene
            if (index == -1)
                asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            else
                asyncLoad = SceneManager.LoadSceneAsync(index);

            CurrentText txtLoadingProgression = CanvasLoadingManager.instance.txtLoadingProgression;

            while (!asyncLoad.isDone) {
                if (txtLoadingProgression)
                    txtLoadingProgression.DisplayTextComponent(txtLoadingProgression.gameObject, Mathf.Round(asyncLoad.progress * 100) + "%");
                yield return null; 
            }

            Debug.Log("Loading Scene " + index + "with list " + whichMethodist + "is finished");

            yield return new WaitForEndOfFrame();

            if (txtLoadingProgression)
                txtLoadingProgression.DisplayTextComponent(txtLoadingProgression.gameObject, "");
          

            b_IsLoadingFinished = true;

            if (initAutomaticallyNewScene || IsSceneInTheList())
                initNewSceneEvent?.Invoke();

            yield return null; 
            #endregion
        }

        //-> Call all the methods in the list 
        IEnumerator CallAllTheMethodsOneByOneWithSpecificCustomMethodList(int whichMethodist)
        {
            #region
            b_EverythingDoneBeforeLoading = false;

            for (var i = 0; i < multiMethodList[whichMethodist].methodsList.Count; i++)
            {
                yield return new WaitUntil(() => callMethods.Call_One_Bool_Method(multiMethodList[whichMethodist].methodsList, i) == true);
            }

            b_EverythingDoneBeforeLoading = true;

            yield return null;
            #endregion
        }

        public void InitScene()
        {
            #region 
            StartCoroutine(InitSceneRoutine()); 
            #endregion
        }

        public IEnumerator InitSceneRoutine()
        {
            #region 
            //-> Init Scene
            SceneInitManager.instance.b_IsInitDone = false;
            StartCoroutine(SceneInitManager.instance.CallAllTheMethodsOneByOne());

            //-> Wait until the init is finished
            yield return new WaitUntil(() => SceneInitManager.instance.b_IsInitDone == true);

            //-> Close Canvas Loading
            StartCoroutine(CanvasLoadingManager.instance.CloseCanvasLoading());

            yield return null; 
            #endregion
        }

       bool IsSceneInTheList()
        {
            #region 
            for (var i = 0; i < forceInitIfInTheList.Count; i++)
            {
                if (forceInitIfInTheList[i] == newScene)
                {
                    return true;
                }
            }

            return false; 
            #endregion
        }
    }
}
