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
}
