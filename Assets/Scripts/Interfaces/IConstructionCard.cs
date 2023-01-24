using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace moon
{
    public interface IConstructionCard : IPlayCard
    {
        public Player Rover { get; }
        public void SetRover(Player player); 
        public IEnumerable<GameObject> Icons { get; }

        public System.Action<Player> AddRoverEvent { get; set; }
        public System.Action RemoveRoverEvent { get; set; }
    }
}
