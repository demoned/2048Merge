using System;
using UnityEngine;

/// <summary>
/// 广告管理器 - IronSource封装
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
    private string pendingPlacement;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
#if UNITY_ANDROID || UNITY_IOS
        InitIronSource();
#endif
    }

    void InitIronSource()
    {
        if (testMode)
        {
            // 测试设备ID（从Logcat/Xcode日志获取）
            IronSource.Agent.setMetaData("is_test_suite", "enable");
        }

        IronSource.Agent.init(appKey,
            IronSourceAdUnits.REWARDED_VIDEO,
            IronSourceAdUnits.INTERSTITIAL);

        // 激励视频事件
        IronSourceRewardedVideoEvents.onAdRewardedEvent += OnRewarded;
        IronSourceRewardedVideoEvents.onAdShowFailedEvent += OnRewardFailed;

        // 插页广告事件
        IronSourceInterstitialEvents.onAdClosedEvent += OnInterstitialClosed;

        // 预加载插页广告
        IronSource.Agent.loadInterstitial();

        Debug.Log("[AdManager] IronSource初始化完成 | testMode=" + testMode);
    }

    /// <summary>
    /// 显示激励视频
    /// placement: "revive" / "win_double"
    /// onSuccess: 用户看完广告后回调
    /// </summary>
    public void ShowRewardedAd(string placement, Action onSuccess = null)
    {
        pendingPlacement = placement;
        pendingRewardCallback = onSuccess;

#if UNITY_ANDROID || UNITY_IOS
        if (IronSource.Agent.isRewardedVideoAvailable())
        {
            IronSource.Agent.showRewardedVideo(placement);
            return;
        }
#endif
        // 编辑器/广告不可用时直接给奖励（方便测试）
        Debug.LogWarning("[AdManager] 广告不可用，直接给奖励（测试模式）");
        onSuccess?.Invoke();
    }

    /// <summary>
    /// 显示插页广告（关卡结束时调用，每3关触发一次）
    /// </summary>
    public void TryShowInterstitial()
    {
        int playCount = SaveManager.LoadInt("PlayCount", 0) + 1;
        SaveManager.SaveInt("PlayCount", playCount);

        if (playCount % 3 != 0) return; // 每3局一次

#if UNITY_ANDROID || UNITY_IOS
        if (IronSource.Agent.isInterstitialReady())
            IronSource.Agent.showInterstitial();
        else
            IronSource.Agent.loadInterstitial();
#endif
    }

    // ─────────────────────────────────────────
    // 广告回调
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
        // 失败时不给奖励，让玩家重试
    }

    void OnInterstitialClosed(IronSourceAdInfo info)
    {
        // 插页关闭后预加载下一个
        IronSource.Agent.loadInterstitial();
    }

    void OnApplicationPause(bool paused)
    {
#if UNITY_ANDROID || UNITY_IOS
        IronSource.Agent.onApplicationPause(paused);
#endif
    }
}
