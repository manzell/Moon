using System.Collections.Generic;
using UnityEngine;

namespace moon
{
    public class UI_ConstructionCard : UI_PlayCard
    {
        [SerializeField] Transform productionArea;

        public override void Style()
        {
            foreach (var icon in (Card as IConstructionCard).Icons)
            {
                RectTransform rect = Instantiate(icon.Prefab, productionArea).GetComponent<RectTransform>();
                Vector2 delta = productionArea.GetComponent<RectTransform>().sizeDelta;
                UI_Icon ui = rect.GetComponent<UI_Icon>(); 

                float iconSize = Mathf.Min(delta.x, delta.y) - 5;

                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, iconSize);
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, iconSize);

                ui.Style(icon);
            }

            base.Style();
        }
    }
}
