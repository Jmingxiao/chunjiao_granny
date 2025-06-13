using UnityEngine;
using System.Collections;

public class CuttingBoard : MonoBehaviour
{
    public float chopTime = 2f;
    public Vector3 itemOffset = new Vector3(0, 0.5f, 0);
    public Vector3 barOffset = new Vector3(0, 1f, 0);
    
    private GameObject currentItem;
    private bool isProcessing;
    private GameObject progressBar;
    private bool returning;
    
    void Start()
    {
        var col = GetComponent<Collider2D>();
        if (!col) col = gameObject.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (returning && other.gameObject == currentItem)
        {
            returning = false;
            return;
        }
        
        if (currentItem || isProcessing) return;
        
        var ing = other.GetComponent<Ingredient>();
        if (!ing || ing.IsCooked() || !other.GetComponent<DraggableObject>()) return;
        
        AcceptItem(other.gameObject);
    }
    
    void AcceptItem(GameObject item)
    {
        currentItem = item;
        var rb = item.GetComponent<Rigidbody2D>();
        if (rb) rb.bodyType = RigidbodyType2D.Kinematic;
        
        item.transform.position = transform.position + itemOffset;
        
        var ing = item.GetComponent<Ingredient>();
        if (ing && ing.state == IngredientState.Raw)
            StartCoroutine(ProcessItem());
    }
    
    IEnumerator ProcessItem()
    {
        isProcessing = true;
        var ing = currentItem.GetComponent<Ingredient>();
        
        // Progress bar
        progressBar = new GameObject("Progress");
        progressBar.transform.position = transform.position + barOffset;
        var bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bg.transform.SetParent(progressBar.transform);
        bg.transform.localScale = new Vector3(1, 0.1f, 1);
        bg.GetComponent<Renderer>().material.color = Color.gray;
        
        var fill = GameObject.CreatePrimitive(PrimitiveType.Quad);
        fill.transform.SetParent(progressBar.transform);
        fill.transform.localScale = new Vector3(0, 0.08f, 1);
        fill.GetComponent<Renderer>().material.color = Color.green;
        
        // Chop
        float t = 0;
        while (t < chopTime)
        {
            t += Time.deltaTime;
            float p = t / chopTime;
            fill.transform.localScale = new Vector3(p, 0.08f, 1);
            fill.transform.localPosition = new Vector3((p - 1) * 0.5f, 0, -0.01f);
            yield return null;
        }
        
        Destroy(progressBar);
        
        // Replace
        if (ing.choppedPrefab)
        {
            var chopped = Instantiate(ing.choppedPrefab, currentItem.transform.position, Quaternion.identity);
            SetupChopped(chopped, ing);
            Destroy(currentItem);
            currentItem = chopped;
        }
        
        isProcessing = false;
    }
    
    void SetupChopped(GameObject item, Ingredient orig)
    {
        item.tag = "Ingredient";
        var ing = item.GetComponent<Ingredient>();
        if (!ing) ing = item.AddComponent<Ingredient>();
        ing.type = orig.type;
        ing.state = IngredientState.Chopped;
        ing.displayName = "Chopped " + orig.displayName;
        
        if (!item.GetComponent<DraggableObject>())
            item.AddComponent<DraggableObject>();
        
        var rb = item.GetComponent<Rigidbody2D>();
        if (!rb) rb = item.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (currentItem && other.gameObject == currentItem)
        {
            var rb = currentItem.GetComponent<Rigidbody2D>();
            if (rb) rb.bodyType = RigidbodyType2D.Dynamic;
            
            StartCoroutine(CheckPlate(currentItem));
            currentItem = null;
        }
    }
    
    IEnumerator CheckPlate(GameObject item)
    {
        yield return new WaitForSeconds(0.5f);
        if (!item) yield break;
        
        bool onPlate = false;
        var cols = Physics2D.OverlapCircleAll(item.transform.position, 0.5f);
        foreach (var col in cols)
        {
            if (col.CompareTag("Plate"))
            {
                onPlate = true;
                break;
            }
        }
        
        if (!onPlate && item.GetComponent<Ingredient>()?.state == IngredientState.Chopped)
        {
            ReturnToBoard(item);
        }
    }
    
    void ReturnToBoard(GameObject item)
    {
        if (!item || currentItem) return;
        
        returning = true;
        currentItem = item;
        StartCoroutine(AnimateReturn(item));
    }
    
    IEnumerator AnimateReturn(GameObject item)
    {
        var start = item.transform.position;
        var target = transform.position + itemOffset;
        var rb = item.GetComponent<Rigidbody2D>();
        if (rb) rb.bodyType = RigidbodyType2D.Kinematic;
        
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 2;
            if (item) item.transform.position = Vector3.Lerp(start, target, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
        
        returning = false;
    }
}