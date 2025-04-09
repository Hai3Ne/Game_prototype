using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace IslandDefense.Troops.Editor
{
    /// <summary>
    /// Editor cải tiến với khả năng lưu/nạp cấu hình và kéo thả vector
    /// </summary>
    public class ImprovedBehaviorEditor : EditorWindow
    {
        private TroopConfigSO selectedConfig;
        private int selectedBehaviorIndex = -1;
        private int selectedVectorIndex = -1;
        private Vector2 scrollPosition;
        
        // Nhiều vector
        private int numberOfVectors = 8;
        private float vectorSpread = 180f;
        private float startAngle = -90f;
        
        // Vẽ vector
        private Texture2D gridTexture;
        private float gridSize = 10f;
        private float circleScaleFactor = 0.7f; // Hệ số thu nhỏ vòng tròn
        
        // Kéo thả
        private bool isDraggingVector = false;
        private int draggingVectorIndex = -1;
        
        // Tab
        private int currentTab = 0;
        private string[] tabNames = { "Chỉnh Sửa", "Lưu/Nạp", "Tùy Chọn" };
        
        // Đường dẫn thư mục lưu
        private string saveFolder = "Assets/VectorPresets";
        private string currentPresetName = "NewPreset";
        private List<string> availablePresets = new List<string>();
        private Vector2 presetScrollPosition;
        
        [MenuItem("Island Defense/Improved Behavior Editor")]
        public static void ShowWindow()
        {
            GetWindow<ImprovedBehaviorEditor>("Improved Behavior Editor");
        }
        
        private void OnEnable()
        {
            CreateGridTexture();
            RefreshPresetList();
        }
        
        private void CreateGridTexture()
        {
            gridTexture = new Texture2D(2, 2);
            gridTexture.SetPixel(0, 0, new Color(0.2f, 0.2f, 0.2f));
            gridTexture.SetPixel(1, 0, new Color(0.2f, 0.2f, 0.2f));
            gridTexture.SetPixel(0, 1, new Color(0.2f, 0.2f, 0.2f));
            gridTexture.SetPixel(1, 1, new Color(0.2f, 0.2f, 0.2f));
            gridTexture.Apply();
        }
        
        private void RefreshPresetList()
        {
            availablePresets.Clear();
            
            // Đảm bảo thư mục tồn tại
            if (!Directory.Exists(saveFolder))
            {
                Directory.CreateDirectory(saveFolder);
                AssetDatabase.Refresh();
            }
            
            // Liệt kê các file .json
            string[] files = Directory.GetFiles(saveFolder, "*.json");
            foreach (string file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                availablePresets.Add(fileName);
            }
        }
        
        private void OnGUI()
        {
            DrawToolbar();
            
            EditorGUILayout.Space();
            
            // Tabs
            currentTab = GUILayout.Toolbar(currentTab, tabNames, EditorStyles.toolbarButton);
            
            EditorGUILayout.Space();
            
            if (selectedConfig == null)
            {
                EditorGUILayout.HelpBox("Vui lòng chọn một Troop Config", MessageType.Info);
                return;
            }
            
            switch (currentTab)
            {
                case 0: // Tab Chỉnh Sửa
                    DrawEditTab();
                    break;
                case 1: // Tab Lưu/Nạp
                    DrawSaveLoadTab();
                    break;
                case 2: // Tab Tùy Chọn
                    DrawOptionsTab();
                    break;
            }
            
            // Áp dụng thay đổi
            if (GUI.changed)
            {
                EditorUtility.SetDirty(selectedConfig);
            }
        }
        
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            GUILayout.Label("Troop Config:", GUILayout.Width(80));
            
            selectedConfig = (TroopConfigSO)EditorGUILayout.ObjectField(
                selectedConfig, typeof(TroopConfigSO), false, GUILayout.Width(200));
            
            if (GUILayout.Button("Create New", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                CreateNewConfig();
            }
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Reset Selected", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                selectedBehaviorIndex = -1;
                selectedVectorIndex = -1;
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawEditTab()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            DrawTroopInfo();
            
            EditorGUILayout.Space();
            
            DrawBehaviors();
            
            EditorGUILayout.Space();
            
            if (selectedBehaviorIndex >= 0 && selectedBehaviorIndex < selectedConfig.Behaviors.Count)
            {
                DrawSelectedBehavior();
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawSaveLoadTab()
        {
            if (selectedBehaviorIndex < 0 || selectedBehaviorIndex >= selectedConfig.Behaviors.Count)
            {
                EditorGUILayout.HelpBox("Hãy chọn một behavior để lưu/nạp cấu hình vector", MessageType.Info);
                return;
            }
            
            BehaviorConfig behavior = selectedConfig.Behaviors[selectedBehaviorIndex];
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Lưu/Nạp Cấu Hình Vector cho: {behavior.name}", EditorStyles.boldLabel);
            
            // Lưu cấu hình
            EditorGUILayout.LabelField("Lưu Cấu Hình", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            currentPresetName = EditorGUILayout.TextField("Tên Preset:", currentPresetName);
            
            GUI.enabled = !string.IsNullOrEmpty(currentPresetName);
            if (GUILayout.Button("Lưu", GUILayout.Width(80)))
            {
                SaveVectorPreset(behavior, currentPresetName);
            }
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Nạp cấu hình
            EditorGUILayout.LabelField("Nạp Cấu Hình", EditorStyles.boldLabel);
            
            if (availablePresets.Count > 0)
            {
                presetScrollPosition = EditorGUILayout.BeginScrollView(presetScrollPosition, GUILayout.Height(200));
                
                foreach (string preset in availablePresets)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    if (GUILayout.Button(preset, EditorStyles.label))
                    {
                        LoadVectorPreset(behavior, preset);
                    }
                    
                    if (GUILayout.Button("Nạp", EditorStyles.miniButton, GUILayout.Width(60)))
                    {
                        LoadVectorPreset(behavior, preset);
                    }
                    
                    if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(20)))
                    {
                        if (EditorUtility.DisplayDialog("Xóa Preset", 
                            $"Bạn có chắc chắn muốn xóa '{preset}'?", 
                            "Xóa", "Hủy"))
                        {
                            DeleteVectorPreset(preset);
                        }
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.HelpBox("Chưa có preset nào được lưu", MessageType.Info);
            }
            
            if (GUILayout.Button("Làm Mới Danh Sách"))
            {
                RefreshPresetList();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawOptionsTab()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Tùy Chọn Hiển Thị", EditorStyles.boldLabel);
            
            // Tỷ lệ vòng tròn
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Kích Thước Vòng Tròn:", GUILayout.Width(150));
            circleScaleFactor = EditorGUILayout.Slider(circleScaleFactor, 0.3f, 1.0f);
            EditorGUILayout.EndHorizontal();
            
            // Kích thước lưới
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Kích Thước Lưới:", GUILayout.Width(150));
            gridSize = EditorGUILayout.Slider(gridSize, 5f, 20f);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Tùy chọn lưu trữ
            EditorGUILayout.LabelField("Tùy Chọn Lưu Trữ", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Thư Mục Lưu:", GUILayout.Width(150));
            
            EditorGUILayout.LabelField(saveFolder);
            
            if (GUILayout.Button("Đổi", GUILayout.Width(60)))
            {
                string newPath = EditorUtility.OpenFolderPanel("Chọn Thư Mục Lưu", Application.dataPath, "");
                
                if (!string.IsNullOrEmpty(newPath))
                {
                    // Chuyển đổi thành đường dẫn tương đối với Unity project
                    if (newPath.StartsWith(Application.dataPath))
                    {
                        saveFolder = "Assets" + newPath.Substring(Application.dataPath.Length);
                        RefreshPresetList();
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Lỗi", 
                            "Vui lòng chọn thư mục trong project Unity", 
                            "OK");
                    }
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void CreateNewConfig()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Troop Config", 
                "NewTroopConfig", 
                "asset", 
                "Create a new Troop Config asset");
            
            if (string.IsNullOrEmpty(path))
                return;
                
            TroopConfigSO newConfig = CreateInstance<TroopConfigSO>();
            AssetDatabase.CreateAsset(newConfig, path);
            AssetDatabase.SaveAssets();
            
            selectedConfig = newConfig;
        }
        
        private void DrawTroopInfo()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("Troop Information", EditorStyles.boldLabel);
            
            selectedConfig.troopName = EditorGUILayout.TextField("Name", selectedConfig.troopName);
            selectedConfig.description = EditorGUILayout.TextField("Description", selectedConfig.description);
            selectedConfig.icon = (Sprite)EditorGUILayout.ObjectField("Icon", selectedConfig.icon, typeof(Sprite), false);
            selectedConfig.prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", selectedConfig.prefab, typeof(GameObject), false);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Stats", EditorStyles.boldLabel);
            
            selectedConfig.maxHealth = EditorGUILayout.FloatField("Max Health", selectedConfig.maxHealth);
            selectedConfig.attackPower = EditorGUILayout.FloatField("Attack Power", selectedConfig.attackPower);
            selectedConfig.attackRange = EditorGUILayout.FloatField("Attack Range", selectedConfig.attackRange);
            selectedConfig.attackSpeed = EditorGUILayout.FloatField("Attack Speed", selectedConfig.attackSpeed);
            selectedConfig.moveSpeed = EditorGUILayout.FloatField("Move Speed", selectedConfig.moveSpeed);
            selectedConfig.rotationSpeed = EditorGUILayout.FloatField("Rotation Speed", selectedConfig.rotationSpeed);
            selectedConfig.maxSteeringForce = EditorGUILayout.FloatField("Max Steering Force", selectedConfig.maxSteeringForce);
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawBehaviors()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("Behaviors", EditorStyles.boldLabel);
            
            // Kiểm tra null
            if (selectedConfig.behaviors == null)
            {
                selectedConfig.behaviors = new List<BehaviorConfig>();
            }
            
            // Nút thêm tất cả Behavior
            if (GUILayout.Button("Add All Behavior Types"))
            {
                AddAllBehaviorTypes();
            }
            
            // Danh sách behaviors hiện có
            for (int i = 0; i < selectedConfig.behaviors.Count; i++)
            {
                BehaviorConfig behavior = selectedConfig.behaviors[i];
                
                // Đảm bảo behavior không null
                if (behavior == null)
                {
                    selectedConfig.behaviors[i] = new BehaviorConfig
                    {
                        name = "NewBehavior",
                        BehaviorType = BehaviorType.Seek
                    };
                    behavior = selectedConfig.behaviors[i];
                }
                
                EditorGUILayout.BeginHorizontal();
                
                // Nút chọn
                bool isSelected = (selectedBehaviorIndex == i);
                GUI.color = isSelected ? Color.cyan : Color.white;
                
                if (GUILayout.Button(isSelected ? "✓" : " ", GUILayout.Width(25)))
                {
                    selectedBehaviorIndex = isSelected ? -1 : i;
                    selectedVectorIndex = -1;
                }
                
                GUI.color = Color.white;
                
                // Tên behavior
                EditorGUILayout.LabelField($"{behavior.name} ({behavior.BehaviorType})");
                
                // Xác suất
                behavior.Probability = EditorGUILayout.Slider(behavior.Probability, 0, 1, GUILayout.Width(100));
                
                // Nút xóa
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    if (EditorUtility.DisplayDialog("Delete Behavior", 
                        "Are you sure you want to delete this behavior?", 
                        "Delete", "Cancel"))
                    {
                        selectedConfig.behaviors.RemoveAt(i);
                        if (selectedBehaviorIndex == i)
                        {
                            selectedBehaviorIndex = -1;
                        }
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            // Nút thêm behavior mới
            if (GUILayout.Button("Add Behavior"))
            {
                ShowAddBehaviorMenu();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void AddAllBehaviorTypes()
        {
            foreach (BehaviorType behaviorType in System.Enum.GetValues(typeof(BehaviorType)))
            {
                // Kiểm tra xem loại behavior đã tồn tại chưa
                bool exists = false;
                foreach (var behavior in selectedConfig.behaviors)
                {
                    if (behavior != null && behavior.BehaviorType == behaviorType)
                    {
                        exists = true;
                        break;
                    }
                }
                
                // Thêm behavior nếu chưa tồn tại
                if (!exists)
                {
                    AddBehavior(behaviorType);
                }
            }
        }
        
        private void ShowAddBehaviorMenu()
        {
            GenericMenu menu = new GenericMenu();
            
            foreach (BehaviorType behaviorType in System.Enum.GetValues(typeof(BehaviorType)))
            {
                menu.AddItem(new GUIContent(behaviorType.ToString()), false, () => {
                    AddBehavior(behaviorType);
                });
            }
            
            menu.ShowAsContext();
        }
        
        private void AddBehavior(BehaviorType behaviorType)
        {
            BehaviorConfig newBehavior = new BehaviorConfig
            {
                name = behaviorType.ToString() + "Behavior",
                BehaviorType = behaviorType,
                Weight = 1.0f,
                Probability = behaviorType == BehaviorType.Attack ? 0.95f : 0.5f,
                directionVectors = new List<DirectionVectorConfig>()
            };
            
            // Thêm vector mặc định nếu cần
            if (behaviorType == BehaviorType.Attack || 
                behaviorType == BehaviorType.Flee || 
                behaviorType == BehaviorType.Defense)
            {
                newBehavior.directionVectors.Add(new DirectionVectorConfig
                {
                    direction = Vector3.forward,
                    probability = 1.0f
                });
            }
            
            // Thêm behavior mới vào danh sách
            selectedConfig.behaviors.Add(newBehavior);
            selectedBehaviorIndex = selectedConfig.behaviors.Count - 1;
            
            EditorUtility.SetDirty(selectedConfig);
        }
        
        private void DrawSelectedBehavior()
        {
            BehaviorConfig behavior = selectedConfig.behaviors[selectedBehaviorIndex];
            
            if (behavior == null)
            {
                return;
            }
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField($"Editing: {behavior.name}", EditorStyles.boldLabel);
            
            // Thông tin cơ bản
            behavior.name = EditorGUILayout.TextField("Name", behavior.name);
            EditorGUI.BeginDisabledGroup(true); // Không cho phép thay đổi loại behavior
            EditorGUILayout.EnumPopup("Type", behavior.BehaviorType);
            EditorGUI.EndDisabledGroup();
            
            behavior.Weight = EditorGUILayout.Slider("Weight", behavior.Weight, 0, 1);
            behavior.Probability = EditorGUILayout.Slider("Probability", behavior.Probability, 0, 1);
            
            // Tham số bổ sung dựa trên loại behavior
            switch (behavior.BehaviorType)
            {
                case BehaviorType.Seek:
                case BehaviorType.Flee:
                case BehaviorType.Arrival:
                    behavior.ArrivalRadius = EditorGUILayout.FloatField("Arrival Radius", behavior.ArrivalRadius);
                    behavior.SlowingRadius = EditorGUILayout.FloatField("Slowing Radius", behavior.SlowingRadius);
                    break;
                
                case BehaviorType.Separation:
                case BehaviorType.Cohesion:
                case BehaviorType.Alignment:
                    behavior.NeighborRadius = EditorGUILayout.FloatField("Neighbor Radius", behavior.NeighborRadius);
                    behavior.SeparationRadius = EditorGUILayout.FloatField("Separation Radius", behavior.SeparationRadius);
                    break;
            }
            
            EditorGUILayout.Space();
            
            // Direction Vectors
            EditorGUILayout.LabelField("Direction Vectors", EditorStyles.boldLabel);
            
            // Kiểm tra null và khởi tạo nếu cần
            if (behavior.directionVectors == null)
            {
                behavior.directionVectors = new List<DirectionVectorConfig>();
            }
            
            // Công cụ tạo vector
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("Vector Creation Tools", EditorStyles.miniBoldLabel);
            
            numberOfVectors = EditorGUILayout.IntSlider("Number of Vectors", numberOfVectors, 1, 16);
            vectorSpread = EditorGUILayout.Slider("Spread Angle", vectorSpread, 10f, 360f);
            startAngle = EditorGUILayout.Slider("Starting Angle", startAngle, -180f, 180f);
            
            if (GUILayout.Button("Generate Even Vectors"))
            {
                GenerateEvenVectors(behavior);
            }
            
            EditorGUILayout.EndVertical();
            
            // Khu vực vẽ vector
            DrawVectorArea(behavior);
            
            EditorGUILayout.Space();
            
            // Danh sách vector
            EditorGUILayout.LabelField("Vector List", EditorStyles.miniBoldLabel);
            
            // Tính tổng xác suất để hiển thị phần trăm
            float totalProb = 0f;
            foreach (var vector in behavior.directionVectors)
            {
                totalProb += vector.probability;
            }
            
            // Hiển thị từng vector
            for (int i = 0; i < behavior.directionVectors.Count; i++)
            {
                DirectionVectorConfig vector = behavior.directionVectors[i];
                
                EditorGUILayout.BeginHorizontal();
                
                // Nút chọn
                bool isSelected = (selectedVectorIndex == i);
                GUI.color = isSelected ? Color.cyan : Color.white;
                
                if (GUILayout.Toggle(isSelected, "", GUILayout.Width(15)) != isSelected)
                {
                    selectedVectorIndex = isSelected ? -1 : i;
                }
                
                GUI.color = Color.white;
                
                // Hiển thị vector
                Vector3 normalized = vector.direction.normalized;
                EditorGUILayout.LabelField($"Vec {i+1}: [{normalized.x:F2}, {normalized.y:F2}, {normalized.z:F2}]", 
                    GUILayout.Width(250));
                
                // Hiển thị phần trăm
                float percent = totalProb > 0 ? (vector.probability / totalProb) * 100f : 0f;
                EditorGUILayout.LabelField($"{percent:F1}%", GUILayout.Width(50));
                
                // Xác suất
                vector.probability = EditorGUILayout.Slider(vector.probability, 0, 1, GUILayout.Width(100));
                behavior.directionVectors[i] = vector; // Phải gán lại vì struct là value type
                
                // Nút xóa
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    behavior.directionVectors.RemoveAt(i);
                    if (selectedVectorIndex == i)
                    {
                        selectedVectorIndex = -1;
                    }
                    break;
                }
                
                EditorGUILayout.EndHorizontal();
                
                // Hiển thị chi tiết nếu được chọn
                if (isSelected)
                {
                    EditorGUI.indentLevel++;
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Direction:", GUILayout.Width(100));
                    
                    Vector3 newDir = EditorGUILayout.Vector3Field("", vector.direction);
                    if (newDir != vector.direction)
                    {
                        vector.direction = newDir;
                        behavior.directionVectors[i] = vector;
                    }
                    
                    EditorGUILayout.EndHorizontal();
                    
                    if (GUILayout.Button("Normalize", GUILayout.Width(100)))
                    {
                        vector.direction = vector.direction.normalized;
                        behavior.directionVectors[i] = vector;
                    }
                    
                    EditorGUI.indentLevel--;
                }
            }
            
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginHorizontal();
            
            // Nút thêm vector
            if (GUILayout.Button("Add Vector", GUILayout.Width(100)))
            {
                DirectionVectorConfig newVector = new DirectionVectorConfig
                {
                    direction = Vector3.forward,
                    probability = 0.5f
                };
                
                behavior.directionVectors.Add(newVector);
                selectedVectorIndex = behavior.directionVectors.Count - 1;
            }
            
            // Nút chuẩn hóa
            if (GUILayout.Button("Normalize Probabilities", GUILayout.Width(180)))
            {
                NormalizeProbabilities(behavior);
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawVectorArea(BehaviorConfig behavior)
        {
            // Lấy khu vực vẽ
            float size = 300;
            Rect rect = GUILayoutUtility.GetRect(size, size);
            
            // Điểm trung tâm
            Vector2 center = new Vector2(rect.x + rect.width / 2, rect.y + rect.height / 2);
            float radius = (rect.width / 2 - 10) * circleScaleFactor;
            
            // Vẽ background
            EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f));
            
            // Vẽ grid
            for (int x = 0; x < rect.width; x += (int)gridSize)
            {
                Handles.color = new Color(0.3f, 0.3f, 0.3f);
                Handles.DrawLine(
                    new Vector3(rect.x + x, rect.y, 0),
                    new Vector3(rect.x + x, rect.y + rect.height, 0)
                );
            }
            
            for (int y = 0; y < rect.height; y += (int)gridSize)
            {
                Handles.color = new Color(0.3f, 0.3f, 0.3f);
                Handles.DrawLine(
                    new Vector3(rect.x, rect.y + y, 0),
                    new Vector3(rect.x + rect.width, rect.y + y, 0)
                );
            }
            
            // Vẽ đường tròn tham chiếu
            Handles.color = new Color(0.4f, 0.4f, 0.4f);
            Handles.DrawWireDisc(center, Vector3.forward, radius);
            
            // Kiểm tra null
            if (behavior.directionVectors == null || behavior.directionVectors.Count == 0)
            {
                // Vẽ điểm giữa
                Handles.color = Color.yellow;
                Handles.DrawSolidDisc(center, Vector3.forward, 5);
                
                // Hiển thị thông báo
                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.alignment = TextAnchor.MiddleCenter;
                style.normal.textColor = Color.white;
                EditorGUI.LabelField(rect, "No vectors. Click to add.", style);
                
                // Xử lý click chuột để thêm vector
                Event e = Event.current;
                if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
                {
                    Vector2 direction = e.mousePosition - center;
                    if (direction.magnitude > 5) // Tránh click vào chính giữa
                    {
                        direction.Normalize();
                        AddVectorAt(behavior, new Vector3(direction.x, 0, direction.y));
                        e.Use();
                    }
                }
                
                return;
            }
            
            // Tính toán độ dài lớn nhất để scale
            float maxProb = 0.01f; // Giá trị nhỏ để tránh chia cho 0
            foreach (var vector in behavior.directionVectors)
            {
                if (vector.probability > maxProb)
                {
                    maxProb = vector.probability;
                }
            }
            
            // Vẽ từng vector
            for (int i = 0; i < behavior.directionVectors.Count; i++)
            {
                DirectionVectorConfig vector = behavior.directionVectors[i];
                
                // Vector 2D (từ XZ của vector 3D)
                Vector2 dir2D = new Vector2(vector.direction.x, vector.direction.z);
                if (dir2D.magnitude > 0)
                {
                    dir2D.Normalize();
                }
                else
                {
                    dir2D = Vector2.up; // Default nếu vector không hợp lệ
                }
                
                // Độ dài dựa trên xác suất
                float length = radius * (vector.probability / maxProb);
                
                // Điểm cuối
                Vector2 end = center + dir2D * length;
                
                // Màu sắc
                Color color = (i == selectedVectorIndex) ? Color.red : Color.green;
                Handles.color = color;
                
                // Vẽ đường vector
                Handles.DrawLine(center, end);
                
                // Vẽ đầu mũi tên
                Vector2 arrowDir = (end - center).normalized;
                Vector2 arrowLeft = end - arrowDir * 10 + new Vector2(-arrowDir.y, arrowDir.x) * 5;
                Vector2 arrowRight = end - arrowDir * 10 + new Vector2(arrowDir.y, -arrowDir.x) * 5;
                
                Handles.DrawLine(end, arrowLeft);
                Handles.DrawLine(end, arrowRight);
                
                // Vẽ nhãn
                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.normal.textColor = color;
                style.alignment = TextAnchor.MiddleCenter;
                
                // Hiển thị số index
                Handles.Label(center + dir2D * (length * 0.5f), (i + 1).ToString(), style);
            }
            
            // Vẽ điểm giữa
            Handles.color = Color.yellow;
            Handles.DrawSolidDisc(center, Vector3.forward, 5);
            
            // Xử lý sự kiện chuột
            HandleMouseEvents(rect, center, radius, behavior);
        }
        
        private void HandleMouseEvents(Rect rect, Vector2 center, float radius, BehaviorConfig behavior)
        {
            Event e = Event.current;
            
            if (!rect.Contains(e.mousePosition))
                return;
                
            // Thêm vector mới khi click
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                Vector2 direction = e.mousePosition - center;
                
                if (direction.magnitude > 5) // Tránh click vào điểm trung tâm
                {
                    // Kiểm tra xem có click vào vector nào không
                    int closestVector = -1;
                    float closestDistance = 10f; // Ngưỡng phát hiện
                    
                    for (int i = 0; i < behavior.directionVectors.Count; i++)
                    {
                        DirectionVectorConfig vector = behavior.directionVectors[i];
                        Vector2 dir2D = new Vector2(vector.direction.x, vector.direction.z).normalized;
                        Vector2 endPoint = center + dir2D * radius;
                        
                        float dist = DistancePointLine(e.mousePosition, center, endPoint);
                        if (dist < closestDistance)
                        {
                            closestDistance = dist;
                            closestVector = i;
                        }
                    }
                    
                    if (closestVector >= 0)
                    {
                        // Click vào vector hiện có -> chọn và bắt đầu kéo
                        selectedVectorIndex = closestVector;
                        isDraggingVector = true;
                        draggingVectorIndex = closestVector;
                    }
                    else
                    {
                        // Click vào khoảng trống -> tạo vector mới
                        direction.Normalize();
                        AddVectorAt(behavior, new Vector3(direction.x, 0, direction.y));
                    }
                    
                    e.Use();
                    Repaint();
                }
            }
            // Kéo vector
            else if (e.type == EventType.MouseDrag && isDraggingVector && 
                    draggingVectorIndex >= 0 && draggingVectorIndex < behavior.directionVectors.Count)
            {
                Vector2 newDirection = e.mousePosition - center;
                
                if (newDirection.magnitude > 5f)
                {
                    newDirection.Normalize();
                    
                    // Cập nhật hướng vector
                    DirectionVectorConfig vector = behavior.directionVectors[draggingVectorIndex];
                    vector.direction = new Vector3(newDirection.x, 0, newDirection.y);
                    behavior.directionVectors[draggingVectorIndex] = vector;
                    
                    e.Use();
                    Repaint();
                }
            }
            // Kết thúc kéo
            else if (e.type == EventType.MouseUp && isDraggingVector)
            {
                isDraggingVector = false;
                draggingVectorIndex = -1;
                e.Use();
            }
        }
        
        private void AddVectorAt(BehaviorConfig behavior, Vector3 direction)
        {
            DirectionVectorConfig newVector = new DirectionVectorConfig
            {
                direction = direction,
                probability = 0.5f
            };
            
            behavior.directionVectors.Add(newVector);
            selectedVectorIndex = behavior.directionVectors.Count - 1;
            
            EditorUtility.SetDirty(selectedConfig);
        }
        
        private void GenerateEvenVectors(BehaviorConfig behavior)
        {
            if (behavior.directionVectors == null)
            {
                behavior.directionVectors = new List<DirectionVectorConfig>();
            }
            
            // Xác nhận xóa vector hiện có nếu có
            if (behavior.directionVectors.Count > 0)
            {
                bool clear = EditorUtility.DisplayDialog(
                    "Clear Existing Vectors", 
                    "Do you want to clear existing vectors before generating new ones?", 
                    "Yes", "No");
                    
                if (clear)
                {
                    behavior.directionVectors.Clear();
                }
            }
            
            // Tạo vector phân bố đều
            float angleStep = vectorSpread / numberOfVectors;
            float currentAngle = startAngle;
            float equalProbability = 1.0f / numberOfVectors;
            
            for (int i = 0; i < numberOfVectors; i++)
            {
                // Chuyển đổi góc thành vector
                float radians = currentAngle * Mathf.Deg2Rad;
                Vector3 direction = new Vector3(Mathf.Sin(radians), 0, Mathf.Cos(radians));
                
                DirectionVectorConfig newVector = new DirectionVectorConfig
                {
                    direction = direction.normalized,
                    probability = equalProbability
                };
                
                behavior.directionVectors.Add(newVector);
                currentAngle += angleStep;
            }
            
            selectedVectorIndex = 0;
            EditorUtility.SetDirty(selectedConfig);
        }
        
        private void NormalizeProbabilities(BehaviorConfig behavior)
        {
            if (behavior.directionVectors == null || behavior.directionVectors.Count == 0)
                return;
                
            float total = 0f;
            
            // Tính tổng
            foreach (var vector in behavior.directionVectors)
            {
                total += vector.probability;
            }
            
            // Chuẩn hóa
            if (total > 0)
            {
                for (int i = 0; i < behavior.directionVectors.Count; i++)
                {
                    DirectionVectorConfig vector = behavior.directionVectors[i];
                    vector.probability = vector.probability / total;
                    behavior.directionVectors[i] = vector;
                }
            }
            
            EditorUtility.SetDirty(selectedConfig);
        }
        
        private float DistancePointLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            Vector2 line = lineEnd - lineStart;
            float len = line.magnitude;
            if (len < 0.0001f)
                return Vector2.Distance(point, lineStart);
                
            Vector2 v = point - lineStart;
            float d = Vector2.Dot(v, line) / len;
            d = Mathf.Clamp01(d);
            
            Vector2 closest = lineStart + line * d;
            return Vector2.Distance(point, closest);
        }
        
        // Hàm lưu cấu hình vector
        private void SaveVectorPreset(BehaviorConfig behavior, string presetName)
        {
            if (behavior.directionVectors == null || behavior.directionVectors.Count == 0)
            {
                EditorUtility.DisplayDialog("Lỗi", "Không có vector nào để lưu", "OK");
                return;
            }
            
            // Đảm bảo thư mục tồn tại
            if (!Directory.Exists(saveFolder))
            {
                Directory.CreateDirectory(saveFolder);
            }
            
            // Tạo đối tượng để lưu
            VectorPreset preset = new VectorPreset
            {
                presetName = presetName,
                behaviorType = behavior.BehaviorType.ToString(),
                vectors = new List<SerializableVector>()
            };
            
            // Thêm các vector
            foreach (var vector in behavior.directionVectors)
            {
                preset.vectors.Add(new SerializableVector
                {
                    x = vector.direction.x,
                    y = vector.direction.y,
                    z = vector.direction.z,
                    probability = vector.probability
                });
            }
            
            // Chuyển đổi thành JSON
            string json = JsonUtility.ToJson(preset, true);
            
            // Lưu file
            string filePath = Path.Combine(saveFolder, presetName + ".json");
            File.WriteAllText(filePath, json);
            
            AssetDatabase.Refresh();
            RefreshPresetList();
            
            EditorUtility.DisplayDialog("Đã Lưu", $"Preset '{presetName}' đã được lưu thành công", "OK");
        }
        
        // Hàm nạp cấu hình vector
        private void LoadVectorPreset(BehaviorConfig behavior, string presetName)
        {
            string filePath = Path.Combine(saveFolder, presetName + ".json");
            
            if (!File.Exists(filePath))
            {
                EditorUtility.DisplayDialog("Lỗi", $"Không tìm thấy preset '{presetName}'", "OK");
                return;
            }
            
            // Đọc file
            string json = File.ReadAllText(filePath);
            
            // Parse JSON
            VectorPreset preset = JsonUtility.FromJson<VectorPreset>(json);
            
            if (preset == null || preset.vectors == null || preset.vectors.Count == 0)
            {
                EditorUtility.DisplayDialog("Lỗi", "Preset không hợp lệ hoặc không có vector nào", "OK");
                return;
            }
            
            // Hiển thị xác nhận nếu behavior type khác nhau
            if (preset.behaviorType != behavior.BehaviorType.ToString())
            {
                bool proceed = EditorUtility.DisplayDialog("Cảnh Báo", 
                    $"Preset này được tạo cho behavior '{preset.behaviorType}', nhưng bạn đang áp dụng cho '{behavior.BehaviorType}'.\n\nTiếp tục?", 
                    "Tiếp Tục", "Hủy");
                    
                if (!proceed)
                {
                    return;
                }
            }
            
            // Xác nhận ghi đè
            if (behavior.directionVectors != null && behavior.directionVectors.Count > 0)
            {
                bool overwrite = EditorUtility.DisplayDialog("Xác Nhận", 
                    "Việc này sẽ thay thế tất cả các vector hiện có. Tiếp tục?", 
                    "Tiếp Tục", "Hủy");
                    
                if (!overwrite)
                {
                    return;
                }
                
                behavior.directionVectors.Clear();
            }
            else
            {
                behavior.directionVectors = new List<DirectionVectorConfig>();
            }
            
            // Thêm các vector từ preset
            foreach (var vec in preset.vectors)
            {
                behavior.directionVectors.Add(new DirectionVectorConfig
                {
                    direction = new Vector3(vec.x, vec.y, vec.z),
                    probability = vec.probability
                });
            }
            
            EditorUtility.SetDirty(selectedConfig);
            EditorUtility.DisplayDialog("Đã Nạp", $"Preset '{presetName}' đã được nạp thành công với {preset.vectors.Count} vectors", "OK");
        }
        
        // Hàm xóa preset
        private void DeleteVectorPreset(string presetName)
        {
            string filePath = Path.Combine(saveFolder, presetName + ".json");
            
            if (!File.Exists(filePath))
            {
                EditorUtility.DisplayDialog("Lỗi", $"Không tìm thấy preset '{presetName}'", "OK");
                return;
            }
            
            // Xóa file
            File.Delete(filePath);
            AssetDatabase.Refresh();
            RefreshPresetList();
            
            EditorUtility.DisplayDialog("Đã Xóa", $"Preset '{presetName}' đã được xóa", "OK");
        }
    }
    
    // Lớp để lưu trữ vector trong JSON
    [System.Serializable]
    public class SerializableVector
    {
        public float x;
        public float y;
        public float z;
        public float probability;
    }
    
    // Lớp để lưu trữ preset trong JSON
    [System.Serializable]
    public class VectorPreset
    {
        public string presetName;
        public string behaviorType;
        public List<SerializableVector> vectors;
    }
}