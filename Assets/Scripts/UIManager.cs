using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI总管理器 - 所有界面切换/数据显示
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("主界面")]
    public GameObject mainMenuPanel;
    public TextMeshProUGUI menuBestText;
    public Button playButton;

    [Header("游戏UI")]
    public GameObject gamePanel;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI bestText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI targetText;
    public TextMeshProUGUI levelText;

    [Header("胜利弹窗")]
    public GameObject winPanel;
    public TextMeshProUGUI winScoreText;
    public Button winDoubleBtn;    // 看广告双倍
    public Button winNextBtn;      // 下一关

    [Header("失败弹窗")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI overScoreText;
    public TextMeshProUGUI overBestText;
    public Button reviveBtn;       // 看广告复活
    public Button retryBtn;        // 重试

    [Header("计时器颜色")]
    public Color normalColor = Color.white;
    public Color urgentColor = Color.red;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 绑定按钮事件
        playButton?.onClick.AddListener(() => GameManager.Instance?.StartGame());
        winDoubleBtn?.onClick.AddListener(OnWinDouble);
        winNextBtn?.onClick.AddListener(() => GameManager.Instance?.StartGame());
        reviveBtn?.onClick.AddListener(OnRevive);
        retryBtn?.onClick.AddListener(() => GameManager.Instance?.RestartGame());

        ShowMainMenu();
    }

    // ─────────────────────────────────────────
    // 界面切换
    // ─────────────────────────────────────────

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        gamePanel.SetActive(false);
        winPanel.SetActive(false);
        gameOverPanel.SetActive(false);

        menuBestText.text = "最高分: " + ScoreManager.Instance?.GetBestScore();
    }

    public void ShowGameUI()
    {
        mainMenuPanel.SetActive(false);
        gamePanel.SetActive(true);
        winPanel.SetActive(false);
        gameOverPanel.SetActive(false);

        int lv = GameManager.Instance?.CurrentLevel ?? 0;
        levelText.text = "第 " + (lv + 1) + " 关";
        targetText.text = "目标: " + GameManager.LevelTargets[lv];
        UpdateScore(0);
        bestText.text = "最高: " + ScoreManager.Instance?.GetBestScore();
    }

    public void ShowWinPanel(int score)
    {
        winPanel.SetActive(true);
        winScoreText.text = "得分: " + score;
    }

    public void ShowGameOverPanel(int score)
    {
        gameOverPanel.SetActive(true);
        overScoreText.text = "得分: " + score;
        overBestText.text = "最高: " + ScoreManager.Instance?.GetBestScore();
    }

    // ─────────────────────────────────────────
    // 数据更新
    // ─────────────────────────────────────────

    public void UpdateScore(int score)
    {
        scoreText.text = "分数: " + score;
        bestText.text = "最高: " + ScoreManager.Instance?.GetBestScore();
    }

    public void UpdateTimer(float seconds)
    {
        int s = Mathf.CeilToInt(seconds);
        timerText.text = "⏱ " + s + "s";
        timerText.color = seconds < 10f ? urgentColor : normalColor;
    }

    // ─────────────────────────────────────────
    // 广告按钮回调
    // ─────────────────────────────────────────

    void OnWinDouble()
    {
        AdManager.Instance?.ShowRewardedAd("win_double", () =>
        {
            ScoreManager.Instance?.DoubleScore();
            winDoubleBtn.interactable = false;
            winDoubleBtn.GetComponentInChildren<TextMeshProUGUI>().text = "✅ 已双倍";
        });
    }

    void OnRevive()
    {
        AdManager.Instance?.ShowRewardedAd("revive", () =>
        {
            GameManager.Instance?.ReviveGame();
        });
    }
}
