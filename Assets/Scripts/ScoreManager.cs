using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    private int current;
    private int best;

    const string KEY_BEST = "BestScore";

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        best = SaveManager.LoadInt(KEY_BEST, 0);
    }

    public void Add(int points)
    {
        current += points;
        if (current > best)
        {
            best = current;
            SaveManager.SaveInt(KEY_BEST, best);
        }
    }

    public void DoubleScore()
    {
        current *= 2;
        if (current > best)
        {
            best = current;
            SaveManager.SaveInt(KEY_BEST, best);
        }
        UIManager.Instance?.UpdateScore(current);
    }

    public void Reset() => current = 0;
    public int GetCurrentScore() => current;
    public int GetBestScore() => best;
}
