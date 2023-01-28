using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector; 

namespace moon
{
    [CreateAssetMenu]
    public class GameSettings : SerializedScriptableObject
    {
        [field: SerializeField] public Queue<Era> Eras;
        [field: SerializeField] public List<BaseCard> Bases; 
        [field: SerializeField] public List<Resource> StartingResources;
        [field: SerializeField] public ExpeditionCard FirstPlayerExpedition; 
        [field: SerializeField] public List<ExpeditionCard> ExpeditionCards; 
    }
}
