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

        public Turn(Player player) => Player = player;

        public void NextTurn()
        {
            Game.CurrentGame.TriggerEndTurnEvent_ClientRpc(Player.OwnerClientId);

            Player nextPlayer = Game.CurrentGame.Players.Where(player => Game.CurrentGame.Players.IndexOf(player) > Game.CurrentGame.Players.IndexOf(Player) && 
                player.Hand.OfType<PlayCard>().Count() > 0).OrderBy(player => Game.CurrentGame.Players.IndexOf(player)).FirstOrDefault();

            if (nextPlayer != null)
                Game.CurrentGame.StartTurn(nextPlayer); 
            else
                Game.CurrentGame.CurrentRound.NextRound();
        }
    }
}
