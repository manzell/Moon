using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using Sirenix.Utilities;

namespace moon
{
    public class Game : NetworkBehaviour
    {
        public static System.Action<Player> AddPlayerEvent;
        public static System.Action StartGameEvent, EndGameEvent;
        public static System.Action<ReputationCard> ClaimRepCardEvent, AddRepCardEvent;

        public static Game CurrentGame { get; private set; }
        public static Graphics Graphics;
        public static Resources Resources;
        public static Flags Flags;
        
        [SerializeField] Graphics graphics;
        [SerializeField] Resources resources;
        [SerializeField] Flags flags;

        public GameSettings GameSettings => CurrentGame.gameSettings;
        [SerializeField] GameSettings gameSettings;

        public UI_SelectionWindow SelectionWindow => CurrentGame.selectionWindowPrefab; 
        [SerializeField] UI_SelectionWindow selectionWindowPrefab; 

        public Player Player;
        public List<Player> Players = new();

        public Queue<Era> Eras { get; private set; } 
        public HashSet<Card> Cards { get; private set; } = new(); 
        public Stack<PlayCard> Deck { get; private set; } = new(); 
        public List<ReputationCard> ReputationCards { get; private set; } = new();
        public List<ExpeditionCard> ExpeditionCards { get; private set; } = new();
        public List<BaseCard> BaseCards { get; private set; } = new();
        public List<PlayCard> Discards { get; private set; } = new(); 
        public Dictionary<Flag, List<Resource>> Rewards { get; private set; } = new(); 

        public Era CurrentEra;
        public Phase CurrentPhase;
        public Round CurrentRound;
        public Turn CurrentTurn;

        void Awake()
        {
            CurrentGame = this; 
            Graphics = graphics;
            Flags = flags;
            Resources = resources;
            Eras = new(GameSettings.Eras);
            Cards.Add(gameSettings.FirstPlayerExpedition);
            Cards.UnionWith(gameSettings.ExpeditionCards);
            Cards.UnionWith(gameSettings.Bases);
            Cards.UnionWith(gameSettings.Eras.SelectMany(era => era.Deck)); 
        }

        public void AddPlayer(Player player)
        {
            if (Players.Count < GameSettings.MaxPlayers)
            {
                Players.Add(player);
                player.name = $"Player {Players.Count}";

                Debug.Log($"{player.name} Added to the Game!");

                if (player.IsOwner)
                    Player = player;

                AddPlayerEvent?.Invoke(player);  
            }
        }

        #region Game-Turn-Order-Management

        [ServerRpc] public void StartGame_ServerRpc()
        {
            if (Players.Count > 0)
            {
                BaseCards.AddRange(gameSettings.Bases.OrderBy(x => Random.value));
                ExpeditionCards.AddRange(gameSettings.ExpeditionCards.OrderBy(x => Random.value));

                for (int i = 0; i < Players.Count; i++)
                {
                    Player player = Players[i];
                    Players[i].AddResources(gameSettings.StartingResources);
                    Players[i].SetBaseCard(BaseCards[i]);                

                    if (i == 0 && Players.Count > 1)
                        Players[i].AddCardToHand(gameSettings.FirstPlayerExpedition);
                    else
                        Players[i].AddCardToHand(ExpeditionCards[i]);
                }

                TriggerStartGameEvent_ClientRpc();
                StartEra();
            }
            else
            {
                Debug.LogWarning($"Must have between 1 and {gameSettings.MaxPlayers} players connected");
            }
        }
        [ClientRpc] void TriggerStartGameEvent_ClientRpc() => StartGameEvent?.Invoke();

        public void EndGame() // Move most of this to clientside?
        {
            TriggerEndGameEvent_ClientRpc();

            int maxVP = Players.Max(player => player.Resources.Count(resource => resource == Resources.vp));

            IEnumerable<Player> winningPlayers = Players.Where(player => player.Resources.Count(resource => resource == Resources.vp) == maxVP);

            if (winningPlayers.Count() == 1)
                Debug.Log($"Congrats, {winningPlayers.First().name} Won the game!");
            else
                Debug.Log($"{string.Join(" & ", winningPlayers.Select(player => player.name))} tied for First Place");
        }
        [ClientRpc] void TriggerEndGameEvent_ClientRpc() => EndGameEvent?.Invoke();

        public void StartEra()
        {
            StartEra_ClientRpc();
            CurrentEra?.StartEra();
        }
        [ClientRpc] public void StartEra_ClientRpc()
        {
            CurrentEra = Eras.Dequeue(); 
            Debug.Log($"Starting {CurrentEra.name}");

            Era.EraStartEvent?.Invoke();

        }
        [ClientRpc] public void TriggerEndEraEvent_ClientRpc() => Era.EraEndEvent?.Invoke();

