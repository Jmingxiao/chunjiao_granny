using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class Dish : MonoBehaviour, IPointerClickHandler
{
    [Header("Dish信息")]
    [SerializeField] private string dishName = "Default Dish";
    [SerializeField] private CustomerMenu dishMenuData; // 旧系统兼容
    [SerializeField] private Recipe associatedRecipe; // 新的Recipe系统
    [SerializeField] private float completeness = 1.0f; // 菜品完成度 (0-1)
    
    [Header("传递设置")]
    [SerializeField] private float deliveryRange = 5f; // 传递距离
    [SerializeField] private LayerMask customerLayer; // 顾客所在的层
    
    private ArrangableItem arrangableItem;
    private bool isActivated = false;
    
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
            StartCoroutine(CheckArrangementStatus());
        }
    }
    
    /// <summary>
    /// 设置关联的Recipe
    /// </summary>
    public void SetRecipe(Recipe recipe)
    {
        associatedRecipe = recipe;
        dishName = recipe.recipeName;
    }
    
    IEnumerator CheckArrangementStatus()
    {
        while (true)
        {
            if (arrangableItem.IsArranged && !isActivated)
            {
                isActivated = true;
                Debug.Log($"{dishName} 已加入列表，现在可以传递给customer");
            }
            else if (!arrangableItem.IsArranged && isActivated)
            {
                isActivated = false;
            }
            
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isActivated)
        {
            // 查找附近等待这道菜的顾客
            CustomerNPC targetCustomer = FindCustomerWaitingForDish();
            
            if (targetCustomer != null)
            {
                DeliverToCustomer(targetCustomer);
            }
            else
            {
                // 显示提示：没有顾客在等待这道菜
                ShowNoCustomerMessage();
            }
        }
    }
    
    /// <summary>
    /// 查找等待这道菜的顾客
    /// </summary>
    private CustomerNPC FindCustomerWaitingForDish()
    {
        // 方法1：查找所有在范围内的顾客
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, deliveryRange, customerLayer);
        
        foreach (Collider2D collider in nearbyColliders)
        {
            CustomerNPC customer = collider.GetComponent<CustomerNPC>();
            if (customer != null)
            {
                // 优先检查Recipe系统
                if (associatedRecipe != null && customer.IsWaitingForRecipe(associatedRecipe))
                {
                    return customer;
                }
                // 向后兼容旧系统
                else if (dishMenuData != null && customer.IsWaitingForDish(dishMenuData))
                {
                    return customer;
                }
                // 通过名称匹配
                else if (customer.IsWaitingForDishName(dishName))
                {
                    return customer;
                }
            }
        }
        
        // 方法2：如果范围内没有，查找场景中所有等待这道菜的顾客
        CustomerNPC[] allCustomers = FindObjectsOfType<CustomerNPC>();
        
        // 按距离排序
        var sortedCustomers = allCustomers
            .Where(c => {
                if (associatedRecipe != null)
                    return c.IsWaitingForRecipe(associatedRecipe);
                else if (dishMenuData != null)
                    return c.IsWaitingForDish(dishMenuData);
                else
                    return c.IsWaitingForDishName(dishName);
            })
            .OrderBy(c => Vector2.Distance(transform.position, c.transform.position))
            .ToList();
        
        if (sortedCustomers.Count > 0)
        {
            // 返回最近的等待该菜品的顾客
            return sortedCustomers[0];
        }
        
        return null;
    }
    
    /// <summary>
    /// 传递给指定顾客
    /// </summary>
    private void DeliverToCustomer(CustomerNPC customer)
    {
        Debug.Log($"{dishName} 正在传递给顾客");
        
        // 计算菜品完成度（可以基于制作过程）
        float finalCompleteness = CalculateDishCompleteness();
        
        // 传递给顾客
        if (associatedRecipe != null)
        {
            customer.ReceiveRecipe(associatedRecipe, finalCompleteness);
        }
        else
        {
            customer.ReceiveDish(finalCompleteness);
        }
        
        // 从排列中移除
        if (arrangableItem != null)
        {
            arrangableItem.ResetArrangement();
        }
        
        // 停用状态
        isActivated = false;
        
        // 播放传递动画（可选）
        StartCoroutine(DeliveryAnimation(customer.transform.position));
    }
    
    /// <summary>
    /// 计算菜品完成度
    /// </summary>
    private float CalculateDishCompleteness()
    {
        // 这里可以根据实际的烹饪系统来计算
        // 例如：基于烹饪时间、配料正确性等
        return completeness;
    }
    
    /// <summary>
    /// 传递动画
    /// </summary>
    private IEnumerator DeliveryAnimation(Vector3 targetPosition)
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // 抛物线运动
            transform.position = Vector3.Lerp(startPos, targetPosition, t);
            transform.position += Vector3.up * Mathf.Sin(t * Mathf.PI) * 2f;
            
            yield return null;
        }
        
        // 动画结束后销毁
        Destroy(gameObject);
    }
    
    /// <summary>
    /// 显示没有顾客的提示
    /// </summary>
    private void ShowNoCustomerMessage()
    {
        Debug.Log($"没有顾客在等待 {dishName}");
        // 这里可以显示UI提示
    }
    
    /// <summary>
    /// 在编辑器中显示传递范围
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, deliveryRange);
    }
}