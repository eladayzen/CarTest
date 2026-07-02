// Description: Attached to Grp_ColorPanel. Use to initialize the Color panel when the player customize the color of a vehicle part.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class ColorPanel : MonoBehaviour
    {
        public GameObject           ColorSlotRef;
        public List<GameObject>     slotlist = new List<GameObject>();

        public void InitColorPanel(List<Color> colors,int currentSelection)
        {
            #region
            if (slotlist.Count == 0)
            {
                for (var i = 0; i < transform.childCount; i++)
                    slotlist.Add(transform.GetChild(i).gameObject);
            }

            if(slotlist.Count > 0)
            {
                foreach (GameObject slot in slotlist)
                    Destroy(slot);
            }

            slotlist.Clear();

            int counter = 0;
            foreach (Color newColor in colors)
            {
                GameObject newSlot = Instantiate(ColorSlotRef, this.transform);
                slotlist.Add(newSlot);

                newSlot.transform.GetChild(1).GetComponent<ColorSlot>().Img.color = newColor;
                newSlot.transform.GetChild(1).GetComponent<ColorSlot>().Index = counter;

                counter++;
            }

            TS_EventSystem.instance.eventSystem.SetSelectedGameObject(slotlist[currentSelection].transform.GetChild(1).gameObject);
            #endregion
        }
    }

}
