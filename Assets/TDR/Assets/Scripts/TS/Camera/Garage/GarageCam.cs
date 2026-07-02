// Description: GarageCam. Use to move a camera and look at a target.
// The script is used in the main menu, on Preview track system and on the result sequence (Podium)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class GarageCam : MonoBehaviour
    {
        public List<Transform>      posList = new List<Transform>();
        public Transform            target;
        public Transform            Camera;
        private int                 currentTextTarget = 0;
        public float                durationBetweenTwoPoints = 3;

        public AnimationCurve       animCurve;

        public bool                 isAvailable = true;

        public enum MovingObject    { Camera, Target}
        public MovingObject         movingObject = MovingObject.Camera;

        Transform                   objToMove;

        void Start()
        {
        }

        void OnDisable()
        {
            #region
            StopAllCoroutines();
            #endregion
        }

        void OnEnable()
        {
            #region
            if (isAvailable)
                StartCoroutine(MoveCamRoutine());
            #endregion
        }

        IEnumerator MoveCamRoutine()
        {
            #region
            float t = 0;

            if (objToMove == null)
            {
                if (movingObject == MovingObject.Target)
                    objToMove = target;
                else
                    objToMove = Camera;
            }

            while (t < 1)
            {
                t += Time.deltaTime / durationBetweenTwoPoints;
                objToMove.position = Vector3.Lerp(posList[currentTextTarget].position, posList[(currentTextTarget + 1) % posList.Count].position, animCurve.Evaluate(t));
                Camera.transform.LookAt(target);
                yield return null;
            }

            currentTextTarget ++;
            currentTextTarget %= posList.Count;

            StartCoroutine(MoveCamRoutine());
            yield return null;
            #endregion
        }
    }

}
