using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    // Singleton pattern
    public static UIManager Instance { get; private set; }
    
    [Header("Main Game Panels")]
    public GameObject mainMenuPanel;
    public GameObject gamePanel;
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    
    [Header("HUD Elements")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI customersServedText;
    public TextMeshProUGUI reputationText;
    
    [Header("Customer UI")]
    public GameObject customerOrderPrefab;
    public Transform customerOrdersContainer;
    private Dictionary<Customer, GameObject> customerOrders = new Dictionary<Customer, GameObject>();
    
    [Header("Food Preparation UI")]
    public GameObject foodPreparationPanel;
    public Transform ingredientsContainer;
    public Button completeFoodButton;
    
    private GameManager gameManager;
    private Restaurant restaurant;
    
    private void Awake()
    {
        // Singleton setup
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
    
    private void Start()
    {
        // Get references
        gameManager = FindObjectOfType<GameManager>();
        restaurant = FindObjectOfType<Restaurant>();
        
        // Check which scene we're in
        if (gameManager != null && gameManager.CurrentState == GameManager.GameState.MainMenu)
        {
            ShowMainMenu();
        }
        else if (gameManager != null && gameManager.CurrentState == GameManager.GameState.Playing)
        {
            ShowGameUI();
        }
        
        // Add listeners
        if (completeFoodButton != null)
            completeFoodButton.onClick.AddListener(CompleteFoodPreparation);
    }
    
    private void Update()
    {
        // Update HUD if game is active
        if (gameManager.CurrentState == GameManager.GameState.Playing)
        {
            UpdateHUD();
        }
    }
    
    public void UpdateHUD()
    {
        if (moneyText != null)
            moneyText.text = $"${gameManager.Money}";
        
        if (dayText != null)
            dayText.text = $"Day {gameManager.CurrentDay}";
        
        if (customersServedText != null)
            customersServedText.text = $"Customers: {gameManager.CustomersSatisfied}";
        
        if (reputationText != null && restaurant != null)
            reputationText.text = $"Reputation: {restaurant.reputation}%";
    }
    
    public void ShowMainMenu()
    {
        SetActivePanels(mainMenuPanel);
    }
    
    public void ShowGameUI()
    {
        SetActivePanels(gamePanel);
    }
    
    public void ShowPauseMenu()
    {
        SetActivePanels(pausePanel, gamePanel);
    }
    
    public void ShowGameOver()
    {
        SetActivePanels(gameOverPanel);
    }
    
    private void SetActivePanels(params GameObject[] activePanels)
    {
        // Deactivate all panels
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (foodPreparationPanel != null) foodPreparationPanel.SetActive(false);
        
        // Activate specified panels
        foreach (GameObject panel in activePanels)
        {
            if (panel != null)
                panel.SetActive(true);
        }
    }
    
    // Button click handlers
    public void OnStartGameClicked()
    {
        gameManager.StartGame();
        ShowGameUI();
    }
    
    public void OnPauseClicked()
    {
        gameManager.PauseGame();
        
        if (gameManager.CurrentState == GameManager.GameState.Paused)
            ShowPauseMenu();
        else
            ShowGameUI();
    }
    
    public void OnResumeClicked()
    {
        gameManager.PauseGame(); // Unpause
        ShowGameUI();
    }
    
    public void OnExitGameClicked()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    public void OnEndDayClicked()
    {
        if (restaurant != null)
            restaurant.EndDay();
    }
    
    // Food preparation
    public void ShowFoodPreparationPanel(Recipe recipe)
    {
        SetActivePanels(foodPreparationPanel, gamePanel);
        
        // TODO: Populate ingredients based on recipe
    }
    
    private void CompleteFoodPreparation()
    {
        // TODO: Create food item based on selected ingredients
        foodPreparationPanel.SetActive(false);
    }
    
    // Customer order UI
    public void AddCustomerOrder(Customer customer, Recipe recipe)
    {
        if (customerOrderPrefab != null && customerOrdersContainer != null)
        {
            GameObject orderUI = Instantiate(customerOrderPrefab, customerOrdersContainer);
            
            // TODO: Set up order UI with recipe details
            
            customerOrders.Add(customer, orderUI);
        }
    }
    
    public void RemoveCustomerOrder(Customer customer)
    {
        if (customerOrders.TryGetValue(customer, out GameObject orderUI))
        {
            Destroy(orderUI);
            customerOrders.Remove(customer);
        }
    }
    
    // UI feedback
    public void ShowNotification(string message, float duration = 3f)
    {
        Debug.Log($"Notification: {message}");
        // TODO: Show an actual UI notification
    }
} 