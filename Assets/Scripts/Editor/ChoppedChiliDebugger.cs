using UnityEngine;
using UnityEditor;

public class ChoppedChiliDebugger : MonoBehaviour
{
    [MenuItem("Tools/Debug Chopped Chili")]
    public static void DebugChoppedChili()
    {
        // 查找所有ChoppedChili对象
        GameObject[] allChoppedChilis = GameObject.FindObjectsOfType<GameObject>();
        int count = 0;
        
        foreach (GameObject obj in allChoppedChilis)
        {
            if (obj.name.Contains("ChoppedChili"))
            {
                count++;
                Debug.Log($"[调试] 找到ChoppedChili: {obj.name}");
                Debug.Log($"  - 位置: {obj.transform.position}");
                Debug.Log($"  - 激活状态: {obj.activeSelf}");
                Debug.Log($"  - 缩放: {obj.transform.localScale}");
                
                SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Debug.Log($"  - Sprite: {sr.sprite?.name ?? "NULL"}");
                    Debug.Log($"  - 颜色: {sr.color}");
                    Debug.Log($"  - Sorting Order: {sr.sortingOrder}");
                    Debug.Log($"  - Sorting Layer: {sr.sortingLayerName}");
                }
                else
                {
                    Debug.LogWarning($"  - 警告: 没有SpriteRenderer!");
                }
            }
        }
        
        Debug.Log($"总共找到 {count} 个ChoppedChili对象");
    }
    
    [MenuItem("Tools/Fix Chopped Chili Visibility")]
    public static void FixChoppedChiliVisibility()
    {
        GameObject choppedChili = GameObject.Find("ChoppedChili");
        if (choppedChili == null)
        {
            Debug.LogError("找不到ChoppedChili预制体!");
            return;
        }
        
        // 修复SpriteRenderer
        SpriteRenderer sr = choppedChili.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = 5; // 确保在前面
            sr.color = Color.white; // 确保不透明
            
            // 如果没有sprite，创建一个临时的
            if (sr.sprite == null)
            {
                Debug.Log("ChoppedChili没有sprite，创建临时sprite...");
                Texture2D tex = new Texture2D(64, 64);
                Color[] pixels = new Color[64 * 64];
                
                // 创建一个简单的切碎效果（多个小方块）
                for (int y = 0; y < 64; y++)
                {
                    for (int x = 0; x < 64; x++)
                    {
                        // 创建棋盘图案表示切碎
                        bool isBlock = ((x / 8) % 2 == 0) ^ ((y / 8) % 2 == 0);
                        pixels[y * 64 + x] = isBlock ? new Color(0.8f, 0.1f, 0.1f) : Color.clear;
                    }
                }
                
                tex.SetPixels(pixels);
                tex.Apply();
                
                sr.sprite = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 100);
            }
            
            Debug.Log("ChoppedChili预制体已修复");
        }
        
        // 标记为已修改
        EditorUtility.SetDirty(choppedChili);
    }
}