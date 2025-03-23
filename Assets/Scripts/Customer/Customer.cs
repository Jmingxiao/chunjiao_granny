using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Customer : MonoBehaviour
{
    public enum CustomerState { Waiting, OrderingFood, WaitingForFood, Eating, Leaving, Gone }
    
    [Header("Customer Settings")]
    public float patienceLevel = 100f;
    public float patienceDecayRate = 5f;
    public float eatingSpeed = 1f;
    public int maxSatisfaction = 5;
    
    [Header("Movement")]
    public float moveSpeed = 2f;
    
    // Current state
    private CustomerState state;
    public CustomerState CurrentState => state;
    
    // Food preferences
    private List<string> preferredIngredients = new List<string>();
    private Recipe desiredOrder;
    private FoodItem receivedOrder;
    
    // Timers
    private float waitingTimer = 0f;
    private float eatingTimer = 0f;
    
    private void Start()
    {
        SetCustomerState(CustomerState.Waiting);
        GeneratePreferences();
    }
    
    private void Update()
    {
        switch (state)
        {
            case CustomerState.Waiting:
                HandleWaiting();
                break;
            case CustomerState.OrderingFood:
                // Handled by events/triggers
                break;
            case CustomerState.WaitingForFood:
                HandleWaitingForFood();
                break;
            case CustomerState.Eating:
                HandleEating();
                break;
            case CustomerState.Leaving:
                // Movement handled by MoveTowards
                break;
        }
    }
    
    private void GeneratePreferences()
    {
        // Would typically use available recipes from the game's database
        // For now, we just generate a simple preference
        Debug.Log("Customer preferences generated");
    }
    
    private void HandleWaiting()
    {
        // Decrease patience while waiting
        patienceLevel -= patienceDecayRate * Time.deltaTime;
        
        if (patienceLevel <= 0)
        {
            LeaveRestaurant(0); // Leave unsatisfied
        }
    }
    
    private void HandleWaitingForFood()
    {
        // Decrease patience faster while waiting for food
        patienceLevel -= patienceDecayRate * 1.5f * Time.deltaTime;
        
        if (patienceLevel <= 0)
        {
            LeaveRestaurant(0); // Leave unsatisfied
        }
    }
    
    private void HandleEating()
    {
        eatingTimer += Time.deltaTime * eatingSpeed;
        
        if (eatingTimer >= 10f) // Arbitrary eating time
        {
            SetCustomerState(CustomerState.Leaving);
            StartCoroutine(LeaveAfterEating());
        }
    }
    
    public void MoveToCounter(Vector3 counterPosition)
    {
        StartCoroutine(MoveTowards(counterPosition, () => {
            SetCustomerState(CustomerState.OrderingFood);
        }));
    }
    
    public void MoveToTable(Vector3 tablePosition)
    {
        StartCoroutine(MoveTowards(tablePosition, () => {
            SetCustomerState(CustomerState.Eating);
        }));
    }
    
    public void LeaveRestaurant(int satisfaction)
    {
        SetCustomerState(CustomerState.Leaving);
        // Signal to the restaurant that this customer is done
        StartCoroutine(Leave(satisfaction));
    }
    
    private IEnumerator MoveTowards(Vector3 targetPosition, System.Action onComplete)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, 
                targetPosition, 
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }
        
        onComplete?.Invoke();
    }
    
    private IEnumerator Leave(int satisfaction)
    {
        Vector3 exitPosition = new Vector3(10f, 0f, 0f); // Example exit point
        
        yield return StartCoroutine(MoveTowards(exitPosition, null));
        
        // Notify restaurant
        FindObjectOfType<Restaurant>().CustomerFinished(this, satisfaction);
        SetCustomerState(CustomerState.Gone);
        Destroy(gameObject);
    }
    
    private IEnumerator LeaveAfterEating()
    {
        // Calculate satisfaction based on food match with preferences
        int satisfaction = CalculateSatisfaction();
        yield return StartCoroutine(Leave(satisfaction));
    }
    
    private int CalculateSatisfaction()
    {
        // In a full implementation, this would check how well the received order
        // matches the customer's preferences
        return Mathf.Clamp(Random.Range(1, maxSatisfaction + 1), 1, maxSatisfaction);
    }
    
    public void ReceiveFood(FoodItem food)
    {
        receivedOrder = food;
        SetCustomerState(CustomerState.Eating);
        eatingTimer = 0f;
    }
    
    public Recipe GetOrder()
    {
        return desiredOrder;
    }
    
    private void SetCustomerState(CustomerState newState)
    {
        state = newState;
    }
} 