using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "RecipeDatabase", menuName = "Shawarma/Recipe Database")]
public class RecipeDatabase : ScriptableObject
{
     [Header("所有可用的食材")]
    public List<IngredientData> allIngredients = new List<IngredientData>();
    
    [Header("所有菜谱")]
    public List<RecipeConfig> recipes = new List<RecipeConfig>();
    
    [System.Serializable]
    public class RecipeConfig
    {
        public string recipeName;
        public string description;
        public float basePrice = 5f;
        public Sprite icon;
        
        [Header("所需食材")]
        public List<IngredientData> requiredIngredients = new List<IngredientData>();
        
        [Header("成品")]
        public GameObject dishPrefab; // 菜品预制体
        
        [Header("属性")]
        public int difficulty = 1;
        [Range(0f, 1f)]
        public float popularity = 0.5f;
        
        public Recipe ToRecipe()
        {
            Recipe recipe = new Recipe(recipeName, description, basePrice);
            recipe.icon = icon;
            recipe.difficulty = difficulty;
            recipe.popularity = popularity;
            recipe.dishPrefab = dishPrefab; // 设置菜品预制体
            
            foreach (var ingredient in requiredIngredients)
            {
                recipe.AddIngredient(ingredient);
            }
            
            return recipe;
        }
    }
    
    /// <summary>
    /// 获取所有配置的菜谱
    /// </summary>
    public List<Recipe> GetAllRecipes()
    {
        List<Recipe> recipeList = new List<Recipe>();
        
        foreach (var config in recipes)
        {
            recipeList.Add(config.ToRecipe());
        }
        
        return recipeList;
    }
}
