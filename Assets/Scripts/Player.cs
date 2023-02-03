using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Netcode;
using Sirenix.Utilities;

namespace moon
{
    public class Player : NetworkBehaviour
    {
        public static Player GetById(ulong playerID) => Game.Players.FirstOrDefault(player => player.OwnerClientId == playerID);

        public static System.Action<Player, PlayCard> AddCardToTableauEvent, RemoveCardFromTableauEvent;
        public System.Action<BaseCard> setBaseCardEvent; 
        public System.Action<IEnumerable<Resource>> AddResourcesEvent, LoseResourcesEvent;
        public System.Action<Card> AddCardToHandEvent, RemoveCardFromHandEvent;
        public System.Action<ReputationCard> ClaimReputationCardEvent;
        public System.Action<IConstructionCard> DeployRoverEvent, RecallRoverEvent;
        
        public List<Resource> Resources { get; private set; } = new();
        public List<PlayCard> Tableau { get; private set; } = new();
        public List<Card> Hand { get; private set; } = new();
        public List<ReputationCard> ReputationCards { get; private set; } = new();
        public List<IConstructionCard> RoverLocations { get; private set; } = new();
        public BaseCard BaseCard { get; private set; }

        public IEnumerable<Flag> Flags => Tableau.OfType<FlagCard>().SelectMany(card => card.Flags)
            .Union(RoverLocations.OfType<FlagCard>().SelectMany(card => card.Flags))
            .Union(new List<Flag>() { BaseCard.Flag });

        public ExpeditionCard ExpeditionCard => Hand.OfType<ExpeditionCard>().FirstOrDefault(); 

        public override void OnNetworkSpawn()
        {
            Game.AddPlayer(this);
        }

        public void AddResources(IEnumerable<Resource> resources)
        {
            foreach (Resource resource in resources.Distinct())
            {
                IEnumerable<Resource> thisResource = Resources.Where(res => res == resource);

                Resources.AddRange(resources);
                TriggerAddResourcesEvent_ClientRpc(resources.Select(res => res.ID).ToArray());
            }
        }        
        public void RemoveResources(IEnumerable<Resource> resources)
        {
            foreach(Resource resource in resources.Distinct())
            {
                IEnumerable<Resource> resourcesToRemove = Resources.Where(res => res == resource || res == Game.Resources.wildcard)
                                .OrderByDescending(resource => resource != Game.Resources.wildcard)
                                .Take(resources.Count(r => r == resource));

                resourcesToRemove.ForEach(resource => Resources.Remove(resource));
                TriggerRemoveResourcesEvent_ClientRpc(resourcesToRemove.Select(res => res.ID).ToArray());
            }
        }
        [ClientRpc] void TriggerAddResourcesEvent_ClientRpc(int[] resourceIDs) => AddResourcesEvent?.Invoke(resourceIDs.Select(id => Resource.GetById(id)));
        [ClientRpc] void TriggerRemoveResourcesEvent_ClientRpc(int[] resourceIDs) => AddResourcesEvent?.Invoke(resourceIDs.Select(id => Resource.GetById(id)));

        public void SetBaseCard(BaseCard card)
        {
            BaseCard = card;
            TriggerSetBaseCardEvent_ClientRpc(card.ID); 
        }
        [ClientRpc] void TriggerSetBaseCardEvent_ClientRpc(int cardID) => setBaseCardEvent?.Invoke(Card.GetById<BaseCard>(cardID)); 

        public void AddCardToTableau(PlayCard card)
        {
            Tableau.Add(card);
            TriggerAddCardToTableauEvent_ClientRpc(card.ID);
        }
        public void RemoveCardFromTableau(PlayCard card)
        {
            if(Tableau.Remove(card))
                TriggerRemoveCardFromTableauEvent_ClientRpc(card.ID);
        }
        [ClientRpc] void TriggerAddCardToTableauEvent_ClientRpc(int cardID) => AddCardToTableauEvent?.Invoke(this, Card.GetById<PlayCard>(cardID));
        [ClientRpc] void TriggerRemoveCardFromTableauEvent_ClientRpc(int cardID) => RemoveCardFromTableauEvent?.Invoke(this, Card.GetById<PlayCard>(cardID));

        public void AddCardsToHand(IEnumerable<Card> cards) => cards.ForEach(card => AddCardToHand(card)); 
        public void AddCardToHand(Card card)
        {
            Hand.Add(card);
            TriggerAddCardToHandEvent_ClientRpc(card.ID);
        }
        public void RemoveCardsFromHand(IEnumerable<Card> cards) => cards.ForEach(card => RemoveCardFromHand(card));
        public void RemoveCardFromHand(Card card)
        {
            if(Hand.Remove(card))
                TriggerRemoveCardFromHandEvent_ClientRpc(card.ID);
        }
        [ClientRpc] void TriggerAddCardToHandEvent_ClientRpc(int cardId) => AddCardToHandEvent?.Invoke(Card.GetById(cardId));
        [ClientRpc] void TriggerRemoveCardFromHandEvent_ClientRpc(int cardId) => AddCardToHandEvent?.Invoke(Card.GetById(cardId));

        public void ClaimReputationCard(ReputationCard card)
        {
            ReputationCards.Add(card);
            TriggerClaimRepCardEvent_ClientRpc(card.ID);
        }
        [ClientRpc] void TriggerClaimRepCardEvent_ClientRpc(int cardID) => ClaimReputationCardEvent?.Invoke(Card.GetById<ReputationCard>(cardID));

        public void DeployRover(IConstructionCard card)
        {
            RoverLocations.Add(card);
            TriggerDeployRoverEvent_ClientRpc(card.ID); 
        }
        public void RecallRover(IConstructionCard card)
        {
            RoverLocations.Remove(card);
            TriggerRecallRoverEvent_ClientRpc(card.ID);
        }
        [ClientRpc] void TriggerDeployRoverEvent_ClientRpc(int cardID) => DeployRoverEvent?.Invoke(Card.GetById<IConstructionCard>(cardID));
        [ClientRpc] void TriggerRecallRoverEvent_ClientRpc(int cardID) => RecallRoverEvent?.Invoke(Card.GetById<IConstructionCard>(cardID));

        public bool CanAfford(IEnumerable<Resource> resourceCost, IEnumerable<Flag> flagCost)
        {
            List<Resource> rcost = new(resourceCost);
            List<Flag> fcost = new(flagCost);

            foreach (Resource resource in Resources)
                rcost.Remove(resource); 

            foreach (Flag flag in Flags)
                fcost.Remove(flag);

            return fcost.Count() == 0 && rcost.Count() <= Resources.Count(resource => resource == Game.Resources.wildcard);
        }
        public void Discard(Card card) => RemoveCardsFromHand(new List<Card>() { card }); 
    }
}
