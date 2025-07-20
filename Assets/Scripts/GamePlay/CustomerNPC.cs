using UnityEngine;
using System.Collections;

public class CustomerNPC : MonoBehaviour
{
    // These are assigned by the CustomerSpawner
    private CustomerSpawner customerSpawner;
    private GameObject topicBubbleUIPrefab;

    // Public property to access the customer's menu data
    public CustomerMenu MenuData { get; private set; }

    [Header("Customer Specific Settings")]
    [Tooltip("Default time (seconds) a customer waits before leaving if their order isn't taken.")]
    [SerializeField] private float defaultWaitingTime = 15f;

    private GameObject currentTopicBubbleInstance; // The active instance of the menu bubble
    private Coroutine currentWaitingCoroutine;
    private Coroutine currentOrderCountdownCoroutine;
    private bool hasOrderBeenTaken = false;

    /// <summary>
    /// Initializes the CustomerNPC with references to the spawner and the topic bubble prefab.
    /// This is called once when the object pool is initialized.
    /// </summary>
    /// <param name="spawner">The CustomerSpawner instance.</param>
    /// <param name="bubblePrefab">The Topic Bubble UI Prefab.</param>
    public void Initialize(CustomerSpawner spawner, GameObject bubblePrefab)
    {
        customerSpawner = spawner;
        topicBubbleUIPrefab = bubblePrefab;
    }

    /// <summary>
    /// Sets the menu for this customer. This is called by the CustomerSpawner
    /// when a customer is activated from the pool.
    /// </summary>
    /// <param name="menu">The CustomerMenu ScriptableObject for this customer's order.</param>
    public void SetMenu(CustomerMenu menu)
    {
        MenuData = menu;
    }

    /// <summary>
    /// Called when the GameObject becomes active (either from Instantiate or SetActive(true) from pool).
    /// </summary>
    private void OnEnable()
    {
        hasOrderBeenTaken = false; // Reset state
        DisplayOrderBubble(); // Show the customer's order bubble
        currentWaitingCoroutine = StartCoroutine(DefaultWaitingRoutine()); // Start default wait timer
    }

    /// <summary>
    /// Called when the GameObject becomes inactive (SetActive(false) for pooling).
    /// </summary>
    private void OnDisable()
    {
        CleanUp(); // Ensure all coroutines are stopped and bubble is destroyed
    }

    /// <summary>
    /// Displays the customer's order bubble (UI).
    /// </summary>
    private void DisplayOrderBubble()
    {
        if (MenuData == null)
        {
            Debug.LogError("CustomerNPC: Attempted to display bubble without a menu assigned!", this);
            return;
        }
        if (topicBubbleUIPrefab == null)
        {
            Debug.LogError("CustomerNPC: topicBubbleUIPrefab is not assigned! Cannot display order bubble.", this);
            return;
        }

        // Destroy any existing bubble first, in case of a pool reuse issue
        if (currentTopicBubbleInstance != null)
        {
            Destroy(currentTopicBubbleInstance);
            currentTopicBubbleInstance = null;
        }

        // Instantiate the bubble prefab
        currentTopicBubbleInstance = Instantiate(topicBubbleUIPrefab, transform);
        // Position it above the customer (adjust offset as needed)
        currentTopicBubbleInstance.transform.localPosition = new Vector3(0, 2f, 0);

        CustomerMenuDisplay menuDisplay = currentTopicBubbleInstance.GetComponent<CustomerMenuDisplay>();
        if (menuDisplay != null)
        {
            menuDisplay.SetupMenu(MenuData, this); // Pass menu data and this NPC reference
        }
        else
        {
            Debug.LogError("CustomerMenuDisplay component not found on TopicBubbleUI Prefab!", topicBubbleUIPrefab);
        }
    }

    /// <summary>
    /// Coroutine for the customer's initial waiting period.
    /// </summary>
    private IEnumerator DefaultWaitingRoutine()
    {
        yield return new WaitForSeconds(defaultWaitingTime);

        if (!hasOrderBeenTaken)
        {
            Debug.Log($"{MenuData.dishName} customer left because order was not taken in time.");
            LeaveRestaurant();
        }
    }

    /// <summary>
    /// Called by CustomerMenuDisplay when the player clicks the order bubble.
    /// </summary>
    public void TakeOrder()
    {
        if (hasOrderBeenTaken) return; // Prevent taking order multiple times

        hasOrderBeenTaken = true;

        // Stop the default waiting coroutine
        if (currentWaitingCoroutine != null)
        {
            StopCoroutine(currentWaitingCoroutine);
            currentWaitingCoroutine = null;
        }

        // Destroy the order bubble UI
        if (currentTopicBubbleInstance != null)
        {
            Destroy(currentTopicBubbleInstance);
            currentTopicBubbleInstance = null;
        }

        // Start the order countdown based on the dish's calculated waiting time
        currentOrderCountdownCoroutine = StartCoroutine(OrderCountdownRoutine(MenuData.CalculatedOrderCountdownTime));
    }

    /// <summary>
    /// Coroutine for the customer's waiting period AFTER their order has been taken.
    /// </summary>
    /// <param name="countdownTime">The total time the customer will wait.</param>
    private IEnumerator OrderCountdownRoutine(float countdownTime)
    {
        Debug.Log($"Customer for {MenuData.dishName} is now waiting for their order for {countdownTime:F1} seconds.");
        yield return new WaitForSeconds(countdownTime);

        // If the order countdown finishes, the customer leaves (meaning the dish wasn't delivered in time)
        Debug.Log($"Customer for {MenuData.dishName} left because dish was not delivered in time.");
        LeaveRestaurant();
    }

    /// <summary>
    /// Placeholder method for when the player delivers a dish to this customer.
    /// You would implement dish checking and rating logic here.
    /// </summary>
    /// <param name="dishCompleteness">A value from 0.0 to 1.0 indicating how complete the dish is.</param>
    public void ReceiveDish(float dishCompleteness)
    {
        // Stop any active waiting/countdown coroutines
        CleanUp();

        int rating = MenuData.GetRating(dishCompleteness);
        Debug.Log($"Customer for {MenuData.dishName} received dish with completeness {dishCompleteness:P1}. Rating: {rating}");

        // Here, you would typically add the rating to the player's score,
        // play an animation, give money, etc.

        LeaveRestaurant(); // Customer leaves after receiving their dish
    }

    /// <summary>
    /// Handles the customer leaving the restaurant (returning to pool).
    /// </summary>
    private void LeaveRestaurant()
    {
        CleanUp(); // Ensure all coroutines are stopped and bubble is destroyed
        customerSpawner.ReturnCustomerToPool(this); // Return self to the pool
    }

    /// <summary>
    /// Stops all running coroutines and cleans up the order bubble.
    /// </summary>
    private void CleanUp()
    {
        if (currentWaitingCoroutine != null)
        {
            StopCoroutine(currentWaitingCoroutine);
            currentWaitingCoroutine = null;
        }
        if (currentOrderCountdownCoroutine != null)
        {
            StopCoroutine(currentOrderCountdownCoroutine);
            currentOrderCountdownCoroutine = null;
        }
        if (currentTopicBubbleInstance != null)
        {
            Destroy(currentTopicBubbleInstance); // Destroy the UI bubble instance
            currentTopicBubbleInstance = null;
        }
    }
}
