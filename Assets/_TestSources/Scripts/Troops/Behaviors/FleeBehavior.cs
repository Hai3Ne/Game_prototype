using IslandDefense.Troops;
using UnityEngine;

/// <summary>
    /// Implementation of Flee behavior - moves away from a target
    /// </summary>
    public class FleeBehavior : SteeringBehavior
    {
        [Header("Flee Settings")]
        [SerializeField] private float maxFleeForce = 15f;
        [SerializeField] private float fleeRadius = 10f; // Only flee if within this radius

        public override Vector3 CalculateForce()
        {
            if (troopBase.TargetTransform == null)
                return Vector3.zero;
                
            Vector3 toTarget = transform.position - troopBase.TargetTransform.position;
            float distanceToTarget = toTarget.magnitude;
            
            // Only flee if within flee radius
            if (distanceToTarget > fleeRadius)
                return Vector3.zero;
                
            // Steering = normalized vector away from target * desired speed
            Vector3 desiredVelocity = toTarget.normalized * troopBase.MoveSpeed;
            
            // Steering = desired velocity - current velocity
            Vector3 steeringForce = desiredVelocity - troopBase.CurrentVelocity;
            
            // Limit steering force
            if (steeringForce.magnitude > maxFleeForce)
            {
                steeringForce = steeringForce.normalized * maxFleeForce;
            }
            
            return steeringForce;
        }
        
        public override void OnDrawGizmos()
        {
            if (troopBase != null && troopBase.TargetTransform != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, fleeRadius);
                
                // Draw flee vector if within radius
                Vector3 toTarget = transform.position - troopBase.TargetTransform.position;
                if (toTarget.magnitude <= fleeRadius)
                {
                    Gizmos.DrawLine(transform.position, transform.position + toTarget.normalized * 2f);
                }
            }
        }
        
        public override void InitializeFromConfig(BehaviorConfig behaviorConfig)
        {
            base.InitializeFromConfig(behaviorConfig);
            fleeRadius = behaviorConfig.SlowingRadius * 2f; // Use slowing radius parameter for flee radius
        }
    }