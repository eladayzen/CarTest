using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace TS.Generics
{
    public class OnTriggerFeedback : MonoBehaviour
    {
        public Action<Collider> CarOnTriggerEnterAction;
        public Action<Collision> CarOnCollisionEnterAction;

        void OnTriggerEnter(Collider other)
        {
            CarOnTriggerEnterAction?.Invoke(other);
        }

        void OnCollisionEnter(Collision collision)
        {
            CarOnCollisionEnterAction?.Invoke(collision);
        }
    }

   
    }

