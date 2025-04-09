using IslandDefense.Troops;
using UnityEngine;

namespace StateMachine
{
    /// <summary>
    /// Idle state - troop is waiting for orders
    /// </summary>
    public class TroopIdleState : ATroopState
    {
        private float autoSearchInterval = 2f;
        private float lastSearchTime = 0f;
        private LayerMask enemyLayer;
        private float searchRadius = 10f;
        
        public TroopIdleState(TroopController controller, TroopBase troopBase, TroopView view) : base(controller, troopBase, view)
        {
            enemyLayer = LayerMask.GetMask("Enemy");
        }
        
        public override void Enter()
        {
            view.PlayIdleAnimation();
        }
        
        public override void Update()
        {
            // Auto-search for enemies periodically
            if (Time.time >= lastSearchTime + autoSearchInterval)
            {
                lastSearchTime = Time.time;
                controller.FindTarget(searchRadius, enemyLayer);
            }
        }
        
        public override void Exit()
        {
            // Nothing specific to do here
        }
    }
}