using UnityEngine;
using System.Collections.Generic;

public class Plate : MonoBehaviour
{
    [Header("盘子设置")]
    public int maxItems = 5; // 最多可以放置的食材数量
    public float itemSpacing = 0.3f; // 食材之间的间距
    public Vector3 firstItemOffset = new Vector3(0, 0.3f, 0); // 第一个食材的偏移
    
    private List<GameObject> itemsOnPlate = new List<GameObject>();
    
    void Start()
    {
        // 确保有必要的组件
        if (GetComponent<Collider2D>() != null)
        {
            GetComponent<Collider2D>().isTrigger = true;
        }
        
        Debug.Log("盘子已准备就绪");
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // 检查是否已满
        if (itemsOnPlate.Count >= maxItems)
        {
            Debug.Log("盘子已满！");
            return;
        }
        
        // 检查是否是食材
        Ingredient ingredient = other.GetComponent<Ingredient>();
        if (ingredient == null)
        {
            return;
        }
        
        // 检查是否已经在盘子上
        if (itemsOnPlate.Contains(other.gameObject))
        {
            return;
        }
        
        // 将食材添加到盘子上
        AddItemToPlate(other.gameObject);
    }
    
    void AddItemToPlate(GameObject item)
    {
        Debug.Log($"将 {item.name} 添加到盘子上");
        
        // 添加到列表
        itemsOnPlate.Add(item);
        
        // 设置物理属性
        Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.velocity = Vector2.zero;
        }
        
        // 设置位置
        ArrangeItemsOnPlate();
        
        // 可以添加音效或视觉效果
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        // 如果食材被拖走，从列表中移除
        if (itemsOnPlate.Contains(other.gameObject))
        {
            RemoveItemFromPlate(other.gameObject);
        }
    }
    
    void RemoveItemFromPlate(GameObject item)
    {
        Debug.Log($"从盘子上移除 {item.name}");
        
        itemsOnPlate.Remove(item);
        
        // 恢复物理属性
        Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
        
        // 重新排列剩余的食材
        ArrangeItemsOnPlate();
    }
    
    void ArrangeItemsOnPlate()
    {
        // 重新排列盘子上的所有食材
        for (int i = 0; i < itemsOnPlate.Count; i++)
        {
            if (itemsOnPlate[i] != null)
            {
                Vector3 position = transform.position + firstItemOffset;
                
                // 以圆形方式排列
                if (i > 0)
                {
                    float angle = (360f / Mathf.Max(itemsOnPlate.Count, 3)) * i;
                    float radius = itemSpacing;
                    position.x += Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
                    position.y += Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
                }
                
                itemsOnPlate[i].transform.position = position;
            }
        }
    }
    
    public List<GameObject> GetItemsOnPlate()
    {
        return new List<GameObject>(itemsOnPlate);
    }
    
    public bool IsEmpty()
    {
        return itemsOnPlate.Count == 0;
    }
    
    public bool IsFull()
    {
        return itemsOnPlate.Count >= maxItems;
    }
}