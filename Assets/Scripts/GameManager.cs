using UnityEngine;

/// <summary>
/// 游戏状态管理器 - 全局单例
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState { Menu, Playing, Paused, Win, GameOver }
    public GameState CurrentState { get; private set; }

    // 关卡目标配置
    public static readonly int[] LevelTargets = { 256, 512, 1024, 2048, 4096 };
    public static readonly float[] LevelTimeLimits = { 120f, 90f, 75f, 60f, 45f };

    private int currentLevel = 0;
    public int CurrentLevel => currentLevel;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);

        currentLevel = SaveManager.LoadInt("CurrentLevel", 0);
    }

    public void StartGame()
    {
        CurrentState = GameState.Playing;
        UIManager.Instance?.ShowGameUI();
        GameBoard.Instance?.StartGame(
            LevelTargets[currentLevel],
            LevelTimeLimits[currentLevel]
        );
    }

    public void OnWin()
    {
        CurrentState = GameState.Win;
        currentLevel = Mathf.Min(currentLevel + 1, LevelTargets.Length - 1);
        SaveManager.SaveInt("CurrentLevel", currentLevel);
        UIManager.Instance?.ShowWinPanel(ScoreManager.Instance.GetCurrentScore());
        AdManager.Instance?.ShowRewardedAd("win_double");
    }

    public void OnGameOver()
    {
        CurrentState = GameState.GameOver;
        UIManager.Instance?.ShowGameOverPanel(ScoreManager.Instance.GetCurrentScore());
    }

    public void RestartGame()
    {
        ScoreManager.Instance?.Reset();
        StartGame();
    }

    public void ReviveGame()
    {
        // 看广告后复活：时间+15秒，继续游戏
        CurrentState = GameState.Playing;
        GameBoard.Instance?.AddTime(15f);
        UIManager.Instance?.ShowGameUI();
    }

    public void GoToMenu()
    {
        CurrentState = GameState.Menu;
        UIManager.Instance?.ShowMainMenu();
    }
}
