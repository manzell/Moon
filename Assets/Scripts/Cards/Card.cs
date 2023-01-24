using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace moon
{
    public class Card : ScriptableObject, ICard
    {
        public enum CardType { Expedition, Production, Flag, Action, Victory }

        [SerializeField] UI_Card prefab;
        [SerializeField] CardType type;
        [SerializeField] Era era;
        [SerializeField] Sprite cardImage;
        [SerializeField] int id; 

        public UI_Card Prefab => prefab; 
        public CardType Type => type;
        public Era Era => era; 
        public Sprite CardImage => cardImage;
        public int ID => id; 
    }
}
