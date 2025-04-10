using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SudukoGrid : MonoBehaviour
{
    public static SudukoGrid instance;
    public GameObject cellPrefab;
    public Transform gridParent;
    private SudukoCell[,] grid = new SudukoCell[9, 9];
    private int[,] solution;

    /* [SerializeField] private int swapCount = 0;
     public TextMeshProUGUI swapCountText;*/
    [SerializeField] int chancesRemaining = 3;
    [SerializeField] private TextMeshProUGUI chancesRemainingText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI personalBestTimerText;
    [SerializeField] private Button backButton;
    [SerializeField] private SudokuPuzzleManager puzzleManager;
    [SerializeField] private TextMeshProUGUI hintsRemainingText;
    [SerializeField] private Button hintButton;

    [SerializeField] private int hintsRemaining = 3;
    private float levelTimer = 0f;
    private bool isTiming = false;
    public float CurrentTimer => levelTimer; // Accessor for external scripts


    private bool gameWon = false;
    private int currentLevel;
    private const int GRID_SIZE = 9;
    private const int BLOCK_SIZE = 3;
    private const int TOTAL_SUM = 45;

    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }

        if (hintButton != null)
        {
            hintButton.onClick.AddListener(UseHint);
        }

        // GenerateGrid();
        //UpdateSwapCountDisplay();
        UpdateLevelDisplay();
    }

    private void LateUpdate()
    {
        if (isTiming)
        {
            levelTimer += Time.deltaTime;
            UpdateTimerDisplay();
        }
        if (hintsRemainingText != null)
        {
            hintsRemainingText.text = $"Hints: {hintsRemaining}";
        }
        chancesRemainingText.text = $"Chances Remaining: {chancesRemaining}";
    }

    public void UseHint()
    {
        if (hintsRemaining <= 0) //|| gameWon
        {
            Debug.Log("No hints remaining");
            return;
        }

        // Find all cells that don't match the solution
        List<(int rowPos, int colPos)> incorrectCells = new List<(int rowPos, int colPos)>();
        for (int r = 0; r < GRID_SIZE; r++)
        {
            for (int c = 0; c < GRID_SIZE; c++)
            {
                if (!grid[r, c].IsFixed && grid[r, c].GetNumber() != solution[r, c])
                {
                    incorrectCells.Add((r, c));
                }
            }
        }

        if (incorrectCells.Count == 0)
        {
            Debug.Log("All cells are correct!");
            return;
        }

        // Find pairs of incorrect cells that could be swapped to improve the grid
        List<((int r1, int c1), (int r2, int c2))> swappablePairs = new List<((int r1, int c1), (int r2, int c2))>();

        for (int i = 0; i < incorrectCells.Count; i++)
        {
            var (r1, c1) = incorrectCells[i];
            int currentVal1 = grid[r1, c1].GetNumber();

            for (int j = i + 1; j < incorrectCells.Count; j++)
            {
                var (r2, c2) = incorrectCells[j];
                int currentVal2 = grid[r2, c2].GetNumber();

                // Check if swapping would improve the situation
                if (currentVal1 == solution[r2, c2] && currentVal2 == solution[r1, c1])
                {
                    swappablePairs.Add(((r1, c1), (r2, c2)));
                }
            }
        }

        if (swappablePairs.Count == 0)
        {
            // Fall back to original behavior if no swappable pairs found
            int randomIndex = Random.Range(0, incorrectCells.Count);
            var (cellRow, cellCol) = incorrectCells[randomIndex];

            grid[cellRow, cellCol].SetNumber(solution[cellRow, cellCol]);
            grid[cellRow, cellCol].SetColor(Color.green);
            Debug.Log("No swappable pairs found. Revealing one correct cell.");
        }
        else
        {
            // Choose a random pair to swap
            int randomPairIndex = Random.Range(0, swappablePairs.Count);
            var ((r1, c1), (r2, c2)) = swappablePairs[randomPairIndex];

            // Swap the values
            int temp = grid[r1, c1].GetNumber();
            grid[r1, c1].SetNumber(grid[r2, c2].GetNumber());
            grid[r2, c2].SetNumber(temp);

            // Highlight the swapped cells
            grid[r1, c1].SetColor(Color.yellow);
            grid[r2, c2].SetColor(Color.yellow);

           // Debug.Log($"Swapped cells at ({r1},{c1}) and ({r2},{c2})");
        }

        hintsRemaining--;

        CheckWinCondition();
    }
    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            timerText.text = $"Time: {levelTimer:F2}s";
        }
    }

    public void StartTimer()
    {
        levelTimer = 0f;
        isTiming = true;
    }

    public void StopTimer()
    {
        isTiming = false;
    }

    public float GetElapsedTime()
    {
        return levelTimer;
    }
    public void SetHighScoreTimer(float time)
    {
        personalBestTimerText.text = time.ToString();
    }

    #region GenrateGrid
    public void GenerateGrid()
    {
        Debug.Log("Generating Grid");
        float cellSize = 200f;
        Vector2 startPosition = new Vector2(50, -50);

        ClearLevelCell();

        for (int row = 0; row < GRID_SIZE; row++)
        {
            for (int col = 0; col < GRID_SIZE; col++)
            {
                GameObject newCell = Instantiate(cellPrefab, gridParent);
                SudukoCell cell = newCell.GetComponent<SudukoCell>();

                RectTransform rectTransform = newCell.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = startPosition + new Vector2(col * cellSize, -row * cellSize);

                grid[row, col] = cell;
                cell.SetCoordinates(row, col);
            }
        }
    }

    private void ClearLevelCell()
    {
        if (gridParent == null)
            return;

        foreach (Transform child in gridParent)
        {
            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }
    }
    #endregion

    #region Cell Selection and Swapping
    private SudukoCell firstSelectedCell = null;

    public void HandleCellClick(SudukoCell clickedCell)
    {
        if (gameWon || clickedCell.IsFixed)
        {
            return;
        }

        if (firstSelectedCell == null)
        {
            // First cell selection
            firstSelectedCell = clickedCell;
            firstSelectedCell.SetColor(Color.yellow);
        }
        else
        {
            // Second cell selection
            firstSelectedCell.SetColor(Color.white);
            SwapCells(firstSelectedCell, clickedCell);
            firstSelectedCell = null;
        }
        CheckWinCondition();
    }

    public void SwapCells(SudukoCell cellA, SudukoCell cellB)
    {
       /* if (swapCount <= 0)
        {
            swapCountText.text = "Game Over";
            return;
        }*/

        if(chancesRemaining <= 0)
        {
            chancesRemainingText.text = "Game Over";
            return;
        }

        if (cellA == cellB || cellA.IsFixed || cellB.IsFixed)
            return;

        int numberA = cellA.GetNumber();
        int numberB = cellB.GetNumber();

        cellA.SetNumber(numberB);
        cellB.SetNumber(numberA);

      //  swapCount--;
        //UpdateSwapCountDisplay();

       /* CheckCellMatchesSolution(cellA.row, cellA.col);
        CheckCellMatchesSolution(cellB.row, cellB.col);*/

        CheckBothCells(cellA.row, cellA.col, cellB.row, cellB.col);
        CheckWinCondition();
    }
    #endregion

    #region Cell Validation and Coloring Active
  

    void CheckBothCells(int rowA, int colA, int rowB, int colB)
    {
        bool isCorrectA = CheckSingleCell(rowA, colA);
        bool isCorrectB = CheckSingleCell(rowB, colB);

        if (!isCorrectA && !isCorrectB)
        {
            chancesRemaining -= 1;
        }
    }
    bool CheckSingleCell(int row, int col)
    {
        if (grid[row, col].IsFixed)
            return true;

        int currentNum = grid[row, col].GetNumber();
        int solutionNum = solution[row, col];

        if (currentNum == solutionNum)
        {
            grid[row, col].SetColor(Color.green);
            return true;
        }
        else
        {
            grid[row, col].SetColor(Color.white);
            return false;
        }
    }

    /* private void UpdateCellValidityDisplay(int row, int col)
   {
       CheckCellMatchesSolution(row, col);
   }*/



    /* private void CheckCellMatchesSolution(int row, int col)
     {
         if (grid[row, col].IsFixed)
             return;

         int currentNum = grid[row, col].GetNumber();
         int solutionNum = solution[row, col];

         if (currentNum == solutionNum)
         {
             grid[row, col].SetColor(Color.green);
         }
         else
         {
             chancesRemaining -= 1;
             grid[row, col].SetColor(Color.white);
         }
     }*/
    #endregion

    #region Level Management
    public void SetSolution(int[,] newSolution)
    {
        solution = new int[GRID_SIZE, GRID_SIZE];
        for (int row = 0; row < GRID_SIZE; row++)
        {
            for (int col = 0; col < GRID_SIZE; col++)
            {
                solution[row, col] = newSolution[row, col];
            }
        }
    }

    public void SetFixedCell(int row, int col, int value)
    {
        if (grid[row, col] != null)
        {
            grid[row, col].SetNumber(value, true);
            grid[row, col].SetColor(Color.blue);
         //   GetFixedCells().Add((row, col));
        }
    }
    public void SetHintsRemaining(int count)
    {
        hintsRemaining = count;
    }

    public void InitializeRemainingCells()
    {
        StartTimer();
        chancesRemaining = 3;
        hintsRemaining = 3;
        HashSet<(int, int)> fixedCells = GetFixedCells();
        List<int> nonFixedNumbers = new List<int>();
        List<(int, int)> nonFixedPositions = new List<(int, int)>();

        // Get non-fixed positions and collect numbers to shuffle
        for (int row = 0; row < GRID_SIZE; row++)
        {
            for (int col = 0; col < GRID_SIZE; col++)
            {
                if (!fixedCells.Contains((row, col)))
                {
                    nonFixedNumbers.Add(solution[row, col]);
                    nonFixedPositions.Add((row, col));
                }
            }
        }

        // Shuffle non-fixed numbers
        System.Random rand = new System.Random();
        for (int i = 0; i < nonFixedNumbers.Count; i++)
        {
            int j = rand.Next(i, nonFixedNumbers.Count);
            int temp = nonFixedNumbers[i];
            nonFixedNumbers[i] = nonFixedNumbers[j];
            nonFixedNumbers[j] = temp;
        }

        // Set shuffled numbers to non-fixed cells
        for (int i = 0; i < nonFixedPositions.Count; i++)
        {
            var pos = nonFixedPositions[i];
            int newNum = nonFixedNumbers[i];
            grid[pos.Item1, pos.Item2].SetNumber(newNum);
        }

        gameWon = false;
        firstSelectedCell = null;
    }

    public void ClearGrid()
    {
       /* for (int row = 0; row < GRID_SIZE; row++)
        {
            for (int col = 0; col < GRID_SIZE; col++)
            {
                grid[row, col].SetNumber(0 , false);
              //  grid[row, col].SetFixed(false);
                grid[row, col].SetColor(Color.white);
            }
        }

        gameWon = false;*/
    }

  /*  public void SetSwapCount(int count)
    {
        swapCount = count;
        UpdateSwapCountDisplay();
    }*/

    public void SetCurrentLevel(int level)
    {
        currentLevel = level;
        UpdateLevelDisplay();
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
      //  UpdateLevelDisplay();
    }

    public void UpdateLevelDisplay()
    {
        if (levelText != null)
        {
            levelText.text = $"Level {currentLevel}";
        }
    }

    private void OnBackButtonClicked()
    {
        if (puzzleManager != null)
        {
            puzzleManager.ReturnToLevelSelect();
        }
    }
    #endregion

    #region Helper Methods
    private HashSet<(int, int)> GetFixedCells()
    {
        HashSet<(int, int)> fixedCells = new HashSet<(int, int)>();
        for (int row = 0; row < GRID_SIZE; row++)
        {
            for (int col = 0; col < GRID_SIZE; col++)
            {
                if (grid[row, col].IsFixed)
                {
                    fixedCells.Add((row, col));
                   // Debug.Log("Fixed Cell: " + row + ", " + col);
                }
            }
        }
        return fixedCells;
    }

    public int[,] GetCurrentGridState()
    {
        int[,] state = new int[GRID_SIZE, GRID_SIZE];
        for (int row = 0; row < GRID_SIZE; row++)
        {
            for (int col = 0; col < GRID_SIZE; col++)
            {
                state[row, col] = grid[row, col].GetNumber();
            }
        }
        return state;
    }

   /* private void UpdateSwapCountDisplay()
    {
        if (swapCountText != null)
            swapCountText.text = $"Swaps: {swapCount}";
    }*/

    private static void LogBoard(int[,] board, string msg)
    {
        string boardStr = "";
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                boardStr += board[row, col] + " ";
            }
            boardStr += "\n";
        }
        Debug.Log(msg + " - Puzzle State:\n" + boardStr);
    }
    #endregion

    #region Win Condition
    public bool CheckBoardValidity()
    {
        // Check rows
        for (int row = 0; row < GRID_SIZE; row++)
        {
            if (!IsValidSet(GetRowValues(row)))
            {
                return false;
            }
        }

        // Check columns
        for (int col = 0; col < GRID_SIZE; col++)
        {
            if (!IsValidSet(GetColumnValues(col)))
            {
                return false;
            }
        }

        // Check 3x3 blocks
        for (int blockRow = 0; blockRow < BLOCK_SIZE; blockRow++)
        {
            for (int blockCol = 0; blockCol < BLOCK_SIZE; blockCol++)
            {
                if (!IsValidSet(GetBlockValues(blockRow * BLOCK_SIZE, blockCol * BLOCK_SIZE)))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private List<int> GetRowValues(int row)
    {
        List<int> values = new List<int>();
        for (int col = 0; col < GRID_SIZE; col++)
        {
            values.Add(grid[row, col].GetNumber());
        }
        return values;
    }

    private List<int> GetColumnValues(int col)
    {
        List<int> values = new List<int>();
        for (int row = 0; row < GRID_SIZE; row++)
        {
            values.Add(grid[row, col].GetNumber());
        }
        return values;
    }

    private List<int> GetBlockValues(int startRow, int startCol)
    {
        List<int> values = new List<int>();
        for (int r = startRow; r < startRow + BLOCK_SIZE; r++)
        {
            for (int c = startCol; c < startCol + BLOCK_SIZE; c++)
            {
                values.Add(grid[r, c].GetNumber());
            }
        }
        return values;
    }

    private bool IsValidSet(List<int> values)
    {
        HashSet<int> set = new HashSet<int>(values);
        return set.Count == GRID_SIZE && values.Sum() == TOTAL_SUM;
    }

    public void CheckWinCondition()
    {
        if (CheckBoardValidity())
        {
            Debug.Log("Sudoku Solved! Congratulations!");
            gameWon = true;
            ShowWinMessage();

            // Mark level as completed
            if (puzzleManager != null)
            {
                puzzleManager.MarkLevelCompleted(currentLevel, GetElapsedTime());
                puzzleManager.LoadNextLevel();
            }
        }
        else if(chancesRemaining <= 0)
        {
            Debug.Log("0  chaces reamining");
            chancesRemainingText.text = "Game Over";
            //add couroutine or delay it 
            StartCoroutine(Delay());

        }
    }

    IEnumerator Delay()
    {
        yield return new WaitForSeconds(1);
        if (puzzleManager != null)
        {
            puzzleManager.ReturnToLevelSelect();
        }

    }

    void ShowWinMessage()
    {
        StopTimer();
        // Set all cells to green to indicate victory
        for (int row = 0; row < GRID_SIZE; row++)
        {
            for (int col = 0; col < GRID_SIZE; col++)
            {
                grid[row, col].SetColor(Color.green);
            }
        }

        //swapCountText.text = "YOU WIN!";

        Debug.Log("YOU WIN!");
       
        gameWon = true;

    }

    public bool IsGameWon()
    {
        return gameWon;
    }
    #endregion
}