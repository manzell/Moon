using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Netcode;
using Mono.Cecil;
using Sirenix.Utilities;

namespace moon
{
    public class Player : NetworkBehaviour
    {
        public static Player GetById(ulong playerID) => Game.Players.FirstOrDefault(player => player.OwnerClientId == playerID);

        public System.Action<IEnumerable<Resource>> AddResourcesEvent, LoseResourcesEvent;
        public static System.Action<Player, PlayCard> AddCardToTableauEvent, RemoveCardFromTableauEvent;
        public System.Action<Card> AddCardToHandEvent, RemoveCardFromHandEvent;
        public System.Action<ReputationCard> ClaimReputationCardEvent;
        public System.Action<IConstructionCard> DeployRoverEvent, RecallRoverEvent;
        public System.Action<ResourceCard> ProduceEvent; 

        public ClientRpcParams rpcParams { get; private set; }
        
        public List<Resource> Resources { get; private set; } = new();
        public List<PlayCard> Tableau { get; private set; } = new();
        public List<Card> Hand { get; private set; } = new();
        public List<ReputationCard> ReputationCards { get; private set; } = new();
        public List<IConstructionCard> RoverLocations { get; private set; } = new();
        public BaseCard BaseCard { get; private set; } // Has no RPCs

        public IEnumerable<Flag> Flags => Tableau.OfType<FlagCard>().SelectMany(card => card.Flags)
            .Union(RoverLocations.OfType<FlagCard>().SelectMany(card => card.Flags));

        public ExpeditionCard ExpeditionCard => Hand.OfType<ExpeditionCard>().FirstOrDefault(); 

        public override void OnNetworkSpawn()
        {
            rpcParams = new()  { Send = new() { TargetClientIds = new List<ulong>() { OwnerClientId } } };

            Game.AddPlayer(this);
            Game.StartGameEvent += OnGameStart;

            if (IsOwner)
                FindObjectOfType<UI_Game>().SetPlayer(this);
        }

        public void OnGameStart()
        {
            AddResources(Game.GameSettings.StartingResources); 
        }

        public void AddResources(IEnumerable<Resource> resources) => ModifyResource_ClientRpc(resources.Select(resource => resource.ID).ToArray(), true, rpcParams);
        public void RemoveResources(IEnumerable<Resource> resources) => ModifyResource_ClientRpc(resources.Select(resource => resource.ID).ToArray(), false, rpcParams);
        [ClientRpc] void ModifyResource_ClientRpc(int[] ResourceIDs, bool add, ClientRpcParams args)
        {
            foreach (int id in ResourceIDs.Distinct())
            {
                IEnumerable<Resource> resources = ResourceIDs.Where(rid => rid == id).Select(rid => Game.Resources.all.FirstOrDefault(resource => resource.ID == rid));

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

                Debug.Log($"{name} {(resources.Count() > 0 ? "+" : "-")}{resources.Count()} {resources.First().name}");
            }
        }

        public void AddCardToTableau(PlayCard card) => ModifyCardsInTableau_ClientRpc(card.ID, true, rpcParams);
        public void RemoveCardFromTableau(PlayCard card) => ModifyCardsInTableau_ClientRpc(card.ID, false, rpcParams);
        [ClientRpc] void ModifyCardsInTableau_ClientRpc(int cardID, bool add, ClientRpcParams args)
        {
            PlayCard card = Game.Cards.OfType<PlayCard>().FirstOrDefault(card => card.ID == cardID);

            if(add)
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
       
        public void AddCardsToHand(IEnumerable<Card> cards) => ModifyCardsInHand_ClientRpc(cards.Select(card => card.ID).ToArray(), true, rpcParams);
        public void RemoveCardsFromHand(IEnumerable<Card> cards) => ModifyCardsInHand_ClientRpc(cards.Select(card => card.ID).ToArray(), false, rpcParams);
        [ClientRpc] void ModifyCardsInHand_ClientRpc(int[] cardIDs, bool add, ClientRpcParams args)
        {
            foreach(int id in cardIDs)
            {
                Card card = Game.Cards.FirstOrDefault(card => card.ID == id);
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

        public void AddReputationCard(ReputationCard card) => AddReputationCard_ClientRpc(card.ID, rpcParams); 
        [ClientRpc] void AddReputationCard_ClientRpc(int cardID, ClientRpcParams args)
        {
            ReputationCard card = Game.Cards.OfType<ReputationCard>().FirstOrDefault(card => card.ID == cardID); 
            ReputationCards.Add(card);
            ClaimReputationCardEvent?.Invoke(card);
        }

        public void DeployRover(IConstructionCard card) => ModifyRoverLocation_ClientRpc(card.ID, true, rpcParams);
        public void RecallRover(IConstructionCard card) => ModifyRoverLocation_ClientRpc(card.ID, false, rpcParams);
        [ClientRpc] void ModifyRoverLocation_ClientRpc(int cardID, bool add, ClientRpcParams args)
        {
            IConstructionCard card = Game.Cards.OfType<IConstructionCard>().FirstOrDefault(card => card.ID == cardID);
            if(add)
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
            {
                if (rcost.Remove(resource))
                    Debug.Log($"Paid down 1 {resource.name}"); 
            }

            foreach (Flag flag in Flags)
                fcost.Remove(flag);

            Debug.Log($"Player owes {rcost.Count} Resources (has {Resources.Count(resource => resource == Game.Resources.wildcard)} Wildcards) and {fcost.Count} Flags"); 

            return fcost.Count() == 0 && rcost.Count() <= Resources.Count(resource => resource == Game.Resources.wildcard);
        }
        public void Discard(Card card) => RemoveCardsFromHand(new List<Card>() { card }); 
    }
}
