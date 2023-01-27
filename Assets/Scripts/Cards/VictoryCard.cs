using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace moon
{
    public class VictoryCard : PlayCard
    {
        [field: SerializeReference] public IntCalc VP { get; private set; }
    }

    [System.Serializable]
    public class VPPerResourceSpent : IntCalc
    {
        [SerializeField] Resource resource;

        protected override int Calculate(Player player) => Game.CurrentEra.Turns.Where(turn => turn.Player == player)
            .SelectMany(turn => turn.Actions.OfType<PlayCardAction>()).Sum(action => (action.Card as PlayCard).ResourceRequirements.Count(res => res == resource));
    }

    [System.Serializable]
    public class VPPerResourceProduction : IntCalc
    {
        [SerializeField] Resource resource;
        protected override int Calculate(Player player) => player.Tableau.OfType<ResourceCard>()
            .Sum(card => card.CardResources.Count(res => res == resource));
    }

    [System.Serializable]
    public class VPPerResource : IntCalc
    {
        [SerializeField] Resource resource;
        protected override int Calculate(Player player) => player.Resources.Count(res => res == resource);
    }
}
