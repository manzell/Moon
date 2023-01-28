using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace moon
{
    public interface ICard : ISelectable
    {
        public int ID { get; }
        public string name { get; }
        public Era Era { get; }
        public Sprite CardImage { get; }
        public Card.CardType Type { get; }
    }
}
