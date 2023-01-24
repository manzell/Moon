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
        Player player;
        Game game; 

        public void SetPlayer(Player player)
        {
            this.player = player;

            game = FindObjectOfType<Game>();

            player.AddResourceEvent += UpdateCount;
            player.LoseResourceEvent += UpdateCount;

            Style();
            UpdateCount(resource); 
        }

        void Style()
        {
            background.color = resource.Color; 
            resourceIcon.sprite = resource.Icon;
        }

        void UpdateCount(Resource r)
        {
            if (r == resource)
                resourceCount.text = player.Resources.Count(resource => resource == r).ToString(); 
        }
    }
}
