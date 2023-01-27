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

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.selectedObject.TryGetComponent(out UI_Rover UI))
                FindObjectOfType<Game>().DeployRover_ServerRpc(Game.Player.OwnerClientId, Card.ID);
        }
    }
}
