using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector; 

namespace moon
{
    [CreateAssetMenu]
    public class Graphics : SerializedScriptableObject
    {
        [SerializeField] GameObject expeditionCardPrefab, baseCardPrefab, resourceCardPrefab, flagCardPrefab, actionCardPrefab, victoryCardPrefab;
        [SerializeField] UI_SelectionWindow selectionPrefab; 
        [SerializeField] Dictionary<Card.CardType, Color> cardTypeColors; 

        public GameObject ExpeditionCardPrefab => expeditionCardPrefab;
        public GameObject BaseCardPrefab => baseCardPrefab;
        public GameObject ResourceCardPrefab => resourceCardPrefab;
        public GameObject FlagCardPrefab => flagCardPrefab;
        public GameObject ActionCardPrefab => actionCardPrefab;
        public GameObject VictoryCardPrefab => victoryCardPrefab;
        public UI_SelectionWindow SelectionPrefab => selectionPrefab; 
        public Dictionary<Card.CardType, Color> CardTypeColors => cardTypeColors;


    }
}
