// Description: DisplayGearRPMAndSpeed: Display on UI vehicle Speed and RPM
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class DisplayGearRPMAndSpeed : MonoBehaviour
    {
        public bool                     AutoInit = true;
        public bool                     IsInitDone = false;
        public AudioCarEngine           CarEngine;
        public CarController            carController;
        public CarState                 carState;
        public Rigidbody                rb;

        public List<GameObject>         CanvasRpmModuleList = new List<GameObject>();

        // RPM
        [HideInInspector]
        public List<RectTransform>      NeedleList = new List<RectTransform>();

        // Gear
        [HideInInspector]
        public List<CurrentText>        TxtGearList = new List<CurrentText>();
        public string                   sNeutral = "N";
        public string                   sFirstGear = "1";
        public string                   sRevers = "R";

        // Speed
        public enum SpeedType           { KmH,MPH,Custom}
        public SpeedType                speedType = SpeedType.KmH;
        public float                    customSpeedMultiplier = 3.6f;
        //public bool                     isKMHDisplayed = true;
        public float                    KMHorMPH = 0;               // velocity magnitude -> 1m/s
        public string                   sKMHorMPH = " km/h";
        [HideInInspector]
        public List<CurrentText>        TxtSpeedList = new List<CurrentText>();

        [HideInInspector]
        public Countdown                countdown;

        int                             playerID = 0;

        public int                      needleRotationMinRPM = 150;      // Represents the rotation of the needle for 0 RPM
        [Tooltip("-90 -> 8k RPM | -60 -> 7K RPM")]
        public int                      needleRotationMaxRPM = -90;      // Represents the rotation of the needle to reach the maximum car rpm.
        public int                      needleRefRotationMaxRPM = -90;   // Represents the rotation of the needle to reach the maximum rpm in the dial.

        float                           needleZ = 0;                    // target rotation for needle Z rotation
        float                           speedNeedle = 250;              // Needle rotation speed

     
        void Start()
        {
            #region
            if (AutoInit)
                StartCoroutine(InitAudioCarEngineRoutine());
            #endregion
        }

        public bool BAudioCarEngineRoutine()
        {
            #region
            StartCoroutine(InitAudioCarEngineRoutine());
            return true;
            #endregion
        }

        IEnumerator InitAudioCarEngineRoutine()
        {
            #region
            if (Countdown.instance)
                countdown = Countdown.instance;

            if (carController.vehicleInfo)
            {
                yield return new WaitUntil(() => carController.vehicleInfo.b_InitDone);

                playerID = carController.vehicleInfo.playerNumber;
            }
               
            // Init Canvas RPM
            CanvasRPMTag[] allCanvasRPMTag = FindObjectsByType<CanvasRPMTag>( FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (CanvasRPMTag canvasRPM in allCanvasRPMTag)
            {
               
                if (canvasRPM.ReturnPlayerID() == playerID)
                {
                    TxtGearList.Add(canvasRPM.TxtGear);
                    TxtSpeedList.Add(canvasRPM.TxtSpeed);
                    NeedleList.Add(canvasRPM.Needle);

                    if (!canvasRPM.CanvasInsideVehicle) CanvasRpmModuleList.Add(canvasRPM.gameObject);


                    // Max RPM red border
                    InitMaxRPMRedBorer(canvasRPM);
                }
            }

            IsInitDone = true;

            yield return null;
            #endregion
        }

        void Update()
        {
            #region
            if (IsInitDone)
            {
                if(carState.carPlayerType == CarState.CarPlayerType.Human ||
                   carState.carPlayerType == CarState.CarPlayerType.AI && playerID == 0)
                {
                    DisplayGear();
                    DisplayRPM();
                    DisplaySpeed();
                }
            }
            #endregion
        }

        void DisplayGear()
        {
            #region
            if (rb)
            {
                float carSpeed = rb.linearVelocity.magnitude * 3.6f;

                foreach (CurrentText txtGear in TxtGearList)
                {
                    if (txtGear)
                    {
                        bool find = false;
                        for (var i = CarEngine.gearSpeed.Count - 1; i >= 0; i--)
                        {
                            if (carSpeed >= CarEngine.gearSpeed[i])
                            {
                                txtGear.DisplayTextComponent(txtGear.gameObject, CarEngine.currentGear.ToString());
                                find = true;
                                break;
                            }
                        }

                        if (carState.carDirection == CarState.CarDirection.Backward)
                            txtGear.DisplayTextComponent(txtGear.gameObject, sRevers);
                        else if (!find && carState.carDirection == CarState.CarDirection.Forward)
                            txtGear.DisplayTextComponent(txtGear.gameObject, sFirstGear);
                        else if (!find && carState.carDirection == CarState.CarDirection.Stop)
                            txtGear.DisplayTextComponent(txtGear.gameObject, sNeutral);
                    }
                   
                }
            }
            #endregion
        }

        void DisplayRPM()
        {
            #region
            float zTarget = needleRotationMaxRPM + (-needleRotationMaxRPM + needleRotationMinRPM) * (1 - CarEngine.rpm);
            needleZ = Mathf.MoveTowards(needleZ,zTarget,Time.deltaTime * speedNeedle);
            needleZ = Mathf.Clamp(needleZ, needleRotationMaxRPM, needleRotationMinRPM);

            foreach (RectTransform needle in NeedleList)
                if (needle) needle.localEulerAngles = new Vector3(0, 0, needleZ);

            #endregion
        }

        void DisplaySpeed()
        {
            #region
            if (speedType == SpeedType.KmH) KMHorMPH = rb.linearVelocity.magnitude * 3.6f;
            else if(speedType == SpeedType.MPH) KMHorMPH = rb.linearVelocity.magnitude * 2.237f;
            else KMHorMPH = rb.linearVelocity.magnitude * customSpeedMultiplier;

            foreach (CurrentText txtGear in TxtSpeedList)
            {
                if (txtGear)
                {
                    string tSpeed = Mathf.RoundToInt(KMHorMPH) + sKMHorMPH;

                    if (countdown && !countdown.b_IsCountdownEnded)
                        tSpeed = "0" + sKMHorMPH;


                    txtGear.DisplayTextComponent(txtGear.gameObject, tSpeed);
                }
                 
            }
            #endregion
        }

        public void RPMmoduleState(bool isEnabled)
        {
            #region 
            // Enable or disable RPM canvas
            for (var i = 0; i < CanvasRpmModuleList.Count; i++)
                CanvasRpmModuleList[i].SetActive(isEnabled); 
            #endregion
        }

        public void InitMaxRPMRedBorer(CanvasRPMTag canvasRPM)
        {
            #region 
            float length = Mathf.Abs(needleRefRotationMaxRPM) - Mathf.Abs(needleRotationMaxRPM);
            if (needleRotationMaxRPM > 0)
                length = needleRotationMaxRPM - needleRefRotationMaxRPM;

            length = Mathf.Abs(length);
            float fillAmount = (0.084f / 30f) * length;
            canvasRPM.imMaxRPM.fillAmount = fillAmount; 
            #endregion
        }
    }

}
