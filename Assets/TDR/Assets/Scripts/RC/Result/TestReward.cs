using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics{
    public class TestReward : MonoBehaviour
    {

        public RewardAssistant RewardAssist;
      
        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                GameObject newRewardPrefab = Instantiate(RewardAssist.rewardList.rewardParamsList[2].giftPrefab, RewardAssist.grpGifts);

                newRewardPrefab.transform.GetChild(1).GetComponent<CurrentText>().NewTextWithSpecificID(154, 0);
                newRewardPrefab.transform.SetSiblingIndex(1);

                SoundFxManager.instance.Play(SfxList.instance.listAudioClip[RewardAssist.rewardList.sfxID]);
            }
           
        }
    }

}
