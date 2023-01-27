using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace moon
{
    public class Round
    {
        public static System.Action<Round> StartRoundEvent, EndRoundEvent;
        public List<Turn> Turns { get; private set; } = new();

        public void StartRound()
        {
            Debug.Log("Starting Round");
            Game.CurrentRound = this;
            StartRoundEvent?.Invoke(this);

            Turn turn = new Turn(Game.Players.First());
            Turns.Add(turn);
            turn.StartTurn(); 
        }

        public void NextRound(Round previousRound)
        {
            if (Game.Players.Any(player => player.Hand.OfType<PlayCard>().Any()))
                new Round().StartRound();
            else
                Game.CurrentPhase.EndPhase();
        }

        public void EndRound()
        {
            PassCards();
            if(Game.Players.Count() > 2)
                ShiftTurnOrder();
            
            EndRoundEvent?.Invoke(this);
            NextRound(this);
        }

        public void PassCards()
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

        public void ShiftTurnOrder()
        {
                Player firstPlayer = Game.Players.First();
                Game.Players.Remove(firstPlayer);
                Game.Players.Add(firstPlayer);
        }
    }
}