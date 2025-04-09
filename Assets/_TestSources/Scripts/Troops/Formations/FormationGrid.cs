using UnityEngine;

/// <summary>
/// Ô lưới chứa vị trí đội hình
/// </summary>
[System.Serializable]
public class FormationCell
{
    [SerializeField] private Vector3 worldPosition;
    [SerializeField] private bool isOccupied = false;
    [SerializeField] private bool isValid = true;
        
    public Vector3 WorldPosition { get => worldPosition; set => worldPosition = value; }
    public bool IsOccupied { get => isOccupied; set => isOccupied = value; }
    public bool IsValid { get => isValid; set => isValid = value; }
}
    
/// <summary>
/// Quản lý lưới các vị trí trên bản đồ, nơi các đội quân có thể di chuyển đến
/// </summary>
public class FormationGrid : MonoBehaviour
{
    [SerializeField] private Vector3 gridCenter = Vector3.zero;
    [SerializeField] private float gridRadius = 50f;
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private float cellSize = 5f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private bool showGizmos = true;
        
    private FormationCell[,] cells;
        
    private void Awake()
    {
        // Nếu chưa được khởi tạo, khởi tạo grid với kích thước mặc định
        if (cells == null)
        {
            Initialize(gridRadius, gridWidth, gridHeight);
        }
    }
        
    /// <summary>
    /// Khởi tạo lưới vị trí
    /// </summary>
    public void Initialize(float radius, int width, int height)
    {
        gridRadius = radius;
        gridWidth = width;
        gridHeight = height;
            
        // Tính toán kích thước ô dựa trên bán kính và số ô
        cellSize = (gridRadius * 2) / Mathf.Max(gridWidth, gridHeight);
            
        // Tạo mảng cells
        cells = new FormationCell[gridWidth, gridHeight];
            
        // Tính toán vị trí góc dưới bên trái của lưới
        Vector3 bottomLeft = new Vector3(
            gridCenter.x - (gridWidth * cellSize / 2),
            gridCenter.y,
            gridCenter.z - (gridHeight * cellSize / 2)
        );
            
        // Tạo các ô
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                Vector3 worldPos = new Vector3(
                    bottomLeft.x + (x * cellSize) + (cellSize / 2),
                    bottomLeft.y,
                    bottomLeft.z + (z * cellSize) + (cellSize / 2)
                );
                    
                // Kiểm tra xem ô có hợp lệ không dựa trên:
                // 1. Nằm trong bán kính quy định
                // 2. Không nằm trong chướng ngại vật
                bool isValid = IsPositionValid(worldPos);
                    
                cells[x, z] = new FormationCell
                {
                    WorldPosition = worldPos,
                    IsOccupied = false,
                    IsValid = isValid
                };
            }
        }
            
        Debug.Log($"Formation grid initialized with {gridWidth}x{gridHeight} cells");
    }
        
    /// <summary>
    /// Kiểm tra xem vị trí có hợp lệ không
    /// </summary>
    private bool IsPositionValid(Vector3 position)
    {
        // Kiểm tra khoảng cách từ tâm
        float distanceFromCenter = Vector3.Distance(position, gridCenter);
        if (distanceFromCenter > gridRadius)
            return false;
                
        // Kiểm tra có nằm trong chướng ngại vật không
        if (Physics.CheckSphere(position, cellSize / 2, obstacleLayer))
            return false;
                
        return true;
    }
        
    /// <summary>
    /// Lấy ô tại vị trí thế giới
    /// </summary>
    public FormationCell GetCellAtWorldPosition(Vector3 worldPosition)
    {
        // Tính toán chỉ số x, z từ vị trí thế giới
        Vector3 bottomLeft = new Vector3(
            gridCenter.x - (gridWidth * cellSize / 2),
            gridCenter.y,
            gridCenter.z - (gridHeight * cellSize / 2)
        );
            
        int x = Mathf.FloorToInt((worldPosition.x - bottomLeft.x) / cellSize);
        int z = Mathf.FloorToInt((worldPosition.z - bottomLeft.z) / cellSize);
            
        // Kiểm tra giới hạn
        if (x < 0 || x >= gridWidth || z < 0 || z >= gridHeight)
            return null;
                
        return cells[x, z];
    }
        
    /// <summary>
    /// Tìm ô trống và hợp lệ gần nhất với vị trí thế giới
    /// </summary>
    public FormationCell FindNearestValidCell(Vector3 worldPosition)
    {
        // Lấy ô tại vị trí hiện tại
        FormationCell currentCell = GetCellAtWorldPosition(worldPosition);
            
        // Nếu ô hiện tại hợp lệ và trống, trả về luôn
        if (currentCell != null && currentCell.IsValid && !currentCell.IsOccupied)
            return currentCell;
                
        // Tìm ô gần nhất
        FormationCell nearestCell = null;
        float nearestDistance = float.MaxValue;
            
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                FormationCell cell = cells[x, z];
                    
                if (cell.IsValid && !cell.IsOccupied)
                {
                    float distance = Vector3.Distance(worldPosition, cell.WorldPosition);
                        
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestCell = cell;
                    }
                }
            }
        }
            
        return nearestCell;
    }
        
    /// <summary>
    /// Đặt trạng thái chiếm giữ cho ô
    /// </summary>
    public void SetCellOccupied(Vector3 worldPosition, bool occupied)
    {
        FormationCell cell = GetCellAtWorldPosition(worldPosition);
        if (cell != null)
        {
            cell.IsOccupied = occupied;
        }
    }
        
    /// <summary>
    /// Đánh dấu tất cả các ô không bị chiếm
    /// </summary>
    public void ResetOccupiedCells()
    {
        if (cells == null)
            return;
                
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                cells[x, z].IsOccupied = false;
            }
        }
    }
        
    /// <summary>
    /// Vẽ lưới trong Scene view
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showGizmos || cells == null)
            return;
                
        // Vẽ tâm của lưới
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(gridCenter, 1f);
            
        // Vẽ vòng tròn biểu thị phạm vi của lưới
        Gizmos.color = Color.cyan;
        DrawCircle(gridCenter, gridRadius, 32);
            
        // Vẽ các ô
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                FormationCell cell = cells[x, z];
                    
                if (!cell.IsValid)
                    continue; // Skip invalid cells
                        
                if (cell.IsOccupied)
                {
                    Gizmos.color = Color.red;
                }
                else
                {
                    Gizmos.color = Color.green;
                }
                    
                Gizmos.DrawWireCube(
                    new Vector3(cell.WorldPosition.x, cell.WorldPosition.y + 0.1f, cell.WorldPosition.z), 
                    new Vector3(cellSize * 0.8f, 0.1f, cellSize * 0.8f)
                );
            }
        }
    }
        
    /// <summary>
    /// Vẽ vòng tròn
    /// </summary>
    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angle = 0f;
        float angleStep = 2 * Mathf.PI / segments;
            
        Vector3 previousPoint = center + new Vector3(radius, 0, 0);
            
        for (int i = 0; i < segments; i++)
        {
            angle += angleStep;
                
            Vector3 nextPoint = center + new Vector3(
                radius * Mathf.Cos(angle),
                0,
                radius * Mathf.Sin(angle)
            );
                
            Gizmos.DrawLine(previousPoint, nextPoint);
                
            previousPoint = nextPoint;
        }
    }
}