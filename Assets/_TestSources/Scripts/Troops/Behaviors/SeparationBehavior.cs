using IslandDefense.Troops;
using UnityEngine;

/// <summary>
    /// Implementation of Separation behavior - avoids crowding neighbors
    /// </summary>
    public class SeparationBehavior : SteeringBehavior
    {
        [Header("Separation Settings")]
        [SerializeField] private float separationRadius = 2f;
        [SerializeField] private float maxSeparationForce = 15f;
        [SerializeField] private LayerMask troopLayer;
        
        public override Vector3 CalculateForce()
        {
            Vector3 steeringForce = Vector3.zero;
            int neighborCount = 0;
            
            // Find all nearby troops
            Collider[] neighbors = Physics.OverlapSphere(transform.position, separationRadius, troopLayer);
            
            foreach (var neighbor in neighbors)
            {
                // Skip self
                if (neighbor.transform == transform)
                    continue;
                    
                // Calculate vector away from neighbor
                Vector3 toNeighbor = transform.position - neighbor.transform.position;
                float distance = toNeighbor.magnitude;
                
                // The closer the neighbor, the stronger the repulsion
                Vector3 repulsionForce = toNeighbor.normalized / distance;
                steeringForce += repulsionForce;
                neighborCount++;
            }
            
            // Average the forces
            if (neighborCount > 0)
            {
                steeringForce /= neighborCount;
                steeringForce = steeringForce.normalized * troopBase.MoveSpeed;
                
                // Calculate steering force
                steeringForce = steeringForce - troopBase.CurrentVelocity;
                
                // Limit force
                if (steeringForce.magnitude > maxSeparationForce)
                {
                    steeringForce = steeringForce.normalized * maxSeparationForce;
                }
            }
            
            return steeringForce;
        }
        
        public override void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f); // Orange
            Gizmos.DrawWireSphere(transform.position, separationRadius);
        }
        
        public override void InitializeFromConfig(BehaviorConfig behaviorConfig)
        {
            base.InitializeFromConfig(behaviorConfig);
            separationRadius = behaviorConfig.SeparationRadius;
        }
    }