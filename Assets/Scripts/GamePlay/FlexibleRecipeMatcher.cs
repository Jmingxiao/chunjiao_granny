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
        // 获取菜谱需要的所有食材名称
        List<string> requiredNames = recipe.requiredIngredients.Select(f => f.itemName).ToList();
        
        // 获取锅中的所有食材名称
        List<string> availableNames = potIngredients.Select(i => i.ingredientName).ToList();
        
        // 创建副本用于匹配
        List<string> tempRequired = new List<string>(requiredNames);
        List<string> tempAvailable = new List<string>(availableNames);
        
        // 尝试匹配每个需要的食材
        for (int i = tempRequired.Count - 1; i >= 0; i--)
        {
            string required = tempRequired[i];
            
            // 在可用食材中查找匹配
            for (int j = tempAvailable.Count - 1; j >= 0; j--)
            {
                if (IsIngredientMatch(required, tempAvailable[j]))
                {
                    // 找到匹配，从两个列表中移除
                    tempRequired.RemoveAt(i);
                    tempAvailable.RemoveAt(j);
                    break;
                }
            }
        }
        
        // 如果所有需要的食材都找到了匹配，返回true
        return tempRequired.Count == 0;
    }
    
    /// <summary>
    /// 灵活的食材名称匹配
    /// </summary>
    private static bool IsIngredientMatch(string recipeName, string ingredientName)
    {
        // 转换为小写进行比较
        string name1 = NormalizeName(recipeName);
        string name2 = NormalizeName(ingredientName);
        
        // 1. 完全匹配
        if (name1 == name2)
            return true;
            
        // 2. 包含匹配（一个包含另一个）
        if (name1.Contains(name2) || name2.Contains(name1))
            return true;
            
        // 3. 关键词匹配
        if (HasCommonKeyword(name1, name2))
            return true;
            
        // 4. 同义词匹配
        if (AreSynonyms(name1, name2))
            return true;
            
        // 5. 相似度匹配（编辑距离）
        if (CalculateSimilarity(name1, name2) > 0.7f)
            return true;
            
        return false;
    }
    
    /// <summary>
    /// 标准化名称（移除特殊字符，统一格式）
    /// </summary>
    private static string NormalizeName(string name)
    {
        // 转小写
        name = name.ToLower();
        
        // 移除多余空格
        name = Regex.Replace(name, @"\s+", " ").Trim();
        
        // 移除常见的修饰词
        string[] removeWords = { "fresh", "raw", "cooked", "sliced", "diced", "chopped" };
        foreach (var word in removeWords)
        {
            name = name.Replace(word + " ", "");
        }
        
        return name;
    }
    
    /// <summary>
    /// 检查是否有共同关键词
    /// </summary>
    private static bool HasCommonKeyword(string name1, string name2)
    {
        // 提取关键词
        string[] keywords1 = ExtractKeywords(name1);
        string[] keywords2 = ExtractKeywords(name2);
        
        // 查找共同关键词
        foreach (var k1 in keywords1)
        {
            foreach (var k2 in keywords2)
            {
                if (k1 == k2 && k1.Length > 3) // 关键词长度大于3
                    return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 提取关键词
    /// </summary>
    private static string[] ExtractKeywords(string name)
    {
        // 分割单词
        string[] words = name.Split(' ');
        
        // 过滤掉短词和常见词
        string[] commonWords = { "the", "and", "or", "with", "sauce" };
        
        return words.Where(w => w.Length > 2 && !commonWords.Contains(w)).ToArray();
    }
    
    /// <summary>
    /// 同义词词典
    /// </summary>
    private static Dictionary<string, List<string>> synonymDictionary = new Dictionary<string, List<string>>
    {
        { "chicken", new List<string> { "poultry", "鸡肉", "チキン" } },
        { "beef", new List<string> { "steak", "牛肉", "ビーフ" } },
        { "pita", new List<string> { "pita bread", "pocket bread", "arabic bread" } },
        { "flatbread", new List<string> { "naan", "roti", "lavash", "bread" } },
        { "tomato", new List<string> { "tomatoes", "番茄", "トマト" } },
        { "lettuce", new List<string> { "salad", "greens", "生菜" } },
        { "onion", new List<string> { "onions", "洋葱", "オニオン" } },
        { "garlic sauce", new List<string> { "garlic", "toum", "garlic mayo" } },
        { "tahini", new List<string> { "sesame sauce", "sesame paste" } },
        { "sauce", new List<string> { "dressing", "condiment" } }
    };
    
    /// <summary>
    /// 检查是否为同义词
    /// </summary>
    private static bool AreSynonyms(string word1, string word2)
    {
        foreach (var entry in synonymDictionary)
        {
            var allSynonyms = new List<string>(entry.Value);
            allSynonyms.Add(entry.Key);
            
            if (allSynonyms.Contains(word1) && allSynonyms.Contains(word2))
                return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 计算字符串相似度（0-1）
    /// </summary>
    private static float CalculateSimilarity(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
            return 0f;
            
        int maxLen = Mathf.Max(s1.Length, s2.Length);
        int distance = LevenshteinDistance(s1, s2);
        
        return 1f - (float)distance / maxLen;
    }
    
    /// <summary>
    /// Levenshtein编辑距离
    /// </summary>
    private static int LevenshteinDistance(string s1, string s2)
    {
        int[,] d = new int[s1.Length + 1, s2.Length + 1];
        
        for (int i = 0; i <= s1.Length; i++)
            d[i, 0] = i;
            
        for (int j = 0; j <= s2.Length; j++)
            d[0, j] = j;
            
        for (int i = 1; i <= s1.Length; i++)
        {
            for (int j = 1; j <= s2.Length; j++)
            {
                int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                d[i, j] = Mathf.Min(
                    d[i - 1, j] + 1,
                    d[i, j - 1] + 1,
                    d[i - 1, j - 1] + cost
                );
            }
        }
        
        return d[s1.Length, s2.Length];
    }
    
    /// <summary>
    /// 获取匹配详情（用于调试）
    /// </summary>
    public static string GetMatchingDetails(Recipe recipe, List<IngredientData> potIngredients)
    {
        string details = $"=== 匹配详情: {recipe.recipeName} ===\n";
        
        foreach (var required in recipe.requiredIngredients)
        {
            bool found = false;
            string matchedWith = "";
            
            foreach (var available in potIngredients)
            {
                if (IsIngredientMatch(required.itemName, available.ingredientName))
                {
                    found = true;
                    matchedWith = available.ingredientName;
                    break;
                }
            }
            
            if (found)
            {
                details += $"✓ {required.itemName} → {matchedWith}\n";
            }
            else
            {
                details += $"✗ {required.itemName} (未找到)\n";
            }
        }
        
        return details;
    }
}