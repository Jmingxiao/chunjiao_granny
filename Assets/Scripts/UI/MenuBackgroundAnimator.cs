using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuBackgroundAnimator : MonoBehaviour
{
    [Header("Parallax Settings")]
    public bool enableParallax = true;
    public float parallaxSpeed = 0.5f;
    public float parallaxAmount = 30f;
    
    [Header("Background Color")]
    public bool useColorCycle = true;
    public Image backgroundImage;
    public Color[] backgroundColors;
    public float colorTransitionDuration = 5f;
    
    [Header("Floating Elements")]
    public bool enableFloatingElements = true;
    public RectTransform[] floatingElements;
    public float floatMinSpeed = 0.5f;
    public float floatMaxSpeed = 1.5f;
    public float floatMinAmount = 20f;
    public float floatMaxAmount = 50f;
    
    private Vector2 initialPosition;
    private Vector2[] floatingElementsInitialPositions;
    private float[] floatingElementsSpeeds;
    private float[] floatingElementsOffsets;
    private float[] floatingElementsAmounts;
    private int currentColorIndex = 0;
    private Coroutine colorTransitionCoroutine;
    
    private void Start()
    {
        initialPosition = transform.position;
        
        // Initialize floating elements
        if (enableFloatingElements && floatingElements != null && floatingElements.Length > 0)
        {
            floatingElementsInitialPositions = new Vector2[floatingElements.Length];
            floatingElementsSpeeds = new float[floatingElements.Length];
            floatingElementsOffsets = new float[floatingElements.Length];
            floatingElementsAmounts = new float[floatingElements.Length];
            
            for (int i = 0; i < floatingElements.Length; i++)
            {
                if (floatingElements[i] != null)
                {
                    floatingElementsInitialPositions[i] = floatingElements[i].anchoredPosition;
                    floatingElementsSpeeds[i] = Random.Range(floatMinSpeed, floatMaxSpeed);
                    floatingElementsOffsets[i] = Random.Range(0f, 2f * Mathf.PI);
                    floatingElementsAmounts[i] = Random.Range(floatMinAmount, floatMaxAmount);
                }
            }
        }
        
        // Start color cycle
        if (useColorCycle && backgroundImage != null && backgroundColors != null && backgroundColors.Length >= 2)
        {
            backgroundImage.color = backgroundColors[0];
            StartNextColorTransition();
        }
    }
    
    private void Update()
    {
        // Parallax effect on mouse movement
        if (enableParallax)
        {
            float mouseX = Input.mousePosition.x / Screen.width - 0.5f;
            float mouseY = Input.mousePosition.y / Screen.height - 0.5f;
            
            Vector2 offset = new Vector2(-mouseX * parallaxAmount, -mouseY * parallaxAmount);
            transform.position = Vector2.Lerp(transform.position, initialPosition + offset, Time.deltaTime * parallaxSpeed);
        }
        
        // Update floating elements
        if (enableFloatingElements && floatingElements != null)
        {
            for (int i = 0; i < floatingElements.Length; i++)
            {
                if (floatingElements[i] != null)
                {
                    // Create floating motion using sine waves with different frequencies and offsets
                    float xOffset = Mathf.Sin(Time.time * floatingElementsSpeeds[i] + floatingElementsOffsets[i]) * (floatingElementsAmounts[i] * 0.5f);
                    float yOffset = Mathf.Cos(Time.time * floatingElementsSpeeds[i] * 0.6f + floatingElementsOffsets[i]) * floatingElementsAmounts[i];
                    
                    floatingElements[i].anchoredPosition = floatingElementsInitialPositions[i] + new Vector2(xOffset, yOffset);
                }
            }
        }
    }
    
    private void StartNextColorTransition()
    {
        if (colorTransitionCoroutine != null)
        {
            StopCoroutine(colorTransitionCoroutine);
        }
        
        int nextColorIndex = (currentColorIndex + 1) % backgroundColors.Length;
        colorTransitionCoroutine = StartCoroutine(TransitionToColor(backgroundColors[nextColorIndex]));
        currentColorIndex = nextColorIndex;
    }
    
    private IEnumerator TransitionToColor(Color targetColor)
    {
        Color startColor = backgroundImage.color;
        float elapsedTime = 0f;
        
        while (elapsedTime < colorTransitionDuration)
        {
            backgroundImage.color = Color.Lerp(startColor, targetColor, elapsedTime / colorTransitionDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        backgroundImage.color = targetColor;
        yield return new WaitForSeconds(colorTransitionDuration);
        
        // Start the next transition
        StartNextColorTransition();
    }
} 