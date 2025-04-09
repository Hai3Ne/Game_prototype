using System;
using System.Collections.Generic;
using System.Linq;
using IslandDefense.Core;
using UnityEngine;

/// <summary>
/// Quản lý input từ người chơi
/// </summary>
public class InputManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameManager gameManager;
        
    [Header("Input Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask troopLayer;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float dragThreshold = 5f;
        
    [Header("Camera Control")]
    [SerializeField] private float cameraSpeed = 20f;
    [SerializeField] private float cameraRotateSpeed = 100f;
    [SerializeField] private float cameraPanBorderThickness = 10f;
    [SerializeField] private float cameraZoomSpeed = 15f;
    [SerializeField] private float minZoomDistance = 5f;
    [SerializeField] private float maxZoomDistance = 50f;
        
    // State variables
    private Vector3 dragStartPosition;
    private bool isDragging = false;
    private bool isBoxSelecting = false;
    private Rect selectionRect;
        
    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
                
        if (gameManager == null)
            gameManager = GetComponent<GameManager>();
    }
        
    private void Update()
    {
        // Camera Controls
        HandleCameraMovement();
            
        // Mouse Controls
        HandleMouseInput();
            
        // Keyboard Controls
        HandleKeyboardInput();
    }
        
    private void OnGUI()
    {
        // Draw selection box if active
        if (isBoxSelecting)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, new Color(0.5f, 0.5f, 0.9f, 0.3f));
            texture.Apply();
                
            GUI.skin.box.normal.background = texture;
            GUI.Box(selectionRect, "");
                
            // Draw border
            Texture2D borderTexture = new Texture2D(1, 1);
            borderTexture.SetPixel(0, 0, new Color(0.5f, 0.5f, 0.9f, 0.8f));
            borderTexture.Apply();
                
            GUI.skin.box.normal.background = borderTexture;
                
            // Top border
            GUI.Box(new Rect(selectionRect.x, selectionRect.y, selectionRect.width, 2), "");
            // Bottom border
            GUI.Box(new Rect(selectionRect.x, selectionRect.y + selectionRect.height - 2, selectionRect.width, 2), "");
            // Left border
            GUI.Box(new Rect(selectionRect.x, selectionRect.y, 2, selectionRect.height), "");
            // Right border
            GUI.Box(new Rect(selectionRect.x + selectionRect.width - 2, selectionRect.y, 2, selectionRect.height), "");
        }
    }
        
    /// <summary>
    /// Xử lý di chuyển camera
    /// </summary>
    private void HandleCameraMovement()
    {
        if (mainCamera == null)
            return;
                
        // Camera position
        Vector3 cameraMovement = Vector3.zero;
            
        // Keyboard movement
        if (Input.GetKey(KeyCode.W) || Input.mousePosition.y >= Screen.height - cameraPanBorderThickness)
        {
            cameraMovement.z += cameraSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S) || Input.mousePosition.y <= cameraPanBorderThickness)
        {
            cameraMovement.z -= cameraSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A) || Input.mousePosition.x <= cameraPanBorderThickness)
        {
            cameraMovement.x -= cameraSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D) || Input.mousePosition.x >= Screen.width - cameraPanBorderThickness)
        {
            cameraMovement.x += cameraSpeed * Time.deltaTime;
        }
            
        // Apply movement in local space
        mainCamera.transform.Translate(cameraMovement, Space.Self);
            
        // Camera rotation with middle mouse button
        if (Input.GetMouseButton(2))
        {
            float rotationX = Input.GetAxis("Mouse X") * cameraRotateSpeed * Time.deltaTime;
            mainCamera.transform.RotateAround(mainCamera.transform.position, Vector3.up, rotationX);
        }
            
        // Camera zoom with scroll wheel
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            // Get direction from camera to focus point
            Vector3 cameraDirection = mainCamera.transform.forward;
                
            // Adjust distance
            float distance = scrollInput * cameraZoomSpeed;
                
            // Move camera
            Vector3 newPosition = mainCamera.transform.position + cameraDirection * distance;
                
            // Limit zoom distance
            if (newPosition.y > minZoomDistance && newPosition.y < maxZoomDistance)
            {
                mainCamera.transform.position = newPosition;
            }
        }
    }
        
    /// <summary>
    /// Xử lý input từ chuột
    /// </summary>
    private void HandleMouseInput()
    {
        // Left mouse button down
        if (Input.GetMouseButtonDown(0))
        {
            dragStartPosition = Input.mousePosition;
            isDragging = true;
            isBoxSelecting = false;
        }
            
        // Left mouse button held
        if (Input.GetMouseButton(0) && isDragging)
        {
            // If dragged beyond threshold, start box selection
            if (!isBoxSelecting && Vector2.Distance(dragStartPosition, Input.mousePosition) > dragThreshold)
            {
                isBoxSelecting = true;
            }
                
            // Update selection rectangle
            if (isBoxSelecting)
            {
                float width = Input.mousePosition.x - dragStartPosition.x;
                float height = Input.mousePosition.y - dragStartPosition.y;
                    
                selectionRect = new Rect(
                    width < 0 ? dragStartPosition.x + width : dragStartPosition.x,
                    height < 0 ? dragStartPosition.y + height : dragStartPosition.y,
                    Mathf.Abs(width),
                    Mathf.Abs(height)
                );
            }
        }
            
        // Left mouse button up
        if (Input.GetMouseButtonUp(0))
        {
            if (isBoxSelecting)
            {
                // Process units in selection box
                ProcessSelectionBox();
            }
            else if (!isBoxSelecting && isDragging)
            {
                // Single click
                HandleSingleClick();
            }
                
            isDragging = false;
            isBoxSelecting = false;
        }
            
        // Right mouse button down
        if (Input.GetMouseButtonDown(1))
        {
            // Move or attack command
            HandleRightClick();
        }
    }
        
    /// <summary>
    /// Xử lý input từ bàn phím
    /// </summary>
    private void HandleKeyboardInput()
    {
        // Formation controls
        if (Input.GetKeyDown(KeyCode.F))
        {
            CycleFormation();
        }
            
        // Pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameManager != null)
            {
                gameManager.TogglePause();
            }
        }
    }
        
    /// <summary>
    /// Xử lý khi người chơi click chuột trái
    /// </summary>
    private void HandleSingleClick()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
            
        // Try to select a troop first
        if (Physics.Raycast(ray, out hit, 1000f, troopLayer))
        {
            TroopBase troop = hit.collider.GetComponent<TroopBase>();
            if (troop != null && troop.Squad != null)
            {
                // Select the squad this troop belongs to
                if (gameManager != null)
                {
                    gameManager.SelectSquad(troop.Squad);
                }
            }
        }
        // Then try to select an enemy
        else if (Physics.Raycast(ray, out hit, 1000f, enemyLayer))
        {
            // Just log for now
            Debug.Log("Enemy clicked: " + hit.collider.name);
        }
        // Finally, deselect if clicking on empty ground
        else if (Physics.Raycast(ray, out hit, 1000f, groundLayer))
        {
            if (gameManager != null)
            {
                gameManager.SelectSquad(null);
            }
        }
    }
        
    /// <summary>
    /// Xử lý khi người chơi click chuột phải
    /// </summary>
    private void HandleRightClick()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
            
        // Check if we hit the ground
        if (Physics.Raycast(ray, out hit, 1000f, groundLayer))
        {
            // Move selected squad to position
            if (gameManager != null)
            {
                gameManager.MoveSelectedSquadToPosition(hit.point);
            }
        }
        // Check if we hit an enemy
        else if (Physics.Raycast(ray, out hit, 1000f, enemyLayer))
        {
            // Attack enemy - would implement attack command here
            Debug.Log("Attack command on: " + hit.collider.name);
                
            // This would be replaced with proper attack logic
            SquadFormation selectedSquad = null;
            if (gameManager != null)
            {
                selectedSquad = gameManager.GetComponent<GameManager>().CurrentSelectedSquad;
            }
                
            if (selectedSquad != null)
            {
                // Move squad to enemy position
                selectedSquad.MoveToPosition(hit.collider.transform.position, selectedSquad.transform.rotation);
            }
        }
    }
        
    /// <summary>
    /// Xử lý khi người chơi kéo chọn nhiều đơn vị
    /// </summary>
    [Obsolete("Obsolete")]
    private void ProcessSelectionBox()
    {
        // Convert screen-space selection box to viewport space
        Rect viewportRect = new Rect(
            selectionRect.x / Screen.width,
            selectionRect.y / Screen.height,
            selectionRect.width / Screen.width,
            selectionRect.height / Screen.height
        );
            
        // Find all troops in the scene
        TroopBase[] allTroops = GameObject.FindObjectsOfType<TroopBase>();
        HashSet<SquadFormation> selectedSquads = new HashSet<SquadFormation>();
            
        foreach (TroopBase troop in allTroops)
        {
            // Skip enemy troops
            if (troop.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                continue;
                    
            // Convert troop position to viewport point
            Vector3 viewportPoint = mainCamera.WorldToViewportPoint(troop.transform.position);
                
            // Check if troop is within selection box
            if (viewportRect.Contains(new Vector2(viewportPoint.x, viewportPoint.y)))
            {
                // Add its squad to selection
                if (troop.Squad != null)
                {
                    selectedSquads.Add(troop.Squad);
                }
            }
        }
            
        // Select the first squad in the selection
        // In a more advanced implementation, we could support multiple squad selection
        if (selectedSquads.Count > 0 && gameManager != null)
        {
            gameManager.SelectSquad(selectedSquads.First());
        }
    }
        
    /// <summary>
    /// Thay đổi kiểu đội hình của squad đã chọn
    /// </summary>
    private void CycleFormation()
    {
        SquadFormation selectedSquad = null;
        if (gameManager != null)
        {
            selectedSquad = gameManager.GetComponent<GameManager>().CurrentSelectedSquad;
        }
            
        if (selectedSquad != null)
        {
            // Get current formation
            FormationType currentFormation = selectedSquad.Formation;
                
            // Cycle to next formation
            FormationType nextFormation = FormationType.Square;
                
            switch (currentFormation)
            {
                case FormationType.Square:
                    nextFormation = FormationType.Line;
                    break;
                case FormationType.Line:
                    nextFormation = FormationType.Column;
                    break;
                case FormationType.Column:
                    nextFormation = FormationType.V;
                    break;
                case FormationType.V:
                    nextFormation = FormationType.Square;
                    break;
            }
                
            // Set new formation
            selectedSquad.SetFormationType(nextFormation);
                
            Debug.Log($"Formation changed to: {nextFormation}");
        }
    }
}