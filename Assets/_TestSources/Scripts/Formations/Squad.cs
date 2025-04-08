using System.Collections.Generic;
using UnityEngine;

public enum FormationType
{
    Line,
    Column,
    Square
}
public class Squad : MonoBehaviour
{
    [Header("Squad Settings")]
    public string squadName = "Squad";
    public int maxTroops = 12;
    
    [Header("Formation")]
    public FormationType currentFormation = FormationType.Square;
    public float unitSpacing = 2f;
    
    // References
    private List<TroopBase> troops = new List<TroopBase>();
    
    public bool AddTroop(TroopBase troop)
    {
        if (troops.Count >= maxTroops)
            return false;
        
        if (!troops.Contains(troop))
        {
            troops.Add(troop);
            troop.AssignToSquad(this, troops.Count - 1);
            UpdateFormation();
            return true;
        }
        
        return false;
    }
    
    public bool RemoveTroop(TroopBase troop)
    {
        if (troops.Contains(troop))
        {
            int index = troops.IndexOf(troop);
            troops.Remove(troop);
            troop.AssignToSquad(null, -1);
            
            // Update indices for remaining troops
            for (int i = index; i < troops.Count; i++)
            {
                troops[i].formationIndex = i;
            }
            
            UpdateFormation();
            return true;
        }
        
        return false;
    }
    
    public void ChangeFormation(FormationType newFormation)
    {
        currentFormation = newFormation;
        UpdateFormation();
    }
    
    public void MoveSquad(Vector3 targetPosition)
    {
        transform.position = targetPosition;
        UpdateFormation();
    }
    
    private void UpdateFormation()
    {
        List<Vector3> formationPositions = GetFormationPositions(
            currentFormation, 
            troops.Count, 
            unitSpacing
        );
        
        for (int i = 0; i < troops.Count; i++)
        {
            if (i < formationPositions.Count)
            {
                // Get world position
                Vector3 worldPos = transform.TransformPoint(formationPositions[i]);
                
                // Tell troop to move to position
                troops[i].MoveToFormationPosition(worldPos);
            }
        }
    }
    
    public List<TroopBase> GetTroops()
    {
        return new List<TroopBase>(troops);
    }
    
    // Formation position calculations
    private List<Vector3> GetFormationPositions(FormationType formationType, int troopCount, float spacing)
    {
        switch (formationType)
        {
            case FormationType.Line:
                return GetLineFormation(troopCount, spacing);
            case FormationType.Column:
                return GetColumnFormation(troopCount, spacing);
            case FormationType.Square:
                return GetSquareFormation(troopCount, spacing);
            default:
                return GetSquareFormation(troopCount, spacing);
        }
    }
    
    private List<Vector3> GetLineFormation(int troopCount, float spacing)
    {
        List<Vector3> positions = new List<Vector3>();
        float totalWidth = (troopCount - 1) * spacing;
        float startX = -totalWidth / 2f;
        
        for (int i = 0; i < troopCount; i++)
        {
            float x = startX + (i * spacing);
            positions.Add(new Vector3(x, 0, 0));
        }
        
        return positions;
    }
    
    private List<Vector3> GetColumnFormation(int troopCount, float spacing)
    {
        List<Vector3> positions = new List<Vector3>();
        float totalDepth = (troopCount - 1) * spacing;
        float startZ = -totalDepth / 2f;
        
        for (int i = 0; i < troopCount; i++)
        {
            float z = startZ + (i * spacing);
            positions.Add(new Vector3(0, 0, z));
        }
        
        return positions;
    }
    
    private List<Vector3> GetSquareFormation(int troopCount, float spacing)
    {
        List<Vector3> positions = new List<Vector3>();
        
        int columns = Mathf.CeilToInt(Mathf.Sqrt(troopCount));
        int rows = Mathf.CeilToInt((float)troopCount / columns);
        
        float totalWidth = (columns - 1) * spacing;
        float totalDepth = (rows - 1) * spacing;
        
        float startX = -totalWidth / 2f;
        float startZ = -totalDepth / 2f;
        
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                int index = row * columns + col;
                
                if (index < troopCount)
                {
                    float x = startX + (col * spacing);
                    float z = startZ + (row * spacing);
                    
                    positions.Add(new Vector3(x, 0, z));
                }
            }
        }
        
        return positions;
    }
}