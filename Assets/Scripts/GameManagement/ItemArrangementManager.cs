using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;

public class ItemArrangementManager : MonoBehaviour
{
   [Header("排列设置")]
    [SerializeField] private float itemSpacingX = 1.5f; // 水平间距
    [SerializeField] private float itemSpacingY = 1.5f; // 垂直间距
    [SerializeField] private Vector2 startPosition = new Vector2(-5f, 3f); // 左上角起始位置
    [SerializeField] private int maxItemsPerRow = 5; // 每行最大物品数
    
    [Header("排列方向")]
    [SerializeField] private bool leftToRight = true; // 从左到右排列
    [SerializeField] private bool topToBottom = true; // 从上到下换行
    
    private List<ArrangableItem> arrangedItems = new List<ArrangableItem>();
    private static ItemArrangementManager instance;
    
    public static ItemArrangementManager Instance => instance;
    
    void Awake()
    {
        instance = this;
        
        // 确保有EventSystem
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }
        
        // 确保相机有Physics2DRaycaster
        if (Camera.main != null && Camera.main.GetComponent<Physics2DRaycaster>() == null)
        {
            Camera.main.gameObject.AddComponent<Physics2DRaycaster>();
        }
    }
    
    public void AddItemToArrangement(ArrangableItem item)
    {
        if (!arrangedItems.Contains(item))
        {
            arrangedItems.Add(item);
            RearrangeItems();
        }
    }
    
    public void RemoveItemFromArrangement(ArrangableItem item)
    {
        if (arrangedItems.Remove(item))
        {
            RearrangeItems();
        }
    }
    
    private void RearrangeItems()
    {
        if (arrangedItems.Count == 0) return;
        
        for (int i = 0; i < arrangedItems.Count; i++)
        {
            if (arrangedItems[i] != null)
            {
                Vector2 position = CalculateItemPosition(i);
                arrangedItems[i].SetTargetPosition(position);
            }
        }
        
        // 清理空引用
        arrangedItems.RemoveAll(item => item == null);
    }
    
    private Vector2 CalculateItemPosition(int index)
    {
        // 计算行列位置
        int row = index / maxItemsPerRow;
        int col = index % maxItemsPerRow;
        
        // 计算X坐标
        float x = startPosition.x;
        if (leftToRight)
        {
            x += col * itemSpacingX;
        }
        else
        {
            x -= col * itemSpacingX;
        }
        
        // 计算Y坐标
        float y = startPosition.y;
        if (topToBottom)
        {
            y -= row * itemSpacingY;
        }
        else
        {
            y += row * itemSpacingY;
        }
        
        return new Vector2(x, y);
    }
    
    // 获取下一个空位的位置（用于预览或其他用途）
    public Vector2 GetNextEmptyPosition()
    {
        return CalculateItemPosition(arrangedItems.Count);
    }
    
    // 批量排列所有未排列的物品
    public void ArrangeAllUnassignedItems()
    {
        ArrangableItem[] allItems = FindObjectsOfType<ArrangableItem>();
        foreach (var item in allItems)
        {
            if (!item.IsArranged)
            {
                item.ArrangeItem();
            }
        }
    }
    
    // 重置所有物品的排列状态
    public void ResetAllItems()
    {
        List<ArrangableItem> itemsToReset = new List<ArrangableItem>(arrangedItems);
        foreach (var item in itemsToReset)
        {
            if (item != null)
            {
                item.ResetArrangement();
            }
        }
        arrangedItems.Clear();
    }
}
