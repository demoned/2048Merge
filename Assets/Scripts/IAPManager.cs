using System;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

/// <summary>
/// 内购管理器 - Unity IAP封装
/// 商品设计（欧美中休最优变现组合）：
/// 
/// 消耗型（Consumable）：
///   coins_small  - $0.99  - 100金币（新手入门）
///   coins_medium - $2.99  - 350金币（最佳性价比，标"HOT"）
///   coins_large  - $6.99  - 900金币（大R首选）
///   extra_time   - $0.99  - +30秒×3次（功能型，高转化）
/// 
/// 非消耗型（NonConsumable）：
///   no_ads       - $1.99  - 永久去广告（转化率最高）
///   vip_pack     - $4.99  - VIP礼包（去广告+500金币+专属皮肤）
/// 
/// 订阅型（Subscription）：
///   weekly_vip   - $1.99/周 - 每周VIP（双倍金币+去广告）
/// </summary>
public class IAPManager : MonoBehaviour, IDetailedStoreListener
{
    public static IAPManager Instance;

    // 商品ID（需与Google Play Console / App Store Connect一致）
    public const string COINS_SMALL  = "com.yourgame.coins_small";
    public const string COINS_MEDIUM = "com.yourgame.coins_medium";
    public const string COINS_LARGE  = "com.yourgame.coins_large";
    public const string EXTRA_TIME   = "com.yourgame.extra_time";
    public const string NO_ADS       = "com.yourgame.no_ads";
    public const string VIP_PACK     = "com.yourgame.vip_pack";
    public const string WEEKLY_VIP   = "com.yourgame.weekly_vip";

    // 金币数量配置
    private const int COINS_SMALL_AMOUNT  = 100;
    private const int COINS_MEDIUM_AMOUNT = 350;
    private const int COINS_LARGE_AMOUNT  = 900;
    private const int EXTRA_TIME_USES     = 3;
    private const int VIP_PACK_COINS      = 500;

    private IStoreController storeController;
    private IExtensionProvider extensions;
    private bool isInitialized = false;

