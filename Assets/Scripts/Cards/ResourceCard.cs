using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace moon
{
    public class ResourceCard : PlayCard, IConstructionCard
    {
        public System.Action<Player> AddRoverEvent { get; set; }
        public System.Action RemoveRoverEvent { get; set; }

        public Player Rover { get; private set; }
        [field: SerializeReference] public ProductionAction productionAction { get; private set; }

        public IEnumerable<GameObject> Icons => productionAction.Icons;
        public void SetRover(Player player) => Rover = player; 
    }

    [System.Serializable]
    public abstract class ProductionAction
    {
        public virtual IEnumerable<GameObject> Icons => resources.Select(r => r.Prefab);
        [SerializeField] protected List<Resource> resources;

        public abstract Task<List<Resource>> Production();
    }

    public class BasicProduction : ProductionAction
    {
        public override Task<List<Resource>> Production()
        {
            var x = Task<List<Resource>>.FromResult(resources);
            Debug.Log(resources.First().name); 

            return x; 
        }
    }

    public class SelectProduction : ProductionAction
    {
        public override IEnumerable<GameObject> Icons => resources.Select(r => Game.Resources.wildcard.Prefab);

        public override async Task<List<Resource>> Production() => 
            new List<Resource>() { await new Selection<Resource>(resources).Completion };
    }
}
