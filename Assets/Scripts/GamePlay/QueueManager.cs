using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueueManager : MonoBehaviour
{
    // Start is called before the first frame update
    private static QueueManager instance;
    public static QueueManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<QueueManager>();
            return instance;
        }
    }

    [Header("Queue Settings")]
    [Tooltip("Starting position for the queue line")]
    [SerializeField] private Transform queueStartPosition;
    
    [Tooltip("Space between customers in queue")]
    [SerializeField] private float queueSpacing = 1.5f;
    
    [Tooltip("Direction of queue (1 for right, -1 for left)")]
    [SerializeField] private float queueDirection = 1f;
    
    [Header("Visual Settings")]
    [SerializeField] private bool showQueueLine = true;
    [SerializeField] private Color queueLineColor = Color.yellow;
    [SerializeField] private int maxQueueVisualLength = 10;

    private List<CustomerNPC> customerQueue = new List<CustomerNPC>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        
        // 如果没有设置队列起始位置，使用自身位置
        if (queueStartPosition == null)
        {
            queueStartPosition = transform;
        }
    }

    /// <summary>
    /// 添加顾客到队列
    /// </summary>
    public void AddToQueue(CustomerNPC customer)
    {
        if (customer == null || customerQueue.Contains(customer)) return;
        
        customerQueue.Add(customer);
        UpdateAllQueuePositions();
        
        Debug.Log($"Customer added to queue. Queue length: {customerQueue.Count}");
    }

    /// <summary>
    /// 从队列中移除顾客
    /// </summary>
    public void RemoveFromQueue(CustomerNPC customer)
    {
        if (customer == null) return;
        
        if (customerQueue.Remove(customer))
        {
            UpdateAllQueuePositions();
            Debug.Log($"Customer removed from queue. Queue length: {customerQueue.Count}");
        }
    }

    /// <summary>
    /// 更新所有排队顾客的位置
    /// </summary>
    private void UpdateAllQueuePositions()
    {
        for (int i = 0; i < customerQueue.Count; i++)
        {
            if (customerQueue[i] != null)
            {
                Vector3 queuePos = GetQueuePosition(i);
                customerQueue[i].UpdateQueuePosition(queuePos, i);
            }
        }
    }

    /// <summary>
    /// 计算队列中指定位置的世界坐标
    /// </summary>
    private Vector3 GetQueuePosition(int index)
    {
        Vector3 basePosition = queueStartPosition.position;
        Vector3 offset = new Vector3(queueDirection * queueSpacing * index, 0, 0);
        return basePosition + offset;
    }

    /// <summary>
    /// 获取队列长度
    /// </summary>
    public int GetQueueLength()
    {
        return customerQueue.Count;
    }

    /// <summary>
    /// 检查顾客是否在队首
    /// </summary>
    public bool IsFirstInQueue(CustomerNPC customer)
    {
        return customerQueue.Count > 0 && customerQueue[0] == customer;
    }

    /// <summary>
    /// 获取队首顾客
    /// </summary>
    public CustomerNPC GetFirstInQueue()
    {
        return customerQueue.Count > 0 ? customerQueue[0] : null;
    }

    /// <summary>
    /// 清空队列（游戏结束等情况使用）
    /// </summary>
    public void ClearQueue()
    {
        customerQueue.Clear();
        Debug.Log("Queue cleared");
    }

    /// <summary>
    /// 在Scene视图中绘制队列指示线
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showQueueLine || queueStartPosition == null) return;

        Gizmos.color = queueLineColor;
        
        // 绘制队列起始点
        Gizmos.DrawWireSphere(queueStartPosition.position, 0.3f);
        
        // 绘制队列线
        Vector3 start = queueStartPosition.position;
        Vector3 end = start + new Vector3(queueDirection * queueSpacing * maxQueueVisualLength, 0, 0);
        Gizmos.DrawLine(start, end);
        
        // 绘制队列位置标记
        for (int i = 0; i < maxQueueVisualLength; i++)
        {
            Vector3 pos = GetQueuePosition(i);
            Gizmos.DrawWireCube(pos, Vector3.one * 0.3f);
        }
        
        // 显示当前队列信息
        #if UNITY_EDITOR
        if (Application.isPlaying)
        {
            UnityEditor.Handles.Label(queueStartPosition.position + Vector3.up * 1.5f, 
                $"Queue Length: {customerQueue.Count}");
        }
        #endif
    }

    /// <summary>
    /// 获取队列起始位置（供CustomerSpawner使用）
    /// </summary>
    public Vector3 GetQueueStartPosition()
    {
        return queueStartPosition != null ? queueStartPosition.position : transform.position;
    }
}
