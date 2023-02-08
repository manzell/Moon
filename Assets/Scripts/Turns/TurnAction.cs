using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; 

namespace moon
{
    [System.Serializable] 
    public abstract class TurnAction
    {
        public static System.Action<TurnAction> ActionEvent; 

        protected Game game; 
        public Player Player { get; private set; }
        public ICard Card { get; protected set; }

        public virtual void Execute(Player player, bool force = false)
        {
            Player = player;

            if (force || Can(player))
            {
                Do(player);
                Game.CurrentGame.CurrentTurn.Actions.Add(this);
                ActionEvent?.Invoke(this); 
            }
            else
            {
                Debug.Log($"Cannot Do {this}");
            }
        }

        protected abstract void Do(Player player);
        public virtual bool Can(Player player) => true;
    }

    public abstract class PlayCardAction : TurnAction
    {
        protected PlayCard card => Card as PlayCard;

        public override bool Can(Player player) => base.Can(player) && !Game.CurrentGame.CurrentTurn.Actions.OfType<PlayCardAction>().Any();
    }

    public class BuildAction : PlayCardAction
    {
        public BuildAction(ICard card) => Card = card; 
        public override bool Can(Player player) => base.Can(player) && player.CanAfford(card.ResourceRequirements, card.FlagRequirements);

        protected async override void Do(Player player)
        {
            player.RemoveResources(card.ResourceRequirements);
            player.RemoveCardFromHand(Card as Card);
            player.AddCardToTableau(card);

            if (Card is ResourceCard resourceCard)
                player.AddResources(await resourceCard.productionAction.Production(player));
        }
    }

    public class AssimilateAction : PlayCardAction
    {
        public AssimilateAction(ICard card) => Card = card;
        public override bool Can(Player player) => !Game.CurrentGame.CurrentTurn.Actions.OfType<PlayCardAction>().Any();
        protected override void Do(Player player)
        {
            player.AddResources(card.AssimilationValue);
            player.RemoveCardFromHand(Card as Card);
        }
    }

    public class ExpeditionAction : TurnAction
    {
        public ExpeditionAction(ICard card) => Card = card;
        public override bool Can(Player player) => !Game.CurrentGame.CurrentTurn.Actions.OfType<ExpeditionAction>().Any();
        protected override void Do(Player player) => (Card as ExpeditionCard).ExpeditionAction.Execute(player);
    }

    public class RoverAction : TurnAction
    {
        public RoverAction(ICard card) => Card = card;
        List<Resource> cost = new() { Game.Resources.rover };

        public override bool Can(Player player)
        {
            bool canAfford = player.CanAfford(cost, new List<Flag>());
            bool noExpeditions = !Game.CurrentGame.CurrentTurn.Actions.OfType<ExpeditionAction>().Any();

            if (!canAfford)
                Debug.Log($"{player.name} Does not have a Rover");

            if (!noExpeditions)
                Debug.Log($"{player.name} has already deployed a rover this turn"); 

            return canAfford && noExpeditions;
        }

        protected async override void Do(Player player)
        {
            player.RemoveResources(cost);
            player.DeployRover(Card as IConstructionCard); 

            // This needs to happen downstream of an event. Go to the server instead
            Game.CurrentGame.SetRover_ServerRpc(Card.ID, player.OwnerClientId); 

            if (Card is ResourceCard resourceCard)
                player.AddResources(await resourceCard.productionAction.Production(player));
        }
    }

    public class FlipCardAction : TurnAction
    {
        public FlipCardAction(ICard card) => Card = card;
        public override bool Can(Player player) => !Game.CurrentGame.CurrentTurn.Actions.OfType<FlipCardAction>().Any();
        protected override void Do(Player player) => (Card as ActionCard).FlipAction.Execute(player);
    }

    public class ClaimReputationAction : TurnAction
    {
        public ClaimReputationAction(ICard card) => Card = card;
        public override bool Can(Player player) => !Game.CurrentGame.CurrentTurn.Actions.OfType<ClaimReputationAction>().Any();
        protected override void Do(Player player)
        {
            Game.CurrentGame.ReputationCards.Remove(Card as ReputationCard);
            player.ClaimReputationCard(Card as ReputationCard); 
        }
    }

    public class FreeBuildAction : TurnAction
    {
        public FreeBuildAction(ICard card) => Card = card;
        protected async override void Do(Player player)
        {
            Debug.Log($"Building {Card.name} in {player.name}'s Tableau");
            player.RemoveCardFromHand(Card as Card);
            player.AddCardToTableau(Card as PlayCard);

            if (Card is ResourceCard resourceCard)
                player.AddResources(await resourceCard.productionAction.Production(player));
        }
    }
}
