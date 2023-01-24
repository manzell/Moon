using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace moon
{
    public class ResourceCard : PlayCard, IConstructionCard
    {
        public Action<Player> AddRoverEvent { get; set; }
        public Action RemoveRoverEvent { get; set; }

        public Player Rover { get; private set; }
        [field: SerializeReference] public ProductionAction productionAction { get; private set; }

        public IEnumerable<GameObject> Icons => productionAction.Icons;
        public void SetRover(Player player) => Rover = player; 
    }

    [System.Serializable]
    public abstract class ProductionAction : Calculation<List<Resource>>
    {
        public abstract IEnumerable<GameObject> Icons { get; } 
    }

    public class BasicProduction : ProductionAction
    {
        public override IEnumerable<GameObject> Icons => resources.Select(r => r.Prefab); 
        [SerializeField] List<Resource> resources;

        protected override List<Resource> Calculate(Player player) => resources;
    }

    public class SelectProduction : ProductionAction
    {
        public override IEnumerable<GameObject> Icons => new List<GameObject> { Game.Resources.wildcard.Prefab }; 
        [SerializeField] List<Resource> resources;

        protected override List<Resource> Calculate(Player player)
        {
            UI_SelectionWindow window = GameObject.Instantiate(Game.Graphics.SelectionPrefab, GameObject.FindObjectOfType<UI_Game>().transform);

            Debug.Log("Choice Calculation pre");
            window.StartSelection();

            // TODO Something needs to be awaited somewhere? 
            Debug.Log("Choice Calculation post"); 
            return new List<Resource>() { window.Selection.Task.Result as Resource };
        }
    }
}
