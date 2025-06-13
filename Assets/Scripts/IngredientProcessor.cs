using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 食材加工处理器 - 添加到CutBoard上
/// 管理不同食材的加工配方和输出
/// </summary>
public class IngredientProcessor : MonoBehaviour
{
    [System.Serializable]
    public class ProcessingRecipe
    {
        [Header("输入")]
        public string inputIngredientName = "辣椒";  // 原材料名称
        public IngredientType inputType = IngredientType.Chili;  // 原材料类型
        
        [Header("输出")]
        public GameObject processedPrefab;  // 加工后的预制体
        public string outputName = "切好的辣椒";  // 输出名称
        
        [Header("加工设置")]
        public float processingTime = 2f;  // 加工时间
        public Color progressBarColor = Color.green;  // 进度条颜色
    }
    
    [Header("加工配方")]
    public List<ProcessingRecipe> recipes = new List<ProcessingRecipe>();
    
    [Header("加工设置")]
    public Vector3 itemPlacementOffset = new Vector3(0, 0.5f, 0);  // 原材料放置偏移
    public Vector3 progressBarOffset = new Vector3(0, 1f, 0);  // 进度条偏移
    
    [Header("成品生成位置")]
    public bool useExactPosition = true;  // 使用精确位置还是相对偏移
    public Vector3 exactOutputPosition = new Vector3(0.83f, -1.91f, 0);  // 精确生成位置
    public Vector3 outputSpawnOffset = new Vector3(0, 0.8f, -0.5f);  // 成品生成偏移（当useExactPosition为false时使用）
    
    [Header("状态")]
    public bool isProcessing = false;
    public GameObject currentItem;
    public GameObject processedItem;
    
    private GameObject progressBar;
    private Collider2D boardCollider;
    
    void Start()
    {
        // 确保有触发器碰撞体
        boardCollider = GetComponent<Collider2D>();
        if (boardCollider == null)
        {
            boardCollider = gameObject.AddComponent<BoxCollider2D>();
        }
        boardCollider.isTrigger = true;
        
        // 如果没有配方，添加默认配方
        if (recipes.Count == 0)
        {
            recipes.Add(new ProcessingRecipe());
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // 如果正在处理或已有物品，忽略
        if (isProcessing || currentItem != null) return;
        
        // 检查是否是可拖拽的原材料
        if (!other.CompareTag("Draggable") && !other.CompareTag("Ingredient")) return;
        
        Ingredient ingredient = other.GetComponent<Ingredient>();
        if (ingredient == null) return;
        
        // 检查是否是生的原材料
        if (ingredient.state != IngredientState.Raw) return;
        
        // 查找匹配的配方
        ProcessingRecipe recipe = FindRecipe(ingredient);
        if (recipe != null && recipe.processedPrefab != null)
        {
            AcceptItem(other.gameObject, recipe);
        }
        else
        {
            Debug.Log($"[加工器] 没有找到 {ingredient.displayName} 的加工配方");
        }
    }
    
    ProcessingRecipe FindRecipe(Ingredient ingredient)
    {
        foreach (var recipe in recipes)
        {
            if (recipe.inputType == ingredient.type ||
                recipe.inputIngredientName == ingredient.displayName)
            {
                return recipe;
            }
        }
        return null;
    }
    
    void AcceptItem(GameObject item, ProcessingRecipe recipe)
    {
        currentItem = item;
        Debug.Log($"[加工器] 接受原材料: {item.name}");
        
        // 停止物理模拟
        Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.velocity = Vector2.zero;
        }
        
        // 放置到案板上
        item.transform.position = transform.position + itemPlacementOffset;
        
        // 禁用拖拽 - 直接禁用DraggableObject组件
        DraggableObject draggableObject = item.GetComponent<DraggableObject>();
        if (draggableObject != null)
        {
            draggableObject.enabled = false;
        }
        
        // 开始加工
        StartCoroutine(ProcessItem(recipe));
    }
    
    IEnumerator ProcessItem(ProcessingRecipe recipe)
    {
        isProcessing = true;
        Debug.Log($"[加工器] 开始加工 {recipe.inputIngredientName} -> {recipe.outputName}");
        
        // 创建进度条
        CreateProgressBar(recipe.progressBarColor);
        
        // 处理进度
        float elapsedTime = 0f;
        while (elapsedTime < recipe.processingTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / recipe.processingTime;
            UpdateProgressBar(progress);
            yield return null;
        }
        
        // 销毁进度条
        DestroyProgressBar();
        
        // 隐藏原材料
        if (currentItem != null)
        {
            currentItem.SetActive(false);
            Debug.Log($"[加工器] 隐藏原材料: {currentItem.name}");
        }
        
        // 生成加工后的食材
        Vector3 spawnPosition;
        if (useExactPosition)
        {
            spawnPosition = exactOutputPosition;
            Debug.Log($"[加工器] 使用精确位置: {spawnPosition}");
        }
        else
        {
            spawnPosition = transform.position + outputSpawnOffset;
            Debug.Log($"[加工器] 使用相对位置: {spawnPosition}");
        }
        
        processedItem = Instantiate(recipe.processedPrefab, spawnPosition, Quaternion.identity);
        processedItem.name = recipe.outputName;
        
        // 设置加工后食材的属性
        SetupProcessedItem(processedItem, recipe);
        
        Debug.Log($"[加工器] 生成成品: {processedItem.name} at {spawnPosition}");
        
        // 清理原材料
        if (currentItem != null)
        {
            Destroy(currentItem, 0.5f);
        }
        
        isProcessing = false;
        currentItem = null;
    }
    
