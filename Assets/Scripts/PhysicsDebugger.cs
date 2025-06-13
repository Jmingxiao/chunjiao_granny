using UnityEngine;

public class PhysicsDebugger : MonoBehaviour
{
    void Start()
    {
        // 检查重力设置
        Vector2 gravity = Physics2D.gravity;
        Debug.Log($"2D物理重力设置: {gravity}");
        
        // 检查当前对象的物理组件
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Debug.Log($"{gameObject.name} - BodyType: {rb.bodyType}, GravityScale: {rb.gravityScale}, Mass: {rb.mass}");
        }
        
        // 检查碰撞器
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Debug.Log($"{gameObject.name} - Collider类型: {col.GetType().Name}, 已启用: {col.enabled}");
        }
    }
    
    void FixedUpdate()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
        {
            // 每隔一段时间输出位置，确认是否在移动
            if (Time.frameCount % 30 == 0)
            {
                Debug.Log($"{gameObject.name} 位置: {transform.position}, 速度: {rb.velocity}");
            }
        }
    }
}