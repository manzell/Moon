using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.Netcode;
using TMPro;
using Sirenix.Utilities;

namespace moon
{
    public class Era : SerializedScriptableObject
    {
        public static System.Action EraStartEvent, EraEndEvent;
        [field: SerializeField] public List<Card> Deck { get; private set; }
        [field: SerializeField] public Dictionary<Flag, List<Resource>> Rewards { get; private set; }
        [field: SerializeField] public List<Phase> Phases { get; private set; }
        [field: SerializeField] public int Multiplier { get; private set; }

        public List<Turn> Turns { get; private set; }

        public void StartEra()
        {
            Game.AddCardsToDeck(Deck.OfType<PlayCard>());
            Game.ShuffleDeck();
            Game.AddReputationCards(Deck.OfType<ReputationCard>().OrderBy(x => Random.value).Take(Game.GameSettings.ReputationCardsPerEra));
            Turns = new();

            foreach (Flag flag in Game.Rewards.Keys.Union(Rewards.Keys))
            {
                if (Game.Rewards.ContainsKey(flag))
                    Game.Rewards[flag].AddRange(Rewards[flag]);
                else
                    Game.Rewards.Add(flag, Rewards[flag]);
            }

            FindObjectOfType<Game>().TriggerStartEraEvent_ClientRpc();
            Phases.First().StartPhase();
        }

        public void NextEra()
        {
            FindObjectOfType<Game>().TriggerEndEraEvent_ClientRpc();

            if (Game.Eras.TryPeek(out Era era))
                era.StartEra();
            else
                Game.EndGame();
        }
    }
}
