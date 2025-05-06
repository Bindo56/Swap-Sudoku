using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.UI;
//using System;
//using static UnityEditor.Progress;

public class SudokuPuzzleManager : MonoBehaviour
{
    [SerializeField] private SudukoGrid sudokuGrid;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private GameObject levelSelectPanel;
    [SerializeField] private GameObject levelButtonPrefab;
    [SerializeField] private Transform levelButtonContainer;
    public GameObject pausePanel;

    public Transform gameOverPanel;
    public Button giveChanceBtn;
    public Button levlSelectionBtn;
    public TextMeshProUGUI gameOverText;
    
    private const int TOTAL_PUZZLES = 100;
    private const string SAVE_FILE_NAME = "sudoku_puzzles.json";
    private const string PROGRESS_FILE_NAME = "sudoku_progress.json";
    
    private List<SudokuPuzzleData> puzzles = new List<SudokuPuzzleData>(); 
    private SudokuProgressData progressData;
    
    [System.Serializable]
    public class SudokuPuzzleData
    {
        public int levelNumber; //testing
        public List<int> solutionFlat;
      //  public int[,] solution;
        public List<FixedCellData> fixedCells;
        public int initialHints = 3;
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

        [JsonIgnore] // this won't get serialized
        public int[,] Solution2D
        {
            get
            {
                if (solutionFlat == null || solutionFlat.Count != 81)
                {
                    Debug.LogError("solutionFlat is null or incorrectly sized.");
                    return new int[9, 9];
                }

                int[,] grid = new int[9, 9];
                for (int i = 0; i < solutionFlat.Count; i++)
                {
                    int row = i / 9;
                    int col = i % 9;
                    grid[row, col] = solutionFlat[i];
                }
                return grid;
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
            public float timer;
            public bool completed;
            
            public LevelStatus(int levelNumber,float timer, bool completed)
            {
                this.levelNumber = levelNumber;
                this.timer = timer;
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

    public void SetGameOverPanel(string gameOverTextS)
    {
        gameOverText.text = gameOverTextS;
    }

    public void LoadNextLevel()
    {
        int currentLevel = sudokuGrid.GetCurrentLevel();
        int nextLevel = currentLevel + 1;
        Debug.Log(nextLevel + " -Level");

        if (nextLevel <= TOTAL_PUZZLES)
        {
            LoadLevel(nextLevel);
            sudokuGrid.UpdateLevelDisplay();
        }
        else
        {
            Debug.Log("No more levels!");
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
            
            // Yield to prevent freezing the UI //IMP DON'T REMOVE THIS // REMOVING THIS CAUSE THE UNITY TO EXCUTE THIS IN SINGLE FRAME /// 
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
    int shuffleAttempts;
    private SudokuPuzzleData GeneratePuzzle(int levelNumber)
    {
        System.Random seededRng = new System.Random(levelNumber);
        SudokuPuzzleData puzzle = new SudokuPuzzleData();
        puzzle.levelNumber = levelNumber;

        shuffleAttempts = 0;

        // Generate a full valid solution
        int[,] fullSolution = GenerateFullSolution(seededRng);

        // Store the flattened version in puzzle.solutionFlat for better json serilizations 
        puzzle.solutionFlat = new List<int>();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                puzzle.solutionFlat.Add(fullSolution[row, col]);
            }
        }

        
        int[,] solution = puzzle.Solution2D;

        int fixedCellsCount = Mathf.Max(20, 40 - (levelNumber / 10)); // Easy = more fixed
        puzzle.fixedCells = GenerateFixedCells(solution, fixedCellsCount,seededRng);
        puzzle.initialHints = 3; // You could vary this by level difficulty if desired

        int[,] playBoard = new int[9, 9];
        foreach (var fixedCell in puzzle.fixedCells)
        {
            playBoard[fixedCell.row, fixedCell.col] = fixedCell.value;
        }

        int moveNeededCount = 0;
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (playBoard[row, col] != solution[row, col])
                {
                    moveNeededCount++;
                }
            }
        }

        int extraMoves = 0;
        if (levelNumber < 15)
            extraMoves = 15;
        else if (levelNumber >= 30)
            extraMoves = seededRng.Next(5, 9);
        else
            extraMoves = 10;

        puzzle.initialSwaps = moveNeededCount + extraMoves;
        return puzzle;
    }
    
    private List<SudokuPuzzleData.FixedCellData> GenerateFixedCells(int[,] solution, int fixedCellsCount , System.Random rand)
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
        
      //  System.Random rand = new System.Random();
        
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
    
    private int[,] GenerateFullSolution(System.Random rng)
    {
        int[,] board = new int[9, 9];
        FillBoard(board,rng);
        return board;
    }
    
    private bool FillBoard(int[,] board, System.Random rng)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (board[row, col] == 0)
                {
                    foreach (int num in GetShuffledNumbers(rng))
                    {
                        if (IsValidNumber(board, row, col, num))
                        {
                            board[row, col] = num;
                            if (FillBoard(board, rng))
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
    
    private List<int> GetShuffledNumbers(System.Random rng)
    {
        shuffleAttempts++; // Increment on each  level to set level  difficultys //can also redesign according to design
        List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
      //  System.Random rng = new System.Random();

        for (int i = numbers.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            int temp = numbers[i];
            numbers[i] = numbers[j];
            numbers[j] = temp;
        }

        return numbers;

        
    }
    
    private void SavePuzzlesToJson() // sav levels json
    {
        string filePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (var puzzle in puzzles)
            {
                string line = JsonConvert.SerializeObject(puzzle, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    TypeNameHandling = TypeNameHandling.Auto,
                    Formatting = Formatting.None 
                });

                writer.WriteLine(line);
            }
        }

        Debug.Log($"Saved {puzzles.Count} puzzles to {filePath}");

    }

  
    private void InitializeProgressData()
    {
        progressData = new SudokuProgressData();
        
        for (int i = 1; i <= TOTAL_PUZZLES; i++)
        {
          //  Debug.Log(i);
            progressData.levels.Add(new SudokuProgressData.LevelStatus(i,0 ,false));
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
    public void ClearLevelSelectPanel()
    {
        foreach (Transform child in levelSelectPanel.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void LoadPuzzlesAndProgress()
    {
        string puzzlesPath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        string progressPath = Path.Combine(Application.persistentDataPath, PROGRESS_FILE_NAME);

        // Load puzzles
        if (File.Exists(puzzlesPath))
        {
            puzzles = new List<SudokuPuzzleData>();
            string[] lines = File.ReadAllLines(puzzlesPath);

            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var puzzle = JsonConvert.DeserializeObject<SudokuPuzzleData>(line, new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    });
                    puzzles.Add(puzzle);
                }
            }

            Debug.Log($"Loaded {puzzles.Count} puzzles");
        }

        // Load progress
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

    [ContextMenu("Delete Saved Game Data")] //call from inspector ,,script name and then  three dot and click on delete saved game data
    private void DeleteSavedGameData()
    {
        string puzzleFilePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        string progressFilePath = Path.Combine(Application.persistentDataPath, PROGRESS_FILE_NAME);

        bool deletedAny = false;

        if (File.Exists(puzzleFilePath))
        {
            File.Delete(puzzleFilePath);
            Debug.Log("Deleted puzzle save data.");
            deletedAny = true;
        }

        if (File.Exists(progressFilePath))
        {
            File.Delete(progressFilePath);
            Debug.Log("Deleted progress save data.");
            deletedAny = true;
        }

        if (!deletedAny)
        {
            Debug.LogWarning("No save files found to delete.");
        }
    }

    private void ShowLevelSelect()
    {
        levelSelectPanel.SetActive(true);
        
 
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
                sudokuGrid.SetHighScoreTimer(levelStatus.timer);
                
            }
            
            bool isAvailable = levelStatus.levelNumber == 1 || 
                               (levelStatus.levelNumber > 1 && progressData.levels[i-1].completed);
            // changes here according to level design
            button.interactable = isAvailable;// changes
            
           
            int levelNumber = levelStatus.levelNumber;

            button.onClick.AddListener(() => LoadLevel(levelNumber));
        }
    }
    
    public void LoadLevel(int levelNumber)
    {
        
        SudokuPuzzleData puzzle = puzzles.Find(p => p.levelNumber == levelNumber);
        if (puzzle == null)
        {
            Debug.LogError($"Puzzle for level {levelNumber} not found!");
            return;
        }
      
           SudokuProgressData.LevelStatus levelStatus = progressData.levels[levelNumber - 1];
           sudokuGrid.SetHighScoreTimer(levelStatus.timer);
        
        // Hide level select
        levelSelectPanel.SetActive(false);
        // Modify the grid
        //  sudokuGrid.ClearGrid();
        sudokuGrid.GenerateGrid();
      //  sudokuGrid.SetSolution(puzzle.solution);
        sudokuGrid.SetSolution(puzzle.Solution2D);
        sudokuGrid.SetHintsRemaining(puzzle.initialHints);
        // PrintGrid(puzzle.solution);
        PrintGrid(puzzle.Solution2D);
       // sudokuGrid.SetSwapCount(puzzle.initialSwaps);
        sudokuGrid.SetCurrentLevel(levelNumber);
        
        // Set fixed cells
        foreach (var fixedCell in puzzle.fixedCells)
        {
            sudokuGrid.SetFixedCell(fixedCell.row, fixedCell.col, fixedCell.value);
        }
        
        // Reset grid
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
    public void MarkLevelCompleted(int levelNumber, float completionTime)
    {

        var level = progressData.levels.Find(l => l.levelNumber == levelNumber);
        if (level != null)
        {
            level.completed = true;

            if (level.timer == 0f || completionTime < level.timer)
            {
                level.timer = completionTime;
                Debug.Log($"New best time for level {levelNumber}: {completionTime:F2}s");
            }

            SaveProgressData();
        }
        /*SudokuProgressData.LevelStatus level = progressData.levels.Find(l => l.levelNumber == levelNumber);
        if (level != null)
        {
            level.completed = true;
            SaveProgressData();
        }*/
    }

    public void OpenPause()
    {
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void ClosePause()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
    }


    public void ReturnToLevelSelect()
    {
        //  sudokuGrid.gameObject.SetActive(false);
        ClearLevelSelectPanel();
        ShowLevelSelect();
    }
}
