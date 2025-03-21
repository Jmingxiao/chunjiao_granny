using UnityEngine;
using System.Collections;

public class TitleLogoAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    public float pulseScale = 0.1f;
    public float pulseSpeed = 1f;
    public float rotationAmount = 5f;
    public float rotationSpeed = 0.5f;
    
    [Header("Color Settings")]
    public bool colorPulse = true;
    public Color baseColor = new Color(1f, 0.8f, 0.2f, 1f);
    public Color pulseColor = new Color(1f, 0.5f, 0f, 1f);
    
    private Vector3 startScale;
    private SpriteRenderer spriteRenderer;
    private RectTransform rectTransform;
    
    private void Start()
    {
        // Try to get either SpriteRenderer (for world space) or RectTransform (for UI)
        spriteRenderer = GetComponent<SpriteRenderer>();
        rectTransform = GetComponent<RectTransform>();
        
        if (rectTransform != null)
        {
            startScale = rectTransform.localScale;
        }
        else
        {
            startScale = transform.localScale;
        }
    }
    
    private void Update()
    {
        // Pulse scale
        float scaleFactor = 1f + pulseScale * Mathf.Sin(Time.time * pulseSpeed);
        Vector3 newScale = startScale * scaleFactor;
        
        // Apply scale
        if (rectTransform != null)
        {
            rectTransform.localScale = newScale;
        }
        else
        {
            transform.localScale = newScale;
        }
        
        // Gentle rotation
        float rotationZ = rotationAmount * Mathf.Sin(Time.time * rotationSpeed);
        transform.rotation = Quaternion.Euler(0, 0, rotationZ);
        
        // Color pulse
        if (colorPulse && spriteRenderer != null)
        {
            float colorLerp = (Mathf.Sin(Time.time * pulseSpeed * 0.5f) + 1f) * 0.5f;
            spriteRenderer.color = Color.Lerp(baseColor, pulseColor, colorLerp);
        }
    }
} 