using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace moon
{
    [CreateAssetMenu (menuName = "Cards/Base Card")]
    public class BaseCard : Card
    {
        [field: SerializeField] public Resource ProductionResource { get; private set; }
        [field: SerializeField] public Flag Flag { get; private set; }
    }
}
