// Description: PreviewTrackAssistant:  Work in association with PreviewTrackSystem
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class PreviewTrackAssistant : MonoBehaviour
    {
        public Transform camPos;
        public Transform refPosOpti;

        void Start()
        {
            InitOptiRefPos();
        }

        public void DisplayCurrentTrackName(CurrentText currentText)
        {
            #region
            string name = GameModeGlobal.instance.NameOfTheTrackDependingGameMode();
            currentText.DisplayTextComponent(currentText.gameObject, name); 
            #endregion
        }

        public void NextStep()
        {
            #region
            SceneStepsManager.instance.NextStep(); 
            #endregion
        }

        public void MoveOptiPosition()
        {
            #region
            TSOptiGrid tSOptiGrid = FindAnyObjectByType<TSOptiGrid>();

            if (tSOptiGrid && refPosOpti)
                refPosOpti.position = tSOptiGrid.transform.position + Vector3.left * 1000; 
            #endregion
        }

        public void InitOptiRefPos()
        {
            #region
            if (camPos && refPosOpti)
                refPosOpti.position = camPos.position;
            #endregion
        }
    }
}
