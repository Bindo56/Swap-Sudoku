using System.Collections.Generic;

public static class SudokuSolver
{
    private static System.Random rng = new System.Random();

    public static int[,] GenerateSudokuSolution()
    {
        int[,] board = new int[9, 9];
        Solve(board);
        return board;
    }

    /* public static int[,] SolvePuzzle(int[,] board)
     {
         *//* int[,] board = (int[,])puzzle.Clone();
          return Solve(board) ? board : null;*//*
         int[,] solvedBoard = (int[,])board.Clone(); // Clone the board to avoid modifying the original
         if (Solve(solvedBoard))
         {
             return solvedBoard;

         }
         return null;

     }*/
    public static int[,] SolvePuzzle(int[,] puzzle, HashSet<(int, int)> fixedCells)
    {
        int[,] board = (int[,])puzzle.Clone();
        return Solve(board, fixedCells) ? board : null;
    }


    /* public static int[,] GeneratePuzzleFromSolution(int[,] solution)
     {
         int[,] puzzle = (int[,])solution.Clone();
         int removeCount = 40; 

         while (removeCount > 0)
         {
             int row = rng.Next(0, 9);
             int col = rng.Next(0, 9);
             if (puzzle[row, col] != 0)
             {
                 puzzle[row, col] = 0;
                 removeCount--;
             }
         }
         return puzzle;
     }*/

    private static bool Solve(int[,] board)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (board[row, col] == 0) // Find an empty cell
                {
                    List<int> numbers = GetShuffledNumbers();
                    foreach (int num in numbers)
                    {
                        if (IsValid(board, row, col, num))
                        {
                            board[row, col] = num; // Place the number
                            if (Solve(board))
                            {
                                return true; // If it leads to a solution, return true
                            }
                            board[row, col] = 0; // Otherwise, backtrack

                        }
                    }
                    return false; // No valid number found, backtrack
                }
            }
        }
        return true; // Solved 
    }

    private static bool Solve(int[,] board, HashSet<(int, int)> fixedCells)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (board[row, col] == 0 && !fixedCells.Contains((row, col)))
                {
                    foreach (int num in GetShuffledNumbers())
                    {
                        if (IsValid(board, row, col, num))
                        {
                            board[row, col] = num;
                            if (Solve(board, fixedCells))
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




    private static List<int> GetShuffledNumbers()
    {
        List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        // Fisher-Yates shuffle
        for (int i = numbers.Count - 1; i > 0; i--)
        {
            int j = rng.Next(0, i + 1);
            (numbers[i], numbers[j]) = (numbers[j], numbers[i]);
        }

        return numbers;
    }

    private static bool IsValid(int[,] board, int row, int col, int num)
    {
        for (int i = 0; i < 9; i++)
        {
            if (board[row, i] == num || board[i, col] == num)
                return false;
        }

        int startRow = (row / 3) * 3;
        int startCol = (col / 3) * 3;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (board[startRow + i, startCol + j] == num)
                    return false;
            }
        }
        return true;
    }
}
