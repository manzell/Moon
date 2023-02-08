using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace moon
{
    public class UI_PlayCardDrag : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        RectTransform rect;
        Transform previousParent;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (UI_Game.IsOurTurn)
            {
                eventData.selectedObject = this.gameObject;
                rect = GetComponent<RectTransform>();

                GetComponent<CanvasGroup>().alpha = 0.7f;
                GetComponent<CanvasGroup>().blocksRaycasts = false;
                FindObjectsOfType<UI_CardPlayArea>().ForEach(cpa => cpa.GetComponentsInChildren<Graphic>().ForEach(graphic => graphic.raycastTarget = true));

                previousParent = transform.parent;
                transform.SetParent(previousParent.parent); // this pops us up one level in the hierarchy
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.selectedObject == this.gameObject)
                rect.anchoredPosition += eventData.delta;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Debug.Log("OnEndDrag");
            GetComponent<CanvasGroup>().alpha = 1f;
            GetComponent<CanvasGroup>().blocksRaycasts = true;
            FindObjectsOfType<UI_CardPlayArea>().ForEach(cpa => cpa.GetComponentsInChildren<Graphic>().ForEach(graphic => graphic.raycastTarget = false));

            // If we drop the card on a valid location, it'll have it's parent transform changed. If so, let's make sure to delete, and if it hasn't we want to pull it back in
            // to our hand. 
            if (eventData.selectedObject.transform.parent == previousParent.parent)
                transform.SetParent(previousParent);
            else
                Destroy(eventData.selectedObject); 
        }
    }
}
