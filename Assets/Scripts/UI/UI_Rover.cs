using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Linq;
using Mono.Cecil;

namespace moon
{
    // This is the UI for the player's Rover Store. 
    public class UI_Rover : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] Image roverImage;
        [SerializeField] TextMeshProUGUI count;

        RectTransform rect;
        GameObject rover;

        public void Setup()
        {
            Game.CurrentGame.Player.AddResourcesEvent += r => { if (r.Any(resource => resource == Game.Resources.rover)) Style(); };
            Game.CurrentGame.Player.LoseResourcesEvent += r => { if (r.Any(resource => resource == Game.Resources.rover)) Style(); };
            Style();
        }

        void Style() => count.text = Game.CurrentGame.Player.Resources.Count(resource => resource == Game.Resources.rover).ToString(); 

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (Game.CurrentGame.Player.Resources.Count(resource => resource == Game.Resources.rover) >= 1)
            {
                rover = Instantiate(roverImage.gameObject, transform);
                rect = rover.GetComponent<RectTransform>();

                eventData.selectedObject = this.gameObject;

                rover.GetComponent<CanvasGroup>().alpha = 0.9f;
                rover.GetComponent<CanvasGroup>().blocksRaycasts = false;
            }
            else
            {
                eventData.pointerDrag = null;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (rect != null)
                rect.anchoredPosition += eventData.delta;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Destroy(rover);
            rect = null; 
        }
    }
}
