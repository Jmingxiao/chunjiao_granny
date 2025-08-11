using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;


// 极简版 - 只有点击排列功能
public class ArrangableItem : MonoBehaviour, IPointerClickHandler
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 5f;
    
    private bool isMoving = false;
    private bool isArranged = false;
    private Vector2 targetPosition;
    private Vector2 moveStartPosition;
    private float moveStartTime;
    
    void Awake()
    {
        // 确保有Collider2D
        if (GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
        }
    }
    
    void Update()
    {
        if (isMoving)
        {
            float elapsed = Time.time - moveStartTime;
            float duration = Vector2.Distance(moveStartPosition, targetPosition) / moveSpeed;
            
            if (elapsed < duration)
            {
                float t = elapsed / duration;
                transform.position = Vector2.Lerp(moveStartPosition, targetPosition, t);
            }
            else
            {
                transform.position = targetPosition;
                isMoving = false;
            }
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isArranged)
        {
            ArrangeItem();
        }
    }
    
    public void ArrangeItem()
    {
        if (ItemArrangementManager.Instance != null)
        {
            ItemArrangementManager.Instance.AddItemToArrangement(this);
            isArranged = true;
        }
    }
    
    public void SetTargetPosition(Vector2 position)
    {
        targetPosition = position;
        moveStartPosition = transform.position;
        moveStartTime = Time.time;
        isMoving = true;
    }
    
    public void ResetArrangement()
    {
        isArranged = false;
        if (ItemArrangementManager.Instance != null)
        {
            ItemArrangementManager.Instance.RemoveItemFromArrangement(this);
        }
    }
    
    public bool IsArranged => isArranged;

}