using IslandDefense.Troops;
using UnityEngine;

/// <summary>
    /// Implementation of Arrival behavior - slows down when approaching target
    /// </summary>
    public class ArrivalBehavior : SteeringBehavior
    {
        [Header("Arrival Settings")]
        [SerializeField] private float arrivalRadius = 1f;
        [SerializeField] private float slowingRadius = 5f;
        [SerializeField] private float maxArrivalForce = 10f;
        
        public override Vector3 CalculateForce()
        {
            if (troopBase.TargetTransform == null)
                return Vector3.zero;
                
            Vector3 toTarget = troopBase.TargetTransform.position - transform.position;
            float distance = toTarget.magnitude;
            
            if (distance < arrivalRadius)
                return -troopBase.CurrentVelocity; // Stop if we're within arrival radius
                
            Vector3 desiredVelocity;
            
            // Calculate desired speed based on distance to target
            float desiredSpeed;
            if (distance > slowingRadius)
            {
                desiredSpeed = troopBase.MoveSpeed;
            }
            else
            {
                desiredSpeed = troopBase.MoveSpeed * (distance / slowingRadius);
            }
            
            // Calculate desired velocity
            desiredVelocity = toTarget.normalized * desiredSpeed;
            
            // Steering = desired velocity - current velocity
            Vector3 steeringForce = desiredVelocity - troopBase.CurrentVelocity;
            
            // Limit steering force
            if (steeringForce.magnitude > maxArrivalForce)
            {
                steeringForce = steeringForce.normalized * maxArrivalForce;
            }
            
            return steeringForce;
        }
        
        public override void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, slowingRadius);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, arrivalRadius);
        }
        
        public override void InitializeFromConfig(BehaviorConfig behaviorConfig)
        {
            base.InitializeFromConfig(behaviorConfig);
            arrivalRadius = behaviorConfig.ArrivalRadius;
            slowingRadius = behaviorConfig.SlowingRadius;
        }
    }