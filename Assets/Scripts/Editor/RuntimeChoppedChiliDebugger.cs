using UnityEngine;
using UnityEditor;

public class RuntimeChoppedChiliDebugger : EditorWindow
{
    [MenuItem("Tools/Debug Runtime ChoppedChili")]
    public static void ShowWindow()
    {
        GetWindow<RuntimeChoppedChiliDebugger>("ChoppedChili Debugger");
    }
    
    void OnGUI()
    {
        if (GUILayout.Button("检查ChoppedChili(Clone)"))
        {
            DebugChoppedChiliClone();
        }
        
        if (GUILayout.Button("修复ChoppedChili(Clone)可见性"))
        {
            FixChoppedChiliClone();
        }
    }
    
    static void DebugChoppedChiliClone()
    {
        GameObject clone = GameObject.Find("ChoppedChili(Clone)");
        if (clone == null)
        {
            Debug.LogError("找不到ChoppedChili(Clone)!");
            return;
        }
        
        Debug.Log("=== ChoppedChili(Clone) 状态 ===");
        Debug.Log($"位置: {clone.transform.position}");
        Debug.Log($"缩放: {clone.transform.localScale}");
        Debug.Log($"激活: {clone.activeSelf}");
        
        SpriteRenderer sr = clone.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Debug.Log($"--- SpriteRenderer ---");
            Debug.Log($"Sprite: {(sr.sprite != null ? sr.sprite.name : "NULL")}");
            Debug.Log($"颜色: {sr.color} (Alpha: {sr.color.a})");
            Debug.Log($"Sorting Order: {sr.sortingOrder}");
            Debug.Log($"Sorting Layer: {sr.sortingLayerName}");
            Debug.Log($"启用状态: {sr.enabled}");
            
            if (sr.sprite != null)
            {
                Debug.Log($"Sprite大小: {sr.sprite.bounds.size}");
                Debug.Log($"Pixels Per Unit: {sr.sprite.pixelsPerUnit}");
            }
        }
        else
        {
            Debug.LogError("没有SpriteRenderer组件!");
        }
        
        // 检查原始预制体
        GameObject original = GameObject.Find("ChoppedChili");
        if (original != null)
        {
            Debug.Log($"原始ChoppedChili位置: {original.transform.position}");
            SpriteRenderer originalSr = original.GetComponent<SpriteRenderer>();
            if (originalSr != null && originalSr.sprite != null)
            {
                Debug.Log($"原始Sprite: {originalSr.sprite.name}");
            }
        }
    }
    
    static void FixChoppedChiliClone()
    {
        GameObject clone = GameObject.Find("ChoppedChili(Clone)");
        if (clone == null) return;
        
        SpriteRenderer sr = clone.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = clone.AddComponent<SpriteRenderer>();
        }
        
        // 修复渲染设置
        sr.sortingOrder = 100; // 确保在最前面
        sr.color = Color.white;
        sr.enabled = true;
        
        // 如果没有sprite，从原始辣椒复制或创建新的
        if (sr.sprite == null)
        {
            // 尝试从原始辣椒获取sprite
            GameObject chili = GameObject.Find("辣椒_0");
            if (chili == null) chili = GameObject.Find("辣椒_1");
            if (chili == null) chili = GameObject.Find("Chili");
            
            if (chili != null)
            {
                SpriteRenderer chiliSr = chili.GetComponent<SpriteRenderer>();
                if (chiliSr != null && chiliSr.sprite != null)
                {
                    sr.sprite = chiliSr.sprite;
                    sr.color = new Color(0.6f, 0.1f, 0.1f); // 深红色
                    Debug.Log("使用了原始辣椒的sprite");
                }
            }
            
            if (sr.sprite == null)
            {
                // 创建临时sprite
                Texture2D tex = new Texture2D(64, 64);
                Color[] pixels = new Color[64 * 64];
                Color choppedColor = new Color(0.8f, 0.1f, 0.1f);
                
                // 创建切碎图案
                for (int y = 0; y < 64; y++)
                {
                    for (int x = 0; x < 64; x++)
                    {
                        bool isPiece = ((x / 8 + y / 8) % 2 == 0);
                        pixels[y * 64 + x] = isPiece ? choppedColor : Color.clear;
                    }
                }
                
                tex.SetPixels(pixels);
                tex.Apply();
                
                sr.sprite = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 100);
                Debug.Log("创建了新的切碎sprite");
            }
        }
        
        // 确保位置和缩放合理
        if (clone.transform.localScale.x < 0.1f)
        {
            clone.transform.localScale = Vector3.one * 0.5f;
        }
        
        Debug.Log($"修复完成! 位置: {clone.transform.position}, Sorting Order: {sr.sortingOrder}");
    }
}