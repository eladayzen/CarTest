using UnityEngine;
using System.Collections.Generic;

namespace TS.Generics
{
    [CreateAssetMenu(fileName = "PlayerMainMenuSelection", menuName = "TS/PlayerMainMenuSelection")]
    public class PlayerMainMenuSelection : ScriptableObject
    {
        public bool     helpBox;
        public int      HowManyPlayer;
        public int      currentGameMode;
        public int      howManyVehicleInSelectedGameMode;

        public int      currentDifficulty = 0;

        public int      vehicleMode3 = 0;

        public int      currentCamStyle = 0;

        public List<string> camNameList = new List<string>();
    }
}

