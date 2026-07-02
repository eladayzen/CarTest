// Description: interface IOptiGridCondition<T>: Use to create condition when OptiGrid check if an object is consider as a target.
using UnityEngine;

namespace TS.Generics
{
    public interface IOptiGridCondition<GameObject>
    {
        bool IsThisObjectATarget(GameObject target);
    }
}
