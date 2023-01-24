using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

namespace moon
{
    public class Game : NetworkBehaviour
    {
        public static System.Action StartGameEvent, EndGameEvent;
        public static System.Action<ReputationCard> ClaimReputationCardEvent, AddReputationCardToGameEvent; 

        public static GameSettings GameSettings => FindObjectOfType<Game>().gameSettings;
        [SerializeField] GameSettings gameSettings;

        public static Graphics Graphics; 
        [SerializeField] Graphics graphics;

        public static Resources Resources;
        [SerializeField] Resources resources;

        public static Flags Flags;
        [SerializeField] Flags flags;

        public static Player Player;
        public static List<Player> Players = new();

        public static Queue<Era> Eras { get; private set; } // Player never needs to know. 
        public static HashSet<Card> Cards { get; private set; } = new(); // This is a handy dandy reference. Should be a ClientRPC
        public static Stack<PlayCard> Deck { get; private set; } = new(); // Player should not be able to access Deck. 
        public static List<ReputationCard> ReputationCards { get; private set; } = new(); // The player needs to know what rep cards are available. 
        public static List<PlayCard> Discards { get; private set; } = new(); // Player should not be able to access Discards. 
        public static Dictionary<Flag, List<Resource>> Rewards { get; private set; } = new(); // Player 

        public static Era CurrentEra;
        public static Phase CurrentPhase;
        public static Round CurrentRound;
        public static Turn CurrentTurn;

        void Awake()
        {
            Graphics = graphics;
            Flags = flags;
            Resources = resources;
        }

        public static void AddPlayer(Player player)
        {
            if (Players.Count < 5)
            {
                Players.Add(player);
                player.name = $"Player {Players.Count}";

                Debug.Log($"{player.name} Added to the Game!");

                if (player.IsOwner)
                    Player = player;
            }
        }

        #region Game-Turn-Order-Management

        public static void StartGame()
        {
            Eras = new(GameSettings.Eras);
            Cards.AddRange(Eras.SelectMany(era => era.Deck));

            if (Players.Count > 0 && Players.Count < 6)
            {
                Debug.Log($"Starting {Eras.Count}-Era Game for {Players.Count} players");
                StartGameEvent?.Invoke();

                Eras.Dequeue().StartEra();
            }
            else
            {
                Debug.LogWarning("Must have between 2 and 5 players connected");
            }
        }

        public static void EndTurn()
        {
            if (CurrentTurn.CanEndTurn)
                CurrentTurn.EndTurn();
        }

        public static void EndGame()
        {
            EndGameEvent?.Invoke();

            int maxVP = Players.Max(player => player.Resources.Count(resource => resource == Resources.vp));

            IEnumerable<Player> winningPlayers = Players.Where(player => player.Resources.Count(resource => resource == Resources.vp) == maxVP);

            if (winningPlayers.Count() == 1)
                Debug.Log($"Congrats, {winningPlayers.First().name} Won the game!");
            else
                Debug.Log($"{string.Join(" & ", winningPlayers.Select(player => player.name))} tied for First Place");
        }
        #endregion

        #region Basic Player Actions

        [ServerRpc]
        public void BuildStructure_ServerRpc(ulong playerID, int cardID)
        {
            Player player = Players.FirstOrDefault(player => playerID == player.OwnerClientId);
            PlayCard card = player.Hand.OfType<PlayCard>().FirstOrDefault(card => card.ID == cardID);

            new PlayCardAction(player, card).Execute();
        }

        [ServerRpc]
        public void Assimilate_ServerRpc(ulong playerID, int cardID)
        {
            Player player = Players.FirstOrDefault(player => playerID == player.OwnerClientId);
            PlayCard card = player.Hand.OfType<PlayCard>().FirstOrDefault(card => card.ID == cardID);
            new AssimilateAction(player, card).Execute();
        }

        [ServerRpc]
        public void Expedition_ServerRpc(ulong playerID, int cardID)
        {
            Player player = Players.FirstOrDefault(player => playerID == player.OwnerClientId);
            ExpeditionCard card = player.Hand.OfType<ExpeditionCard>().FirstOrDefault(card => card.ID == cardID);

            new ExpeditionAction(card, player).Execute(); 
        }

        [ServerRpc]
        public void DeployRover_ServerRpc(ulong playerID, int destinationcardID)
        {
            Player player = Players.FirstOrDefault(player => playerID == player.OwnerClientId);
            IConstructionCard card = player.Hand.OfType<IConstructionCard>().FirstOrDefault(card => card.ID == destinationcardID);

            new RoverAction(player, card).Execute();
        }

        [ServerRpc]
        public void UseCardAction_ServerRpc(ulong playerID, int cardID)
        {
            Player player = Players.FirstOrDefault(player => playerID == player.OwnerClientId);
            ActionCard card = player.Hand.OfType<ActionCard>().FirstOrDefault(card => card.ID == cardID);

            new FlipCardAction(card, player).Execute();
        }

        [ServerRpc]
        public void ClaimReputation_ServerRpc(ulong playerID, int cardID)
        {
            Player player = Players.FirstOrDefault(player => playerID == player.OwnerClientId);
            ReputationCard card = player.Hand.OfType<ReputationCard>().FirstOrDefault(card => card.ID == cardID);

            new ClaimReputationAction(card, player).Execute();  
        }
        #endregion

        public void AddReputationCards(IEnumerable<ReputationCard> cards)
        {
            foreach (ReputationCard card in cards)
                ModifyReputationCard_ClientRpc(card.ID, true); 
        }

        [ClientRpc] public void AddCardToDeck_ClientRpc(int cardID)
        {
            Card card = Cards.FirstOrDefault(card => card.ID == cardID);

            if(card != null)
                if (card is PlayCard playCard)
                    Deck.Push(playCard);
        }

        [ClientRpc] public void ModifyReputationCard_ClientRpc(int cardID, bool add)
        {
            ReputationCard card = Cards.OfType<ReputationCard>().FirstOrDefault(card => card.ID == cardID);

            if (add)
            {
                ReputationCards.Add(card);
                AddReputationCardToGameEvent?.Invoke(card); 
            }
            else
            {
                ReputationCards.Remove(card);
                ClaimReputationCardEvent?.Invoke(card); 
            }
        }
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