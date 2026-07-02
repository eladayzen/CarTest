using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    [CreateAssetMenu(fileName = "dataThumbnail", menuName = "TS/ThumbnailData")]
    public class ThumbnailData : ScriptableObject
    {
        public string path = "Assets/";
        public string screenshotName = "Thumbnail_01";

        /*public bool MoreOptions;
        public bool HelpBox;
        public int currentelectedDatas = 0;
        */
    }
}

