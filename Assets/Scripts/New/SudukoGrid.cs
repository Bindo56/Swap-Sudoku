
using System.Collections;
using System.Collections.Generic;
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

    void Start()
    {
        GenerateGrid();
        GeneratePuzzle();
        UpdateSwapCountDisplay();
    }

    void GenerateGrid()
    {
        float cellSize = 200f; 
        Vector2 startPosition = new Vector2(50,-50); 

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
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
    #region SwapCell
    private SudukoCell firstSelectedCell = null;

    public void HandleCellClick(SudukoCell clickedCell)
    {
        if (clickedCell.IsFixed)
        {
            return;
        }

        else if (firstSelectedCell == null)
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

            CheckWinCondition(); // Validate the Sudoku grid
        }
    }

    private void SwapCells(SudukoCell cellA, SudukoCell cellB)
    {
        if (swapCount <= 0)
        {
            swapCountText.text = "Game Over";
            return;
        }
        if (cellA == cellB || cellA.IsFixed || cellB.IsFixed) return;

        int numberA = cellA.GetNumber();
        int numberB = cellB.GetNumber();

       /* if (!IsValidMove(cellA.row, cellA.col, numberB) || !IsValidMove(cellB.row, cellB.col, numberA))
        {
            Debug.Log("Invalid move!");
            return;
        }*/

        cellA.SetNumber(numberB);
        cellB.SetNumber(numberA);

        swapCount--;
        UpdateSwapCountDisplay();

    }
    #endregion


    /* public void CheckWinCondition()
     {
         // Add logic to check if the Sudoku puzzle is solved
         Debug.Log("Checking win condition...");
     }*/
    void GeneratePuzzle()
    {
        #region Depricated PuzzleGen
        /* solution = SudokuSolver.GenerateSudokuSolution();
         int[,] puzzle = (int[,])solution.Clone();*//*
       
        int[,] solution = GenerateFullSolution();//genrate;
        PrintBoard(solution, "FirstGen");
        // Clone the solution to form our puzzle board.
        int[,] puzzle = (int[,])solution.Clone();
        Debug.Log("GenratePuzzle");
        PrintBoard(puzzle  , "currentPuzzle");
        ShuffleSudoku(puzzle, 20);
        PrintBoard(puzzle, "ShuffleSudoku");

        HashSet<(int, int)> fixedCells = new HashSet<(int, int)>();
        Dictionary<int, HashSet<int>> rowFixedNumbers = new Dictionary<int, HashSet<int>>();
        Dictionary<int, HashSet<int>> colFixedNumbers = new Dictionary<int, HashSet<int>>();
        System.Random rand = new System.Random();

        
        for (int i = 0; i < 9; i++)
        {
            rowFixedNumbers[i] = new HashSet<int>();
            colFixedNumbers[i] = new HashSet<int>();
        }

        while (fixedCells.Count < 40)
        {
            int row = rand.Next(9);
            int col = rand.Next(9);
            int num = puzzle[row, col];
           // grid[row, col].SetNumber(puzzle[row, col]);
            //  number is not repeated in the row or column
            if (!fixedCells.Contains((row, col)) &&
                !rowFixedNumbers[row].Contains(num) &&
                !colFixedNumbers[col].Contains(num))
            {
                fixedCells.Add((row, col));
                rowFixedNumbers[row].Add(num);
                colFixedNumbers[col].Add(num);

              //  grid[row, col].SetDraggable(false);
              //  grid[row, col].SetNumber(num);
                grid[row, col].SetColor(Color.green);
                //grid[row, col].SetNumber(num, true);
            }
        }

       
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (!fixedCells.Contains((row, col)))
                {
                   // ShuffleSudoku(puzzle[row, col], 10);
                    grid[row, col].SetNumber(puzzle[row, col]);
                   // grid[row, col].SetDraggable(true); // Allow swapping for these cells
                }
            }
        }*/
        #endregion


        int[,] solution = GenerateFullSolution();
        PrintBoard(solution, "Solution Of Puzzle");

      
        int[,] puzzle = (int[,])solution.Clone();

       
        HashSet<(int, int)> fixedCells = new HashSet<(int, int)>();
        Dictionary<int, HashSet<int>> rowFixedNumbers = new Dictionary<int, HashSet<int>>();
        Dictionary<int, HashSet<int>> colFixedNumbers = new Dictionary<int, HashSet<int>>();
        System.Random rand = new System.Random();

        for (int i = 0; i < 9; i++)
        {
            rowFixedNumbers[i] = new HashSet<int>();
            colFixedNumbers[i] = new HashSet<int>();
        }

       
        while (fixedCells.Count < 40)
        {
            int row = rand.Next(9);
            int col = rand.Next(9);
            int num = puzzle[row, col];
            
            if (!fixedCells.Contains((row, col)) &&
                !rowFixedNumbers[row].Contains(num) &&
                !colFixedNumbers[col].Contains(num))
            {
                fixedCells.Add((row, col));
                rowFixedNumbers[row].Add(num);
                colFixedNumbers[col].Add(num);
              
                grid[row, col].SetNumber(num, true); 
                grid[row, col].SetColor(Color.green);
            }
        }

       
        List<int> nonFixedNumbers = new List<int>();
        List<(int, int)> nonFixedPositions = new List<(int, int)>();

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
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
          //  grid[pos.Item1, pos.Item2].SetDraggable(true); // Allow these cells to be interactable.
        }

        // Optionally, print the final board configuration.
        PrintBoard(puzzle, "Puzzle");
    }

    int[,] GenerateFullSolution()
    {
        int[,] board = new int[9, 9];
        FillBoard(board);
        return board;
    }

    bool FillBoard(int[,] board)
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
        // Check the row and column.
        for (int i = 0; i < 9; i++)
        {
            if (board[row, i] == num || board[i, col] == num)
                return false;
        }

        // Check the 3x3 block.
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
    System.Random rng = new System.Random();
    List<int> GetShuffledNumbers()
    {
       // System.Random rng = new System.Random();
        List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        for (int i = numbers.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (numbers[i], numbers[j]) = (numbers[j], numbers[i]);
        }
        return numbers;
    }


    #region shufflePuzzel
    void ShuffleSudoku(int[,] board, int swaps)
    {
        System.Random rand = new System.Random();

       
        for (int band = 0; band < 3; band++)
        {
            int firstRow = band * 3;
            int swapWith = firstRow + rand.Next(3);
            SwapRows(board, firstRow, swapWith);
        }

        
        for (int stack = 0; stack < 3; stack++)
        {
            int firstCol = stack * 3;
            int swapWith = firstCol + rand.Next(3);
            SwapColumns(board, firstCol, swapWith);
        }

       
        if (rand.Next(2) == 0)
        {
            Transpose(board);
        }
    }
    void SwapRows(int[,] board, int rowA, int rowB)
    {
        for (int col = 0; col < 9; col++)
        {
            int temp = board[rowA, col];
            board[rowA, col] = board[rowB, col];
            board[rowB, col] = temp;
        }
    }

    void SwapColumns(int[,] board, int colA, int colB)
    {
        for (int row = 0; row < 9; row++)
        {
            int temp = board[row, colA];
            board[row, colA] = board[row, colB];
            board[row, colB] = temp;
        }
    }

    void Transpose(int[,] board)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = row + 1; col < 9; col++)
            {
                int temp = board[row, col];
                board[row, col] = board[col, row];
                board[col, row] = temp;
            }
        }
    }
    #endregion

    public SudukoCell GetCellAtPosition(Vector3 position)
    {
        foreach (var cell in grid)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(cell.GetComponent<RectTransform>(), position))
            {
                
                return cell;
            }
        }
        return null;
    }

    #region SolutionPuzzle
    private int CalculateConflict(int[,] board)
    {
        int conflict = 0;
        // Check rows.
        for (int row = 0; row < 9; row++)
        {
            HashSet<int> seen = new HashSet<int>();
            for (int col = 0; col < 9; col++)
            {
                int num = board[row, col];
                if (seen.Contains(num))
                    conflict++;
                else
                    seen.Add(num);
            }
        }
        // Check columns.
        for (int col = 0; col < 9; col++)
        {
            HashSet<int> seen = new HashSet<int>();
            for (int row = 0; row < 9; row++)
            {
                int num = board[row, col];
                if (seen.Contains(num))
                    conflict++;
                else
                    seen.Add(num);
            }
        }
        return conflict;
    }
    public IEnumerator AutoSolveBySwapping()
    {
        int[,] board = GetCurrentGridState();
        HashSet<(int, int)> fixedCells = GetFixedCells();

        int conflict = CalculateConflict(board);
        int iterations = 0;
        int maxIterations = 10000; // Safety limit to prevent endless loops.//dopnt higher

        Debug.Log("Starting automated swap-solver. Initial conflict: " + conflict);

        while (conflict > 0 && iterations < maxIterations)
        {
            bool improved = false;

            // Process each 3×3 block.
            for (int blockRow = 0; blockRow < 3; blockRow++)
            {
                for (int blockCol = 0; blockCol < 3; blockCol++)
                {
                    // Get list of non-fixed cells in this block.
                    List<(int row, int col)> nonFixedCells = new List<(int, int)>();
                    for (int r = blockRow * 3; r < blockRow * 3 + 3; r++)
                    {
                        for (int c = blockCol * 3; c < blockCol * 3 + 3; c++)
                        {
                            if (!fixedCells.Contains((r, c)))
                            {
                                nonFixedCells.Add((r, c));
                            }
                        }
                    }

                   
                    for (int i = 0; i < nonFixedCells.Count; i++)
                    {
                        for (int j = i + 1; j < nonFixedCells.Count; j++)
                        {
                            var cell1 = nonFixedCells[i];
                            var cell2 = nonFixedCells[j];

                           
                            int temp = board[cell1.row, cell1.col];
                            board[cell1.row, cell1.col] = board[cell2.row, cell2.col];
                            board[cell2.row, cell2.col] = temp;

                            int newConflict = CalculateConflict(board);

                            
                            if (newConflict < conflict)
                            {
                                conflict = newConflict;
                                improved = true;
                               
                                grid[cell1.row, cell1.col].SetNumber(board[cell1.row, cell1.col]);
                                grid[cell2.row, cell2.col].SetNumber(board[cell2.row, cell2.col]);
                            }
                            else
                            {
                                
                                temp = board[cell1.row, cell1.col];
                                board[cell1.row, cell1.col] = board[cell2.row, cell2.col];
                                board[cell2.row, cell2.col] = temp;
                            }

                            yield return new WaitForSeconds(0.01f); 
                        }
                    }
                }
            }

            // ramdom swap 
            if (!improved)
            {
                int blockRow = rng.Next(3);
                int blockCol = rng.Next(3);
                List<(int row, int col)> nonFixedCells = new List<(int, int)>();
                for (int r = blockRow * 3; r < blockRow * 3 + 3; r++)
                {
                    for (int c = blockCol * 3; c < blockCol * 3 + 3; c++)
                    {
                        if (!fixedCells.Contains((r, c)))
                            nonFixedCells.Add((r, c));
                    }
                }
                if (nonFixedCells.Count >= 2)
                {
                    int index1 = rng.Next(nonFixedCells.Count);
                    int index2 = rng.Next(nonFixedCells.Count);
                    while (index2 == index1)
                        index2 = rng.Next(nonFixedCells.Count);

                    var cell1 = nonFixedCells[index1];
                    var cell2 = nonFixedCells[index2];

                    // Swap cells.
                    int temp = board[cell1.row, cell1.col];
                    board[cell1.row, cell1.col] = board[cell2.row, cell2.col];
                    board[cell2.row, cell2.col] = temp;

                    grid[cell1.row, cell1.col].SetNumber(board[cell1.row, cell1.col]);
                    grid[cell2.row, cell2.col].SetNumber(board[cell2.row, cell2.col]);
                }
            }

            iterations++;
            yield return null;
        }

        if (conflict == 0)
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    grid[row, col].SetNumber(board[row, col]);
                    // Optionally, update the color to indicate the cell is solved.
                    grid[row, col].SetColor(Color.white);
                }
            }
         //   Debug.Log("Puzzle solved by swapping!");
            Debug.Log("Puzzle solved by swapping!");
                    PrintBoard(board, "solvedAuto");
        }
        else
        {
            Debug.Log("Max iterations reached, puzzle not solved.");
        }
    }
    public void CheckSolvability()
    {
        /* int[,] currentPuzzle = GetCurrentGridState();
         int[,] solved = SudokuSolver.SolvePuzzle(currentPuzzle);*/
        // SolvePuzzle();
        StartCoroutine(AutoSolveBySwapping());
       // Debug.Log(solved != null ? "Puzzle is solvable!" : "No solution exists!");
    }

    public IEnumerator AutoSolveWithSumCheck()
    {
        int[,] currentPuzzle = GetCurrentGridState();
        HashSet<(int, int)> fixedCells = GetFixedCells();

        
        for (int row = 0; row < 9; row++)
            for (int col = 0; col < 9; col++)
                if (!fixedCells.Contains((row, col)))
                    currentPuzzle[row, col] = 0;

        Debug.Log("Starting automated solve (sum‑elimination) process...");
        PrintBoard(currentPuzzle, "currentPuzzle");

        
        bool solved = SolveWithSumCheck(currentPuzzle, fixedCells);

        
        if (solved)
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (!fixedCells.Contains((row, col)))
                    {
                        grid[row, col].SetNumber(currentPuzzle[row, col]);
                        yield return new WaitForSeconds(0.1f); 
                    }
                }
            }
            Debug.Log("Puzzle solved automatically using sum elimination!");
            PrintBoard(currentPuzzle , "currentPuzzle");
        }
        else
        {
            Debug.Log("Could not complete solve using sum elimination alone!");
        }
    }

    bool SolveWithSumCheck(int[,] board, HashSet<(int, int)> fixedCells)
    {
        bool changed = true;
        while (changed)
        {
            changed = false;
            // Check each row.
            for (int row = 0; row < 9; row++)
            {
                int missingCount = 0;
                int missingCol = -1;
                int sumRow = 0;
                for (int col = 0; col < 9; col++)
                {
                    int num = board[row, col];
                    if (num == 0)
                    {
                        missingCount++;
                        missingCol = col;
                    }
                    else
                    {
                        sumRow += num;
                    }
                }
                if (missingCount == 1)
                {
                    int missingNum = 45 - sumRow;
                    if (!fixedCells.Contains((row, missingCol)) && board[row, missingCol] == 0)
                    {
                        board[row, missingCol] = missingNum;
                        changed = true;
                    }
                }
            }

            // Check each column.
            for (int col = 0; col < 9; col++)
            {
                int missingCount = 0;
                int missingRow = -1;
                int sumCol = 0;
                for (int row = 0; row < 9; row++)
                {
                    int num = board[row, col];
                    if (num == 0)
                    {
                        missingCount++;
                        missingRow = row;
                    }
                    else
                    {
                        sumCol += num;
                    }
                }
                if (missingCount == 1)
                {
                    int missingNum = 45 - sumCol;
                    if (!fixedCells.Contains((missingRow, col)) && board[missingRow, col] == 0)
                    {
                        board[missingRow, col] = missingNum;
                        changed = true;
                    }
                }
            }

            // Check each 3x3 block.
            for (int blockRow = 0; blockRow < 3; blockRow++)
            {
                for (int blockCol = 0; blockCol < 3; blockCol++)
                {
                    int missingCount = 0;
                    int missingR = -1, missingC = -1;
                    int sumBlock = 0;
                    for (int r = blockRow * 3; r < blockRow * 3 + 3; r++)
                    {
                        for (int c = blockCol * 3; c < blockCol * 3 + 3; c++)
                        {
                            int num = board[r, c];
                            if (num == 0)
                            {
                                missingCount++;
                                missingR = r;
                                missingC = c;
                            }
                            else
                            {
                                sumBlock += num;
                            }
                        }
                    }
                    if (missingCount == 1)
                    {
                        int missingNum = 45 - sumBlock;
                        if (!fixedCells.Contains((missingR, missingC)) && board[missingR, missingC] == 0)
                        {
                            board[missingR, missingC] = missingNum;
                            changed = true;
                        }
                    }
                }
            }
        }

        // Check if the board is complete.
        for (int row = 0; row < 9; row++)
            for (int col = 0; col < 9; col++)
                if (board[row, col] == 0)
                    return false;
        return true;
    }
    #endregion

    private void UpdateSwapCountDisplay()
    {
        if (swapCountText != null)
            swapCountText.text = $"Swaps: {swapCount}";
    }
    private static void PrintBoard(int[,] board , string msg)
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
        Debug.Log(msg + "  - Puzzle State:\n" + boardStr );
       // Debug.Log(msg);
    }
    #region DepricatedSolvePuzzle
    public void SolvePuzzle()
    {
        int[,] currentPuzzle = GetCurrentGridState();
        HashSet<(int, int)> fixedCells = GetFixedCells();
        Debug.Log("Board");
        PrintBoard(currentPuzzle , "currentPuzzle");
        int[,] solved = SudokuSolver.SolvePuzzle(currentPuzzle, fixedCells);

        if (solved != null)
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (!grid[row, col].IsFixed)
                    {
                        grid[row, col].SetNumber(solved[row, col]);
                    }
                }
            }
            Debug.Log("Puzzle solved successfully!");
            PrintBoard(solved,"Solved");
        }
        else
        {
            Debug.Log("No solution exists with current fixed cells!");
        }
       

        /*int[,] currentPuzzle = GetCurrentGridState();
        int[,] solved = SudokuSolver.SolvePuzzle(currentPuzzle);

        if (solved != null)
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (!grid[row, col].IsFixed)
                    {
                        grid[row, col].SetNumber(solved[row, col]);
                    }
                }
            }
        }*/



        //new 
        // int[,] solved = SudokuSolver.SolvePuzzle(testBoard);
        /* int[,] currentPuzzle = GetCurrentGridState();
         PrintBoard(currentPuzzle);
         for (int row = 0; row < 9; row++)
         {
             for (int col = 0; col < 9; col++)
             {
                 if (!grid[row, col].IsFixed)
                 {
                     grid[row, col].SetNumber(solution[row, col]);
                 }
             }
         }
         PrintBoard(solution);
         Debug.Log("Puzzle solved using original solution!");*/
        /*
                int[,] currentPuzzle = GetCurrentGridState();

                //   Debug.Log("🔹 CURRENT BOARD BEFORE SOLVING:");
                int[,] puzzle =
              {
                { 5, 3, 0, 0, 7, 0, 0, 0, 0 },
                { 6, 0, 0, 1, 9, 5, 0, 0, 0 },
                { 0, 9, 8, 0, 0, 0, 0, 6, 0 },
                { 8, 0, 0, 0, 6, 0, 0, 0, 3 },
                { 4, 0, 0, 8, 0, 3, 0, 0, 1 },
                { 7, 0, 0, 0, 2, 0, 0, 0, 6 },
                { 0, 6, 0, 0, 0, 0, 2, 8, 0 },
                { 0, 0, 0, 4, 1, 9, 0, 0, 5 },
                { 0, 0, 0, 0, 8, 0, 0, 7, 9 }
              };
                PrintBoard(currentPuzzle);

                int[,] solved = SudokuSolver.SolvePuzzle(currentPuzzle);
                PrintBoard(solved);

                if (solved != null)
                {
                    Debug.Log(" Puzzle solved successfully!");
                    PrintBoard(solved);
                }
                else
                {
                    Debug.Log(" No solution exists for this puzzle!");
                }*/
        #endregion

    }
    private HashSet<(int, int)> GetFixedCells()
    {
        HashSet<(int, int)> fixedCells = new HashSet<(int, int)>();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
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
        int[,] state = new int[9, 9];
        for (int row = 0; row < 9; row++)
            for (int col = 0; col < 9; col++)
                state[row, col] = grid[row, col].GetNumber();
        return state;
    }

    public bool CheckBoardValidity()
    {
        // row
        for (int row = 0; row < 9; row++)
        {
            HashSet<int> rowNumbers = new HashSet<int>();
            int rowSum = 0;
            for (int col = 0; col < 9; col++)
            {
                int num = grid[row, col].GetNumber();
                rowNumbers.Add(num);
                rowSum += num;
            }
            if (rowNumbers.Count != 9 || rowSum != 45)
            {
                return false;
            }
        }

        // col
        for (int col = 0; col < 9; col++)
        {
            HashSet<int> colNumbers = new HashSet<int>();
            int colSum = 0;
            for (int row = 0; row < 9; row++)
            {
                int num = grid[row, col].GetNumber();
                colNumbers.Add(num);
                colSum += num;
            }
            if (colNumbers.Count != 9 || colSum != 45)
            {
                return false;
            }
        }

        //  3x3 
        for (int blockRow = 0; blockRow < 3; blockRow++)
        {
            for (int blockCol = 0; blockCol < 3; blockCol++)
            {
                HashSet<int> blockNumbers = new HashSet<int>();
                int blockSum = 0;
                for (int row = blockRow * 3; row < blockRow * 3 + 3; row++)
                {
                    for (int col = blockCol * 3; col < blockCol * 3 + 3; col++)
                    {
                        int num = grid[row, col].GetNumber();
                        blockNumbers.Add(num);
                        blockSum += num;
                    }
                }
                if (blockNumbers.Count != 9 || blockSum != 45)
                {
                    return false;
                }
            }
        }
        return true;
    }


    //Depricated
    /* public bool IsValidMove(int row, int col, int number)
     {
         // Check row
         for (int i = 0; i < 9; i++)
         {
             if (grid[row, i].GetNumber() == number) return false;
         }

         // Check column
         for (int i = 0; i < 9; i++)
         {
             if (grid[i, col].GetNumber() == number) return false;
         }

         // Check 3x3 box
         int startRow = (row / 3) * 3;
         int startCol = (col / 3) * 3;
         for (int i = 0; i < 3; i++)
         {
             for (int j = 0; j < 3; j++)
             {
                 if (grid[startRow + i, startCol + j].GetNumber() == number) return false;
             }
         }
         return true;
     }*/

    #region checkIfSolved
   

    public void CheckWinCondition()
    {
        if (CheckBoardValidity())
        {
            Debug.Log("Sudoku Solved!");
            ShowWinMessage();
        }
        else
        {
            Debug.Log("Solution is not valid!");
        }
    }

    void ShowWinMessage()
    {
       //reload or chenge secne
       
       
        Debug.Log("?? YOU WIN! ??");
    }
    #endregion
}
