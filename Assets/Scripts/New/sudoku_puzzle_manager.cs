using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.UI;
using System;

public class SudokuPuzzleManager : MonoBehaviour
{
    [SerializeField] private SudukoGrid sudokuGrid;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private GameObject levelSelectPanel;
    [SerializeField] private GameObject levelButtonPrefab;
    [SerializeField] private Transform levelButtonContainer;
    
    private const int TOTAL_PUZZLES = 100;
    private const string SAVE_FILE_NAME = "sudoku_puzzles.json";
    private const string PROGRESS_FILE_NAME = "sudoku_progress.json";
    
    private List<SudokuPuzzleData> puzzles = new List<SudokuPuzzleData>();
    private SudokuProgressData progressData;
    
    [System.Serializable]
    public class SudokuPuzzleData
    {
        public int levelNumber; //testing
        public int[,] solution;
        public List<FixedCellData> fixedCells;
        public int initialSwaps;
        
        [System.Serializable]
        public class FixedCellData
        {
            public int row;
            public int col;
            public int value;
            
            public FixedCellData(int row, int col, int value)
            {
                this.row = row;
                this.col = col;
                this.value = value;
            }
        }
    }
    
    [System.Serializable]
    public class SudokuProgressData
    {
        public List<LevelStatus> levels = new List<LevelStatus>();
        
        [System.Serializable]
        public class LevelStatus
        {
            public int levelNumber;
            public bool completed;
            
            public LevelStatus(int levelNumber, bool completed)
            {
                this.levelNumber = levelNumber;
                this.completed = completed;
            }
        }
    }
    
    private void Start()
    {
        sudokuGrid = SudukoGrid.instance;
        // Check if puzzles already exist, otherwise generate them
        if (!DoPuzzlesExist())
        {
           
            StartCoroutine(GenerateAndSavePuzzles());
        }
        else
        {
            LoadPuzzlesAndProgress();
            ShowLevelSelect();
        }
    }
    
    private bool DoPuzzlesExist()
    {
        string filePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        return File.Exists(filePath);
    }
    
    private IEnumerator GenerateAndSavePuzzles()
    {
        loadingPanel.SetActive(true);
        levelSelectPanel.SetActive(false);
        
        puzzles.Clear();
        
        for (int i = 0; i < TOTAL_PUZZLES; i++)
        {
            // Update loading text
            loadingText.text = $"Generating puzzles: {i+1}/{TOTAL_PUZZLES}";
            
            // Create puzzle
            SudokuPuzzleData puzzle = GeneratePuzzle(i+1);
            puzzles.Add(puzzle);
            
            // Yield to prevent freezing the UI
            if (i % 5 == 0)
            {
                yield return null;
            }
        }
        
        loadingText.text = "Saving puzzles...";
        yield return null;
        
        SavePuzzlesToJson();
        InitializeProgressData();
        
        loadingPanel.SetActive(false);
        ShowLevelSelect();
    }
    
    private SudokuPuzzleData GeneratePuzzle(int levelNumber)
    {
        SudokuPuzzleData puzzle = new SudokuPuzzleData();
        puzzle.levelNumber = levelNumber;
        
        // Generate a full Sudoku solution
        puzzle.solution = GenerateFullSolution();
        
        // Create fixed cells (the difficulty increases with level)
        int fixedCellsCount = Mathf.Max(20, 40 - (levelNumber / 10)); // Between 20-40 fixed cells
        puzzle.fixedCells = GenerateFixedCells(puzzle.solution, fixedCellsCount);
        
        // Set initial swaps based on level difficulty
        puzzle.initialSwaps = 30 + (levelNumber / 5); // Between 30-50 swaps
        
        return puzzle;
    }
    
    private List<SudokuPuzzleData.FixedCellData> GenerateFixedCells(int[,] solution, int fixedCellsCount)
    {
        List<SudokuPuzzleData.FixedCellData> fixedCells = new List<SudokuPuzzleData.FixedCellData>();
        HashSet<(int, int)> usedPositions = new HashSet<(int, int)>();
        Dictionary<int, HashSet<int>> rowFixedNumbers = new Dictionary<int, HashSet<int>>();
        Dictionary<int, HashSet<int>> colFixedNumbers = new Dictionary<int, HashSet<int>>();
        
        for (int i = 0; i < 9; i++)
        {
            rowFixedNumbers[i] = new HashSet<int>();
            colFixedNumbers[i] = new HashSet<int>();
        }
        
        System.Random rand = new System.Random();
        
        while (fixedCells.Count < fixedCellsCount)
        {
            int row = rand.Next(9);
            int col = rand.Next(9);
            int value = solution[row, col];
            
            if (!usedPositions.Contains((row, col)) && 
                !rowFixedNumbers[row].Contains(value) && 
                !colFixedNumbers[col].Contains(value))
            {
                usedPositions.Add((row, col));
                rowFixedNumbers[row].Add(value);
                colFixedNumbers[col].Add(value);
                
                fixedCells.Add(new SudokuPuzzleData.FixedCellData(row, col, value));
            }
        }
        
        return fixedCells;
    }
    
    private int[,] GenerateFullSolution()
    {
        int[,] board = new int[9, 9];
        FillBoard(board);
        return board;
    }
    
