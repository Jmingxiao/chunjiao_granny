using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CustomerNPC : MonoBehaviour
{
    // These are assigned by the CustomerSpawner
    private CustomerSpawner customerSpawner;
    private GameObject topicBubbleUIPrefab;

    // Public property to access the customer's menu data
    public CustomerMenu MenuData { get; private set; }
    
    // Recipe相关
    private Recipe desiredRecipe;
    public Recipe DesiredRecipe => desiredRecipe;

    [Header("Customer Specific Settings")]
    [Tooltip("Default time (seconds) a customer waits before leaving if their order isn't taken.")]
    [SerializeField] private float defaultWaitingTime = 15f;
    
    [Header("Recipe Settings")]
    [Tooltip("Recipe Database - 手动拖拽RecipeDatabase资源到这里")]
    [SerializeField] private RecipeDatabase recipeDatabase;
    
    [Header("Debug Info")]
    [Tooltip("显示顾客需要的菜品名称（仅用于调试）")]
    public string dishCustomerRequire = null;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float reachThreshold = 0.1f; // Distance to consider "reached" a position
    
    // Movement states
    private enum CustomerState
    {
        MovingToSeat,
        WaitingInQueue,
        Seated,
        Leaving
    }
    
    private CustomerState currentState = CustomerState.MovingToSeat;
    private Transform targetSeat;
    private Vector3 queuePosition;
    private int queueIndex = -1;
    
    private GameObject currentTopicBubbleInstance;
    private Coroutine currentWaitingCoroutine;
    private Coroutine currentOrderCountdownCoroutine;
    private Coroutine currentMovementCoroutine;
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
        hasOrderBeenTaken = false;
        currentState = CustomerState.MovingToSeat;
        dishCustomerRequire = null; // 重置调试信息
        
        // 检查RecipeDatabase是否已分配
        if (recipeDatabase == null)
        {
            Debug.LogError("RecipeDatabase未分配！请在Inspector中拖拽RecipeDatabase到Customer NPC Prefab上", this);
        }
        
        // Try to find an available seat
        Transform availableSeat = SeatManager.Instance?.GetAvailableSeat();
        
        if (availableSeat != null)
        {
            // Claim the seat and move to it
            SeatManager.Instance.OccupySeat(availableSeat, this);
            targetSeat = availableSeat;
            currentMovementCoroutine = StartCoroutine(MoveToSeat());
        }
        else
        {
            // No seats available, join the queue
            JoinQueue();
        }
    }

    /// <summary>
    /// Called when the GameObject becomes inactive (SetActive(false) for pooling).
    /// </summary>
    private void OnDisable()
    {
        CleanUp();
        
        // If customer was in queue, remove them
        if (currentState == CustomerState.WaitingInQueue)
        {
            QueueManager.Instance?.RemoveFromQueue(this);
        }
        
        // If customer had a seat, free it
        if (targetSeat != null)
        {
            SeatManager.Instance?.FreeSeat(targetSeat);
            targetSeat = null;
        }
        
        // 清空调试信息
        dishCustomerRequire = null;
    }

    private IEnumerator MoveToSeat()
    {
        currentState = CustomerState.MovingToSeat;
        
        while (Vector3.Distance(transform.position, targetSeat.position) > reachThreshold)
        {
            Vector3 direction = (targetSeat.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            
            // Optional: Flip sprite based on movement direction
            UpdateSpriteDirection(direction.x);
            
            yield return null;
        }
        
        // Reached the seat
        transform.position = targetSeat.position;
        OnReachedSeat();
    }

    private void OnReachedSeat()
    {
        currentState = CustomerState.Seated;
        
        // 随机选择一个Recipe
        SelectRandomRecipe();
        
        // 打印顾客想要的食物
        if (desiredRecipe != null)
        {
            Debug.Log($"<color=yellow>顾客已落座！想要: {desiredRecipe.recipeName}</color>");
            
            // 如果还在使用旧的MenuData系统，暂时显示bubble
            if (MenuData != null)
            {
                DisplayOrderBubble();
                currentWaitingCoroutine = StartCoroutine(DefaultWaitingRoutine());
            }
            else
            {
                // 新系统：立即进入等待食物状态
                hasOrderBeenTaken = true;
                float waitTime = 30f; // 默认等待时间
                currentOrderCountdownCoroutine = StartCoroutine(OrderCountdownRoutine(waitTime));
            }
        }
        else
        {
            Debug.LogError("无法选择Recipe！");
            LeaveRestaurant();
        }
    }
    
    /// <summary>
    /// 随机选择一个Recipe
    /// </summary>
    private void SelectRandomRecipe()
    {
        if (recipeDatabase == null || recipeDatabase.recipes.Count == 0)
        {
            Debug.LogError("RecipeDatabase为空或没有配置任何Recipe！");
            return;
        }
        
        // 基于流行度的加权随机选择
        float totalPopularity = 0f;
        foreach (var config in recipeDatabase.recipes)
        {
            totalPopularity += config.popularity;
        }
        
        float randomValue = Random.Range(0f, totalPopularity);
        float currentSum = 0f;
        
        foreach (var config in recipeDatabase.recipes)
        {
            currentSum += config.popularity;
            if (randomValue <= currentSum)
            {
                desiredRecipe = config.ToRecipe();
                // 更新调试信息
                dishCustomerRequire = desiredRecipe.recipeName;
                break;
            }
        }
        
        // 如果还是没有选中（不应该发生），选择第一个
        if (desiredRecipe == null && recipeDatabase.recipes.Count > 0)
        {
            desiredRecipe = recipeDatabase.recipes[0].ToRecipe();
            dishCustomerRequire = desiredRecipe.recipeName;
        }
    }

    private void JoinQueue()
    {
        currentState = CustomerState.WaitingInQueue;
        QueueManager.Instance?.AddToQueue(this);
        
        // Move to queue position
        if (currentMovementCoroutine != null)
            StopCoroutine(currentMovementCoroutine);
        currentMovementCoroutine = StartCoroutine(MoveToQueuePosition());
    }

    public void UpdateQueuePosition(Vector3 newPosition, int index)
    {
        queuePosition = newPosition;
        queueIndex = index;
        
        // If already in queue, update movement
        if (currentState == CustomerState.WaitingInQueue && currentMovementCoroutine != null)
        {
            StopCoroutine(currentMovementCoroutine);
            currentMovementCoroutine = StartCoroutine(MoveToQueuePosition());
        }
    }

    private IEnumerator MoveToQueuePosition()
    {
        while (Vector3.Distance(transform.position, queuePosition) > reachThreshold)
        {
            Vector3 direction = (queuePosition - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            
            UpdateSpriteDirection(direction.x);
            
            yield return null;
        }
        
        // Start checking for available seats periodically
        StartCoroutine(CheckForAvailableSeats());
    }

    private IEnumerator CheckForAvailableSeats()
    {
        while (currentState == CustomerState.WaitingInQueue)
        {
            Transform availableSeat = SeatManager.Instance?.GetAvailableSeat();
            if (availableSeat != null && queueIndex == 0) // Only first in queue can take seat
            {
                // Leave queue and take the seat
                QueueManager.Instance?.RemoveFromQueue(this);
                SeatManager.Instance.OccupySeat(availableSeat, this);
                targetSeat = availableSeat;
                
                if (currentMovementCoroutine != null)
                    StopCoroutine(currentMovementCoroutine);
                currentMovementCoroutine = StartCoroutine(MoveToSeat());
                
                yield break;
            }
            
            yield return new WaitForSeconds(0.5f); // Check every half second
        }
    }

    public bool IsWaitingForDish(CustomerMenu dishMenu)
    {
        // 检查是否已经点过餐且还没收到菜
        if (!hasOrderBeenTaken || MenuData == null)
            return false;
        
        // 检查是否是顾客点的菜
        return MenuData == dishMenu || 
            (MenuData != null && dishMenu != null && MenuData.dishName == dishMenu.dishName);
    }
    
    /// <summary>
    /// 检查是否在等待特定的Recipe
    /// </summary>
    public bool IsWaitingForRecipe(Recipe recipe)
    {
        // 检查是否已经选择了Recipe并且在等待
        if (!hasOrderBeenTaken || desiredRecipe == null || recipe == null)
            return false;
        
        // 比较Recipe名称
        return desiredRecipe.recipeName == recipe.recipeName;
    }
    
    /// <summary>
    /// 检查是否在等待特定名称的菜品
    /// </summary>
    public bool IsWaitingForDishName(string dishName)
    {
        if (!hasOrderBeenTaken)
            return false;
            
        // 优先检查Recipe系统
        if (desiredRecipe != null)
            return desiredRecipe.recipeName == dishName;
            
        // 向后兼容旧的MenuData系统
        if (MenuData != null)
            return MenuData.dishName == dishName;
            
        return false;
    }

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

    private IEnumerator DefaultWaitingRoutine()
    {
        yield return new WaitForSeconds(defaultWaitingTime);

        if (!hasOrderBeenTaken)
        {
            Debug.Log($"{MenuData.dishName} customer left because order was not taken in time.");
            LeaveRestaurant();
        }
    }

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

    private IEnumerator OrderCountdownRoutine(float countdownTime)
    {
        string dishName = desiredRecipe != null ? desiredRecipe.recipeName : 
                         MenuData != null ? MenuData.dishName : "未知菜品";
                         
        Debug.Log($"Customer for {dishName} is now waiting for their order for {countdownTime:F1} seconds.");
        yield return new WaitForSeconds(countdownTime);

        // If the order countdown finishes, the customer leaves (meaning the dish wasn't delivered in time)
        Debug.Log($"Customer for {dishName} left because dish was not delivered in time.");
        LeaveRestaurant();
    }

    public void ReceiveDish(float dishCompleteness)
    {
        // 只有在等待食物状态才能接收
        if (currentState != CustomerState.Seated || !hasOrderBeenTaken)
        {
            string rejectedDishName = desiredRecipe != null ? desiredRecipe.recipeName : 
                             MenuData != null ? MenuData.dishName : "未知菜品";
            Debug.Log($"顾客不在等待食物状态，无法接收 {rejectedDishName}");
            return;
        }
        
        // 停止等待计时器
        if (currentOrderCountdownCoroutine != null)
        {
            StopCoroutine(currentOrderCountdownCoroutine);
            currentOrderCountdownCoroutine = null;
        }

        // 计算评分
        int rating = 5; // 默认满分
        
        // 如果使用旧系统
        if (MenuData != null)
        {
            rating = MenuData.GetRating(dishCompleteness);
        }
        else
        {
            // Recipe系统的简单评分
            rating = Mathf.RoundToInt(dishCompleteness * 5);
            rating = Mathf.Clamp(rating, 1, 5);
        }
        
        string receivedDishName = desiredRecipe != null ? desiredRecipe.recipeName : 
                         MenuData != null ? MenuData.dishName : "未知菜品";
                         
        Debug.Log($"<color=green>顾客收到了 {receivedDishName}! 完成度: {dishCompleteness:P1}, 评分: {rating}</color>");

        // 这里可以添加：
        // - 播放满意/不满意动画
        // - 给予金币奖励
        // - 更新分数系统
        
        // 等待一小段时间后离开（模拟用餐）
        StartCoroutine(EatAndLeave(rating));
    }
    
    /// <summary>
    /// 接收Recipe（新方法）
    /// </summary>
    public void ReceiveRecipe(Recipe recipe, float completeness = 1f)
    {
        if (recipe == null || !IsWaitingForRecipe(recipe))
        {
            Debug.Log($"顾客不需要这个Recipe: {recipe?.recipeName}");
            return;
        }
        
        ReceiveDish(completeness);
    }
    
    /// <summary>
    /// 用餐后离开
    /// </summary>
    private IEnumerator EatAndLeave(int rating)
    {
        // 显示评分反馈
        if (rating >= 4)
            Debug.Log($"<color=green>顾客很满意！</color>");
        else if (rating >= 2)
            Debug.Log($"<color=yellow>顾客觉得还行。</color>");
        else
            Debug.Log($"<color=red>顾客不太满意...</color>");
            
        // 等待一会儿（模拟用餐）
        yield return new WaitForSeconds(1f);
        
        // 离开餐厅
        LeaveRestaurant();
    }

    private void LeaveRestaurant()
    {
        // Free the seat if customer was seated
        if (targetSeat != null && currentState == CustomerState.Seated)
        {
            SeatManager.Instance?.FreeSeat(targetSeat);
            targetSeat = null;
        }
        
        // Start leaving animation
        currentState = CustomerState.Leaving;
        
        if (currentMovementCoroutine != null)
            StopCoroutine(currentMovementCoroutine);
        currentMovementCoroutine = StartCoroutine(LeaveToLeft());
    }

    private IEnumerator LeaveToLeft()
    {
        // 向左移动直到离开屏幕
        float exitX = transform.position.x - 20f;
        
        while (transform.position.x > exitX)
        {
            transform.position += Vector3.left * moveSpeed * Time.deltaTime;
            UpdateSpriteDirection(-1); // 面向左边
            yield return null;
        }
        
        // 等待3秒后销毁
        yield return new WaitForSeconds(3f);
        
        // 清理并销毁
        CleanUp();
        Destroy(gameObject);
    }

    private void UpdateSpriteDirection(float direction)
    {
        if (direction < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);
        else if (direction > 0.01f)
            transform.localScale = new Vector3(1, 1, 1);
    }

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
        if (currentMovementCoroutine != null)
        {
            StopCoroutine(currentMovementCoroutine);
            currentMovementCoroutine = null;
        }
        if (currentTopicBubbleInstance != null)
        {
            Destroy(currentTopicBubbleInstance);
            currentTopicBubbleInstance = null;
        }
    }

    /// <summary>                                          
    /// 获取顾客是否已经点餐
    /// </summary>
    public bool HasOrderBeenTaken => hasOrderBeenTaken;
}