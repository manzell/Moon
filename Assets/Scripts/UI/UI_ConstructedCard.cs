using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace moon
{
    public class UI_ConstructedCard : UI_Card, IDropHandler
    {
        [SerializeField] GameObject roverBay;
        [SerializeField] Transform icons;

        private void Start()
        {
            if(Card is IConstructionCard card)
                GetComponent<UI_RoverBay>()?.SetCard(card);
        }

        public override void Style()
        {
            base.Style();
            backgroundImage.color = Game.Graphics.CardTypeColors[Card.Type];

            foreach(Resource resource in (Card as IConstructionCard).Resources)
                Instantiate(resource.Prefab, icons).GetComponent<UI_Icon>().Style(resource); 
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.selectedObject.TryGetComponent(out UI_Rover UI))
                new RoverAction(Card).Execute(Game.CurrentGame.Player);
        }
    }
}
