//Desciption: ButtonNavigation.cs. Attached to buttons. Used to "Animate" the buttons
// depending the button state: selected, PointerEnter, Exit, CLick ....
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace TS.Generics
{
    public class ButtonNavigation : MonoBehaviour,
                                    IPointerEnterHandler,
                                    IPointerExitHandler,
                                    IPointerClickHandler,
                                    ISelectHandler,
                                    IDeselectHandler,
                                    IPointerDownHandler,
                                    IPointerUpHandler
    {
        [HideInInspector]
        public bool             SeeInspector;
        [HideInInspector]
        public bool             moreOptions;
        [HideInInspector]
        public bool             helpBox = true;

        public bool             b_AutoSelect = true;
        public bool             b_ClicSoundSelect = true;
        public int              clicSoundID = 0;

        public bool             b_OnClicSound = true;
        public int              onClicSoundIDAllowed = 1;
        public int              onClicSoundIDWrong = 4;
        private float           volumeClic = .5f;

        public bool             b_ScaleSelect = true;
        private Vector3         refScale = new Vector3(1,1,1);
        public Vector3          scale = new Vector3(1.1f, 1.1f,1);
        public float            scaleSpeed = 2;
      
        public CanvasGroup      canvasGroup;

        public UnityEvent       eventInit;

        public UnityEvent       eventEnter;
        public UnityEvent       eventExit;

        public UnityEvent       eventPointerDown;
        public UnityEvent       eventPointerUp;

        private ButtonCustom    buttonCustom;

        
        void Start()
        {
            #region
            buttonCustom = GetComponent<ButtonCustom>();

            refScale = gameObject.GetComponent<RectTransform>().localScale;

            // Find the button parent that contain the component CanvasGroup
            Transform objWithCanvasGroup = gameObject.transform.parent;
            if (gameObject.transform.parent != null)
            {
                while (canvasGroup == null)
                {
                    if (objWithCanvasGroup.GetComponent<CanvasGroup>())
                    { canvasGroup = objWithCanvasGroup.GetComponent<CanvasGroup>(); }
                    else
                    {
                        if (objWithCanvasGroup.transform.parent != null)
                        { objWithCanvasGroup = objWithCanvasGroup.transform.parent; }
                        else
                            break;
                    }
                }
            }

            Button btn = GetComponent<Button>();
            if (btn) btn.onClick.AddListener(OnClickTS);


            eventInit.Invoke(); 
            #endregion
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            #region 
            // Desktop | Other platform
            if (IntroInfo.instance.globalDatas.returnButtonNavigationAllowed() &&
                Cursor.visible)
            {
                if (TS_EventSystem.instance.eventSystem.currentSelectedGameObject != this.gameObject)
                {
                    if (b_AutoSelect && InfoPlayerTS.instance.returnCheckState(0)
                        && !CanvasLoadingManager.instance.b_CanvasLoadingIsDisplayed)
                    {
                        SetSelected();
                    }
                }
            }  
            #endregion
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            #region 
            // Desktop | Other platform
            if (IntroInfo.instance.globalDatas.returnButtonNavigationAllowed() &&
                    InfoPlayerTS.instance.returnCheckState(0) 
                    && !CanvasLoadingManager.instance.b_CanvasLoadingIsDisplayed
                && Cursor.visible)
            {
            } 
            #endregion
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //OnClickTS();
        }

        // Button is selected.
        public void OnSelect(BaseEventData eventData)
        {
            #region
            // Desktop | Other platform
            if (IntroInfo.instance.globalDatas.returnButtonNavigationAllowed())
            {
                if (b_ClicSoundSelect &&
                    InfoPlayerTS.instance.returnCheckState(0) &&
                    !CanvasLoadingManager.instance.b_CanvasLoadingIsDisplayed
                    && SceneInitManager.instance.b_IsInitDone)
                    SoundFxManager.instance.Play(SfxList.instance.listAudioClip[clicSoundID], volumeClic);
                //StartCoroutine(Test());
                //TypoSize0();
                if (b_ScaleSelect)
                {
                    StopAllCoroutines();
                    StartCoroutine(ChangeScaleRoutine(scale));

                }

                eventEnter.Invoke();
            } 
            #endregion
        }

        public void OnDeselect(BaseEventData eventData)
        {
            #region
            // Desktop | Other platform
            if (IntroInfo.instance.globalDatas.returnButtonNavigationAllowed() &&
                    !InfoPlayerTS.instance.b_ProcessToDisplayNewPageInProgress)
            {
                if (b_ScaleSelect)
                {
                    StopAllCoroutines();
                    StartCoroutine(ChangeScaleRoutine(refScale));

                }
                eventExit.Invoke();
            } 
            #endregion
        }

        public void SetSelected()
        {
            #region 
            TS_EventSystem.instance.eventSystem.SetSelectedGameObject(this.gameObject); 
            #endregion
        } 
      
        IEnumerator ChangeScaleRoutine(Vector3 target)
        {
            #region 
            RectTransform rectTrans = gameObject.GetComponent<RectTransform>();
            while (rectTrans.localScale != target)
            {
                rectTrans.localScale = Vector3.MoveTowards(rectTrans.localScale, target, Time.deltaTime * scaleSpeed);

                yield return null;
            }

            yield return null; 
            #endregion
        }

        public void OnPointerDown(PointerEventData pointerEventData)
        {
            #region 
            if (IntroInfo.instance.globalDatas.returnButtonNavigationAllowed() &&
                   Cursor.visible)
            {
                eventPointerDown.Invoke();
            } 
            #endregion
        }

        public void OnPointerUp(PointerEventData pointerEventData)
        {
            #region 
            if (IntroInfo.instance.globalDatas.returnButtonNavigationAllowed() &&
                   Cursor.visible)
            {
                eventPointerUp.Invoke();
            } 
            #endregion
        }

        public void OnClickTS()
        {
            #region 
            if (canvasGroup.interactable)
            {
                if (buttonCustom && b_OnClicSound && gameObject.activeInHierarchy)
                {
                    StartCoroutine(buttonCustom.CallAllTheMethodsOneByOne((checkCondition) =>
                    {
                        if (checkCondition)
                            SoundFxManager.instance.Play(SfxList.instance.listAudioClip[onClicSoundIDAllowed], volumeClic);
                        else
                            SoundFxManager.instance.Play(SfxList.instance.listAudioClip[onClicSoundIDWrong], volumeClic);
                    }));
                }
                else if (b_OnClicSound) SoundFxManager.instance.Play(SfxList.instance.listAudioClip[onClicSoundIDAllowed], volumeClic);

            } 
            #endregion
        }
    }

}
