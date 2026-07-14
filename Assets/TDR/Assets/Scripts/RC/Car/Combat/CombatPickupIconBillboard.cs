// Description: CombatPickupIconBillboard. Keeps a world-space icon canvas riding the
// near-camera face of its pickup sphere - repositioned toward the camera every frame (not just
// rotated) so the icon reads as painted on the visible surface instead of buried inside the
// opaque mesh, and always faces the camera regardless of the pickup's own spin.
using UnityEngine;

namespace TS.Generics
{
    public class CombatPickupIconBillboard : MonoBehaviour
    {
        public Transform              center;
        public float                  offsetRadius = 0.6f;

        Camera                        cam;

        void LateUpdate()
        {
            #region
            if (cam == null) cam = Camera.main;
            if (cam == null || center == null) return;

            Vector3 toCam = cam.transform.position - center.position;
            if (toCam.sqrMagnitude < 0.0001f) return;
            toCam.Normalize();

            transform.position = center.position + toCam * offsetRadius;
            transform.rotation = Quaternion.LookRotation(toCam);
            #endregion
        }
    }
}
