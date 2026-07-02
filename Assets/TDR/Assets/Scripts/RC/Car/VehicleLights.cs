// Description: VehicleLights: Manage the vehicle lights. Attached to the vehicle.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TS.Generics
{
    public class VehicleLights : MonoBehaviour
    {
        CarController                   carController;
        CarState                        carState;

        [System.Serializable]
        public class TaillLightParams
        {
            public MeshRenderer     brakeLightMR;
            public int              materialID;
        }

        public List<TaillLightParams>   brakeLightsList = new List<TaillLightParams>();

        bool                            allowBrakeLightChangeState = true;

        public UnityEvent               brakeLightInitEvent;
        public UnityEvent               brakeLightOnEvent;
        public UnityEvent               brakeLightOffEvent;
        bool                            isBrakeLightOn = false;


        void Start()
        {
            #region
            carController = GetComponent<CarController>();
            carState = GetComponent<CarState>();
            brakeLightInitEvent?.Invoke(); 
            #endregion
        }

        void Update()
        {
            #region
            BrakeFeedback(); 
            #endregion
        }

        public void BrakeFeedback()
        {
            #region
            if (carController)
            {
                if (carState.carDirection == CarState.CarDirection.Backward)
                {
                    if (carState.moveDir == CarMoveDirection.forward && !isBrakeLightOn)
                    {
                        isBrakeLightOn = true;
                        brakeLightOnEvent?.Invoke();
                    }
                       
                    else if (carState.moveDir != CarMoveDirection.forward && isBrakeLightOn)
                    {
                        isBrakeLightOn = false;
                        brakeLightOffEvent?.Invoke();
                    }    
                }
                else
                {
                    if (allowBrakeLightChangeState)
                    {
                        if (carController.acceleration >= 0 && isBrakeLightOn)
                        {
                            StartCoroutine(WaitBrakLightRoutine());
                            brakeLightOffEvent?.Invoke();
                            isBrakeLightOn = false;
                        }
                        else if (carController.acceleration < 0f && !isBrakeLightOn)
                        {
                            brakeLightOnEvent?.Invoke();
                            isBrakeLightOn = true;
                            StartCoroutine(WaitBrakLightRoutine());
                        }
                    }
                }
            }
            #endregion
        }

        IEnumerator WaitBrakLightRoutine()
        {
            #region
            var t = 0f;
            allowBrakeLightChangeState = false;
            while (t < .25f)
            {
                if (PauseManager.instance && !PauseManager.instance.Bool_IsGamePaused)
                {
                    t += Time.deltaTime;
                    yield return null;
                }
                else
                    t = .5f;
            }
            allowBrakeLightChangeState = true;

            yield return null;
            #endregion
        }

        public void BrakeLightOn()
        {
            #region
            for (var i = 0; i < brakeLightsList.Count; i++)
                if (brakeLightsList[i].brakeLightMR)
                    brakeLightsList[i].brakeLightMR.materials[brakeLightsList[i].materialID].SetColor("_EmissionColor", Color.red);
            #endregion
        }
        public void BrakeLightOff()
        {
            #region
            for (var i = 0; i < brakeLightsList.Count; i++)
                if (brakeLightsList[i].brakeLightMR)
                    brakeLightsList[i].brakeLightMR.materials[brakeLightsList[i].materialID].SetColor("_EmissionColor", Color.black);
            #endregion
        }
    }

}
