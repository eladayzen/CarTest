// This script is attached to the camera use to set up the vehicle camera position.
using UnityEngine;

namespace TS.Generics
{
    public class VehicleCamPresetTag : MonoBehaviour
    {
        private void Awake()
        {
            Destroy(gameObject);
        }
    }

}
