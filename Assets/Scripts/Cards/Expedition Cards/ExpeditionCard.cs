using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace moon
{
    public class ExpeditionCard : Card
    {
        [field: SerializeField] public string expeditionText { get; private set; }
        [field: SerializeReference] public TurnAction ExpeditionAction { get; private set; }
        public bool Used { get; private set; }
    }

    public class FirstExpeditionAction : TurnAction
    {
        protected override async void Do(Player player)
        {
            Selection<PlayCard> selection = new(player, player.Hand.OfType<PlayCard>());
            await selection.Completion; 

            if(selection.SelectedItem != null)
            {
                player.RemoveCardFromHand(selection.SelectedItem);
                player.AddCardToHand(Game.CurrentGame.Deck.Pop());
            }
        }
    }

    public class ImpulsiveExpedition : TurnAction
    {
        protected override void Do(Player player)
        {
            // Select a Flag
            Flag flag = new();

            // Select a Resource
            Resource resource = Game.Resources.wildcard;

            Game.CurrentGame.CurrentEra.Rewards[flag].Add(resource); 
        }
    }

    public class InfluentialExpedition : TurnAction
    {
        protected override async void Do(Player player)
        {
            Selection<Flag> selection = new(player, Game.Flags.all); 
            await selection.Completion;

            Game.CurrentGame.CurrentEra.Rewards[selection.SelectedItem].Add(Game.Resources.vp);
        }
    }

    public class GearheadExpedition : TurnAction
    {
        protected override void Do(Player player)
        {
            ActionEvent += Reward;
            Turn.EndTurnEvent += turn => ActionEvent -= Reward;

            void Reward(TurnAction turn)
            {
                if (turn is RoverAction && turn.Player == player)
                {
                    turn.Player.AddResources(new List<Resource>() { Game.Resources.vp });
                    ActionEvent -= Reward;
                }
            }
        }
    }

    public class ResourcefulExpedition : TurnAction
    {
        protected override async void Do(Player player)
        {
            Selection<Resource> selection = new(player, new List<Resource>() { Game.Resources.water, Game.Resources.energy, Game.Resources.metals, Game.Resources.organics });

            await selection.Completion;

            player.AddResources(new List<Resource>() { selection.SelectedItem });
        }
    }
}
