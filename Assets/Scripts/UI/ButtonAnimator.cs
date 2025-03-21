using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(Button))]
public class ButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Hover Animation")]
    public bool useHoverAnimation = true;
    public float hoverScaleMultiplier = 1.1f;
    public float hoverAnimationDuration = 0.2f;
    
    [Header("Click Animation")]
    public bool useClickAnimation = true;
    public float clickScaleMultiplier = 0.9f;
    
    [Header("Color Animation")]
    public bool useColorAnimation = false;
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(0.9f, 0.9f, 1f);
    public Color pressedColor = new Color(0.8f, 0.8f, 0.9f);
    
    private Vector3 originalScale;
    private Image buttonImage;
    private Button button;
    private bool isPointerOver = false;
    private Coroutine scaleCoroutine;
    
    private void Awake()
    {
        originalScale = transform.localScale;
        buttonImage = GetComponent<Image>();
        button = GetComponent<Button>();
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable) return;
        
        isPointerOver = true;
        
        if (useHoverAnimation)
        {
            if (scaleCoroutine != null)
                StopCoroutine(scaleCoroutine);
                
            scaleCoroutine = StartCoroutine(ScaleAnimation(hoverScaleMultiplier));
        }
        
        if (useColorAnimation && buttonImage != null)
        {
            buttonImage.color = hoverColor;
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!button.interactable) return;
        
        isPointerOver = false;
        
        if (useHoverAnimation)
        {
            if (scaleCoroutine != null)
                StopCoroutine(scaleCoroutine);
                
            scaleCoroutine = StartCoroutine(ScaleAnimation(1f));
        }
        
        if (useColorAnimation && buttonImage != null)
        {
            buttonImage.color = normalColor;
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable) return;
        
        if (useClickAnimation)
        {
            if (scaleCoroutine != null)
                StopCoroutine(scaleCoroutine);
                
            scaleCoroutine = StartCoroutine(ScaleAnimation(clickScaleMultiplier));
        }
        
        if (useColorAnimation && buttonImage != null)
        {
            buttonImage.color = pressedColor;
        }
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!button.interactable) return;
        
        if (useClickAnimation)
        {
            if (scaleCoroutine != null)
                StopCoroutine(scaleCoroutine);
                
            float targetScale = isPointerOver ? hoverScaleMultiplier : 1f;
            scaleCoroutine = StartCoroutine(ScaleAnimation(targetScale));
        }
        
        if (useColorAnimation && buttonImage != null)
        {
            buttonImage.color = isPointerOver ? hoverColor : normalColor;
        }
    }
    
    private IEnumerator ScaleAnimation(float targetMultiplier)
    {
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = originalScale * targetMultiplier;
        float elapsedTime = 0f;
        
        while (elapsedTime < hoverAnimationDuration)
        {
            transform.localScale = Vector3.Lerp(startScale, targetScale, elapsedTime / hoverAnimationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        transform.localScale = targetScale;
    }
} 