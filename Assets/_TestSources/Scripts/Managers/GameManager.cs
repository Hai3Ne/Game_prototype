using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject troopPrefab;
    public GameObject enemyPrefab;
    public GameObject squadPrefab;
    
    [Header("Game Settings")]
    public int squadSize = 9;
    
    private Squad playerSquad;
    private Camera mainCamera;
    
    private void Start()
    {
        mainCamera = Camera.main;
        InitializeGame();
    }
    
    private void InitializeGame()
    {
        // Create player squad
        GameObject squadObj = Instantiate(squadPrefab, Vector3.zero, Quaternion.identity);
        playerSquad = squadObj.GetComponent<Squad>();
        
        // Create troops and add to squad
        for (int i = 0; i < squadSize; i++)
        {
            GameObject troopObj = Instantiate(troopPrefab, Vector3.zero, Quaternion.identity);
            TroopBase troop = troopObj.GetComponent<TroopBase>();
            
            // Add steering behaviors
            troopObj.AddComponent<SeekBehavior>();
            troopObj.AddComponent<SeparationBehavior>();
            troopObj.AddComponent<ArrivalBehavior>();
            
            // Add to squad
            if (troop != null)
            {
                playerSquad.AddTroop(troop);
            }
        }
    }
    
    private void Update()
    {
        // Handle player input
        HandleInput();
    }
    
    private void HandleInput()
    {
        // Right-click to move squad
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                // Move squad to clicked point
                playerSquad.MoveSquad(hit.point);
            }
        }
        
        // Number keys to change formation
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            playerSquad.ChangeFormation(FormationType.Line);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            playerSquad.ChangeFormation(FormationType.Column);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            playerSquad.ChangeFormation(FormationType.Square);
        }
    }
}