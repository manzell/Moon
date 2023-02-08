using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;

namespace moon
{
    public class Era : SerializedScriptableObject
    {
        public static System.Action EraStartEvent, EraEndEvent;
        [field: SerializeField] public List<Card> Deck { get; private set; }
        [field: SerializeField] public Dictionary<Flag, List<Resource>> Rewards { get; private set; }
        [field: SerializeField] public List<Phase> Phases { get; private set; }
        [field: SerializeField] public int Multiplier { get; private set; }

        public List<Turn> Turns { get; private set; } = new(); 

        public void StartEra()
        {
            foreach (Flag flag in Game.CurrentGame.Rewards.Keys.Union(Rewards.Keys))
            {
                if (Game.CurrentGame.Rewards.ContainsKey(flag))
                    Game.CurrentGame.Rewards[flag].AddRange(Rewards[flag]);
                else
                    Game.CurrentGame.Rewards.Add(flag, Rewards[flag]);
            }

            Game.CurrentGame.AddRepCards(Deck.OfType<ReputationCard>().OrderBy(x => Random.value)
                .Take(Game.CurrentGame.GameSettings.ReputationCardsPerEra)); 
            Game.CurrentGame.AddCardsToDeck(Deck.OfType<PlayCard>());
            Game.CurrentGame.ShuffleDeck();
                        
            Game.CurrentGame.StartPhase();
        }

        public void NextEra()
        {
            Game.CurrentGame.TriggerEndEraEvent_ClientRpc();

            if (Game.CurrentGame.Eras.TryPeek(out Era era))
                era.StartEra();
            else
                Game.CurrentGame.EndGame();
        }
    }
}
