using System;
using UnityEngine;

/// <summary>
/// 广告管理器 - IronSource LevelPlay 9.x
/// ⚠️ 未导入SDK时自动进入模拟模式（编辑器可正常编译运行）
/// 导入SDK后修改顶部 #define 即可激活真实广告
/// </summary>

// 导入IronSource SDK后，把下面这行注释取消
// #define IRONSOURCE_ENABLED

public class AdManager : MonoBehaviour
{
    public static AdManager Instance;

    [Header("IronSource配置")]
    [Tooltip("从IronSource后台复制App Key填这里")]
    public string appKey = "YOUR_IRONSOURCE_APP_KEY";

    [Header("测试模式（上线前改为false）")]
    public bool testMode = true;

    private Action pendingRewardCallback;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
#if IRONSOURCE_ENABLED
        InitSDK();
#else
        Debug.Log("[AdManager] 模拟模式运行（SDK未导入）");
#endif
    }

#if IRONSOURCE_ENABLED
    void InitSDK()
    {
        if (testMode)
            IronSource.Agent.setMetaData("is_test_suite", "enable");

        IronSource.Agent.init(appKey);

        // 激励视频回调
        IronSourceRewardedVideoEvents.onAdRewardedEvent   += OnRewarded;
        IronSourceRewardedVideoEvents.onAdShowFailedEvent += OnRewardFailed;

        // 插页广告回调
        IronSourceInterstitialEvents.onAdClosedEvent      += OnInterstitialClosed;
        IronSourceInterstitialEvents.onAdLoadFailedEvent  += OnInterstitialLoadFailed;

        IronSource.Agent.loadInterstitial();
        Debug.Log("[AdManager] LevelPlay 9.x 初始化完成");
    }
#endif

    // ─────────────────────────────────────────
    // 公开接口（有无SDK都可调用）
    // ─────────────────────────────────────────

    public void ShowRewardedAd(string placement, Action onSuccess = null)
    {
        pendingRewardCallback = onSuccess;

#if IRONSOURCE_ENABLED
        if (IronSource.Agent.isRewardedVideoAvailable())
        {
            IronSource.Agent.showRewardedVideo(placement);
            return;
        }
#endif
        // 模拟模式/广告不可用：直接给奖励
        Debug.Log("[AdManager] 模拟奖励触发: " + placement);
        onSuccess?.Invoke();
        pendingRewardCallback = null;
    }

    public void TryShowInterstitial()
    {
        if (!AreAdsEnabled()) return;

        int count = SaveManager.LoadInt("PlayCount", 0) + 1;
        SaveManager.SaveInt("PlayCount", count);
        if (count % 3 != 0) return;

#if IRONSOURCE_ENABLED
        if (IronSource.Agent.isInterstitialReady())
            IronSource.Agent.showInterstitial();
        else
            IronSource.Agent.loadInterstitial();
#else
        Debug.Log("[AdManager] 模拟插页广告");
#endif
    }

    public void SetAdsEnabled(bool enabled)
    {
        SaveManager.SaveInt("AdsEnabled", enabled ? 1 : 0);
    }

    public bool AreAdsEnabled()
    {
        if (SaveManager.LoadInt("NoAds", 0) == 1) return false;
        return true;
    }

    // ─────────────────────────────────────────
    // SDK 回调（仅SDK存在时编译）
    // ─────────────────────────────────────────

#if IRONSOURCE_ENABLED
    void OnRewarded(IronSourcePlacement placement, IronSourceAdInfo info)
    {
        Debug.Log("[AdManager] 激励视频完成");
        pendingRewardCallback?.Invoke();
        pendingRewardCallback = null;
    }

    void OnRewardFailed(IronSourceAdInfo info, IronSourceError error)
    {
        Debug.LogWarning("[AdManager] 激励失败: " + error.getDescription());
        pendingRewardCallback = null;
    }

    void OnInterstitialClosed(IronSourceAdInfo info)
    {
        IronSource.Agent.loadInterstitial();
    }

    void OnInterstitialLoadFailed(IronSourceError error)
    {
        Debug.LogWarning("[AdManager] 插页加载失败: " + error.getDescription());
    }

    void OnApplicationPause(bool paused)
    {
        IronSource.Agent.onApplicationPause(paused);
    }
#endif
}
