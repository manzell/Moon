using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using Sirenix.Utilities;

namespace moon
{
    public class UI_Game : MonoBehaviour
    {
        public static bool IsOurTurn { get; private set; }

        [SerializeField] Transform productionCards, flagCards, actionCards, victoryCards, opponentCards, hand, reputationCards, baseFlag, baseResource; 
        [SerializeField] UI_ConstructedCard constructedPrefab;
        [SerializeField] TextMeshProUGUI messageBox, baseName; 
        [SerializeField] GameObject endTurnButton, splash, baseArea;
        [SerializeField] Transform startGameButton, startHostButton, startClientButton;

        Player player;

        private void Start()
        {
            Game.AddPlayerEvent += SetPlayer;
            Screen.SetResolution(1920, 1080, false);
        }

        public void SetPlayer(Player player)
        {
            if(player.IsOwner)
            {
                this.player = player;

                Game.StartGameEvent += OnGameStart;
                Game.AddRepCardEvent += OnAddRepCard;
                
                Turn.StartTurnEvent += OnTurnStart;
                Player.AddCardToTableauEvent += AddCardToTableau;
                player.enableTurnEndEvent += () => endTurnButton.GetComponent<Button>().interactable = true;

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
            IsOurTurn = Game.CurrentGame.Player == player; 
            endTurnButton.SetActive(IsOurTurn);
            endTurnButton.GetComponent<Button>().interactable = false; 

            if (IsOurTurn)
                SetMessage($"Your Turn");
            else
                SetMessage($"{player.name}'s Turn");
        }

        public void OnGainCard(ICard card)
        {
            //Debug.Log($"Adding {card.name} to {player.name}'s Hand");
            Instantiate(card.Prefab, hand).GetComponent<UI_Card>().Setup(card);
        }

        public void OnLoseCard(ICard card)
        {
            //Debug.Log($"Removing {card.name} from {player.name}'s Hand");
            Destroy(GetComponentsInChildren<UI_Card>().Where(ui => card == ui.Card).FirstOrDefault()?.gameObject); 
        }

        public void AddCardToTableau(Player player, PlayCard card)
        {
            UI_Card ui = null; 

            if(player.IsOwner)
            {
                if (card is ResourceCard productionCard)
                    ui = Instantiate(card.Prefab, productionCards).GetComponent<UI_Card>();
                else if (card is FlagCard flagCard)
                    ui = Instantiate(card.Prefab, flagCards).GetComponent<UI_Card>();
                else if (card is ActionCard actionCard)
                    ui = Instantiate(card.Prefab, actionCards).GetComponent<UI_Card>();
                else if (card is VictoryCard victoryCard)
                    ui = Instantiate(card.Prefab, victoryCards).GetComponent<UI_Card>();

                if(ui != null)
                {
                    ui.Setup(card);
                    Destroy(ui.gameObject.GetComponent<UI_PlayCardDrag>()); 
                }
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

            Instantiate(card.ProductionResource.Prefab, baseResource).GetComponent<UI_Icon>().Style(card.ProductionResource);
            Instantiate(card.Flag.Prefab, baseFlag).GetComponent<UI_Icon>().Style(card.Flag); 
        }
    }
}