        public void StartPhase()
        {
            StartPhase_ClientRpc();
            CurrentPhase?.StartPhase();
        }
        [ClientRpc] public void StartPhase_ClientRpc()
        {
            CurrentPhase = CurrentEra.Phases[CurrentEra.Phases.IndexOf(CurrentPhase) + 1];
            Phase.PhaseStartEvent?.Invoke();
        }
        [ClientRpc] public void TriggerEndPhaseEvent_ClientRpc() => Phase.PhaseEndEvent?.Invoke();

        public void StartRound() => StartRound_ClientRpc();
        [ClientRpc] public void StartRound_ClientRpc()
        {
            Debug.Log("Starting Round");
            Round.StartRoundEvent?.Invoke();
            CurrentRound = new Round();
        }
        [ClientRpc] public void TriggerEndRoundEvent_ClientRpc() => Round.EndRoundEvent?.Invoke();

        public void StartTurn(Player player) => StartTurn_ClientRpc(player.OwnerClientId); 
        [ClientRpc] public void StartTurn_ClientRpc(ulong playerID)
        {
            Player player = Player.GetById(playerID);

            CurrentTurn = new Turn(player);
            CurrentEra.Turns.Add(CurrentTurn);
            Debug.Log($"Starting {player.name} turn");
            Turn.StartTurnEvent?.Invoke(player);
        }
        [ClientRpc] public void TriggerEndTurnEvent_ClientRpc(ulong playerID) => Turn.EndTurnEvent?.Invoke(Player.GetById(playerID));

        public void EndTurnButton() => EndTurnButton_ServerRpc();
        [ServerRpc(RequireOwnership = false)] void EndTurnButton_ServerRpc() => CurrentTurn.NextTurn();
        #endregion

        public void AddCardsToDeck(IEnumerable<PlayCard> cards)
        {
            cards.ForEach(card => Deck.Push(card));
        }
        public void ShuffleDeck() => Deck = new(Deck.OrderBy(x => Random.value));

        public void AddRepCards(IEnumerable<ReputationCard> cards) => AddRepCards_ServerRpc(cards.Select(card => card.ID).ToArray()); 
        [ServerRpc (RequireOwnership = false)] public void AddRepCards_ServerRpc(int[] cardIDs)
        {
            cardIDs.ForEach(id => {
                ReputationCard card = Card.GetById<ReputationCard>(id); 
                ReputationCards.Add(card);
                CurrentGame.TriggerAddRepCardEvent_ClientRpc(card.ID);
            });
        }
        [ClientRpc] void TriggerAddRepCardEvent_ClientRpc(int id) => AddRepCardEvent?.Invoke(Card.GetById<ReputationCard>(id));

        [ServerRpc] public void PassCardsRight_ServerRpc()
        {
            if (Players.Count > 1)
            {
                List<Card> firstPlayerCards = new(Players.First().Hand);
                List<Card> lastPlayerCards = new(Players.Last().Hand);

                for (int i = 0; i < Players.Count - 1; i++)
                {
                    Player player = Players[i];
                    player.RemoveCardsFromHand(player.Hand);
                    player.AddCardsToHand(Players[i + 1].Hand);
                }

                Players.Last().RemoveCardsFromHand(lastPlayerCards);
                Players.Last().AddCardsToHand(firstPlayerCards);
            }
        }

        [ServerRpc] public void ShiftTurnOrder_ServerRpc()
        {
            Player firstPlayer = Game.CurrentGame.Players.First();
            Game.CurrentGame.Players.Remove(firstPlayer);
            Game.CurrentGame.Players.Add(firstPlayer);
        }

        [ServerRpc] public void SetRover_ServerRpc(int cardID, ulong playerID) => SetRover_ClientRpc(cardID, playerID);

        [ClientRpc] public void SetRover_ClientRpc(int cardID, ulong playerID) => Card.GetById<IConstructionCard>(cardID).SetRover(Player.GetById(playerID)); 
    }

    [System.Serializable] public class Resources
    {
        public List<Resource> all => new() { energy, metals, organics, water, wildcard, vp, rover }; 
        [field: SerializeField] public Resource energy { get; private set; }
        [field: SerializeField] public Resource metals { get; private set; }
        [field: SerializeField] public Resource organics { get; private set; }
        [field: SerializeField] public Resource water { get; private set; }
        [field: SerializeField] public Resource wildcard { get; private set; }
        [field: SerializeField] public Resource vp { get; private set; }
        [field: SerializeField] public Resource rover { get; private set; }
    }

    [System.Serializable] public class Flags
    {
        public List<Flag> all => new() { food, housing, production, science, transportation }; 
        [field: SerializeField] public Flag food { get; private set; }
        [field: SerializeField] public Flag housing { get; private set; }
        [field: SerializeField] public Flag production { get; private set; }
        [field: SerializeField] public Flag science { get; private set; }
        [field: SerializeField] public Flag transportation { get; private set; }
    }
}