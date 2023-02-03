using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using Sirenix.Utilities;
using TMPro;

namespace moon
{
    public class Game : NetworkBehaviour
    {
        public static System.Action<Player> AddPlayerEvent; 
        public static System.Action StartGameEvent, EndGameEvent;
        public static System.Action<ReputationCard> ClaimRepCardEvent, AddRepCardEvent;

        public static GameSettings GameSettings => FindObjectOfType<Game>().gameSettings;
        [SerializeField] GameSettings gameSettings;

        public static Graphics Graphics; 
        [SerializeField] Graphics graphics;

        public static Resources Resources;
        [SerializeField] Resources resources;

        public static Flags Flags;
        [SerializeField] Flags flags;

        public static UI_SelectionWindow SelectionWindow => FindObjectOfType<Game>().selectionWindowPrefab; 
        [SerializeField] UI_SelectionWindow selectionWindowPrefab; 

        public static Player Player;
        public static List<Player> Players = new();

        public static Queue<Era> Eras { get; private set; } 
        public static HashSet<Card> Cards { get; private set; } = new(); 
        public static Stack<PlayCard> Deck { get; private set; } = new(); 
        public static List<ReputationCard> ReputationCards { get; private set; } = new();
        public static List<ExpeditionCard> ExpeditionCards { get; private set; } = new();
        public static List<BaseCard> BaseCards { get; private set; } = new();
        public static List<PlayCard> Discards { get; private set; } = new(); 
        public static Dictionary<Flag, List<Resource>> Rewards { get; private set; } = new(); 

        public static Era CurrentEra;
        public static Phase CurrentPhase;
        public static Round CurrentRound;
        public static Turn CurrentTurn;

        void Awake()
        {
            Graphics = graphics;
            Flags = flags;
            Resources = resources;
            Eras = new(GameSettings.Eras);
            Cards.Add(gameSettings.FirstPlayerExpedition);
            Cards.UnionWith(gameSettings.ExpeditionCards);
            Cards.UnionWith(gameSettings.Bases);
            Cards.UnionWith(gameSettings.Eras.SelectMany(era => era.Deck)); 
        }

        public static void AddPlayer(Player player)
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
                    Players[i].AddResources(gameSettings.StartingResources);
                    Players[i].SetBaseCard(BaseCards[i]);                

                    if (i == 0 && Players.Count > 1)
                        Players[i].AddCardsToHand(new List<ExpeditionCard>() { gameSettings.FirstPlayerExpedition });
                    else
                        Players[i].AddCardsToHand(new List<ExpeditionCard>() { ExpeditionCards[i] });
                }

                TriggerStartGameEvent_ClientRpc();

