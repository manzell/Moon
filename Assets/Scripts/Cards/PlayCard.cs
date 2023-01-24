using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace moon
{
    public abstract class PlayCard : Card, IPlayCard
    {
        public System.Action PlayEvent; 

        [SerializeField] string subtitle, quote; 
        [SerializeField] List<Flag> flagRequirements;
        [SerializeField] List<Resource> assimilationValue, resourceRequirements, cardResources;
        [SerializeField] Response<PlayCard> OnPlay;

        public List<Resource> AssimilationValue => assimilationValue;
        public List<Resource> ResourceRequirements => resourceRequirements;
        public List<Resource> CardResources => cardResources;
        public List<Flag> FlagRequirements => flagRequirements;
        public string Subtitle => subtitle;
        public string Quote => quote;

        public virtual void Play()
        {
            PlayEvent?.Invoke();
            OnPlay?.Do(this);
        }
    }

    public struct PlayCardData
    {
        public List<Resource> CardResources;
    }
}
