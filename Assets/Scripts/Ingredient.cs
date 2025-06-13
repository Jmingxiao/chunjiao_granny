using UnityEngine;
using System.Collections;

// 食材组件，标记物体是食材并定义其类型和状态
[System.Serializable]
public enum IngredientType
{
    Chili,
    Cabbage,
    GreenPepper,
    PorkBelly,
    Scallion,
    Other
}

[System.Serializable]
public enum IngredientState
{
    Raw,        // 生的
    Chopped,    // 切过的
    Cooked      // 熟的
}

public class Ingredient : MonoBehaviour
{
    [Header("食材属性")]
    public IngredientType type = IngredientType.Chili;
    public IngredientState state = IngredientState.Raw;
    
    [Header("切过后的预制体")]
    public GameObject choppedPrefab; // 切过后的预制体
    
    [Header("显示设置")]
    public string displayName = "辣椒"; // 显示名称
    
    // 私有变量用于自动销毁功能
    private float checkDelay = 3f; // 停止拖拽后多久开始检查
    private float destroyDelay = 0.5f; // 确认不在有效表面后多久销毁
    private Coroutine destroyCheckCoroutine;
    
    void Start()
    {
        // 确保有正确的标签
        if (gameObject.tag != "Ingredient")
        {
            gameObject.tag = "Ingredient";
        }
    }
    
    void OnMouseUp()
    {
        // 当停止拖拽时，如果是切过的状态，开始检查
        if (state == IngredientState.Chopped)
        {
            StartDestroyCheck();
        }
    }
    
    void StartDestroyCheck()
    {
        // 停止之前的检查
        if (destroyCheckCoroutine != null)
        {
            StopCoroutine(destroyCheckCoroutine);
        }
        
        // 开始新的检查
        destroyCheckCoroutine = StartCoroutine(CheckAndDestroy());
    }
    
    IEnumerator CheckAndDestroy()
    {
        // 等待一段时间，给玩家反应时间
        yield return new WaitForSeconds(checkDelay);
        
        // 检查是否在有效表面上
        if (!IsOnValidSurface())
        {
            // 添加简单的淡出效果
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                float fadeTime = destroyDelay;
                float elapsed = 0;
                Color startColor = sr.color;
                
                while (elapsed < fadeTime)
                {
                    elapsed += Time.deltaTime;
                    float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
                    sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                    yield return null;
                }
            }
            
            // 销毁物体
            Debug.Log($"[Ingredient] {displayName} 不在有效表面上，已销毁");
            Destroy(gameObject);
        }
    }
    
    bool IsOnValidSurface()
    {
        // 使用OverlapCircle检测周围的碰撞体
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, 0.5f);
        
        foreach (Collider2D col in nearbyColliders)
        {
            if (col.gameObject == gameObject) continue;
            
            // 检查是否是案板
            if (col.GetComponent<IngredientProcessor>() != null ||
                col.gameObject.name.ToLower().Contains("board") ||
                col.gameObject.name.ToLower().Contains("cutboard"))
            {
                return true;
            }
            
            // 检查是否是盘子
            if (col.CompareTag("Plate") ||
                col.gameObject.name.ToLower().Contains("plate"))
            {
                return true;
            }
        }
        
        return false;
    }
    
    // 当状态改变时调用此方法
    public void SetState(IngredientState newState)
    {
        state = newState;
        
        // 如果刚变成切过的状态，不立即检查（等待放置）
        if (state == IngredientState.Chopped)
        {
            // 可以在这里添加一些初始化逻辑
        }
    }
    
    // 取消销毁检查（比如被重新拿起时）
    void OnMouseDown()
    {
        if (destroyCheckCoroutine != null)
        {
            StopCoroutine(destroyCheckCoroutine);
            destroyCheckCoroutine = null;
            
            // 确保恢复完全不透明
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 1f;
                sr.color = c;
            }
        }
    }
    
    // 获取食材的完整名称（包含状态）
    public string GetFullName()
    {
        string stateName = "";
        switch (state)
        {
            case IngredientState.Raw:
                stateName = "";
                break;
            case IngredientState.Chopped:
                stateName = "切好的";
                break;
            case IngredientState.Cooked:
                stateName = "熟的";
                break;
        }
        
        return stateName + displayName;
    }
    
    // 检查是否可以被切
    public bool CanBeChopped()
    {
        return state == IngredientState.Raw;
    }
    
    // 检查是否是熟食
    public bool IsCooked()
    {
        return state == IngredientState.Cooked;
    }
}