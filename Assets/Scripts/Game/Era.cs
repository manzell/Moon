using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector; 

namespace moon
{
    public class Era : SerializedScriptableObject
    {
        public static System.Action<Era> EraStartEvent, EraEndEvent;
        [field: SerializeField] public List<Card> Deck { get; private set; }
        [field: SerializeField] public Dictionary<Flag, List<Resource>> Rewards { get; private set; }
        [field: SerializeField] public List<Phase> Phases { get; private set; }
        [field: SerializeField] public int Multiplier { get; private set; }

        public List<Turn> Turns { get; private set; }

        public void StartEra()
        {
            Debug.Log($"Starting {name}");
            Game.CurrentEra = this;
            Turns = new(); 

            AddEraCardsToDeck();
            AddEraRewards();

            EraStartEvent?.Invoke(this);

            Phases.First().StartPhase(); 
        }

        public void NextEra(Era previousEra)
        {
            if (Game.Eras.Count > 0)
                Game.Eras.Dequeue().StartEra();
            else
                Game.EndGame();
        }

        public void EndEra()
        {
            EraEndEvent?.Invoke(this);
            NextEra(this);
        }

        public void AddEraCardsToDeck()
        {
            Game game = FindObjectOfType<Game>();

            foreach (PlayCard card in Deck.OfType<PlayCard>().OrderBy(x => Random.value))
                game.AddCardToDeck_ClientRpc(card.ID);

            game.AddReputationCards(Deck.OfType<ReputationCard>().OrderBy(x => Random.value).Take(3));
                           

        }

        public void AddEraRewards()
        {
            // Adds Rewards for the Upcoming Era to the general rewards pile
            foreach (Flag flag in Game.Rewards.Keys.Union(Rewards.Keys))
            {
                if (Game.Rewards.ContainsKey(flag))
                    Game.Rewards[flag].AddRange(Rewards[flag]);
                else
                    Game.Rewards.Add(flag, Rewards[flag]);
            }
        }
    }
}
