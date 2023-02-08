using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; 

namespace moon
{
    public class UI_PlayCard : UI_Card
    {
        [SerializeField] protected TextMeshProUGUI subtitle, quote;
        [SerializeField] Transform resourceCostArea, flagCostArea, assimilationArea;
        [SerializeField] RawImage cardTypeBackground;

        public override void Style()
        {
            if(Card is IPlayCard playCard)
            {
                subtitle.text = playCard.Subtitle;
                quote.text = playCard.Quote;

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
    }
}