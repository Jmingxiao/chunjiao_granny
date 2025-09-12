using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HintNPC : MonoBehaviour
{
    [SerializeField] private string[] hints;
    [SerializeField] private GameObject hintBubble;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private float displayTime = 4f;
    [SerializeField] private AudioClip hintSound;
    [SerializeField] private AudioSource audioSource;

    private Coroutine hideCoroutine;

    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // If hint bubble is not assigned, create one
        if (hintBubble == null)
        {
            CreateHintBubble();
        }

        // Hide hint bubble initially
        hintBubble.SetActive(false);
    }

    private void OnMouseDown()
    {
        ShowRandomHint();
    }

    private void ShowRandomHint()
    {
        if (hints.Length == 0) return;

        // Get random hint
        string randomHint = hints[Random.Range(0, hints.Length)];
        
        // Update UI
        hintText.text = randomHint;
        
        // Show hint bubble
        hintBubble.SetActive(true);
        
        // Play sound if available
        if (hintSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hintSound);
        }
        
        // Auto-hide after delay
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }
        hideCoroutine = StartCoroutine(HideHintAfterDelay());
    }

    private IEnumerator HideHintAfterDelay()
    {
        yield return new WaitForSeconds(displayTime);
        hintBubble.SetActive(false);
        hideCoroutine = null;
    }

    private void CreateHintBubble()
    {
        // Create hint bubble game object
        hintBubble = new GameObject("HintBubble");
        hintBubble.transform.SetParent(transform);
        hintBubble.transform.localPosition = new Vector3(0, 1.5f, 0);
        
        // Add canvas
        Canvas canvas = hintBubble.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 10;
        
        CanvasScaler scaler = hintBubble.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 100;
        
        // Add background image
        GameObject background = new GameObject("Background");
        background.transform.SetParent(hintBubble.transform);
        background.transform.localPosition = Vector3.zero;
        
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(1, 1, 1, 0.8f);
        
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.sizeDelta = new Vector2(3, 1.5f);
        bgRect.localScale = Vector3.one;
        
        // Add text
        GameObject textObj = new GameObject("HintText");
        textObj.transform.SetParent(background.transform);
        textObj.transform.localPosition = Vector3.zero;
        
        hintText = textObj.AddComponent<TextMeshProUGUI>();
        hintText.alignment = TextAlignmentOptions.Center;
        hintText.fontSize = 0.3f;
        hintText.color = Color.black;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(2.8f, 1.4f);
        textRect.localScale = Vector3.one;
    }
} 