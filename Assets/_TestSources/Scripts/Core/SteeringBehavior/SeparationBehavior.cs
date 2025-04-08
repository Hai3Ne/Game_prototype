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
        
        Collider[] neighbors = Physics.OverlapSphere(transform.position, separationRadius, troopLayer);
        
        foreach (Collider neighbor in neighbors)
        {
            if (neighbor.gameObject == gameObject) continue;
            Vector3 direction = transform.position - neighbor.transform.position;
            float distance = direction.magnitude;
            if (distance > 0)
            {
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