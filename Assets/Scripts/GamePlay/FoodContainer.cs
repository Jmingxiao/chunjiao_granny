using UnityEngine;
using DG.Tweening;

public class FoodContainer : ClickableObject
{
    private SpriteRenderer meshRenderer;
    Material material;
    int foodCount = 0;

    [Header("Food Container Settings")]
    [Tooltip("The target pot where food will be thrown.")]
    public Transform targetPot;
    public int piecesToThrow = 5;
    public float throwDuration = 0.5f;
    public float throwDelay = 0.05f;
    

    [Header("Click Animation Settings")]
    [Tooltip("The scale factor for the click animation.")]
    public float clickScaleFactor = 0.8f;
    public float clickDuration = 0.2f;
    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponent<SpriteRenderer>();
        if (meshRenderer)
            material = meshRenderer.material;
    }

    protected override void OnClick()
    {
        ClickAnimation();
        ThrowFood("meat");
        Debug.Log(gameObject.name + " clicked!");
    }

    private void ClickAnimation()
    {
        transform.DOKill(); // stop ongoing tweens on this transform
        transform.DOScale(clickScaleFactor, clickDuration)
            .SetEase(Ease.OutQuad)
            .SetLoops(2, LoopType.Yoyo);
    }
   
    public void ThrowFood(string foodTag)
    {
        for (int i = 0; i < piecesToThrow; i++)
        {
            float delay = i * throwDelay;
            StartCoroutine(ThrowSingleFoodWithDelay(foodTag, delay));
        }
    }

    private System.Collections.IEnumerator ThrowSingleFoodWithDelay(string foodTag, float delay=0f)
    {
        yield return new WaitForSeconds(delay);

        GameObject foodPiece = FoodContainerPool.Instance.GetObject(foodTag);
        if (foodPiece == null)
        {
            Debug.LogWarning("No food piece available in pool for tag: " + foodTag);
            yield break;
        }

        foodPiece.transform.position = transform.position;
        Vector3 start = transform.position;
        Vector3 end = targetPot.position;

        float elapsed = 0f;
        while (elapsed < throwDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / throwDuration;
            Vector3 pos = Vector3.Lerp(start, end, t);
            pos.y += Mathf.Sin(t * Mathf.PI) * 0.5f;
            foodPiece.transform.position = pos;
            yield return null;
        }

        FoodContainerPool.Instance.ReturnObject(foodTag, foodPiece);
    }
}
