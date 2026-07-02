using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class DustParticlesManager : MonoBehaviour
    {
        [System.Serializable]
        public class ParticleParams
        {
            public ParticleSystem particleSystem;
            public RoadType surface = RoadType.Default;
            public float rateOverDistance = 3;
        }
        public List<ParticleParams> particleList = new List<ParticleParams>();
    }
}
