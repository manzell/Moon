using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace moon
{
    public class Card : ScriptableObject, ICard
    {
        public enum CardType { Expedition, Reputation, Production, Flag, Action, Victory }

        [SerializeField] GameObject prefab;
        [SerializeField] CardType type;
        [SerializeField] Era era;
        [SerializeField] Sprite cardImage;
        [SerializeField] int id; 

        public GameObject Prefab => prefab; 
        public CardType Type => type;
        public Era Era => era; 
        public Sprite CardImage => cardImage;
        public int ID => id; 

        public static Card GetById(int cardID) => Game.Cards.FirstOrDefault(card => card.ID == cardID);
        public static T GetById<T>(int cardID) where T : ICard => Game.Cards.OfType<Card>().OfType<T>().FirstOrDefault(card => card.ID == cardID);
    }
}
