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

public class Cutboard : MonoBehaviour,IPointerClickHandler
{
   [Header("切菜板设置")]
    [SerializeField] private Transform ingredientSlot;
    [SerializeField] private float cuttingTime = 3f;
    [SerializeField] private bool autoCutting = true;
    
    [Header("事件")]
    public IngredientEvent OnCuttingCompleted; // 切割完成时触发
    public CutIngredientEvent OnCutIngredientReady; // 切好的食材准备好时触发

    [Header("弹出设置")]
    [SerializeField] private float ejectForce = 10f; // 弹出力度
    [SerializeField] private float ejectDuration = 1f; // 弹出动画时长
    [SerializeField] private AnimationCurve ejectCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

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
    
    /// <summary>
    /// 完成切割
    /// </summary>
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
    /// <summary>
    /// 处理点击事件
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        EjectIngredient();
    }
    
    /// <summary>
    /// 弹出当前食材
    /// </summary>
    public void EjectIngredient()
    {
        GameObject ingredientToEject = null;
        
        // 确定要弹出的食材
        if (currentCutIngredient != null)
        {
            // 优先弹出切好的食材
            ingredientToEject = currentCutIngredient;
            currentCutIngredient = null;
            Debug.Log("弹出切好的食材");
        }
        else if (currentIngredient != null)
        {
            // 弹出正在切或未切的食材
            ingredientToEject = currentIngredient;
            currentIngredient = null;
            
            // 如果正在切，停止切菜
            if (isCutting)
            {
                StopCutting();
                Debug.Log("停止切菜并弹出食材");
            }
            else
            {
                Debug.Log("弹出未切的食材");
            }
        }
        
        // 执行弹出
        if (ingredientToEject != null)
        {
            StartCoroutine(EjectAnimation(ingredientToEject));
            
            // 重置切菜板状态
            ResetCutBoard();
        }
        else
        {
            Debug.Log("切菜板上没有食材");
        }
    }
     // 添加按键支持（可选）
    void Update()
    {
        // 按E键也可以弹出食材
        if (Input.GetKeyDown(KeyCode.E) && (currentIngredient != null || currentCutIngredient != null))
        {
            EjectIngredient();
        }
    }
    /// <summary>
    /// 弹出动画
    /// </summary>
    private IEnumerator EjectAnimation(GameObject ingredient)
    {
        // 解除父子关系
        ingredient.transform.parent = null;
        
        // 计算弹出方向（向上并偏向一侧）
        Vector3 startPos = ingredient.transform.position;
        Vector3 ejectDirection = new Vector3(
            Random.Range(-1f, 1f), // 随机左右
            2f,                     // 向上
            Random.Range(-0.5f, 0.5f) // 轻微前后
        ).normalized;
        
        Vector3 targetPos = startPos + ejectDirection * ejectForce;
        
        // 添加旋转
        Vector3 randomRotation = new Vector3(
            Random.Range(-360f, 360f),
            Random.Range(-360f, 360f),
            Random.Range(-360f, 360f)
        );
        
        float elapsed = 0f;
        
        // 如果有刚体，暂时禁用
        Rigidbody rb = ingredient.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        
        // 弹出动画
        while (elapsed < ejectDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / ejectDuration;
            float curveValue = ejectCurve.Evaluate(t);
            
            // 位置插值
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, curveValue);
            
            // 添加抛物线效果
            float height = Mathf.Sin(t * Mathf.PI) * 2f;
            currentPos.y += height;
            
            ingredient.transform.position = currentPos;
            
            // 旋转
            ingredient.transform.rotation = Quaternion.Euler(randomRotation * t);
            
            yield return null;
        }
        
        // 销毁食材
        Destroy(ingredient);
        Debug.Log("食材已弹出并销毁");
    }
    
    /// <summary>
    /// 替代的弹出方案（使用DOTween）
    /// </summary>
    private void EjectWithDOTween(GameObject ingredient)
    {
        // 解除父子关系
        ingredient.transform.parent = null;
        
        // 随机方向
        Vector3 randomDirection = new Vector3(
            Random.Range(-3f, 3f),
            Random.Range(5f, 8f),
            Random.Range(-2f, 2f)
        );
        
        // 创建动画序列
        Sequence ejectSequence = DOTween.Sequence();
        
        // 弹出移动
        ejectSequence.Append(
            ingredient.transform.DOMove(
                ingredient.transform.position + randomDirection, 
                ejectDuration
            ).SetEase(Ease.OutQuad)
        );
        
        // 旋转
        ejectSequence.Join(
            ingredient.transform.DORotate(
                new Vector3(
                    Random.Range(-720f, 720f),
                    Random.Range(-720f, 720f),
                    Random.Range(-720f, 720f)
                ), 
                ejectDuration, 
                RotateMode.FastBeyond360
            )
        );
        
        // 缩小消失
        ejectSequence.Join(
            ingredient.transform.DOScale(0f, ejectDuration).SetEase(Ease.InBack)
        );
        
        // 完成后销毁
        ejectSequence.OnComplete(() => {
            Destroy(ingredient);
            Debug.Log("食材已弹出并销毁");
        });
    }
    
    public bool IsAvailable() => !isOccupied;
    public bool IsCutting() => isCutting;
    public bool HasIngredient() => isOccupied && currentIngredient != null;
    public float CuttingTime => cuttingTime;
}