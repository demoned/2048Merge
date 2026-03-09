using UnityEngine;

/// <summary>
/// 金币系统 - 广告和内购的通用奖励货币
/// </summary>
public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    const string KEY = "Coins";
    private int coins;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        coins = SaveManager.LoadInt(KEY, 0);
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        SaveManager.SaveInt(KEY, coins);
        UIManager.Instance?.UpdateCoins(coins);
        Debug.Log($"[Coins] +{amount} → 总计: {coins}");
    }

    public bool SpendCoins(int amount)
    {
        if (coins < amount) return false;
        coins -= amount;
        SaveManager.SaveInt(KEY, coins);
        UIManager.Instance?.UpdateCoins(coins);
        return true;
    }

    public int GetCoins() => coins;

    // 金币消费：使用道具（+30秒）
    public bool UseExtraTime()
    {
        int uses = SaveManager.LoadInt("ExtraTimeUses", 0);
        if (uses <= 0)
        {
            // 没有道具，可以花金币买
            if (!SpendCoins(20)) return false;
        }
        else
        {
            SaveManager.SaveInt("ExtraTimeUses", uses - 1);
        }
        GameBoard.Instance?.AddTime(30f);
        UIManager.Instance?.ShowToast("+30秒！");
        return true;
    }
}
