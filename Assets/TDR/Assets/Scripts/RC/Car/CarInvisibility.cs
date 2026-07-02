// Description: CarInvisibility. Attached to the vehicle.
// Make the car blinking until the vehicle is inivisible after the respawn phase.
// Call by vehicleDamage script attached to the vehicle. Called during respawn.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class CarInvisibility : MonoBehaviour
    {
        VehicleDamage       vehicleDamage;
        CarController       carController;

        List<GameObject>    colliderList = new List<GameObject>();

        List<GameObject>    bodyList = new List<GameObject>();

        Transform           refForOverlapBoxSize;

        public int          vehicleRef_Layer = 9;
        public int          invisibilityRef_Layer = 15;

        void Start()
        {
            #region
            vehicleRef_Layer = LayersRef.instance.layersListData.listLayerInfo[vehicleRef_Layer].layerID;
            invisibilityRef_Layer = LayersRef.instance.layersListData.listLayerInfo[invisibilityRef_Layer].layerID;

            vehicleDamage = GetComponent<VehicleDamage>();
            carController = GetComponent<CarController>();

            // Generate the list of colliders used for this car
            Collider[] allColliders = carController.grp_Colliders.transform.GetComponentsInChildren<Collider>(true);

            foreach (Collider col in allColliders)
                colliderList.Add(col.gameObject);

            // Generate a list of objects used for the car body
            bodyList.Add(carController.grp_Body);

            for(var i = 0; i < carController.wheelsList.Count; i++)
                bodyList.Add(carController.wheelsList[i].wheelAxisX.GetChild(0).gameObject);

            VehicleTriggerTag vehicleTriggerTag = carController.grp_Colliders.transform.GetComponentInChildren<VehicleTriggerTag>(true);
            refForOverlapBoxSize = vehicleTriggerTag.transform;

            #endregion
        }

        public void Invisibility()
        {
            #region
            StartCoroutine(InvisibilityRoutine());
            #endregion
        }

        IEnumerator InvisibilityRoutine()
        {
            #region
            foreach (GameObject obj in colliderList)
                obj.layer = invisibilityRef_Layer;

            for(var i = 0; i < 5; i++)
            {

                foreach (GameObject obj in bodyList)
                    obj.SetActive(false);

                float t = 0;
                float duration = .25f;
                
                
                while (t < duration)
                {
                    if (!PauseManager.instance.Bool_IsGamePaused)
                        t += Time.deltaTime;

                    yield return null;
                }

                foreach (GameObject obj in bodyList)
                    obj.SetActive(true);

                t = 0;
                while (t < duration)
                {
                    if (!PauseManager.instance.Bool_IsGamePaused)
                        t += Time.deltaTime;

                    yield return null;
                }

                if (CheckIfCarInCollisionWithAnotherCar() && i == 4)
                    i--;
            }

            foreach (GameObject obj in colliderList)
                obj.layer = vehicleRef_Layer;

            vehicleDamage.b_Invincibility = false;

            yield return null;
            #endregion
        }

        bool CheckIfCarInCollisionWithAnotherCar()
        {
            #region
            Collider[] hitColliders = Physics.OverlapBox(carController.transform.position, refForOverlapBoxSize.localScale * .5f, carController.transform.rotation);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.gameObject.layer == vehicleRef_Layer &&
                    !hitCollider.isTrigger)
                {
                    bool find = false;
                    for (var i = 0; i < colliderList.Count; i++)
                        if (hitCollider.gameObject == colliderList[i].gameObject)
                            find = true;

                    if (!find)
                        return true;
                }

            }
            return false; 
            #endregion
        }

    }

}
