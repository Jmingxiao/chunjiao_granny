using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KitchenStation : MonoBehaviour
{
    public enum StationType { Meat, Vegetable, Assembly, Counter, Seating }
    
    [Header("Station Settings")]
    public StationType stationType;
    public string stationName;
    public float processingSpeed = 1f;
    public int maxCapacity = 1;
    
    [Header("Status")]
    public bool isOccupied = false;
    public bool isProcessing = false;
    
    // References
    private SpriteRenderer spriteRenderer;
    private List<FoodItem> currentItems = new List<FoodItem>();
    private float processingTimer = 0f;
    
    // Visual feedback
    public Color idleColor = Color.white;
    public Color occupiedColor = Color.yellow;
    public Color processingColor = Color.green;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
    }
    
    private void Start()
    {
        UpdateVisuals();
    }
    
    private void Update()
    {
        if (isProcessing)
        {
            HandleProcessing();
        }
        
        // Handle touch input for mobile
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            HandleTouchInput();
        }
        
        UpdateVisuals();
    }
    
    public bool CanAcceptItem(FoodItem item)
    {
        if (isProcessing || currentItems.Count >= maxCapacity)
            return false;
            
        switch (stationType)
        {
            case StationType.Meat:
                return item.type == FoodItem.FoodType.Meat;
            case StationType.Vegetable:
                return item.type == FoodItem.FoodType.Vegetable;
            case StationType.Assembly:
                return true; // Can accept any ingredient for assembly
            case StationType.Counter:
                return false; // Counter doesn't process ingredients
            case StationType.Seating:
                return false; // Seating is for customers, not food items
            default:
                return false;
        }
    }
    
    public void AddItem(FoodItem item)
    {
        if (CanAcceptItem(item))
        {
            currentItems.Add(item);
            isOccupied = true;
            
            // Automatically start processing
            StartProcessing();
        }
    }
    
    public void StartProcessing()
    {
        if (currentItems.Count > 0 && !isProcessing)
        {
            isProcessing = true;
            processingTimer = 0f;
        }
    }
    
    private void HandleProcessing()
    {
        if (currentItems.Count == 0)
        {
            isProcessing = false;
            return;
        }
        
        processingTimer += Time.deltaTime * processingSpeed;
        
        // Process based on station type
        switch (stationType)
        {
            case StationType.Meat:
                ProcessMeat();
                break;
            case StationType.Vegetable:
                ProcessVegetable();
                break;
            case StationType.Assembly:
                ProcessAssembly();
                break;
            case StationType.Seating:
                ProcessSeating();
                break;
        }
    }
    
    private void ProcessMeat()
    {
        // Process each meat item
        bool allDone = true;
        
        foreach (FoodItem meat in currentItems)
        {
            if (!meat.isCooked)
            {
                allDone = false;
                meat.Cook(Time.deltaTime * processingSpeed);
            }
        }
        
        if (allDone)
        {
            isProcessing = false;
            // Signal that the meat is ready
            Debug.Log($"Meat cooked at {stationName}");
        }
    }
    
    private void ProcessVegetable()
    {
        // Vegetables are processed faster
        if (processingTimer >= 2f)
        {
            foreach (FoodItem veg in currentItems)
            {
                // Mark as "processed" in some way
                veg.preparationTime = 0;
            }
            
            isProcessing = false;
            Debug.Log($"Vegetables prepared at {stationName}");
        }
    }
    
    private void ProcessAssembly()
    {
        // Assembly takes a fixed amount of time
        if (processingTimer >= 5f)
        {
            // Create assembled food item (would be a proper recipe in full implementation)
            isProcessing = false;
            Debug.Log($"Food assembled at {stationName}");
        }
    }
    
    private void ProcessSeating()
    {
        // Seating takes longer - customers eat their food
        if (processingTimer >= 10f)
        {
            isProcessing = false;
            Debug.Log($"Customer finished eating at {stationName}");
        }
    }
    
    public List<FoodItem> RemoveItems()
    {
        List<FoodItem> items = new List<FoodItem>(currentItems);
        currentItems.Clear();
        isOccupied = false;
        isProcessing = false;
        
        return items;
    }
    
    private void UpdateVisuals()
    {
        if (spriteRenderer != null)
        {
            if (isProcessing)
            {
                spriteRenderer.color = processingColor;
            }
            else if (isOccupied)
            {
                spriteRenderer.color = occupiedColor;
            }
            else
            {
                spriteRenderer.color = idleColor;
            }
        }
    }
    
    private void OnMouseDown()
    {
        HandleClick();
    }
    
    private void HandleTouchInput()
    {
        Touch touch = Input.GetTouch(0);
        Vector3 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);
        touchPosition.z = 0;
        
        Collider2D hitCollider = Physics2D.OverlapPoint(touchPosition);
        
        if (hitCollider != null && hitCollider.gameObject == gameObject)
        {
            HandleClick();
        }
    }
    
    private void HandleClick()
    {
        // Don't process clicks while the UI is in focus
        if (InputManagerExists() && !InputManager.Instance.IsPointerOverUI())
        {
            if (isOccupied && !isProcessing)
            {
                // Pick up items
                List<FoodItem> items = RemoveItems();
                Debug.Log($"Picked up items from {stationName}");
            }
            else if (!isOccupied)
            {
                // Player could place items here
                Debug.Log($"Station {stationName} ready for items");
            }
            
            // Visual feedback for touch
            StartCoroutine(TouchFeedback());
        }
        else if (!InputManagerExists())
        {
            // Fallback if no InputManager exists
            if (isOccupied && !isProcessing)
            {
                List<FoodItem> items = RemoveItems();
                Debug.Log($"Picked up items from {stationName}");
            }
            else if (!isOccupied)
            {
                Debug.Log($"Station {stationName} ready for items");
            }
            
            // Visual feedback for touch
            StartCoroutine(TouchFeedback());
        }
    }
    
    private bool InputManagerExists()
    {
        // Check if InputManager exists before trying to access it
        return InputManager.Instance != null;
    }
    
    private IEnumerator TouchFeedback()
    {
        // Simple visual feedback for touch
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.7f); // Flash white
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
        else
        {
            yield return null;
        }
    }
} 