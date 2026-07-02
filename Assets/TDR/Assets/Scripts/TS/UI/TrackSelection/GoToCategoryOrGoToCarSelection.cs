using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class GoToCategoryOrGoToCarSelection : MonoBehaviour
    {
        public bool useOnlyOneVehicleCategory = false;
       public void ChooseMenuToDisplayDependingTheNimberOfCategory()
        {
            #region 
            int currentSelectedTrack = 0;
            // Arcade
            if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 0)
            {
                currentSelectedTrack = GameModeArcade.instance.currentSelection;
            }
            // Time Trial
            if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 1)
            {
                currentSelectedTrack = GameModeTimeTrial.instance.currentSelection;
            }


            TracksData.trackParams trackParams = DataRef.instance.tracksData.listTrackParams[currentSelectedTrack];
            if (!DataRef.instance.arcadeModeData.bDisplayTrackUsingTrackListOrder)
            {
                int specialOrderID = DataRef.instance.arcadeModeData.customTrackList[currentSelectedTrack];
                trackParams = DataRef.instance.tracksData.listTrackParams[specialOrderID];
            }

           if(useOnlyOneVehicleCategory)
                this.GetComponent<ButtonCustom>().DisplayNewPage(15);
           else
                this.GetComponent<ButtonCustom>().DisplayNewPage(20);

            #endregion
        }
    }

}
