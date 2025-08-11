using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

public class Dish : MonoBehaviour, IPointerClickHandler
{
     [Header("Dish信息")]
    [SerializeField] private string dishName = "Default Dish";
    
    private ArrangableItem arrangableItem;
    private bool isActivated = false; // 只有被排列后才激活
    
    void Awake()
    {
        // 确保有Collider2D
        if (GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
        }
        
        // 获取ArrangableItem引用
        arrangableItem = GetComponent<ArrangableItem>();
    }
    
    void Start()
    {
        // 订阅排列事件
        if (arrangableItem != null)
        {
            // 监听排列状态变化
            StartCoroutine(CheckArrangementStatus());
        }
    }
    
    System.Collections.IEnumerator CheckArrangementStatus()
    {
        while (true)
        {
            // 检查是否被排列
            if (arrangableItem.IsArranged && !isActivated)
            {
                // 激活Dish功能
                isActivated = true;
                Debug.Log($"{dishName} 已加入列表，现在可以传递给customer");
            }
            else if (!arrangableItem.IsArranged && isActivated)
            {
                // 如果从列表中移除，停用Dish功能
                isActivated = false;
            }
            
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // 只有激活状态（在列表中）才响应点击
        if (isActivated)
        {
            DeliverToCustomer();
        }
        // 如果未激活，点击事件会被ArrangableItem处理（排列到列表）
    }
    
    private void DeliverToCustomer()
    {
        Debug.Log($"{dishName} 会传给customer");
        
        // 从排列中移除
        if (arrangableItem != null)
        {
            arrangableItem.ResetArrangement();
        }
        
        // 停用状态
        isActivated = false;
        
        // 隐藏dish（模拟传递给customer）
        gameObject.SetActive(false);
        
        // 或者如果你想要销毁
        // Destroy(gameObject, 0.1f);
    }
}
