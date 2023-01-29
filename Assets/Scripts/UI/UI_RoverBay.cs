using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; 

namespace moon
{
    // This Controls the Availability Icon of the Rover Space on Constructed Cards
    public class UI_RoverBay : MonoBehaviour, IDropHandler
    {
        [SerializeField] Image RoverIcon;
        [SerializeField] Sprite AvailableIcon, OccupiedIcon;

        IConstructionCard card;

        public void SetCard(IConstructionCard card)
        {
            this.card = card;

            card.AddRoverEvent += player => Style();
            card.RemoveRoverEvent += Style; 

            Style(); 
        }

        public void Style()
        {
            RoverIcon.sprite = card.Rover == null ? AvailableIcon : OccupiedIcon;
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.selectedObject.TryGetComponent(out UI_Rover UI))
                FindObjectOfType<Game>().DeployRover_ServerRpc(Game.Player.OwnerClientId, card.ID); 
        }
    }
}
