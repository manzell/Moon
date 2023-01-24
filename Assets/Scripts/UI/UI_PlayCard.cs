using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; 

namespace moon
{
    public class UI_PlayCard : UI_Card, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] protected TextMeshProUGUI subtitle, quote;
        [SerializeField] Transform resourceCostArea, flagCostArea, assimilationArea;
        [SerializeField] RawImage cardTypeBackground;
        RectTransform rect;
        Transform previousParent; 

        public override void Style()
        {
            if(Card is IPlayCard playCard)
            {
                subtitle.text = playCard.Subtitle;
                quote.text = playCard.Quote;

                cardTypeBackground.color = Game.Graphics.CardTypeColors[playCard.Type];

                foreach (Transform t in resourceCostArea) Destroy(t.gameObject);
                foreach (Transform t in flagCostArea) Destroy(t.gameObject);
                foreach (Transform t in assimilationArea) Destroy(t.gameObject);

                foreach (Resource resource in playCard.ResourceRequirements)
                {
                    GameObject x = Instantiate(resource.Prefab, resourceCostArea);
                    x.name = $"{Card.name} {resource.name}"; 
                    x.GetComponent<UI_Icon>().Style(resource);
                }

                foreach (Flag flag in playCard.FlagRequirements)
                {
                    GameObject x = Instantiate(flag.Prefab, flagCostArea);
                    x.name = $"{Card.name} {flag.name}"; 
                    x.GetComponent<UI_Icon>().Style(flag);
                }

                foreach (Resource resource in playCard.AssimilationValue)
                { 
                    GameObject x = Instantiate(resource.Prefab, assimilationArea);
                    x.name = $"{Card.name} {resource.name}";
                    x.GetComponent<UI_Icon>().Style(resource);
                }
            }

            base.Style();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if(Game.CurrentTurn.Player.IsOwner)
            {
                rect = GetComponent<RectTransform>();
                GetComponent<CanvasGroup>().alpha = 0.7f;
                GetComponent<CanvasGroup>().blocksRaycasts = false;

                previousParent = transform.parent;
                eventData.selectedObject = this.gameObject;

                transform.SetParent(previousParent.parent);
                FindObjectsOfType<UI_CardPlayArea>().ForEach(cpa => cpa.GetComponentsInChildren<Graphic>().ForEach(graphic => graphic.raycastTarget = true));
            }
            else
            {
                eventData.pointerDrag = null; 
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (Game.CurrentTurn.Player.IsOwner)
                rect.anchoredPosition += eventData.delta;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (Game.CurrentTurn.Player.IsOwner)
            {
                if(transform.parent == previousParent.parent)
                    transform.SetParent(previousParent, false); 

                GetComponent<CanvasGroup>().alpha = 1f;
                GetComponent<CanvasGroup>().blocksRaycasts = true;

                FindObjectsOfType<UI_CardPlayArea>().ForEach(cpa => cpa.GetComponentsInChildren<Graphic>().ForEach(graphic => graphic.raycastTarget = false));
            }
        }
    }
}