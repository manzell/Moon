using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace moon
{
    public class UI_CardPlayArea : MonoBehaviour, IDropHandler
    {
        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.selectedObject.TryGetComponent(out UI_PlayCard UI))
                FindObjectOfType<Game>().BuildStructure_ServerRpc(Game.Player.OwnerClientId, UI.Card.ID);
        }
    }
}
