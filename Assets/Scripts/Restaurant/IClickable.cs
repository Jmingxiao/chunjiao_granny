using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigitalRubyShared;
public class IClickable :  MonoBehaviour
{
    protected TapGestureRecognizer tapGesture;

    protected LongPressGestureRecognizer longPressGesture;

    protected virtual void Start()
    {
        CreateTapGesture();
        CreateLongPressGesture();
    }
    protected void CreateTapGesture()
    {
        tapGesture = new TapGestureRecognizer();
        tapGesture.StateUpdated += TapGestureCallback;
        FingersScript.Instance.AddGesture(tapGesture);
    }
    private void TapGestureCallback(GestureRecognizer gesture)
    {
        Vector3 pos = new Vector3(gesture.FocusX, gesture.FocusY, 0.0f);
        pos = Camera.main.ScreenToWorldPoint(pos);
        RaycastHit2D hit = Physics2D.CircleCast(pos, 10.0f, Vector2.zero);
        if(hit.collider!=null&& hit.collider.gameObject == gameObject)
        {
           OnClickDown();
        }
    }
    protected virtual void OnClickDown(){

    }

    protected void CreateLongPressGesture()
    {
        longPressGesture = new LongPressGestureRecognizer();
        longPressGesture.MaximumNumberOfTouchesToTrack = 1;
        longPressGesture.StateUpdated += LongPressGestureCallback;
        FingersScript.Instance.AddGesture(longPressGesture);
    }
    protected virtual void LongPressGestureCallback(GestureRecognizer gesture)
    {
        if (gesture.State == GestureRecognizerState.Began)
        {
            BeginDrag(gesture.FocusX, gesture.FocusY);
        }
        else if (gesture.State == GestureRecognizerState.Executing)
        {
            DragTo(gesture.FocusX, gesture.FocusY);
        }
        else if (gesture.State == GestureRecognizerState.Ended)
        {
            EndDrag(longPressGesture.VelocityX, longPressGesture.VelocityY);
        }
    }

    private void BeginDrag(float screenX, float screenY)
    {
        Vector3 pos = new Vector3(screenX, screenY, 0.0f);
        pos = Camera.main.ScreenToWorldPoint(pos);
        RaycastHit2D hit = Physics2D.CircleCast(pos, 10.0f, Vector2.zero);
        if (hit.collider != null && hit.collider.gameObject == gameObject)
        {
            OnBeginDrag(screenX, screenY);
        }
        else
        {
            longPressGesture.Reset();
        }
    }
    protected virtual void OnBeginDrag(float screenX, float screenY){ }

    protected virtual void DragTo(float screenX, float screenY){ }

    protected virtual void EndDrag(float velocityXScreen, float velocityYScreen){ }

}
