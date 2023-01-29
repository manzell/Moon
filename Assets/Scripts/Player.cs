using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Netcode;
using Mono.Cecil;
using Sirenix.Utilities;
using System.Security.Cryptography;
using Unity.VisualScripting;
using TMPro;

namespace moon
{
    public class Player : NetworkBehaviour
    {
        public static Player GetById(ulong playerID) => Game.Players.FirstOrDefault(player => player.OwnerClientId == playerID);

        public static System.Action<Player, PlayCard> AddCardToTableauEvent, RemoveCardFromTableauEvent;
        public System.Action setBaseCardEvent; 
        public System.Action<IEnumerable<Resource>> AddResourcesEvent, LoseResourcesEvent;
        public System.Action<Card> AddCardToHandEvent, RemoveCardFromHandEvent;
        public System.Action<ReputationCard> ClaimReputationCardEvent;
        public System.Action<IConstructionCard> DeployRoverEvent, RecallRoverEvent;
        public System.Action<ResourceCard> ProduceEvent; 
        
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

        public void AddResources(IEnumerable<Resource> resources) => ModifyResource_ClientRpc(resources.Select(resource => resource.ID).ToArray(), true);
        public void RemoveResources(IEnumerable<Resource> resources) => ModifyResource_ClientRpc(resources.Select(resource => resource.ID).ToArray(), false);
        [ClientRpc] void ModifyResource_ClientRpc(int[] ResourceIDs, bool add)
        {
            foreach (int id in ResourceIDs.Distinct())
            {
                Resource resource = Game.Resources.all.FirstOrDefault(resource => resource.ID == id);
                IEnumerable<Resource> resources = ResourceIDs.Where(rid => rid == id).Select(rid => resource);

                if(add)
                {
                    Resources.AddRange(resources);
                    AddResourcesEvent?.Invoke(resources);
                }
                else
                {
                    IEnumerable<Resource> resourcesToRemove = Resources.Where(resource => resource == Game.Resources.wildcard || resource.ID == id)
                        .OrderByDescending(resource => resource != Game.Resources.wildcard).Take(resources.Count());

                    resourcesToRemove.ForEach(resource => Resources.Remove(resource)); 
                    LoseResourcesEvent?.Invoke(resourcesToRemove);
                }
            }
        }

        public void SetBase(BaseCard card) => SetBase_ClientRpc(card.ID); 
        [ClientRpc] void SetBase_ClientRpc(int baseID)
        {
            BaseCard = Card.GetById<BaseCard>(baseID);
            setBaseCardEvent?.Invoke(); 
        }

        public void AddCardToTableau(PlayCard card) => ModifyCardsInTableau_ClientRpc(card.ID, true);
        public void RemoveCardFromTableau(PlayCard card) => ModifyCardsInTableau_ClientRpc(card.ID, false);
        [ClientRpc] void ModifyCardsInTableau_ClientRpc(int cardID, bool add)
        {
            PlayCard card = Card.GetById<PlayCard>(cardID); 

            if (add)
            {
                Tableau.Add(card);
                AddCardToTableauEvent?.Invoke(this, card);
            }
            else
            {
                Tableau.Remove(card);
                RemoveCardFromTableauEvent?.Invoke(this, card);
            }
        }
       
        public void AddCardsToHand(IEnumerable<Card> cards) => ModifyCardsInHand_ClientRpc(cards.Select(card => card.ID).ToArray(), true);
        public void RemoveCardsFromHand(IEnumerable<Card> cards) => ModifyCardsInHand_ClientRpc(cards.Select(card => card.ID).ToArray(), false);
        [ClientRpc] void ModifyCardsInHand_ClientRpc(int[] cardIDs, bool add)
        {
            foreach (int id in cardIDs)
            {
                Card card = Card.GetById(id);

                if (add)
                {
                    Hand.Add(card);
                    AddCardToHandEvent?.Invoke(card);
                }
                else
                {
                    Hand.Remove(card);
                    RemoveCardFromHandEvent?.Invoke(card);
                }
            }
        }

        public void AddReputationCard(ReputationCard card) => AddReputationCard_ClientRpc(card.ID); 
        [ClientRpc] void AddReputationCard_ClientRpc(int cardID)
        {
            ReputationCard card = Card.GetById<ReputationCard>(cardID);
            ReputationCards.Add(card);
            ClaimReputationCardEvent?.Invoke(card);
        }

        public void DeployRover(IConstructionCard card) => ModifyRoverLocation_ClientRpc(card.ID, true);
        public void RecallRover(IConstructionCard card) => ModifyRoverLocation_ClientRpc(card.ID, false);
        [ClientRpc] void ModifyRoverLocation_ClientRpc(int cardID, bool add)
        {
            IConstructionCard card = Card.GetById<IConstructionCard>(cardID);

            if (add)
            {
                RoverLocations.Add(card);
                DeployRoverEvent?.Invoke(card);
            }
            else
            {
                RoverLocations.Remove(card);
                RecallRoverEvent?.Invoke(card);
            }
        }

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
