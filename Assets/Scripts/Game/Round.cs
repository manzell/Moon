using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace moon
{
    public class Round
    {
        public static System.Action StartRoundEvent, EndRoundEvent;
        public List<Turn> Turns { get; private set; } = new();

        public Round()
        {
            Game.CurrentRound = this;
            GameObject.FindObjectOfType<Game>().TriggerStartRoundEvent_ClientRpc();

            Turns.Add(new Turn(Game.Players.First()));
        }

        public void NextRound()
        {
            GameObject.FindObjectOfType<Game>().TriggerEndRoundEvent_ClientRpc(); 

            if (Game.Players.Any(player => player.Hand.OfType<PlayCard>().Any()))
            {
                if (Game.Players.Count() > 1)
                    PassCardsRight();
                if (Game.Players.Count() > 2)
                    ShiftTurnOrder();

                new Round();
            } 
            else
                Game.CurrentPhase.NextPhase();
        }

        public void PassCardsRight() 
        {
            if(Game.Players.Count > 1)
            {
                List<Card> firstPlayerCards = new(Game.Players[0].Hand);

                for (int i = 0; i < Game.Players.Count - 1; i++)
                {
                    Game.Players[i].RemoveCardsFromHand(Game.Players[i].Hand);
                    Game.Players[i].AddCardsToHand(Game.Players[i + 1].Hand);
                }

                Game.Players.Last().RemoveCardsFromHand(Game.Players.Last().Hand); 
                Game.Players.Last().AddCardsToHand(firstPlayerCards);
            }
        }

        public void ShiftTurnOrder() // Server Side
        {
                Player firstPlayer = Game.Players.First();
                Game.Players.Remove(firstPlayer);
                Game.Players.Add(firstPlayer);
        }
    }
}