
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SudukoCell : MonoBehaviour 
{
    public TextMeshProUGUI numberText;
    public int row, col;
    private SudukoGrid gridManager;
    public bool IsFixed { get; private set; } = false; // Default to false

    void Start()
    {
        gridManager = FindObjectOfType<SudukoGrid>();
    }

    public void SetCoordinates(int r, int c)
    {
        row = r;
        col = c;
    }

    public void SetNumber(int number, bool isFixed = false)
    {
        numberText.text = number == 0 ? "" : number.ToString();
        numberText.fontSize = 70;
        IsFixed = isFixed;

        if (IsFixed)
        {
            GetComponent<Image>().color = Color.green; // Highlight fixed cells
        }
        else
        {
            GetComponent<Image>().color = Color.white; // Reset non-fixed cells
        }
    }

    public int GetNumber()
    {
        return string.IsNullOrEmpty(numberText.text) ? 0 : int.Parse(numberText.text);
    }

    public void OnCellClicked()
    {
        gridManager.HandleCellClick(this);
    }

    public void SetColor(Color color)
    {
        GetComponent<Image>().color = color;
    }

}
