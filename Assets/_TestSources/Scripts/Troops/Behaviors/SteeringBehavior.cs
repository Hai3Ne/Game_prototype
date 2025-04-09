using IslandDefense.Troops;
using UnityEngine;

/// <summary>
/// Base class for all steering behaviors
/// </summary>
public abstract class SteeringBehavior : MonoBehaviour
{
    [Header("Behavior Configuration")]
    [SerializeField] protected float weight = 1.0f;
    [SerializeField] [Range(0f, 1f)] protected float probability = 1.0f;
        
    protected TroopBase troopBase;
    protected TroopController troopController;
    protected BehaviorConfig config;
        
    // Properties
    public float Weight { get => weight; set => weight = value; }
    public float Probability { get => probability; set => probability = Mathf.Clamp01(value); }
        
    protected virtual void Awake()
    {
        troopBase = GetComponentInParent<TroopBase>();
        troopController = GetComponentInParent<TroopController>();
    }
        
    /// <summary>
    /// Initialize behavior from configuration
    /// </summary>
    public virtual void InitializeFromConfig(BehaviorConfig behaviorConfig)
    {
        config = behaviorConfig;
        weight = behaviorConfig.Weight;
        probability = behaviorConfig.Probability;
    }
        
    /// <summary>
    /// Calculate the steering force for this behavior
    /// </summary>
    public abstract Vector3 CalculateForce();
        
    /// <summary>
    /// Determine if this behavior should be applied based on probability
    /// </summary>
    public virtual bool ShouldApply()
    {
        return Random.value <= probability;
    }
        
    /// <summary>
    /// Draw gizmos to visualize the behavior
    /// </summary>
    public virtual void OnDrawGizmos()
    {
        // Override in derived classes for visualization
    }
}