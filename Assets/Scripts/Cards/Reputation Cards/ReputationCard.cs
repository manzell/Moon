using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace moon
{
    public class ReputationCard : Card
    {
        public enum Level { Bronze, Silver, Gold }

        [field: SerializeField] public Level GoalLevel { get; private set; }
        [field: SerializeField] public int VP { get; private set; }
        [field: SerializeField] public string conditionText { get; private set; }

        [field: SerializeReference] public IReputationCalc CanClaim { get; private set; }
        [field: SerializeReference] public TurnAction ClaimAction { get; private set; } 

        public Player Owner { get; private set; } 

        public void OnClaimCard(Player player)
        {
            if (CanClaim.Value(player))
            {
                Owner = player;
                ClaimAction.Execute(player);
            }
        }
    }

    public interface IReputationCalc 
    { 
        public bool Value(Player player);  
    }

    public class SynchronizedCalc : Calculation<bool>, IReputationCalc
    {
        [SerializeField] int numRequired = 2;
        protected override bool Calculate(Player player)
        {
            IEnumerable<VictoryCard> grayCards = player.Tableau.OfType<VictoryCard>();
            return grayCards.Any(card => grayCards.Count(c => c == card) >= numRequired);
        }
    }

    public class OmnipotentCalc : Calculation<bool>, IReputationCalc
    {
        [SerializeField] int setsRequired = 3;
        protected override bool Calculate(Player player)
        {
            int gray = player.Tableau.OfType<ActionCard>().Count(); 
            int yellow = player.Tableau.OfType<FlagCard>().Count();
            int blue = player.Tableau.OfType<ResourceCard>().Count();
            int pink = player.Tableau.OfType<VictoryCard>().Count();

            return Mathf.Min(gray, yellow, blue, pink) >= setsRequired; 
        }
    }

    public class FuturisticCalc : Calculation<bool>, IReputationCalc
    {
        [SerializeField] int numRequired = 2;
        protected override bool Calculate(Player player)
        {
            return player.Tableau.OfType<ResourceCard>().Count(card => card.CardResources.Count() >= 3) >= numRequired;
        }
    }

    public class InvestedCalc : Calculation<bool>, IReputationCalc
    {
        [SerializeField] int numRequired = 8;
        protected override bool Calculate(Player player)
        {
            return player.Tableau.OfType<FlagCard>().Count() >= numRequired; 
        }
    }

    public class BombasticCalc : Calculation<bool>, IReputationCalc
    {
        [SerializeField] int numRequired = 2;
        protected override bool Calculate(Player player)
        {
            return player.Tableau.OfType<FlagCard>().Count(card => card.FlagRequirements.Count() >= 3) >= numRequired; 
        }
    }

    public class ProsperousCalc : Calculation<bool>, IReputationCalc
    {
        [SerializeField] int numRequired = 3;
        protected override bool Calculate(Player player)
        {
            return Mathf.Min(player.Resources.Count(res => res == Game.Resources.metals), player.Resources.Count(res => res == Game.Resources.water), 
                player.Resources.Count(res => res == Game.Resources.energy), player.Resources.Count(res => res == Game.Resources.organics)) >= numRequired;
        }
    }

    public class CapableCalc : Calculation<bool>, IReputationCalc
    {
        [SerializeField] int numRequired = 7; 
        protected override bool Calculate(Player player)
        {
            return Game.CurrentGame.CurrentTurn.Actions.OfType<BuildAction>().Sum(action => (action.Card as PlayCard).ResourceRequirements.Count()) >= numRequired;
        }
    }

    public class AmbitiousCalc : Calculation<bool>, IReputationCalc
    {
        [SerializeField] int numRequired = 3;
        protected override bool Calculate(Player player)
        {
            return player.Tableau.OfType<VictoryCard>().Count() >= numRequired; 
        }
    }

    public class TeemingCalc : Calculation<bool>, IReputationCalc
    {
        [SerializeField] int numRequired = 6;
        protected override bool Calculate(Player player)
        {
            return player.Tableau.OfType<IConstructionCard>().Count(card => card.Rover != null) >= numRequired; 
        }
    }

    public class DynamicCalc : Calculation<bool>, IReputationCalc
    {
        [SerializeField] int numRequired = 5;
        protected override bool Calculate(Player player)
        {
            return player.Tableau.OfType<ResourceCard>().Count() >= numRequired; 
        }
    }

    public class DiversifiedCalc : Calculation<bool>, IReputationCalc
    {
        protected override bool Calculate(Player player)
        {
            return player.Flags.Contains(Game.Flags.food) && player.Flags.Contains(Game.Flags.science) && player.Flags.Contains(Game.Flags.housing) &&
                player.Flags.Contains(Game.Flags.transportation) && player.Flags.Contains(Game.Flags.food);
        }
    }

    public class FocusedCalc : Calculation<bool>, IReputationCalc
    {
        [SerializeField] int numRequired = 6; 
        protected override bool Calculate(Player player)
        {
            return Mathf.Max(player.Flags.Count(flag => flag == Game.Flags.food), player.Flags.Count(flag => flag == Game.Flags.science),
                player.Flags.Count(flag => flag == Game.Flags.housing), player.Flags.Count(flag => flag == Game.Flags.transportation),
                player.Flags.Count(flag => flag == Game.Flags.production)) >= numRequired;
        }
    }

    public class IndustriousCalc : Calculation<bool>, IReputationCalc
    {
        [SerializeField] int numRequired = 8;
        protected override bool Calculate(Player player)
        {
            return player.Tableau.OfType<ResourceCard>().Sum(card => card.CardResources.Count()) >= numRequired; 
        }
    }

    public class CreativeCalc : Calculation<bool>, IReputationCalc
    {
        [SerializeField] int numRequired = 3;

        protected override bool Calculate(Player player)
        {
            return player.Tableau.OfType<ActionCard>().Count(card => card.FlipAction.used == true) >= numRequired; 
        }
    }

    public class ProactiveCalc : Calculation<bool>, IReputationCalc
    {
        [SerializeField] int numRequired = 4;

        protected override bool Calculate(Player player)
        {
            return Game.CurrentGame.CurrentTurn.Actions.OfType<BuildAction>().Sum(action => (action.Card as PlayCard).ResourceRequirements.Count()) >= numRequired;
        }
    }

    public class StockedCalc : Calculation<bool>, IReputationCalc
    {
        [SerializeField] int numRequired = 5;

        protected override bool Calculate(Player player)
        {
            return Mathf.Max(player.Resources.Count(res => res == Game.Resources.metals), player.Resources.Count(res => res == Game.Resources.water),
                player.Resources.Count(res => res == Game.Resources.energy), player.Resources.Count(res => res == Game.Resources.organics)) >= numRequired;
        }
    }

    public class RoamingCalc : Calculation<bool>, IReputationCalc
    {
        protected override bool Calculate(Player player)
        {
            return player.Resources.Count(res => res == Game.Resources.rover) == 0; 
        }
    }

    public class DistinctiveCalc : Calculation<bool>, IReputationCalc
    {
        [SerializeField] int numRequired = 3;
        protected override bool Calculate(Player player)
        {
            return Mathf.Max(player.Flags.Count(flag => flag == Game.Flags.food), player.Flags.Count(flag => flag == Game.Flags.science),
                player.Flags.Count(flag => flag == Game.Flags.housing), player.Flags.Count(flag => flag == Game.Flags.transportation),
                player.Flags.Count(flag => flag == Game.Flags.production)) >= numRequired;
        }
    }

    public class BoldCalc : Calculation<bool>, IReputationCalc
    {
        protected override bool Calculate(Player player)
        {
            return Game.CurrentGame.CurrentTurn?.Actions.OfType<AssimilateAction>().Count() > 0; 
        }
    }

    public class SpecializedCalc : Calculation<bool>, IReputationCalc
    {
        [SerializeField] int numRequired = 2;
        protected override bool Calculate(Player player)
        {
            IEnumerable<ResourceCard> cards = player.Tableau.OfType<ResourceCard>();
            return cards.Any(card => cards.Count(c => c == card) >= numRequired);
        }
    }

    public class InnovativeCalc : Calculation<bool>, IReputationCalc
    {
        [SerializeField] int numRequired = 3;
        protected override bool Calculate(Player player)
        {
            return player.Tableau.Sum(card => card.CardResources.Count(res => res == Game.Resources.vp)) >= numRequired; 
        }
    }

    public class VersatileCalc : Calculation<bool>, IReputationCalc
    {
        protected override bool Calculate(Player player)
        {
            return Mathf.Min(player.Tableau.OfType<ResourceCard>().Count(), player.Tableau.OfType<FlagCard>().Count(), 
                player.Tableau.OfType<VictoryCard>().Count(), player.Tableau.OfType<ActionCard>().Count()) >= 1;
        }
    }

    public class NullAction : TurnAction
    {
        protected override void Do(Player player)
        {
            throw new System.NotImplementedException(); 
        }
    }

    public class BoldAction : TurnAction
    {
        protected override void Do(Player player)
        {
            ActionEvent += a => { if (a is AssimilateAction) TakeHeartsOnAssimilation(a); };
        }

        void TakeHeartsOnAssimilation(TurnAction action)
        {
            if(action.Player == Player)
            {
                List<Resource> vps = new(); 

                for(int i = 0; i < Game.CurrentGame.CurrentEra.Multiplier; i++)
                    vps.Add(Game.Resources.vp);

                Player.AddResources(vps); 
            }
        }
    }

    public class DistinctiveAction : TurnAction
    {
        protected override void Do(Player player) => ScoringPhase.FlagRewardEvent += TakeHeartsOnFlagScore; 

        void TakeHeartsOnFlagScore(Flag flag, Player winner, List<Resource> resources)
        {
            if (winner == Player)
            {
                List<Resource> vps = new();

                for (int i = 0; i < Game.CurrentGame.CurrentEra.Multiplier; i++)
                    vps.Add(Game.Resources.vp);

                Player.AddResources(vps);
            }
        }
    }

    public class SpecializedAction : TurnAction
    {
        protected override void Do(Player player) => ActionEvent += TakeHeartOnRoverAction; 

        void TakeHeartOnRoverAction(TurnAction turn)
        {
            if (turn is RoverAction && turn.Player == Player) 
                Player.AddResources(new List<Resource>() { Game.Resources.vp});
        }
    }

    public class StockedAction : TurnAction
    {
        protected override void Do(Player player) => ActionEvent += TakeHeartOnFlip;

        void TakeHeartOnFlip(TurnAction action)
        {
            if (action is FlipAction && action.Player == Player)
                Player.AddResources(Enumerable.Repeat(Game.Resources.vp, Game.CurrentGame.CurrentEra.Multiplier)); 
        }
    }

    public class VersatileAction: TurnAction
    {
        protected override void Do(Player player) => Phase.PhaseStartEvent += TakeHeartsForStructureSets;

        void TakeHeartsForStructureSets()
        {
            int action = Player.Tableau.OfType<ActionCard>().Count();
            int resource = Player.Tableau.OfType<ResourceCard>().Count();
            int flag = Player.Tableau.OfType<FlagCard>().Count();
            int victory = Player.Tableau.OfType<VictoryCard>().Count();

            List<Resource> vps = new();

            for (int i = 0; i < Game.CurrentGame.CurrentEra.Multiplier * Mathf.Min(action, resource, flag, victory); i++)
                vps.Add(Game.Resources.vp);

            Player.AddResources(vps);
        }
    }

    public class RoamingAction : TurnAction
    {
        protected override void Do(Player player)
        {
            Phase.PhaseStartEvent += async () => { 
                Selection<Resource> selection = new(player, Game.Resources.all);
                Game.CurrentGame.SelectionWindow.SetTitle("Gain one Extra Resource");

                await selection.Completion;

                player.AddResources(new List<Resource>() { selection.SelectedItem }); 
            }; 
        }
    }

    public class InnovativeAction : TurnAction
    {
        protected override void Do(Player player)
        {
            throw new System.NotImplementedException(); 
        }
    }
}
