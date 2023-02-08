using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace moon
{
    public class UI_CardPlayArea : MonoBehaviour, IDropHandler
    {
        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.selectedObject.TryGetComponent(out UI_PlayCard UI))
            {
                TurnAction action = new BuildAction(UI.Card as PlayCard);

                if(action.Can(Game.CurrentGame.Player))
                {
                    eventData.selectedObject.transform.SetParent(this.transform);
                    action.Execute(Game.CurrentGame.Player);
                    Game.CurrentGame.Player.enableTurnEndEvent?.Invoke(); 
                }
            }
        }
    }
}
