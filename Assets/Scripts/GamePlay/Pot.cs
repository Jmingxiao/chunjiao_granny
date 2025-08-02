using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Pot : MonoBehaviour
{
    
   [SerializeField] private Cutboard cutboard;
    [SerializeField] private PotType potType;
    
    [Header("存储的食材")]
    [SerializeField] private List<IngredientData> ingredientsInPot = new List<IngredientData>();
    [SerializeField] private List<GameObject> ingredientObjects = new List<GameObject>();
    
    [Header("菜谱系统")]
    [SerializeField] private List<Recipe> availableRecipes;
    [SerializeField] private bool autoStartCooking = true;
    [SerializeField] private bool showMatchDetails = true; // 显示匹配详情
    
    [Header("菜品预制体")]
    [SerializeField] private List<DishPrefabMapping> dishPrefabs = new List<DishPrefabMapping>();
    
    [System.Serializable]
    public class DishPrefabMapping
    {
        public string recipeName;
        public GameObject dishPrefab;
    }
    
    [Header("烹饪状态")]
    [SerializeField] public bool isCooking = false;
    [SerializeField] private Recipe currentRecipe = null;
    private Coroutine cookingCoroutine;
    
    private void Start()
    {
        if (cutboard == null)
        {
            cutboard = FindObjectOfType<Cutboard>();
        }
        
        // 如果没有手动设置菜谱，获取通用菜谱
        if (availableRecipes == null || availableRecipes.Count == 0)
        {
            availableRecipes = Recipe.GetCommonRecipes();
        }
        
        if (cutboard != null)
        {
            cutboard.OnCuttingCompleted.AddListener(HandleCuttingCompleted);
        }
    }
    
    private void OnDestroy()
    {
        if (cutboard != null)
        {
            cutboard.OnCuttingCompleted.RemoveListener(HandleCuttingCompleted);
        }
    }
    
    private void HandleCuttingCompleted(IngredientData ingredientData)
    {
        if (ingredientData.potType == potType && !isCooking)
        {
            GameObject cutIngredient = cutboard.RemoveCutIngredient();
            
            if (cutIngredient != null)
            {
                // 将食材移到锅中
                cutIngredient.transform.position = transform.position + Vector3.up * (ingredientsInPot.Count * 0.2f);
                cutIngredient.transform.parent = transform;
                
                // 存储食材
                ingredientsInPot.Add(ingredientData);
                ingredientObjects.Add(cutIngredient);
                
                Debug.Log($"添加食材: {ingredientData.ingredientName} (总计: {ingredientsInPot.Count})");
                
                // 检查可制作的菜品
                CheckRecipes();
            }
        }
    }

    /// <summary>
    /// 检查可制作的菜品
    /// </summary>
    private void CheckRecipes()
    {
        if (isCooking) return;
        
        // 按人气度排序
        availableRecipes.Sort((a, b) => b.popularity.CompareTo(a.popularity));
        
        foreach (var recipe in availableRecipes)
        {
            if (FlexibleRecipeMatcher.CanMakeRecipe(recipe, ingredientsInPot))
            {
                Debug.Log($"<color=green>可以制作: {recipe.recipeName}</color>");
                
                if (showMatchDetails)
                {
                    Debug.Log(FlexibleRecipeMatcher.GetMatchingDetails(recipe, ingredientsInPot));
                }
                
                if (autoStartCooking)
                {
                    StartCooking(recipe);
                    break;
                }
            }
        }
    }
    
    /// <summary>
    /// 开始烹饪
    /// </summary>
    /// <param name="recipe">菜谱</param>
    public void StartCooking(Recipe recipe)
    {
        if (isCooking || recipe == null) return;
        
        currentRecipe = recipe;
        isCooking = true;
        
        if (cookingCoroutine != null)
        {
            StopCoroutine(cookingCoroutine);
        }
        
        cookingCoroutine = StartCoroutine(CookingProcess());
    }
    
    private IEnumerator CookingProcess()
    {
        float cookingTime = currentRecipe.CalculatePreparationTime();
        Debug.Log($"开始烹饪 {currentRecipe.recipeName} ({cookingTime}秒)");
        
        // 等待烹饪
        yield return new WaitForSeconds(cookingTime);
        
        // 生成菜品
        //Recipe的recipeName是菜谱的名称，dishPrefabs是菜谱的预制体
        //GetDishPrefab(currentRecipe.recipeName)是获取菜谱的预制体
        
        GameObject dishPrefab = GetDishPrefab(currentRecipe.recipeName);
        if (dishPrefab != null)
        {
            GameObject dish = Instantiate(dishPrefab, 
                transform.position + Vector3.up * 2f, 
                Quaternion.identity);

                
            Debug.Log($"<color=yellow>{currentRecipe.recipeName} 完成！</color>");
        }
        else
        {
            Debug.LogWarning($"未找到 {currentRecipe.recipeName} 的预制体");
        }
        
        // 清空锅中的食材
        ClearPot();
        
        // 重置状态
        isCooking = false;
        currentRecipe = null;
    }

    public void TryAddDirectIngredient(IngredientData ingredientData)
    {
        if (ingredientData.CanDirectlyToPot())
        {
            ingredientsInPot.Add(ingredientData);
        }
    }


    /// <summary>
    /// 获取菜谱的预制体
    /// </summary>
    /// <param name="recipeName">菜谱的名称</param>
    /// <returns>菜谱的预制体</returns>
    private GameObject GetDishPrefab(string recipeName)
    {
        var mapping = dishPrefabs.Find(d => 
            d.recipeName.ToLower() == recipeName.ToLower());
        return mapping?.dishPrefab;
    }
    
    
    private void ClearPot()
    {
        foreach (var obj in ingredientObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        
        ingredientsInPot.Clear();
        ingredientObjects.Clear();
    }
    
    /// <summary>
    /// 手动检查可制作的菜谱
    /// </summary>
    [ContextMenu("Check Available Recipes")]
    public void DebugCheckRecipes()
    {
        Debug.Log("=== 可制作的菜谱 ===");
        foreach (var recipe in availableRecipes)
        {
            if (FlexibleRecipeMatcher.CanMakeRecipe(recipe, ingredientsInPot))
            {
                Debug.Log($"✓ {recipe.recipeName}");
                Debug.Log(FlexibleRecipeMatcher.GetMatchingDetails(recipe, ingredientsInPot));
            }
        }
    }
    public bool IsCooking()
    {
        return isCooking;
    }
}