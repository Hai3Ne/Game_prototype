using UnityEngine;
using UnityEngine.UI;
using IslandDefense.Core;
using IslandDefense.Troops;

public class TroopCreator : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Button createSoldierButton;
    [SerializeField] private Button createArcherButton;
    [SerializeField] private Button createCavalryButton;
    [SerializeField] private TroopConfigSO soldierConfig;
    [SerializeField] private TroopConfigSO archerConfig;
    [SerializeField] private TroopConfigSO cavalryConfig;
    
    private void Start()
    {
        if (createSoldierButton != null)
            createSoldierButton.onClick.AddListener(CreateSoldierSquad);
            
        if (createArcherButton != null)
            createArcherButton.onClick.AddListener(CreateArcherSquad);
            
        if (createCavalryButton != null)
            createCavalryButton.onClick.AddListener(CreateCavalrySquad);
    }
    
    public void CreateSoldierSquad()
    {
        if (gameManager != null)
        {
            Vector3 position = new Vector3(0, 0, 0); // Vị trí mặc định, thay đổi cho phù hợp
            gameManager.CreateSquad(position, FormationType.Square, soldierConfig, 9);
        }
    }
    
    public void CreateArcherSquad()
    {
        if (gameManager != null)
        {
            Vector3 position = new Vector3(5, 0, 0); // Vị trí mặc định, thay đổi cho phù hợp
            gameManager.CreateSquad(position, FormationType.Line, archerConfig, 9);
        }
    }
    
    public void CreateCavalrySquad()
    {
        if (gameManager != null)
        {
            Vector3 position = new Vector3(-5, 0, 0); // Vị trí mặc định, thay đổi cho phù hợp
            gameManager.CreateSquad(position, FormationType.V, cavalryConfig, 9);
        }
    }
}