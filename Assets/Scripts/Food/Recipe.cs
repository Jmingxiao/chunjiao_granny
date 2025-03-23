using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Recipe
{
    public string recipeName;
    public string description;
    public float basePrice;
    public float preparationTime;
    public Sprite icon;
    
    public List<FoodItem> requiredIngredients = new List<FoodItem>();
    
    // Difficulty of preparation
    public int difficulty = 1;
    
    // Recipe popularity that affects how often customers order it
    public float popularity = 0.5f;
    
    public Recipe(string name, string description, float basePrice)
    {
        this.recipeName = name;
        this.description = description;
        this.basePrice = basePrice;
        this.preparationTime = 10f; // Base preparation time in seconds
    }
    
    public void AddIngredient(FoodItem ingredient)
    {
        requiredIngredients.Add(ingredient);
        
        // Update preparation time based on ingredients
        preparationTime += ingredient.preparationTime;
    }
    
    public float CalculatePrice()
    {
        float totalPrice = basePrice;
        
        foreach (FoodItem ingredient in requiredIngredients)
        {
            totalPrice += ingredient.GetSellPrice() * 0.7f; // 70% of ingredient price
        }
        
        return totalPrice;
    }
    
    public float CalculatePreparationTime(float speedMultiplier = 1f)
    {
        return preparationTime / speedMultiplier;
    }
    
    public bool CanPrepare(List<FoodItem> availableIngredients)
    {
        // Simplified check - in a full game this would be more sophisticated
        foreach (FoodItem required in requiredIngredients)
        {
            bool found = false;
            
            foreach (FoodItem available in availableIngredients)
            {
                if (available.itemName == required.itemName && available.type == required.type)
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
    
    // Static method to get common recipes
    public static List<Recipe> GetCommonRecipes()
    {
        List<FoodItem> commonIngredients = FoodItem.GetCommonFoodItems();
        List<Recipe> recipes = new List<Recipe>();
        
        // Chicken Shawarma
        Recipe chickenShawarma = new Recipe(
            "Chicken Shawarma",
            "Classic shawarma with chicken, vegetables and sauce in pita bread",
            5f
        );
        
        // Find and add ingredients
        foreach (FoodItem item in commonIngredients)
        {
            if (item.itemName == "Pita Bread" || 
                item.itemName == "Chicken" || 
                item.itemName == "Lettuce" || 
                item.itemName == "Tomato" ||
                item.itemName == "Garlic Sauce")
            {
                chickenShawarma.AddIngredient(item);
            }
        }
        
        recipes.Add(chickenShawarma);
        
        // Beef Shawarma
        Recipe beefShawarma = new Recipe(
            "Beef Shawarma",
            "Hearty beef shawarma with fresh vegetables and tahini sauce",
            6f
        );
        
        foreach (FoodItem item in commonIngredients)
        {
            if (item.itemName == "Flatbread" || 
                item.itemName == "Beef" || 
                item.itemName == "Onion" || 
                item.itemName == "Cucumber" ||
                item.itemName == "Tahini")
            {
                beefShawarma.AddIngredient(item);
            }
        }
        
        recipes.Add(beefShawarma);
        
        return recipes;
    }
} 