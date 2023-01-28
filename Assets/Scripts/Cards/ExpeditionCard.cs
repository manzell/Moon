using System.Collections;
using System.Collections.Generic;
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
        protected override void Do(Player player)
        {
            // Select a card in hand to Discard

            player.AddCardsToHand(new List<Card>() { Game.Deck.Pop() }); 
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

            Game.CurrentEra.Rewards[flag].Add(resource); 
        }
    }

    public class InfluentialExpedition : TurnAction
    {
        protected override void Do(Player player)
        {
            // Select a Flag
            Flag flag = new();

            Game.CurrentEra.Rewards[flag].Add(Game.Resources.vp);
        }
    }

    public class GearheadExpedition : TurnAction
    {
        protected override void Do(Player player)
        {
            ActionEvent += Reward;
            Turn.EndTurnEvent += turn => ActionEvent -= Reward; 
        }

        void Reward(TurnAction turn)
        {
            turn.Player.AddResources(new List<Resource>() { Game.Resources.vp });
            ActionEvent -= Reward; 
        }
    }

    public class ResourcefulExpedition : TurnAction
    {
        protected override void Do(Player player)
        {
            // Select Resource
            Resource resource = Game.Resources.wildcard;

            player.AddResources(new List<Resource>() { resource });
        }
    }
}
