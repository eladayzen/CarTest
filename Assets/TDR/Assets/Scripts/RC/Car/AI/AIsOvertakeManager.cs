using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class AIsOvertakeManager : MonoBehaviour
    {
        public static AIsOvertakeManager    instance = null;

        public List<CarAI>                  carAIList = new List<CarAI>();
        public List<bool>                   carOvertakeStateList = new List<bool>();
        public bool                         isInitDone = false;


        void Awake()
        {
            #region 
            //-> Check if instance already exists
            if (instance == null)
                instance = this; 
            #endregion
        }

        void Start()
        {
            #region 
            StartCoroutine(InitRoutine()); 
            #endregion
        }

        void Update()
        {
            #region 
            if (isInitDone)
            {
                CheckOvertakeState();
            } 
            #endregion
        }

        void CheckOvertakeState()
        {
            #region 
            for (var i = 0; i < carAIList.Count; i++)
            {
                if (carAIList[i].IsAiAllowedToOvertakeCar() && !IsAnOtherCarOvertakeCurrently(i))
                    carOvertakeStateList[i] = true;
                else
                    carOvertakeStateList[i] = false;
            } 
            #endregion
        }

        bool IsAnOtherCarOvertakeCurrently(int playerNumber)
        {
            #region 
            for (var i = 0; i < carOvertakeStateList.Count; i++)
            {
                if (i != playerNumber && carOvertakeStateList[i])
                    return true;
            }

            return false; 
            #endregion
        }

        IEnumerator InitRoutine()
        {
            #region 
            yield return new WaitUntil(() => VehiclesRef.instance.b_InitDone);

            for (var i = 0; i < VehiclesRef.instance.listVehicles.Count; i++)
            {
                carAIList.Add(VehiclesRef.instance.listVehicles[i].GetComponent<CarAI>());
                carOvertakeStateList.Add(false);
            }

            isInitDone = true;

            yield return null; 
            #endregion
        }
    }
}
