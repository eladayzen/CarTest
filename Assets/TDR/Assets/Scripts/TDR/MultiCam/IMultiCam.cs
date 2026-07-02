using UnityEngine;

namespace TS.Generics
{
    public interface IMultiCam
    {
        void InitCam(GameObject obj,int index, CamPreset camPreset );
        void UpdateCam(GameObject obj);
    }
}
