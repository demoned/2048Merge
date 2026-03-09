using System;
using UnityEngine;

/// <summary>
/// 广告管理器 - IronSource LevelPlay SDK 9.x 适配版
/// 支持：激励视频 / 插页广告
/// </summary>
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
#if UNITY_ANDROID || UNITY_IOS
        InitSDK();
#endif
    }

    void InitSDK()
    {
        if (testMode)
            IronSource.Agent.setMetaData("is_test_suite", "enable");

        // LevelPlay 9.x 初始化方式
        IronSource.Agent.init(appKey);

        // 激励视频事件（LevelPlay 9.x）
        IronSourceRewardedVideoEvents.onAdRewardedEvent      += OnRewarded;
        IronSourceRewardedVideoEvents.onAdShowFailedEvent    += OnRewardFailed;

        // 插页广告事件
        IronSourceInterstitialEvents.onAdClosedEvent         += OnInterstitialClosed;
        IronSourceInterstitialEvents.onAdLoadFailedEvent     += OnInterstitialFailed;

        // 预加载插页广告
        IronSource.Agent.loadInterstitial();

        Debug.Log("[AdManager] LevelPlay 9.x 初始化完成 | testMode=" + testMode);
    }

    // ─────────────────────────────────────────
    // 公开接口
    // ─────────────────────────────────────────

    /// <summary>
    /// 显示激励视频
    /// placement: "revive" / "win_double"
    /// </summary>
    public void ShowRewardedAd(string placement, Action onSuccess = null)
    {
        pendingRewardCallback = onSuccess;

#if UNITY_ANDROID || UNITY_IOS
        if (IronSource.Agent.isRewardedVideoAvailable())
        {
            IronSource.Agent.showRewardedVideo(placement);
            return;
        }
#endif
        // 编辑器/广告不可用直接给奖励（测试用）
        Debug.LogWarning("[AdManager] 广告不可用，直接给奖励（测试模式）");
        onSuccess?.Invoke();
    }

    /// <summary>
    /// 尝试显示插页广告（每3局触发一次）
    /// </summary>
    public void TryShowInterstitial()
    {
        if (!AreAdsEnabled()) return;

        int count = SaveManager.LoadInt("PlayCount", 0) + 1;
        SaveManager.SaveInt("PlayCount", count);
        if (count % 3 != 0) return;

#if UNITY_ANDROID || UNITY_IOS
        if (IronSource.Agent.isInterstitialReady())
            IronSource.Agent.showInterstitial();
        else
            IronSource.Agent.loadInterstitial();
#endif
    }

    /// <summary>去广告购买后调用</summary>
    public void SetAdsEnabled(bool enabled)
    {
        SaveManager.SaveInt("AdsEnabled", enabled ? 1 : 0);
        Debug.Log("[AdManager] 广告: " + (enabled ? "开启" : "已关闭"));
    }

    public bool AreAdsEnabled()
    {
        if (SaveManager.LoadInt("NoAds", 0) == 1) return false;
        if (IAPManager.Instance?.IsVipActive() == true) return false;
        return true;
    }

    // ─────────────────────────────────────────
    // SDK 回调
    // ─────────────────────────────────────────

    void OnRewarded(IronSourcePlacement placement, IronSourceAdInfo info)
    {
        Debug.Log("[AdManager] 激励视频完成: " + placement.getPlacementName());
        pendingRewardCallback?.Invoke();
        pendingRewardCallback = null;
    }

    void OnRewardFailed(IronSourceAdInfo info, IronSourceError error)
    {
        Debug.LogWarning("[AdManager] 激励视频失败: " + error.getDescription());
        pendingRewardCallback = null;
    }

    void OnInterstitialClosed(IronSourceAdInfo info)
    {
        IronSource.Agent.loadInterstitial(); // 预加载下一个
    }

    void OnInterstitialFailed(IronSourceError error)
    {
        Debug.LogWarning("[AdManager] 插页广告加载失败: " + error.getDescription());
    }

    void OnApplicationPause(bool paused)
    {
#if UNITY_ANDROID || UNITY_IOS
        IronSource.Agent.onApplicationPause(paused);
#endif
    }
}
