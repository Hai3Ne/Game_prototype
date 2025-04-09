using System.Collections.Generic;
using IslandDefense.Troops;
using UnityEngine;

/// <summary>
/// Base model class for all troops and enemies in the game.
/// Follows MVC pattern as the Model component.
/// </summary>
public class TroopBase : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected TroopController controller;
    [SerializeField] protected TroopView view;

    [Header("Stats")]
    [SerializeField] protected float health;
    [SerializeField] protected float maxHealth;
    [SerializeField] protected float attackPower;
    [SerializeField] protected float attackRange;
    [SerializeField] protected float attackSpeed;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float rotationSpeed;

    [Header("Configuration")]
    [SerializeField] protected TroopConfigSO troopConfig;
        
    // Internal properties
    protected List<SteeringBehavior> activeBehaviors = new List<SteeringBehavior>();
    protected Vector3 currentVelocity;
    protected Vector3 currentSteeringForce;
    protected Transform targetTransform;
    protected TroopBase targetTroop;
    protected int squadIndex; // Position in the squad (0-8 for a 3x3 formation)
    protected SquadFormation squad;

    // Properties
    public float Health { get => health; set => health = Mathf.Clamp(value, 0, maxHealth); }
    public float MaxHealth { get => maxHealth; }
    public float AttackPower { get => attackPower; }
    public float AttackRange { get => attackRange; }
    public float AttackSpeed { get => attackSpeed; }
    public float MoveSpeed { get => moveSpeed; }
    public float RotationSpeed { get => rotationSpeed; }
    public Vector3 CurrentVelocity { get => currentVelocity; set => currentVelocity = value; }
    public Vector3 CurrentSteeringForce { get => currentSteeringForce; }
    public Transform TargetTransform { get => targetTransform; set => targetTransform = value; }
    public TroopBase TargetTroop { get => targetTroop; set => targetTroop = value; }
    public int SquadIndex { get => squadIndex; set => squadIndex = value; }
    public SquadFormation Squad { get => squad; set => squad = value; }
    public TroopConfigSO TroopConfig { get => troopConfig; }

    protected virtual void Awake()
    {
        // Ensure references are set
        if (controller == null)
            controller = GetComponent<TroopController>();
            
        if (view == null)
            view = GetComponent<TroopView>();
    }

    protected virtual void Start()
    {
        InitializeFromConfig();
    }

    /// <summary>
    /// Initialize troop data from the attached Scriptable Object
    /// </summary>
    public virtual void InitializeFromConfig()
    {
        if (troopConfig == null)
        {
            Debug.LogError("TroopConfig is not assigned to " + gameObject.name);
            return;
        }

        maxHealth = troopConfig.MaxHealth;
        health = maxHealth;
        attackPower = troopConfig.AttackPower;
        attackRange = troopConfig.AttackRange;
        attackSpeed = troopConfig.AttackSpeed;
        moveSpeed = troopConfig.MoveSpeed;
        rotationSpeed = troopConfig.RotationSpeed;

        // Initialize behaviors from config
        InitializeBehaviors();
    }

    /// <summary>
    /// Initialize steering behaviors from config
    /// </summary>
    protected virtual void InitializeBehaviors()
    {
        // Clear any existing behaviors
        activeBehaviors.Clear();

        // Add behaviors from config
        foreach (var behaviorConfig in troopConfig.Behaviors)
        {
            SteeringBehavior behavior = controller.AddBehavior(behaviorConfig.BehaviorType);
                
            if (behavior != null)
            {
                behavior.Weight = behaviorConfig.Weight;
                behavior.Probability = behaviorConfig.Probability;
                    
                // Initialize additional behavior-specific properties if needed
                behavior.InitializeFromConfig(behaviorConfig);
                    
                activeBehaviors.Add(behavior);
            }
        }
    }

    /// <summary>
    /// Apply damage to the troop
    /// </summary>
    public virtual void TakeDamage(float damage)
    {
        Health -= damage;
            
        // Notify view to update health bar
        view.UpdateHealthBar(Health, MaxHealth);
            
        // Play hit animation/effect
        view.PlayHitEffect();
            
        if (Health <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Handle troop death
    /// </summary>
    protected virtual void Die()
    {
        // Notify squad about death
        if (squad != null)
        {
            squad.RemoveTroop(this);
        }
            
        // Play death animation
        view.PlayDeathAnimation(() => {
            // Callback after death animation
            Destroy(gameObject);
        });
    }

    /// <summary>
    /// Calculate the combined steering force based on active behaviors
    /// </summary>
    public virtual Vector3 CalculateSteeringForce()
    {
        Vector3 totalForce = Vector3.zero;
            
        // Calculate forces from all active behaviors
        foreach (var behavior in activeBehaviors)
        {
            // Check if behavior should be applied based on probability
            if (behavior.ShouldApply())
            {
                Vector3 force = behavior.CalculateForce() * behavior.Weight;
                totalForce += force;
            }
        }
            
        // Limit the total force to the max force
        if (totalForce.magnitude > troopConfig.MaxSteeringForce)
        {
            totalForce = totalForce.normalized * troopConfig.MaxSteeringForce;
        }
            
        currentSteeringForce = totalForce;
        return totalForce;
    }

    /// <summary>
    /// Update the troop's position based on the current velocity
    /// </summary>
    public virtual void UpdatePosition(Vector3 steeringForce, float deltaTime)
    {
        // Update velocity based on steering force
        currentVelocity += steeringForce * deltaTime;
            
        // Limit velocity to max speed
        if (currentVelocity.magnitude > moveSpeed)
        {
            currentVelocity = currentVelocity.normalized * moveSpeed;
        }
            
        // Update position
        transform.position += currentVelocity * deltaTime;
            
        // Update rotation to face movement direction
        if (currentVelocity.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(currentVelocity);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                targetRotation, 
                rotationSpeed * deltaTime
            );
        }
    }
}