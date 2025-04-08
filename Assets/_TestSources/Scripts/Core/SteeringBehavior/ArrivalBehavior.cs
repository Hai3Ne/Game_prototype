// Arrival Behavior

using UnityEngine;

public class ArrivalBehavior : SteeringBehavior
{
    public Transform target;
    public Vector3 targetPosition;
    public bool useTransform = true;
    public float slowingDistance = 5f;
    
    public override Vector3 CalculateForce()
    {
        Vector3 desiredPosition = useTransform && target != null 
            ? target.position 
            : targetPosition;
        
        // Calculate direction to target
        Vector3 toTarget = desiredPosition - transform.position;
        float distance = toTarget.magnitude;
        
        // Calculate desired velocity
        Vector3 desiredVelocity;
        
        // If we're within slowing distance, scale the speed
        if (distance < slowingDistance)
        {
            desiredVelocity = toTarget.normalized * (troop.maxSpeed * (distance / slowingDistance));
        }
        else
        {
            desiredVelocity = toTarget.normalized * troop.maxSpeed;
        }
        
        // Calculate steering force
        return desiredVelocity - troop.Velocity;
    }
}