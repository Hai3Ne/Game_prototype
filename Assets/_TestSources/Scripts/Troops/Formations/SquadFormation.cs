using System.Collections;
using System.Collections.Generic;
using IslandDefense.Troops;
using UnityEngine;

/// <summary>
/// Types of formations
/// </summary>
public enum FormationType
{
    Square, // 3x3 grid
    Line,   // Horizontal line
    Column, // Vertical line
    V       // V-shaped
}
    
/// <summary>
/// Manages a squad of troops in formation
/// </summary>
public class SquadFormation : MonoBehaviour
{
    [Header("Formation Settings")]
    [SerializeField] private FormationType formationType = FormationType.Square;
    [SerializeField] private float spacingDistance = 2.0f;
    [SerializeField] private Transform formationCenter;
        
    [Header("Squad")]
    [SerializeField] private List<TroopBase> troops = new List<TroopBase>();
    [SerializeField] private int maxTroopCount = 9;
        
    [Header("Movement")]
    [SerializeField] private float formationReorganizeTime = 1.0f;
    [SerializeField] private float movementTolerance = 0.5f;
    [SerializeField] private float rotationSpeed = 5.0f;
        
    // Internal state
    private Vector3[,] formationPositions = new Vector3[3, 3];
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isMoving = false;
        
    // Properties
    public FormationType Formation { get => formationType; set => SetFormationType(value); }
    public int CurrentTroopCount => troops.Count;
        
    private void Awake()
    {
        if (formationCenter == null)
        {
            formationCenter = transform;
        }
            
        // Initialize formation positions
        UpdateFormationPositions();
    }
        
    private void Update()
    {
        if (isMoving)
        {
            // Move formation center towards target
            formationCenter.position = Vector3.MoveTowards(
                formationCenter.position, 
                targetPosition, 
                Time.deltaTime * 5f
            );
                
            // Rotate formation
            formationCenter.rotation = Quaternion.Slerp(
                formationCenter.rotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );
                
            // Check if we've reached the target
            if (Vector3.Distance(formationCenter.position, targetPosition) < movementTolerance)
            {
                isMoving = false;
                    
                // Reorganize formation once we've reached the destination
                StartCoroutine(ReorganizeFormation());
            }
        }
    }
        
    /// <summary>
    /// Set the formation type and update positions
    /// </summary>
    public void SetFormationType(FormationType type)
    {
        formationType = type;
        UpdateFormationPositions();
            
        // If we already have troops, update their positions
        if (troops.Count > 0)
        {
            StartCoroutine(ReorganizeFormation());
        }
    }
        
    /// <summary>
    /// Calculate formation positions based on type
    /// </summary>
    private void UpdateFormationPositions()
    {
        switch (formationType)
        {
            case FormationType.Square:
                // 3x3 grid
                for (int x = 0; x < 3; x++)
                {
                    for (int z = 0; z < 3; z++)
                    {
                        float xPos = (x - 1) * spacingDistance;
                        float zPos = (z - 1) * spacingDistance;
                        formationPositions[x, z] = new Vector3(xPos, 0, zPos);
                    }
                }
                break;
                    
            case FormationType.Line:
                // Horizontal line
                for (int x = 0; x < 3; x++)
                {
                    for (int z = 0; z < 3; z++)
                    {
                        float xPos = (x * 3 + z - 4) * spacingDistance * 0.75f;
                        float zPos = 0;
                        formationPositions[x, z] = new Vector3(xPos, 0, zPos);
                    }
                }
                break;
                    
            case FormationType.Column:
                // Vertical line
                for (int x = 0; x < 3; x++)
                {
                    for (int z = 0; z < 3; z++)
                    {
                        float xPos = 0;
                        float zPos = (x * 3 + z - 4) * spacingDistance * 0.75f;
                        formationPositions[x, z] = new Vector3(xPos, 0, zPos);
                    }
                }
                break;
                    
            case FormationType.V:
                // V-shaped formation
                for (int x = 0; x < 3; x++)
                {
                    for (int z = 0; z < 3; z++)
                    {
                        int index = x * 3 + z;
                        float xPos = 0;
                        float zPos = 0;
                            
                        if (index == 0) { xPos = 0; zPos = 2 * spacingDistance; } // Center front
                        else if (index == 1) { xPos = -spacingDistance; zPos = spacingDistance; } // Left mid
                        else if (index == 2) { xPos = spacingDistance; zPos = spacingDistance; } // Right mid
                        else if (index == 3) { xPos = -2 * spacingDistance; zPos = 0; } // Left rear
                        else if (index == 4) { xPos = 0; zPos = 0; } // Center rear
                        else if (index == 5) { xPos = 2 * spacingDistance; zPos = 0; } // Right rear
                        else if (index == 6) { xPos = -3 * spacingDistance; zPos = -spacingDistance; } // Far left
                        else if (index == 7) { xPos = -spacingDistance; zPos = -spacingDistance; } // Mid left rear
                        else if (index == 8) { xPos = spacingDistance; zPos = -spacingDistance; } // Mid right rear
                            
                        formationPositions[x, z] = new Vector3(xPos, 0, zPos);
                    }
                }
                break;
        }
    }
        
