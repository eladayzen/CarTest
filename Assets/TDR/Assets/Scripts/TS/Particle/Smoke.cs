// Description: Smoke.cs. Attached to wheel.
// Modify at runtime some particle parameters depending the speed and the steering of the vehicle.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class Smoke : MonoBehaviour
    {
        // Info smoke Sprite not on the floor
        public CarController                        carController;
        public ParticleSystem                       ps;
        public ParticleSystemInheritVelocityMode    inheritMode;
        float                                       currentSteer = 0;
        float                                       currentRateOverDistance = 0;

        public float                                rateOverDistanceMax = 5;

        public float minScale = .1f;
        public float maxScale = .2f;

        void Update()
        {
            #region
            if (Time.frameCount % 4 == carController.vehicleInfo.playerNumber % 4)
            {
                float carMag = carController.rb.linearVelocity.magnitude;

                float ratio = carMag / 50f;
                ratio = Mathf.Clamp01(ratio);

                var inheritVelocity = ps.inheritVelocity;

                inheritVelocity.curveMultiplier = .5f * ratio;
                inheritVelocity.mode = inheritMode;

                currentSteer = Mathf.MoveTowards(currentSteer, Mathf.Abs(carController.steer), Time.deltaTime);
                var main = ps.main;
                main.startSize = Mathf.Clamp(.2f * currentSteer, minScale, maxScale);

                var shape = ps.shape;
                shape.radius = Mathf.Clamp(.075f * (1 - ratio) + .075f * currentSteer, .05f, .12f);

                var em = ps.emission;

                float tmpRateOverDistance;
                if (carMag < 4)
                    tmpRateOverDistance = Mathf.MoveTowards(currentRateOverDistance, 0, Time.deltaTime);
                else
                    tmpRateOverDistance = Mathf.MoveTowards(currentRateOverDistance, rateOverDistanceMax, Time.deltaTime);

                em.rateOverDistance = currentRateOverDistance;
                currentRateOverDistance = tmpRateOverDistance;
            }
               
            #endregion
        }
    }

}

