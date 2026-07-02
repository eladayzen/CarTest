// Description: CarReflectionProbe. Update the renderer probe manually
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class CarReflectionProbe : MonoBehaviour
    {
        public bool             isInitDone = false;

        public ReflectionProbe  reflectionProbe;
        public VehicleInfo      vehicleInfo;
    }
}
