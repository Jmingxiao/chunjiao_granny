using UnityEngine;
using DG.Tweening;
using System.Collections;

public class FoodContainer : ClickableObject
{
   private SpriteRenderer meshRenderer;
    Material material;
    
    [Header("Food Container Settings")]
    [Tooltip("The ingredient data for this container")]
    public IngredientData ingredientData; // 使用IngredientData替代foodTag
    
    [Header("Target Settings")]
    [Tooltip("The target CutBoard where food will be thrown")]
    public Cutboard targetCutBoard;
    [Tooltip("The target Pot for direct ingredients")]
    public Pot targetPot;
    
    [Header("Throwing Settings")]
    public float throwDuration = 0.5f;
    public AnimationCurve throwCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float throwHeight = 2f;
    
    [Header("Click Animation Settings")]
    public float clickScaleFactor = 0.8f;
    public float clickDuration = 0.2f;
    
    void Start()
    {
        meshRenderer = GetComponent<SpriteRenderer>();
        if (meshRenderer)
            material = meshRenderer.material;
            
        // 自动查找目标
        if (targetCutBoard == null)
        {
            targetCutBoard = FindObjectOfType<Cutboard>();
        }
        
        if (targetPot == null)
        {
            targetPot = FindObjectOfType<Pot>();
        }
        
        // 验证配置
        if (ingredientData == null)
        {
            Debug.LogError($"FoodContainer {gameObject.name} 没有设置IngredientData!");
        }
    }
    
    protected override void OnClick()
    {
        if (ingredientData == null)
        {
            Debug.LogError("没有配置食材数据！");
            return;
        }
        
        // 根据食材类型决定目标
        if (ingredientData.RequiresCutting())
        {
            // 需要切的食材 - 检查切菜板
            if (targetCutBoard == null)
            {
                Debug.LogWarning("没有找到目标切菜板！");
                return;
            }
            
            if (!targetCutBoard.IsAvailable())
            {
                Debug.Log("切菜板正在使用中！");
                return;
            }
            
            ClickAnimation();
            ThrowToCutBoard();
        }
        else if (ingredientData.CanDirectlyToPot())
        {
            // 直接使用的食材 - 检查锅
            if (targetPot == null)
            {
                Debug.LogWarning("没有找到目标锅！");
                return;
            }
            
            if (targetPot.IsCooking())
            {
                Debug.Log("锅正在烹饪中！");
                return;
            }
            
            ClickAnimation();
            ThrowToPot();
        }
    }
    
    private void ClickAnimation()
    {
        transform.DOKill();
        transform.DOScale(clickScaleFactor, clickDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                transform.DOScale(1f, clickDuration).SetEase(Ease.OutQuad);
            });
    }
    
    /// <summary>
    /// 扔食材到切菜板
    /// </summary>
    private void ThrowToCutBoard()
    {
        StartCoroutine(ThrowToCutBoardCoroutine());
    }
    
    private IEnumerator ThrowToCutBoardCoroutine()
    {
        // 创建食材实例
        GameObject foodPiece = Instantiate(ingredientData.rawPrefab);
        
        // 添加Ingredient组件并设置数据
        Ingredient ingredient = foodPiece.GetComponent<Ingredient>();
        if (ingredient == null)
        {
            ingredient = foodPiece.AddComponent<Ingredient>();
        }
        ingredient.SetIngredientData(ingredientData);
        
        // 设置起始位置
        foodPiece.transform.position = transform.position;
        Vector3 targetPos = targetCutBoard.transform.position + Vector3.up * 0.5f;
        
        // 飞行动画
        yield return StartCoroutine(FlyToTarget(foodPiece, targetPos));
        
        // 尝试放置到切菜板
        bool placed = targetCutBoard.PlaceIngredient(foodPiece);
        
        if (!placed)
        {
            Debug.Log("放置失败，销毁食材");
            Destroy(foodPiece);
        }
    }
    
    /// <summary>
    /// 直接扔食材到锅
    /// </summary>
    private void ThrowToPot()
    {
        StartCoroutine(ThrowToPotCoroutine());
    }
    
    private IEnumerator ThrowToPotCoroutine()
    {
        // 对于直接使用的食材，使用processedPrefab
        GameObject foodPiece = Instantiate(ingredientData.processedPrefab);
        
        // 添加Ingredient组件
        Ingredient ingredient = foodPiece.GetComponent<Ingredient>();
        if (ingredient == null)
        {
            ingredient = foodPiece.AddComponent<Ingredient>();
        }
        ingredient.SetIngredientData(ingredientData);
        
        // 设置起始位置
        foodPiece.transform.position = transform.position;
        Vector3 targetPos = targetPot.transform.position + Vector3.up * 1f;
        
        // 飞行动画
        yield return StartCoroutine(FlyToTarget(foodPiece, targetPos));
        
        // 直接添加到锅
        targetPot.TryAddDirectIngredient(ingredientData);
        
        // 销毁临时对象（因为TryAddDirectIngredient会创建新的）
        Destroy(foodPiece);
    }
    
    /// <summary>
    /// 通用的飞行动画
    /// </summary>
    private IEnumerator FlyToTarget(GameObject obj, Vector3 targetPos)
    {
        Vector3 startPos = obj.transform.position;
        
        // 临时禁用物理
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
        
        Collider col = obj.GetComponent<Collider>();
        if (col != null) col.enabled = false;
        
        // 飞行动画
        float elapsed = 0f;
        while (elapsed < throwDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / throwDuration;
            float curveValue = throwCurve.Evaluate(t);
            
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, curveValue);
            currentPos.y += Mathf.Sin(t * Mathf.PI) * throwHeight;
            
            obj.transform.position = currentPos;
            yield return null;
        }
        
        obj.transform.position = targetPos;
        
        // 恢复物理
        if (col != null) col.enabled = true;
        if (rb != null) rb.isKinematic = false;
    }
}
