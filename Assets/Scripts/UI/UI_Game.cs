using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.Utilities;
using TMPro;
using Unity.Netcode;

namespace moon
{
    public class UI_Game : MonoBehaviour
    {
        [SerializeField] Player player;
        [SerializeField] Transform productionCards, flagCards, actionCards, victoryCards, opponentCards, hand; 
        [SerializeField] UI_ConstructedCard constructedPrefab;
        [SerializeField] TextMeshProUGUI messageBox;

        private void Start()
        {
            Turn.StartTurnEvent += OnTurnStart; 
        }

        public void SetPlayer(Player player)
        {
            this.player = player;
            Player.AddCardToTableauEvent += AddCardToTableau;
            player.AddCardToHandEvent += OnGainCard;

            foreach (Transform t in productionCards)
                Destroy(t.gameObject);
            foreach (Transform t in flagCards)
                Destroy(t.gameObject);
            foreach (Transform t in actionCards)
                Destroy(t.gameObject);
            foreach (Transform t in victoryCards)
                Destroy(t.gameObject);
            foreach (Transform t in opponentCards)
                Destroy(t.gameObject);
            foreach (Transform t in hand)
                Destroy(t.gameObject);

            FindObjectOfType<UI_Rover>().SetPlayer(player);
            FindObjectsOfType<UI_ResourceTrack>().ForEach(track => track.SetPlayer(player));
        }

        void OnTurnStart(Turn turn)
        {
            if (turn.Player.IsOwner)
                SetMessage("It's your turn!"); 
            else
                SetMessage($"It's {turn.Player.name}'s Turn!");
        }

        public void OnGainCard(Card card)
        {
            UI_Card ui = Instantiate(card.Prefab, hand);
            ui.Setup(card); 
        }

        public void AddCardToTableau(Player player, PlayCard card)
        {
            if(player.IsOwner)
            {
                if (card is ResourceCard productionCard)
                    Instantiate(card.Prefab, productionCards);
                else if (card is FlagCard flagCard)
                    Instantiate(card.Prefab, flagCards);
                else if (card is ActionCard actionCard)
                    Instantiate(card.Prefab, actionCards);
                else if (card is VictoryCard victoryCard)
                    Instantiate(card.Prefab, victoryCards);
            }
            else
            {
                UI_ConstructedCard construct = Instantiate(constructedPrefab, opponentCards);
                construct.Setup(card); 
            }
        }

        public void SetMessage(string message)
        {
            messageBox.text = message; 
        }
    }
}
