// Description: PredictNextPosition.cs. Predict collision with the layerMask Border
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class PredictNextPosition : MonoBehaviour
    {
        public CarController            controller;
        public List<Transform>          predictionList = new List<Transform>();
        public int                      howManyFramePrediction = 2;
        Collider                        m_Collider;
        public bool                     collisionDetected = false;

        public int                      layerMaskRef         = 14;               // Border
        public List<int>                layerList = new List<int> { 14, 16 };
        // [HideInInspector]
        public LayerMask                layerCollision;
        public Collider                 refBodyCollider;

       

        void Start()
        {
            #region
            string[] layerUsed = new string[layerList.Count];
            for (var i = 0; i < layerList.Count; i++)
                layerUsed[i] = LayerMask.LayerToName(LayersRef.instance.layersListData.listLayerInfo[layerList[i]].layerID);

            layerCollision = LayerMask.GetMask(layerUsed);

            m_Collider = predictionList[0].GetChild(0).GetComponent<BoxCollider>();
            m_Collider.transform.localScale = refBodyCollider.transform.localScale; 
            #endregion
        }

        void FixedUpdate()
        {
            #region
            PredictPosition();
            DetectCollision(); 
            #endregion
        }

        void PredictPosition()
        {
            #region
            // Predicted Position
            predictionList[0].transform.position = transform.position + controller.rb.linearVelocity * Time.fixedDeltaTime * howManyFramePrediction;

            // Predicted Orientation
            float angleIncreasePrediction = controller.rb.angularVelocity.magnitude * Time.fixedDeltaTime * 3;
            Vector3 rotationAxis = controller.rb.angularVelocity.normalized;
            Quaternion newRotation = Quaternion.AngleAxis(angleIncreasePrediction * Mathf.Rad2Deg, rotationAxis);

            predictionList[0].transform.rotation = controller.transform.rotation * newRotation; 
            #endregion
        }
     
        void DetectCollision()
        {
            #region
            Collider[] allColliders = Physics.OverlapBox(m_Collider.bounds.center, m_Collider.transform.localScale * .5f, transform.rotation, layerCollision);
            int counter = 0;
            if (allColliders != null)
            {
                foreach (Collider col in allColliders)
                {
                    var destructibleProps = col.GetComponent<DestructiblePropsTag>();
                      //Debug.Log("layer : " + col.gameObject.layer);
                    if (destructibleProps && !destructibleProps.isAlreadyUsed)
                    {
                        destructibleProps.Explosion(controller.rb.position, controller.rb.linearVelocity.magnitude);
                    }
                    else
                    {
                        if (col.gameObject.layer == layerMaskRef)
                        {
                            counter++;

                            if (!destructibleProps)
                            {
                                if (controller.rb.linearVelocity.magnitude > 30)
                                    controller.rb.linearVelocity *= .9f;

                                controller.rb.AddForceAtPosition(-controller.rb.linearVelocity, controller.rb.position);
                            }
                           
                        }
                    }
                }
            }

            if (counter > 0)
                collisionDetected = true;
            else
                collisionDetected = false;

            #endregion
        }

        void OnDrawGizmos()
        {
            #region
            Gizmos.color = Color.red;

            if (m_Collider)
            {
                Matrix4x4 cubeTransform = Matrix4x4.TRS(m_Collider.transform.position, m_Collider.transform.rotation, m_Collider.transform.localScale);
                Gizmos.matrix = cubeTransform;
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            } 
            #endregion
        }
    }
   
}
