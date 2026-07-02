// Description: Inside a vehicle, attached to VehicleCustomization. Hub to manage vehicle customization
// For each vehicle it is possible to create customization.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public enum OrbitalState { KeepPosition, NewPosition };
    public class VehicleCustomization : MonoBehaviour
    {
        [HideInInspector]
        public bool                         b_InitDone;
        public bool                         AutoInit = true;
        public int                          VehicleID = 0;
       

        [System.Serializable]
        public class Section
        {
            public string                   Name = "";
            public int                      TextList = 0;
            public int                      TextID = 0;
            public Transform                ObjSection;
            public IVehicleCustomization    ISection;
          
            public OrbitalState             orbitalState = OrbitalState.KeepPosition;
            public Vector3                  newCamPosition;
        }

        public List<Section>                CustomSectionList = new List<Section>();


        void Start()
        {
            #region
            if (AutoInit)
                StartCoroutine(InitRoutine());
            #endregion
        }

        IEnumerator InitRoutine()
        {
            #region
            bool isPlayerInAGameplayScene = VehiclesRef.instance != null ? true : false;
            if (isPlayerInAGameplayScene)
                yield return new WaitUntil(() => transform.parent.GetComponent<VehiclePrefabInit>().vehicleInfo.b_InitDone);
            
            int playerNumber = transform.parent.GetComponent<VehiclePrefabInit>().vehicleInfo.playerNumber;
            int howManyPlayer = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer;

            int counter = 0;
            foreach (Section section in CustomSectionList)
            {
                if (section.ObjSection)
                    section.ISection = section.ObjSection.GetComponent<IVehicleCustomization>();

                if (section.ISection != null)
                {
                   if( InfoVehicleCustomization.instance.CustomizationVehicleList.Count > VehicleID)
                    {
                        //Debug.Log("vID: " + VehicleID + " | " + counter);
                        InfoVehicleCustomization.Section carParams = InfoVehicleCustomization.instance.CustomizationVehicleList[VehicleID].Vehicle[counter];

                        if (section.ISection.GetItemsNumber() > 0 && 
                            (playerNumber < howManyPlayer
                            ||
                            !isPlayerInAGameplayScene))
                        {
                            if ((howManyPlayer == 2 && playerNumber == 1))
                            {
                                CustomizeAICar(section, playerNumber, counter);
                            }
                            else
                            {
                                section.ISection.SetCurrentSelection(carParams.CurrentSelection);
                                section.ISection.CustomizationProcess();
                            }
                        }
                        else if (section.ISection.GetItemsNumber() > 0)
                        {
                            CustomizeAICar(section,playerNumber, counter);
                        }
                    }
                   
                }
                counter++;
            }

            b_InitDone = true;
            //Debug.Log("Init: Vehicle Customization -> Done");
            yield return null;
            #endregion
        }

        public bool InitCustomPerformance()
        {
            #region
            int counter = 0;
            foreach (Section section in CustomSectionList)
            {
                if (section.ObjSection)
                    section.ISection = section.ObjSection.GetComponent<IVehicleCustomization>();

                if (section.ISection != null)
                    section.ISection.InitCustomPerformance();
                counter++;
            }
            return true;
            #endregion
        }

        public void CustomizeAICar(Section section,int playerNumber, int indexSection)
        {
            #region 
            section.ISection.SetVehicleIDCategory(VehicleID);
            section.ISection.SetIndexSection(indexSection);
            section.ISection.SetCurrentSelection(-playerNumber - 1);
            section.ISection.CustomizationProcess(); 
            #endregion
        }
    }

}
