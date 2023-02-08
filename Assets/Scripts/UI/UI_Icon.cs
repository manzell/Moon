using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

namespace moon
{
    public class UI_Icon : MonoBehaviour
    {
        public IIcon Icon { get; private set; }
        [SerializeField] Image iconImage;
        [SerializeField] Image backgroundImage;

        private void Start()
        {
            if (Icon != null)
                Style(Icon); 
        }

        public void Style(IIcon icon)
        {
            Icon = icon; 
            backgroundImage.color = icon.Color;
            iconImage.sprite = icon.Icon;

            RectTransform rect = GetComponent<RectTransform>();
            Vector2 delta = transform.parent.GetComponent<RectTransform>().sizeDelta;

            float iconSize = Mathf.Min(delta.x, delta.y) - 2.5f;

            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, iconSize);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, iconSize);
        }
    }
}
