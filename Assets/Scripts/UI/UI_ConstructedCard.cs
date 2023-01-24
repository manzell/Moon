using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

namespace moon
{
    public class UI_ConstructedCard : UI_Card
    { 
        [SerializeField] GameObject roverBay, icons;

        private void Start()
        {
            if(Card is IConstructionCard card)
                GetComponentInChildren<UI_RoverBay>().SetCard(card); 
        }

        public override void Style()
        {
            base.Style();
            backgroundImage.color = Game.Graphics.CardTypeColors[Card.Type];
        }
    }
}
