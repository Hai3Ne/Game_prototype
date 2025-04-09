using IslandDefense.Troops;

namespace StateMachine
{
    /// <summary>
    /// Dead state - troop is dead
    /// </summary>
    public class TroopDeadState : ATroopState
    {
        public TroopDeadState(TroopController controller, TroopBase troopBase, TroopView view) : base(controller, troopBase, view)
        {
        }
        
        public override void Enter()
        {
            view.PlayDeathAnimation(null);
        }
        
        public override void Update()
        {
            // Nothing to do, we're dead
        }
        
        public override void Exit()
        {
            // This should never happen, but just in case
        }
    }
}