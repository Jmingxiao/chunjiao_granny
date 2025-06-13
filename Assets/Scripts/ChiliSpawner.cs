using UnityEngine;

public class ChiliSpawner : MonoBehaviour
{
    [Header("辣椒预制体")]
    public GameObject chiliPrefab; // 需要在Inspector中设置
    
    [Header("生成设置")]
    public float spawnOffsetY = 0.2f; // Y轴生成偏移
    
    private GameObject currentChili; // 当前正在拖拽的辣椒
    private bool isDragging = false;
    private Camera mainCamera;
    
    void Start()
    {
        // 获取主相机引用
        mainCamera = Camera.main;
        
        // 如果没有设置预制体，尝试从场景中获取
        if (chiliPrefab == null)
        {
            GameObject sceneChili = GameObject.Find("Chili");
            if (sceneChili != null)
            {
                chiliPrefab = sceneChili;
                Debug.Log("已从场景中找到Chili对象作为模板");
                
                // 确保模板辣椒不会掉落
                Rigidbody2D templateRb = sceneChili.GetComponent<Rigidbody2D>();
                if (templateRb != null)
                {
                    templateRb.bodyType = RigidbodyType2D.Kinematic;
                }
            }
        }
    }
    
    void OnMouseDown()
    {
        if (chiliPrefab == null)
        {
            Debug.LogError("未设置辣椒预制体！");
            return;
        }
        
        // 获取鼠标世界坐标
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        mouseWorldPos.z = 0;
        
        // 实例化辣椒
        currentChili = Instantiate(chiliPrefab, mouseWorldPos, Quaternion.identity);
        currentChili.SetActive(true);
        currentChili.name = "Chili_Generated_" + Time.time;
        currentChili.tag = "Ingredient"; // 设置标签
        
        // 移除调试脚本（如果有的话）
        PhysicsDebugger debugger = currentChili.GetComponent<PhysicsDebugger>();
        if (debugger != null)
        {
            Destroy(debugger);
        }
        
        // 添加拖动功能
        DraggableObject draggable = currentChili.GetComponent<DraggableObject>();
        if (draggable == null)
        {
            draggable = currentChili.AddComponent<DraggableObject>();
        }
        
        // 添加食材组件
        Ingredient ingredient = currentChili.GetComponent<Ingredient>();
        if (ingredient == null)
        {
            ingredient = currentChili.AddComponent<Ingredient>();
            ingredient.type = IngredientType.Chili;
            ingredient.state = IngredientState.Raw;
            ingredient.displayName = "辣椒";
            
            // 设置切好的预制体（如果有的话）
            GameObject choppedPrefab = GameObject.Find("ChoppedChili");
            if (choppedPrefab != null)
            {
                ingredient.choppedPrefab = choppedPrefab;
            }
        }
        
        // 确保有正确的物理组件
        Rigidbody2D rb = currentChili.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = currentChili.AddComponent<Rigidbody2D>();
        }
        
        // 设置为Kinematic，开始拖拽
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 1f; // 预设重力比例
        
        isDragging = true;
        Debug.Log($"生成辣椒: {currentChili.name}");
    }
    
    void OnMouseDrag()
    {
        if (isDragging && currentChili != null)
        {
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            mouseWorldPos.z = 0;
            currentChili.transform.position = mouseWorldPos;
        }
    }
    
    void OnMouseUp()
    {
        if (isDragging && currentChili != null)
        {
            // 释放时切换为Dynamic，开始物理模拟
            Rigidbody2D rb = currentChili.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                Debug.Log($"释放辣椒，启用物理: {currentChili.name}");
            }
            
            isDragging = false;
            currentChili = null;
        }
    }
    
    Vector3 GetMouseWorldPosition()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f; // 相机距离
        return mainCamera.ScreenToWorldPoint(mousePos);
    }
    
    void OnDestroy()
    {
        // 清理
        if (isDragging && currentChili != null)
        {
            Destroy(currentChili);
        }
    }
}