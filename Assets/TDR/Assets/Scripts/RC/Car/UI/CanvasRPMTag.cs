// Description: CanvasRPMTag. Use as a tag on Canvas_RPM object.
using UnityEngine;
using UnityEngine.UI;

namespace TS.Generics
{
    public class CanvasRPMTag : MonoBehaviour
    {
        public bool             CanvasInsideVehicle = false;
        public VehicleInfo      VInfo;
        public int              PlayerID = 0;
        public RectTransform    Needle;
        public CurrentText      TxtGear;
        public CurrentText      TxtSpeed;
        public Image            imMaxRPM;

        public int ReturnPlayerID()
        {
            #region 
            if (CanvasInsideVehicle && VInfo)
                return VInfo.playerNumber;

            return PlayerID; 
            #endregion
        }
    }

}
