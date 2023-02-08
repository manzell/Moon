using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace moon
{
    public class UI_ExpeditionCard : UI_Card, IPointerDownHandler
    {
        [SerializeField] TextMeshProUGUI cardText;

        public override void Style()
        {
            cardName.text = Card.name; 
            cardText.text = (Card as ExpeditionCard).expeditionText;
        }

        float lastClick = 0;

        public void OnPointerDown(PointerEventData eventData)
        {
            // Note: We check that this card is in the player's Tableau on the Server end

            if (Time.time - lastClick < 0.25f && Time.time > lastClick) // Arbitrary Double-click time width;
            {
                lastClick = Time.time + 2f; // Arbitrary Double-click lock out 
                new ExpeditionAction(Card as ExpeditionCard).Execute(Game.CurrentGame.Player);
            }
            else
                lastClick = Time.time;
        }
    }
}
