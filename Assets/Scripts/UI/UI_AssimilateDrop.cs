using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace moon
{
    public class UI_AssimilateDrop : MonoBehaviour, IDropHandler
    {
        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.selectedObject.TryGetComponent(out UI_PlayCard UI))
            {
                Game game = FindObjectOfType<Game>();
                game.Assimilate_ServerRpc(Game.Player.OwnerClientId, UI.Card.ID);
            }
        }
    }
}
