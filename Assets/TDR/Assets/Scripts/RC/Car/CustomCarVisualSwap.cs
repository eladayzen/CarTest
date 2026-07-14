// Description: Attached to CarRef. Swaps in a custom body mesh only on the human player's
// car instance, keeping every AI car on the stock body. Purely visual, no other systems touched.
using System.Collections;
using UnityEngine;

namespace TS.Generics
{
    public class CustomCarVisualSwap : MonoBehaviour
    {
        [SerializeField] private GameObject customBody;
        [SerializeField] private GameObject stockBody;

        private VehicleInfo vehicleInfo;

        void Awake()
        {
            vehicleInfo = GetComponent<VehicleInfo>();
            if (customBody != null)
                customBody.SetActive(false);
        }

        void Start()
        {
            StartCoroutine(ApplyWhenReady());
        }

        IEnumerator ApplyWhenReady()
        {
            yield return new WaitUntil(() => vehicleInfo != null && vehicleInfo.b_InitDone
                && InfoRememberMainMenuSelection.instance != null);

            bool isHumanPlayer = vehicleInfo.playerNumber < InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer;
            if (!isHumanPlayer)
                yield break;

            if (customBody != null)
                customBody.SetActive(true);
            if (stockBody != null)
                stockBody.SetActive(false);
        }
    }
}
