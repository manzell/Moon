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
            ShiftTurnOrder();
            
            EndRoundEvent?.Invoke(this);
            NextRound(this);
        }

        public void PassCards()
        {
            List<Card> firstPlayerCards = Game.Players.First().Hand;

            for (int i = 0; i < Game.Players.Count - 1; i++)
            {
                Card card = Game.Players[i + 1].Hand.First();

                Game.Players[i + 1].RemoveCardFromHand(card);
                Game.Players[i].AddCardToHand(card);
            }

            foreach (Card card in firstPlayerCards)
                Game.Players.Last().AddCardToHand(card);
        }

        public void ShiftTurnOrder()
        {
            Player firstPlayer = Game.Players.First();
            Game.Players.Remove(firstPlayer);
            Game.Players.Add(firstPlayer);
        }
    }
}