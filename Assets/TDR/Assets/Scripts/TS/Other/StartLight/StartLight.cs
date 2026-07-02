// Description: Used to create a start Light during the countdown
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TS.Generics
{
    public class StartLight : MonoBehaviour
    {
        public int                      id = 0;
        public UnityEvent               eventInitStartLight;
        public List<UnityEvent>         eventsList = new List<UnityEvent>();

        public void Start()
        {
            #region
            eventInitStartLight?.Invoke();
            #endregion
        }

        public void StartEventList(int listID)
        {
            #region
            eventsList[listID]?.Invoke();
            #endregion
        }


        public void Gray(GameObject obj)
        {
            #region 
            // Debug.Log("Gray");
            obj.GetComponent<Renderer>().material.SetColor("_EmissionColor", new Color(0f, 0f, 0f)); 
            #endregion

        }

        public void Red(GameObject obj)
        {
            #region
            //Debug.Log("Red");
            obj.GetComponent<Renderer>().material.SetColor("_EmissionColor", new Color(.6f, 0, 0)); 
            #endregion
        }

        public void Green(GameObject obj)
        {
            #region 
            obj.GetComponent<Renderer>().material.SetColor("_EmissionColor", new Color(0, .3f, 0)); 
            #endregion
        }
    }
}
