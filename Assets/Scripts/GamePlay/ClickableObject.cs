using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ClickableObject : MonoBehaviour
{// A flag to ensure OnClick is only called once per press.
    private bool isPressed = false;

    private void Update()
    {
#if UNITY_EDITOR
        HandleEditorInput();
#else
        HandleMobileInput();
#endif
    }

    /// <summary>
    /// Handles touch input for mobile devices.
    /// </summary>
    private void HandleMobileInput()
    {
        // Check if there is at least one touch on the screen.
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // Trigger OnClick only on the frame the touch begins.
            if (touch.phase == TouchPhase.Began)
            {
                CheckForHit(touch.position);
            }
        }
    }

    /// <summary>
    /// Handles mouse input for use within the Unity Editor.
    /// </summary>
    private void HandleEditorInput()
    {
        // Trigger OnClick only on the frame the left mouse button is pressed.
        if (Input.GetMouseButtonDown(0))
        {
            CheckForHit(Input.mousePosition);
        }
    }

    /// <summary>
    /// Casts a ray from the given screen position to check for a hit.
    /// </summary>
    /// <param name="screenPosition">The position on the screen to cast a ray from.</param>
    private void CheckForHit(Vector2 screenPosition)
    {
        // Convert the screen position to a world point.
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(screenPosition);
        Vector2 checkPosition = new Vector2(worldPoint.x, worldPoint.y);

        // Cast a ray from the position.
        RaycastHit2D hit = Physics2D.Raycast(checkPosition, Vector2.zero);

        // Check if the ray hit this object's collider.
        if (hit.collider != null && hit.collider.gameObject == gameObject)
        {
            OnClick();
        }
    }


    protected virtual void OnClick()
    {
    }

}