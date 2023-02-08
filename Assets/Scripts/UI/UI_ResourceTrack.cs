using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using TMPro;
using System.Linq; 

namespace moon
{
    public class UI_ResourceTrack : MonoBehaviour
    {
        [SerializeField] Resource resource;
        [SerializeField] Image background, resourceIcon;
        [SerializeField] TextMeshProUGUI resourceCount;

        public void Setup()
        {
            Debug.Log($"Setting up resource monitor for {resource.name}");
            Game.CurrentGame.Player.AddResourcesEvent += r => UpdateCount();
            Game.CurrentGame.Player.LoseResourcesEvent += r => UpdateCount();
            Style();
            UpdateCount(); 
        }

        void Style()
        {
            background.color = resource.Color; 
            resourceIcon.sprite = resource.Icon;
        }

        void UpdateCount() => resourceCount.text = Game.CurrentGame.Player.Resources.Count(resource => resource == this.resource).ToString();
    }
}
