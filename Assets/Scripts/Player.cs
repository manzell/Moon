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
        public static Player GetById(ulong playerID) => Game.CurrentGame.Players.FirstOrDefault(player => player.OwnerClientId == playerID);

        public static System.Action<Player, PlayCard> AddCardToTableauEvent, RemoveCardFromTableauEvent;
        public System.Action<BaseCard> setBaseCardEvent;
        public System.Action enableTurnEndEvent; 
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
            Game.CurrentGame.AddPlayer(this);
        }

        public void AddResources(IEnumerable<Resource> resources) => resources.Distinct().ForEach(resource => AddResources_ServerRpc(resources.Where(res => res == resource).Select(r => r.ID).ToArray()));
        public void RemoveResources(IEnumerable<Resource> resources) => resources.Distinct().ForEach(resource => RemoveResources_ServerRpc(resources.Where(res => res == resource).Select(r => r.ID).ToArray()));
        [ServerRpc(RequireOwnership = false)] void AddResources_ServerRpc(int[] resourceIDs) => AddResources_ClientRpc(resourceIDs);
        [ClientRpc] void AddResources_ClientRpc(int[] resourceIDs)
        {
            IEnumerable<Resource> resources = resourceIDs.Select(id => Resource.GetById(id));

            Resources.AddRange(resources);
            AddResourcesEvent?.Invoke(resources);
        }
        [ServerRpc(RequireOwnership = false)] void RemoveResources_ServerRpc(int[] resourceIDs) => RemoveResources_ClientRpc(resourceIDs);
        [ClientRpc] void RemoveResources_ClientRpc(int[] resourceIDs)
        {
            foreach (int id in resourceIDs)
            {
                IEnumerable<Resource> resourcesToRemove = Resources.Where(res => res.ID == id || res == Game.Resources.wildcard)
                    .OrderByDescending(res => res != Game.Resources.wildcard).Take(resourceIDs.Count());

                resourcesToRemove.ForEach(resource => Resources.Remove(resource));
                LoseResourcesEvent?.Invoke(resourceIDs.Select(id => Resource.GetById(id)));
            }
        }

        public void SetBaseCard(BaseCard card) => SetBaseCard_ServerRpc(card.ID);
        [ServerRpc(RequireOwnership = false)] void SetBaseCard_ServerRpc(int cardID) => SetBaseCard_ClientRpc(cardID); 
        [ClientRpc] void SetBaseCard_ClientRpc(int cardID)
        {
            BaseCard = Card.GetById<BaseCard>(cardID);
            setBaseCardEvent?.Invoke(BaseCard);
        }

        public void AddCardToTableau(PlayCard card) => AddCardToTableau_ServerRpc(card.ID);
        public void RemoveCardFromTableau(PlayCard card) => RemoveCardFromTableau_ServerRpc(card.ID);
        [ServerRpc(RequireOwnership = false)] void AddCardToTableau_ServerRpc(int cardID) => AddCardToTableau_ClientRpc(cardID);
        [ClientRpc] void AddCardToTableau_ClientRpc(int cardID)
        {
            PlayCard card = Card.GetById<PlayCard>(cardID);
            Tableau.Add(card);
            AddCardToTableauEvent?.Invoke(this, card);
        }
        [ServerRpc(RequireOwnership = false)] void RemoveCardFromTableau_ServerRpc(int cardID) => RemoveCardFromTableau_ClientRpc(cardID);
        [ClientRpc] void RemoveCardFromTableau_ClientRpc(int cardID)
        {
            PlayCard card = Card.GetById<PlayCard>(cardID);
            if (Tableau.Remove(card))
                RemoveCardFromTableauEvent?.Invoke(this, card);
        }

        public void AddCardsToHand(IEnumerable<Card> cards) => cards.ForEach(card => AddCardToHand(card)); 
        public void AddCardToHand(Card card) => AddCardToHand_ServerRpc(card.ID);
        public void RemoveCardsFromHand(IEnumerable<Card> cards) => new List<Card>(cards).ForEach(card => RemoveCardFromHand(card));
        public void RemoveCardFromHand(Card card) => RemoveCardFromHand_ServerRpc(card.ID);
        [ServerRpc(RequireOwnership = false)] void AddCardToHand_ServerRpc(int cardId) => AddCardToHand_ClientRpc(cardId); 
        [ClientRpc] void AddCardToHand_ClientRpc(int cardId)
        {
            Card card = Card.GetById(cardId);
            Hand.Add(card);
            AddCardToHandEvent?.Invoke(card);
        }
        [ServerRpc(RequireOwnership = false)] void RemoveCardFromHand_ServerRpc(int cardId) => RemoveCardFromHand_ClientRpc(cardId); 
        [ClientRpc] void RemoveCardFromHand_ClientRpc(int cardId)
        {
            Card card = Card.GetById(cardId);

            if (Hand.Remove(card))
                RemoveCardFromHandEvent?.Invoke(card);
        }

        public void ClaimReputationCard(ReputationCard card) => ClaimRepCard_ServerRpc(card.ID);
        [ServerRpc(RequireOwnership = false)] void ClaimRepCard_ServerRpc(int cardID) => ClaimRepCard_ClientRpc(cardID); 
        [ClientRpc] void ClaimRepCard_ClientRpc(int cardID)
        {
            ReputationCard card = Card.GetById<ReputationCard>(cardID); 
            ReputationCards.Add(card);
            ClaimReputationCardEvent?.Invoke(Card.GetById<ReputationCard>(cardID));
        }

        public void DeployRover(IConstructionCard card) => DeployRover_ServerRpc(card.ID); 
        public void RecallRover(IConstructionCard card) => RecallRover_ServerRpc(card.ID);
        [ServerRpc(RequireOwnership = false)] void DeployRover_ServerRpc(int cardID) => DeployRover_ClientRpc(cardID); 
        [ClientRpc] void DeployRover_ClientRpc(int cardID)
        {
            IConstructionCard card = Card.GetById<IConstructionCard>(cardID);
            RoverLocations.Add(card);
            DeployRoverEvent?.Invoke(card);
        }
        [ServerRpc(RequireOwnership = false)] void RecallRover_ServerRpc(int cardID) => RecallRover_ClientRpc(cardID); 
        [ClientRpc] void RecallRover_ClientRpc(int cardID)
        {
            IConstructionCard card = Card.GetById<IConstructionCard>(cardID); 
            RoverLocations.Remove(card);
            RecallRoverEvent?.Invoke(Card.GetById<IConstructionCard>(cardID));
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
    }
}
