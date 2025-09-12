using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    
    [Header("Input Settings")]
    public float clickHoldThreshold = 0.2f;
    
    // Input state tracking
    private bool isMouseDown = false;
    private float mouseDownTime = 0f;
    private Vector3 mouseDownPosition;
    
    // Callbacks
    public delegate void ClickAction(Vector3 position);
    public event ClickAction OnClick;
    
    public delegate void DragAction(Vector3 startPosition, Vector3 currentPosition);
    public event DragAction OnDrag;
    
    public delegate void DragEndAction(Vector3 startPosition, Vector3 endPosition);
    public event DragEndAction OnDragEnd;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;
            
        HandleMouseInput();
        HandleKeyboardInput();
    }
    
    private void HandleMouseInput()
    {
        // Mouse down
        if (Input.GetMouseButtonDown(0))
        {
            isMouseDown = true;
            mouseDownTime = Time.time;
            mouseDownPosition = GetMouseWorldPosition();
        }
        
        // Mouse held down (drag)
        if (isMouseDown && Time.time - mouseDownTime > clickHoldThreshold)
        {
            Vector3 currentMousePosition = GetMouseWorldPosition();
            OnDrag?.Invoke(mouseDownPosition, currentMousePosition);
        }
        
        // Mouse up
        if (Input.GetMouseButtonUp(0) && isMouseDown)
        {
            Vector3 mouseUpPosition = GetMouseWorldPosition();
            float clickDuration = Time.time - mouseDownTime;
            
            // If quick click
            if (clickDuration < clickHoldThreshold)
            {
                OnClick?.Invoke(mouseUpPosition);
            }
            else
            {
                // End of drag
                OnDragEnd?.Invoke(mouseDownPosition, mouseUpPosition);
            }
            
            isMouseDown = false;
        }
    }
    
    private void HandleKeyboardInput()
    {
        // Pause game with Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameManager.Instance.PauseGame();
        }
        
        // Other keyboard shortcuts
        if (Input.GetKeyDown(KeyCode.R)) // Example: 'R' to restart day
        {
            FindObjectOfType<Restaurant>()?.EndDay();
        }
    }
    
    public Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z; // Set distance from camera
        return Camera.main.ScreenToWorldPoint(mousePos);
    }
    
    // Helper method to check if mouse is over UI element
    public bool IsPointerOverUI()
    {
        // In a real implementation, this would use the EventSystem
        // return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        return false;
    }
    
    // Helper method to get clicked object
    public GameObject GetClickedObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
        
        if (hit.collider != null)
        {
            return hit.collider.gameObject;
        }
        
        return null;
    }
} 