using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.EventSystems;

namespace moon
{
    public class UI_ReputationCard : UI_Card, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] TextMeshProUGUI cardText, victoryPoints;
        [SerializeField] Color gold, silver, bronze;
        RectTransform rect;
        Transform previousParent;

        private void Start()
        {
            Game.ClaimRepCardEvent += repCard => Style(); 
        }

        public override void Style()
        {
            if(Card is ReputationCard repCard)
            {
                cardName.text = repCard.name; 
                cardText.text = repCard.conditionText;
                victoryPoints.text = repCard.VP.ToString();

                if (repCard.Owner)
                    backgroundImage.color = Color.gray; 
                else
                {
                    if(repCard.GoalLevel == ReputationCard.Level.Bronze)
                        backgroundImage.color = bronze;
                    else if (repCard.GoalLevel == ReputationCard.Level.Silver)
                        backgroundImage.color = silver;
                    else if (repCard.GoalLevel == ReputationCard.Level.Gold)
                        backgroundImage.color = gold;
                }

                if (repCard.CanClaim.Value(Game.Player))
                    GetComponent<CanvasGroup>().alpha = 0.5f;
                else
                    GetComponent<CanvasGroup>().alpha = 1f; 
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (Game.CurrentTurn.Player.IsOwner && (Card as ReputationCard).Owner == null &&
                (Card as ReputationCard).CanClaim.Value(Game.Player))
            {
                rect = GetComponent<RectTransform>();
                GetComponent<CanvasGroup>().alpha = 0.7f;
                GetComponent<CanvasGroup>().blocksRaycasts = false;

                previousParent = transform.parent;
                eventData.selectedObject = this.gameObject;

                transform.SetParent(previousParent.parent);
            }
            else
            {
                eventData.pointerDrag = null;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (Game.CurrentTurn.Player.IsOwner && (Card as ReputationCard).Owner == null &&
                (Card as ReputationCard).CanClaim.Value(Game.Player))
            {

                //Debug.Log($"22 {Game.CurrentTurn.Player.IsOwner} && {(Card as ReputationCard).Owner == null} && {(Card as ReputationCard).ClaimAction.Can(Game.Player)}");
                rect.anchoredPosition += eventData.delta;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            //Debug.Log("UI_PlayCard.OnEndDrag() - This is moving the card back into our hand area");
            eventData.selectedObject.transform.SetParent(previousParent);
            GetComponent<CanvasGroup>().blocksRaycasts = true;

            Style(); 
        }
    }
}
