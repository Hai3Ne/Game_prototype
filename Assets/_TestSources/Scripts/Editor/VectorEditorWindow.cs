using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace IslandDefense.Troops.Editor
{
    /// <summary>
    /// Dedicated window for editing direction vectors
    /// </summary>
    public class VectorEditorWindow : EditorWindow
    {
        // Reference to the behavior being edited
        private BehaviorConfig _currentBehavior;
        private int _selectedVectorIndex = -1;
        
        // Drawing variables
        private float _circleScaleFactor = 0.9f; // Default larger scale factor
        private float _gridSize = 10f;
        private Texture2D _gridTexture;
        
        // Dragging variables
        private bool _isDraggingVector = false;
        private int _draggingVectorIndex = -1;
        
        // Window size
        private readonly Vector2 _minWindowSize = new Vector2(700, 600);
        
        // Update state
        private bool _needsRepaint = false;
        
        // Scroll position for vector list
        private Vector2 _vectorListScrollPosition;
        
        // Options tab
        private int _currentTab = 0;
        private readonly string[] _tabNames = { "Vectors", "Options" };
        
        // Vector generation options
        private int _numberOfVectors = 8;
        private float _vectorSpread = 180f;
        private float _startAngle = -90f;
        
        // Callback delegates
        private System.Action<BehaviorConfig, int> _onVectorSelected;
        private System.Action<BehaviorConfig> _onBehaviorModified;
        
        // Create and show window
        public static VectorEditorWindow ShowWindow(BehaviorConfig behavior, int selectedVectorIndex,
            System.Action<BehaviorConfig, int> onVectorSelected,
            System.Action<BehaviorConfig> onBehaviorModified)
        {
            VectorEditorWindow window = GetWindow<VectorEditorWindow>("Vector Editor");
            window.minSize = window._minWindowSize;
            window._currentBehavior = behavior;
            window._selectedVectorIndex = selectedVectorIndex;
            window._onVectorSelected = onVectorSelected;
            window._onBehaviorModified = onBehaviorModified;
            window.CreateGridTexture();
            window.Show();
            return window;
        }
        
        // Update current behavior
        public void UpdateBehavior(BehaviorConfig behavior, int selectedVectorIndex)
        {
            _currentBehavior = behavior;
            _selectedVectorIndex = selectedVectorIndex;
            _needsRepaint = true;
            Repaint();
        }
        
        private void OnEnable()
        {
            CreateGridTexture();
        }
        
        private void CreateGridTexture()
        {
            _gridTexture = new Texture2D(2, 2);
            _gridTexture.SetPixel(0, 0, new Color(0.2f, 0.2f, 0.2f));
            _gridTexture.SetPixel(1, 0, new Color(0.2f, 0.2f, 0.2f));
            _gridTexture.SetPixel(0, 1, new Color(0.2f, 0.2f, 0.2f));
            _gridTexture.SetPixel(1, 1, new Color(0.2f, 0.2f, 0.2f));
            _gridTexture.Apply();
        }
        
        private void OnGUI()
        {
            if (_currentBehavior == null)
            {
                EditorGUILayout.HelpBox("No behavior selected.", MessageType.Info);
                return;
            }
            
            // Title
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Editing Vectors for: {_currentBehavior.name} ({_currentBehavior.BehaviorType})", EditorStyles.boldLabel);
            EditorGUILayout.EndVertical();
            
            // Tab selection
            _currentTab = GUILayout.Toolbar(_currentTab, _tabNames, EditorStyles.toolbarButton);
            
            EditorGUILayout.Space();
            
            // Tab content
            switch (_currentTab)
            {
                case 0: // Vectors tab
                    DrawVectorsTab();
                    break;
                case 1: // Options tab
                    DrawOptionsTab();
                    break;
            }
            
            // Apply changes
            if (GUI.changed || _needsRepaint)
            {
                _needsRepaint = false;
                _onBehaviorModified?.Invoke(_currentBehavior);
                Repaint();
            }
        }
        
        private void DrawVectorsTab()
        {
            // Split layout into two columns
            EditorGUILayout.BeginHorizontal();
            
            // Left side: Vector visualization (2/3 width)
            EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.65f));
            
            // Display options
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Display Settings", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Circle Size:", GUILayout.Width(150));
            _circleScaleFactor = EditorGUILayout.Slider(_circleScaleFactor, 0.5f, 1.0f);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Grid Size:", GUILayout.Width(150));
            _gridSize = EditorGUILayout.Slider(_gridSize, 5f, 20f);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // Vector visualization area
            DrawVectorArea();
            
            EditorGUILayout.Space();
            
            // Tools section
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Add Vector", GUILayout.Width(120)))
            {
                AddVectorAt(new Vector3(0, 0, 1));
            }
            
            if (GUILayout.Button("Normalize Probabilities", GUILayout.Width(180)))
            {
                NormalizeProbabilities();
            }
            
            if (GUILayout.Button("Clear All Vectors", GUILayout.Width(120)))
            {
                ClearAllVectors();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            
            // Right side: Vector list and details (1/3 width)
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(position.width * 0.33f));
            
            // Vector list section
            EditorGUILayout.LabelField("Vector List", EditorStyles.boldLabel);
            
            if (_currentBehavior.directionVectors == null || _currentBehavior.directionVectors.Count == 0)
            {
                EditorGUILayout.HelpBox("No vectors yet. Click in the circle to add vectors.", MessageType.Info);
            }
            else
            {
                // Calculate total probability for percentage display
                float totalProb = 0f;
                foreach (var vector in _currentBehavior.directionVectors)
                {
                    totalProb += vector.probability;
                }
                
                // Draw vector list with scrolling
                _vectorListScrollPosition = EditorGUILayout.BeginScrollView(_vectorListScrollPosition, GUILayout.Height(250));
                
                for (int i = 0; i < _currentBehavior.directionVectors.Count; i++)
                {
                    DirectionVectorConfig vector = _currentBehavior.directionVectors[i];
                    float percent = totalProb > 0 ? (vector.probability / totalProb) * 100f : 0f;
                    
                    EditorGUILayout.BeginHorizontal(
                        _selectedVectorIndex == i ? EditorStyles.helpBox : EditorStyles.label);
                    
                    // Select button
                    bool isSelected = (_selectedVectorIndex == i);
                    GUI.color = isSelected ? Color.cyan : Color.white;
                    
                    if (GUILayout.Toggle(isSelected, "", GUILayout.Width(20)) != isSelected)
                    {
                        _selectedVectorIndex = isSelected ? -1 : i;
                        _onVectorSelected?.Invoke(_currentBehavior, _selectedVectorIndex);
                    }
                    
                    GUI.color = Color.white;
                    
                    // Vector info
                    Vector3 normalized = vector.direction.normalized;
                    EditorGUILayout.LabelField($"#{i+1}: [{normalized.x:F2}, {normalized.z:F2}] - {percent:F1}%", 
                        GUILayout.Width(160));
                    
                    // Delete button
                    if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(20)))
                    {
                        if (EditorUtility.DisplayDialog("Delete Vector", 
                            $"Are you sure you want to delete vector #{i+1}?", 
                            "Delete", "Cancel"))
                        {
                            _currentBehavior.directionVectors.RemoveAt(i);
                            if (_selectedVectorIndex == i)
                            {
                                _selectedVectorIndex = -1;
                                _onVectorSelected?.Invoke(_currentBehavior, _selectedVectorIndex);
                            }
                            else if (_selectedVectorIndex > i)
                            {
                                _selectedVectorIndex--;
                                _onVectorSelected?.Invoke(_currentBehavior, _selectedVectorIndex);
                            }
                            _onBehaviorModified?.Invoke(_currentBehavior);
                            break;
                        }
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndScrollView();
            }
            
            EditorGUILayout.Space();
            
            // Selected vector details
            if (_selectedVectorIndex >= 0 && _selectedVectorIndex < _currentBehavior.directionVectors.Count)
            {
                DrawSelectedVectorDetails();
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawOptionsTab()
        {
            EditorGUILayout.BeginVertical();
            
            // Vector generation settings
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Vector Generation Settings", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Number of Vectors:", GUILayout.Width(150));
            _numberOfVectors = EditorGUILayout.IntSlider(_numberOfVectors, 1, 16);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Spread Angle:", GUILayout.Width(150));
            _vectorSpread = EditorGUILayout.Slider(_vectorSpread, 10f, 360f);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Start Angle:", GUILayout.Width(150));
            _startAngle = EditorGUILayout.Slider(_startAngle, -180f, 180f);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Generation buttons
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Generate Even Vectors", GUILayout.Height(30)))
            {
                GenerateEvenVectors();
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Generation patterns
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quick Patterns", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Forward Only", GUILayout.Height(25)))
            {
                GenerateForwardVector();
            }
            
            if (GUILayout.Button("Forward Fan", GUILayout.Height(25)))
            {
                GenerateForwardFan();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Circle", GUILayout.Height(25)))
            {
                _numberOfVectors = 8;
                _vectorSpread = 360f;
                _startAngle = 0f;
                GenerateEvenVectors();
            }
            
            if (GUILayout.Button("Forward Right", GUILayout.Height(25)))
            {
                GenerateForwardRightPattern();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // Behavior-specific options
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Behavior-Specific Settings ({_currentBehavior.BehaviorType})", EditorStyles.boldLabel);
            
            // Additional parameters based on behavior type
            switch (_currentBehavior.BehaviorType)
            {
                case BehaviorType.Seek:
                // case BehaviorType.Flee:
                case BehaviorType.Arrival:
                    _currentBehavior.ArrivalRadius = EditorGUILayout.FloatField("Arrival Radius", _currentBehavior.ArrivalRadius);
                    _currentBehavior.SlowingRadius = EditorGUILayout.FloatField("Slowing Radius", _currentBehavior.SlowingRadius);
                    break;
                
                case BehaviorType.Separation:
                case BehaviorType.Cohesion:
                case BehaviorType.Alignment:
                    _currentBehavior.NeighborRadius = EditorGUILayout.FloatField("Neighbor Radius", _currentBehavior.NeighborRadius);
                    _currentBehavior.SeparationRadius = EditorGUILayout.FloatField("Separation Radius", _currentBehavior.SeparationRadius);
                    break;
                
                case BehaviorType.Attack:
                    EditorGUILayout.HelpBox("Attack vectors determine how troops approach and position themselves for attacking enemies.", MessageType.Info);
                    break;
                
                case BehaviorType.Flee:
                    EditorGUILayout.HelpBox("Flee vectors determine escape directions when troops are in danger.", MessageType.Info);
                    break;
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawVectorArea()
        {
            // Get drawing area - take most of the window
            float size = Mathf.Min(position.width * 0.65f, position.height - 200) - 20;
            Rect rect = GUILayoutUtility.GetRect(size, size);
            
            // Center point
            Vector2 center = new Vector2(rect.x + rect.width / 2, rect.y + rect.height / 2);
            float radius = (rect.width / 2 - 10) * _circleScaleFactor;
            
            // Draw background
            EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f));
            
            // Draw grid
            for (int x = 0; x < rect.width; x += (int)_gridSize)
            {
                Handles.color = new Color(0.3f, 0.3f, 0.3f);
                Handles.DrawLine(
                    new Vector3(rect.x + x, rect.y, 0),
                    new Vector3(rect.x + x, rect.y + rect.height, 0)
                );
            }
            
            for (int y = 0; y < rect.height; y += (int)_gridSize)
            {
                Handles.color = new Color(0.3f, 0.3f, 0.3f);
                Handles.DrawLine(
                    new Vector3(rect.x, rect.y + y, 0),
                    new Vector3(rect.x + rect.width, rect.y + y, 0)
                );
            }
            
            // Draw attack range circle
            Handles.color = new Color(0.4f, 0.4f, 0.4f);
            Handles.DrawWireDisc(center, Vector3.forward, radius);
            
            // Draw coordinates axes
            Handles.color = new Color(0.6f, 0.2f, 0.2f); // X-axis - light red
            Handles.DrawLine(
                new Vector3(center.x - radius, center.y, 0),
                new Vector3(center.x + radius, center.y, 0)
            );
            
            Handles.color = new Color(0.2f, 0.6f, 0.2f); // Z-axis - light green
            Handles.DrawLine(
                new Vector3(center.x, center.y - radius, 0),
                new Vector3(center.x, center.y + radius, 0)
            );
            
            // Draw axis labels
            GUIStyle axisStyle = new GUIStyle(GUI.skin.label);
            axisStyle.normal.textColor = Color.white;
            axisStyle.alignment = TextAnchor.MiddleCenter;
            
            // X and Z axis labels
            GUI.Label(new Rect(center.x + radius + 5, center.y - 10, 20, 20), "X", axisStyle);
            GUI.Label(new Rect(center.x - 10, center.y - radius - 20, 20, 20), "Z", axisStyle);
            
            // Draw troop attack range text
            GUIStyle rangeStyle = new GUIStyle(GUI.skin.label);
            rangeStyle.normal.textColor = Color.yellow;
            rangeStyle.alignment = TextAnchor.MiddleCenter;
            
            GUI.Label(new Rect(rect.x, rect.y + rect.height - 20, rect.width, 20), 
                "Circle represents attack range reference", rangeStyle);
            
            // Check for empty vector list
            if (_currentBehavior.directionVectors == null || _currentBehavior.directionVectors.Count == 0)
            {
                // Draw center point
                Handles.color = Color.yellow;
                Handles.DrawSolidDisc(center, Vector3.forward, 5);
                
                // Display message
                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.alignment = TextAnchor.MiddleCenter;
                style.normal.textColor = Color.white;
                EditorGUI.LabelField(rect, "No vectors. Click to add a new vector.", style);
                
                // Handle mouse clicks to add vector
                Event e = Event.current;
                if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
                {
                    Vector2 direction = e.mousePosition - center;
                    if (direction.magnitude > 5) // Avoid clicks at the center
                    {
                        direction.Normalize();
                        AddVectorAt(new Vector3(direction.x, 0, direction.y));
                        e.Use();
                    }
                }
                
                return;
            }
            
            // Calculate max probability for scaling
            float maxProb = 0.01f; // Small value to avoid division by zero
            foreach (var vector in _currentBehavior.directionVectors)
            {
                if (vector.probability > maxProb)
                {
                    maxProb = vector.probability;
                }
            }
            
            // Draw each vector
            for (int i = 0; i < _currentBehavior.directionVectors.Count; i++)
            {
                DirectionVectorConfig vector = _currentBehavior.directionVectors[i];
                
                // 2D vector (from XZ of 3D vector)
                Vector2 dir2D = new Vector2(vector.direction.x, vector.direction.z);
                if (dir2D.magnitude > 0)
                {
                    dir2D.Normalize();
                }
                else
                {
                    dir2D = Vector2.up; // Default if vector is invalid
                }
                
                // Length based on probability
                float length = radius * (vector.probability / maxProb);
                
                // End point
                Vector2 end = center + dir2D * length;
                
                // Color
                Color color = (i == _selectedVectorIndex) ? Color.red : Color.green;
                Handles.color = color;
                
                // Draw vector line
                Handles.DrawLine(center, end);
                
                // Draw arrow head
                Vector2 arrowDir = (end - center).normalized;
                Vector2 arrowLeft = end - arrowDir * 10 + new Vector2(-arrowDir.y, arrowDir.x) * 5;
                Vector2 arrowRight = end - arrowDir * 10 + new Vector2(arrowDir.y, -arrowDir.x) * 5;
                
                Handles.DrawLine(end, arrowLeft);
                Handles.DrawLine(end, arrowRight);
                
                // Draw label
                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.normal.textColor = color;
                style.alignment = TextAnchor.MiddleCenter;
                
                // Display index number
                Handles.Label(center + dir2D * (length * 0.5f), (i + 1).ToString(), style);
                
                // Display percentage at arrow tip
                float percent = (maxProb > 0) ? (vector.probability / maxProb) * 100f : 0f;
                GUIStyle percentStyle = new GUIStyle(GUI.skin.label);
                percentStyle.normal.textColor = color;
                percentStyle.alignment = TextAnchor.MiddleLeft;
                Handles.Label(end + arrowDir * 5, $"{vector.probability:F2} ({percent:F0}%)", percentStyle);
            }
            
            // Draw center point
            Handles.color = Color.yellow;
            Handles.DrawSolidDisc(center, Vector3.forward, 5);
            
            // Handle mouse events
            HandleMouseEvents(rect, center, radius);
        }
        
        private void DrawSelectedVectorDetails()
        {
            DirectionVectorConfig vector = _currentBehavior.directionVectors[_selectedVectorIndex];
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Vector #{_selectedVectorIndex + 1} Details", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Direction:", GUILayout.Width(80));
            
            Vector3 newDir = EditorGUILayout.Vector3Field("", vector.direction);
            if (newDir != vector.direction)
            {
                vector.direction = newDir;
                _currentBehavior.directionVectors[_selectedVectorIndex] = vector;
            }
            
            if (GUILayout.Button("Normalize", GUILayout.Width(80)))
            {
                vector.direction = vector.direction.normalized;
                _currentBehavior.directionVectors[_selectedVectorIndex] = vector;
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Probability:", GUILayout.Width(80));
            
            float newProb = EditorGUILayout.Slider(vector.probability, 0f, 1f);
            if (newProb != vector.probability)
            {
                vector.probability = newProb;
                _currentBehavior.directionVectors[_selectedVectorIndex] = vector;
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Delete button
            if (GUILayout.Button("Delete Vector"))
            {
                if (EditorUtility.DisplayDialog("Delete Vector", 
                    $"Are you sure you want to delete vector #{_selectedVectorIndex + 1}?", 
                    "Delete", "Cancel"))
                {
                    _currentBehavior.directionVectors.RemoveAt(_selectedVectorIndex);
                    _selectedVectorIndex = -1;
                    _onVectorSelected?.Invoke(_currentBehavior, _selectedVectorIndex);
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void HandleMouseEvents(Rect rect, Vector2 center, float radius)
        {
            Event e = Event.current;
            
            if (!rect.Contains(e.mousePosition))
                return;
                
            // Add new vector on click
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                Vector2 direction = e.mousePosition - center;
                
                if (direction.magnitude > 5) // Avoid clicks at the center
                {
                    // Check if clicked on an existing vector
                    int closestVector = -1;
                    float closestDistance = 10f; // Detection threshold
                    
                    for (int i = 0; i < _currentBehavior.directionVectors.Count; i++)
                    {
                        DirectionVectorConfig vector = _currentBehavior.directionVectors[i];
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
                        // Clicked on existing vector -> select and start dragging
                        _selectedVectorIndex = closestVector;
                        _isDraggingVector = true;
                        _draggingVectorIndex = closestVector;
                        _onVectorSelected?.Invoke(_currentBehavior, _selectedVectorIndex);
                    }
                    else
                    {
                        // Clicked on empty space -> create new vector
                        direction.Normalize();
                        AddVectorAt(new Vector3(direction.x, 0, direction.y));
                    }
                    
                    e.Use();
                    Repaint();
                }
            }
            // Drag vector
            else if (e.type == EventType.MouseDrag && _isDraggingVector && 
                    _draggingVectorIndex >= 0 && _draggingVectorIndex < _currentBehavior.directionVectors.Count)
            {
                Vector2 newDirection = e.mousePosition - center;
                
                if (newDirection.magnitude > 5f)
                {
                    newDirection.Normalize();
                    
                    // Update vector direction
                    DirectionVectorConfig vector = _currentBehavior.directionVectors[_draggingVectorIndex];
                    vector.direction = new Vector3(newDirection.x, 0, newDirection.y);
                    _currentBehavior.directionVectors[_draggingVectorIndex] = vector;
                    
                    e.Use();
                    Repaint();
                }
            }
            // End dragging
            else if (e.type == EventType.MouseUp && _isDraggingVector)
            {
                _isDraggingVector = false;
                _draggingVectorIndex = -1;
                e.Use();
            }
        }
        
        private void AddVectorAt(Vector3 direction)
        {
            if (_currentBehavior.directionVectors == null)
            {
                _currentBehavior.directionVectors = new List<DirectionVectorConfig>();
            }
            
            DirectionVectorConfig newVector = new DirectionVectorConfig
            {
                direction = direction,
                probability = 0.5f
            };
            
            _currentBehavior.directionVectors.Add(newVector);
            _selectedVectorIndex = _currentBehavior.directionVectors.Count - 1;
            _onVectorSelected?.Invoke(_currentBehavior, _selectedVectorIndex);
        }
        
        private void NormalizeProbabilities()
        {
            if (_currentBehavior.directionVectors == null || _currentBehavior.directionVectors.Count == 0)
                return;
                
            float total = 0f;
            
            // Calculate total
            foreach (var vector in _currentBehavior.directionVectors)
            {
                total += vector.probability;
            }
            
            // Normalize
            if (total > 0)
            {
                for (int i = 0; i < _currentBehavior.directionVectors.Count; i++)
                {
                    DirectionVectorConfig vector = _currentBehavior.directionVectors[i];
                    vector.probability = vector.probability / total;
                    _currentBehavior.directionVectors[i] = vector;
                }
            }
            
            _onBehaviorModified?.Invoke(_currentBehavior);
        }
        
        private void ClearAllVectors()
        {
            if (_currentBehavior.directionVectors == null || _currentBehavior.directionVectors.Count == 0)
                return;
                
            if (EditorUtility.DisplayDialog("Clear All Vectors", 
                "Are you sure you want to delete all vectors?", 
                "Clear All", "Cancel"))
            {
                _currentBehavior.directionVectors.Clear();
                _selectedVectorIndex = -1;
                _onVectorSelected?.Invoke(_currentBehavior, _selectedVectorIndex);
                _onBehaviorModified?.Invoke(_currentBehavior);
            }
        }
        
        private void GenerateEvenVectors()
        {
            if (_currentBehavior.directionVectors == null)
            {
                _currentBehavior.directionVectors = new List<DirectionVectorConfig>();
            }
            
            // Confirm clearing existing vectors if any
            if (_currentBehavior.directionVectors.Count > 0)
            {
                bool clear = EditorUtility.DisplayDialog(
                    "Clear Existing Vectors", 
                    "Do you want to clear existing vectors before generating new ones?", 
                    "Yes", "No");
                    
                if (clear)
                {
                    _currentBehavior.directionVectors.Clear();
                }
            }
            
            // Create evenly distributed vectors
            float angleStep = _vectorSpread / _numberOfVectors;
            float currentAngle = _startAngle;
            float equalProbability = 1.0f / _numberOfVectors;
            
            for (int i = 0; i < _numberOfVectors; i++)
            {
                // Convert angle to vector
                float radians = currentAngle * Mathf.Deg2Rad;
                Vector3 direction = new Vector3(Mathf.Sin(radians), 0, Mathf.Cos(radians));
                
                DirectionVectorConfig newVector = new DirectionVectorConfig
                {
                    direction = direction.normalized,
                    probability = equalProbability
                };
                
                _currentBehavior.directionVectors.Add(newVector);
                currentAngle += angleStep;
            }
            
            _selectedVectorIndex = 0;
            _onVectorSelected?.Invoke(_currentBehavior, _selectedVectorIndex);
            _onBehaviorModified?.Invoke(_currentBehavior);
        }
        
        private void GenerateForwardVector()
        {
            if (_currentBehavior.directionVectors == null)
            {
                _currentBehavior.directionVectors = new List<DirectionVectorConfig>();
            }
            
            // Confirm clearing existing vectors if any
            if (_currentBehavior.directionVectors.Count > 0)
            {
                bool clear = EditorUtility.DisplayDialog(
                    "Clear Existing Vectors", 
                    "Do you want to clear existing vectors before generating new pattern?", 
                    "Yes", "No");
                    
                if (clear)
                {
                    _currentBehavior.directionVectors.Clear();
                }
            }
            
            // Create one forward vector
            DirectionVectorConfig newVector = new DirectionVectorConfig
            {
                direction = new Vector3(0, 0, 1), // Forward
                probability = 1.0f
            };
            
            _currentBehavior.directionVectors.Add(newVector);
            _selectedVectorIndex = 0;
            _onVectorSelected?.Invoke(_currentBehavior, _selectedVectorIndex);
            _onBehaviorModified?.Invoke(_currentBehavior);
        }
        
        private void GenerateForwardFan()
        {
            if (_currentBehavior.directionVectors == null)
            {
                _currentBehavior.directionVectors = new List<DirectionVectorConfig>();
            }
            
            // Confirm clearing existing vectors if any
            if (_currentBehavior.directionVectors.Count > 0)
            {
                bool clear = EditorUtility.DisplayDialog(
                    "Clear Existing Vectors", 
                    "Do you want to clear existing vectors before generating new pattern?", 
                    "Yes", "No");
                    
                if (clear)
                {
                    _currentBehavior.directionVectors.Clear();
                }
            }
            
            // Create a forward fan with 3 vectors
            // Center vector (highest probability)
            _currentBehavior.directionVectors.Add(new DirectionVectorConfig
            {
                direction = new Vector3(0, 0, 1), // Forward
                probability = 0.6f
            });
            
            // Left vector
            _currentBehavior.directionVectors.Add(new DirectionVectorConfig
            {
                direction = new Vector3(-0.3f, 0, 0.95f).normalized, // Forward-left
                probability = 0.2f
            });
            
            // Right vector
            _currentBehavior.directionVectors.Add(new DirectionVectorConfig
            {
                direction = new Vector3(0.3f, 0, 0.95f).normalized, // Forward-right
                probability = 0.2f
            });
            
            _selectedVectorIndex = 0;
            _onVectorSelected?.Invoke(_currentBehavior, _selectedVectorIndex);
            _onBehaviorModified?.Invoke(_currentBehavior);
        }
        
        private void GenerateForwardRightPattern()
        {
            if (_currentBehavior.directionVectors == null)
            {
                _currentBehavior.directionVectors = new List<DirectionVectorConfig>();
            }
            
            // Confirm clearing existing vectors if any
            if (_currentBehavior.directionVectors.Count > 0)
            {
                bool clear = EditorUtility.DisplayDialog(
                    "Clear Existing Vectors", 
                    "Do you want to clear existing vectors before generating new pattern?", 
                    "Yes", "No");
                    
                if (clear)
                {
                    _currentBehavior.directionVectors.Clear();
                }
            }
            
            // Forward vector (highest probability)
            _currentBehavior.directionVectors.Add(new DirectionVectorConfig
            {
                direction = new Vector3(0, 0, 1), // Forward
                probability = 0.5f
            });
            
            // Slightly right
            _currentBehavior.directionVectors.Add(new DirectionVectorConfig
            {
                direction = new Vector3(0.3f, 0, 0.95f).normalized, // Forward-right
                probability = 0.3f
            });
            
            // Hard right
            _currentBehavior.directionVectors.Add(new DirectionVectorConfig
            {
                direction = new Vector3(0.7f, 0, 0.7f).normalized, // Right diagonal
                probability = 0.2f
            });
            
            _selectedVectorIndex = 0;
            _onVectorSelected?.Invoke(_currentBehavior, _selectedVectorIndex);
            _onBehaviorModified?.Invoke(_currentBehavior);
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
    }
}