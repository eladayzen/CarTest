// Description: ObstaclePosition. used to tag obstacle.
using UnityEngine;

namespace TS.Generics
{
    public class ObstaclePosition : MonoBehaviour
    {
        public int              closestCheckPoint = -1;
     
        public int              dir = 1;
        public float            distToPath = 0;

        public PathObstacle     PathObstacle;

        public bool             isObstacleAVehicle = false;

        void OnDrawGizmosSelected()
        {
        }
    }
}
