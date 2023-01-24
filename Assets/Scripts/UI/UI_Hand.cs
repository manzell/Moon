using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace moon
{
    public class UI_Hand : MonoBehaviour, IDropHandler
    {
        public void OnDrop(PointerEventData eventData)
        {
            char[] nombre = new char[4]; 

            if (eventData.selectedObject.TryGetComponent(out UI_ReputationCard UI))
                FindObjectOfType<Game>().ClaimReputation_ServerRpc(Game.Player.NetworkObjectId, UI.Card.GetHashCode()); 
        }
    }
}