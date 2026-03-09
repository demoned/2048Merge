using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 核心棋盘逻辑 - 2048 Merge + 关卡挑战
/// </summary>
public class GameBoard : MonoBehaviour
{
    [Header("Board Settings")]
    public int boardSize = 4; // 4x4 棋盘
    public GameObject tilePrefab;
    public Transform boardParent;

    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI targetText;
    public GameObject gameOverPanel;
    public GameObject winPanel;

    [Header("Level Settings")]
    public int targetValue = 2048; // 目标数字
    public float timeLimit = 60f;  // 限时60秒

    // 内部状态
    private int[,] board;
    private GameObject[,] tileObjects;
    private int score = 0;
    private float timeRemaining;
    private bool gameActive = false;

    // 颜色映射（欧美流行的明亮色系）
    private Dictionary<int, Color> tileColors = new Dictionary<int, Color>()
    {
        { 2,    new Color(0.93f, 0.89f, 0.85f) },
        { 4,    new Color(0.93f, 0.88f, 0.78f) },
        { 8,    new Color(0.95f, 0.69f, 0.47f) },
        { 16,   new Color(0.96f, 0.58f, 0.39f) },
        { 32,   new Color(0.96f, 0.49f, 0.37f) },
        { 64,   new Color(0.96f, 0.37f, 0.23f) },
        { 128,  new Color(0.93f, 0.81f, 0.45f) },
        { 256,  new Color(0.93f, 0.80f, 0.38f) },
        { 512,  new Color(0.93f, 0.78f, 0.31f) },
        { 1024, new Color(0.93f, 0.77f, 0.25f) },
        { 2048, new Color(0.93f, 0.76f, 0.18f) },
    };

    void Start()
    {
        InitBoard();
        StartGame();
    }

    void Update()
    {
        if (!gameActive) return;

        // 倒计时
        timeRemaining -= Time.deltaTime;
        UpdateTimerUI();

        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            GameOver(false);
        }