    /// <summary>
    /// Add a troop to the squad
    /// </summary>
    public bool AddTroop(TroopBase troop)
    {
        if (troops.Count >= maxTroopCount)
        {
            Debug.LogWarning("Squad is full, cannot add more troops");
            return false;
        }
            
        troops.Add(troop);
        troop.SquadIndex = troops.Count - 1;
        troop.Squad = this;
            
        // Update troop position
        AssignTroopPosition(troop);
            
        return true;
    }
        
    /// <summary>
    /// Remove a troop from the squad
    /// </summary>
    public bool RemoveTroop(TroopBase troop)
    {
        if (!troops.Contains(troop))
        {
            return false;
        }
            
        int removedIndex = troop.SquadIndex;
        troops.Remove(troop);
            
        // Update indices of remaining troops
        for (int i = removedIndex; i < troops.Count; i++)
        {
            troops[i].SquadIndex = i;
        }
            
        return true;
    }
        
    /// <summary>
    /// Assign a troop to its correct position in formation
    /// </summary>
    private void AssignTroopPosition(TroopBase troop)
    {
        int index = troop.SquadIndex;
        int x = index / 3;
        int z = index % 3;
            
        // Calculate local position in formation
        Vector3 localPosition = formationPositions[x, z];
            
        // Convert to world position
        Vector3 worldPosition = formationCenter.TransformPoint(localPosition);
            
        // Move troop to position
        TroopController controller = troop.GetComponent<TroopController>();
        if (controller != null)
        {
            controller.MoveToPosition(worldPosition);
        }
    }
        
    /// <summary>
    /// Move the entire formation to a new position
    /// </summary>
    public void MoveToPosition(Vector3 position, Quaternion rotation)
    {
        targetPosition = position;
        targetRotation = rotation;
        isMoving = true;
            
        // Show cautious behavior if moving towards enemies
        Collider[] enemies = Physics.OverlapSphere(position, 10f, LayerMask.GetMask("Enemy"));
        bool enemiesNearby = enemies.Length > 0;
            
        // Play appropriate animations for all troops
        foreach (var troop in troops)
        {
            TroopView view = troop.GetComponent<TroopView>();
            if (view != null)
            {
                if (enemiesNearby)
                {
                    view.PlayCautiousMoveAnimation();
                }
                else
                {
                    view.PlayMoveAnimation();
                }
            }
        }
    }
        
    /// <summary>
    /// Reorganize troops into formation
    /// </summary>
    private IEnumerator ReorganizeFormation()
    {
        // Wait a moment before reorganizing
        yield return new WaitForSeconds(formationReorganizeTime);
            
        // Update formation positions
        UpdateFormationPositions();
            
        // Assign each troop to its position
        for (int i = 0; i < troops.Count; i++)
        {
            TroopBase troop = troops[i];
            AssignTroopPosition(troop);
                
            // Slight delay between moving each troop to make it look natural
            yield return new WaitForSeconds(0.1f);
        }
    }
        
    /// <summary>
    /// Draw gizmos to visualize formation
    /// </summary>
    private void OnDrawGizmos()
    {
        if (formationCenter == null)
            return;
                
        Gizmos.color = Color.blue;
            
        // Draw formation center
        Gizmos.DrawSphere(formationCenter.position, 0.5f);
            
        // Draw formation positions
        UpdateFormationPositions();
            
        for (int x = 0; x < 3; x++)
        {
            for (int z = 0; z < 3; z++)
            {
                Vector3 worldPos = formationCenter.TransformPoint(formationPositions[x, z]);
                Gizmos.DrawWireSphere(worldPos, 0.3f);
                    
                // Draw line to center
                Gizmos.DrawLine(formationCenter.position, worldPos);
            }
        }
            
        // Draw direction indicator
        Gizmos.color = Color.green;
        Gizmos.DrawRay(formationCenter.position, formationCenter.forward * 2f);
    }
}