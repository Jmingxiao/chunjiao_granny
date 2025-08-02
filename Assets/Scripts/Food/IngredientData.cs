using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PotType{
        Steamer,
        Pot,
    }

[CreateAssetMenu(fileName = "New Ingredient", menuName = "Shawarma/Ingredient Data")]
public class IngredientData : ScriptableObject
{
    [Header("基本信息")]
    public string ingredientName;
    public Sprite icon;
    
    [Header("预制体")]
    public GameObject rawPrefab;      // 原始食材预制体
    public GameObject processedPrefab; // 加工后的食材预制体（如果有的话）

    public PotType potType;

     /// <summary>
    /// 是否需要切菜板处理
    /// </summary>
    public bool RequiresCutting()
    {
        // 如果有原始预制体和加工预制体，说明需要切
        return rawPrefab != null && processedPrefab != null;
    }
    
    /// <summary>
    /// 是否可以直接放入锅
    /// </summary>
    public bool CanDirectlyToPot()
    {
        // 如果没有原始预制体但有加工预制体，说明可以直接用
        return rawPrefab == null && processedPrefab != null;
    }
    
    /// <summary>
    /// 获取可使用的预制体
    /// </summary>
    public GameObject GetUsablePrefab()
    {
        // 优先返回加工预制体，如果没有则返回原始预制体
        return processedPrefab != null ? processedPrefab : rawPrefab;
    }
}