        // 处理滑动输入
        HandleInput();
    }

    /// <summary>
    /// 初始化棋盘
    /// </summary>
    void InitBoard()
    {
        board = new int[boardSize, boardSize];
        tileObjects = new GameObject[boardSize, boardSize];

        // 创建瓦片对象
        for (int r = 0; r < boardSize; r++)
        {
            for (int c = 0; c < boardSize; c++)
            {
                GameObject tile = Instantiate(tilePrefab, boardParent);
                tileObjects[r, c] = tile;
                UpdateTileVisual(r, c, 0);
            }
        }
    }

    /// <summary>
    /// 开始游戏
    /// </summary>
    public void StartGame()
    {
        score = 0;
        timeRemaining = timeLimit;
        gameActive = true;

        // 清空棋盘
        for (int r = 0; r < boardSize; r++)
            for (int c = 0; c < boardSize; c++)
                board[r, c] = 0;

        // 随机放置2个初始数字
        SpawnRandom();
        SpawnRandom();

        UpdateScoreUI();
        UpdateTimerUI();
        targetText.text = "目标: " + targetValue;

        gameOverPanel.SetActive(false);
        winPanel.SetActive(false);
    }

    /// <summary>
    /// 随机在空位生成2或4
    /// </summary>
    void SpawnRandom()
    {
        List<Vector2Int> emptyTiles = new List<Vector2Int>();
        for (int r = 0; r < boardSize; r++)
            for (int c = 0; c < boardSize; c++)
                if (board[r, c] == 0)
                    emptyTiles.Add(new Vector2Int(r, c));

        if (emptyTiles.Count == 0) return;

        Vector2Int pos = emptyTiles[Random.Range(0, emptyTiles.Count)];
        board[pos.x, pos.y] = Random.value < 0.9f ? 2 : 4;
        UpdateTileVisual(pos.x, pos.y, board[pos.x, pos.y]);

        // 生成动画
        StartCoroutine(SpawnAnimation(tileObjects[pos.x, pos.y]));
    }

    /// <summary>
    /// 处理滑动输入
    /// </summary>
    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            Move(Vector2Int.up);
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            Move(Vector2Int.down);
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            Move(Vector2Int.left);
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            Move(Vector2Int.right);
    }

    /// <summary>
    /// 移动逻辑（支持上下左右）
    /// </summary>
    public void Move(Vector2Int direction)
    {
        if (!gameActive) return;

        bool moved = false;

        // 遍历顺序根据方向决定
        int[] rowOrder = GetOrder(direction.x, boardSize);
        int[] colOrder = GetOrder(direction.y, boardSize);
        bool[,] merged = new bool[boardSize, boardSize];

        foreach (int r in rowOrder)
        {
            foreach (int c in colOrder)
            {
                if (board[r, c] == 0) continue;

                int nr = r + direction.x;
                int nc = c + direction.y;

                while (IsInBounds(nr, nc))
                {
                    if (board[nr, nc] == 0)
                    {
                        // 移动到空格
                        board[nr, nc] = board[nr - direction.x, nc - direction.y];
                        board[nr - direction.x, nc - direction.y] = 0;
                        UpdateTileVisual(nr, nc, board[nr, nc]);
                        UpdateTileVisual(nr - direction.x, nc - direction.y, 0);
                        moved = true;
                        nr += direction.x;
                        nc += direction.y;
                    }
                    else if (board[nr, nc] == board[nr - direction.x, nc - direction.y] && !merged[nr, nc])
                    {
                        // 合并！
                        board[nr, nc] *= 2;
                        score += board[nr, nc];
                        board[nr - direction.x, nc - direction.y] = 0;
                        merged[nr, nc] = true;
                        UpdateTileVisual(nr, nc, board[nr, nc]);
                        UpdateTileVisual(nr - direction.x, nc - direction.y, 0);
                        moved = true;

                        // 连击特效
                        StartCoroutine(MergeAnimation(tileObjects[nr, nc]));

                        // 检查胜利
                        if (board[nr, nc] >= targetValue)
                        {
                            GameOver(true);
                            return;
                        }
                        break;
                    }
                    else break;
                }
            }
        }

        if (moved)
        {
            SpawnRandom();
            UpdateScoreUI();

            // 检查是否还能移动
            if (!CanMove())
                GameOver(false);
        }
    }

    /// <summary>
    /// 检查是否还有合法移动
    /// </summary>
    bool CanMove()
    {
        for (int r = 0; r < boardSize; r++)
            for (int c = 0; c < boardSize; c++)
            {
                if (board[r, c] == 0) return true;
                if (r + 1 < boardSize && board[r, c] == board[r + 1, c]) return true;
                if (c + 1 < boardSize && board[r, c] == board[r, c + 1]) return true;
            }
        return false;
    }

    /// <summary>
    /// 游戏结束（win=true胜利，false失败）
    /// </summary>
    void GameOver(bool win)
    {
        gameActive = false;
        if (win)
        {
            winPanel.SetActive(true);
            // TODO: 触发广告激励（胜利后双倍金币）
            AdManager.Instance?.ShowRewardedAd("win_double");
        }
        else
        {
            gameOverPanel.SetActive(true);
            // TODO: 触发广告（失败后看广告复活）
            AdManager.Instance?.ShowRewardedAd("revive");
        }
    }

    // ---- 辅助方法 ----

    bool IsInBounds(int r, int c) => r >= 0 && r < boardSize && c >= 0 && c < boardSize;

    int[] GetOrder(int dir, int size)
    {
        int[] order = new int[size];
        for (int i = 0; i < size; i++)
            order[i] = dir > 0 ? size - 1 - i : i;
        return order;
    }

    void UpdateTileVisual(int r, int c, int value)
    {
        GameObject tile = tileObjects[r, c];
        Image bg = tile.GetComponent<Image>();
        TextMeshProUGUI text = tile.GetComponentInChildren<TextMeshProUGUI>();

        if (value == 0)
        {
            bg.color = new Color(0.80f, 0.75f, 0.71f);
            text.text = "";
        }
        else
        {
            bg.color = tileColors.ContainsKey(value) ? tileColors[value] : new Color(0.2f, 0.2f, 0.2f);
            text.text = value.ToString();
            text.color = value <= 4 ? new Color(0.47f, 0.43f, 0.40f) : Color.white;
        }
    }

    void UpdateScoreUI() => scoreText.text = "分数: " + score;

    void UpdateTimerUI()
    {
        int seconds = Mathf.CeilToInt(timeRemaining);
        timerText.text = "⏱ " + seconds + "s";
        timerText.color = timeRemaining < 10 ? Color.red : Color.white;
    }

    // ---- 动画协程 ----

    IEnumerator SpawnAnimation(GameObject tile)
    {
        tile.transform.localScale = Vector3.zero;
        float t = 0;
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            tile.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t / 0.15f);
            yield return null;
        }
        tile.transform.localScale = Vector3.one;
    }

    IEnumerator MergeAnimation(GameObject tile)
    {
        float t = 0;
        while (t < 0.1f)
        {
            t += Time.deltaTime;
            float scale = 1 + Mathf.Sin(t / 0.1f * Mathf.PI) * 0.2f;
            tile.transform.localScale = Vector3.one * scale;
            yield return null;
        }
        tile.transform.localScale = Vector3.one;
    }
}
