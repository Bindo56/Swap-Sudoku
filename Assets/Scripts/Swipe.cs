using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class Swipe : MonoBehaviour
{

    public Button buttonPrefab;
    public Transform gridParent;
    public float cellSize = 60f;
    public float spacing = 5f;
    public Vector2 gridCenter = new Vector2(0, 0);

    private Button[,] buttons = new Button[9, 9];
    private int[,] numbers = new int[9, 9];
    private bool[,] isBlocked = new bool[9, 9];
    private bool[] blockedRows = new bool[9];
    private bool[] blockedCols = new bool[9];

    private int selectedRow = -1, selectedCol = -1;

    void Start()
    {
        if (gridCenter == Vector2.zero)
        {
            gridCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        }

        GenerateGrid();
        GenerateValidSudoku();
        UpdateUI();
    }

    void GenerateGrid()
    {
        float gridWidth = (9 * cellSize) + (8 * spacing);
        float gridHeight = (9 * cellSize) + (8 * spacing);
        Vector2 startPosition = new Vector2(gridCenter.x - gridWidth / 2, gridCenter.y + gridHeight / 2);

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                Button newButton = Instantiate(buttonPrefab, gridParent);
                newButton.name = $"Button_{row}_{col}";

                float posX = startPosition.x + col * (cellSize + spacing);
                float posY = startPosition.y - row * (cellSize + spacing);

                newButton.transform.localPosition = new Vector3(posX, posY, 0);
                newButton.transform.localScale = Vector3.one;

                int r = row, c = col;
                newButton.onClick.AddListener(() => OnTileClick(r, c));

                buttons[row, col] = newButton;
            }
        }
    }

    void GenerateValidSudoku()
    {
        List<int> availableNumbers = new List<int>();

        for (int i = 1; i <= 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                availableNumbers.Add(i);
            }
        }

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                bool placed = false;
                while (!placed && availableNumbers.Count > 0)
                {
                    int randomIndex = Random.Range(0, availableNumbers.Count);
                    int num = availableNumbers[randomIndex];

                    if (IsValidPlacement(row, col, num))
                    {
                        numbers[row, col] = num;
                        availableNumbers.RemoveAt(randomIndex);
                        placed = true;
                    }
                }
                isBlocked[row, col] = (Random.value < 0.3f);
            }
        }
    }

    bool IsValidPlacement(int row, int col, int num)
    {
        for (int i = 0; i < 9; i++)
        {
            if (numbers[row, i] == num || numbers[i, col] == num)
                return false;
        }

        int boxRowStart = (row / 3) * 3;
        int boxColStart = (col / 3) * 3;
        for (int r = boxRowStart; r < boxRowStart + 3; r++)
        {
            for (int c = boxColStart; c < boxColStart + 3; c++)
            {
                if (numbers[r, c] == num)
                    return false;
            }
        }
        return true;
    }

    void UpdateUI()
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                TMP_Text textComponent = buttons[row, col].GetComponentInChildren<TMP_Text>();
                if (textComponent != null)
                {
                    textComponent.text = numbers[row, col].ToString();
                }

                Image btnImage = buttons[row, col].GetComponent<Image>();
                if (btnImage != null)
                {
                    btnImage.color = isBlocked[row, col] ? Color.green : Color.blue;
                }
            }
        }
        CheckCompletedLines();
    }

    void CheckCompletedLines()
    {
        for (int i = 0; i < 9; i++)
        {
            if (!blockedRows[i] && IsRowComplete(i))
            {
                SetRowColor(i, Color.green);
                blockedRows[i] = true;
                LockRow(i);
            }

            if (!blockedCols[i] && IsColumnComplete(i))
            {
                SetColumnColor(i, Color.green);
                blockedCols[i] = true;
                LockColumn(i);
            }
        }
    }

    bool IsRowComplete(int row)
    {
        bool[] seen = new bool[10];
        for (int col = 0; col < 9; col++)
        {
            int num = numbers[row, col];
            if (num < 1 || num > 9 || seen[num]) return false;
            seen[num] = true;
        }
        return true;
    }

    bool IsColumnComplete(int col)
    {
        bool[] seen = new bool[10];
        for (int row = 0; row < 9; row++)
        {
            int num = numbers[row, col];
            if (num < 1 || num > 9 || seen[num]) return false;
            seen[num] = true;
        }
        return true;
    }

    void LockRow(int row)
    {
        for (int col = 0; col < 9; col++)
        {
            isBlocked[row, col] = true;
        }
    }

    void LockColumn(int col)
    {
        for (int row = 0; row < 9; row++)
        {
            isBlocked[row, col] = true;
        }
    }

    void OnTileClick(int row, int col)
    {
        if (isBlocked[row, col]) return;

        if (selectedRow == -1 && selectedCol == -1)
        {
            selectedRow = row;
            selectedCol = col;
        }
        else
        {
            if (!IsRowOrColumnLocked(row, col) && !IsRowOrColumnLocked(selectedRow, selectedCol))
            {
                SwapNumbers(selectedRow, selectedCol, row, col);
            }
            selectedRow = -1;
            selectedCol = -1;
        }
    }

    void SwapNumbers(int row1, int col1, int row2, int col2)
    {
        if (!isBlocked[row1, col1] && !isBlocked[row2, col2])
        {
            int temp = numbers[row1, col1];
            numbers[row1, col1] = numbers[row2, col2];
            numbers[row2, col2] = temp;
            UpdateUI();
        }
    }

    bool IsRowOrColumnLocked(int row, int col)
    {
        return blockedRows[row] || blockedCols[col];
    }

    void SetRowColor(int row, Color color)
    {
        for (int col = 0; col < 9; col++)
        {
            Image btnImage = buttons[row, col].GetComponent<Image>();
            if (btnImage != null)
            {
                btnImage.color = color;
            }
        }
    }

    void SetColumnColor(int col, Color color)
    {
        for (int row = 0; row < 9; row++)
        {
            Image btnImage = buttons[row, col].GetComponent<Image>();
            if (btnImage != null)
            {
                btnImage.color = color;
            }
        }
    }
}
