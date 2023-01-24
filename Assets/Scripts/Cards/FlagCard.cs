using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace moon
{
    public class FlagCard : PlayCard, IConstructionCard
    {
        public Action<Player> AddRoverEvent { get; set; }
        public Action RemoveRoverEvent { get; set; }
        public Player Rover { get; private set; }
        [SerializeField] List<Flag> flags = new();
        public List<Flag> Flags => flags;
        public IEnumerable<GameObject> Icons => flags.Select(flag => flag.Prefab);


        public void SetRover(Player player) => Rover = player; 
    }
}
