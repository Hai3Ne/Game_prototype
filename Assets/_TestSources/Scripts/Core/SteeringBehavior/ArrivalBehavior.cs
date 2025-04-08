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
        Vector3 desiredPosition = useTransform && target 
            ? target.position 
            : targetPosition;
        
        Vector3 toTarget = desiredPosition - transform.position;
        float distance = toTarget.magnitude;
        
        Vector3 desiredVelocity;
        if (distance < slowingDistance)
        {
            desiredVelocity = toTarget.normalized * (troop.maxSpeed * (distance / slowingDistance));
        }
        else
        {
            desiredVelocity = toTarget.normalized * troop.maxSpeed;
        }
        
        return desiredVelocity - troop.Velocity;
    }
}