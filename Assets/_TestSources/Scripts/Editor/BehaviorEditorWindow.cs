using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace IslandDefense.Troops.Editor
{
    /// <summary>
    /// Enhanced editor with ability to save/load configurations and edit vectors in a separate window
    /// </summary>
    public class ImprovedBehaviorEditor : EditorWindow
    {
        private TroopConfigSO selectedConfig;
        private int selectedBehaviorIndex = -1;
        private int selectedVectorIndex = -1;
        private Vector2 scrollPosition;
        
        // Reference to vector window
        private VectorEditorWindow vectorWindow;
        
        // Vector generation options
        private int numberOfVectors = 8;
        private float vectorSpread = 180f;
        private float startAngle = -90f;
        
        // Tab
        private int currentTab = 0;
        private string[] tabNames = { "Edit", "Save/Load", "Options" };
        
        // Save folder path
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
            RefreshPresetList();
        }
        
        private void OnDisable() 
        {
            // Close vector window when main window is closed
            if (vectorWindow != null)
            {
                vectorWindow.Close();
                vectorWindow = null;
            }
        }
        
        private void RefreshPresetList()
        {
            availablePresets.Clear();
            
            // Ensure directory exists
            if (!Directory.Exists(saveFolder))
            {
                Directory.CreateDirectory(saveFolder);
                AssetDatabase.Refresh();
            }
            
            // List all JSON files
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
                EditorGUILayout.HelpBox("Please select a Troop Config", MessageType.Info);
                return;
            }
            
            switch (currentTab)
            {
                case 0: // Edit Tab
                    DrawEditTab();
                    break;
                case 1: // Save/Load Tab
                    DrawSaveLoadTab();
                    break;
                case 2: // Options Tab
                    DrawOptionsTab();
                    break;
            }
            
            // Apply changes
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
                CloseVectorWindow();
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
                EditorGUILayout.HelpBox("Please select a behavior to save/load vector configurations", MessageType.Info);
                return;
            }
            
            BehaviorConfig behavior = selectedConfig.Behaviors[selectedBehaviorIndex];
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Save/Load Vector Configuration for: {behavior.name}", EditorStyles.boldLabel);
            
            // Save configuration
            EditorGUILayout.LabelField("Save Configuration", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            currentPresetName = EditorGUILayout.TextField("Preset Name:", currentPresetName);
            
            GUI.enabled = !string.IsNullOrEmpty(currentPresetName);
            if (GUILayout.Button("Save", GUILayout.Width(80)))
            {
                SaveVectorPreset(behavior, currentPresetName);
            }
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Load configuration
            EditorGUILayout.LabelField("Load Configuration", EditorStyles.boldLabel);
            
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
                    
                    if (GUILayout.Button("Load", EditorStyles.miniButton, GUILayout.Width(60)))
                    {
                        LoadVectorPreset(behavior, preset);
                    }
                    
                    if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(20)))
                    {
                        if (EditorUtility.DisplayDialog("Delete Preset", 
                            $"Are you sure you want to delete '{preset}'?", 
                            "Delete", "Cancel"))
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
                EditorGUILayout.HelpBox("No presets saved yet", MessageType.Info);
            }
            
            if (GUILayout.Button("Refresh List"))
            {
                RefreshPresetList();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawOptionsTab()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Display Options", EditorStyles.boldLabel);
            
            // Display options moved to VectorEditorWindow
            EditorGUILayout.HelpBox("Vector display options are available in the Vector Editor window.", MessageType.Info);
            
            EditorGUILayout.Space();
            
            // Save options
            EditorGUILayout.LabelField("Save Options", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Save Folder:", GUILayout.Width(150));
            
            EditorGUILayout.LabelField(saveFolder);
            
            if (GUILayout.Button("Change", GUILayout.Width(60)))
            {
                string newPath = EditorUtility.OpenFolderPanel("Select Save Folder", Application.dataPath, "");
                
                if (!string.IsNullOrEmpty(newPath))
                {
                    // Convert to path relative to Unity project
                    if (newPath.StartsWith(Application.dataPath))
                    {
                        saveFolder = "Assets" + newPath.Substring(Application.dataPath.Length);
                        RefreshPresetList();
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Error", 
                            "Please select a folder within the Unity project", 
                            "OK");
                    }
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // Vector creation tools
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Vector Generation Tools", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Number of Vectors:", GUILayout.Width(150));
            numberOfVectors = EditorGUILayout.IntSlider(numberOfVectors, 1, 16);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Spread Angle:", GUILayout.Width(150));
            vectorSpread = EditorGUILayout.Slider(vectorSpread, 10f, 360f);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Start Angle:", GUILayout.Width(150));
            startAngle = EditorGUILayout.Slider(startAngle, -180f, 180f);
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("Generate Even Vectors"))
            {
                if (selectedBehaviorIndex >= 0 && selectedBehaviorIndex < selectedConfig.Behaviors.Count)
                {
                    GenerateEvenVectors(selectedConfig.Behaviors[selectedBehaviorIndex]);
                    UpdateVectorWindow();
                }
            }
            
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
            
            // Check for null
            if (selectedConfig.behaviors == null)
            {
                selectedConfig.behaviors = new List<BehaviorConfig>();
            }
            
            // Add all behavior types button
            if (GUILayout.Button("Add All Behavior Types"))
            {
                AddAllBehaviorTypes();
            }
            
            // List of existing behaviors
            for (int i = 0; i < selectedConfig.behaviors.Count; i++)
            {
                BehaviorConfig behavior = selectedConfig.behaviors[i];
                
                // Ensure behavior is not null
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
                
                // Select button
                bool isSelected = (selectedBehaviorIndex == i);
                GUI.color = isSelected ? Color.cyan : Color.white;
                
                if (GUILayout.Button(isSelected ? "✓" : " ", GUILayout.Width(25)))
                {
                    if (isSelected)
                    {
                        selectedBehaviorIndex = -1;
                        selectedVectorIndex = -1;
                        CloseVectorWindow();
                    }
                    else
                    {
                        selectedBehaviorIndex = i;
                        selectedVectorIndex = -1;
                        UpdateVectorWindow();
                    }
                }
                
                GUI.color = Color.white;
                
                // Behavior name
                EditorGUILayout.LabelField($"{behavior.name} ({behavior.BehaviorType})");
                
                // Probability
                behavior.Probability = EditorGUILayout.Slider(behavior.Probability, 0, 1, GUILayout.Width(100));
                
                // Delete button
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
                            selectedVectorIndex = -1;
                            CloseVectorWindow();
                        }
                        else if (selectedBehaviorIndex > i)
                        {
                            selectedBehaviorIndex--;
                            UpdateVectorWindow();
                        }
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            // Add new behavior button
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
                // Check if behavior type already exists
                bool exists = false;
                foreach (var behavior in selectedConfig.behaviors)
                {
                    if (behavior != null && behavior.BehaviorType == behaviorType)
                    {
                        exists = true;
                        break;
                    }
                }
                
                // Add behavior if it doesn't exist
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
            
            // Add default vector if needed
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
            
            // Add new behavior to list
            selectedConfig.behaviors.Add(newBehavior);
            selectedBehaviorIndex = selectedConfig.behaviors.Count - 1;
            selectedVectorIndex = -1;
            
            EditorUtility.SetDirty(selectedConfig);
            
            // Open vector window
            UpdateVectorWindow();
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
            
            // Basic info
            behavior.name = EditorGUILayout.TextField("Name", behavior.name);
            EditorGUI.BeginDisabledGroup(true); // Don't allow changing behavior type
            EditorGUILayout.EnumPopup("Type", behavior.BehaviorType);
            EditorGUI.EndDisabledGroup();
            
            behavior.Weight = EditorGUILayout.Slider("Weight", behavior.Weight, 0, 1);
            behavior.Probability = EditorGUILayout.Slider("Probability", behavior.Probability, 0, 1);
            
            // Additional parameters based on behavior type
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
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Direction Vectors", EditorStyles.boldLabel);
            
            // Open vector window button
            if (GUILayout.Button("Open Vector Editor", GUILayout.Width(200)))
            {
                OpenVectorWindow();
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Check for null and initialize if needed
            if (behavior.directionVectors == null)
            {
                behavior.directionVectors = new List<DirectionVectorConfig>();
            }
            
            // Total probability
            float totalProb = 0f;
            foreach (var vector in behavior.directionVectors)
            {
                totalProb += vector.probability;
            }
            
            // Display list
            EditorGUILayout.LabelField($"Vector count: {behavior.directionVectors.Count}");
            
            if (behavior.directionVectors.Count > 0)
            {
                // Display vector list with percentages
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                for (int i = 0; i < Mathf.Min(behavior.directionVectors.Count, 5); i++)
                {
                    var vector = behavior.directionVectors[i];
                    float percent = totalProb > 0 ? (vector.probability / totalProb) * 100f : 0f;
                    
                    EditorGUILayout.BeginHorizontal();
                    
                    bool isSelected = (selectedVectorIndex == i);
                    GUI.color = isSelected ? Color.cyan : Color.white;
                    
                    if (GUILayout.Toggle(isSelected, "", GUILayout.Width(15)) != isSelected)
                    {
                        selectedVectorIndex = isSelected ? -1 : i;
                        UpdateVectorWindow();
                    }
                    
                    GUI.color = Color.white;
                    
                    Vector3 normalized = vector.direction.normalized;
                    EditorGUILayout.LabelField($"Vec {i+1}: [{normalized.x:F2}, {normalized.y:F2}, {normalized.z:F2}] - {percent:F1}%");
                    
                    EditorGUILayout.EndHorizontal();
                }
                
                if (behavior.directionVectors.Count > 5)
                {
                    EditorGUILayout.LabelField($"...and {behavior.directionVectors.Count - 5} more vectors");
                }
                
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox("No vectors. Open Vector Editor to add some.", MessageType.Info);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        // Open vector window
        private void OpenVectorWindow()
        {
            if (selectedBehaviorIndex < 0 || selectedBehaviorIndex >= selectedConfig.behaviors.Count)
                return;
                
            BehaviorConfig behavior = selectedConfig.behaviors[selectedBehaviorIndex];
            
            // If window is already open, just update it
            if (vectorWindow != null)
            {
                vectorWindow.UpdateBehavior(behavior, selectedVectorIndex);
                vectorWindow.Focus();
                return;
            }
            
            // Open new window
            vectorWindow = VectorEditorWindow.ShowWindow(
                behavior, 
                selectedVectorIndex,
                OnVectorSelected,
                OnBehaviorModified
            );
        }
        
        // Update vector window if open
        private void UpdateVectorWindow()
        {
            if (vectorWindow == null || selectedBehaviorIndex < 0 || selectedBehaviorIndex >= selectedConfig.behaviors.Count)
                return;
                
            BehaviorConfig behavior = selectedConfig.behaviors[selectedBehaviorIndex];
            vectorWindow.UpdateBehavior(behavior, selectedVectorIndex);
        }
        
        // Close vector window
        private void CloseVectorWindow()
        {
            if (vectorWindow != null)
            {
                vectorWindow.Close();
                vectorWindow = null;
            }
        }
        
        // Handle vector selection in vector window
        private void OnVectorSelected(BehaviorConfig behavior, int vectorIndex)
        {
            // Update selected vector index
            selectedVectorIndex = vectorIndex;
            Repaint();
        }
        
        // Handle behavior modification in vector window
        private void OnBehaviorModified(BehaviorConfig behavior)
        {
            // Mark config as modified
            EditorUtility.SetDirty(selectedConfig);
            Repaint();
        }
        
        private void GenerateEvenVectors(BehaviorConfig behavior)
        {
            if (behavior.directionVectors == null)
            {
                behavior.directionVectors = new List<DirectionVectorConfig>();
            }
            
            // Confirm clearing existing vectors if any
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
            
            // Create evenly distributed vectors
            float angleStep = vectorSpread / numberOfVectors;
            float currentAngle = startAngle;
            float equalProbability = 1.0f / numberOfVectors;
            
            for (int i = 0; i < numberOfVectors; i++)
            {
                // Convert angle to vector
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
        
        // Save vector preset
        private void SaveVectorPreset(BehaviorConfig behavior, string presetName)
        {
            if (behavior.directionVectors == null || behavior.directionVectors.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", "No vectors to save", "OK");
                return;
            }
            
            // Ensure directory exists
            if (!Directory.Exists(saveFolder))
            {
                Directory.CreateDirectory(saveFolder);
            }
            
            // Create object to save
            VectorPreset preset = new VectorPreset
            {
                presetName = presetName,
                behaviorType = behavior.BehaviorType.ToString(),
                vectors = new List<SerializableVector>()
            };
            
            // Add vectors
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
            
            // Convert to JSON
            string json = JsonUtility.ToJson(preset, true);
            
            // Save file
            string filePath = Path.Combine(saveFolder, presetName + ".json");
            File.WriteAllText(filePath, json);
            
            AssetDatabase.Refresh();
            RefreshPresetList();
            
            EditorUtility.DisplayDialog("Saved", $"Preset '{presetName}' saved successfully", "OK");
        }
        
        // Load vector preset
        private void LoadVectorPreset(BehaviorConfig behavior, string presetName)
        {
            string filePath = Path.Combine(saveFolder, presetName + ".json");
            
            if (!File.Exists(filePath))
            {
                EditorUtility.DisplayDialog("Error", $"Preset '{presetName}' not found", "OK");
                return;
            }
            
            // Read file
            string json = File.ReadAllText(filePath);
            
            // Parse JSON
            VectorPreset preset = JsonUtility.FromJson<VectorPreset>(json);
            
            if (preset == null || preset.vectors == null || preset.vectors.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", "Invalid preset or no vectors", "OK");
                return;
            }
            
            // Show confirmation if behavior types differ
            if (preset.behaviorType != behavior.BehaviorType.ToString())
            {
                bool proceed = EditorUtility.DisplayDialog("Warning", 
                    $"This preset was created for '{preset.behaviorType}', but you're applying it to '{behavior.BehaviorType}'.\n\nContinue?", 
                    "Continue", "Cancel");
                    
                if (!proceed)
                {
                    return;
                }
            }
            
            // Confirm overwrite
            if (behavior.directionVectors != null && behavior.directionVectors.Count > 0)
            {
                bool overwrite = EditorUtility.DisplayDialog("Confirm", 
                    "This will replace all existing vectors. Continue?", 
                    "Continue", "Cancel");
                    
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
            
            // Add vectors from preset
            foreach (var vec in preset.vectors)
            {
                behavior.directionVectors.Add(new DirectionVectorConfig
                {
                    direction = new Vector3(vec.x, vec.y, vec.z),
                    probability = vec.probability
                });
            }
            
            EditorUtility.SetDirty(selectedConfig);
            UpdateVectorWindow();
            
            EditorUtility.DisplayDialog("Loaded", $"Preset '{presetName}' loaded successfully with {preset.vectors.Count} vectors", "OK");
        }
        
        // Delete preset
        private void DeleteVectorPreset(string presetName)
        {
            string filePath = Path.Combine(saveFolder, presetName + ".json");
            
            if (!File.Exists(filePath))
            {
                EditorUtility.DisplayDialog("Error", $"Preset '{presetName}' not found", "OK");
                return;
            }
            
            // Delete file
            File.Delete(filePath);
            AssetDatabase.Refresh();
            RefreshPresetList();
            
            EditorUtility.DisplayDialog("Deleted", $"Preset '{presetName}' has been deleted", "OK");
        }
    }
    
    // Class to store vector in JSON
    [System.Serializable]
    public class SerializableVector
    {
        public float x;
        public float y;
        public float z;
        public float probability;
    }
    
    // Class to store preset in JSON
    [System.Serializable]
    public class VectorPreset
    {
        public string presetName;
        public string behaviorType;
        public List<SerializableVector> vectors;
    }
}