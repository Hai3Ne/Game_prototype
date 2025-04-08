using UnityEngine;

public abstract class SteeringBehavior : MonoBehaviour
{
    [Header("Behavior Settings")]
    public bool isEnabled = true;
    public float weight = 1f;
    
    protected TroopBase troop;
    
    protected virtual void Awake()
    {
        troop = GetComponent<TroopBase>();
    }
    
    public abstract Vector3 CalculateForce();
}