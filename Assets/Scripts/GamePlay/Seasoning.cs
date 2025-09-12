using UnityEngine;
using DG.Tweening;
using System.Collections;

public class Seasoning : ClickableObject
{
    [Header("Click Animation Settings")]
    [Tooltip("The scale factor for the click animation.")]
    public float clickScaleFactor = 0.8f;
    public float clickDuration = 0.2f;
    private bool isAnimating = false;

    protected override void OnClick()
    {
        if (!isAnimating)
        {
            ClickAnimation();
            StartCoroutine(PerformSeasoning());
        }

    }
    // Update is called once per frame
    private void ClickAnimation()
    {
        transform.DOKill(); // stop ongoing tweens on this transform
        transform.DOScale(clickScaleFactor, clickDuration)
            .SetEase(Ease.OutQuad).OnComplete(() =>
        {
            transform.DOScale(1f, clickDuration).SetEase(Ease.OutQuad);
        });
    }
    private IEnumerator PerformSeasoning()
    {
        isAnimating = true;
        Debug.Log("Seasoning animation");
        yield return new WaitForSeconds(3.0f);
        isAnimating = false;
    }
}
