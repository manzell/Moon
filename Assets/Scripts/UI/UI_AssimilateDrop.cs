using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace moon
{
    public class UI_AssimilateDrop : MonoBehaviour, IDropHandler
    {
        public static System.Action<PlayCard> AssimilationEvent; 

        Game game;

        private void Start()
        {
            game = FindObjectOfType<Game>();
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.selectedObject.TryGetComponent(out UI_PlayCard UI))
                AssimilationEvent?.Invoke(UI.Card as PlayCard); 
        }
    }
}
