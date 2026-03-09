using UnityEngine;

/// <summary>
/// 手机触摸滑动输入处理
/// 支持：上下左右滑动 + 最小滑动距离过滤（避免误触）
/// </summary>
public class SwipeInput : MonoBehaviour
{
    [Header("Settings")]
    public float minSwipeDistance = 50f; // 最小滑动距离(px)
    public GameBoard gameBoard;

    private Vector2 touchStart;
    private bool isSwiping = false;

    void Update()
    {
        // PC测试用键盘（GameBoard已处理）
        // 移动端触摸滑动
        HandleTouchInput();
    }

    void HandleTouchInput()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                touchStart = touch.position;
                isSwiping = true;
                break;

            case TouchPhase.Ended:
                if (!isSwiping) return;
                isSwiping = false;

                Vector2 delta = touch.position - touchStart;

                // 过滤太短的滑动
                if (delta.magnitude < minSwipeDistance) return;

                // 判断主方向
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                {
                    // 横向滑动
                    gameBoard?.Move(delta.x > 0 ? Vector2Int.right : Vector2Int.left);
                }
                else
                {
                    // 纵向滑动（注意Unity Y轴方向）
                    gameBoard?.Move(delta.y > 0 ? Vector2Int.up : Vector2Int.down);
                }
                break;

            case TouchPhase.Canceled:
                isSwiping = false;
                break;
        }
    }
}
