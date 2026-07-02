// Description: TS_PostProcess: Access Post Process profile and methods to modified the Post Process at runtime
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TS.Generics
{
    public class TS_PostProcess : MonoBehaviour
    {
        //-> URP
        private Volume              volume;
        private VolumeProfile       volumeProfile;
        private Vignette            vignette;
        private ChromaticAberration chromaticAberration;
        private ColorAdjustments    colorAdjustments;
        private float               currentchromaticAberration;
        private float               currentColorAdjustments;

        // Start is called before the first frame update
        void Start()
        {
            #region 
            // Access volume component
            volume = GetComponent<Volume>();

            // Access volume profile
            volumeProfile = volume.profile;

            // Access Vignette Fx
            volumeProfile.TryGet(out vignette);

            // Access chromaticAberration Fx
            volumeProfile.TryGet(out chromaticAberration);

            // Access colorAdjustments Fx
            volumeProfile.TryGet(out colorAdjustments);

            // Modify a property
            //if (vignette)
            //    vignette.intensity.Override(0.5f); 
            #endregion
        }

        public void BoosterFxOn()
        {
            #region 
            if (chromaticAberration)
            {
                currentchromaticAberration = Mathf.MoveTowards(currentchromaticAberration, 1, Time.deltaTime * 2);
                chromaticAberration.intensity.Override(currentchromaticAberration);
            } 
            #endregion
        }

        public void BoosterFxOff()
        {
            #region 
            if (chromaticAberration)
            {
                currentchromaticAberration = Mathf.MoveTowards(currentchromaticAberration, 0, Time.deltaTime);
                chromaticAberration.intensity.Override(currentchromaticAberration);
            } 
            #endregion
        }

        public void ColorAdjustmentsOn()
        {
            #region 
            if (colorAdjustments && currentColorAdjustments != 100)
            {
                currentColorAdjustments = Mathf.MoveTowards(currentColorAdjustments, -10, Time.deltaTime * 50);
                colorAdjustments.saturation.Override(currentColorAdjustments);
            } 
            #endregion
        }

        public void ColorAdjustmentsOff()
        {
            #region 
            if (colorAdjustments && currentColorAdjustments != 0)
            {
                currentColorAdjustments = Mathf.MoveTowards(currentColorAdjustments, 0, Time.deltaTime * 50);
                colorAdjustments.saturation.Override(currentColorAdjustments);
            } 
            #endregion
        }

    }
}

