// Description: TutoAssistant: Attached to TutoManager. Methods called by TutoManager.
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class TutoAssistant : MonoBehaviour
    {
        public int              TSInputKeyBrake = 8;
        private bool            bInitBrake;



        private void OnDestroy()
        {
            if (bInitBrake)
                InfoInputs.instance.ListOfInputsForEachPlayer[0].listOfButtons[TSInputKeyBrake].OnGetKeyDownReceived -= BrakeKeyDown;
        }

        public void InitBrake()
        {
            //InfoInputs.instance.ListOfInputsForEachPlayer[0].listOfButtons[TSInputKeyBrake].OnGetKeyDownReceived += BrakeKeyDown;
            //bInitBrake = true;
        }

        void BrakeKeyDown()
        {
            Debug.Log("Brake button is pressed");
        }

    }
}
