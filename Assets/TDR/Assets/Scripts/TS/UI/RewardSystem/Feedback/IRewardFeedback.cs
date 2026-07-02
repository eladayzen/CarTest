// Description: interface IRewardFeedback<T>: Use to validate that a reward feedback ended.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public interface IRewardFeedback
    {
        void AnimProcess();
        bool ISGiftAnimDone();

        void UpdateCoin(int value);
    }
}
