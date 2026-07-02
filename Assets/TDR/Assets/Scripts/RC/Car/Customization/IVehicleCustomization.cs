// Description: Interface used by scripts: VehicleCustomColor, VehicleCustomModels, VehicleCustomvalue
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public interface IVehicleCustomization
    {
        void CustomizationProcess();

        int GetCurrentSelection();

        void SetCurrentSelection(int value);

        void SetIndexSection(int indexSection);

        void SetVehicleIDCategory(int vehicleIDCategory);

        Sprite GetSprite();

        int GetItemsNumber();

        void InitCustomPerformance();

        List<int> GetPrice();
    }
}
