using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public static class FlexibleRecipeMatcher
{
    /// <summary>
    /// 检查锅中的食材是否满足菜谱要求
    /// </summary>
    public static bool CanMakeRecipe(Recipe recipe, List<IngredientData> potIngredients)
    {
        // 现在可以直接比较IngredientData对象
        foreach (var required in recipe.requiredIngredients)
        {
            bool found = false;
            
            foreach (var available in potIngredients)
            {
                // 直接比较引用
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