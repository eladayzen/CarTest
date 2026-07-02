// Description: GrpPodiumTag. Attached to podiums. Use to find the podium object in the scene.
using UnityEngine;

namespace TS.Generics
{
    public class GrpPodiumTag : MonoBehaviour
    {
        public int          id;
        public Transform    playerOne;
        public Transform    playerTwo;
        public Transform    grp3DModels;
        public Transform    grp_Camera;

        public bool disableOnStart = true;

        private void Start()
        {
            #region 
            if (disableOnStart)
            {
                if (grp_Camera && grp_Camera.gameObject.activeSelf) grp_Camera.gameObject.SetActive(false);
                if (grp3DModels && grp3DModels.gameObject.activeSelf) grp3DModels.gameObject.SetActive(false);
            }
         
            #endregion
        }
    }

}

