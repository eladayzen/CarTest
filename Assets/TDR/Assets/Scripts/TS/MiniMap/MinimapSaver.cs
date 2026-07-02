// Description: MinimapSaver. Attached to MinimapCam object.
using UnityEngine;
using UnityEngine.UI;

namespace TS.Generics
{
    public class MinimapSaver : MonoBehaviour
    {
        public string path = "zMinimap";
        public string minimapName = "New Minimap";

        public Image imgMinimapP1;
        public Image imgMinimapP2;

        private void OnDrawGizmosSelected()
        {
            #region
            Vector3 bottomLeft = transform.position - transform.up * 1200 - transform.right * 1200;
            Vector3 bottomRight = transform.position - transform.up * 1200 + transform.right * 1200;
            Vector3 upLeft = transform.position + transform.up * 1200 - transform.right * 1200;
            Vector3 upRight = transform.position + transform.up * 1200 + transform.right * 1200;

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(bottomLeft, 50);
            Gizmos.DrawSphere(bottomRight, 50);
            Gizmos.DrawSphere(upLeft, 50);
            Gizmos.DrawSphere(upRight, 50); 
            #endregion
        }
    }

    
}
