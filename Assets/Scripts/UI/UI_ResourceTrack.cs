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

            player.AddResourcesEvent += UpdateCount;
            player.LoseResourcesEvent += UpdateCount;

            Style();
            UpdateCount(player.Resources); 
        }

        void Style()
        {
            background.color = resource.Color; 
            resourceIcon.sprite = resource.Icon;
        }

        void UpdateCount(IEnumerable<Resource> resources) => resourceCount.text = player.Resources.Count(resource => resource == this.resource).ToString();
    }
}
