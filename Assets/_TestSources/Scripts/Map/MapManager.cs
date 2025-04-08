using System.Collections.Generic;
using UnityEngine;
public class MapManager : MonoBehaviour
{
    public List<MapNode> allNodes = new List<MapNode>();
    
    private void Start()
    {
        // Find all nodes in the scene if not set manually
        if (allNodes.Count == 0)
        {
            allNodes.AddRange(FindObjectsOfType<MapNode>());
        }
    }
}