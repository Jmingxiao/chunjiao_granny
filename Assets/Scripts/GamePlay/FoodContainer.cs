using UnityEngine;
using DG.Tweening;
using System.Collections;

public class FoodContainer : ClickableObject
{
   private SpriteRenderer meshRenderer;
    Material material;
    
    [Header("Food Container Settings")]
    [Tooltip("The type of food this container holds")]
    public string foodTag = "meat";
    
    [Header("Target Settings")]
    [Tooltip("The target CutBoard where food will be thrown")]
    public Cutboard targetCutBoard;
    [Tooltip("Alternative: Find CutBoard by tag")]
    public string cutBoardTag = "CutBoard";
    
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
            
        // 如果没有手动设置目标，尝试查找
        if (targetCutBoard == null)
        {
            GameObject cutBoardObj = GameObject.FindGameObjectWithTag(cutBoardTag);
            if (cutBoardObj != null)
            {
                targetCutBoard = cutBoardObj.GetComponent<Cutboard>();
            }
            else
            {
                targetCutBoard = FindObjectOfType<Cutboard>();
            }
        }
    }
    
    protected override void OnClick()
    {
        // 检查切菜板是否可用
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
        ThrowFoodToCutBoard();
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
    
    public void ThrowFoodToCutBoard()
    {
        StartCoroutine(ThrowFoodCoroutine());
    }
    
    private IEnumerator ThrowFoodCoroutine()
    {
        // 从对象池获取食材
        GameObject foodPiece = FoodContainerPool.Instance.GetObject(foodTag);
        if (foodPiece == null)
        {
            Debug.LogWarning($"对象池中没有可用的食材: {foodTag}");
            yield break;
        }
        
        // 确保食材有Ingredient组件
        Ingredient ingredient = foodPiece.GetComponent<Ingredient>();
        if (ingredient == null)
        {
            Debug.LogError("食材预制体缺少Ingredient组件！");
            FoodContainerPool.Instance.ReturnObject(foodTag, foodPiece);
            yield break;
        }
        
        // 设置起始位置
        foodPiece.transform.position = transform.position;
        foodPiece.transform.rotation = Quaternion.identity;
        
        // 获取目标位置
        Vector3 startPos = transform.position;
        Vector3 targetPos = targetCutBoard.transform.position + Vector3.up * 0.5f;
        
        // 临时禁用碰撞和物理
        Rigidbody rb = foodPiece.GetComponent<Rigidbody>();
        bool wasKinematic = false;
        if (rb != null)
        {
            wasKinematic = rb.isKinematic;
            rb.isKinematic = true;
        }
        
        Collider col = foodPiece.GetComponent<Collider>();
        bool wasEnabled = true;
        if (col != null)
        {
            wasEnabled = col.enabled;
            col.enabled = false;
        }
        
        // 飞行动画
        float elapsed = 0f;
        while (elapsed < throwDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / throwDuration;
            float curveValue = throwCurve.Evaluate(t);
            
            // 计算位置
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, curveValue);
            // 添加抛物线高度
            currentPos.y += Mathf.Sin(t * Mathf.PI) * throwHeight;
            
            foodPiece.transform.position = currentPos;
            
            yield return null;
        }
        
        // 确保到达准确位置
        foodPiece.transform.position = targetPos;
        
        // 恢复碰撞和物理
        if (col != null)
        {
            col.enabled = wasEnabled;
        }
        if (rb != null)
        {
            rb.isKinematic = wasKinematic;
        }
        
        // 尝试放置到切菜板
        bool placed = targetCutBoard.PlaceIngredient(foodPiece);
        
        if (placed)
        {
            Debug.Log($"成功将 {foodTag} 放置到切菜板！");
            // 可选：自动开始切菜
            // targetCutBoard.StartCutting();
        }
        else
        {
            Debug.Log("放置失败，归还食材到对象池");
            FoodContainerPool.Instance.ReturnObject(foodTag, foodPiece);
        }
    }
}
