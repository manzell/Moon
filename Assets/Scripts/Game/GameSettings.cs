using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using Sirenix.OdinInspector; 

namespace moon
{
    [CreateAssetMenu]
    public class GameSettings : SerializedScriptableObject
    {
        [field: SerializeField] public Queue<Era> Eras { get; private set; }
        [field: SerializeField] public List<BaseCard> Bases { get; private set; }
        [field: SerializeField] public List<Resource> StartingResources { get; private set; }
        [field: SerializeField] public ExpeditionCard FirstPlayerExpedition { get; private set; }
        [field: SerializeField] public List<ExpeditionCard> ExpeditionCards { get; private set; }

        [field: SerializeField] public int MaxPlayers { get; private set; } = 5; 
        [field: SerializeField] public int ReputationCardsPerEra { get; private set; } = 3;
        [field: SerializeField] public List<Image> PlayerImages { get; private set; }
    }
}
