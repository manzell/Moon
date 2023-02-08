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
                PlayCard card = UI.Card as PlayCard; 

                if (Game.CurrentGame.Player.Hand.Contains(card))
                {
                    TurnAction action = new AssimilateAction(card);
                    action.Execute(Game.CurrentGame.Player);

                    Game.CurrentGame.Player.enableTurnEndEvent?.Invoke();
                }
            }
        }
    }
}
