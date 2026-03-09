using System;
using UnityEngine;

// 导入Unity IAP包后，把下面这行注释取消
// #define IAP_ENABLED

#if IAP_ENABLED
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
#endif

/// <summary>
/// 内购管理器 - Unity IAP封装
/// 未安装IAP包时自动模拟（编辑器正常运行）
/// </summary>
public class IAPManager :
#if IAP_ENABLED
    MonoBehaviour, IDetailedStoreListener
#else
    MonoBehaviour
#endif
{
    public static IAPManager Instance;

    public const string COINS_SMALL  = "com.yourgame.coins_small";
    public const string COINS_MEDIUM = "com.yourgame.coins_medium";
    public const string COINS_LARGE  = "com.yourgame.coins_large";
    public const string EXTRA_TIME   = "com.yourgame.extra_time";
    public const string NO_ADS       = "com.yourgame.no_ads";
    public const string VIP_PACK     = "com.yourgame.vip_pack";
    public const string WEEKLY_VIP   = "com.yourgame.weekly_vip";

    private const int COINS_SMALL_AMOUNT  = 100;
    private const int COINS_MEDIUM_AMOUNT = 350;
    private const int COINS_LARGE_AMOUNT  = 900;

#if IAP_ENABLED
    private IStoreController storeController;
    private IExtensionProvider extensions;
    private bool isInitialized = false;
#endif

    private Action onPurchaseSuccess;
    private Action onPurchaseFailed;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
#if IAP_ENABLED
        InitIAP();
#else
        Debug.Log("[IAP] 模拟模式运行（Unity IAP未安装）");
#endif
    }

#if IAP_ENABLED
    void InitIAP()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(COINS_SMALL,  ProductType.Consumable);
        builder.AddProduct(COINS_MEDIUM, ProductType.Consumable);
        builder.AddProduct(COINS_LARGE,  ProductType.Consumable);
        builder.AddProduct(EXTRA_TIME,   ProductType.Consumable);
        builder.AddProduct(NO_ADS,       ProductType.NonConsumable);
        builder.AddProduct(VIP_PACK,     ProductType.NonConsumable);
        builder.AddProduct(WEEKLY_VIP,   ProductType.Subscription);
        UnityPurchasing.Initialize(this, builder);
    }
#endif

    // ─────────────────────────────────────────
    // 购买接口
    // ─────────────────────────────────────────

    public void BuyProduct(string productId, Action onSuccess = null, Action onFail = null)
    {
        onPurchaseSuccess = onSuccess;
        onPurchaseFailed  = onFail;

#if IAP_ENABLED
        if (!isInitialized) { onFail?.Invoke(); return; }
        var product = storeController.products.WithID(productId);
        if (product != null && product.availableToPurchase)
            storeController.InitiatePurchase(product);
        else
            onFail?.Invoke();
#else
        // 模拟模式：直接模拟购买成功（方便测试流程）
        Debug.Log("[IAP] 模拟购买: " + productId);
        ProcessReward(productId);
        onSuccess?.Invoke();
#endif
    }

    public void BuyNoAds()       => BuyProduct(NO_ADS);
    public void BuyVipPack()     => BuyProduct(VIP_PACK);
    public void BuyWeeklyVip()   => BuyProduct(WEEKLY_VIP);
    public void BuyCoinsSmall()  => BuyProduct(COINS_SMALL);
    public void BuyCoinsMedium() => BuyProduct(COINS_MEDIUM);
    public void BuyCoinsLarge()  => BuyProduct(COINS_LARGE);
    public void BuyExtraTime()   => BuyProduct(EXTRA_TIME);

    public void RestorePurchases()
    {
#if IAP_ENABLED && UNITY_IOS
        extensions?.GetExtension<IAppleExtensions>()
            .RestoreTransactions(r => Debug.Log("[IAP] 恢复: " + r));
#endif
    }

    public bool IsNoAdsPurchased() => SaveManager.LoadInt("NoAds", 0) == 1;
    public bool IsVipActive()      => SaveManager.LoadInt("WeeklyVIP", 0) == 1;

    public string GetPrice(string productId)
    {
#if IAP_ENABLED
        if (!isInitialized) return "--";
        return storeController.products.WithID(productId)?.metadata.localizedPriceString ?? "--";
#else
        // 模拟价格
        switch (productId)
        {
            case NO_ADS:       return "$1.99";
            case VIP_PACK:     return "$4.99";
            case WEEKLY_VIP:   return "$1.99/wk";
            case COINS_SMALL:  return "$0.99";
            case COINS_MEDIUM: return "$2.99";
            case COINS_LARGE:  return "$6.99";
            case EXTRA_TIME:   return "$0.99";
            default:           return "--";
        }
#endif
    }

    // ─────────────────────────────────────────
    // 奖励发放（有无SDK都走这里）
    // ─────────────────────────────────────────

    void ProcessReward(string id)
    {
        switch (id)
        {
            case COINS_SMALL:  CoinManager.Instance?.AddCoins(COINS_SMALL_AMOUNT); break;
            case COINS_MEDIUM: CoinManager.Instance?.AddCoins(COINS_MEDIUM_AMOUNT); break;
            case COINS_LARGE:  CoinManager.Instance?.AddCoins(COINS_LARGE_AMOUNT); break;
            case EXTRA_TIME:
                int uses = SaveManager.LoadInt("ExtraTimeUses", 0) + 3;
                SaveManager.SaveInt("ExtraTimeUses", uses);
                break;
            case NO_ADS:
                SaveManager.SaveInt("NoAds", 1);
                AdManager.Instance?.SetAdsEnabled(false);
                break;
            case VIP_PACK:
                SaveManager.SaveInt("NoAds", 1);
                AdManager.Instance?.SetAdsEnabled(false);
                CoinManager.Instance?.AddCoins(500);
                break;
            case WEEKLY_VIP:
                SaveManager.SaveInt("WeeklyVIP", 1);
                break;
        }
        UIManager.Instance?.ShowToast("购买成功！");
    }

#if IAP_ENABLED
    // ─────────────────────────────────────────
    // IDetailedStoreListener
    // ─────────────────────────────────────────

    public void OnInitialized(IStoreController ctrl, IExtensionProvider ext)
    {
        storeController = ctrl; extensions = ext; isInitialized = true;
        Debug.Log("[IAP] ✅ 初始化成功");
    }

    public void OnInitializeFailed(InitializationFailureReason r) =>
        Debug.LogWarning("[IAP] 初始化失败: " + r);

    public void OnInitializeFailed(InitializationFailureReason r, string msg) =>
        Debug.LogWarning($"[IAP] 初始化失败: {r} | {msg}");

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        ProcessReward(args.purchasedProduct.definition.id);
        onPurchaseSuccess?.Invoke();
        onPurchaseSuccess = null;
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product p, PurchaseFailureReason r)
    {
        Debug.LogWarning($"[IAP] 购买失败: {p.definition.id} | {r}");
        onPurchaseFailed?.Invoke(); onPurchaseFailed = null;
    }

    public void OnPurchaseFailed(Product p, PurchaseFailureDescription d)
    {
        Debug.LogWarning($"[IAP] 购买失败: {p.definition.id} | {d.message}");
        onPurchaseFailed?.Invoke(); onPurchaseFailed = null;
    }
#endif
}
