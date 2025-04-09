using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IslandDefense.Troops;
using Random = UnityEngine.Random;

namespace IslandDefense.Core
{
    /// <summary>
    /// Quản lý trạng thái game và các hệ thống chính
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        [Header("Game Settings")]
        [SerializeField] private float gameTime = 0f;
        [SerializeField] private bool isPaused = false;
        [SerializeField] private Transform islandCenter;
        [SerializeField] private float islandRadius = 50f;
        
        [Header("Squad Management")]
        [SerializeField] private SquadFormation[] playerSquads;
        [SerializeField] private int maxSquads = 5;
        [SerializeField] private SquadFormation currentSelectedSquad;
        
        [Header("Troop Prefabs")]
        [SerializeField] private GameObject soldierPrefab;
        [SerializeField] private GameObject archerPrefab;
        [SerializeField] private GameObject cavalryPrefab;
        
        [Header("Enemy Management")]
        [SerializeField] private float enemySpawnInterval = 30f;
        [SerializeField] private float initialSpawnDelay = 10f;
        [SerializeField] private Transform[] enemySpawnPoints;
        [SerializeField] private EnemyWave[] enemyWaves;
        
        [Header("Formation Grid")]
        [SerializeField] private FormationGrid formationGrid;
        
        // Các biến nội bộ
        private int currentWave = 0;
        private float nextSpawnTime = 0f;
        private InputManager inputManager;
        private List<EnemyController> activeEnemies = new List<EnemyController>();
        
        // public properties
        public SquadFormation CurrentSelectedSquad => currentSelectedSquad;
        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            
            // Đảm bảo InputManager được khởi tạo
            inputManager = GetComponent<InputManager>();
            if (inputManager == null)
            {
                inputManager = gameObject.AddComponent<InputManager>();
            }
            
