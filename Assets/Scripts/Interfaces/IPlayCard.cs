using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace moon
{
    public interface IPlayCard : ICard
    {
        public string Subtitle { get; }
        public List<Resource> AssimilationValue { get; }
        public List<Resource> ResourceRequirements { get; }
        public List<Flag> FlagRequirements { get; }
    
        public string Quote { get; }
    }
}
