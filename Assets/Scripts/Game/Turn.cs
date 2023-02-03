using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace moon
{
    public class Turn
    {
        public static System.Action<Player> StartTurnEvent, EndTurnEvent;

        public Player Player { get; private set; }
        public List<TurnAction> Actions { get; private set; } = new(); 

        public bool CanEndTurn => Actions.OfType<PlayCardAction>().Count() > 0; 

        public Turn(Player player)
        {
            Player = player;
            Game.CurrentTurn = this;
            Game.CurrentEra.Turns.Add(this);
            GameObject.FindObjectOfType<Game>().TriggerStartTurnEvent_ClientRpc(player.OwnerClientId); 
        }

        public void NextTurn()
        {
            GameObject.FindObjectOfType<Game>().TriggerEndTurnEvent_ClientRpc(Player.OwnerClientId);
            Player nextPlayer = Game.Players.Where(player => Game.Players.IndexOf(player) > Game.Players.IndexOf(Player) && 
                player.Hand.OfType<PlayCard>().Count() > 0).FirstOrDefault();

            if (nextPlayer != null)
                new Turn(nextPlayer);
            else
                Game.CurrentRound.NextRound();
        }
    }
}
