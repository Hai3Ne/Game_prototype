using System.Collections.Generic;
using IslandDefense.Troops;
using UnityEngine;

namespace StateMachine
{
    /// <summary>
    /// State machine for managing troop states
    /// </summary>
    public class TroopStateMachine : MonoBehaviour
    {
        private Dictionary<TroopStateType, ATroopState> states = new Dictionary<TroopStateType, ATroopState>();
        private ATroopState currentState;
        private TroopStateType currentStateType;
        
        private TroopController controller;
        private TroopBase troopBase;
        private TroopView view;
        
        public TroopStateType CurrentStateType => currentStateType;
        
        /// <summary>
        /// Initialize the state machine
        /// </summary>
        public void Initialize(TroopController controller, TroopBase troopBase, TroopView view)
        {
            this.controller = controller;
            this.troopBase = troopBase;
            this.view = view;
            
            // Create states
            states.Add(TroopStateType.Idle, new TroopIdleState(controller, troopBase, view));
            states.Add(TroopStateType.Move, new TroopMoveState(controller, troopBase, view));
            states.Add(TroopStateType.Attack, new TroopAttackState(controller, troopBase, view));
            states.Add(TroopStateType.Defend, new TroopDefendState(controller, troopBase, view));
            states.Add(TroopStateType.Flee, new TroopFleeState(controller, troopBase, view));
            states.Add(TroopStateType.Dead, new TroopDeadState(controller, troopBase, view));
        }
        
        /// <summary>
        /// Change to a new state
        /// </summary>
        public void ChangeState(TroopStateType newStateType)
        {
            // Exit current state
            if (currentState != null)
            {
                currentState.Exit();
            }
            
            // Enter new state
            currentStateType = newStateType;
            currentState = states[newStateType];
            currentState.Enter();
        }
        
        /// <summary>
        /// Update the current state
        /// </summary>
        public void UpdateState()
        {
            if (currentState != null)
            {
                currentState.Update();
            }
        }
    }
}