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

    [Header("Spawn Timing")]
    [Tooltip("生成模式：固定间隔或随机间隔")]
    [SerializeField] private SpawnMode spawnMode = SpawnMode.RandomInterval;
    
    [Tooltip("固定生成间隔（秒）- 仅在Fixed Interval模式下使用")]
    [SerializeField] private float fixedSpawnInterval = 5f;
    
    [Tooltip("最小生成间隔（秒）- 仅在Random Interval模式下使用")]
    [SerializeField] private float minSpawnInterval = 3f;
    [Tooltip("最大生成间隔（秒）- 仅在Random Interval模式下使用")]
    [SerializeField] private float maxSpawnInterval = 10f;
    
    [Header("Advanced Timing")]
    [Tooltip("使用自定义生成时间表")]
    [SerializeField] private bool useCustomSchedule = false;
    [Tooltip("自定义生成时间表")]
    [SerializeField] private List<SpawnSchedule> customSpawnSchedule = new List<SpawnSchedule>();
    
    [Header("Spawn Limits")]
    [Tooltip("同时存在的最大顾客数")]
    [SerializeField] private int maxCustomersOnScreen = 5;
    [Tooltip("每波生成的顾客数量")]
    [SerializeField] private int customersPerWave = 1;
    
    [Header("Spawn Position")]
    [Tooltip("顾客生成的X坐标")]
    [SerializeField] private float spawnXPosition = -10f;
    [Tooltip("顾客生成Y坐标最小值")]
    [SerializeField] private float spawnYMin = -3f;
    [Tooltip("顾客生成Y坐标最大值")]
    [SerializeField] private float spawnYMax = 3f;

    [Header("Menu Data")]
    [Tooltip("List of all available CustomerMenu ScriptableObjects.")]
    [SerializeField] private List<CustomerMenu> availableMenus;
    
    [Header("Recipe System")]
    [Tooltip("Recipe Database - 如果设置了会覆盖CustomerMenu系统")]
    [SerializeField] private RecipeDatabase recipeDatabase;
    
    [Header("Queue Settings")]
    [Tooltip("Reference to the QueueManager in the scene")]
    [SerializeField] private QueueManager queueManager;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private float nextSpawnTime;
    [SerializeField] private int totalCustomersSpawned = 0;

    // 生成模式枚举
    public enum SpawnMode
    {
        FixedInterval,    // 固定间隔
        RandomInterval,   // 随机间隔
        WaveBasedOnly    // 仅基于波次（等待当前波次清空后再生成）
    }
    
    // 自定义生成计划
    [System.Serializable]
    public class SpawnSchedule
    {
        public string waveName = "Wave";
        public float delayBeforeWave = 0f;
        public int customerCount = 3;
        public float intervalBetweenCustomers = 1f;
        public bool waitForWaveCompletion = true;
    }

    // Internal object pool for customers
    private List<CustomerNPC> customerPool = new List<CustomerNPC>();
    private List<CustomerNPC> activeCustomers = new List<CustomerNPC>();
    private Coroutine spawnCoroutine;
    private int currentWaveIndex = 0;
    private bool isWaveInProgress = false;

    private void Start()
    {
        ValidateReferences();
        InitializeCustomerPool(maxCustomersOnScreen * 2);
        
        // 根据设置选择生成方式
        if (useCustomSchedule && customSpawnSchedule.Count > 0)
        {
            spawnCoroutine = StartCoroutine(CustomScheduleSpawnRoutine());
        }
        else
        {
            spawnCoroutine = StartCoroutine(SpawnCustomersRoutine());
        }
    }

    private void ValidateReferences()
    {
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
        
        // 验证菜单系统
        if (recipeDatabase == null && (availableMenus == null || availableMenus.Count == 0))
        {
            Debug.LogError("Neither RecipeDatabase nor CustomerMenus are assigned! Customers won't have orders.", this);
        }
        
        // 查找Queue Manager
        if (queueManager == null)
        {
            queueManager = FindObjectOfType<QueueManager>();
            if (queueManager == null && showDebugInfo)
            {
                Debug.LogWarning("QueueManager not found in scene! Customers won't be able to queue.", this);
            }
        }
        
        // 查找Seat Manager
        if (SeatManager.Instance == null && showDebugInfo)
        {
            Debug.LogWarning("SeatManager not found in scene! Customers won't be able to find seats.", this);
        }
    }

    private void OnDisable()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
    }

    /// <summary>
    /// 初始化顾客对象池
    /// </summary>
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
            
            customer.Initialize(this, topicBubbleUIPrefab);
            obj.SetActive(false);
            customerPool.Add(customer);
        }
        
        if (showDebugInfo)
            Debug.Log($"Initialized customer pool with {poolSize} customers.");
    }

    /// <summary>
    /// 标准生成协程（固定或随机间隔）
    /// </summary>
    private IEnumerator SpawnCustomersRoutine()
    {
        while (true)
        {
            // 计算下次生成时间
            float spawnDelay = CalculateNextSpawnDelay();
            nextSpawnTime = Time.time + spawnDelay;
            
            yield return new WaitForSeconds(spawnDelay);

            // 检查是否可以生成
            if (CanSpawnCustomer())
            {
                // 生成指定数量的顾客
                for (int i = 0; i < customersPerWave; i++)
                {
                    if (CanSpawnCustomer())
                    {
                        SpawnCustomer();
                        
                        // 如果不是最后一个，稍微延迟
                        if (i < customersPerWave - 1)
                        {
                            yield return new WaitForSeconds(0.5f);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 自定义计划生成协程
    /// </summary>
    private IEnumerator CustomScheduleSpawnRoutine()
    {
        while (currentWaveIndex < customSpawnSchedule.Count)
        {
            SpawnSchedule currentWave = customSpawnSchedule[currentWaveIndex];
            
            if (showDebugInfo)
                Debug.Log($"准备生成波次: {currentWave.waveName}");
            
            // 波次前延迟
            if (currentWave.delayBeforeWave > 0)
            {
                yield return new WaitForSeconds(currentWave.delayBeforeWave);
            }
            
            // 如果需要等待上一波完成
            if (currentWave.waitForWaveCompletion && isWaveInProgress)
            {
                yield return new WaitUntil(() => activeCustomers.Count == 0);
            }
            
            isWaveInProgress = true;
            
            // 生成这一波的顾客
            for (int i = 0; i < currentWave.customerCount; i++)
            {
                if (CanSpawnCustomer())
                {
                    SpawnCustomer();
                    
                    if (i < currentWave.customerCount - 1)
                    {
                        yield return new WaitForSeconds(currentWave.intervalBetweenCustomers);
                    }
                }
                else
                {
                    // 如果达到上限，等待
                    yield return new WaitUntil(() => CanSpawnCustomer());
                    i--; // 重试这个顾客
                }
            }
            
            isWaveInProgress = false;
            currentWaveIndex++;
            
            // 如果所有波次完成，可以选择循环或停止
            if (currentWaveIndex >= customSpawnSchedule.Count)
            {
                if (showDebugInfo)
                    Debug.Log("所有波次已完成");
                // 可以选择重置: currentWaveIndex = 0;
                yield break;
            }
        }
    }

    /// <summary>
    /// 计算下次生成延迟
    /// </summary>
    private float CalculateNextSpawnDelay()
    {
        switch (spawnMode)
        {
            case SpawnMode.FixedInterval:
                return fixedSpawnInterval;
                
            case SpawnMode.RandomInterval:
                return Random.Range(minSpawnInterval, maxSpawnInterval);
                
            case SpawnMode.WaveBasedOnly:
                // 等待所有顾客离开
                return activeCustomers.Count == 0 ? 0.1f : 1f;
                
            default:
                return 5f;
        }
    }

    /// <summary>
    /// 检查是否可以生成新顾客
    /// </summary>
    private bool CanSpawnCustomer()
    {
        return activeCustomers.Count < maxCustomersOnScreen;
    }

    /// <summary>
    /// 生成一个顾客
    /// </summary>
    private void SpawnCustomer()
    {
        CustomerNPC customerToSpawn = GetCustomerFromPool();

        if (customerToSpawn != null)
        {
            Vector3 spawnPosition = new Vector3(spawnXPosition, Random.Range(spawnYMin, spawnYMax), 0f);
            customerToSpawn.transform.position = spawnPosition;

            // 如果使用旧的菜单系统
            if (recipeDatabase == null && availableMenus != null && availableMenus.Count > 0)
            {
                CustomerMenu selectedMenu = availableMenus[Random.Range(0, availableMenus.Count)];
                customerToSpawn.SetMenu(selectedMenu);
            }

            customerToSpawn.gameObject.SetActive(true);
            activeCustomers.Add(customerToSpawn);
            totalCustomersSpawned++;
            
            if (showDebugInfo)
                Debug.Log($"Spawned customer #{totalCustomersSpawned} at {spawnPosition}. Active: {activeCustomers.Count}");
        }
        else
        {
            Debug.LogWarning("No available customer in pool. Consider increasing pool size.");
        }
    }

    /// <summary>
    /// 从池中获取一个顾客
    /// </summary>
    private CustomerNPC GetCustomerFromPool()
    {
        CustomerNPC customer = customerPool.FirstOrDefault(c => !c.gameObject.activeInHierarchy);
        
        // 如果池中没有可用的，扩展池
        if (customer == null && customerPool.Count < maxCustomersOnScreen * 3)
        {
            GameObject obj = Instantiate(customerNPCPrefab, transform);
            customer = obj.GetComponent<CustomerNPC>();
            if (customer != null)
            {
                customer.Initialize(this, topicBubbleUIPrefab);
                obj.SetActive(false);
                customerPool.Add(customer);
                
                if (showDebugInfo)
                    Debug.Log("扩展了顾客池");
            }
        }
        
        return customer;
    }

    /// <summary>
    /// 将顾客返回池中
    /// </summary>
    public void ReturnCustomerToPool(CustomerNPC customer)
    {
        if (customer == null) return;

        if (activeCustomers.Remove(customer))
        {
            customer.gameObject.SetActive(false);
            
            if (showDebugInfo)
                Debug.Log($"Customer returned to pool. Active customers: {activeCustomers.Count}");
        }
    }

    /// <summary>
    /// 获取队列起始位置
    /// </summary>
    public Vector3 GetQueueStartPosition()
    {
        if (queueManager != null)
            return queueManager.GetQueueStartPosition();
        
        return transform.position + Vector3.right * 5f;
    }

    /// <summary>
    /// 手动生成顾客（用于测试）
    /// </summary>
    [ContextMenu("Spawn Customer Now")]
    public void SpawnCustomerManually()
    {
        if (CanSpawnCustomer())
        {
            SpawnCustomer();
        }
        else
        {
            Debug.LogWarning("Cannot spawn: Maximum customers reached!");
        }
    }

    /// <summary>
    /// 清空所有活跃顾客（用于测试）
    /// </summary>
    [ContextMenu("Clear All Customers")]
    public void ClearAllCustomers()
    {
        List<CustomerNPC> customersToReturn = new List<CustomerNPC>(activeCustomers);
        foreach (var customer in customersToReturn)
        {
            ReturnCustomerToPool(customer);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 绘制生成区域
        Gizmos.color = Color.green;
        Vector3 topLeft = new Vector3(spawnXPosition - 0.5f, spawnYMax, 0);
        Vector3 topRight = new Vector3(spawnXPosition + 0.5f, spawnYMax, 0);
        Vector3 bottomLeft = new Vector3(spawnXPosition - 0.5f, spawnYMin, 0);
        Vector3 bottomRight = new Vector3(spawnXPosition + 0.5f, spawnYMin, 0);
        
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
        
        // 绘制生成点示例
        for (int i = 0; i < 3; i++)
        {
            float y = Mathf.Lerp(spawnYMin, spawnYMax, i / 2f);
            Gizmos.DrawWireSphere(new Vector3(spawnXPosition, y, 0), 0.3f);
        }
    }
}
