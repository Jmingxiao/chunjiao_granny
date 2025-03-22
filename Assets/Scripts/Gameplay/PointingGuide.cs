using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class PointingGuide : MonoBehaviour
{
    private Camera cam;
    private Transform camtrans;
    private CircleCollider2D circleCollider;
    [Range(0, 5)]
    public float easingDuration = 3f;
    public SpriteRenderer cover;

    void Start()
    {
        circleCollider = GetComponent<CircleCollider2D>();
        camtrans = Camera.main.transform;
        cam = Camera.main;
    }

    public void Update()
    {
        if(Input.GetMouseButtonDown(0)){
            var mousePos = Input.mousePosition;
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);

            if( Vector2.Distance(mousePos,transform.position) < circleCollider.radius)
            {
                OnClick();
            }
        }
    }

    void OnClick()
    {
        var target= new Vector3(transform.position.x,transform.position.y, camtrans.position.z);
        var cammoveToTarget = DOTween.To(() => camtrans.position, x => camtrans.position = x, target, 3f).SetEase(Ease.Linear);
        var camScaling = DOTween.To(() => cam.orthographicSize, x => cam.orthographicSize = x, 2.0f, 3f).SetEase(Ease.Linear);
        var coverFade = cover.DOFade(1, 3f).SetEase(Ease.Linear).onComplete += () => SceneManager.LoadScene(1);
        cammoveToTarget.Restart();
        camScaling.Restart();
    }
}
