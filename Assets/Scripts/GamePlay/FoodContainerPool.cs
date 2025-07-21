using System.Collections.Generic;
using UnityEngine;

public class FoodContainerPool : MonoBehaviour
{
    public static FoodContainerPool Instance;
    
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
        public bool expandable = true; // 是否可扩展
    }
    
    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;
    private Dictionary<string, Pool> poolConfig;
    
    void Awake()
    {
        Instance = this;
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        poolConfig = new Dictionary<string, Pool>();
        
        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            
            // 验证预制体
            if (pool.prefab == null)
            {
                Debug.LogError($"池 {pool.tag} 的预制体为空！");
                continue;
            }
            
            // 验证预制体有Ingredient组件
            if (pool.prefab.GetComponent<Ingredient>() == null)
            {
                Debug.LogWarning($"池 {pool.tag} 的预制体缺少Ingredient组件！");
            }
            
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = CreatePoolObject(pool);
                objectPool.Enqueue(obj);
            }
            
            poolDictionary.Add(pool.tag, objectPool);
            poolConfig.Add(pool.tag, pool);
        }
    }
    
    private GameObject CreatePoolObject(Pool pool)
    {
        GameObject obj = Instantiate(pool.prefab);
        obj.SetActive(false);
        obj.transform.SetParent(transform); // 组织层级
        return obj;
    }
    
    public GameObject GetObject(string tag)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"不存在标签为 {tag} 的对象池");
            return null;
        }
        
        Queue<GameObject> pool = poolDictionary[tag];
        
        // 如果池为空且可扩展，创建新对象
        if (pool.Count == 0 && poolConfig[tag].expandable)
        {
            GameObject newObj = CreatePoolObject(poolConfig[tag]);
            pool.Enqueue(newObj);
        }
        
        if (pool.Count == 0)
        {
            Debug.LogWarning($"对象池 {tag} 已空！");
            return null;
        }
        
        GameObject obj = pool.Dequeue();
        obj.SetActive(true);
        obj.transform.SetParent(null); // 从池中移出
        return obj;
    }
    
    public void ReturnObject(string tag, GameObject obj)
    {
        if (obj == null) return;
        
        obj.SetActive(false);
        obj.transform.SetParent(transform); // 放回池中
        
        if (poolDictionary.ContainsKey(tag))
        {
            poolDictionary[tag].Enqueue(obj);
        }
        else
        {
            Destroy(obj); // 如果池不存在，销毁对象
        }
    }
    
    public bool IsObjectFromPool(GameObject obj)
    {
        return obj.transform.parent == transform;
    }
}
