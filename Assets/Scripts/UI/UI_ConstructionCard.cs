using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; 

namespace moon
{
    public class UI_ConstructionCard : UI_PlayCard
    {
        [SerializeField] Transform productionArea;

        public override void Style()
        {
            IEnumerable<GameObject> icons = (Card as IConstructionCard).Icons;

            foreach (var token in icons)
            {
                RectTransform rect = Instantiate(token, productionArea).GetComponent<RectTransform>();
                Vector2 delta = productionArea.GetComponent<RectTransform>().sizeDelta; 

                float iconSize = Mathf.Min(delta.x, delta.y) - 5;

                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, iconSize);
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, iconSize);
            }

            base.Style();
        }
    }
}
