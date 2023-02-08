using UnityEngine;

namespace moon
{
    public class Flag : ScriptableObject, IIcon, ISelectable
    {
        public int ID => id;
        [SerializeField] Sprite icon;
        [SerializeField] Color color;
        [SerializeField] GameObject prefab; 

        public Sprite Icon => icon;
        public Color Color => color;
        public GameObject Prefab => prefab;
        [SerializeField] int id;
    }
}
