using System.Collections.Generic;
using UnityEngine;

public class MapNode : MonoBehaviour
{
    public List<MapNode> connectedNodes = new List<MapNode>();
    public bool isWalkable = true;
    
    private void OnDrawGizmos()
    {
        // Draw node
        Gizmos.color = isWalkable ? Color.green : Color.red;
        Gizmos.DrawSphere(transform.position, 0.5f);
        
        // Draw connections
        Gizmos.color = Color.white;
        foreach (MapNode node in connectedNodes)
        {
            if (node != null)
            {
                Gizmos.DrawLine(transform.position, node.transform.position);
            }
        }
    }
}
