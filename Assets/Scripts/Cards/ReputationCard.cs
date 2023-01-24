using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace moon
{
    [CreateAssetMenu (menuName ="Cards/Reputation Card")]
    public class ReputationCard : Card
    {
        public enum Level { Bronze, Silver, Gold }

        public string Name { get; private set; }
        public Level GoalLevel { get; private set; }
        public int VP { get; private set; }

        public Conditional ClaimCondition { get; private set; }
        public CardAction OnClaimAction { get; private set; } 
    }
}
