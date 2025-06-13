using UnityEngine;

public class DraggableObject : MonoBehaviour
{
    private bool isDragging = false;
    private Camera mainCamera;
    private Rigidbody2D rb;
    private Vector3 offset;
    private float originalGravityScale;
    
    void Awake()
    {
        // 在Awake中初始化，确保更早获取引用
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        
        if (rb != null)
        {
            originalGravityScale = rb.gravityScale;
        }
    }
    
    void OnMouseDown()
    {
        if (rb != null)
        {
            // 开始拖拽时，设置为Kinematic
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        
        // 计算鼠标点击位置与物体中心的偏移
        Vector3 mousePos = GetMouseWorldPosition();
        offset = transform.position - mousePos;
        
        isDragging = true;
        Debug.Log($"开始拖拽: {gameObject.name}");
    }
    
    void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 mousePos = GetMouseWorldPosition();
            transform.position = mousePos + offset;
        }
    }
    
    void OnMouseUp()
    {
        if (isDragging)
        {
            if (rb != null)
            {
                // 释放时恢复为Dynamic
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = originalGravityScale;
            }
            
            isDragging = false;
            Debug.Log($"释放拖拽: {gameObject.name}");
        }
    }
    
    Vector3 GetMouseWorldPosition()
    {
        // 确保有相机引用
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        if (mainCamera == null)
        {
            Debug.LogError("找不到主相机！");
            return Vector3.zero;
        }
        
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f; // 相机距离
        return mainCamera.ScreenToWorldPoint(mousePos);
    }
    
    void OnDisable()
    {
        isDragging = false;
    }
    
    void OnDestroy()
    {
        isDragging = false;
    }
}