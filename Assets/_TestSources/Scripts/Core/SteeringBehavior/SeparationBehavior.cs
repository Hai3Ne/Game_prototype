// Separation Behavior
using UnityEngine;

public class SeparationBehavior : SteeringBehavior
{
    public float separationRadius = 2f;
    public LayerMask troopLayer;
    
    public override Vector3 CalculateForce()
    {
        Vector3 separationForce = Vector3.zero;
        int neighborCount = 0;
        
        // Find all nearby troops
        Collider[] neighbors = Physics.OverlapSphere(transform.position, separationRadius, troopLayer);
        
        foreach (Collider neighbor in neighbors)
        {
            // Skip self
            if (neighbor.gameObject == gameObject) continue;
            
            // Calculate direction away from neighbor
            Vector3 direction = transform.position - neighbor.transform.position;
            float distance = direction.magnitude;
            
            if (distance > 0)
            {
                // Scale by inverse square of distance
                separationForce += direction.normalized / (distance * distance);
                neighborCount++;
            }
        }
        
        // Average
        if (neighborCount > 0)
        {
            separationForce /= neighborCount;
            separationForce = separationForce.normalized * troop.maxSpeed;
        }
        
        return separationForce;
    }
}