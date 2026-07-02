// Description:
using UnityEngine;


namespace TS.Generics
{
    public class AllScenes : MonoBehaviour
    {
        public static AllScenes instance = null;            // Static instance of GameManager which allows it to be accessed by any other script.

        void Awake()
        {
            #region
            //-> Check if instance already exists
            if (instance == null)
                instance = this;

            //-> If instance already exists and it's not this:
            else if (instance != this)
                Destroy(gameObject); 
            #endregion
        }

        void Start()
        {
            #region 
            DontDestroyOnLoad(gameObject); 
            #endregion
        }
    }
}
