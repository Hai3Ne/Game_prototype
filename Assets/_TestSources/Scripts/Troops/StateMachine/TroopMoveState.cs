using IslandDefense.Troops;
using UnityEngine;

namespace StateMachine
{
    /// <summary>
    /// Move state - troop is moving to a position
    /// </summary>
    public class TroopMoveState : ATroopState
    {
        private float _arrivalDistance = 0.5f;
        
        public TroopMoveState(TroopController controller, TroopBase troopBase, TroopView view) : base(controller, troopBase, view)
        {
        }
        
        public override void Enter()
        {
            view.PlayMoveAnimation();
        }
        
        public override void Update()
        {
            // Check if we've reached the target
            if (troopBase.TargetTransform != null)
            {
                float distanceToTarget = Vector3.Distance(troopBase.transform.position, troopBase.TargetTransform.position);
                
                if (distanceToTarget <= _arrivalDistance)
                {
                    controller.OnReachedTarget();
                }
            }
            else
            {
                // If we have no target, go back to idle
                controller.OnLostTarget();
            }
        }
        
        public override void Exit()
        {
            // Nothing specific to do here
        }
    }
}