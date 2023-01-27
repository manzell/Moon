using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

namespace moon
{
    public class UI_Card : MonoBehaviour
    {
        protected Game game; 
        [field: SerializeField] public ICard Card { get; private set; }
        [SerializeField] protected TextMeshProUGUI cardName;
        [SerializeField] protected Image cardImage;
        [SerializeField] protected RawImage backgroundImage;

        public void Awake()
        {
            game = FindObjectOfType<Game>();

            if (Card != null)
                Setup(Card); 
        }

        public virtual void Setup(ICard card)
        {
            this.Card = card;
            Style(); 
        }

        public virtual void Style()
        {
            name = Card.name; 
            cardName.text = Card.name;
            //cardImage.sprite = Card.CardImage;
        }
    }
}
