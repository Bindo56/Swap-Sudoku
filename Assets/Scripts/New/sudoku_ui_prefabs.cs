using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SudokuUISetup : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private SudokuPuzzleManager puzzleManager;
    [SerializeField] private SudukoGrid sudokuGrid;
    
    [Header("Loading Panel")]
    [SerializeField] private GameObject loadingPanelPrefab;
    [SerializeField] private Transform canvasTransform;
    
    [Header("Level Select Panel")]
    [SerializeField] private GameObject levelSelectPanelPrefab;
    [SerializeField] private GameObject levelButtonPrefab;
    
    private void Awake()
    {
        SetupUI();
    }
    
    private void SetupUI()
    {
        // Create loading panel
        GameObject loadingPanel = Instantiate(loadingPanelPrefab, canvasTransform);
        TextMeshProUGUI loadingText = loadingPanel.GetComponentInChildren<TextMeshProUGUI>();
        loadingText.text = "Generating puzzles...";
        
        // Create level select panel
        GameObject levelSelectPanel = Instantiate(levelSelectPanelPrefab, canvasTransform);
        Transform levelButtonContainer = levelSelectPanel.transform.Find("ButtonContainer");
        
        // Set references
        if (puzzleManager != null)
        {
            // Access via reflection to set private serialized fields
            System.Type type = puzzleManager.GetType();
            
            type.GetField("loadingPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(puzzleManager, loadingPanel);
                
            type.GetField("loadingText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(puzzleManager, loadingText);
                
            type.GetField("levelSelectPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(puzzleManager, levelSelectPanel);
                
            type.GetField("levelButtonPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(puzzleManager, levelButtonPrefab);
                
            type.GetField("levelButtonContainer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(puzzleManager, levelButtonContainer);
                
            type.GetField("sudokuGrid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(puzzleManager, sudokuGrid);
        }
        
        // Hide sudoku grid initially
        if (sudokuGrid != null)
        {
           // sudokuGrid.gameObject.SetActive(false);
            
            // Set reference to puzzle manager
            System.Type gridType = sudokuGrid.GetType();
            gridType.GetField("puzzleManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(sudokuGrid, puzzleManager);
        }
    }
}
