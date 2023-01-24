using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace moon
{
    public interface IIcon 
    {
        public Sprite Icon { get; }
        public Color Color { get; }
        public GameObject Prefab { get; }
    }
}
