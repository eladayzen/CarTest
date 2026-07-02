// Description: OrbitalCam. Use to move around the vehicle in the garage and the vehicle cutomization
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class OrbitalCam : MonoBehaviour
    {
        bool                        allowsCameraMovement = false;

        public Vector3              initPosition = new Vector3(0, 0, 18);

        public float                CamSpeed = 30;
        public float                camSpeedGamepad = 5;

        public float                DampSpeed = 5;

        public Transform            PivotCam;
        public Transform            Cam;
        public float                ScollSpeed = 3;
        public float                MinScroll = 10;
        public float                MaxScroll = 20;
        private float               camScrollZPos = 0;
        float                       xRot = 0.0f;
        float                       yRot = 0.0f;

        [HideInInspector]
        public float                currentXRot = 0.0f;
        [HideInInspector]
        public float                currentYRot = 0.0f;

     

        public float                WaitBeforeAutoMode = 2;
        float                       timer;
        float                       multiplier = 1;
        public float                AutoMoveSpeed = 1;

        public float                MinXLimit = 0;
        public float                MaxXLimit = 180;

        public float                MinYLimit = -45;
        public float                MaxYLimit = 115;
        public bool                 isYCompleteRotationAllowed = false;

        public bool                 AutoRotation = true;
        public float                targetAutoMoveXPos = 10;

        public List<Vector3>        SpecialPosList = new List<Vector3>();
        public AnimationCurve       SpecialPosCurve;

        bool                        isAutoModeEnable = false;

        float                       refMinScroll = 10;
        float                       refMaxScroll = 20;
        List<Vector3>               refSpecialPosList = new List<Vector3>();

        float                       currentMinScroll = 0;
        float                       currentMaxScroll = 0;

        bool                        firstTime = true;

        bool                        isMousePressed = false;
        bool                        isGamePadPressed = false;  

        public int                  gamepadZoomInInputID = 5;
        public int                  gamepadZoomOutInputID = 6;

        public float                gamepadScrollSpeed = .05f;
        public bool                 invertPadScrollHorAxis = false;
        public bool                 invertPadScrollVerAxis = false;




        // Call when page is initialized (init)
        public bool EnableOrbitalMovement()
        {
            #region
            InitOrbitalCamera();
            return true;
            #endregion
        }

        // Call when menu page is closed (pageout)
        public void DisableOrbitalMovement()
        {
            #region
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;
            allowsCameraMovement = false;
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false;
            #endregion
        }

        public void InitOrbitalCamera()
        {
            #region
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;
            //Debug.Log("init Cam garage");

            if (firstTime)
            {
                firstTime = false;
                currentMinScroll = MinScroll;
                currentMaxScroll = MaxScroll;
            }

            timer = 0;
         
            currentYRot = initPosition.y;
            currentXRot = initPosition.x;

            var rotation = Quaternion.Euler(currentYRot, currentXRot, 0);
            transform.rotation = rotation;

            Vector3 offsetCamPos = new Vector3(Cam.localPosition.x, Cam.localPosition.y, -initPosition.z);
            Cam.localPosition = offsetCamPos;

            allowsCameraMovement = true;
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false;
            #endregion
        }

        void LateUpdate()
        {
            #region
            if (allowsCameraMovement)
            {
                ReturnMouseInput();
                ReturnGamepadInput();
                UpdateCameraOrbitalRotation();
                AutoMove();
            }
            #endregion
        }

        void UpdateCameraOrbitalRotation()
        {
            #region  
            currentXRot += xRot;
            currentYRot += yRot;

            if (currentXRot < -360) currentXRot += 360;
            if (currentXRot > 360) currentXRot -= 360;

            currentXRot = Mathf.Clamp(currentXRot, MinXLimit, MaxXLimit);

            if (isYCompleteRotationAllowed)
                currentYRot = (currentYRot + 2 * 360) % 360;
            else
                currentYRot = Mathf.Clamp(currentYRot, MinYLimit, MaxYLimit);


            var rotation = Quaternion.Euler(currentXRot, currentYRot, 0);

            transform.rotation = rotation;

            if (!isAutoModeEnable)
            {
                xRot = Mathf.Lerp(xRot, 0, Time.deltaTime * DampSpeed);
                yRot = Mathf.Lerp(yRot, 0, Time.deltaTime * DampSpeed);

                if (xRot >= -.01f && xRot <= .01f) xRot = 0;
                if (yRot >= -.01f && yRot <= .01f) yRot = 0;
            }
            #endregion
        }

        void AutoMove()
        {
            #region
            if (!isMousePressed && AutoRotation 
                &&
                !isGamePadPressed && AutoRotation)
            {
                if (timer < WaitBeforeAutoMode)
                {
                    timer += Time.deltaTime;
                }
                    
                if (timer > WaitBeforeAutoMode)
                {
                    isAutoModeEnable = true;

                    if (!isYCompleteRotationAllowed)
                    {
                        if (currentYRot == MinYLimit)
                            multiplier = 1;
                        else if (currentYRot == MaxYLimit)
                            multiplier = -1;
                    }
                  
                    yRot = multiplier * Time.deltaTime * AutoMoveSpeed;
                    currentYRot += yRot;

                    if (isYCompleteRotationAllowed)
                    {
                        currentYRot = (currentYRot + 2 * 360) % 360;
                    }


                    currentXRot = Mathf.MoveTowards(currentXRot, targetAutoMoveXPos, Time.deltaTime *2);
                }
            }
            #endregion
        }

        void ReturnMouseInput()
        {
            #region
            if (Input.GetMouseButton(0))
            {
                xRot -= Time.deltaTime * Input.GetAxis("Mouse Y") * CamSpeed;
                yRot += Time.deltaTime * Input.GetAxis("Mouse X") * CamSpeed;
                timer = 0;
                isAutoModeEnable = false;
                isMousePressed = true;
            }
            else
            {
                isMousePressed = false;
            }

            if (Cam)
            {
                float scrollValue = Input.mouseScrollDelta.y;

                if (scrollValue != 0)
                {
                    timer = 0;
                    isAutoModeEnable = false;
                }

                float camZPos = scrollValue * ScollSpeed;

                camZPos = Cam.localPosition.z + camZPos;
                camZPos = Mathf.Clamp(camZPos, -currentMaxScroll, -currentMinScroll);

                if(currentMinScroll != refMinScroll)
                    currentMinScroll =  Mathf.Lerp(currentMinScroll, refMinScroll, Time.deltaTime * 10);
                if (currentMaxScroll != refMaxScroll)
                    currentMaxScroll = Mathf.Lerp(currentMaxScroll, refMaxScroll, Time.deltaTime * 10);

                camScrollZPos = camZPos;

                float newZPos = Mathf.Lerp(Cam.localPosition.z, camScrollZPos, Time.deltaTime * 100);

                Cam.localPosition = new Vector3(Cam.localPosition.x, Cam.localPosition.y, newZPos);
            }
            #endregion
        }

        void ReturnGamepadInput()
        {
            #region
            if ((Mathf.Abs(Input.GetAxis("TSOrbitalHorPad")) > .2f 
                || 
                Mathf.Abs(Input.GetAxis("TSOrbitalVertPad")) > .2f) &&
                !isMousePressed)
            {
                int invertHor = invertPadScrollHorAxis == false ? 1 : -1;
                int invertVer = invertPadScrollVerAxis == false ? 1 : -1;
                xRot -= Time.deltaTime * Input.GetAxis("TSOrbitalVertPad") * camSpeedGamepad * invertHor;
                yRot += Time.deltaTime * Input.GetAxis("TSOrbitalHorPad") * camSpeedGamepad * invertVer;
                timer = 0;
                isAutoModeEnable = false;
                isGamePadPressed = true;
            }
            else
            {
                isGamePadPressed = false;
            }

            if (Cam)
            {
                float scrollValue = ReturnZoomInputValue();

                if (scrollValue != 0)
                {
                    timer = 0;
                    isAutoModeEnable = false;
                }

                float camZPos = scrollValue * ScollSpeed;

                camZPos = Cam.localPosition.z + camZPos;
                camZPos = Mathf.Clamp(camZPos, -currentMaxScroll, -currentMinScroll);

                if (currentMinScroll != refMinScroll)
                    currentMinScroll = Mathf.Lerp(currentMinScroll, refMinScroll, Time.deltaTime * 10);
                if (currentMaxScroll != refMaxScroll)
                    currentMaxScroll = Mathf.Lerp(currentMaxScroll, refMaxScroll, Time.deltaTime * 10);

                camScrollZPos = camZPos;

                float newZPos = Mathf.Lerp(Cam.localPosition.z, camScrollZPos, Time.deltaTime * 100);

                Cam.localPosition = new Vector3(Cam.localPosition.x, Cam.localPosition.y, newZPos);
            }
            #endregion
        }

        public void NewCamPosition(int index)
        {
            #region
            StopAllCoroutines();
            StartCoroutine(NewCamPositionRoutine(index));
            #endregion
        }

        IEnumerator NewCamPositionRoutine(int index)
        {
            #region
            allowsCameraMovement = false;
            float t = 0;
            float duration = .75f;

            float camDistance = Cam.localPosition.z;

            float xPosRef = currentXRot;
            float yPosRef = currentYRot;

            while (t < 1)
            {
                t += Time.deltaTime / duration;
                timer = 0;

                if (refSpecialPosList[index].x <= MaxXLimit && refSpecialPosList[index].x >= MinXLimit)
                    currentXRot = Mathf.Lerp(xPosRef, refSpecialPosList[index].x, SpecialPosCurve.Evaluate(t));

                if (currentXRot < -360) currentXRot = 360;
                if (currentXRot > 360) currentXRot -= 360;

                if (refSpecialPosList[index].y <= MaxYLimit && refSpecialPosList[index].y >= MinYLimit)
                    currentYRot = Mathf.Lerp(yPosRef, refSpecialPosList[index].y, SpecialPosCurve.Evaluate(t));

                var rotation = Quaternion.Euler(currentYRot, currentXRot, 0);
                transform.rotation = rotation;

               // Debug.Log(refSpecialPosList[index].z + " <= " + refMaxScroll + " | " +
                 //   refSpecialPosList[index].z + " >= " + refMinScroll);
                if(refSpecialPosList[index].z <= refMaxScroll && refSpecialPosList[index].z >= refMinScroll)
                {
                    float zPos = Mathf.Lerp(camDistance, -refSpecialPosList[index].z, SpecialPosCurve.Evaluate(t));
                    Vector3 offsetCamPos = new Vector3(Cam.localPosition.x, Cam.localPosition.y, zPos);
                    Cam.localPosition = offsetCamPos;
                }
                yield return null;
            }

            allowsCameraMovement = true;
            yield return null;
            #endregion
        }

        void OnEnable()
        {
            #region
            StopAllCoroutines();
            #endregion
        }
        void OnDisable()
        {
            #region
            StopAllCoroutines();
            #endregion
        }

        public void InitVariableDependingVehicle(float _minScroll,float _maxScroll, List<Vector3> _specialPosList)
        {
            #region
            //Debug.Log(_minScroll + " : "+  _maxScroll);
            refMinScroll = _minScroll == -1 ? MinScroll : _minScroll;
            refMaxScroll = _maxScroll == -1 ? MaxScroll : _maxScroll;

            refSpecialPosList.Clear();
            if (_specialPosList.Count == 0)
            {
                for (var i = 0; i < SpecialPosList.Count; i++)
                    refSpecialPosList.Add(SpecialPosList[i]);
            }
            else
            {
                for (var i = 0; i < _specialPosList.Count; i++)
                    refSpecialPosList.Add(_specialPosList[i]);
            }
            #endregion
        }

        public void NewCamVectorThreePosition(Vector3 newVec)
        {
            #region
            StopAllCoroutines();
            StartCoroutine(NewCamVectorThreePositionRoutine(newVec));
            #endregion
        }

        IEnumerator NewCamVectorThreePositionRoutine(Vector3 newVec)
        {
            #region
            allowsCameraMovement = false;

            float t = 0;
            float duration = .75f;

            float camDistance = Cam.localPosition.z;

            float xPosRef = currentXRot;
            float yPosRef = currentYRot;

             while (t < 1)
             {
                t += Time.deltaTime / duration;
                timer = 0;
                if (newVec.x <= MaxXLimit && newVec.x >= MinXLimit)
                    currentXRot = Mathf.Lerp(xPosRef, newVec.x, SpecialPosCurve.Evaluate(t));

                if ((newVec.y <= MaxYLimit && newVec.y >= MinYLimit) || isYCompleteRotationAllowed)
                    currentYRot = Mathf.Lerp(yPosRef, newVec.y, SpecialPosCurve.Evaluate(t));

                if (newVec.z <= refMaxScroll && newVec.z >= refMinScroll)
                {
                    float zPos = Mathf.Lerp(camDistance, -newVec.z, SpecialPosCurve.Evaluate(t));
                    Vector3 offsetCamPos = new Vector3(Cam.localPosition.x, Cam.localPosition.y, zPos);
                    Cam.localPosition = offsetCamPos;
                }
                var rotation = Quaternion.Euler(currentXRot, currentYRot, 0);

                transform.rotation = rotation;

                yield return null;
             }

            allowsCameraMovement = true;
            yield return null;
            #endregion
        }

        public float ReturnZoomInputValue()
        {
            #region
            float value = Input.GetAxisRaw(InfoInputs.instance.ListOfInputsForEachPlayer[0].listOfButtons[gamepadZoomInInputID]._AxisName);

            if (value > .2f && InfoInputs.instance.ListOfInputsForEachPlayer[0].listOfButtons[gamepadZoomInInputID]._AxisPositiveOrNegative == 1 ||
                    value < -.2f && InfoInputs.instance.ListOfInputsForEachPlayer[0].listOfButtons[gamepadZoomInInputID]._AxisPositiveOrNegative == -1 ||
                    InfoInputs.instance.ListOfInputsForEachPlayer[0].listOfButtons[gamepadZoomInInputID]._AxisPositiveOrNegative == 0)
            {
                return gamepadScrollSpeed;
            }

            value = Input.GetAxisRaw(InfoInputs.instance.ListOfInputsForEachPlayer[0].listOfButtons[gamepadZoomOutInputID]._AxisName);

            if (value > .2f && InfoInputs.instance.ListOfInputsForEachPlayer[0].listOfButtons[gamepadZoomOutInputID]._AxisPositiveOrNegative == 1 ||
                    value < -.2f && InfoInputs.instance.ListOfInputsForEachPlayer[0].listOfButtons[gamepadZoomOutInputID]._AxisPositiveOrNegative == -1 ||
                    InfoInputs.instance.ListOfInputsForEachPlayer[0].listOfButtons[gamepadZoomOutInputID]._AxisPositiveOrNegative == 0)
            {
                return -gamepadScrollSpeed;
            }

            return 0;
            #endregion
        }
    }

}
