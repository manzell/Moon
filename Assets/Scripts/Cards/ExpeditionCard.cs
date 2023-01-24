using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace moon
{
    [CreateAssetMenu (menuName ="Cards/ExpeditionCard")]
    public class ExpeditionCard : Card
    {
        public TurnAction ExpeditionAction { get; private set; }
        public bool Used { get; private set; }
    }
}
