#if (UNITY_EDITOR)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace TS.Generics
{
    [Overlay(typeof(SceneView), id: ID_OVERLAY, displayName: "")]
    public class PathToolOverlay : Overlay, ITransientOverlay
    {
        private const string    ID_OVERLAY = "HandlesCurves";

        SerializedObject        serializedObject;

        SerializedProperty      m_pointsList;
        SerializedProperty      m_closestPoint;
        SerializedProperty      m_totalDistance;
        SerializedProperty      m_distVecList;
        SerializedProperty      m_loop;

        SerializedProperty      m_toolbarScaleHandleLenght;
        SerializedProperty      m_toolbarRotationY;
        SerializedProperty      m_toolbarRotationZ;
        SerializedProperty      m_toolbarTransformX;
        SerializedProperty      m_toolbarTransformY;
        SerializedProperty      m_toolbarTransformZ;
        SerializedProperty      m_sliderMaxValue;
        bool isProcessDone = true;

        public bool visible
        {
            #region 
            get
            {
                return Selection.activeGameObject != null &&
                    Selection.activeGameObject.activeSelf &&
                    Selection.activeGameObject.GetComponent<Bezier>();
            } 
            #endregion
        }

        bool InitValues()
        {
            #region
            if (Selection.activeGameObject)
            {
                serializedObject        = new UnityEditor.SerializedObject(Selection.activeGameObject.GetComponent<Bezier>());

                m_pointsList            = serializedObject.FindProperty("pointsList");
                m_closestPoint          = serializedObject.FindProperty("closestPoint");
                m_totalDistance         = serializedObject.FindProperty("totalDistance");
                m_distVecList           = serializedObject.FindProperty("distVecList");
                m_loop                  = serializedObject.FindProperty("loop");

                m_toolbarScaleHandleLenght = serializedObject.FindProperty("toolbarScaleHandle_Lenght");
                m_toolbarRotationY      = serializedObject.FindProperty("toolbarRotation_Y");
                m_toolbarRotationZ      = serializedObject.FindProperty("toolbarRotation_Z");
                m_toolbarTransformX     = serializedObject.FindProperty("toolbarTransform_X");
                m_toolbarTransformY     = serializedObject.FindProperty("toolbarTransform_Y");
                m_toolbarTransformZ     = serializedObject.FindProperty("toolbarTransform_Z");

                m_sliderMaxValue = serializedObject.FindProperty("sliderMaxValue");
            }
        
            return true; 
            #endregion
        }

        void SelectionHasChanged()
        {
            #region 
            if (Selection.activeGameObject && Selection.activeGameObject.GetComponent<Bezier>())
            {
                //Debug.Log("Selection has Changed: " + Selection.activeGameObject.name);
                InitValues();
            } 
            #endregion
        }

        public override VisualElement CreatePanelContent()
        {
            #region 
            Selection.selectionChanged += () =>
            {
                SelectionHasChanged();
            };

            while (!InitValues()) ;

            Label currentLabel = new Label(text: "Default Name");

            var root = new VisualElement();

            var rootLength = new VisualElement();
            rootLength.style.flexDirection = FlexDirection.Row;

            var titleLabel = new Label(text: "Handle Length");
            titleLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            root.Add(titleLabel);

            titleLabel = new Label(text: "");
            titleLabel.style.width = 15;
            rootLength.Add(titleLabel);

            rootLength.Add(DecreaseHandleLengthButton());
            rootLength.Add(IncreaseHandleLengthButton());

            rootLength.Add(IntegerSliderFloatExample(m_toolbarScaleHandleLenght));
            root.Add(rootLength);

            titleLabel = new Label(text: "Rotation");
            titleLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            root.Add(titleLabel);
            var rootRotationY = new VisualElement();
            rootRotationY.style.flexDirection = FlexDirection.Row;
            titleLabel = new Label(text: "Y");
            titleLabel.style.width = 15;
            titleLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            rootRotationY.Add(titleLabel);

            rootRotationY.Add(NegativeYRotationButton());
            rootRotationY.Add(PositiveYRotationButton());

            rootRotationY.Add(IntegerSliderFloatExample(m_toolbarRotationY));
            root.Add(rootRotationY);

            var rootRotationZ = new VisualElement();
            rootRotationZ.style.flexDirection = FlexDirection.Row;
            titleLabel = new Label(text: "Z");
            titleLabel.style.width = 15;
            titleLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            rootRotationZ.Add(titleLabel);

            rootRotationZ.Add(NegativeZRotationButton());
            rootRotationZ.Add(PositiveZRotationButton());
            rootRotationZ.Add(IntegerSliderFloatExample(m_toolbarRotationZ));

            root.Add(rootRotationZ);

            var rootUpdateButton = new VisualElement();

            rootUpdateButton.Add(VEUpdateButton());

            titleLabel = new Label(text: "Transform");
            titleLabel.style.width = 15;
            root.Add(titleLabel);
            var rootPosX = new VisualElement();
            rootPosX.style.flexDirection = FlexDirection.Row;
            titleLabel = new Label(text: "X");
            titleLabel.style.width = 15;
            rootPosX.Add(titleLabel);
            rootPosX.Add(NewPositionButton(0, -1, "-"));
            rootPosX.Add(NewPositionButton(0,1,"+"));
            rootPosX.Add(IntegerSliderFloatExample(m_toolbarTransformX));
            root.Add(rootPosX);

            var rootPosY = new VisualElement();
            rootPosY.style.flexDirection = FlexDirection.Row;
            titleLabel = new Label(text: "Y");
            titleLabel.style.width = 15;
            rootPosY.Add(titleLabel);
            rootPosY.Add(NewPositionButton(1, -1, "-"));
            rootPosY.Add(NewPositionButton(1, 1, "+"));
            rootPosY.Add(IntegerSliderFloatExample(m_toolbarTransformY));
            root.Add(rootPosY);

            var rootPosZ = new VisualElement();
            rootPosZ.style.flexDirection = FlexDirection.Row;
            titleLabel = new Label(text: "Z");
            titleLabel.style.width = 15;
            rootPosZ.Add(titleLabel);
            rootPosZ.Add(NewPositionButton(2, -1, "-"));
            rootPosZ.Add(NewPositionButton(2, 1, "+"));
            rootPosZ.Add(IntegerSliderFloatExample(m_toolbarTransformZ));
            root.Add(rootPosZ);

            root.Add(rootUpdateButton);

            return root; 
            #endregion
        }

        VisualElement IncreaseHandleLengthButton()
        {
            #region 
            VisualElement root = new VisualElement();

            Button button = new Button();
            button.text = "+";
            button.style.width = 20;
            button.clicked += IncreaseHandle;
            return button; 
            #endregion
        }

        void IncreaseHandle()
        {
            #region
            //Debug.Log("IncreaseHandle");
            ModifyHandleLength(m_toolbarScaleHandleLenght.floatValue);
            #endregion
        }

        VisualElement DecreaseHandleLengthButton()
        {
            #region 
            VisualElement root = new VisualElement();

            Button button = new Button();
            button.text = "-";
            button.style.width = 20;
            button.clicked += DecreaseHandle;
            return button; 
            #endregion
        }

        void DecreaseHandle()
        {
            #region
            ModifyHandleLength(-m_toolbarScaleHandleLenght.floatValue);
            #endregion
        }

        VisualElement PositiveYRotationButton()
        {
            #region 
            VisualElement root = new VisualElement();

            Button button = new Button();
            button.text = "+";
            button.style.width = 20;
            button.clicked += PositiveYRotation;
            return button;
            #endregion
        }

        void PositiveYRotation()
        {
            #region
            //Debug.Log("IncreaseHandle");
            ModifyHandleRotation(m_toolbarRotationY.floatValue,Vector3.up);
            #endregion
        }
       
        VisualElement NegativeYRotationButton()
        {
            #region 
            VisualElement root = new VisualElement();

            Button button = new Button();
            button.text = "-";
            button.style.width = 20;
            button.clicked += NegativeYRotation;
            return button;
            #endregion
        }

        void NegativeYRotation()
        {
            #region
            //Debug.Log("IncreaseHandle");
            ModifyHandleRotation(-m_toolbarRotationY.floatValue, Vector3.up);
            #endregion
        }
       
        VisualElement PositiveZRotationButton()
        {
            #region 
            VisualElement root = new VisualElement();

            Button button = new Button();
            button.text = "+";
            button.style.width = 20;
            button.clicked += PositiveZRotation;
            return button;
            #endregion
        }

        void PositiveZRotation()
        {
            #region
            //Debug.Log("IncreaseHandle");
            ModifyHandleRotation(m_toolbarRotationZ.floatValue, Vector3.left);
            #endregion
        }

        VisualElement NegativeZRotationButton()
        {
            #region 
            VisualElement root = new VisualElement();

            Button button = new Button();
            button.text = "-";
            button.style.width = 20;
            button.clicked += NegativeZRotation;
            return button;
            #endregion
        }

        void NegativeZRotation()
        {
            #region
            //Debug.Log("IncreaseHandle");
            ModifyHandleRotation(-m_toolbarRotationZ.floatValue, Vector3.left);
            #endregion
        }
   
        void ModifyHandleLength(float value = -5)
        {
            #region
            GameObject currentSelection = Selection.activeGameObject;
            serializedObject.Update();
            Vector3 pointPos = SListPoint(m_closestPoint.intValue);
            Vector3 pointPosHandleLeft = SListPoint(m_closestPoint.intValue - 1);
            Vector3 dirL = (pointPosHandleLeft - pointPos).normalized;
            Vector3 pointPosHandleRIght = SListPoint(m_closestPoint.intValue + 1);
            Vector3 dirR = (pointPosHandleLeft - pointPos).normalized;

            Vector3 newPointPosHandleLeft = pointPosHandleLeft + dirL * value;
            Vector3 newPointPosHandleRight = pointPosHandleRIght - dirR * value;

            if(m_closestPoint.intValue != -1)
            {
                if (m_loop.boolValue && m_closestPoint.intValue == 0)
                {
                    pointPosHandleLeft = SListPoint(m_closestPoint.intValue - 2);
                    dirL = (pointPosHandleLeft - pointPos).normalized;
                    newPointPosHandleLeft = pointPosHandleLeft + dirL * value;

                    pointPosHandleRIght = SListPoint(1);
                     dirR = (pointPosHandleLeft - pointPos).normalized;
                     newPointPosHandleRight = pointPosHandleRIght - dirR * value;

                    m_pointsList.GetArrayElementAtIndex(Points(m_closestPoint.intValue - 2)).FindPropertyRelative("points").vector3Value
                             = newPointPosHandleLeft - currentSelection.GetComponent<Bezier>().transform.position;
                    //Debug.Log("Here:" + Points(m_closestPoint.intValue +1));
                    m_pointsList.GetArrayElementAtIndex(Points(1)).FindPropertyRelative("points").vector3Value
                          = newPointPosHandleRight - currentSelection.GetComponent<Bezier>().transform.position;

                }
                else
                {
                    if (m_closestPoint.intValue == 0)
                    {
                        pointPosHandleRIght = SListPoint(1);
                        dirR = (pointPosHandleRIght - pointPos).normalized;
                        newPointPosHandleRight = pointPosHandleRIght + dirR * value;
                        m_pointsList.GetArrayElementAtIndex(Points(1)).FindPropertyRelative("points").vector3Value
                                = newPointPosHandleRight - currentSelection.GetComponent<Bezier>().transform.position;
                    }
                    else
                    {

                        if (m_closestPoint.intValue >= 0)
                            m_pointsList.GetArrayElementAtIndex(Points(m_closestPoint.intValue - 1)).FindPropertyRelative("points").vector3Value
                                = newPointPosHandleLeft - currentSelection.GetComponent<Bezier>().transform.position;

                        if (m_closestPoint.intValue < Points(m_closestPoint.intValue + 1))
                            m_pointsList.GetArrayElementAtIndex(Points(m_closestPoint.intValue + 1)).FindPropertyRelative("points").vector3Value
                                = newPointPosHandleRight - currentSelection.GetComponent<Bezier>().transform.position;
                    }

                }
              
            }

            serializedObject.ApplyModifiedProperties();
            #endregion
        }

        void ModifyHandleRotation(float value,Vector3 rotationAxis)
        {
            #region
            GameObject currentSelection = Selection.activeGameObject;

            serializedObject.Update();

            Vector3 pointPos = SListPoint(m_closestPoint.intValue);
            Vector3 pointPosHandleLeft = SListPoint(m_closestPoint.intValue - 1);
            Vector3 dirL = (pointPosHandleLeft - pointPos).normalized;
            Vector3 pointPosHandleRIght = SListPoint(m_closestPoint.intValue + 1);

            Vector3 forward = Vector3.Cross(dirL, rotationAxis).normalized;

            Vector3 newPointPosHandleLeft = pointPosHandleLeft + forward * value;
            Vector3 newPointPosHandleRight = pointPosHandleRIght - forward * value;

           

            if (m_closestPoint.intValue != -1)
            {
                if (m_loop.boolValue && m_closestPoint.intValue == 0)
                {
                    pointPosHandleLeft = SListPoint(m_closestPoint.intValue - 2);
                    dirL = (pointPosHandleLeft - pointPos).normalized;

                    pointPosHandleRIght = SListPoint(1);
                    float distanceR = Vector3.Distance(pointPos, pointPosHandleRIght);


                    forward = Vector3.Cross(dirL, rotationAxis).normalized;

                     newPointPosHandleLeft = pointPosHandleLeft + forward * value;
                     newPointPosHandleRight = pointPosHandleRIght - forward * value;


                    m_pointsList.GetArrayElementAtIndex(Points(m_closestPoint.intValue - 2)).FindPropertyRelative("points").vector3Value 
                        = newPointPosHandleLeft - currentSelection.GetComponent<Bezier>().transform.position;
                    m_pointsList.GetArrayElementAtIndex(Points(1)).FindPropertyRelative("points").vector3Value 
                        = newPointPosHandleRight - currentSelection.GetComponent<Bezier>().transform.position;

                    // Update Lenght
                    pointPosHandleLeft = SListPoint(m_closestPoint.intValue - 2);
                    pointPosHandleRIght = SListPoint(1);
                    dirL = (pointPosHandleLeft - SListPoint(0)).normalized;
                    newPointPosHandleLeft = pointPos + dirL * distanceR;
                    newPointPosHandleRight = pointPos - dirL * distanceR;
                    m_pointsList.GetArrayElementAtIndex(Points(m_closestPoint.intValue - 2)).FindPropertyRelative("points").vector3Value = newPointPosHandleLeft - currentSelection.GetComponent<Bezier>().transform.position;

                    m_pointsList.GetArrayElementAtIndex(Points(m_closestPoint.intValue + 1)).FindPropertyRelative("points").vector3Value = newPointPosHandleRight - currentSelection.GetComponent<Bezier>().transform.position;


                }
                else
                {
                    if (m_closestPoint.intValue == 0)
                    {
                      
                        pointPosHandleRIght = SListPoint(1);
                        float distanceR = Vector3.Distance(pointPos, pointPosHandleRIght);

                        Vector3 dirR = (pointPosHandleRIght - pointPos).normalized;
                        forward = Vector3.Cross(dirR, rotationAxis).normalized;
                        newPointPosHandleRight = pointPosHandleRIght + forward * value;
                        m_pointsList.GetArrayElementAtIndex(Points(1)).FindPropertyRelative("points").vector3Value
                                = newPointPosHandleRight - currentSelection.GetComponent<Bezier>().transform.position;

                        // Update Lenght

                        pointPosHandleRIght = SListPoint(1);
                        dirL = (pointPosHandleRIght - pointPos).normalized;
                        newPointPosHandleRight = pointPos + dirL * distanceR;
                        m_pointsList.GetArrayElementAtIndex(Points(1)).FindPropertyRelative("points").vector3Value 
                            = newPointPosHandleRight - currentSelection.GetComponent<Bezier>().transform.position;

                    }
                    else
                    {
                        float distanceL = Vector3.Distance(pointPos, pointPosHandleLeft);
                        //float distanceR = Vector3.Distance(pointPos, pointPosHandleLeft);

                        // Rotate
                        if (m_closestPoint.intValue > 0)
                            m_pointsList.GetArrayElementAtIndex(Points(m_closestPoint.intValue - 1)).FindPropertyRelative("points").vector3Value = newPointPosHandleLeft - currentSelection.GetComponent<Bezier>().transform.position;

                        if (m_closestPoint.intValue < Points(m_closestPoint.intValue + 1))
                            m_pointsList.GetArrayElementAtIndex(Points(m_closestPoint.intValue + 1)).FindPropertyRelative("points").vector3Value = newPointPosHandleRight - currentSelection.GetComponent<Bezier>().transform.position;



                        // Update Lenght
                        pointPosHandleLeft = SListPoint(m_closestPoint.intValue - 1);
                        dirL = (pointPosHandleLeft - pointPos).normalized;
                        newPointPosHandleLeft = pointPos + dirL * distanceL;
                        newPointPosHandleRight = pointPos - dirL * distanceL;
                        if (m_closestPoint.intValue > 0)
                            m_pointsList.GetArrayElementAtIndex(Points(m_closestPoint.intValue - 1)).FindPropertyRelative("points").vector3Value = newPointPosHandleLeft - currentSelection.GetComponent<Bezier>().transform.position;

                        if (m_closestPoint.intValue < Points(m_closestPoint.intValue + 1))
                            m_pointsList.GetArrayElementAtIndex(Points(m_closestPoint.intValue + 1)).FindPropertyRelative("points").vector3Value = newPointPosHandleRight - currentSelection.GetComponent<Bezier>().transform.position;



                    }
                }

            }
            serializedObject.ApplyModifiedProperties();
            #endregion
        }

        VisualElement NewPositionButton(int Axis = 0, int positiveValue = 1,string label = "+")
        {
            #region 
            VisualElement root = new VisualElement();

            Button button = new Button();
            button.text = label;
            button.style.width = 20;
           // button.clicked += PositivePos;
            button.RegisterCallback<ClickEvent>((evt) => UpdatePosition(Axis, positiveValue));
            return button;
            #endregion
        }

        void UpdatePosition(int Axis = 0, int positiveValue = 1)
        {
            #region
            //Debug.Log("IncreaseHandle");
            switch (Axis)
            {
                case 0:
                    ModifyHandleTranslation(m_toolbarTransformX.floatValue, Axis, positiveValue);
                    break;
                case 1:
                    ModifyHandleTranslation(m_toolbarTransformY.floatValue, Axis, positiveValue);
                    break;
                case 2:
                    ModifyHandleTranslation(m_toolbarTransformZ.floatValue, Axis, positiveValue);
                    break;
            }
            #endregion
        }

        void ModifyHandleTranslation(float value = 1, int Axis = 0, int positiveValue = 1)
        {
            #region
            GameObject currentSelection = Selection.activeGameObject;
            serializedObject.Update();
           

            Vector3 newPointPosHandle = Vector3.zero;

            switch (Axis)
            {
                case 0:
                    newPointPosHandle += positiveValue * Vector3.right * value;
                    break;
                case 1:
                    newPointPosHandle += positiveValue * Vector3.up * value;
                    break;
                case 2:
                    newPointPosHandle += positiveValue * Vector3.forward * value;
                    break;
            }

            if (m_closestPoint.intValue != -1)
            {
                if (m_loop.boolValue && m_closestPoint.intValue == 0)
                {
                    Vector3 pointPos = SListPoint(0);

                    m_pointsList.GetArrayElementAtIndex(Points(0)).FindPropertyRelative("points").vector3Value
                             = pointPos + newPointPosHandle - currentSelection.GetComponent<Bezier>().transform.position;

                    pointPos = SListPoint(1);
                    m_pointsList.GetArrayElementAtIndex(Points(1)).FindPropertyRelative("points").vector3Value
                          = pointPos + newPointPosHandle - currentSelection.GetComponent<Bezier>().transform.position;

                    pointPos = SListPoint(m_closestPoint.intValue - 1);
                    m_pointsList.GetArrayElementAtIndex(Points(m_closestPoint.intValue - 1)).FindPropertyRelative("points").vector3Value
                          = pointPos + newPointPosHandle - currentSelection.GetComponent<Bezier>().transform.position;

                    pointPos = SListPoint(m_closestPoint.intValue - 2);
                    m_pointsList.GetArrayElementAtIndex(Points(m_closestPoint.intValue - 2)).FindPropertyRelative("points").vector3Value
                        = pointPos + newPointPosHandle - currentSelection.GetComponent<Bezier>().transform.position;
                }
                else
                {
                    if (m_closestPoint.intValue == 0)
                    {
                        Vector3 pointPos = SListPoint(0);

                        m_pointsList.GetArrayElementAtIndex(Points(0)).FindPropertyRelative("points").vector3Value
                                 = pointPos + newPointPosHandle - currentSelection.GetComponent<Bezier>().transform.position;

                        pointPos = SListPoint(1);
                        m_pointsList.GetArrayElementAtIndex(Points(1)).FindPropertyRelative("points").vector3Value
                              = pointPos + newPointPosHandle - currentSelection.GetComponent<Bezier>().transform.position;
                    }
                    else if (m_closestPoint.intValue == m_pointsList.arraySize - 1)
                    {
                        Vector3 pointPos = SListPoint(m_closestPoint.intValue);

                        m_pointsList.GetArrayElementAtIndex(Points(m_closestPoint.intValue)).FindPropertyRelative("points").vector3Value
                                 = pointPos + newPointPosHandle - currentSelection.GetComponent<Bezier>().transform.position;

                        pointPos = SListPoint(m_closestPoint.intValue - 1);
                        m_pointsList.GetArrayElementAtIndex(Points(m_closestPoint.intValue - 1)).FindPropertyRelative("points").vector3Value
                              = pointPos + newPointPosHandle - currentSelection.GetComponent<Bezier>().transform.position;
                    }
                    else
                    {
                        Vector3 pointPos = SListPoint(m_closestPoint.intValue);

                        m_pointsList.GetArrayElementAtIndex(Points(m_closestPoint.intValue)).FindPropertyRelative("points").vector3Value
                                 = pointPos + newPointPosHandle - currentSelection.GetComponent<Bezier>().transform.position;

                        pointPos = SListPoint(m_closestPoint.intValue - 1);
                        m_pointsList.GetArrayElementAtIndex(Points(m_closestPoint.intValue - 1)).FindPropertyRelative("points").vector3Value
                              = pointPos + newPointPosHandle - currentSelection.GetComponent<Bezier>().transform.position;

                        pointPos = SListPoint(m_closestPoint.intValue + 1);
                        m_pointsList.GetArrayElementAtIndex(Points(m_closestPoint.intValue + 1)).FindPropertyRelative("points").vector3Value
                              = pointPos + newPointPosHandle - currentSelection.GetComponent<Bezier>().transform.position;
                    }

                }

            }

            serializedObject.ApplyModifiedProperties();
            #endregion
        }

        VisualElement VEUpdateButton()
        {
            #region 
            VisualElement root = new VisualElement();

            Button button = new Button();
            button.text = "Update";
            button.clicked += UpdateRoad;
            return button;
            #endregion
        }

        void UpdateRoad()
        {
            #region 
            if (isProcessDone)
                UpdateRoad(0, HowManyCurvePoints() / 3, true, true); 
            #endregion
        }
        int HowManyCurvePoints()
        {
            #region
            return m_pointsList.arraySize;
            #endregion
        }

        void UpdateRoad(int firstPointUpdate, int lastPointUpdate, bool updateBorder, bool isRoadMeshRemoved, Bezier specificBezier = null, GameObject crossRoad = null)
        {
            #region
            isProcessDone = false;
            GameObject currentSelection = Selection.activeGameObject;
            Bezier myScript = (Bezier)currentSelection.GetComponent<Bezier>();
            if (specificBezier)
                myScript = specificBezier;

            RoadMeshGen.ReturnTotalCurveDistance(myScript);
            while (!RoadMeshGen.isProcessDone) { }

            if (myScript.isRoadMeshUpdated)
            {
                if (isRoadMeshRemoved)
                {
                    myScript.GetComponent<MeshFilter>().sharedMesh = null;
                    myScript.GetComponent<MeshCollider>().sharedMesh = null;
                }

                int howManyPoints = 0;
                if (specificBezier)
                {
                    if (specificBezier.totalDistance == 0)
                        specificBezier.totalDistance = 40;
                    howManyPoints = specificBezier.distVecList.Count;
                }
                else
                {
                    serializedObject.Update();

                    if (m_totalDistance.floatValue == 0)
                        m_totalDistance.floatValue = 40;

                    serializedObject.ApplyModifiedProperties();

                    howManyPoints = m_distVecList.arraySize;
                }
                
                RoadMeshGen.RoadMesh(
                    firstPointUpdate,
                    lastPointUpdate,
                    myScript,
                    howManyPoints,
                    RoadMeshGen.RoadStyle.Wire,
                    RoadMeshGen.PlaneFace.Front,
                    true,
                    myScript.roadSubdivisionWhenGenerated);

                while (!RoadMeshGen.isProcessDone) { }
            }

            isProcessDone = true;
            if (crossRoad) Selection.activeGameObject = crossRoad;
            #endregion
        }

        public Vector3 SListPoint(int index)
        {
            #region
            GameObject currentSelection = Selection.activeGameObject;
            return m_pointsList.GetArrayElementAtIndex(Points(index)).FindPropertyRelative("points").vector3Value + currentSelection.GetComponent<Bezier>().transform.position;
            #endregion
        }
        int Points(int value)
        {
            #region

            return (value + m_pointsList.arraySize) % m_pointsList.arraySize;
            #endregion
        }

        VisualElement IntegerSliderFloatExample(SerializedProperty _sliderProperty)
        {
            #region 
            VisualElement root = new VisualElement();
            if (_sliderProperty != null)
            {
                root.style.flexDirection = FlexDirection.Row;

                // Slider
                Slider slider = new Slider(0, m_sliderMaxValue.floatValue, SliderDirection.Horizontal, 0);

                slider.BindProperty(_sliderProperty);
                slider.style.width = 70;
                root.Add(slider);

                // Value
                FloatField floatField = new FloatField();
                floatField.BindProperty(_sliderProperty);
                floatField.style.width = 30;
                root.Add(floatField);
            }

            return root;
            #endregion
        }
    }

}
#endif