using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Netcode;

namespace moon
{
    public class Player : NetworkBehaviour
    {
        public static Player GetById(ulong playerID) => Game.Players.FirstOrDefault(player => player.NetworkObjectId == playerID);

        public System.Action<Resource> AddResourceEvent, LoseResourceEvent;
        public static System.Action<Player, PlayCard> AddCardToTableauEvent, RemoveCardFromTableauEvent;
        public System.Action<Card> AddCardToHandEvent, RemoveCardFromHandEvent;
        public System.Action<ReputationCard> ClaimReputationCardEvent;
        public System.Action<IConstructionCard> DeployRoverEvent, RecallRoverEvent;

        public System.Action<ResourceCard> ProduceEvent; 

        Game game;
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
            base.OnNetworkSpawn();

            rpcParams = new()  { Send = new() { TargetClientIds = new List<ulong>() { OwnerClientId } } }; 
            game = FindObjectOfType<Game>();
                
            game.AddPlayer(this);

            if (IsOwner)
                FindObjectOfType<UI_Game>().SetPlayer(this);
        }

        public void AddResources(List<Resource> resources) => ModifyResource_ClientRpc(resources.Select(resource => resource.ID).ToArray(), true, rpcParams);
        public void RemoveResources(List<Resource> resources) => ModifyResource_ClientRpc(resources.Select(resource => resource.ID).ToArray(), false, rpcParams);
        [ClientRpc] void ModifyResource_ClientRpc(int[] ResourceIDs, bool add, ClientRpcParams args)
        {
            foreach (int id in ResourceIDs)
            {
                Resource resource = Game.Resources.all.FirstOrDefault(resource => resource.ID == id);

                if(add)
                {
                    Resources.Add(resource);
                    AddResourceEvent?.Invoke(resource);
                }
                else
                {
                    Resources.Remove(resource);
                    LoseResourceEvent?.Invoke(resource);
                }
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
       
        public void AddCardToHand(Card card) => ModifyCardsInHand_ClientRpc(card.ID, true, rpcParams);
        public void RemoveCardFromHand(Card card) => ModifyCardsInHand_ClientRpc(card.ID, false, rpcParams);
        [ClientRpc] void ModifyCardsInHand_ClientRpc(int cardID, bool add, ClientRpcParams args)
        {
            Card card = Game.Cards.FirstOrDefault(card => card.ID == cardID);
            if(add)
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


        public bool CanAfford(List<Resource> resourceCost, List<Flag> flagCost)
        {
            List<Resource> playerResources = new(Resources);
            List<Flag> playerFlags = new(Flags);

            foreach (Resource resource in playerResources)
                resourceCost.Remove(resource);
            foreach (Flag flag in playerFlags)
                flagCost.Remove(flag);

            return flagCost.Count == 0 && resourceCost.Count <= Resources.Count(resource => resource == Game.Resources.wildcard);
        }

        public void Discard(Card card) => RemoveCardFromHand(card); 
    }
}
