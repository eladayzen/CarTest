// Description OptiGridAssistant: Check if the vehicle is a real player for the optimization system
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class OptiGridAssistant : MonoBehaviour, IOptiGridCondition<GameObject>
    {
        public bool IsThisObjectATarget(GameObject target)
        {
            #region
            int howManyPlayer = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer;
            if (target.GetComponent<VehicleInfo>())
            {
                int playerID = target.GetComponent<VehicleInfo>().playerNumber;

                if ((playerID == 0 && howManyPlayer > 0) || (playerID == 1 && howManyPlayer > 1))
                    return true;
            }
            else if (target.transform.parent.GetComponent<PreviewTrackAssistant>())
            {
                return true;
            }

            return false; 
            #endregion
        }
    }

}
