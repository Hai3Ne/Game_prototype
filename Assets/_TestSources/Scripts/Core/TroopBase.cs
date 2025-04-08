using UnityEngine;
using System.Collections.Generic;

public class TroopBase : MonoBehaviour
{
    [Header("Movement")]
    public float maxSpeed = 5f;
    public float maxForce = 10f;
    public float mass = 1f;
    
    [Header("References")]
    protected Vector3 velocity = Vector3.zero;
    protected Vector3 acceleration = Vector3.zero;
    
    // Steering behaviors
    public List<SteeringBehavior> steeringBehaviors = new List<SteeringBehavior>();
    public Vector3 Velocity => velocity;
    // Squad management
    public Squad squad;
    public int formationIndex = -1;
    public Vector3 formationOffset;
    
    protected virtual void Start()
    {
        InitializeSteeringBehaviors();
    }
    
    protected virtual void Update()
    {
        // Calculate steering forces
        Vector3 steeringForce = CalculateSteering();
        
        // Apply force
        ApplyForce(steeringForce);
        
        // Update position
        UpdateMovement();
    }
    
    protected virtual void InitializeSteeringBehaviors()
    {
        // Find and initialize any attached steering behaviors
        steeringBehaviors.Clear();
        steeringBehaviors.AddRange(GetComponents<SteeringBehavior>());
    }
    
    protected virtual Vector3 CalculateSteering()
    {
        Vector3 totalForce = Vector3.zero;
        
        foreach (SteeringBehavior behavior in steeringBehaviors)
        {
            if (!behavior.isActiveAndEnabled) continue;
            Vector3 force = behavior.CalculateForce();
            totalForce += force * behavior.weight;
        }
        
        // Limit to max force
        if (totalForce.magnitude > maxForce)
        {
            totalForce = totalForce.normalized * maxForce;
        }
        
        return totalForce;
    }
    
    protected virtual void ApplyForce(Vector3 force)
    {
        // F = ma, a = F/m
        acceleration += force / mass;
    }
    
    protected virtual void UpdateMovement()
    {
        // Update velocity
        velocity += acceleration * Time.deltaTime;
        
        // Limit velocity to max speed
        if (velocity.magnitude > maxSpeed)
        {
            velocity = velocity.normalized * maxSpeed;
        }
        
        // Update position
        if (velocity.magnitude > 0.01f)
        {
            transform.position += velocity * Time.deltaTime;
            
            // Rotate to face movement direction
            transform.forward = velocity.normalized;
        }
        
        // Reset acceleration
        acceleration = Vector3.zero;
    }
    
    public virtual void AssignToSquad(Squad newSquad, int formationPos = -1)
    {
        squad = newSquad;
        formationIndex = formationPos;
    }
    
    public virtual void MoveToFormationPosition(Vector3 targetPosition)
    {
        // Find or add Seek behavior
        SeekBehavior seekBehavior = GetComponent<SeekBehavior>();
        if (!seekBehavior)
        {
            seekBehavior = gameObject.AddComponent<SeekBehavior>();
            steeringBehaviors.Add(seekBehavior);
        }
        
        // Set target
        seekBehavior.SetTarget(targetPosition);
    }
}