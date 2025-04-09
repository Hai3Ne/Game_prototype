using IslandDefense.Troops;
using UnityEngine;

/// <summary>
/// Flee state - troop is fleeing from danger
/// </summary>
public class TroopFleeState : ATroopState
{
    private float fleeTime = 0f;
    private float maxFleeTime = 3f;
        
    public TroopFleeState(TroopController controller, TroopBase troopBase, TroopView view) : base(controller, troopBase, view)
    {
    }
        
    public override void Enter()
    {
        view.PlayFleeAnimation(null);
        fleeTime = 0f;
    }
        
    public override void Update()
    {
        // Update flee time
        fleeTime += Time.deltaTime;
            
        // Return to idle after flee time
        if (fleeTime >= maxFleeTime)
        {
            controller.OnLostTarget();
        }
    }
        
    public override void Exit()
    {
        // Nothing specific to do here
    }
}