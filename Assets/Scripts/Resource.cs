using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace moon
{
    public class Resource : ScriptableObject, IIcon, ISelectable
    {
        public int ID => id; 
        public Sprite Icon => icon;
        public Color Color => color;
        public GameObject Prefab => prefab;

        [SerializeField] Sprite icon;
        [SerializeField] Color color;
        [SerializeField] GameObject prefab;
        [SerializeField] int id; 
    }
}
