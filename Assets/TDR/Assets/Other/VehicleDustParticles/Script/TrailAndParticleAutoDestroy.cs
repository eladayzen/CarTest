using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class TrailAndParticleAutoDestroy : MonoBehaviour
    {
        private Vector3     lastPos;
        public bool         forceDestroyProcess = false;
        public float        autoDestroyDuration = 3;

        void Start()
        {
            StartCoroutine(DestroyRoutine());
        }

        IEnumerator DestroyRoutine()
        {
            var t = 0f;
            lastPos = transform.position;

            while (t < autoDestroyDuration)
            {
                if (!PauseManager.instance.Bool_IsGamePaused)
                {
                    t += Time.deltaTime;
                }
                yield return null;
            }

            if (lastPos == transform.position )
            {
                Destroy(this.gameObject);
            }
            else
            {
                StartCoroutine(DestroyRoutine());
            }

            yield return null;
        }
    }
}

