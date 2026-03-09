using UnityEngine;
using IronSourceSDK; // IronSource SDK 命名空间

/// <summary>
/// 广告管理器 - IronSource集成
/// 变现点：
/// 1. 失败后复活（激励视频）
/// 2. 胜利双倍金币（激励视频）
/// 3. 每关结束（插页广告）
/// </summary>
public class AdManager : MonoBehaviour
{
    public static AdManager Instance;

    [Header("IronSource App Key")]
    [Tooltip("从IronSource后台获取App Key")]
    public string appKey = "YOUR_IRONSOURCE_APP_KEY";

    // 广告回调事件
    private System.Action onRewardEarned;
    private string currentAdPlacement;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitIronSource();
    }

    /// <summary>
    /// 初始化IronSource SDK
    /// </summary>
    void InitIronSource()
    {
        // 初始化SDK（必须在最前调用）
        IronSource.Agent.init(appKey, IronSourceAdUnits.REWARDED_VIDEO, IronSourceAdUnits.INTERSTITIAL);

        // 注册激励视频回调
        IronSourceRewardedVideoEvents.onAdRewardedEvent += OnRewardEarned;
        IronSourceRewardedVideoEvents.onAdShowFailedEvent += OnRewardShowFailed;

        // 注册插页广告回调
        IronSourceInterstitialEvents.onAdReadyEvent += OnInterstitialReady;
        IronSourceInterstitialEvents.onAdShowSucceededEvent += OnInterstitialShown;

        // 预加载插页广告
        IronSource.Agent.loadInterstitial();

        Debug.Log("[AdManager] IronSource初始化完成");
    }

    /// <summary>
    /// 显示激励视频广告
    /// placement: "revive" = 复活, "win_double" = 胜利双倍
    /// </summary>
    public void ShowRewardedAd(string placement, System.Action onSuccess = null)
    {
        currentAdPlacement = placement;
        onRewardEarned = onSuccess;

        if (IronSource.Agent.isRewardedVideoAvailable())
        {
            IronSource.Agent.showRewardedVideo(placement);
        }
        else
        {
            Debug.LogWarning("[AdManager] 激励视频暂不可用，跳过广告直接给奖励（测试模式）");
            // 开发测试期间直接给奖励
            onRewardEarned?.Invoke();
        }
    }

    /// <summary>
    /// 显示插页广告（关卡结束时调用）
    /// </summary>
    public void ShowInterstitialAd()
    {
        if (IronSource.Agent.isInterstitialReady())
        {
            IronSource.Agent.showInterstitial();
        }
        else
        {
            // 重新加载
            IronSource.Agent.loadInterstitial();
            Debug.Log("[AdManager] 插页广告未就绪，重新加载中");
        }
    }

    // ---- 广告回调 ----

    void OnRewardEarned(IronSourcePlacement placement, IronSourceAdInfo adInfo)
    {
        Debug.Log($"[AdManager] 激励视频奖励已获得: {placement.getPlacementName()}");

        if (currentAdPlacement == "revive")
        {
            // 触发复活逻辑
            FindObjectOfType<GameBoard>()?.StartGame();
        }
        else if (currentAdPlacement == "win_double")
        {
            // 触发双倍奖励
            ScoreManager.Instance?.DoubleScore();
        }

        onRewardEarned?.Invoke();
    }

    void OnRewardShowFailed(IronSourceAdInfo adInfo, IronSourceError error)
    {
        Debug.LogError($"[AdManager] 激励视频展示失败: {error.getDescription()}");
    }

    void OnInterstitialReady(IronSourceAdInfo adInfo)
    {
        Debug.Log("[AdManager] 插页广告已就绪");
    }

    void OnInterstitialShown(IronSourceAdInfo adInfo)
    {
        Debug.Log("[AdManager] 插页广告展示完成");
        // 预加载下一个
        IronSource.Agent.loadInterstitial();
    }

    void OnApplicationPause(bool isPaused)
    {
        IronSource.Agent.onApplicationPause(isPaused);
    }
}
