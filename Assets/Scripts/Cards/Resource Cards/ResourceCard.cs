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

        public IEnumerable<IIcon> Icons => productionAction.Icons;

        public IEnumerable<IIcon> Resources => productionAction.resources; 

        public void SetRover(Player player)
        {
            Rover = player;
            AddRoverEvent?.Invoke(player);
        }
    }

    [System.Serializable]
    public abstract class ProductionAction
    {
        public virtual IEnumerable<IIcon> Icons => resources;
        [field: SerializeField] public List<Resource> resources { get; private set; }

        public abstract Task<List<Resource>> Production(Player player);
    }

    public class BasicProduction : ProductionAction
    {
        public override Task<List<Resource>> Production(Player player)
        {
            return Task.FromResult(resources); 
        }
    }

    public class SelectProduction : ProductionAction
    {
        public override IEnumerable<IIcon> Icons => new List<IIcon>() { Game.Resources.wildcard };

        public override async Task<List<Resource>> Production(Player player) => 
            new List<Resource>() { await new Selection<Resource>(player, resources).Completion };
    }
}
