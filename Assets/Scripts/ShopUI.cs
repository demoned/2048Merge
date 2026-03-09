using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 商城UI - 展示内购商品，绑定购买按钮
/// 在GameOverPanel底部或主菜单入口触发
/// </summary>
public class ShopUI : MonoBehaviour
{
    [Header("商城面板")]
    public GameObject shopPanel;

    [Header("商品按钮 - 价格文字自动从IAP读取")]
    public Button btnNoAds;
    public Button btnVipPack;
    public Button btnWeeklyVip;
    public Button btnCoinsSmall;
    public Button btnCoinsMedium;
    public Button btnCoinsLarge;
    public Button btnExtraTime;

    [Header("价格显示")]
    public TextMeshProUGUI priceNoAds;
    public TextMeshProUGUI priceVipPack;
    public TextMeshProUGUI priceWeeklyVip;
    public TextMeshProUGUI priceCoinsSmall;
    public TextMeshProUGUI priceCoinsMedium;
    public TextMeshProUGUI priceCoinsLarge;
    public TextMeshProUGUI priceExtraTime;

    void Start()
    {
        // 绑定购买按钮
        btnNoAds?.onClick.AddListener(()       => Buy(IAPManager.NO_ADS));
        btnVipPack?.onClick.AddListener(()     => Buy(IAPManager.VIP_PACK));
        btnWeeklyVip?.onClick.AddListener(()   => Buy(IAPManager.WEEKLY_VIP));
        btnCoinsSmall?.onClick.AddListener(()  => Buy(IAPManager.COINS_SMALL));
        btnCoinsMedium?.onClick.AddListener(() => Buy(IAPManager.COINS_MEDIUM));
        btnCoinsLarge?.onClick.AddListener(()  => Buy(IAPManager.COINS_LARGE));
        btnExtraTime?.onClick.AddListener(()   => Buy(IAPManager.EXTRA_TIME));

        shopPanel?.SetActive(false);
    }

    public void OpenShop()
    {
        shopPanel.SetActive(true);
        RefreshPrices();

        // 如果已买去广告，隐藏该按钮
        if (IAPManager.Instance?.IsNoAdsPurchased() == true)
            btnNoAds?.gameObject.SetActive(false);
    }

    public void CloseShop() => shopPanel?.SetActive(false);

    void RefreshPrices()
    {
        if (IAPManager.Instance == null) return;
        SetPrice(priceNoAds,       IAPManager.NO_ADS);
        SetPrice(priceVipPack,     IAPManager.VIP_PACK);
        SetPrice(priceWeeklyVip,   IAPManager.WEEKLY_VIP);
        SetPrice(priceCoinsSmall,  IAPManager.COINS_SMALL);
        SetPrice(priceCoinsMedium, IAPManager.COINS_MEDIUM);
        SetPrice(priceCoinsLarge,  IAPManager.COINS_LARGE);
        SetPrice(priceExtraTime,   IAPManager.EXTRA_TIME);
    }

    void SetPrice(TextMeshProUGUI txt, string productId)
    {
        if (txt == null) return;
        txt.text = IAPManager.Instance?.GetPrice(productId) ?? "--";
    }

    void Buy(string productId)
    {
        IAPManager.Instance?.BuyProduct(productId,
            onSuccess: () => {
                RefreshPrices();
                // 购买成功震动反馈
#if UNITY_ANDROID || UNITY_IOS
                Handheld.Vibrate();
#endif
            },
            onFail: () => UIManager.Instance?.ShowToast("购买失败，请重试")
        );
    }
}