            // Khởi tạo danh sách quân đội
            if (playerSquads == null || playerSquads.Length == 0)
            {
                playerSquads = new SquadFormation[maxSquads];
            }
        }
        
        [Obsolete("Obsolete")]
        private void Start()
        {
            nextSpawnTime = Time.time + initialSpawnDelay;
            
            if (formationGrid == null)
            {
                formationGrid = FindObjectOfType<FormationGrid>();
                if (formationGrid == null && islandCenter != null)
                {
                    GameObject gridObj = new GameObject("FormationGrid");
                    gridObj.transform.position = islandCenter.position;
                    formationGrid = gridObj.AddComponent<FormationGrid>();
                    formationGrid.Initialize(islandRadius, 10, 10);
                }
            }
        }
        
        private void Update()
        {
            if (isPaused)
                return;
                
            gameTime += Time.deltaTime;
            
            // Kiểm tra thời gian sinh quân địch
            if (Time.time >= nextSpawnTime && currentWave < enemyWaves.Length)
            {
                SpawnEnemyWave(currentWave);
                currentWave++;
                nextSpawnTime = Time.time + enemySpawnInterval;
            }
            
            // Cập nhật trạng thái game
            UpdateGameState();
        }
        
        /// <summary>
        /// Sinh ra một đợt quân địch
        /// </summary>
        private void SpawnEnemyWave(int waveIndex)
        {
            if (waveIndex >= enemyWaves.Length)
                return;
                
            EnemyWave wave = enemyWaves[waveIndex];
            
            // Đảm bảo có ít nhất một điểm sinh quân
            if (enemySpawnPoints == null || enemySpawnPoints.Length == 0)
            {
                Debug.LogError("No enemy spawn points defined!");
                return;
            }
            
            // Spawn each enemy group in the wave
            foreach (var group in wave.EnemyGroups)
            {
                // Chọn ngẫu nhiên điểm sinh quân
                Transform spawnPoint = enemySpawnPoints[Random.Range(0, enemySpawnPoints.Length)];
                
                // Tạo Squad cho quân địch
                GameObject squadObj = new GameObject($"EnemySquad_Wave{waveIndex}");
                squadObj.transform.position = spawnPoint.position;
                SquadFormation squad = squadObj.AddComponent<SquadFormation>();
                
                // Thiết lập kiểu đội hình
                squad.SetFormationType(group.Formation);
                
                // Sinh quân
                for (int i = 0; i < group.Count; i++)
                {
                    // Tạo enemy từ prefab
                    GameObject enemyObj = Instantiate(group.EnemyPrefab, spawnPoint.position, Quaternion.identity);
                    enemyObj.transform.parent = squadObj.transform;
                    
                    // Thiết lập enemy
                    EnemyController enemy = enemyObj.GetComponent<EnemyController>();
                    if (enemy != null)
                    {
                        enemy.Initialize(group.EnemyConfig);
                        squad.AddTroop(enemy.TroopBase);
                        activeEnemies.Add(enemy);
                    }
                }
                
                // Di chuyển đội địch đến mục tiêu
                Vector3 targetPosition = islandCenter.position + Random.insideUnitSphere * (islandRadius * 0.8f);
                targetPosition.y = spawnPoint.position.y; // Giữ trên mặt phẳng
                
                squad.MoveToPosition(targetPosition, Quaternion.LookRotation(targetPosition - spawnPoint.position));
            }
            
            // Thông báo wave mới
            Debug.Log($"Wave {waveIndex + 1} spawned with {wave.TotalEnemyCount} enemies!");
        }
        
        /// <summary>
        /// Cập nhật trạng thái game
        /// </summary>
        private void UpdateGameState()
        {
            // Loại bỏ các enemy đã bị xóa khỏi danh sách
            activeEnemies.RemoveAll(e => e == null);
            
            // Kiểm tra điều kiện kết thúc game
            if (activeEnemies.Count == 0 && currentWave >= enemyWaves.Length)
            {
                // Victory condition - all waves completed and no enemies left
                Debug.Log("Victory! All enemy waves defeated!");
            }
        }
        
        /// <summary>
        /// Tạo một squad mới tại vị trí chỉ định
        /// </summary>
        public SquadFormation CreateSquad(Vector3 position, FormationType formationType, TroopConfigSO troopConfig, int troopCount)
        {
            // Tìm slot trống
            int slotIndex = -1;
            for (int i = 0; i < playerSquads.Length; i++)
            {
                if (playerSquads[i] == null)
                {
                    slotIndex = i;
                    break;
                }
            }
            
            if (slotIndex == -1)
            {
                Debug.LogWarning("Maximum number of squads reached!");
                return null;
            }
            
            // Tạo squad mới
            GameObject squadObj = new GameObject($"PlayerSquad_{slotIndex}");
            squadObj.transform.position = position;
            SquadFormation squad = squadObj.AddComponent<SquadFormation>();
            
            // Thiết lập kiểu đội hình
            squad.SetFormationType(formationType);
            
            // Thiết lập đúng prefab dựa trên loại troop
            GameObject troopPrefab = null;
            if (troopConfig.TroopName.Contains("Soldier"))
                troopPrefab = soldierPrefab;
            else if (troopConfig.TroopName.Contains("Archer"))
                troopPrefab = archerPrefab;
            else if (troopConfig.TroopName.Contains("Cavalry"))
                troopPrefab = cavalryPrefab;
            else
                troopPrefab = soldierPrefab; // Default
            
            // Tạo troops
            for (int i = 0; i < troopCount && i < 9; i++)
            {
                GameObject troopObj = Instantiate(troopPrefab, position, Quaternion.identity);
                troopObj.transform.parent = squadObj.transform;
                
                TroopBase troop = troopObj.GetComponent<TroopBase>();
                if (troop != null)
                {
                    // Thiết lập config cho troop
                    System.Reflection.FieldInfo troopConfigField = typeof(TroopBase).GetField("troopConfig", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        
                    if (troopConfigField != null)
                    {
                        troopConfigField.SetValue(troop, troopConfig);
                    }
                    
                    troop.InitializeFromConfig();
                    squad.AddTroop(troop);
                }
            }
            
            // Thêm vào danh sách
            playerSquads[slotIndex] = squad;
            
            // Chọn squad vừa tạo
            SelectSquad(squad);
            
            return squad;
        }
        
        /// <summary>
        /// Chọn squad
        /// </summary>
        public void SelectSquad(SquadFormation squad)
        {
            // Bỏ chọn squad hiện tại
            if (currentSelectedSquad != null)
            {
                // Tắt hiệu ứng chọn
                foreach (Transform child in currentSelectedSquad.transform)
                {
                    TroopView view = child.GetComponent<TroopView>();
                    if (view != null)
                    {
                        view.SetSelection(false);
                    }
                }
            }
            
            // Thiết lập squad mới
            currentSelectedSquad = squad;
            
            // Bật hiệu ứng chọn
            if (currentSelectedSquad != null)
            {
                foreach (Transform child in currentSelectedSquad.transform)
                {
                    TroopView view = child.GetComponent<TroopView>();
                    if (view != null)
                    {
                        view.SetSelection(true);
                    }
                }
            }
        }
        
        /// <summary>
        /// Di chuyển squad đã chọn đến vị trí
        /// </summary>
        public void MoveSelectedSquadToPosition(Vector3 position)
        {
            if (currentSelectedSquad != null)
            {
                // Kiểm tra xem vị trí có nằm trong grid không
                if (formationGrid != null)
                {
                    FormationCell cell = formationGrid.GetCellAtWorldPosition(position);
                    if (cell != null && !cell.IsOccupied)
                    {
                        // Di chuyển đến ô trên grid
                        currentSelectedSquad.MoveToPosition(cell.WorldPosition, 
                            Quaternion.LookRotation(islandCenter.position - cell.WorldPosition));
                        
                        // Đánh dấu ô đã bị chiếm
                        cell.IsOccupied = true;
                        
                        // Bỏ đánh dấu ô cũ
                        FormationCell oldCell = formationGrid.GetCellAtWorldPosition(currentSelectedSquad.transform.position);
                        if (oldCell != null && oldCell != cell)
                        {
                            oldCell.IsOccupied = false;
                        }
                    }
                }
                else
                {
                    // Không có grid, di chuyển đến vị trí chỉ định
                    currentSelectedSquad.MoveToPosition(position, currentSelectedSquad.transform.rotation);
                }
            }
        }
        
        /// <summary>
        /// Tạm dừng/tiếp tục game
        /// </summary>
        public void TogglePause()
        {
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : 1f;
        }
    }
    
    /// <summary>
    /// Thông tin về một đợt quân địch
    /// </summary>
    [System.Serializable]
    public class EnemyWave
    {
        [SerializeField] private string waveName = "Wave";
        [SerializeField] private EnemyGroup[] enemyGroups;
        
        public string WaveName => waveName;
        public EnemyGroup[] EnemyGroups => enemyGroups;
        
        public int TotalEnemyCount
        {
            get
            {
                int total = 0;
                foreach (var group in enemyGroups)
                {
                    total += group.Count;
                }
                return total;
            }
        }
    }
    
    /// <summary>
    /// Nhóm quân địch trong một đợt
    /// </summary>
    [System.Serializable]
    public class EnemyGroup
    {
        [SerializeField] private string groupName = "Group";
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private TroopConfigSO enemyConfig;
        [SerializeField] private int count = 5;
        [SerializeField] private FormationType formation = FormationType.Square;
        
        public string GroupName => groupName;
        public GameObject EnemyPrefab => enemyPrefab;
        public TroopConfigSO EnemyConfig => enemyConfig;
        public int Count => count;
        public FormationType Formation => formation;
    }
}