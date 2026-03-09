using UnityEngine;
using TMPro;

/// <summary>
/// 分数管理器 - 记录历史最高分 + 双倍奖励
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("UI")]
    public TextMeshProUGUI bestScoreText;

    private int currentScore = 0;
    private int bestScore = 0;

    const string BEST_SCORE_KEY = "BestScore";

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        bestScore = PlayerPrefs.GetInt(BEST_SCORE_KEY, 0);
        UpdateBestScoreUI();
    }

    public void AddPoints(int points)
    {
        currentScore += points;
        if (currentScore > bestScore)
        {
            bestScore = currentScore;
            PlayerPrefs.SetInt(BEST_SCORE_KEY, bestScore);
            UpdateBestScoreUI();
        }
    }

    /// <summary>
    /// 看广告后双倍分数
    /// </summary>
    public void DoubleScore()
    {
        currentScore *= 2;
        Debug.Log($"[ScoreManager] 双倍奖励！当前分数: {currentScore}");
    }

    public int GetCurrentScore() => currentScore;
    public int GetBestScore() => bestScore;

    void UpdateBestScoreUI()
    {
        if (bestScoreText != null)
            bestScoreText.text = "最高: " + bestScore;
    }
}
