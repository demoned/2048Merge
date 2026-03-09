# 上线前检查清单

## 🛠️ 技术配置

### Unity配置
- [ ] Bundle ID 已修改（非com.yourgame）
- [ ] 版本号：Version 1.0.0，Bundle Version Code：1
- [ ] Target API Level：Android 35 / iOS 16
- [ ] IL2CPP + ARM64 已开启
- [ ] 打包格式：.aab（Android App Bundle）
- [ ] 图标已替换（1024x1024 PNG，无透明）
- [ ] 启动屏已设置

### 广告配置
- [ ] IronSource AppKey已填入AdManager.cs
- [ ] 激励视频placement名称已在IronSource后台创建：
  - "revive"
  - "win_double"
- [ ] testMode已改为false
- [ ] 真机测试广告正常显示

### 内购配置
- [ ] Bundle ID前缀已修改（IAPConfig.cs）
- [ ] Google Play Console内购商品全部创建并启用
- [ ] Unity IAP包已安装（4.9.x）
- [ ] 真机沙盒测试通过（7个商品）
- [ ] 恢复购买功能测试通过（iOS）

### 合规配置
- [ ] 隐私政策URL已填写（Google Play必须）
- [ ] GDPR弹窗已接入（欧洲用户）
- [ ] ATT弹窗已接入（iOS 14+）

---

## 📱 Google Play上架

### 必填资料
- [ ] 应用名称：2048 Merge - Number Puzzle
- [ ] 简短描述（80字）：已准备 → GooglePlay_Listing.md
- [ ] 完整描述（4000字）：已准备 → GooglePlay_Listing.md
- [ ] 截图：手机截图至少2张（推荐5张，1080x1920）
- [ ] 特性图片：1024x500（必须）
- [ ] 图标：512x512 PNG
- [ ] 内容分级问卷：已完成
- [ ] 隐私政策URL：已填写
- [ ] 数据安全问卷：已完成

### 上架流程
```
1. 上传AAB文件到内部测试轨道
2. 完成商店资料填写
3. 提交内容分级问卷
4. 提交数据安全表单
5. 升级到开放测试或正式发布
6. 等待审核（通常1~3天）
```

---

## 🍎 App Store上架

### 必填资料
- [ ] 应用名称：2048 Merge - Number Puzzle
- [ ] 副标题（30字）：Race the Clock, Reach 2048!
- [ ] 描述（4000字）：已准备 → AppStore_Listing.md
- [ ] 关键词（100字）：已准备
- [ ] 截图：iPhone 6.7寸（至少3张）
- [ ] 预览视频（可选但推荐）
- [ ] 图标：1024x1024 PNG（无圆角，无透明）
- [ ] 审核账号：已准备测试账号

### 内购审核注意
- [ ] 所有内购商品在App Store Connect创建
- [ ] 内购描述清晰（审核员会测试）
- [ ] 订阅类必须有取消订阅说明
- [ ] 恢复购买按钮清晰可见

---

## 📊 上线后监控（前7天）

| 指标 | 目标值 | 监控工具 |
|------|------|---------|
| 次日留存 | > 35% | Firebase |
| 7日留存 | > 20% | Firebase |
| 广告填充率 | > 80% | IronSource后台 |
| 广告ARPDAU | > $0.05 | IronSource后台 |
| 崩溃率 | < 1% | Firebase Crashlytics |
| 内购转化率 | > 2% | Google Play Console |

---

## 🚀 买量启动计划

### 第1周（测试期）
```
预算：$50/天
平台：TikTok Ads（优先）
素材：3条横向游戏录屏（展示合并瞬间）
定向：女性 25-45岁，美国/加拿大/英国
目标：CPI < $0.8
```

### 第2-3周（优化期）
```
保留CPI < $0.8的素材
关掉表现差的广告组
预算提升至$150/天
```

### 第4周（放量期）
```
CPI稳定后扩展到Facebook Ads
预算：$300-500/天
新增地区：澳大利亚、德国、法国
```
