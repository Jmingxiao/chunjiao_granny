using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

[System.Serializable]
public class IngredientEvent : UnityEvent<IngredientData> { }

[System.Serializable]
public class CutIngredientEvent : UnityEvent<GameObject> { }

public class Cutboard : MonoBehaviour
{
   [Header("切菜板设置")]
    [SerializeField] private Transform ingredientSlot;
    [SerializeField] private float cuttingTime = 3f;
    [SerializeField] private bool autoCutting = true;
    
    [Header("事件")]
    public IngredientEvent OnCuttingCompleted; // 切割完成时触发
    public CutIngredientEvent OnCutIngredientReady; // 切好的食材准备好时触发

    [Header("UI组件")]
    public Slider cuttingProgressSlider;
    
    [Header("状态")]
    [SerializeField] private bool isOccupied = false;
    [SerializeField] private bool isCutting = false;
    
    private GameObject currentIngredient;
    private GameObject currentCutIngredient; // 当前切好的食材
    private Ingredient currentIngredientComponent;
    private float cuttingProgress = 0f;
    private Coroutine cuttingCoroutine;
    
    void Start()
    {
        if (ingredientSlot == null)
        {
            ingredientSlot = transform;
        }
        if (cuttingProgressSlider != null)
        {
            cuttingProgressSlider.value = 0;
            cuttingProgressSlider.gameObject.SetActive(false);
        }
        
        // 初始化事件
        if (OnCuttingCompleted == null)
            OnCuttingCompleted = new IngredientEvent();
        if (OnCutIngredientReady == null)
            OnCutIngredientReady = new CutIngredientEvent();
    }
    
    public bool PlaceIngredient(GameObject ingredient)
    {
        if (isOccupied)
        {
            Debug.Log("切菜板已被占用！");
            return false;
        }
        
        currentIngredientComponent = ingredient.GetComponent<Ingredient>();
        if (currentIngredientComponent == null)
        {
            Debug.LogError("放置的对象没有Ingredient组件！");
            return false;
        }
        
        if (currentIngredientComponent.Data == null || 
            currentIngredientComponent.Data.processedPrefab == null)
        {
            Debug.Log("这个食材不能被切！");
            return false;
        }
        
        currentIngredient = ingredient;
        isOccupied = true;
        
        ingredient.transform.position = ingredientSlot.position;
        ingredient.transform.parent = ingredientSlot;
        
        Debug.Log($"食材 {currentIngredientComponent.Data.ingredientName} 已放置在切菜板上");
        
        if (autoCutting)
        {
            StartCutting();
        }
        
        return true;
    }
    
    public void StartCutting()
    {
        if (!isOccupied || currentIngredient == null)
        {
            Debug.Log("没有食材可以切！");
            return;
        }
        if (cuttingProgressSlider != null)
        {
            cuttingProgressSlider.gameObject.SetActive(true);
            cuttingProgressSlider.value = 0;
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
    
    private IEnumerator CuttingProcess()
    {
        string ingredientName = currentIngredientComponent.Data.ingredientName;
        Debug.Log($"开始切 {ingredientName}...");
        
        while (cuttingProgress < cuttingTime)
        {
            cuttingProgress += Time.deltaTime;
            float progress = cuttingProgress / cuttingTime;
            if (cuttingProgressSlider != null)
            {
                cuttingProgressSlider.value = progress;
            }   
            yield return null;
        }
        
        
        CompleteCutting();
    }
    
    private void CompleteCutting()
    {
        IngredientData ingredientData = currentIngredientComponent.Data;
        Debug.Log($"{ingredientData.ingredientName} 切好了！");
        
        // 销毁原始食材
        Destroy(currentIngredient);
        
        // 生成切好的食材
        currentCutIngredient = Instantiate(
            ingredientData.processedPrefab, 
            ingredientSlot.position, 
            Quaternion.identity
        );
        if (cuttingProgressSlider != null)
        {
            cuttingProgressSlider.value = 0;
            cuttingProgressSlider.gameObject.SetActive(false);
        }
        // 设置为切菜板的子物体
        currentCutIngredient.transform.parent = ingredientSlot;
        
        // 如果切好的食材也需要Ingredient组件，设置数据
        Ingredient cutIngredientComponent = currentCutIngredient.GetComponent<Ingredient>();
        if (cutIngredientComponent != null)
        {
            cutIngredientComponent.SetIngredientData(ingredientData);
        }
        
        Debug.Log($"生成了切好的 {ingredientData.ingredientName}");
        
        // 触发事件
        OnCuttingCompleted?.Invoke(ingredientData);
        OnCutIngredientReady?.Invoke(currentCutIngredient);
        
        // 重置切菜状态（但保持占用，等待食材被取走）
        isCutting = false;
        cuttingProgress = 0f;
        currentIngredient = null;
        
        if (cuttingCoroutine != null)
        {
            StopCoroutine(cuttingCoroutine);
            cuttingCoroutine = null;
        }
    }
    
    /// <summary>
    /// 移除切好的食材（由锅具调用）
    /// </summary>
    public GameObject RemoveCutIngredient()
    {
        if (currentCutIngredient == null)
        {
            return null;
        }
        
        GameObject ingredient = currentCutIngredient;
        ingredient.transform.parent = null;
        
        currentCutIngredient = null;
        ResetCutBoard();
        
        return ingredient;
    }
    
    /// <summary>
    /// 获取当前切好的食材数据
    /// </summary>
    public IngredientData GetCutIngredientData()
    {
        if (currentCutIngredient != null)
        {
            Ingredient comp = currentCutIngredient.GetComponent<Ingredient>();
            return comp?.Data;
        }
        return null;
    }
    
    /// <summary>
    /// 检查是否有切好的食材
    /// </summary>
    public bool HasCutIngredient()
    {
        return currentCutIngredient != null;
    }
    
    private void ResetCutBoard()
    {
        isOccupied = false;
        isCutting = false;
        currentIngredient = null;
        currentIngredientComponent = null;
        currentCutIngredient = null;
        cuttingProgress = 0f;
        
        if (cuttingCoroutine != null)
        {
            StopCoroutine(cuttingCoroutine);
            cuttingCoroutine = null;
        }
        if (cuttingProgressSlider != null)
        {
            cuttingProgressSlider.value = 0;
            cuttingProgressSlider.gameObject.SetActive(false);
        }
    }
    
    public void StopCutting()
    {
        if (cuttingCoroutine != null)
        {
            StopCoroutine(cuttingCoroutine);
        }
        if (cuttingProgressSlider != null)
        {
            cuttingProgressSlider.gameObject.SetActive(false);
        }
        
        Debug.Log("切菜已停止");
        isCutting = false;
    }
    
    public float GetCuttingProgress()
    {
        return cuttingTime > 0 ? cuttingProgress / cuttingTime : 0f;
    }
    
    public IngredientData GetCurrentIngredientData()
    {
        return currentIngredientComponent?.Data;
    }
    
    public bool IsAvailable() => !isOccupied;
    public bool IsCutting() => isCutting;
    public bool HasIngredient() => isOccupied && currentIngredient != null;
    public float CuttingTime => cuttingTime;
}