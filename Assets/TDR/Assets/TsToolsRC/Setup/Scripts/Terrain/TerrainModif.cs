// Description: TerrainModif: Attached to road,fence, wire.
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class TerrainModif : MonoBehaviour
    {
        [HideInInspector]
        public bool seeInspector = false;

        public List<Terrain> terrList = new List<Terrain>();

        public float roadOffsetHeight = .03f;
    }

}
