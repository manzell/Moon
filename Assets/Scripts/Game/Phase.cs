using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace moon
{
    [System.Serializable]
    public abstract class Phase
    {
        public static System.Action PhaseStartEvent, PhaseEndEvent; 

        protected abstract void OnPhase();

        public void StartPhase() => OnPhase(); 

        public void NextPhase()
        {
            int i = Game.CurrentGame.CurrentEra.Phases.IndexOf(this) + 1;
            Game.CurrentGame.TriggerEndPhaseEvent_ClientRpc();

            if (Game.CurrentGame.CurrentEra.Phases.Count > i)
                Game.CurrentGame.StartPhase(); 
            else
                Game.CurrentGame.CurrentEra.NextEra();
        }
    }

    public class ProductionPhase : Phase
    {
        protected async override void OnPhase()
        {
            foreach(Player player in Game.CurrentGame.Players)
            {
                player.AddResources(new List<Resource>() { player.BaseCard.ProductionResource });

                foreach (ResourceCard card in player.Tableau)
                {
                    List<Resource> resources = await card.productionAction.Production(Game.CurrentGame.Player);
                    Game.CurrentGame.Player.AddResources(resources);
                }
            }
            
            NextPhase();
        }
    }

    public class ConstructionPhase : Phase, IPlayerRounds
    {
        public List<Round> Rounds { get; private set; }

        protected override void OnPhase()
        {
            DealEraCards();
            Game.CurrentGame.StartRound(); 
        }

        public void DealEraCards()
        {
            int cardsToDeal = 6;
            int playerCount = Game.CurrentGame.Players.Count;

            if (playerCount < 4) cardsToDeal++;
            if (playerCount < 3) cardsToDeal++;

            Debug.Log($"Dealing {cardsToDeal} cards to {playerCount} {(playerCount == 1 ? "Players" : "Player")} from deck of {Game.CurrentGame.Deck.Count} Cards");

            for (int i = 0; i < cardsToDeal; i++)
            {
                foreach (Player player in Game.CurrentGame.Players)
                {
                    if (player.Hand.OfType<PlayCard>().Count() < cardsToDeal && Game.CurrentGame.Deck.Any())
                        player.AddCardToHand(Game.CurrentGame.Deck.Pop());
                }
            }
        }
    }

    public class ScoringPhase : Phase
    {
        public static System.Action<Flag, Player, List<Resource>> FlagRewardEvent; 
        protected override void OnPhase()
        {
            foreach (Player player in Game.CurrentGame.Players)
                GiveVPCardRewards(player);

            ScoreFlagRewards();
            NextPhase(); 
        }

        public void ScoreFlagRewards()
        {
            foreach (Flag flag in Game.CurrentGame.Rewards.Keys)
            {
                int max = Game.CurrentGame.Players.Max(player => player.Flags.Count(f => f == flag));
                IEnumerable<Player> qualifyingPlayers = Game.CurrentGame.Players.Where(player => player.Flags.Count(f => f == flag) == max);
                List<Resource> reward = Game.CurrentGame.Rewards[flag]; 

                if (qualifyingPlayers.Count() == 1)
                {
                    qualifyingPlayers.First().AddResources(reward);

                    FlagRewardEvent?.Invoke(flag, qualifyingPlayers.First(), reward);
                    Game.CurrentGame.Rewards[flag].Clear();
                }
            }
        }

        public void GiveVPCardRewards(Player player)
        {
            foreach (VictoryCard card in player.Tableau)
            {
                player.AddResources(card.CardResources);
                player.AddResources(Enumerable.Repeat(Game.Resources.vp, card.VP.Value(player)));
            }
        }
    }

    public class FinalScoring : Phase
    {
        protected override void OnPhase() => Game.CurrentGame.EndGame(); 
    }
}
