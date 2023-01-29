using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

namespace moon
{
    public class Turn
    {
        public static System.Action<Turn> StartTurnEvent, EndTurnEvent;

        public Player Player { get; private set; }
        public List<TurnAction> Actions { get; private set; } = new(); 

        public bool CanEndTurn => Actions.OfType<PlayCardAction>().Count() > 0; 
        public Turn(Player player) => Player = player;

        public void StartTurn()
        {
            Debug.Log($"Starting {Player.name} turn");
            Game.CurrentTurn = this;
            Game.CurrentEra.Turns.Add(this); 
            StartTurnEvent?.Invoke(this);
        }

        public void EndTurn()
        {
            EndTurnEvent?.Invoke(this);
            NextTurn(this);
        }

        void NextTurn(Turn previousTurn)
        {
            Player nextPlayer = Game.Players.Where(player => Game.Players.IndexOf(player) > Game.Players.IndexOf(Player) && 
                player.Hand.OfType<PlayCard>().Count() > 0).FirstOrDefault();

            if (nextPlayer != null)
            {
                Debug.Log($"End Turn [#{Game.CurrentRound.Turns.Count()} - {Game.CurrentTurn.Player.name}");
                Game.CurrentTurn = new Turn(nextPlayer);
                Game.CurrentTurn.StartTurn(); // This does not trigger StartTurn() on the client I think this is a server-only thread. 
            }
            else
            {
                Game.CurrentRound.EndRound();
            }
        }
    }
}
