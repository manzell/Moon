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
    }
}
