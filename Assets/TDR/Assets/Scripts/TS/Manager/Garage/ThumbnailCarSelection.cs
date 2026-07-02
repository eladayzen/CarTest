using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TS.Generics
{
    public class ThumbnailCarSelection : MonoBehaviour
    {
        public List<Sprite> vehicleThumbnailList = new List<Sprite>();


        public List<Image> carrouselList = new List<Image>();


        public void DisplayVehicleSprite(int currentCategory, int currentVehicle)
        {

            carrouselList[3].sprite = vehicleThumbnailList[currentVehicle];
            VehicleGlobalData vehicleData = DataRef.instance.vehicleGlobalData;
            InfoVehicle infoVehicle = InfoVehicle.instance;

            for (var i = 1;i < 4;i++)
            {
                // All vehicles
                if(currentCategory == -1)
                {
                    //-> Use list Order
                    currentVehicle = infoVehicle.currentVehicleDisplayedInTheGarage + i;


                    if(currentVehicle < infoVehicle.vehicleListByCategory[infoVehicle.vehicleListByCategory.Count - 1].vehicleList.Count)
                    {
                        carrouselList[3 + i].gameObject.SetActive(true);
                        carrouselList[3+i].sprite = vehicleThumbnailList[currentVehicle];
                    }
                    else
                    {
                        carrouselList[3 + i].gameObject.SetActive(false);
                        carrouselList[3 + i].sprite = null;
                    }
                }
            }

            for (var i = 0; i < 3; i++)
            {
                // All vehicles
                if (currentCategory == -1)
                {

                    Debug.Log(2-i);
                    //-> Use list Order
                    currentVehicle = infoVehicle.currentVehicleDisplayedInTheGarage - 1 - i;


                    if (currentVehicle >= 0)
                    {
                        carrouselList[2 - i].gameObject.SetActive(true);
                        carrouselList[2 - i].sprite = vehicleThumbnailList[currentVehicle];
                    }
                    else
                    {
                        carrouselList[2 - i].gameObject.SetActive(false);
                        carrouselList[2 - i].sprite = null;
                    }
                }
            }

        }
    }
}