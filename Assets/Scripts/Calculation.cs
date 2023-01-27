using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks; 

namespace moon
{
    [System.Serializable]
    public abstract class Calculation<T>
    {
        bool calculated = false;
        bool calculateOnce = false; 
        protected abstract T Calculate(Player player); 

        public T Value(Player player)
        {
            if (calculateOnce && calculated)
                return default(T);
            else
            {
                calculated = true;
                return Calculate(player);
            }
        }

        public T Test(Player player) => Calculate(player);
    }

    public abstract class IntCalc : Calculation<int> { }
}