    private bool FillBoard(int[,] board)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (board[row, col] == 0)
                {
                    foreach (int num in GetShuffledNumbers())
                    {
                        if (IsValidNumber(board, row, col, num))
                        {
                            board[row, col] = num;
                            if (FillBoard(board))
                                return true;
                            board[row, col] = 0;
                        }
                    }
                    return false;
                }
            }
        }
        return true;
    }
    
    private bool IsValidNumber(int[,] board, int row, int col, int num)
    {
        // Check row and column
        for (int i = 0; i < 9; i++)
        {
            if (board[row, i] == num || board[i, col] == num)
                return false;
        }
        
        // Check 3x3 block
        int startRow = (row / 3) * 3;
        int startCol = (col / 3) * 3;
        
        for (int r = startRow; r < startRow + 3; r++)
        {
            for (int c = startCol; c < startCol + 3; c++)
            {
                if (board[r, c] == num)
                    return false;
            }
        }
        
        return true;
    }
    
    private List<int> GetShuffledNumbers()
    {
        List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        System.Random rng = new System.Random();
        
        for (int i = numbers.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            int temp = numbers[i];
            numbers[i] = numbers[j];
            numbers[j] = temp;
        }
        
        return numbers;
    }
    
    private void SavePuzzlesToJson()
    {
        string filePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        string json = JsonConvert.SerializeObject(puzzles, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented
        });
        
        File.WriteAllText(filePath, json);
        Debug.Log($"Saved puzzles to {filePath}");
    }
    
    private void InitializeProgressData()
    {
        progressData = new SudokuProgressData();
        
        for (int i = 1; i <= TOTAL_PUZZLES; i++)
        {
            Debug.Log(i);
            progressData.levels.Add(new SudokuProgressData.LevelStatus(i, false));
        }
        
        SaveProgressData();
    }
    
    private void SaveProgressData()
    {
        string filePath = Path.Combine(Application.persistentDataPath, PROGRESS_FILE_NAME);
        string json = JsonConvert.SerializeObject(progressData, new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        });
        
        File.WriteAllText(filePath, json);
        Debug.Log($"Saved progress to {filePath}");
    }
    
    private void LoadPuzzlesAndProgress()
    {
        string puzzlesPath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        string progressPath = Path.Combine(Application.persistentDataPath, PROGRESS_FILE_NAME);
        
        if (File.Exists(puzzlesPath))
        {
            string json = File.ReadAllText(puzzlesPath);
            puzzles = JsonConvert.DeserializeObject<List<SudokuPuzzleData>>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
            Debug.Log($"Loaded {puzzles.Count} puzzles");
        }
        
        if (File.Exists(progressPath))
        {
            string json = File.ReadAllText(progressPath);
            progressData = JsonConvert.DeserializeObject<SudokuProgressData>(json);
            Debug.Log("Loaded progress data");
        }
        else
        {
            InitializeProgressData();
        }
    }
    
    private void ShowLevelSelect()
    {
        levelSelectPanel.SetActive(true);
        
        // Clear existing buttons
       /* foreach (Transform child in levelButtonContainer)
        {
            Destroy(child.gameObject);
        }*/
        
        // Create level buttons
        Debug.Log(progressData.levels.Count + "LevelBtn");
        for (int i = 0; i < progressData.levels.Count; i++)
        {
            SudokuProgressData.LevelStatus levelStatus = progressData.levels[i];
            
            GameObject buttonObj = Instantiate(levelButtonPrefab, levelSelectPanel.transform);
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            
            // Set level number
            buttonText.text = $"Level {levelStatus.levelNumber}";
            
            // Change color if completed
            if (levelStatus.completed)
            {
                buttonText.color = Color.green;
            }
            
            // Enable or disable based on availability
            bool isAvailable = levelStatus.levelNumber == 1 || 
                               (levelStatus.levelNumber > 1 && progressData.levels[i-1].completed);
          //  button.interactable = isAvailable;// chganges
            
            // Store level number and set click handler
            int levelNumber = levelStatus.levelNumber;

            button.onClick.AddListener(() => LoadLevel(levelNumber));
        }
    }
    
    public void LoadLevel(int levelNumber)
    {
        // Find the puzzle
        SudokuPuzzleData puzzle = puzzles.Find(p => p.levelNumber == levelNumber);
        if (puzzle == null)
        {
            Debug.LogError($"Puzzle for level {levelNumber} not found!");
            return;
        }
        
        // Hide level select
        levelSelectPanel.SetActive(false);

        // Modify the grid
        //  sudokuGrid.ClearGrid();
        sudokuGrid.GenerateGrid();
        sudokuGrid.SetSolution(puzzle.solution);
        PrintGrid(puzzle.solution);
        sudokuGrid.SetSwapCount(puzzle.initialSwaps);
        sudokuGrid.SetCurrentLevel(levelNumber);
        
        // Set fixed cells
        foreach (var fixedCell in puzzle.fixedCells)
        {
            sudokuGrid.SetFixedCell(fixedCell.row, fixedCell.col, fixedCell.value);
        }
        
        // Reset and show the grid
        sudokuGrid.InitializeRemainingCells();
        sudokuGrid.gameObject.SetActive(true);
    }
    public void PrintGrid(int[,] solution)
    {
        int rows = solution.GetLength(0);
        int cols = solution.GetLength(1);

        string output = "";
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                output += solution[row, col] + " ";
            }
            output += "\n";
        }

        Debug.Log("Grid:\n" + output);
    }
    public void MarkLevelCompleted(int levelNumber)
    {
        SudokuProgressData.LevelStatus level = progressData.levels.Find(l => l.levelNumber == levelNumber);
        if (level != null)
        {
            level.completed = true;
            SaveProgressData();
        }
    }
    
    public void ReturnToLevelSelect()
    {
        sudokuGrid.gameObject.SetActive(false);
        ShowLevelSelect();
    }
}
