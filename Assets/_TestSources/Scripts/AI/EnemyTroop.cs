using UnityEngine;

public class EnemyTroop : TroopBase
{
    [Header("Enemy Settings")]
    public float detectionRadius = 10f;
    public LayerMask playerLayer;
    
    protected override void Start()
    {
        base.Start();
        
        // Add enemy-specific behaviors
        SeekBehavior seekBehavior = gameObject.AddComponent<SeekBehavior>();
        seekBehavior.weight = 1.0f;
        steeringBehaviors.Add(seekBehavior);
    }
    
    protected override void Update()
    {
        // Find nearest player troop
        TroopBase nearestTroop = FindNearestPlayerTroop();
        
        // If found, seek to it
        if (nearestTroop != null)
        {
            SeekBehavior seekBehavior = GetComponent<SeekBehavior>();
            if (seekBehavior != null)
            {
                seekBehavior.SetTarget(nearestTroop.transform);
            }
        }
        
        base.Update();
    }
    
    private TroopBase FindNearestPlayerTroop()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);
        
        if (colliders.Length == 0)
            return null;
        
        TroopBase nearest = null;
        float minDistance = float.MaxValue;
        
        foreach (Collider col in colliders)
        {
            TroopBase troop = col.GetComponent<TroopBase>();
            
            if (troop != null && !(troop is EnemyTroop))
            {
                float distance = Vector3.Distance(transform.position, troop.transform.position);
                
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = troop;
                }
            }
        }
        
        return nearest;
    }
}