using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace moon
{
    [CreateAssetMenu (menuName = "Card/Base Card")]
    public class BaseCard : ScriptableObject
    {
        public string Name { get; private set; }

        public Resource ProductionResource { get; private set; }
        public Flag Flag { get; private set; }
    }
}
