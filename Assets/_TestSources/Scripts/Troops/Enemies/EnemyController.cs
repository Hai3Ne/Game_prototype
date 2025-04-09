using IslandDefense.Troops;
using StateMachine;
using UnityEngine;

/// <summary>
/// Điều khiển AI cho kẻ địch, kế thừa từ TroopController
/// </summary>
public class EnemyController : TroopController
{
    [Header("Enemy Settings")]
    [SerializeField] private float aggroRadius = 15f;
    [SerializeField] private float minAttackDistance = 8f;
    [SerializeField] private float targetUpdateInterval = 1f;
    [SerializeField] private LayerMask playerTroopLayer;
        
    [Header("AI Parameters")]
    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float aggressiveness = 0.7f; // 0-1, how likely to attack
    [SerializeField] private float caution = 0.5f; // 0-1, how cautious when approaching
        
    // Truy cập TroopBase
    private TroopBase troopBase;
        
    // Biến theo dõi thời gian
    private float lastTargetUpdate = 0f;
        
    // Biến trạng thái
    private Vector3 initialPosition;
    private bool hasDestination = false;
    private Vector3 currentDestination;
        
    public TroopBase TroopBase => troopBase;
        
    protected override void Awake()
    {
        base.Awake();
            
        troopBase = GetComponent<TroopBase>();
        if (troopBase == null)
        {
            Debug.LogError("EnemyController requires a TroopBase component!");
        }
            
        // Lưu vị trí ban đầu
        initialPosition = transform.position;
    }
        
    /// <summary>
    /// Khởi tạo kẻ địch với config
    /// </summary>
    public void Initialize(TroopConfigSO enemyConfig)
    {
        if (troopBase == null)
            return;
                
        // Thiết lập config
        System.Reflection.FieldInfo troopConfigField = typeof(TroopBase).GetField("troopConfig", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
        if (troopConfigField != null)
        {
            troopConfigField.SetValue(troopBase, enemyConfig);
        }
            
        // Khởi tạo từ config
        troopBase.InitializeFromConfig();
            
        // Điều chỉnh các tham số dựa trên config
        aggroRadius = enemyConfig.AttackRange * 3f;
        minAttackDistance = enemyConfig.AttackRange * 0.8f;
    }
        
    private void Update()
    {
        // Cập nhật AI
        UpdateEnemyAI();
    }
        
    /// <summary>
    /// Cập nhật AI của kẻ địch
    /// </summary>
    private void UpdateEnemyAI()
    {
        // Cập nhật mục tiêu theo chu kỳ
        if (Time.time > lastTargetUpdate + targetUpdateInterval)
        {
            lastTargetUpdate = Time.time;
            UpdateTarget();
        }
            
        // Nếu đã có mục tiêu, tấn công
        if (troopBase.TargetTroop != null)
        {
            // Kiểm tra khoảng cách
            float distanceToTarget = Vector3.Distance(
                transform.position, 
                troopBase.TargetTroop.transform.position
            );
                
            // Nếu đã vượt quá phạm vi phát hiện, hủy mục tiêu
            if (distanceToTarget > aggroRadius * 1.5f)
            {
                ClearTarget();
            }
            else if (distanceToTarget <= troopBase.AttackRange)
            {
                hasDestination = false;
                if (GetComponent<TroopStateMachine>().CurrentStateType != TroopStateType.Attack)
                {
                    GetComponent<TroopStateMachine>().ChangeState(TroopStateType.Attack);
                }
            }
            // Nếu chưa trong tầm tấn công, di chuyển đến gần hơn
            else
            {
                // Tiếp cận mục tiêu
                ApproachTarget();
            }
        }
        else if (!hasDestination)
        {
            // Chọn điểm đến ngẫu nhiên
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection.y = 0;
                
            currentDestination = initialPosition + randomDirection;
            hasDestination = true;
                
            // Di chuyển đến điểm đến
            MoveToPosition(currentDestination);
        }
    }
        
    /// <summary>
    /// Tìm mục tiêu gần nhất
    /// </summary>
    private void UpdateTarget()
    {
        // Tìm các đơn vị người chơi trong phạm vi phát hiện
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, aggroRadius, playerTroopLayer);
            
        if (hitColliders.Length > 0)
        {
            // Xác định xem có tấn công không dựa trên tính hung hăng
            if (Random.value <= aggressiveness)
            {
                // Tìm đơn vị gần nhất
                TroopBase nearestTroop = null;
                float nearestDistance = float.MaxValue;
                    
                foreach (var hitCollider in hitColliders)
                {
                    TroopBase troop = hitCollider.GetComponent<TroopBase>();
                        
                    if (troop != null)
                    {
                        float distance = Vector3.Distance(transform.position, troop.transform.position);
                            
                        if (distance < nearestDistance)
                        {
                            nearestDistance = distance;
                            nearestTroop = troop;
                        }
                    }
                }
                    
                // Thiết lập mục tiêu mới
                if (nearestTroop != null)
                {
                    troopBase.TargetTroop = nearestTroop;
                    troopBase.TargetTransform = nearestTroop.transform;
                }
            }
        }
    }
        
