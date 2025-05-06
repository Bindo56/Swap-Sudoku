
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static SudukoCell;

public class SudukoCell : MonoBehaviour , IDraggable, IHoldable, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public TextMeshProUGUI numberText;
    public int row, col;
    private SudukoGrid gridManager;
    public bool IsFixed { get; private set; } = false; // Default to false


    // Drag related 
    private Vector3 originalPosition;
    private Transform originalParent;
    Vector3 originalScale;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private bool isDragging = false;
    private float holdTime = 0f;
  //  private float holdThreshold = 0.5f; 
    private bool isHolding = false;

    private Vector2 touchStartPosition;
    private const float dragThreshold = 10f; 
    private bool hasMovedBeyondThreshold = false;

  
    public interface IDraggable
    {
        void OnBeginDrag(PointerEventData eventData);
        void OnDrag(PointerEventData eventData);
        void OnEndDrag(PointerEventData eventData);
    }

    public interface IHoldable
    {
        void OnPointerDown(PointerEventData eventData);
        void OnPointerUp(PointerEventData eventData);
    }

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

       
        canvas = FindObjectOfType<Canvas>();

       
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
           
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                Debug.LogWarning("Canvas render mode should ScreenSpaceOverlay");  //optional  warning  //ignore it as cell is working fine
            }
        }
    }

    void Start()
    {
        gridManager = FindObjectOfType<SudukoGrid>();

       
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        originalScale = transform.localScale;
    }

    void Update()
    {
        // Check if we're holding the cell
       /* if (isHolding && !isDragging)
        {
            holdTime += Time.deltaTime;

            // If we've held long enough, start dragging
            if (holdTime >= holdThreshold && !IsFixed && hasMovedBeyondThreshold)
            {
                isDragging = true;
                PrepareForDrag();
               
            }
        }*/
    }

    private void FixedUpdate()
    {
        if (isHolding && !isDragging)
        {
          //  holdTime += Time.deltaTime;

                isDragging = true;
                PrepareForDrag();
            // If we've held long enough, start dragging
           /* if (holdTime >= holdThreshold && !IsFixed && hasMovedBeyondThreshold)
            {

            }*/
        }
    }

    // Interface implementations
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsFixed)
        {
            isHolding = true;
            holdTime = 0f;
            touchStartPosition = eventData.position;
            hasMovedBeyondThreshold = false;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHolding = false;
        holdTime = 0f;

        if (!isDragging)
        {
            OnCellClicked();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
       
        if (Vector2.Distance(touchStartPosition, eventData.position) > dragThreshold)
        {
            hasMovedBeyondThreshold = true;
        }

        if (!IsFixed && (isDragging || hasMovedBeyondThreshold))
        {
            isDragging = true;
            originalPosition = transform.position;
            originalParent = transform.parent;

           
            canvasGroup.alpha = 0.6f;
            canvasGroup.blocksRaycasts = false;

           
            transform.SetParent(canvas.transform);
            transform.SetAsLastSibling();
        }
        else
        {
            
            eventData.pointerDrag = null;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
      
        if (!hasMovedBeyondThreshold && Vector2.Distance(touchStartPosition, eventData.position) > dragThreshold)
        {
            hasMovedBeyondThreshold = true;
        }

        if (!IsFixed && isDragging)
        {
           
            Vector3 newPosition = eventData.position;

           
            transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);

            
            ClampToScreen();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!IsFixed && isDragging)
        {
            isDragging = false;

            
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;

           
            GameObject hitObject = GetObjectUnderPointer(eventData);
            if (hitObject != null)
            {
                SudukoCell targetCell = hitObject.GetComponent<SudukoCell>();
                if (targetCell != null && targetCell != this && !targetCell.IsFixed)
                {
                   
                    gridManager.SwapCells(this, targetCell);
                }
            }

           
            transform.SetParent(originalParent);
            transform.position = originalPosition;
        }
    }

   
    private void PrepareForDrag()
    {
       
        originalPosition = transform.position;
        originalParent = transform.parent;
    }

    private GameObject GetObjectUnderPointer(PointerEventData eventData) //raycast to get the object under the pointer
    {
       
        var results = new System.Collections.Generic.List<RaycastResult>();  
        canvasGroup.blocksRaycasts = false;
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = eventData.position;

        EventSystem.current.RaycastAll(pointerData, results);
      //  Debug.Log("Raycast results: " + results.Count);

       
        canvasGroup.blocksRaycasts = true;

        foreach (var result in results)
        {
            GameObject hitObject = result.gameObject;

            if (hitObject == gameObject)
                continue;

            if (hitObject.GetComponent<TextMeshProUGUI>() != null)
                continue;

            TextMeshProUGUI parentTMP = hitObject.GetComponentInParent<TextMeshProUGUI>();
            if (parentTMP != null)
                continue;

          //  Debug.Log("Hit object: " + hitObject.name);
            return hitObject;
        }

        return null;
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
            GetComponent<Image>().color = new Color(0.66f, 0.36f, 0.36f); // Highlight fixed cells
        }
        else
        {
            GetComponent<Image>().color = Color.white; // Reset non-fixed cells
        }
    }

   
    private void ClampToScreen()
    {
        Vector3 position = transform.position;

      
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

      
        RectTransform rectTransform = GetComponent<RectTransform>();
        float width = rectTransform.rect.width * rectTransform.lossyScale.x;
        float height = rectTransform.rect.height * rectTransform.lossyScale.y;

        //  screen bounds
        position.x = Mathf.Clamp(position.x, width / 2, screenWidth - width / 2);
        position.y = Mathf.Clamp(position.y, height / 2, screenHeight - height / 2);

        transform.position = position;
    }

    public int GetNumber()
    {
        return string.IsNullOrEmpty(numberText.text) ? 0 : int.Parse(numberText.text);
    }

    public void OnCellClicked()
    {
        // Add subtle animation for feedback
        StartCoroutine(ClickFeedback());

        // Forward to grid manager
        gridManager.HandleCellClick(this);
    }

    private IEnumerator ClickFeedback()
    {
        // Save original scale
        // originalScale = transform.localScale;

        Vector3 shrinkSize = originalScale;

        
        transform.localScale = shrinkSize * 0.95f;

      
        yield return new WaitForSeconds(0.2f);

      //  Debug.Log("Click feedback");
       
        transform.localScale = originalScale;

        
    }
    public void SetColor(Color color)
    {
        GetComponent<Image>().color = color;
    }

    public Color GetColor()
    {
        return GetComponent<Image>().color;
    }

}
