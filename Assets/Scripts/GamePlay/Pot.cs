using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

public class Pot : MonoBehaviour
{
    
   [SerializeField] private Cutboard cutboard;
    [SerializeField] private PotType potType;
    
    [Header("存储的食材")]
    [SerializeField] private List<IngredientData> ingredientsInPot = new List<IngredientData>();
    [SerializeField] private List<GameObject> ingredientObjects = new List<GameObject>();
    
    [Header("菜谱系统")]
    [SerializeField] private RecipeDatabase recipeDatabase; // 使用RecipeDatabase
    [SerializeField] private bool autoStartCooking = true;
    
    [Header("UI组件")]
    public Slider cookingProgressSlider;
    
    [Header("烹饪状态")]
    [SerializeField] private bool isCooking = false;
    [SerializeField] private float cookingTime = 10f;
    private Recipe currentRecipe = null;
    private List<Recipe> cachedRecipes; // 缓存转换后的Recipe对象
    private float cookingProgress = 0f;
    private Coroutine cookingCoroutine;
    
    private void Start()
    {
        if (cutboard == null)
        {
            cutboard = FindObjectOfType<Cutboard>();
        }
        if (cookingProgressSlider != null)
        {
            cookingProgressSlider.value = 0;
            cookingProgressSlider.gameObject.SetActive(false);
        }
        
        // 从RecipeDatabase获取菜谱
        if (recipeDatabase != null)
        {
            cachedRecipes = recipeDatabase.GetAllRecipes();
        }
        else
        {
            cachedRecipes = new List<Recipe>();
            Debug.LogWarning("没有设置RecipeDatabase");
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
                cutIngredient.transform.position = transform.position + Vector3.up * (ingredientsInPot.Count * 0.2f);
                cutIngredient.transform.parent = transform;
                
                ingredientsInPot.Add(ingredientData);
                ingredientObjects.Add(cutIngredient);
                
                Debug.Log($"添加食材: {ingredientData.ingredientName} (总计: {ingredientsInPot.Count})");
                
                CheckRecipes();
            }
        }
    }
    
    /// <summary>
    /// 直接添加食材（用于酱料等）
    /// </summary>
    public bool AddDirectIngredient(IngredientData ingredientData)
    {
        if (isCooking)
        {
            Debug.Log("正在烹饪中，无法添加食材");
            return false;
        }
        
        if (!ingredientData.CanDirectlyToPot())
        {
            Debug.Log($"{ingredientData.ingredientName} 需要先在切菜板上处理");
            return false;
        }
        
        if (ingredientData.potType != potType)
        {
            Debug.Log($"{ingredientData.ingredientName} 不适合这个锅");
            return false;
        }
        
        GameObject ingredientObj = Instantiate(
            ingredientData.processedPrefab,
            transform.position + Vector3.up * (ingredientsInPot.Count * 0.2f),
            Quaternion.identity
        );
        
        Ingredient ingredient = ingredientObj.GetComponent<Ingredient>();
        if (ingredient == null)
        {
            ingredient = ingredientObj.AddComponent<Ingredient>();
        }
        ingredient.SetIngredientData(ingredientData);
        
        ingredientObj.transform.parent = transform;
        
        ingredientsInPot.Add(ingredientData);
        ingredientObjects.Add(ingredientObj);
        
        Debug.Log($"直接添加食材: {ingredientData.ingredientName} (总计: {ingredientsInPot.Count})");
        
        CheckRecipes();
        
        return true;
    }
    
    private void CheckRecipes()
    {
        if (isCooking || cachedRecipes == null) return;
        
        // 遍历所有菜谱
        foreach (var recipe in cachedRecipes)
        {
            // 使用简化的匹配器
            if (FlexibleRecipeMatcher.CanMakeRecipe(recipe, ingredientsInPot))
            {
                Debug.Log($"<color=green>食材齐全，开始制作: {recipe.recipeName}</color>");
                
                if (autoStartCooking)
                {
                    StartCooking(recipe);
                }
                return;
            }
        }
        
        Debug.Log($"当前食材还不足以制作任何菜品");
    }
    
    public void StartCooking(Recipe recipe = null)
    {
        if (isCooking || recipe == null) return;
        if (cookingProgressSlider != null)
        {
            cookingProgressSlider.gameObject.SetActive(true);
            cookingProgressSlider.value = 0;
        }
        
        currentRecipe = recipe;
        isCooking = true;
        cookingProgress = 0f;
        cookingCoroutine = StartCoroutine(CookingProcess());
    }
    
    private IEnumerator CookingProcess()
    {
        
        while (cookingProgress < cookingTime)
        {
            cookingProgress += Time.deltaTime;
            float progress = cookingProgress / cookingTime;
            if (cookingProgressSlider != null)
            {
                cookingProgressSlider.value = progress;
            }   
            yield return null;
        }
        
        Debug.Log($"{currentRecipe.recipeName} 完成！");
        Debug.Log($"售价: ${currentRecipe.CalculatePrice()}");
        /// 生成成品
        GameObject finishedProduct = Instantiate(
            currentRecipe.dishPrefab,
            transform.position + Vector3.up * (ingredientsInPot.Count * 0.2f),
            Quaternion.identity
        );
        finishedProduct.transform.parent = transform;
        CompleteCooking();        
    }
    public void CompleteCooking()
    {
        // 清空锅中的食材
        foreach (var obj in ingredientObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        
        ingredientsInPot.Clear();
        ingredientObjects.Clear();
        
        isCooking = false;
        currentRecipe = null;
        if (cookingProgressSlider != null)
        {
            cookingProgressSlider.value = 0;
            cookingProgressSlider.gameObject.SetActive(false);
        }

        
    }
    
    public bool IsCooking()
    {
        return isCooking;
    }
    
    public bool CanAcceptIngredient()
    {
        return !isCooking && ingredientsInPot.Count < 10;
    }
}