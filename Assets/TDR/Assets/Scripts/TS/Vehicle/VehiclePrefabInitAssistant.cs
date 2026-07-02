using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

namespace TS.Generics
{
    public class VehiclePrefabInitAssistant : MonoBehaviour
    {
        public bool EnableObject(GameObject obj)
        {
            if(obj)obj.SetActive(true);
            return true;
        }
        public bool DisableObject(GameObject obj)
        {
            if (obj) obj.SetActive(false);
            return true;
        }

        public bool UpdateModelLayer(GameObject obj)
        {
            Transform[] children = obj.GetComponentsInChildren<Transform>();

            //-> Change the layer of the 3D models when the player 1 vehicle is displayed on vehicle selection menu page
            if (transform.parent.GetComponent<GarageTagPivot>().ID == 4)
            {
                foreach (Transform child in children)
                {
                    // If differente from layer IgnoreRaycast (Layer = 2)
                    if(child.gameObject.layer != 2)
                    child.gameObject.layer = LayersRef.instance.layersListData.listLayerInfo[5].layerID;    // Layer Triggers
                }
            }

            //-> Change the layer of the 3D models when the player 2 vehicle is displayed on vehicle selection menu page
            if (transform.parent.GetComponent<GarageTagPivot>().ID == 5)
            {
                foreach(Transform child in children)
                {
                    // If differente from layer IgnoreRaycast (Layer = 2)
                    if (child.gameObject.layer != 2)
                        child.gameObject.layer = LayersRef.instance.layersListData.listLayerInfo[8].layerID;    // Layer LimitZone
                }
            }

            return true;
        }

       public bool InitModeFive()
        {
            transform.GetChild(0).GetComponent<VehiclePathFollow>().enabled = false;
            transform.GetChild(0).GetComponent<Rigidbody>().isKinematic = false;
            transform.GetChild(0).GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;

            Debug.Log("INFO:" + this.name + " is set up as Mode 5.");

            return true;
        }

        public bool InitVehicleCustmization()
        {
            VehicleCustomization vehicleCustomization = GetComponentInChildren<VehicleCustomization>();

            if (vehicleCustomization)
                vehicleCustomization.InitCustomPerformance();
            return true;
        }

        public bool DisableTheDriverInsideVehicle()
        {
            TagCharacterInsideVehicle[] chara = FindObjectsByType<TagCharacterInsideVehicle>(FindObjectsSortMode.None);
            for (var i = 0; i < chara.Length; i++)
                chara[i].gameObject.SetActive(false);

            return true;
        }

        public bool GarageSetStiffSuspension()
        {
            CarController carController = GetComponentInChildren<CarController>();

            carController.SetStiffSuspensionInMainMenu();

            return true;
        }
    }
}