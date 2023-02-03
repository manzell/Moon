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
        public ICard Card { get; private set; }

        public void SetCard(ICard card) => Card = card;
        public virtual void Execute(Player player, bool force = false)
        {
            Player = player;
            game = GameObject.FindObjectOfType<Game>();

            if (force || Can(player))
            {
                Do(player);
                Game.CurrentTurn.Actions.Add(this);
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

        public override bool Can(Player player) => base.Can(player) && !Game.CurrentTurn.Actions.OfType<PlayCardAction>().Any();
    }

    public class BuildAction : PlayCardAction
    {
        public override bool Can(Player player) => base.Can(player) && player.CanAfford(card.ResourceRequirements, card.FlagRequirements);

        protected async override void Do(Player player)
        {
            Debug.Log($"Building {Card.name} in {player.name}'s Tableau");
            player.RemoveResources(card.ResourceRequirements);
            player.RemoveCardsFromHand(new List<Card>() { Card as Card });
            player.AddCardToTableau(card);

            if (Card is ResourceCard resourceCard)
                player.AddResources(await resourceCard.productionAction.Production());
        }

    }

    public class AssimilateAction : PlayCardAction
    {
        public override bool Can(Player player) => !Game.CurrentTurn.Actions.OfType<PlayCardAction>().Any();
        protected override void Do(Player player)
        {
            Debug.Log($"Assimilating {Card.name} for {string.Join(" +", card.AssimilationValue)}");
            player.AddResources(card.AssimilationValue);
            player.RemoveCardsFromHand(new List<Card>() { Card as Card });
        }
    }

    public class ExpeditionAction : TurnAction
    {
        public override bool Can(Player player) => !Game.CurrentTurn.Actions.OfType<ExpeditionAction>().Any();
        protected override void Do(Player player) => (Card as ExpeditionCard).ExpeditionAction.Execute(player);
    }

    public class RoverAction : TurnAction
    {
        List<Resource> cost = new() { Game.Resources.rover };

        public override bool Can(Player player) => player.CanAfford(cost, new List<Flag>()) &&
            !Game.CurrentTurn.Actions.OfType<ExpeditionAction>().Any();

        protected async override void Do(Player player)
        {
            Debug.Log("Rover Action"); 
            player.RemoveResources(cost);
            player.DeployRover(Card as IConstructionCard); 

            (Card as IConstructionCard).SetRover(player); // - How do we let the opposing players know that a Rover has been deployed on their card? 

            if (Card is ResourceCard resourceCard)
                player.AddResources(await resourceCard.productionAction.Production());
        }
    }

    public class FlipCardAction : TurnAction
    {
        public override bool Can(Player player) => !Game.CurrentTurn.Actions.OfType<FlipCardAction>().Any();
        protected override void Do(Player player)
        {
            Debug.Log(Card);
            Debug.Log(Card as ActionCard);
            Debug.Log((Card as ActionCard).FlipAction); 
            (Card as ActionCard).FlipAction.Execute(player);
        }
    }

    public class ClaimReputationAction : TurnAction
    {
        public override bool Can(Player player) => !Game.CurrentTurn.Actions.OfType<ClaimReputationAction>().Any();
        protected override void Do(Player player)
        {
            Game.ReputationCards.Remove(Card as ReputationCard);
            player.ClaimReputationCard(Card as ReputationCard); 
        }
    }

    public class FreeBuildAction : TurnAction
    {
        protected async override void Do(Player player)
        {
            Debug.Log($"Building {Card.name} in {player.name}'s Tableau");
            player.RemoveCardsFromHand(new List<Card>() { Card as Card });
            player.AddCardToTableau(Card as PlayCard);

            if (Card is ResourceCard resourceCard)
                player.AddResources(await resourceCard.productionAction.Production());
        }
    }
}
