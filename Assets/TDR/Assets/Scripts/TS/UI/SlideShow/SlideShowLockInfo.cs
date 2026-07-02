// Description: SlideShowLockInfo: Methods to display info about the reason the slide is locked
using UnityEngine;
using UnityEngine.UI;


namespace TS.Generics
{
    public class SlideShowLockInfo : MonoBehaviour
    {
        public Image                    Im_Black;
        public Image                    Im_Lock;
        public CurrentText              txtSlideShowLock;
        public RectTransVariousMethods  Grp_InfoTxt;


        public void ReturnLockInfoSmooth(int value)
        {
            #region
            if (Im_Lock.gameObject.activeSelf && Grp_InfoTxt)
                Grp_InfoTxt.ChangePivotSmooth(value); 
            #endregion
        }
        public void ReturnLockInfoStright(int value)
        {
            #region 
            if (Grp_InfoTxt) Grp_InfoTxt.ChangePivotStraight(value); 
            #endregion
        }
    }

}
