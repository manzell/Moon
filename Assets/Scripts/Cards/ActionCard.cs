using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace moon
{
    public class ActionCard : PlayCard
    {
        [field: SerializeField] public FlipAction FlipAction { get; private set; }
    }

    public abstract class FlipAction : TurnAction
    {
        public bool used { get; private set; } = false;

        public override bool Can(Player player) => used == false && base.Can(player);
        public override void Execute(Player player, bool force = false)
        {
            used = true; 
            base.Execute(player);
        }
    }

    public class RoverStop : FlipAction
    {
        protected override void Do(Player player)
        {
            IEnumerable<IConstructionCard> cards = player.Tableau.OfType<IConstructionCard>().Where(card => card.Rover != null);
            IEnumerable<Resource> rovers = player.Resources.Where(res => res == Game.Resources.rover);
            player.RemoveResources(rovers); 
            (Card as PlayCard).CardResources.AddRange(rovers); 

            cards.ForEach(card => {
                card.SetRover(null);
                (Card as PlayCard).CardResources.Add(Game.Resources.rover);
            });

            player.AddResources(rovers.Select(rover => Game.Resources.vp).Union(cards.Select(card => Game.Resources.vp)));
        }
    }

    public class Printer : FlipAction
    {
        protected override void Do(Player player)
        {

            // Prompt the player to pay 1 metal if they have it.

                // Then prompt the player to select a structure card from the Deck. 
                // Then build the structure in their Tableau
                // Then shuffle the Deck. 
            throw new System.NotImplementedException();
        }
    }

    public class Reservoir : FlipAction
    {
        protected override void Do(Player player)
        {
            player.AddResources(player.Resources.Where(res => res == Game.Resources.water).Select(card => Game.Resources.vp)); 
        }
    }

    public class Charger : FlipAction
    {
        protected override void Do(Player player)
        {
            // Create a Number Selector 
        }
    }
}
