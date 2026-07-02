// Description: DestructiblePropsTag: 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TS.Generics
{
    public class DestructiblePropsTag : MonoBehaviour
    {
        bool m_Started = false;
        public LayerMask m_LayerMask;
        public Collider objCollider;

        public bool isAlreadyUsed = false;

        public List<Rigidbody> rbList = new List<Rigidbody>();
        public List<Transform> overlapBoxList = new List<Transform>();

        [Header("Explosion Parameters")]
        public float minimumCarMagnitude = 20;
        public float force = 1000;
        public float radius = 5;

        [Space]
        [Space]

        public UnityEvent DoSomethingAfterImpactEvent;

        [HideInInspector]
        public float carImpactMag = 0;

        // public Vector3 overlapBoxScale = new Vector3(1, 1, 1);

        public void Explosion(Vector3 pos, float carMag)
        {
            #region
            //Debug.Log(carMag);

            if (carMag >= minimumCarMagnitude)
            {
                carImpactMag = carMag;
                isAlreadyUsed = true;
                GetComponent<BoxCollider>().enabled = false;

                for (var i = 0; i < rbList.Count; i++)
                    rbList[i].isKinematic = false;

                for (var i = 0; i < rbList.Count; i++)
                    rbList[i].AddExplosionForce(force * carMag * .075f, pos, radius);

                DoSomethingAfterImpactEvent?.Invoke();
            }
            #endregion
        }

        public void DisableObject(GameObject obj)
        {
            if (obj) obj.SetActive(false);
        }

        void OnEnable()
        {
            if (m_Started)
            {
                StopAllCoroutines();
                StartCoroutine(CheckIfVehicleIsInsideTheObject());
            }
            m_Started = true;
        }

        void OnDisable()
        {
            if (objCollider)
                objCollider.enabled = false;

            for (var i = 0; i < rbList.Count; i++)
                if (rbList[i]) rbList[i].gameObject.SetActive(false);

        }


        IEnumerator CheckIfVehicleIsInsideTheObject()
        {
            // Debug.Log("Check if ");
            int totalHits = 1;

            while (totalHits > 0)
            {
                totalHits = 0;

                Collider[] hitColliders = Physics.OverlapBox(transform.position, transform.localScale * .5f, transform.rotation, m_LayerMask);

                totalHits += hitColliders.Length;
                //Debug.Log("totalHits A: " + totalHits);
                for (var i = 0; i < rbList.Count; i++)
                {
                    if (rbList[i] && overlapBoxList[i])
                    {
                        hitColliders = Physics.OverlapBox(
                       rbList[i].gameObject.transform.position,
                       overlapBoxList[i].localScale * .5f,
                       rbList[i].gameObject.transform.rotation,
                       m_LayerMask);
                        totalHits += hitColliders.Length;
                    }

                }

                //Debug.Log("totalHits B: " + totalHits);
                yield return new WaitForEndOfFrame();
            }




            if (objCollider)
                objCollider.enabled = true;

            for (var i = 0; i < rbList.Count; i++)
                if (rbList[i]) rbList[i].gameObject.SetActive(true);



            yield return null;
        }

        /* void OnDrawGizmos()
         {
             Gizmos.color = Color.red;
             Matrix4x4 cubeTransform = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
             Gizmos.matrix = cubeTransform;
             Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
         }*/
    }
}

