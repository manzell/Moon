using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace moon
{
    public interface ICard
    {
        public int ID { get; }
        public string name { get; }
        public Era Era { get; }
        public UI_Card Prefab { get; }
        public Sprite CardImage { get; }
        public Card.CardType Type { get; }
    }
}
