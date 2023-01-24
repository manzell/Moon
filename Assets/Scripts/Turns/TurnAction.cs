using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; 

namespace moon
{
    [System.Serializable] 
    public abstract class TurnAction
    {
        protected Game game; 
        public Player player;

        public TurnAction(Player player)
        {
            this.player = player; 
            game = GameObject.FindObjectOfType<Game>(); 
        }

        public void Execute()
        {
            Debug.Log($"Trying {this}");

            if (Can())
                Do();
            else
                Debug.Log($"Cannot Do {this}"); 
        }

        public void Force() => Do(); 

        protected abstract void Do();
        public virtual bool Can() => true; 
    }

    public class PlayCardAction : TurnAction
    {
        protected PlayCard card;
        public PlayCardAction(Player player, PlayCard card) : base(player) => this.card = card; 

        public override bool Can() => player.CanAfford(card.ResourceRequirements, card.FlagRequirements) &&
            !Game.CurrentTurn.Actions.OfType<PlayCardAction>().Any(); 

        protected override void Do()
        {
            player.RemoveResources(card.ResourceRequirements);
            player.RemoveCardFromHand(card);
            player.AddCardToTableau(card); 
        }
    }

    public class AssimilateAction : PlayCardAction
    {
        public AssimilateAction(Player player, PlayCard card) : base(player, card) { }

        public override bool Can() => !Game.CurrentTurn.Actions.OfType<PlayCardAction>().Any();
        protected override void Do()
        {
            player.RemoveCardFromHand(card);
            player.AddResources(card.AssimilationValue); 
        }
    }

    public class ExpeditionAction : TurnAction
    {
        ExpeditionCard card; 
        public ExpeditionAction(ExpeditionCard card, Player player) : base(player) => this.card = card;

        public override bool Can() => !Game.CurrentTurn.Actions.OfType<ExpeditionAction>().Any();
        protected override void Do() => card.ExpeditionAction.Execute(); // NOTE - How and when do we set the player on an Expedition Card? We'll do it later. 
    }

    public class RoverAction : TurnAction
    {
        IConstructionCard destination;
        List<Resource> cost = new() { Game.Resources.rover };
        public RoverAction(Player player, IConstructionCard destination) : base(player) => this.destination = destination;

        public override bool Can() => player.CanAfford(cost, new List<Flag>()) &&
            !Game.CurrentTurn.Actions.OfType<ExpeditionAction>().Any();

        protected override void Do()
        {
            player.RemoveResources(cost);
            player.DeployRover(destination); 

            //destination.SetRover(player); - How do we let the opposing players know that a Rover has been deployed on their card? 

            if (destination is ResourceCard resourceCard)
                player.AddResources(resourceCard.productionAction.Value(player)); 
        }
    }

    public class FlipCardAction : TurnAction
    {
        ActionCard card;
        public FlipCardAction(ActionCard card, Player player) : base(player) => this.card = card; 

        public override bool Can() => !Game.CurrentTurn.Actions.OfType<FlipCardAction>().Any();
        protected override void Do() => card.FlipAction.Execute(); // NOTE - How and when do we set the player on an Flip Card? We'll do it later. 
    }

    public class ClaimReputationAction : TurnAction
    {
        ReputationCard card;
        public ClaimReputationAction(ReputationCard card, Player player) : base(player) => this.card = card;

        public override bool Can() => !Game.CurrentTurn.Actions.OfType<ClaimReputationAction>().Any();
        protected override void Do()
        {
            Game.ReputationCards.Remove(card); // Need to throw an event? Maybe Game.ClaimReputationCard needs to be a client RPC
            player.AddReputationCard(card); 
        }
    }
}