using IslandDefense.Troops;
using UnityEngine;

namespace StateMachine
{
    /// <summary>
    /// Attack state - troop is attacking an enemy
    /// </summary>
    public class TroopAttackState : ATroopState
    {
        private float attackDistanceCheck = 0.5f;
        
        public TroopAttackState(TroopController controller, TroopBase troopBase, TroopView view) : base(controller, troopBase, view)
        {
        }
        
        public override void Enter()
        {
            view.PlayCombatIdleAnimation();
        }
        
        public override void Update()
        {
            // Check if target is still valid
            if (troopBase.TargetTroop == null || troopBase.TargetTransform == null)
            {
                controller.OnLostTarget();
                return;
            }
            
            // Check if target is out of range
            float distanceToTarget = Vector3.Distance(troopBase.transform.position, troopBase.TargetTransform.position);
            
            if (distanceToTarget > troopBase.AttackRange + attackDistanceCheck)
            {
                // Target too far, move closer
                view.PlayMoveAnimation();
            }
            else
            {
                // In range, using AttackBehavior to handle attack logic
                view.PlayCombatIdleAnimation();
            }
        }
        
        public override void Exit()
        {
            // Reset attack indicators
        }
    }
}