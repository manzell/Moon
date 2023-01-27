using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace moon
{
    [System.Serializable]
    public abstract class Phase
    {
        public static System.Action<Phase> PhaseStartEvent, PhaseEndEvent; 
        public System.Action<Phase> phaseStartEvent, phaseEndEvent;

        protected abstract void OnPhase();

        public void StartPhase()
        {
            Debug.Log($"Starting {this}");
            Game.CurrentPhase = this;
            phaseStartEvent?.Invoke(this);
            OnPhase();
        }

        public void EndPhase()
        {
            phaseEndEvent?.Invoke(this);
            NextPhase(this);
        }

        public void NextPhase(Phase previousPhase)
        {
            int i = Game.CurrentEra.Phases.IndexOf(previousPhase) + 1;

            if (Game.CurrentEra.Phases.Count > i)
                Game.CurrentEra.Phases[i].StartPhase();
            else
                Game.CurrentEra.EndEra(); 
        }
    }

    public class ProductionPhase : Phase
    {
        protected override void OnPhase()
        {
            foreach (Player player in Game.Players)
            {
                foreach (ResourceCard card in player.Tableau)
                {
                    player.AddResources(card.productionAction.Value(player));
                    player.ProduceEvent?.Invoke(card); 
                }
            }

            EndPhase();
        }
    }

    public class ConstructionPhase : Phase, IPlayerRounds
    {
        public List<Round> Rounds { get; private set; }

        protected override void OnPhase()
        {
            DealStartingCards();

            Rounds = new(); 
            Round round = new Round();
            Rounds.Add(round);
            round.StartRound(); 
        }

        public void DealStartingCards()
        {
            int cardsToDeal = 6;
            int playerCount = Game.Players.Count;

            if (playerCount < 4) cardsToDeal++;
            if (playerCount < 3) cardsToDeal++;

            Debug.Log($"Dealing {cardsToDeal} cards to {playerCount} {(playerCount == 1 ? "Players" : "Player")}");

            for (int i = 0; i < cardsToDeal; i++)
            {
                foreach (Player player in Game.Players)
                {
                    if (Game.Deck.Count > 0 && player.Hand.Count < cardsToDeal)
                    {
                        Card card = Game.Deck.Pop();
                        Debug.Log($"Dealing {card.name} to {player.name}");
                        
                        player.AddCardsToHand(new List<Card>() { card });
                    }
                }
            }
        }
    }

    public class ScoringPhase : Phase
    {
        public static System.Action<Flag, Player, List<Resource>> FlagRewardEvent; 
        protected override void OnPhase()
        {
            ScoreFlagRewards();

            foreach (Player player in Game.Players)
                GiveVPCardRewards(player);

            NextPhase(this); 
        }

        public void ScoreFlagRewards()
        {
            foreach (Flag flag in Game.Rewards.Keys)
            {
                int max = Game.Players.Max(player => player.Flags.Count(f => f == flag));
                IEnumerable<Player> qualifyingPlayers = Game.Players.Where(player => player.Flags.Count(f => f == flag) == max);
                List<Resource> reward = Game.Rewards[flag]; 

                if (qualifyingPlayers.Count() == 1)
                {
                    qualifyingPlayers.First().AddResources(reward);

                    FlagRewardEvent?.Invoke(flag, qualifyingPlayers.First(), reward);
                    Game.Rewards[flag].Clear();
                }
            }
        }

        public void GiveVPCardRewards(Player player)
        {
            foreach (VictoryCard card in player.Tableau)
            {
                player.AddResources(card.CardResources);

                for(int i = 0; i < card.VP.Value(player); i++)
                    player.AddResources(new List<Resource>() { Game.Resources.vp }); 
            }
        }
    }

    public class FinalScoring : Phase
    {
        protected override void OnPhase() => Game.EndGame(); 
    }
}
