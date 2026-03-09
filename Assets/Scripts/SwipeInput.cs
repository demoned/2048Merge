using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 手机触摸滑动 + PC鼠标拖拽双模式
/// </summary>
public class SwipeInput : MonoBehaviour
{
    public float minSwipeDistance = 40f;

    private Vector2 startPos;
    private bool dragging;

    void Update()
    {
        if (GameManager.Instance?.CurrentState != GameManager.GameState.Playing) return;

        // 触摸
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began) { startPos = t.position; dragging = true; }
            if (t.phase == TouchPhase.Ended && dragging) { ProcessSwipe(t.position - startPos); dragging = false; }
        }

        // PC鼠标模拟
        if (Input.GetMouseButtonDown(0)) { startPos = Input.mousePosition; dragging = true; }
        if (Input.GetMouseButtonUp(0) && dragging)
        {
            ProcessSwipe((Vector2)Input.mousePosition - startPos);
            dragging = false;
        }
    }

    void ProcessSwipe(Vector2 delta)
    {
        if (delta.magnitude < minSwipeDistance) return;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            GameBoard.Instance?.TryMove(delta.x > 0 ? 1 : -1, 0);
        else
            GameBoard.Instance?.TryMove(0, delta.y > 0 ? 1 : -1);
    }
}