                CurrentEra = Eras.Dequeue();
                CurrentEra.StartEra();
            }
            else
            {
                Debug.LogWarning($"Must have between 1 and {gameSettings.MaxPlayers} players connected");
            }
        }
        [ClientRpc] void TriggerStartGameEvent_ClientRpc()
        {
            FindObjectOfType<UI_Game>().SetMessage($"Trigger Start Game - {Player.name}"); 
            StartGameEvent?.Invoke();
        }

        public void EndTurnButton() => EndTurnButton_ServerRpc();
        [ServerRpc] void EndTurnButton_ServerRpc() => CurrentTurn.NextTurn();

        public static void EndGame() // Move most of this to clientside
        {
            FindObjectOfType<Game>().TriggerEndGameEvent_ClientRpc();

            int maxVP = Players.Max(player => player.Resources.Count(resource => resource == Resources.vp));

            IEnumerable<Player> winningPlayers = Players.Where(player => player.Resources.Count(resource => resource == Resources.vp) == maxVP);

            if (winningPlayers.Count() == 1)
                Debug.Log($"Congrats, {winningPlayers.First().name} Won the game!");
            else
                Debug.Log($"{string.Join(" & ", winningPlayers.Select(player => player.name))} tied for First Place");
        }
        [ClientRpc] void TriggerEndGameEvent_ClientRpc() => EndGameEvent?.Invoke();
        #endregion

        #region Basic Player Actions

        [ServerRpc(RequireOwnership = false)]
        public void BuildStructure_ServerRpc(ulong playerID, int cardID)
        {
            Player player = Players.FirstOrDefault(player => playerID == player.OwnerClientId);
            PlayCard card = Card.GetById<PlayCard>(cardID);
            
            TurnAction action = new BuildAction();
            action.SetCard(card);
            action.Execute(player);
        }

        [ServerRpc(RequireOwnership = false)]
        public void Assimilate_ServerRpc(ulong playerID, int cardID)
        {
            Player player = Players.FirstOrDefault(player => playerID == player.OwnerClientId);
            PlayCard card = Card.GetById<PlayCard>(cardID); 
            
            TurnAction action = new AssimilateAction();
            action.SetCard(card);
            action.Execute(player);
        }

        [ServerRpc(RequireOwnership = false)]
        public void Expedition_ServerRpc(ulong playerID, int cardID)
        {
            Player player = Players.FirstOrDefault(player => playerID == player.OwnerClientId);
            ExpeditionCard card = Card.GetById<ExpeditionCard>(cardID); 
           
            TurnAction action = new ExpeditionAction();
            action.SetCard(card);
            action.Execute(player);
        }

        [ServerRpc(RequireOwnership = false)]
        public void DeployRover_ServerRpc(ulong playerID, int destinationcardID)
        {
            Player player = Players.FirstOrDefault(player => playerID == player.OwnerClientId);
            IConstructionCard card = Card.GetById(destinationcardID) as IConstructionCard; 

            TurnAction action = new RoverAction();
            action.SetCard(card);
            action.Execute(player);
        }

        [ServerRpc(RequireOwnership = false)]
        public void UseCardAction_ServerRpc(ulong playerID, int cardID)
        {
            Player player = Player.GetById(playerID); 
            ActionCard card = Card.GetById<ActionCard>(cardID);            
            TurnAction action = new FlipCardAction();

            action.SetCard(card);
            action.Execute(player);
        }

        [ServerRpc(RequireOwnership = false)]
        public void ClaimReputation_ServerRpc(ulong playerID, int cardID)
        {
            Player player = Players.FirstOrDefault(player => playerID == player.OwnerClientId);
            ReputationCard card = Card.GetById<ReputationCard>(cardID);

            TurnAction action = new ClaimReputationAction();
            action.SetCard(card);
            action.Execute(player);
        }
        #endregion

        public static void ShuffleDeck() => Deck = new(Deck.OrderBy(x => Random.value));

        public static void AddCardsToDeck(IEnumerable<PlayCard> cards)
        {
            cards.ForEach(card => Deck.Push(card)); 
        }

        public static void AddReputationCards(IEnumerable<ReputationCard> cards)
        {
            cards.ForEach(card => {
                ReputationCards.Add(card);
                FindObjectOfType<Game>().TriggerAddRepCardEvent_ClientRpc(card.ID);
            });
        }
        [ClientRpc] void TriggerAddRepCardEvent_ClientRpc(int id) => AddRepCardEvent?.Invoke(Card.GetById<ReputationCard>(id));

        [ClientRpc] public void TriggerStartEraEvent_ClientRpc()
        {
            Debug.Log($"Starting {CurrentEra.name}");
            Era.EraStartEvent?.Invoke();
        }
        [ClientRpc] public void TriggerEndEraEvent_ClientRpc() => Era.EraEndEvent?.Invoke();

        [ClientRpc] public void TriggerStartPhaseEvent_ClientRpc() => Phase.PhaseStartEvent?.Invoke();
        [ClientRpc] public void TriggerEndPhaseEvent_ClientRpc() => Phase.PhaseEndEvent?.Invoke();

        [ClientRpc] public void TriggerStartRoundEvent_ClientRpc()
        {
            Debug.Log("Starting Round");
            Round.StartRoundEvent?.Invoke();
        }
        [ClientRpc] public void TriggerEndRoundEvent_ClientRpc() => Round.EndRoundEvent?.Invoke();

        [ClientRpc] public void TriggerStartTurnEvent_ClientRpc(ulong playerID)
        {
            Player player = Players.Where(player => player.OwnerClientId == playerID).FirstOrDefault();

            FindObjectOfType<UI_Game>().SetMessage($"StartTurn_ClientRpc({playerID}: {player.name})");

            Debug.Log($"Starting {player.name} turn");
            Turn.StartTurnEvent?.Invoke(player);
        }
        [ClientRpc] public void TriggerEndTurnEvent_ClientRpc(ulong playerID) => Turn.EndTurnEvent?.Invoke(Players.Where(player => player.OwnerClientId == playerID).FirstOrDefault());
    }

    [System.Serializable]
    public class Resources
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

    [System.Serializable]
    public class Flags
    {
        public List<Flag> all => new() { food, housing, production, science, transportation }; 
        [field: SerializeField] public Flag food { get; private set; }
        [field: SerializeField] public Flag housing { get; private set; }
        [field: SerializeField] public Flag production { get; private set; }
        [field: SerializeField] public Flag science { get; private set; }
        [field: SerializeField] public Flag transportation { get; private set; }
    }
}