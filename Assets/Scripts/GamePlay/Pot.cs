using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Pot : MonoBehaviour
{
    [SerializeField] private Cutboard cutboard; // 引用切菜板
    [SerializeField] private PotType potType;
    
    [Header("存储的食材")]
    [SerializeField] private List<IngredientData> ingredientsInPot = new List<IngredientData>();
    
    private void Start()
    {
        // 如果没有手动指定切菜板，尝试自动查找
        if (cutboard == null)
        {
            cutboard = FindObjectOfType<Cutboard>();
        }
        
        // 订阅切割完成事件
        if (cutboard != null)
        {
            cutboard.OnCuttingCompleted.AddListener(HandleCuttingCompleted);
        }
        else
        {
            Debug.LogError("未找到Cutboard实例");
        }
    }
    
    private void OnDestroy()
    {
        // 取消订阅，防止内存泄漏
        if (cutboard != null)
        {
            cutboard.OnCuttingCompleted.RemoveListener(HandleCuttingCompleted);
        }
    }
    
    /// <summary>
    /// 处理切割完成事件
    /// </summary>
    /// <param name="ingredientData">切割完成的食材数据</param>
    private void HandleCuttingCompleted(IngredientData ingredientData)
    {
        // 检查这个锅是否适合处理这种食材
        if (ingredientData.potType == potType)
        {
            Debug.Log($"锅接收到切好的食材: {ingredientData.ingredientName}");
            
            // 使用Cutboard提供的方法移除切好的食材
            GameObject cutIngredient = cutboard.RemoveCutIngredient();
            
            if (cutIngredient != null)
            {
                // 将食材移到锅中
                cutIngredient.transform.position = transform.position + Vector3.up;
                cutIngredient.transform.parent = transform;
                
                // 将食材数据添加到列表
                ingredientsInPot.Add(ingredientData);
                
                Debug.Log($"食材 {ingredientData.ingredientName} 已加入锅中");
                Debug.Log($"当前锅中有 {ingredientsInPot.Count} 个食材");
                
                // 在这里添加更多处理逻辑：
                // - 开始烹饪过程
                // - 更新UI显示等
            }
        }
        else
        {
            Debug.Log($"这个锅不适合处理 {ingredientData.ingredientName}");
        }
    }
}