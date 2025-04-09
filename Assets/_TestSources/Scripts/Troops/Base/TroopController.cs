using System.Collections;
using System.Collections.Generic;
using StateMachine;
using UnityEngine;

namespace IslandDefense.Troops
{
    /// <summary>
    /// Controller class for troops. Handles decision making and behavior switching.
    /// Follows MVC pattern as the Controller component.
    /// </summary>
    public class TroopController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TroopBase troopBase;
        [SerializeField] private TroopView troopView;
        
        [Header("State Machine")]
        [SerializeField] private TroopStateMachine stateMachine;
        
        // Cached behaviors
        private List<SteeringBehavior> allBehaviors = new List<SteeringBehavior>();
        
        protected virtual void Awake()
        {
            // Ensure references are set
            if (troopBase == null)
                troopBase = GetComponent<TroopBase>();
                
            if (troopView == null)
                troopView = GetComponent<TroopView>();
                
            // Create state machine if not assigned
            if (stateMachine == null)
            {
                stateMachine = gameObject.AddComponent<TroopStateMachine>();
                stateMachine.Initialize(this, troopBase, troopView);
            }
        }
        
        private void Start()
        {
            // Start in idle state
            stateMachine.ChangeState(TroopStateType.Idle);
        }
        
        private void Update()
        {
            stateMachine.UpdateState();
            
            // Calculate steering force and update position
            Vector3 steeringForce = troopBase.CalculateSteeringForce();
            troopBase.UpdatePosition(steeringForce, Time.deltaTime);
        }
        
        /// <summary>
        /// Find and target the nearest enemy
        /// </summary>
        public void FindTarget(float searchRadius, LayerMask enemyLayer)
        {
            Collider[] enemies = Physics.OverlapSphere(transform.position, searchRadius, enemyLayer);
            
            Transform closestTarget = null;
            float closestDistance = float.MaxValue;
            
            foreach (Collider enemy in enemies)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = enemy.transform;
                }
            }
            
            if (closestTarget != null)
            {
                troopBase.TargetTransform = closestTarget;
                troopBase.TargetTroop = closestTarget.GetComponent<TroopBase>();
                
                // Switch to attack state if we found a target
                stateMachine.ChangeState(TroopStateType.Attack);
            }
        }
        
        /// <summary>
        /// Move to a target position
        /// </summary>
        public void MoveToPosition(Vector3 targetPosition)
        {
            // Create a temporary transform to serve as movement target
            GameObject tempTarget = new GameObject("MoveTarget");
            tempTarget.transform.position = targetPosition;
            
            // Set as target
            troopBase.TargetTransform = tempTarget.transform;
            troopBase.TargetTroop = null;
            
            // Switch to move state
            stateMachine.ChangeState(TroopStateType.Move);
            
            // Destroy the temporary target after a delay
            Destroy(tempTarget, 0.1f);
        }
        
        /// <summary>
        /// Add a behavior component of the specified type
        /// </summary>
        public SteeringBehavior AddBehavior(BehaviorType behaviorType)
        {
            // Check if behavior already exists
            foreach (var behavior in allBehaviors)
            {
                if (behavior.GetType().Name == behaviorType.ToString() + "Behavior")
                {
                    return behavior;
                }
            }
            
            // Create new behavior based on type
            SteeringBehavior newBehavior = null;
            
            switch (behaviorType)
            {
                case BehaviorType.Seek:
                    newBehavior = gameObject.AddComponent<SeekBehavior>();
                    break;
                case BehaviorType.Flee:
                    newBehavior = gameObject.AddComponent<FleeBehavior>();
                    break;
                case BehaviorType.Arrival:
                    newBehavior = gameObject.AddComponent<ArrivalBehavior>();
                    break;
                case BehaviorType.Separation:
                    newBehavior = gameObject.AddComponent<SeparationBehavior>();
                    break;
                case BehaviorType.Attack:
                    newBehavior = gameObject.AddComponent<AttackBehavior>();
                    break;
                case BehaviorType.Fear:
                    newBehavior = gameObject.AddComponent<FearBehavior>();
                    break;
                // Other behaviors would be implemented here
                default:
                    Debug.LogWarning($"Behavior type {behaviorType} not implemented yet.");
                    break;
            }
            
            if (newBehavior != null)
            {
                allBehaviors.Add(newBehavior);
            }
            
            return newBehavior;
        }
        
        /// <summary>
        /// Called when the troop has reached its movement target
        /// </summary>
        public void OnReachedTarget()
        {
            // Switch back to idle state
            stateMachine.ChangeState(TroopStateType.Idle);
        }
        
        /// <summary>
        /// Called when the troop's target is out of range or destroyed
        /// </summary>
        public void OnLostTarget()
        {
            troopBase.TargetTransform = null;
            troopBase.TargetTroop = null;
            
            // Switch back to idle state
            stateMachine.ChangeState(TroopStateType.Idle);
        }
    }
}