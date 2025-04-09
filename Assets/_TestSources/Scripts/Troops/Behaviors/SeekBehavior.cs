using IslandDefense.Troops;
using UnityEngine;

/// <summary>
/// Implementation of Seek behavior - moves towards a target
/// </summary>
public class SeekBehavior : SteeringBehavior
{
    [Header("Seek Settings")]
    [SerializeField] private float maxSeekForce = 10f;

    public override Vector3 CalculateForce()
    {
        if (troopBase.TargetTransform == null)
            return Vector3.zero;

        // Calculate desired velocity towards target
        Vector3 desiredVelocity = (troopBase.TargetTransform.position - transform.position).normalized 
                                  * troopBase.MoveSpeed;
            
        // Steering = desired velocity - current velocity
        Vector3 steeringForce = desiredVelocity - troopBase.CurrentVelocity;
            
        // Limit steering force
        if (steeringForce.magnitude > maxSeekForce)
        {
            steeringForce = steeringForce.normalized * maxSeekForce;
        }
            
        return steeringForce;
    }
        
    public override void OnDrawGizmos()
    {
        if (troopBase != null && troopBase.TargetTransform != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, troopBase.TargetTransform.position);
        }
    }
}