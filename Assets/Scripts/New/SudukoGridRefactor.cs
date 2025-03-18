using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class SudukoGrid : MonoBehaviour
{
    public GameObject cellPrefab;
    public Transform gridParent;
    private SudukoCell[,] grid = new SudukoCell[9, 9];
    private int[,] solution;

    [SerializeField] private int swapCount = 0;
    public TextMeshProUGUI swapCountText;

    private bool gameWon = false;
    private const int GRID_SIZE = 9;
    private const int BLOCK_SIZE = 3;
    private const int TOTAL_SUM = 45;

    void Start()
    {
        GenerateGrid();
        GeneratePuzzle();
        UpdateSwapCountDisplay();
    }

    #region GenrateGrid
    void GenerateGrid()
    {
        float cellSize = 200f;
        Vector2 startPosition = new Vector2(50, -50);

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
        if (swapCount <= 0)
        {
            swapCountText.text = "Game Over";
            return;
        }

        if (cellA == cellB || cellA.IsFixed || cellB.IsFixed)
            return;

        int numberA = cellA.GetNumber();
        int numberB = cellB.GetNumber();

        cellA.SetNumber(numberB);
        cellB.SetNumber(numberA);

        swapCount--;
        UpdateSwapCountDisplay();

        CheckCellMatchesSolution(cellA.row, cellA.col);
        CheckCellMatchesSolution(cellB.row, cellB.col);

        CheckWinCondition();

      //  CheckWinCondition();
    }
    #endregion

    #region Cell Validation and Coloring Active
    private void UpdateCellValidityDisplay(int row, int col) //use it for directly check the cell validation in SukoduCell Button 
    {

        CheckCellMatchesSolution(row, col);

        #region Depricated
        /*// Update the 3x3 block containing this cell
        int startRow = (row / BLOCK_SIZE) * BLOCK_SIZE;
        int startCol = (col / BLOCK_SIZE) * BLOCK_SIZE;

        UpdateRowDisplay(row);
        UpdateColumnDisplay(col);


        UpdateBlockDisplay(startRow, startCol);*/

        #endregion
    }

    private void CheckCellMatchesSolution(int row, int col)
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
            grid[row, col].SetColor(Color.white);
        }
    }

    #endregion

    /*  private void UpdateAllCellsValidity()
      {
          for (int row = 0; row < GRID_SIZE; row++)
          {
              for (int col = 0; col < GRID_SIZE; col++)
              {
                  CheckCellMatchesSolution(row, col);
              }
          }
      }*/

    #region Depricated CellValidations Check

    private void UpdateBlockDisplay(int startRow, int startCol)
    {
        int boxSum = 0;
        bool boxHasAllNumbers = true;
        HashSet<int> boxNumbers = new HashSet<int>();

        // Calculate sum and check for duplicates in this 3x3 block
        for (int r = startRow; r < startRow + BLOCK_SIZE; r++)
        {
            for (int c = startCol; c < startCol + BLOCK_SIZE; c++)
            {
                int num = grid[r, c].GetNumber();
                boxSum += num;
                if (num < 1 || num > 9 || !boxNumbers.Add(num))
                {
                    boxHasAllNumbers = false;
                    //return; -- removing this so we continue evaluating
                }
            }
        }

        // Set colors for cells in this block
        if (boxSum == TOTAL_SUM && boxHasAllNumbers)
        {
            // Block is valid, set all cells in this block to green
            for (int r = startRow; r < startRow + BLOCK_SIZE; r++)
            {
                for (int c = startCol; c < startCol + BLOCK_SIZE; c++)
                {
                    if (!grid[r, c].IsFixed)
                    {
                        grid[r, c].SetColor(Color.green);
                    }
                }
            }
        }
        else
        {
            // Block is invalid, set all cells to white unless they're in a valid row
            for (int r = startRow; r < startRow + BLOCK_SIZE; r++)
            {
                for (int c = startCol; c < startCol + BLOCK_SIZE; c++)
                {
                    if (!grid[r, c].IsFixed && grid[r, c].GetColor() != Color.green)
                    {
                        grid[r, c].SetColor(Color.white);
                    }
                }
            }
        }

        #region DepricatedUpdateBlockDisplay

        /*int boxSum = 0;
        bool boxHasAllNumbers = true;
        HashSet<int> boxNumbers = new HashSet<int>();

        // Calculate sum and check for duplicates in this 3x3 block
        for (int r = startRow; r < startRow + BLOCK_SIZE; r++)
        {
            for (int c = startCol; c < startCol + BLOCK_SIZE; c++)
            {
                int num = grid[r, c].GetNumber();
                boxSum += num;
                if (num < 1 || num > 9 || !boxNumbers.Add(num))
                {
                    boxHasAllNumbers = false;
                    //return;
                }
            }
        }

        // Set colors for cells in this block
        for (int r = startRow; r < startRow + BLOCK_SIZE; r++)
        {
            for (int c = startCol; c < startCol + BLOCK_SIZE; c++)
            {
                if (grid[r, c].IsFixed)
                {
                    continue;
                }
                else if (boxSum == TOTAL_SUM && boxHasAllNumbers )  //add check here
                {
                    if (rowHasAllNumbers)
                    {
                   //  grid[r, c].SetColor(Color.green);

                    }
                }
                else
                {
                    grid[r, c].SetColor(Color.white);
                }
            }
        }*/
        #endregion
    }

  
    //  bool rowHasAllNumbers = true;
    private void UpdateRowDisplay(int row)
    {
        int rowSum = 0;
        bool rowHasAllNumbers = true; // Local variable
        HashSet<int> rowNumbers = new HashSet<int>();
       // bool colBool = UpdateColumnDisplay(row);
        // Calculate sum and check for duplicates in this row
        for (int c = 0; c < GRID_SIZE; c++)
        {
            int num = grid[row, c].GetNumber();
            rowSum += num;
            if (num < 1 || num > 9 || !rowNumbers.Add(num))
            {
                rowHasAllNumbers = false;
            }
        }

        // Set colors for non-fixed cells in this row
        if (rowSum == TOTAL_SUM && rowHasAllNumbers )
        {
            // Row is valid, set all cells in this row to green
            for (int c = 0; c < GRID_SIZE; c++)
            {
                if (!grid[row, c].IsFixed)
                {
                    bool retu = ret;
                    if (ret)
                    {
                      grid[row, c].SetColor(Color.green);
                    }

                }
            }
        }
        else
        {
            // Row is invalid, but keep cells green if they're in a valid block
            for (int c = 0; c < GRID_SIZE; c++)
            {
                if (!grid[row, c].IsFixed && grid[row, c].GetColor() != Color.green)
                {
                    grid[row, c].SetColor(Color.white);
                }
            }
        }

        #region Depricated UpdateRowDisplay()
        /* int rowSum = 0;
         HashSet<int> rowNumbers = new HashSet<int>();

         // Calculate sum and check for duplicates in this row
         for (int c = 0; c < GRID_SIZE; c++)
         {
             int num = grid[row, c].GetNumber();
             rowSum += num;
             if (num < 1 || num > 9 || !rowNumbers.Add(num))
             {
                 rowHasAllNumbers = false;
             }
         }

         // Set colors for non-fixed cells in this row
         for (int c = 0; c < GRID_SIZE; c++)
         {
             if (!grid[row, c].IsFixed && grid[row, c].GetColor() != Color.green)
             {
                 if (rowSum == TOTAL_SUM && rowHasAllNumbers)
                 {
                     grid[row, c].SetColor(Color.green);
                 }
                 else if (grid[row, c].GetColor() != Color.green)
                 {
                     grid[row, c].SetColor(Color.white);
                 }
             }
         }*/
        #endregion
    }

    bool ret = true;
    bool colDisplay(bool retu)
    {
        ret = retu;
        return retu;
    }
    private void UpdateColumnDisplay(int col)
    {
        int colSum = 0;
        bool colHasAllNumbers = true; // Local variable
        HashSet<int> colNumbers = new HashSet<int>();

        // Calculate sum and check for duplicates in this column
        for (int r = 0; r < GRID_SIZE; r++)
        {
            int num = grid[r, col].GetNumber();
            colSum += num;
            if (num < 1 || num > 9 || !colNumbers.Add(num))
            {
                colHasAllNumbers = false;
            }
        }

            colDisplay(colHasAllNumbers);
     
        #region Depircated UpdateColumeDisplay
        /* // Set colors for non-fixed cells in this column
         for (int r = 0; r < GRID_SIZE; r++)
         {
             if (!grid[r, col].IsFixed && grid[r, col].GetColor() != Color.green)
             {
                 if (colSum == TOTAL_SUM && colHasAllNumbers)
                 {
                     grid[r, col].SetColor(Color.green);
                 }
                 else if (grid[r, col].GetColor() != Color.green)
                 {
                     grid[r, col].SetColor(Color.white);
                 }
             }
         }*/
        #endregion
    }
    #endregion

    #region Depricated HelperFunctions
    // Add these helper methods to check validity
    /*  private bool IsColumnValid(int col)
      {
          int colSum = 0;
          HashSet<int> colNumbers = new HashSet<int>();

          for (int r = 0; r < GRID_SIZE; r++)
          {
              int num = grid[r, col].GetNumber();
              colSum += num;
              if (num < 1 || num > 9 || !colNumbers.Add(num))
              {
                  return false;
              }
          }

          return colSum == TOTAL_SUM;
      }

      private bool IsBlockValid(int startRow, int startCol)
      {
          int boxSum = 0;
          HashSet<int> boxNumbers = new HashSet<int>();

          for (int r = startRow; r < startRow + BLOCK_SIZE; r++)
          {
              for (int c = startCol; c < startCol + BLOCK_SIZE; c++)
              {
                  int num = grid[r, c].GetNumber();
                  boxSum += num;
                  if (num < 1 || num > 9 || !boxNumbers.Add(num))
                  {
                      return false;
                  }
              }
          }

          return boxSum == TOTAL_SUM;
      }*/
    #endregion

    #region Puzzle Generation
    void GeneratePuzzle()
    {
        // Generate a complete Sudoku solution
        solution = GenerateFullSolution();
        LogBoard(solution, "Solution Of Puzzle");

        // Clone 
        int[,] puzzle = (int[,])solution.Clone();

        //track fix cell
        HashSet<(int, int)> fixedCells = new HashSet<(int, int)>();
        Dictionary<int, HashSet<int>> rowFixedNumbers = new Dictionary<int, HashSet<int>>();
        Dictionary<int, HashSet<int>> colFixedNumbers = new Dictionary<int, HashSet<int>>();
        System.Random rand = new System.Random();

        for (int i = 0; i < GRID_SIZE; i++)
        {
            rowFixedNumbers[i] = new HashSet<int>();
            colFixedNumbers[i] = new HashSet<int>();
        }

        
        while (fixedCells.Count < 40)   //change fixed cell for difficulty
        {
            int row = rand.Next(GRID_SIZE);
            int col = rand.Next(GRID_SIZE);
            int num = puzzle[row, col];

            if (!fixedCells.Contains((row, col)) &&
                !rowFixedNumbers[row].Contains(num) &&
                !colFixedNumbers[col].Contains(num))
            {
                fixedCells.Add((row, col));
                rowFixedNumbers[row].Add(num);
                colFixedNumbers[col].Add(num);

                grid[row, col].SetNumber(num, true);
                grid[row, col].SetColor(Color.blue);
            }
        }

      
        ShuffleNonFixedCells(puzzle, fixedCells);

        LogBoard(puzzle, "Final Puzzle");
    }

    private void ShuffleNonFixedCells(int[,] puzzle, HashSet<(int, int)> fixedCells)
    {
        System.Random rand = new System.Random();
        List<int> nonFixedNumbers = new List<int>();
        List<(int, int)> nonFixedPositions = new List<(int, int)>();

       
        for (int row = 0; row < GRID_SIZE; row++)
        {
            for (int col = 0; col < GRID_SIZE; col++)
            {
                if (!fixedCells.Contains((row, col)))
                {
                    nonFixedNumbers.Add(puzzle[row, col]);
                    nonFixedPositions.Add((row, col));
                }
            }
        }

       
        for (int i = 0; i < nonFixedNumbers.Count; i++)
        {
            int j = rand.Next(i, nonFixedNumbers.Count);
            int temp = nonFixedNumbers[i];
            nonFixedNumbers[i] = nonFixedNumbers[j];
            nonFixedNumbers[j] = temp;
        }

       
        for (int i = 0; i < nonFixedPositions.Count; i++)
        {
            var pos = nonFixedPositions[i];
            int newNum = nonFixedNumbers[i];
            puzzle[pos.Item1, pos.Item2] = newNum;
            grid[pos.Item1, pos.Item2].SetNumber(newNum);
        }
    }

    int[,] GenerateFullSolution()
    {
        int[,] board = new int[GRID_SIZE, GRID_SIZE];
        FillBoard(board);
        return board;
    }

    bool FillBoard(int[,] board)
    {
        for (int row = 0; row < GRID_SIZE; row++)
        {
            for (int col = 0; col < GRID_SIZE; col++)
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
        // Check the row and column
        for (int i = 0; i < GRID_SIZE; i++)
        {
            if (board[row, i] == num || board[i, col] == num)
                return false;
        }

        // Check the 3x3 block
        int startRow = (row / BLOCK_SIZE) * BLOCK_SIZE;
        int startCol = (col / BLOCK_SIZE) * BLOCK_SIZE;
        for (int r = startRow; r < startRow + BLOCK_SIZE; r++)
        {
            for (int c = startCol; c < startCol + BLOCK_SIZE; c++)
            {
                if (board[r, c] == num)
                    return false;
            }
        }
        return true;
    }

    private System.Random rng = new System.Random();

    private List<int> GetShuffledNumbers()
    {
        List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        for (int i = numbers.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (numbers[i], numbers[j]) = (numbers[j], numbers[i]);
        }
        return numbers;
    }
    #endregion


    #region Helper Methods
   /* public SudukoCell GetCellAtPosition(Vector3 position)
    {
        foreach (var cell in grid)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(cell.GetComponent<RectTransform>(), position))
            {
                return cell;
            }
        }
        return null;
    }*/

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
                }
            }
        }
        return fixedCells;
    }

    private int[,] GetCurrentGridState()
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

    private void UpdateSwapCountDisplay()
    {
        if (swapCountText != null)
            swapCountText.text = $"Swaps: {swapCount}";
    }

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
        }
    }

    void ShowWinMessage()
    {
        // Set all cells to green to indicate victory
        for (int row = 0; row < GRID_SIZE; row++)
        {
            for (int col = 0; col < GRID_SIZE; col++)
            {
                grid[row, col].SetColor(Color.green);
            }
        }

       
        swapCountText.text = "YOU WIN!";

       
        Debug.Log("YOU WIN!");
    }
    #endregion
}