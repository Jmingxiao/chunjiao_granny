using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeatManager : MonoBehaviour
{
    private static SeatManager instance;
    public static SeatManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<SeatManager>();
            return instance;
        }
    }

    [Header("Seats")]
    [SerializeField] private Transform[] seatPositions;
    
    // 简单的字典记录哪个座位被谁占用
    private Dictionary<Transform, CustomerNPC> seatOccupancy = new Dictionary<Transform, CustomerNPC>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
            
        // 初始化座位字典
        foreach (Transform seat in seatPositions)
        {
            if (seat != null)
                seatOccupancy[seat] = null;
        }
    }

    /// <summary>
    /// 获取一个空座位
    /// </summary>
    public Transform GetAvailableSeat()
    {
        foreach (var seat in seatOccupancy)
        {
            if (seat.Value == null) // 空座位
                return seat.Key;
        }
        return null;
    }

    /// <summary>
    /// 占用座位
    /// </summary>
    public void OccupySeat(Transform seat, CustomerNPC customer)
    {
        if (seatOccupancy.ContainsKey(seat))
            seatOccupancy[seat] = customer;
    }

    /// <summary>
    /// 释放座位
    /// </summary>
    public void FreeSeat(Transform seat)
    {
        if (seatOccupancy.ContainsKey(seat))
            seatOccupancy[seat] = null;
    }

    /// <summary>
    /// 检查是否有空座位
    /// </summary>
    public bool HasAvailableSeat()
    {
        foreach (var seat in seatOccupancy)
        {
            if (seat.Value == null)
                return true;
        }
        return false;
    }

    // 可选：在Scene视图显示座位状态
    private void OnDrawGizmos()
    {
        if (seatPositions == null) return;
        
        foreach (Transform seat in seatPositions)
        {
            if (seat != null)
            {
                bool isOccupied = Application.isPlaying && seatOccupancy.ContainsKey(seat) && seatOccupancy[seat] != null;
                Gizmos.color = isOccupied ? Color.red : Color.green;
                Gizmos.DrawWireSphere(seat.position, 0.3f);
            }
        }
    }
}
