using IslandDefense.Troops;

namespace StateMachine
{
    /// <summary>
    /// Defend state - troop is defending a position
    /// </summary>
    public class TroopDefendState : ATroopState
    {
        public TroopDefendState(TroopController controller, TroopBase troopBase, TroopView view) : base(controller, troopBase, view)
        {
        }
        
        public override void Enter()
        {
            view.PlayDefendAnimation();
        }
        
        public override void Update()
        {
            // Defend position, attack enemies that get too close
        }
        
        public override void Exit()
        {
            // Nothing specific to do here
        }
    }
}