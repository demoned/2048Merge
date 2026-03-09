using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 核心棋盘逻辑 - 完整版
/// </summary>
public class GameBoard : MonoBehaviour
{
    public static GameBoard Instance;

    [Header("Board")]
    public int boardSize = 4;
    public RectTransform boardRoot;   // 棋盘容器(GridLayout挂载处)
    public GameObject tilePrefab;

    private int[,] board;
    private TileView[,] tileViews;
    private int targetValue;
    private float timeRemaining;
    private bool gameActive;
    private bool[,] merged;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ─────────────────────────────────────────
    // 公开接口
    // ─────────────────────────────────────────

    public void StartGame(int target, float timeLimit)
    {
        targetValue = target;
        timeRemaining = timeLimit;
        gameActive = true;

        InitBoard();
        SpawnRandom();
        SpawnRandom();
    }

    public void AddTime(float seconds)
    {
        timeRemaining += seconds;
        gameActive = true;
    }

    public float GetTimeRemaining() => timeRemaining;
    public bool IsActive() => gameActive;

    // ─────────────────────────────────────────
    // 初始化
    // ─────────────────────────────────────────

    void InitBoard()
    {
        // 清除旧tile
        foreach (Transform child in boardRoot)
            Destroy(child.gameObject);

        board = new int[boardSize, boardSize];
        tileViews = new TileView[boardSize, boardSize];
        merged = new bool[boardSize, boardSize];

        for (int r = 0; r < boardSize; r++)
            for (int c = 0; c < boardSize; c++)
            {
                var go = Instantiate(tilePrefab, boardRoot);
                tileViews[r, c] = go.GetComponent<TileView>();
                tileViews[r, c].SetValue(0);
            }
    }

    // ─────────────────────────────────────────
    // Update
    // ─────────────────────────────────────────

    void Update()
    {
        if (!gameActive) return;

        timeRemaining -= Time.deltaTime;
        UIManager.Instance?.UpdateTimer(Mathf.Max(0, timeRemaining));

        if (timeRemaining <= 0)
        {
            gameActive = false;
            GameManager.Instance?.OnGameOver();
            return;
        }

        HandleKeyboardInput();
    }

    void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))    TryMove(0, 1);
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))  TryMove(0, -1);
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))  TryMove(-1, 0);
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) TryMove(1, 0);
    }

    // ─────────────────────────────────────────
    // 移动逻辑
    // ─────────────────────────────────────────

    public void TryMove(int dx, int dy)
    {
        if (!gameActive) return;

        merged = new bool[boardSize, boardSize];
        bool moved = false;

        // 遍历顺序：朝移动方向从边缘往内遍历
        int[] rows = BuildOrder(boardSize, dy > 0);
        int[] cols = BuildOrder(boardSize, dx > 0);

        foreach (int r in rows)
            foreach (int c in cols)
            {
                if (board[r, c] == 0) continue;
                if (SlideCell(r, c, dx, dy)) moved = true;
            }

        if (moved)
        {
            SpawnRandom();
            UIManager.Instance?.UpdateScore(ScoreManager.Instance.GetCurrentScore());

            if (!CanMove())
            {
                gameActive = false;
                GameManager.Instance?.OnGameOver();
            }
        }
    }

    bool SlideCell(int r, int c, int dx, int dy)
    {
        int val = board[r, c];
        int nr = r + dy;
        int nc = c + dx;
        bool moved = false;

        while (InBounds(nr, nc))
        {
            if (board[nr, nc] == 0)
            {
                // 滑入空格
                board[nr, nc] = val;
                board[nr - dy, nc - dx] = 0;
                tileViews[nr, nc].SetValue(val);
                tileViews[nr - dy, nc - dx].SetValue(0);
                moved = true;
                nr += dy;
                nc += dx;
            }
            else if (board[nr, nc] == val && !merged[nr, nc])
            {
                // 合并！
                int newVal = val * 2;
                board[nr, nc] = newVal;
                board[nr - dy, nc - dx] = 0;
                merged[nr, nc] = true;

                tileViews[nr, nc].SetValue(newVal);
                tileViews[nr, nc].PlayMergeAnim();
                tileViews[nr - dy, nc - dx].SetValue(0);

                ScoreManager.Instance?.Add(newVal);

                if (newVal >= targetValue)
                {
                    gameActive = false;
                    StartCoroutine(DelayedWin());
                }
                return true;
            }
            else break;
        }
        return moved;
    }

    IEnumerator DelayedWin()
    {
        yield return new WaitForSeconds(0.4f);
        GameManager.Instance?.OnWin();
    }

    // ─────────────────────────────────────────
    // 生成随机数字
    // ─────────────────────────────────────────

    void SpawnRandom()
    {
        var empty = new List<Vector2Int>();
        for (int r = 0; r < boardSize; r++)
            for (int c = 0; c < boardSize; c++)
                if (board[r, c] == 0) empty.Add(new Vector2Int(r, c));

        if (empty.Count == 0) return;

        var pos = empty[Random.Range(0, empty.Count)];
        int val = Random.value < 0.9f ? 2 : 4;
        board[pos.x, pos.y] = val;
        tileViews[pos.x, pos.y].SetValue(val);
        tileViews[pos.x, pos.y].PlaySpawnAnim();
    }

    // ─────────────────────────────────────────
    // 工具
    // ─────────────────────────────────────────

    bool CanMove()
    {
        for (int r = 0; r < boardSize; r++)
            for (int c = 0; c < boardSize; c++)
            {
                if (board[r, c] == 0) return true;
                if (InBounds(r + 1, c) && board[r + 1, c] == board[r, c]) return true;
                if (InBounds(r, c + 1) && board[r, c + 1] == board[r, c]) return true;
            }
        return false;
    }

    bool InBounds(int r, int c) => r >= 0 && r < boardSize && c >= 0 && c < boardSize;

    int[] BuildOrder(int size, bool reverse)
    {
        int[] arr = new int[size];
        for (int i = 0; i < size; i++) arr[i] = reverse ? size - 1 - i : i;
        return arr;
    }
}
