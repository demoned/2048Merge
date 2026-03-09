/// <summary>
/// 内购商品配置表 - 上线前在Google Play Console / App Store Connect创建对应商品ID
/// 
/// ┌─────────────────────────────────────────────────────────────────┐
/// │  商品ID                          类型       定价    内容         │
/// ├─────────────────────────────────────────────────────────────────┤
/// │  com.yourgame.no_ads             非消耗     $1.99  永久去广告    │
/// │  com.yourgame.vip_pack           非消耗     $4.99  去广告+礼包   │
/// │  com.yourgame.weekly_vip         订阅       $1.99/周 每周VIP     │
/// │  com.yourgame.coins_small        消耗       $0.99  100金币       │
/// │  com.yourgame.coins_medium       消耗       $2.99  350金币(HOT)  │
/// │  com.yourgame.coins_large        消耗       $6.99  900金币       │
/// │  com.yourgame.extra_time         消耗       $0.99  +30秒×3次    │
/// └─────────────────────────────────────────────────────────────────┘
/// 
/// 上线前替换：
/// 1. 把所有 "com.yourgame" 改为你的Bundle ID前缀
///    例如：com.demoned.merge2048
/// 2. 在Google Play Console → 商品 → 创建上述商品
/// 3. 在App Store Connect → 内购 → 创建上述商品
/// </summary>
public static class IAPConfig
{
    // ⚠️ 修改这里的包名前缀！
    private const string BundlePrefix = "com.yourgame";

    public static readonly string NoAds       = BundlePrefix + ".no_ads";
    public static readonly string VipPack     = BundlePrefix + ".vip_pack";
    public static readonly string WeeklyVip   = BundlePrefix + ".weekly_vip";
    public static readonly string CoinsSmall  = BundlePrefix + ".coins_small";
    public static readonly string CoinsMedium = BundlePrefix + ".coins_medium";
    public static readonly string CoinsLarge  = BundlePrefix + ".coins_large";
    public static readonly string ExtraTime   = BundlePrefix + ".extra_time";
}
