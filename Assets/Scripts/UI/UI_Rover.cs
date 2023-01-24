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

        public Player player {  get; private set; }
        RectTransform rect;
        GameObject rover;
        int rovers; 

        public void SetPlayer(Player player)
        {
            this.player = player; 
            player.AddResourceEvent += r => { if (r == Game.Resources.rover) Style(); };
            player.LoseResourceEvent += r => { if (r == Game.Resources.rover) Style(); };
            Game.StartGameEvent += Style;
        }

        void Style()
        {
            count.text = rovers.ToString();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (rovers >= 1)
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
