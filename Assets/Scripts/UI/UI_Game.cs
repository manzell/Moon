using UnityEngine;
using System.Linq;
using Sirenix.Utilities;
using TMPro;
using UnityEngine.UI;

namespace moon
{
    public class UI_Game : MonoBehaviour
    {
        [SerializeField] Player player;
        [SerializeField] Transform productionCards, flagCards, actionCards, victoryCards, opponentCards, hand, reputationCards, baseFlag, baseResource; 
        [SerializeField] UI_ConstructedCard constructedPrefab;
        [SerializeField] TextMeshProUGUI messageBox, baseName; 
        [SerializeField] GameObject endTurnButton, splash, baseArea;
        [SerializeField] Transform startGameButton, startHostButton, startClientButton;

        private void Start()
        {
            Game.AddPlayerEvent += SetPlayer; 
        }

        public void SetPlayer(Player player)
        {
            if(player.IsOwner)
            {
                this.player = player;

                Game.StartGameEvent += OnGameStart;
                Game.AddRepCardEvent += OnAddRepCard;
                
                Turn.StartTurnEvent += OnTurnStart;
                Turn.StartTurnEvent += player => UpdateEndTurnButton();
                Player.AddCardToTableauEvent += AddCardToTableau;
                TurnAction.ActionEvent += action => UpdateEndTurnButton();

                player.AddCardToHandEvent += OnGainCard;
                player.RemoveCardFromHandEvent += OnLoseCard;
                player.setBaseCardEvent += StyleBase;

                splash.SetActive(false);
                startClientButton.gameObject.SetActive(false);
                startHostButton.gameObject.SetActive(false);
                startGameButton.gameObject.SetActive(player.IsOwnedByServer);

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
            }
        }

        void OnGameStart()
        {
            startGameButton.gameObject.SetActive(false);
            baseArea.SetActive(true);
            GetComponentsInChildren<UI_ResourceTrack>().ForEach(ui => ui.Setup());
            GetComponentInChildren<UI_Rover>().Setup();
        }

        void OnTurnStart(Player player)
        {
            endTurnButton.SetActive(Game.Player == player);

            if (Game.Player == player)
                SetMessage($"Your Turn");
            else
                SetMessage($"{player.name}'s Turn");
        }

        void UpdateEndTurnButton()
        {
            endTurnButton.GetComponent<Button>().interactable = Game.CurrentTurn.Player == Game.Player && Game.CurrentTurn.CanEndTurn;
        }

        public void OnGainCard(Card card)
        {
            UI_Card ui = Instantiate(card.Prefab, hand).GetComponent<UI_Card>(); 
            ui.Setup(card);
        }

        public void OnLoseCard(Card card)
        {
            Destroy(GetComponentsInChildren<UI_Card>().Where(ui => card == (Card)ui.Card).FirstOrDefault()?.gameObject); 
        }

        public void AddCardToTableau(Player player, PlayCard card)
        {
            if(player.IsOwner)
            {
                if (card is ResourceCard productionCard)
                    Instantiate(card.Prefab, productionCards).GetComponent<UI_Card>().Setup(card);
                else if (card is FlagCard flagCard)
                    Instantiate(card.Prefab, flagCards).GetComponent<UI_Card>().Setup(card);
                else if (card is ActionCard actionCard)
                    Instantiate(card.Prefab, actionCards).GetComponent<UI_Card>().Setup(card);
                else if (card is VictoryCard victoryCard)
                    Instantiate(card.Prefab, victoryCards).GetComponent<UI_Card>().Setup(card);
            }
            else
            {
                Instantiate(constructedPrefab, opponentCards).Setup(card); 
            }
        }

        public void SetMessage(object message)
        {
            messageBox.text = message.ToString(); 
        }

        void OnAddRepCard(ReputationCard card)
        {
            UI_Card ui = Instantiate(card.Prefab, reputationCards).GetComponent<UI_Card>();
            ui.gameObject.name = card.name; 
            ui.Setup(card); 
        }

        void StyleBase(BaseCard card)
        {
            baseName.text = card.name;

            GameObject r = Instantiate(card.ProductionResource.Prefab, baseResource);
            GameObject f = Instantiate(card.Flag.Prefab, baseFlag);
        }
    }
}
