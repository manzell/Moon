using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

namespace moon
{
    public class UI_Card : MonoBehaviour
    {
        [field: SerializeField] public ICard Card { get; private set; }
        [SerializeField] protected TextMeshProUGUI cardName;
        [SerializeField] protected Image cardImage;
        [SerializeField] protected Image backgroundImage;

        public void Awake()
        {
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
