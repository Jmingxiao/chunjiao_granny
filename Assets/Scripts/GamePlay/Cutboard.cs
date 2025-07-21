using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Cutboard : MonoBehaviour
{
  [Header("切菜板设置")]
    [SerializeField] private Transform ingredientSlot;
    [SerializeField] private float cuttingTime = 3f; // 固定的切菜时间
    
    [Header("状态")]
    [SerializeField] private bool isOccupied = false;
    [SerializeField] private bool isCutting = false;
    public Slider cuttingProgressSlider;
    
    private GameObject currentIngredient;
    private Ingredient currentIngredientComponent;
    private float cuttingProgress = 0f;
    private Coroutine cuttingCoroutine;
    
    void Start()
    {
        if (ingredientSlot == null)
        {
            ingredientSlot = transform;
        }
    }
    
    /// <summary>
    /// 检测进入触发区域的物体
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        // 自动尝试放置进入触发区域的食材
        if (!isOccupied && other.CompareTag("Ingredient"))
        {
            PlaceIngredient(other.gameObject);
        }
    }
    
    /// <summary>
    /// 也可以用碰撞检测
    /// </summary>
    void OnCollisionEnter(Collision collision)
    {
        // 当物体碰到切菜板时自动尝试放置
        if (!isOccupied && collision.gameObject.GetComponent<Ingredient>() != null)
        {
            PlaceIngredient(collision.gameObject);
        }
    }
    
    /// <summary>
    /// 放置食材到切菜板上
    /// </summary>
    public bool PlaceIngredient(GameObject ingredient)
    {
        if (isOccupied)
        {
            Debug.Log("切菜板已被占用！");
            return false;
        }
        
        // 检查是否有Ingredient组件
        currentIngredientComponent = ingredient.GetComponent<Ingredient>();
        if (currentIngredientComponent == null)
        {
            Debug.LogError("放置的对象没有Ingredient组件！");
            return false;
        }
        
        // 检查是否有加工后的预制体
        if (currentIngredientComponent.Data == null || 
            currentIngredientComponent.Data.processedPrefab == null)
        {
            Debug.Log("这个食材不能被切！");
            return false;
        }
        
        currentIngredient = ingredient;
        isOccupied = true;
        
        // 将食材移动到切菜板位置
        ingredient.transform.position = ingredientSlot.position;
        ingredient.transform.parent = ingredientSlot;
        
        Debug.Log($"食材 {currentIngredientComponent.Data.ingredientName} 已放置在切菜板上");
        
        // 自动开始切菜
        StartCutting();
        
        return true;
    }
    
    /// <summary>
    /// 开始切菜
    /// </summary>
    public void StartCutting()
    {
        if (!isOccupied || currentIngredient == null)
        {
            Debug.Log("没有食材可以切！");
            return;
        }
        
        if (isCutting)
        {
            Debug.Log("正在切菜中...");
            return;
        }
        
        isCutting = true;
        cuttingProgress = 0f;
        cuttingCoroutine = StartCoroutine(CuttingProcess());
    }
    
    /// <summary>
    /// 切菜过程协程
    /// </summary>
    private IEnumerator CuttingProcess()
    {
        string ingredientName = currentIngredientComponent.Data.ingredientName;
        Debug.Log($"开始切 {ingredientName}...");
        
        while (cuttingProgress < cuttingTime)
        {
            cuttingProgress += Time.deltaTime;
            float progress = cuttingProgress / cuttingTime;
            cuttingProgressSlider.value = progress;
            
            // 可以在这里触发进度更新事件
            // Debug.Log($"切菜进度: {progress * 100:F1}%");
            
            yield return null;
        }
        
        // 切菜完成
        CompleteCutting();
    }
    
    /// <summary>
    /// 完成切菜
    /// </summary>
    private void CompleteCutting()
    {
        IngredientData ingredientData = currentIngredientComponent.Data;
        Debug.Log($"{ingredientData.ingredientName} 切好了！");
        
        // 销毁原始食材
        Destroy(currentIngredient);
        
        // 生成切好的食材
        GameObject cutIngredient = Instantiate(
            ingredientData.processedPrefab, 
            ingredientSlot.position, 
            Quaternion.identity
        );
        
        // 如果切好的食材也需要Ingredient组件，设置数据
        Ingredient cutIngredientComponent = cutIngredient.GetComponent<Ingredient>();
        if (cutIngredientComponent != null)
        {
            cutIngredientComponent.SetIngredientData(ingredientData);
        }
        
        Debug.Log($"生成了切好的 {ingredientData.ingredientName}");
        
        // 重置状态
        ResetCutBoard();
    }
    
    /// <summary>
    /// 移除食材（不切菜，直接拿走）
    /// </summary>
    public GameObject RemoveIngredient()
    {
        if (!isOccupied || currentIngredient == null)
        {
            return null;
        }
        
        if (isCutting)
        {
            StopCutting();
        }
        
        GameObject ingredient = currentIngredient;
        ingredient.transform.parent = null;
        
        ResetCutBoard();
        return ingredient;
    }
    
    /// <summary>
    /// 重置切菜板状态
    /// </summary>
    private void ResetCutBoard()
    {
        isOccupied = false;
        isCutting = false;
        currentIngredient = null;
        currentIngredientComponent = null;
        cuttingProgress = 0f;
        
        if (cuttingCoroutine != null)
        {
            StopCoroutine(cuttingCoroutine);
            cuttingCoroutine = null;
        }
    }
    
    /// <summary>
    /// 停止切菜
    /// </summary>
    public void StopCutting()
    {
        if (cuttingCoroutine != null)
        {
            StopCoroutine(cuttingCoroutine);
        }
        
        Debug.Log("切菜已停止");
        isCutting = false;
    }
    
    /// <summary>
    /// 获取切菜进度（0-1）
    /// </summary>
    public float GetCuttingProgress()
    {
        return cuttingTime > 0 ? cuttingProgress / cuttingTime : 0f;
    }
    
    /// <summary>
    /// 获取当前食材数据
    /// </summary>
    public IngredientData GetCurrentIngredientData()
    {
        return currentIngredientComponent?.Data;
    }
    
    // 公共属性
    public bool IsAvailable() => !isOccupied;
    public bool IsCutting() => isCutting;
    public bool HasIngredient() => isOccupied && currentIngredient != null;
    public float CuttingTime => cuttingTime;
}