    void SetupProcessedItem(GameObject item, ProcessingRecipe recipe)
    {
        // 确保有必要的组件
        item.tag = "Ingredient";
        
        // 添加或更新Ingredient组件
        Ingredient ingredient = item.GetComponent<Ingredient>();
        if (ingredient == null)
        {
            ingredient = item.AddComponent<Ingredient>();
        }
        ingredient.state = IngredientState.Chopped;
        ingredient.displayName = recipe.outputName;
        
        // 调用SetState确保正确初始化
        ingredient.SetState(IngredientState.Chopped);
        
        // 确保可以拖拽
        DraggableObject draggableObject = item.GetComponent<DraggableObject>();
        if (draggableObject == null)
        {
            // 添加DraggableObject组件
            draggableObject = item.AddComponent<DraggableObject>();
        }
        
        // 确保组件是启用的
        draggableObject.enabled = true;
        
        // 确保有刚体
        Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = item.AddComponent<Rigidbody2D>();
        }
        rb.mass = 0.5f;
        rb.drag = 0.5f;
        
        // 确保有碰撞体
        if (item.GetComponent<Collider2D>() == null)
        {
            BoxCollider2D col = item.AddComponent<BoxCollider2D>();
            // 自动调整碰撞体大小
            SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                col.size = sr.sprite.bounds.size;
            }
        }
    }
    
    void CreateProgressBar(Color fillColor)
    {
        progressBar = new GameObject("ProcessingProgress");
        progressBar.transform.position = transform.position + progressBarOffset;
        
        // 背景
        GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bg.transform.SetParent(progressBar.transform);
        bg.transform.localPosition = Vector3.zero;
        bg.transform.localScale = new Vector3(1f, 0.1f, 1f);
        Renderer bgRenderer = bg.GetComponent<Renderer>();
        bgRenderer.material.color = Color.gray;
        bgRenderer.sortingOrder = 10;
        
        // 填充条
        GameObject fill = GameObject.CreatePrimitive(PrimitiveType.Quad);
        fill.transform.SetParent(progressBar.transform);
        fill.transform.localPosition = new Vector3(-0.5f, 0, -0.01f);
        fill.transform.localScale = new Vector3(0, 0.08f, 1f);
        fill.name = "Fill";
        Renderer fillRenderer = fill.GetComponent<Renderer>();
        fillRenderer.material.color = fillColor;
        fillRenderer.sortingOrder = 11;
    }
    
    void UpdateProgressBar(float progress)
    {
        if (progressBar != null)
        {
            Transform fill = progressBar.transform.Find("Fill");
            if (fill != null)
            {
                fill.localScale = new Vector3(progress, 0.08f, 1f);
                fill.localPosition = new Vector3((progress - 1f) * 0.5f, 0, -0.01f);
            }
        }
    }
    
    void DestroyProgressBar()
    {
        if (progressBar != null)
        {
            Destroy(progressBar);
            progressBar = null;
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        // 如果加工后的物品离开案板
        if (processedItem != null && other.gameObject == processedItem)
        {
            // 恢复物理模拟
            Rigidbody2D rb = processedItem.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
            }
            
            processedItem = null;
        }
    }
    
    // 在编辑器中绘制辅助线
    void OnDrawGizmosSelected()
    {
        // 原材料放置位置
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + itemPlacementOffset, 0.2f);
        
        // 成品生成位置
        Gizmos.color = Color.green;
        if (useExactPosition)
        {
            // 显示精确位置
            Gizmos.DrawWireSphere(exactOutputPosition, 0.25f);
            Gizmos.DrawLine(transform.position, exactOutputPosition);
            
            // 在精确位置画一个十字
            float crossSize = 0.3f;
            Gizmos.DrawLine(
                exactOutputPosition + Vector3.left * crossSize, 
                exactOutputPosition + Vector3.right * crossSize
            );
            Gizmos.DrawLine(
                exactOutputPosition + Vector3.up * crossSize, 
                exactOutputPosition + Vector3.down * crossSize
            );
        }
        else
        {
            // 显示相对位置
            Gizmos.DrawWireSphere(transform.position + outputSpawnOffset, 0.2f);
        }
        
        // 进度条位置
        Gizmos.color = Color.blue;
        Vector3 barPos = transform.position + progressBarOffset;
        Gizmos.DrawWireCube(barPos, new Vector3(1f, 0.1f, 0.1f));
    }
}