using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Lưu trữ thông tin hướng vector và xác suất
/// </summary>
[System.Serializable]
public struct DirectionVectorConfig
{
    public Vector3 direction;
    [Range(0f, 1f)] public float probability;
}

/// <summary>
/// Lưu trữ thông tin cấu hình cho behavior
/// </summary>
[System.Serializable]
public class BehaviorConfig
{
    public string name;
    [FormerlySerializedAs("behaviorType")] public BehaviorType BehaviorType;
    [FormerlySerializedAs("weight")] [Range(0f, 1f)] public float Weight = 1f;
    [FormerlySerializedAs("probability")] [Range(0f, 1f)] public float Probability = 1f;
        
    public List<DirectionVectorConfig> directionVectors = new List<DirectionVectorConfig>();

    // Tham số bổ sung dành riêng cho từng loại behavior
    public float ArrivalRadius = 1f;
    [FormerlySerializedAs("slowingRadius")] public float SlowingRadius = 5f;
    [FormerlySerializedAs("neighborRadius")] public float NeighborRadius = 5f;
    [FormerlySerializedAs("separationRadius")] public float SeparationRadius = 2f;
    [FormerlySerializedAs("lookAheadDistance")] public float LookAheadDistance = 5f;
    [FormerlySerializedAs("obstacleLayer")] public LayerMask ObstacleLayer;

    /// <summary>
    /// Lấy vector hướng ngẫu nhiên dựa trên xác suất
    /// </summary>
    public Vector3 GetRandomDirectionVector()
    {
        if (directionVectors.Count == 0)
            return Vector3.forward;

        // Tính tổng xác suất
        float totalProbability = 0f;
        foreach (var vector in directionVectors)
        {
            totalProbability += vector.probability;
        }

        // Tạo giá trị ngẫu nhiên
        float random = Random.value;
        float cumulativeProbability = 0f;

        // Tìm vector cần sử dụng
        foreach (var vector in directionVectors)
        {
            float normalizedProb = (totalProbability > 0) 
                ? vector.probability / totalProbability 
                : 1.0f / directionVectors.Count;
                
            cumulativeProbability += normalizedProb;
            if (random <= cumulativeProbability)
            {
                return vector.direction.normalized;
            }
        }

        // Mặc định
        return directionVectors[0].direction.normalized;
    }
}

/// <summary>
/// Scriptable Object để lưu trữ dữ liệu cấu hình cho đơn vị
/// </summary>
[CreateAssetMenu(fileName = "NewTroopConfig", menuName = "Island Defense/Troop Config")]
public class TroopConfigSO : ScriptableObject
{
    [Header("Basic Info")]
    public string troopName = "Troop";
    [TextArea(2, 5)]
    public string description = "A basic troop";
    public Sprite icon;
    public GameObject prefab;

    [Header("Stats")]
    public float maxHealth = 100f;
    public float attackPower = 10f;
    public float attackRange = 2f;
    public float attackSpeed = 1f; // Số đòn tấn công mỗi giây
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float maxSteeringForce = 10f;

    [Header("Behaviors")]
    public List<BehaviorConfig> behaviors = new List<BehaviorConfig>();

    // Properties để truy cập thuộc tính
    public string TroopName => troopName;
    public string Description => description;
    public Sprite Icon => icon;
    public GameObject Prefab => prefab;
    public float MaxHealth => maxHealth;
    public float AttackPower => attackPower;
    public float AttackRange => attackRange;
    public float AttackSpeed => attackSpeed;
    public float MoveSpeed => moveSpeed;
    public float RotationSpeed => rotationSpeed;
    public float MaxSteeringForce => maxSteeringForce;
    public List<BehaviorConfig> Behaviors => behaviors;

    /// <summary>
    /// Thêm một behavior mới
    /// </summary>
    public void AddBehavior(BehaviorConfig behaviorConfig)
    {
        behaviors.Add(behaviorConfig);
    }

    /// <summary>
    /// Xóa một behavior
    /// </summary>
    public void RemoveBehavior(BehaviorConfig behaviorConfig)
    {
        behaviors.Remove(behaviorConfig);
    }
        
    /// <summary>
    /// Tìm kiếm behavior theo loại
    /// </summary>
    public BehaviorConfig FindBehaviorByType(BehaviorType type)
    {
        return behaviors.Find(b => b.BehaviorType == type);
    }
}

/// <summary>
/// Các loại behavior
/// </summary>
public enum BehaviorType
{
    Seek,
    Flee,
    Arrival,
    Separation,
    Cohesion,
    Alignment,
    ObstacleAvoidance,
    PathFollowing,
    Attack,
    Defense,
    Fear
}