# Google Play Console 内购配置指南

## 前置条件
1. Google Play Console账号（$25一次性费用）
2. 应用已创建并上传至少一个APK/AAB
3. 银行账户已验证（用于收款）

---

## 第一步：创建内购商品

进入路径：
```
Google Play Console → 选择你的应用
→ 盈利 (Monetize) → 产品 (Products)
→ 应用内商品 (In-app products)
```

### 消耗型商品（一一创建以下7个）

#### 1. 小金币包
```
商品ID：coins_small
名称：100 Coins
描述：A small bag of coins for power-ups
价格：$0.99（系统会自动换算其他货币）
状态：启用
```

#### 2. 中金币包（主推）
```
商品ID：coins_medium
名称：350 Coins ⭐ Best Value
描述：The best value coin pack! Get 350 coins for power-ups and extra time.
价格：$2.99
状态：启用
```

#### 3. 大金币包
```
商品ID：coins_large
名称：900 Coins
描述：A large bag of coins for serious players
价格：$6.99
状态：启用
```

#### 4. 额外时间
```
商品ID：extra_time
名称：Extra Time x3
描述：Get 3 uses of the +30 seconds power-up
价格：$0.99
状态：启用
```

### 非消耗型商品

#### 5. 去广告
```
商品ID：no_ads
名称：Remove Ads
描述：Permanently remove all ads from the game. One-time purchase, forever ad-free!
价格：$1.99
状态：启用
```

#### 6. VIP礼包
```
商品ID：vip_pack
名称：VIP Bundle 🎁
描述：The ultimate package! Remove all ads + 500 coins + exclusive golden tile skin.
价格：$4.99
状态：启用
```

### 订阅型商品

#### 7. 周VIP
```
进入路径：盈利 → 产品 → 订阅
商品ID：weekly_vip
名称：Weekly VIP
描述：Remove all ads + double coins every day!
免费试用期：3天（建议开启，提升转化）
价格：$1.99/周
宽限期：3天
账单重试期：30天
状态：启用
```

---

## 第二步：配置Google Play Billing Library

在Unity项目中：
```
Window → Package Manager
→ 搜索 "Google Play Billing"
→ 安装最新版本（当前4.x）
```

或手动在 `Packages/manifest.json` 添加：
```json
"com.unity.purchasing": "4.9.3"
```

---

## 第三步：测试内购

### 添加测试账号
```
Google Play Console
→ 设置 → 许可测试
→ 添加你的Gmail测试账号
→ 这些账号购买时不会真实扣费
```

### 沙盒测试流程
```
1. 打包Debug APK（勾选Development Build）
2. 安装到真机（不能用模拟器！）
3. 用测试账号登录Google Play
4. 正常触发购买流程
5. 选择"测试卡"完成支付
```

---

## 第四步：上线检查清单

- [ ] 所有商品ID与代码中IAPManager.cs一致
- [ ] 包名（Bundle ID）与Google Play Console一致
- [ ] 至少1个测试账号完成了每种商品的购买测试
- [ ] 恢复购买功能测试通过
- [ ] 订阅取消后游戏正常降级

---

## 常见错误处理

| 错误信息 | 原因 | 解决方案 |
|---------|------|---------|
| BillingResponseCode.ITEM_UNAVAILABLE | 商品未发布或ID不匹配 | 检查商品ID和发布状态 |
| BillingResponseCode.DEVELOPER_ERROR | 包名/签名不匹配 | 确认签名与Console一致 |
| 无法初始化 | 未安装Google Play服务 | 需要真机测试 |
