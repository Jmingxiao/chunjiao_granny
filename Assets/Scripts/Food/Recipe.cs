using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Recipe
{
     public string recipeName;
    public string description;
    public float basePrice;
    //public float preparationTime;
    public Sprite icon;
    
    public List<IngredientData> requiredIngredients = new List<IngredientData>();
    
    [Header("成品")]
    public GameObject dishPrefab; // 菜品的预制体
    
    // Difficulty of preparation
    public int difficulty = 1;
    
    // Recipe popularity that affects how often customers order it
    public float popularity = 0.5f;
    
    public Recipe(string name, string description, float basePrice)
    {
        this.recipeName = name;
        this.description = description;
        this.basePrice = basePrice;
      ///  this.preparationTime = 10f; // Base preparation time in seconds
    }
    
    public void AddIngredient(IngredientData ingredient)
    {
        requiredIngredients.Add(ingredient);
        
        // 由于IngredientData没有preparationTime，我们可以根据食材数量增加时间
      //  preparationTime += 2f; // 每个食材增加2秒
    }
    
    public float CalculatePrice()
    {
        float totalPrice = basePrice;
        
        // 由于IngredientData没有价格，我们可以根据食材数量计算
        foreach (IngredientData ingredient in requiredIngredients)
        {
            // 每个食材增加1.5的价格
            totalPrice += 1.5f;
        }
        
        return totalPrice;
    }
    /*
    public float CalculatePreparationTime(float speedMultiplier = 1f)
    {
        return preparationTime / speedMultiplier;
    }
    */
    public bool CanPrepare(List<IngredientData> availableIngredients)
    {
        foreach (IngredientData required in requiredIngredients)
        {
            bool found = false;
            
            foreach (IngredientData available in availableIngredients)
            {
                // 直接比较IngredientData对象
                if (available == required)
                {
                    found = true;
                    break;
                }
            }
            
            if (!found)
                return false;
        }
        
        return true;
    }
} 