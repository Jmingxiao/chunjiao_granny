using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class FoodItem
{
    public enum FoodType { Bread, Meat, Vegetable, Sauce, Topping }
    
    public string itemName;
    public FoodType type;
    public int quality;
    public float preparationTime;
    public Sprite icon;
    
    public float costToBuy;
    public float baseSellPrice;
    
    // Cooking properties
    public bool requiresCooking;
    public float cookingTime;
    public bool isCooked;
    
    // Food appearance
    public Color foodColor = Color.white;
    public Vector3 foodScale = Vector3.one;
    
    public FoodItem(string name, FoodType type, int quality = 1)
    {
        this.itemName = name;
        this.type = type;
        this.quality = quality;
        this.preparationTime = 1f;
        this.requiresCooking = type == FoodType.Meat;
        this.cookingTime = 5f;
        this.isCooked = false;
        
        // Default pricing based on quality and type
        this.costToBuy = quality * 2f;
        this.baseSellPrice = quality * 3f;
    }
    
    public bool Cook(float cookAmount)
    {
        if (!requiresCooking)
            return true;
            
        cookingTime -= cookAmount;
        
        if (cookingTime <= 0)
        {
            isCooked = true;
            return true;
        }
        
        return false;
    }
    
    public float GetSellPrice()
    {
        return baseSellPrice * quality;
    }
    
    public override string ToString()
    {
        return $"{itemName} (Quality: {quality})";
    }
    
    // Static method to get common food items
    public static List<FoodItem> GetCommonFoodItems()
    {
        List<FoodItem> items = new List<FoodItem>
        {
            // Breads
            new FoodItem("Pita Bread", FoodType.Bread),
            new FoodItem("Flatbread", FoodType.Bread),
            
            // Meats
            new FoodItem("Chicken", FoodType.Meat),
            new FoodItem("Beef", FoodType.Meat),
            new FoodItem("Lamb", FoodType.Meat),
            
            // Vegetables
            new FoodItem("Lettuce", FoodType.Vegetable),
            new FoodItem("Tomato", FoodType.Vegetable),
            new FoodItem("Onion", FoodType.Vegetable),
            new FoodItem("Cucumber", FoodType.Vegetable),
            
            // Sauces
            new FoodItem("Tahini", FoodType.Sauce),
            new FoodItem("Garlic Sauce", FoodType.Sauce),
            new FoodItem("Hot Sauce", FoodType.Sauce),
            
            // Toppings
            new FoodItem("Pickles", FoodType.Topping),
            new FoodItem("Fries", FoodType.Topping)
        };
        
        return items;
    }
} 