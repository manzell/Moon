using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace moon
{
    public class Round
    {
        public static System.Action StartRoundEvent, EndRoundEvent;
        public List<Turn> Turns { get; private set; } = new();

        public Round()
        {
            Game.CurrentGame.StartTurn(Game.CurrentGame.Players.First()); 
        }

        public void NextRound()
        {
            Game.CurrentGame.TriggerEndRoundEvent_ClientRpc(); 

            if (Game.CurrentGame.Players.Any(player => player.Hand.OfType<PlayCard>().Any()))
            {
                if (Game.CurrentGame.Players.Count() > 1)
                    Game.CurrentGame.PassCardsRight_ServerRpc();
                if (Game.CurrentGame.Players.Count() > 2)
                    Game.CurrentGame.ShiftTurnOrder_ServerRpc();

                new Round();
            } 
            else
                Game.CurrentGame.CurrentPhase.NextPhase();
        }
    }
}