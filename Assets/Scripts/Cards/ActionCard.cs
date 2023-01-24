using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace moon
{
    public class ActionCard : PlayCard
    {
        public FlipCardAction FlipAction { get; private set; }
    }
}