    /// <summary>
    /// Hủy mục tiêu hiện tại
    /// </summary>
    private void ClearTarget()
    {
        troopBase.TargetTroop = null;
        troopBase.TargetTransform = null;
        hasDestination = false;
            
        // Chuyển về trạng thái idle
        if (GetComponent<TroopStateMachine>().CurrentStateType != TroopStateType.Idle)
        {
            GetComponent<TroopStateMachine>().ChangeState(TroopStateType.Idle);
        }
    }
        
    /// <summary>
    /// Tiếp cận mục tiêu với độ thận trọng
    /// </summary>
    private void ApproachTarget()
    {
        if (troopBase.TargetTroop == null)
            return;
                
        // Chọn vector tiếp cận dựa trên độ thận trọng
        Vector3 approachVector;
            
        if (Random.value < caution)
        {
            // Tiếp cận thận trọng - không đi thẳng vào mục tiêu
            float randomAngle = Random.Range(-45f, 45f);
            Vector3 directionToTarget = (troopBase.TargetTroop.transform.position - transform.position).normalized;
                
            // Tạo vector tiếp cận lệch một góc ngẫu nhiên
            approachVector = Quaternion.Euler(0, randomAngle, 0) * directionToTarget;
        }
        else
        {
            // Tiếp cận trực tiếp
            approachVector = (troopBase.TargetTroop.transform.position - transform.position).normalized;
        }
            
        // Tính toán vị trí tiếp cận sao cho giữ khoảng cách tối thiểu
        Vector3 targetPosition = troopBase.TargetTroop.transform.position - (approachVector * minAttackDistance);
            
        // Di chuyển đến vị trí tiếp cận
        currentDestination = targetPosition;
        hasDestination = true;
            
        // Chuyển sang trạng thái di chuyển
        if (GetComponent<TroopStateMachine>().CurrentStateType != TroopStateType.Move)
        {
            GetComponent<TroopStateMachine>().ChangeState(TroopStateType.Move);
        }
            
        // Gọi phương thức di chuyển
        MoveToPosition(targetPosition);
    }
        
    private void OnDrawGizmosSelected()
    {
        // Vẽ phạm vi phát hiện
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRadius);
            
        // Vẽ phạm vi tấn công
        if (troopBase != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, troopBase.AttackRange);
        }
            
        // Vẽ khoảng cách tiếp cận tối thiểu
        Gizmos.color = new Color(1, 0.5f, 0);
        Gizmos.DrawWireSphere(transform.position, minAttackDistance);
            
        // Vẽ hướng đến mục tiêu hoặc điểm đến
        if (troopBase != null && troopBase.TargetTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, troopBase.TargetTransform.position);
        }
        else if (hasDestination)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, currentDestination);
            Gizmos.DrawSphere(currentDestination, 0.5f);
        }
    }
}