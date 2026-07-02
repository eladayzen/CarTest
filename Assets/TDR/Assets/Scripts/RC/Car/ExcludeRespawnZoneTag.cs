// Description: ExcludeRespawnZoneTag. A vehicle is anot allowed to respawn inside this object.
// The vehicle is repawned on respawnPosition position and rotation
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class ExcludeRespawnZoneTag : MonoBehaviour
    {
        public Transform respawnPosition;

        void OnDrawGizmosSelected()
        {
            #region
            if (respawnPosition)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(respawnPosition.position, 3);
            }
            #endregion
        }
    }

}
