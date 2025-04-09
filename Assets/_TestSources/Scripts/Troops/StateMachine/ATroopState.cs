using IslandDefense.Troops;

/// <summary>
/// Troop state types
/// </summary>
public enum TroopStateType
{
    Idle,
    Move,
    Attack,
    Defend,
    Flee,
    Dead
}
    
/// <summary>
/// Base class for troop states
/// </summary>
public abstract class ATroopState
{
    protected TroopController controller;
    protected TroopBase troopBase;
    protected TroopView view;
        
    public ATroopState(TroopController controller, TroopBase troopBase, TroopView view)
    {
        this.controller = controller;
        this.troopBase = troopBase;
        this.view = view;
    }
        
    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}