    // 购买回调
    private Action onPurchaseSuccess;
    private Action onPurchaseFailed;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
    }

    void Start() => InitIAP();

    // ─────────────────────────────────────────
    // 初始化
    // ─────────────────────────────────────────

    void InitIAP()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        // 消耗型商品
        builder.AddProduct(COINS_SMALL,  ProductType.Consumable);
        builder.AddProduct(COINS_MEDIUM, ProductType.Consumable);
        builder.AddProduct(COINS_LARGE,  ProductType.Consumable);
        builder.AddProduct(EXTRA_TIME,   ProductType.Consumable);

        // 非消耗型商品
        builder.AddProduct(NO_ADS,    ProductType.NonConsumable);
        builder.AddProduct(VIP_PACK,  ProductType.NonConsumable);

        // 订阅型商品
        builder.AddProduct(WEEKLY_VIP, ProductType.Subscription);

        UnityPurchasing.Initialize(this, builder);
        Debug.Log("[IAP] 初始化中...");
    }

    // ─────────────────────────────────────────
    // 购买接口
    // ─────────────────────────────────────────

    /// <summary>购买指定商品</summary>
    public void BuyProduct(string productId, Action onSuccess = null, Action onFail = null)
    {
        onPurchaseSuccess = onSuccess;
        onPurchaseFailed  = onFail;

        if (!isInitialized)
        {
            Debug.LogWarning("[IAP] 尚未初始化");
            onFail?.Invoke();
            return;
        }

        var product = storeController.products.WithID(productId);
        if (product != null && product.availableToPurchase)
        {
            Debug.Log($"[IAP] 购买: {productId} | 价格: {product.metadata.localizedPriceString}");
            storeController.InitiatePurchase(product);
        }
        else
        {
            Debug.LogWarning("[IAP] 商品不可购买: " + productId);
            onFail?.Invoke();
        }
    }

    /// <summary>快捷购买方法</summary>
    public void BuyNoAds()       => BuyProduct(NO_ADS);
    public void BuyVipPack()     => BuyProduct(VIP_PACK);
    public void BuyWeeklyVip()   => BuyProduct(WEEKLY_VIP);
    public void BuyCoinsSmall()  => BuyProduct(COINS_SMALL);
    public void BuyCoinsMedium() => BuyProduct(COINS_MEDIUM);
    public void BuyCoinsLarge()  => BuyProduct(COINS_LARGE);
    public void BuyExtraTime()   => BuyProduct(EXTRA_TIME);

    /// <summary>恢复购买（iOS必须实现）</summary>
    public void RestorePurchases()
    {
        if (!isInitialized) return;
#if UNITY_IOS
        var apple = extensions.GetExtension<IAppleExtensions>();
        apple.RestoreTransactions(result =>
        {
            Debug.Log("[IAP] 恢复购买: " + (result ? "成功" : "失败"));
        });
#endif
    }

    // ─────────────────────────────────────────
    // 状态查询
    // ─────────────────────────────────────────

    public bool IsNoAdsPurchased()
    {
        if (!isInitialized) return SaveManager.LoadInt("NoAds", 0) == 1;
        var p = storeController.products.WithID(NO_ADS);
        return p != null && p.hasReceipt;
    }

    public bool IsVipActive()
    {
        if (!isInitialized) return false;
        var p = storeController.products.WithID(WEEKLY_VIP);
        if (p == null || !p.hasReceipt) return false;
        var info = new SubscriptionManager(p, null);
        return info.getSubscriptionInfo().isSubscribed() == Result.True;
    }

    public string GetPrice(string productId)
    {
        if (!isInitialized) return "--";
        var p = storeController.products.WithID(productId);
        return p?.metadata.localizedPriceString ?? "--";
    }

    // ─────────────────────────────────────────
    // IDetailedStoreListener 实现
    // ─────────────────────────────────────────

    public void OnInitialized(IStoreController controller, IExtensionProvider ext)
    {
        storeController = controller;
        extensions = ext;
        isInitialized = true;
        Debug.Log("[IAP] ✅ 初始化成功，商品数: " + controller.products.all.Length);

        // 恢复非消耗型商品状态
        RestoreNonConsumables();
    }

    public void OnInitializeFailed(InitializationFailureReason reason)
    {
        Debug.LogWarning("[IAP] ❌ 初始化失败: " + reason);
    }

    public void OnInitializeFailed(InitializationFailureReason reason, string message)
    {
        Debug.LogWarning($"[IAP] ❌ 初始化失败: {reason} | {message}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        string id = args.purchasedProduct.definition.id;
        Debug.Log("[IAP] ✅ 购买成功: " + id);

        switch (id)
        {
            case COINS_SMALL:
                CoinManager.Instance?.AddCoins(COINS_SMALL_AMOUNT);
                ShowReward($"+{COINS_SMALL_AMOUNT} 金币！");
                break;

            case COINS_MEDIUM:
                CoinManager.Instance?.AddCoins(COINS_MEDIUM_AMOUNT);
                ShowReward($"+{COINS_MEDIUM_AMOUNT} 金币！");
                break;

            case COINS_LARGE:
                CoinManager.Instance?.AddCoins(COINS_LARGE_AMOUNT);
                ShowReward($"+{COINS_LARGE_AMOUNT} 金币！");
                break;

            case EXTRA_TIME:
                int uses = SaveManager.LoadInt("ExtraTimeUses", 0) + EXTRA_TIME_USES;
                SaveManager.SaveInt("ExtraTimeUses", uses);
                ShowReward($"获得 {EXTRA_TIME_USES} 次+30秒道具！");
                break;

            case NO_ADS:
                SaveManager.SaveInt("NoAds", 1);
                AdManager.Instance?.SetAdsEnabled(false);
                ShowReward("广告已永久关闭！感谢支持 ❤️");
                break;

            case VIP_PACK:
                SaveManager.SaveInt("NoAds", 1);
                AdManager.Instance?.SetAdsEnabled(false);
                CoinManager.Instance?.AddCoins(VIP_PACK_COINS);
                SaveManager.SaveInt("VIPSkin", 1);
                ShowReward($"VIP礼包已激活！去广告 + {VIP_PACK_COINS}金币 + 专属皮肤");
                break;

            case WEEKLY_VIP:
                SaveManager.SaveInt("WeeklyVIP", 1);
                ShowReward("周VIP已激活！每日双倍金币 + 去广告");
                break;
        }

        onPurchaseSuccess?.Invoke();
        onPurchaseSuccess = null;
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
    {
        Debug.LogWarning($"[IAP] ❌ 购买失败: {product.definition.id} | {reason}");
        onPurchaseFailed?.Invoke();
        onPurchaseFailed = null;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription desc)
    {
        Debug.LogWarning($"[IAP] ❌ 购买失败详情: {product.definition.id} | {desc.reason} | {desc.message}");
        onPurchaseFailed?.Invoke();
        onPurchaseFailed = null;
    }

    // ─────────────────────────────────────────
    // 辅助
    // ─────────────────────────────────────────

    void RestoreNonConsumables()
    {
        // 检查去广告
        var noAds = storeController.products.WithID(NO_ADS);
        if (noAds != null && noAds.hasReceipt)
        {
            SaveManager.SaveInt("NoAds", 1);
            AdManager.Instance?.SetAdsEnabled(false);
        }
    }

    void ShowReward(string msg)
    {
        UIManager.Instance?.ShowToast(msg);
        Debug.Log("[IAP] 奖励发放: " + msg);
    }
}
