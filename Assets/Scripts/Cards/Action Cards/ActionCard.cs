using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace moon
{
    public class ActionCard : PlayCard
    {
        [field: SerializeReference] public FlipAction FlipAction { get; private set; }
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
        protected override async void Do(Player player)
        {
            if(player.Resources.Count(res => res == Game.Resources.metals) > 0)
            {
                Selection<Resource> selection = new(player, new List<Resource>() { Game.Resources.metals });
                await selection.Completion;

                if (selection.SelectedItem != null)
                {
                    Selection<IConstructionCard> buildSelection = new(player, Game.CurrentGame.Deck.OfType<IConstructionCard>());
                    await buildSelection.Completion; 

                    if(buildSelection.SelectedItem != null)
                    {
                        TurnAction action = new FreeBuildAction(buildSelection.SelectedItem);
                        action.Execute(player);

                        Game.CurrentGame.ShuffleDeck(); 
                    }
                }
            }
        }
    }

    public class Reservoir : FlipAction
    {
        protected override void Do(Player player)
        {
            player.AddResources(player.Resources.Where(res => res == Game.Resources.water).Select(res => Game.Resources.vp));                                 
        }
    }

    public class Charger : FlipAction
    {
        protected override async void Do(Player player)
        {
            NumberSelection selection = new(0, player.Resources.Count(res => res == Game.Resources.energy));
            Game.CurrentGame.SelectionWindow.SetTitle($"Discard any amount of energy and score {Game.CurrentGame.CurrentEra.Multiplier} VP for each");

            await selection.Completion;
            Debug.Log($"Charger->AddResources {selection.SelectedItem} x {Game.CurrentGame.CurrentEra.Multiplier} (Multiplayer)");
            player.AddResources(Enumerable.Repeat(Game.Resources.vp, selection.SelectedItem * Game.CurrentGame.CurrentEra.Multiplier)); 
        }
    }
}
