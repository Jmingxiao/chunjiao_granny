using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // For .Any() and .Contains() if needed

public class CustomerSpawner : MonoBehaviour
{
    // Assign these in the Unity Editor
    [Header("Prefabs")]
    [Tooltip("The CustomerNPC prefab to be spawned.")]
    [SerializeField] private GameObject customerNPCPrefab;
    [Tooltip("The Topic Bubble UI prefab (contains CustomerMenuDisplay script).")]
    [SerializeField] private GameObject topicBubbleUIPrefab;

    [Header("Spawn Settings")]
    [Tooltip("Minimum time (seconds) between customer spawns.")]
    [SerializeField] private float minSpawnFrequency = 3f;
    [Tooltip("Maximum time (seconds) between customer spawns.")]
    [SerializeField] private float maxSpawnFrequency = 10f;
    [Tooltip("Maximum number of customers allowed on screen at once.")]
    [SerializeField] private int maxCustomersOnScreen = 5;
    [Tooltip("The X coordinate from which customers will spawn (e.g., far left of screen).")]
    [SerializeField] private float spawnXPosition = -10f;
    [Tooltip("Minimum Y coordinate for customer spawn position.")]
    [SerializeField] private float spawnYMin = -3f;
    [Tooltip("Maximum Y coordinate for customer spawn position.")]
    [SerializeField] private float spawnYMax = 3f;

    [Header("Menu Data")]
    [Tooltip("List of all available CustomerMenu ScriptableObjects.")]
    [SerializeField] private List<CustomerMenu> availableMenus;

    // Internal object pool for customers
    private List<CustomerNPC> customerPool = new List<CustomerNPC>();
    private List<CustomerNPC> activeCustomers = new List<CustomerNPC>();
    private Coroutine spawnCoroutine;

    private void Start()
    {
        // Validate prefabs and menus
        if (customerNPCPrefab == null)
        {
            Debug.LogError("CustomerNPC Prefab is not assigned in CustomerSpawner!", this);
            return;
        }
        if (topicBubbleUIPrefab == null)
        {
            Debug.LogError("Topic Bubble UI Prefab is not assigned in CustomerSpawner!", this);
            return;
        }
        if (availableMenus == null || availableMenus.Count == 0)
        {
            Debug.LogError("No Customer Menus assigned in CustomerSpawner! Please create and assign some ScriptableObjects.", this);
            return;
        }

        InitializeCustomerPool(maxCustomersOnScreen * 2); // Initialize pool with double the max customers
        spawnCoroutine = StartCoroutine(SpawnCustomersRoutine());
    }

    private void OnDisable()
    {
        // Stop the spawning coroutine when the spawner is disabled
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
    }

    /// <summary>
    /// Initializes the customer object pool.
    /// </summary>
    /// <param name="poolSize">The initial size of the pool.</param>
    private void InitializeCustomerPool(int poolSize)
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(customerNPCPrefab, transform);
            CustomerNPC customer = obj.GetComponent<CustomerNPC>();
            if (!customer)
            {
                Debug.LogError("CustomerNPC prefab does not have a CustomerNPC component!", customerNPCPrefab);
                Destroy(obj);
                continue;
            }
            customer.Initialize(this, topicBubbleUIPrefab); // Pass reference to spawner and bubble prefab
            obj.SetActive(false); // Deactivate immediately
            customerPool.Add(customer);
        }
        Debug.Log($"Initialized customer pool with {poolSize} customers.");
    }

    /// <summary>
    /// Coroutine to continuously spawn customers.
    /// </summary>
    private IEnumerator SpawnCustomersRoutine()
    {
        while (true)
        {
            // Wait for a random interval before attempting to spawn
            float spawnDelay = Random.Range(minSpawnFrequency, maxSpawnFrequency);
            yield return new WaitForSeconds(spawnDelay);

            // Only spawn if we haven't reached the maximum number of active customers
            if (activeCustomers.Count < maxCustomersOnScreen)
            {
                SpawnCustomer();
            }
        }
    }

    /// <summary>
    /// Spawns a customer from the pool.
    /// </summary>
    private void SpawnCustomer()
    {
        CustomerNPC customerToSpawn = GetCustomerFromPool();

        if (customerToSpawn != null)
        {
            Vector3 spawnPosition = new Vector3(spawnXPosition, Random.Range(spawnYMin, spawnYMax), 0f);
            customerToSpawn.transform.position = spawnPosition;

            // Assign a random menu to the customer
            CustomerMenu selectedMenu = availableMenus[Random.Range(0, availableMenus.Count)];
            customerToSpawn.SetMenu(selectedMenu);

            customerToSpawn.gameObject.SetActive(true); // Activate the customer
            activeCustomers.Add(customerToSpawn);
            Debug.Log($"Spawned a new customer at {spawnPosition}. Active customers: {activeCustomers.Count}");
        }
        else
        {
            Debug.LogWarning("No available customer in the pool to spawn. Consider increasing pool size.");
        }
    }

    /// <summary>
    /// Retrieves an inactive customer from the pool.
    /// </summary>
    /// <returns>An inactive CustomerNPC, or null if none are available.</returns>
    private CustomerNPC GetCustomerFromPool()
    {
        // Find the first inactive customer in the pool
        CustomerNPC customer = customerPool.FirstOrDefault(c => !c.gameObject.activeInHierarchy);
        return customer;
    }

    /// <summary>
    /// Returns a customer to the pool, deactivating it and removing it from the active list.
    /// </summary>
    /// <param name="customer">The customer to return.</param>
    public void ReturnCustomerToPool(CustomerNPC customer)
    {
        if (customer == null) return;

        if (activeCustomers.Remove(customer))
        {
            customer.gameObject.SetActive(false); // Deactivate the customer
            Debug.Log($"Customer returned to pool. Active customers: {activeCustomers.Count}");
            // Reset customer state if necessary before deactivating
        }
        else
        {
            Debug.LogWarning("Attempted to return a customer not found in the activeCustomers list.");
        }
    }
}
