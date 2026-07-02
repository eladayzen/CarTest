// Description: AutoTurn. Help car when it collides with border.
// A force is applied to the car to force the car turn in the opposite direction of the impact.
// Attached to objects named AutoTurn_ inside the vehicle.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class AutoTurn : MonoBehaviour
    {
        bool                            isInitDone = false;
        public CarController            CarControl;
        public LayerMask                borderLayer;
        public List<int>                layerMaskList = new List<int>();

        public float                    RayDistance = 5;
        public Transform                ForcePos;
        public int                      ForceApply = 100000;

        public PredictNextPosition      predictNextPosition;

        bool hitDectected = false;

        void CalculateSpring()
        {
            #region 

            if (Time.frameCount % 4 == CarControl.vehicleInfo.playerNumber % 4 || hitDectected)
            {
                float speedRatio = CarControl.rb.linearVelocity.magnitude / CarControl.maxSpeed;
                if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, RayDistance, borderLayer))
                {
                    var destructibleProps = hit.transform.GetComponent<DestructiblePropsTag>();
                    if (!destructibleProps)
                    {
                        if (predictNextPosition.collisionDetected)
                            CarControl.rb.AddForceAtPosition(ForcePos.transform.forward * ForceApply * speedRatio, ForcePos.position);
                        else
                            CarControl.rb.AddForceAtPosition(ForcePos.transform.forward * ForceApply * speedRatio * .5f, ForcePos.position);

                    }
                    hitDectected = true;

                }
                else
                {
                    hitDectected = false;
                }
            }

               
            #endregion
        }

        void FixedUpdate()
        {
            #region 
            if(isInitDone)
                CalculateSpring(); 
            #endregion
        }

        void Start()
        {
            #region 
            borderLayer = InitLayerMask(layerMaskList);
            isInitDone = true; 
            #endregion
        }

        LayerMask InitLayerMask(List<int> layerList)
        {
            #region //-> Init LayerMask
            string[] layerUsed = new string[layerList.Count];
            for (var i = 0; i < layerList.Count; i++)
                layerUsed[i] = LayerMask.LayerToName(LayersRef.instance.layersListData.listLayerInfo[layerList[i]].layerID);

            return LayerMask.GetMask(layerUsed); 
            #endregion
        }
    }

}
