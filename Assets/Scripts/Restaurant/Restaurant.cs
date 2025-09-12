using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Restaurant : MonoBehaviour
{
    [Header("Restaurant Settings")]
    public string restaurantName = "Shawarma Granny";
    public int reputation = 0;
    public int maxCustomersPerDay = 10;

    [Header("Food Station Settings")]
    public Transform kitchenArea;
    public Transform counterArea;
    public Transform seatingArea;

    // References
    private Queue<Customer> waitingCustomers = new Queue<Customer>();
    private List<Customer> activeCustomers = new List<Customer>();
    
    [Header("Restaurant Progression")]
    public int restaurantLevel = 1;
    public int equipmentQuality = 1;
    public float speedMultiplier = 1f;

    // Ingredients and recipes
    public List<FoodItem> availableIngredients = new List<FoodItem>();
    public List<Recipe> availableRecipes = new List<Recipe>();

    // Customer spawning
    private float customerSpawnTimer = 0f;
    public float timeBetweenCustomers = 20f;
    
    private void Start()
    {
        InitializeRestaurant();
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            HandleCustomerSpawning();
            ManageActiveCustomers();
        }
    }

    private void InitializeRestaurant()
    {
        // Set up initial ingredients and recipes
        Debug.Log($"Restaurant {restaurantName} initialized!");
    }

    private void HandleCustomerSpawning()
    {
        customerSpawnTimer += Time.deltaTime;
        
        if (customerSpawnTimer >= timeBetweenCustomers && 
            waitingCustomers.Count + activeCustomers.Count < maxCustomersPerDay)
        {
            SpawnCustomer();
            customerSpawnTimer = 0f;
        }
    }

    private void SpawnCustomer()
    {
        // In a real implementation, this would instantiate a customer prefab
        Debug.Log("New customer arrived at the restaurant");
        // Customer newCustomer = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity).GetComponent<Customer>();
        // waitingCustomers.Enqueue(newCustomer);
    }

    private void ManageActiveCustomers()
    {
        // Check if counter is available and there are waiting customers
        if (waitingCustomers.Count > 0 && counterArea != null)
        {
            // Dequeue customer and serve them
            // Customer customer = waitingCustomers.Dequeue();
            // activeCustomers.Add(customer);
            // customer.MoveToCounter(counterArea.position);
        }
        
        // Manage other customer states and interactions
    }

    public void UpgradeRestaurant()
    {
        if (GameManager.Instance.SpendMoney(restaurantLevel * 100))
        {
            restaurantLevel++;
            maxCustomersPerDay += 2;
            Debug.Log($"Restaurant upgraded to level {restaurantLevel}!");
        }
        else
        {
            Debug.Log("Not enough money to upgrade restaurant!");
        }
    }

    public void UpgradeEquipment()
    {
        if (GameManager.Instance.SpendMoney(equipmentQuality * 50))
        {
            equipmentQuality++;
            speedMultiplier += 0.1f;
            Debug.Log($"Equipment upgraded to level {equipmentQuality}!");
        }
        else
        {
            Debug.Log("Not enough money to upgrade equipment!");
        }
    }

    public void CustomerFinished(Customer customer, int satisfaction)
    {
        activeCustomers.Remove(customer);
        GameManager.Instance.CustomerServed(satisfaction);
        reputation = Mathf.Clamp(reputation + (satisfaction - 3), 0, 100);
    }

    public void EndDay()
    {
        waitingCustomers.Clear();
        foreach (var customer in activeCustomers)
        {
            // Handle any remaining customers
            Destroy(customer.gameObject);
        }
        activeCustomers.Clear();
        GameManager.Instance.EndDay();
    }
} 