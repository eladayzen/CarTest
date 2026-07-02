//Description: SurfaceData. ScriptableObject that contains data about ground surfaces.
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    [CreateAssetMenu(fileName = "SurfaceData", menuName = "TS/SurfaceData")]
    public class SurfaceData : ScriptableObject
    {
        [System.Serializable]
        public class SurfaceParams
        {
            public RoadType     roadType = RoadType.Asphalt;
            public float        speedAmount = 1;
            public float        gripAmount = 1;
            public AudioClip    aSurface;
            public AudioClip    aSkidmark;
            public float        skidmarkPitch = 1;
            public AudioClip    aBrake;

            public SurfaceParams(RoadType _roadType, float _gripAmount, float _speedAmount)
            {
                roadType    = _roadType;
                speedAmount = _speedAmount;
                gripAmount  = _gripAmount;
            }
        }

        public List<SurfaceParams> surfaceList = new List<SurfaceParams>();


        [System.Serializable]
        public class TerrainLayerParams
        {
            public TerrainLayer layer;
            public RoadType     roadType = RoadType.Asphalt;

            public TerrainLayerParams(TerrainLayer _layer, RoadType _roadType)
            {
                layer       = _layer;
                roadType    = _roadType;
            }
        }

        public List<TerrainLayerParams> terrainLayerList = new List<TerrainLayerParams>();

    }
}

