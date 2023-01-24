using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace moon
{
    public class VictoryCard : PlayCard
    {
        [field: SerializeField]  public Calculation<List<Resource>> VP { get; private set; }
    }
}
