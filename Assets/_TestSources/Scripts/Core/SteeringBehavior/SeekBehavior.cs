// Seek Behavior

using UnityEngine;

public class SeekBehavior : SteeringBehavior
{
    public Transform target;
    public Vector3 targetPosition;
    public bool useTransform = true;
    
    public override Vector3 CalculateForce()
    {
        Vector3 desiredPosition = useTransform && target 
            ? target.position 
            : targetPosition;
        
        // Calculate desired velocity
        Vector3 desiredVelocity = (desiredPosition - transform.position).normalized * troop.maxSpeed;
        
        // Calculate steering force (desired velocity - current velocity)
        return desiredVelocity - troop.Velocity;
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        useTransform = true;
    }
    
    public void SetTarget(Vector3 position)
    {
        targetPosition = position;
        useTransform = false;
    }
}