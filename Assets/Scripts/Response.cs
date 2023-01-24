using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace moon
{
    [System.Serializable]
    public abstract class Response<T>
    {
        public abstract void Do(T arg);
    }
}
