# 2048 Merge - 欧美休闲手游项目

## 📁 项目结构
```
Assets/
├── Scripts/
│   ├── GameBoard.cs      ✅ 核心棋盘逻辑（合并/计分/关卡）
│   ├── AdManager.cs      ✅ IronSource广告集成
│   ├── ScoreManager.cs   ✅ 分数系统（本地存储）
│   └── SwipeInput.cs     ✅ 移动端触摸滑动
├── Scenes/               （Unity内手动创建）
└── UI/                   （Unity内搭建UI）
```

## 🚀 快速开始

### 1. Unity环境
- Unity版本：2022.3 LTS
- 渲染管线：URP（2D）
- 目标平台：Android API 35 + iOS 16+

### 2. 依赖插件
```
Package Manager安装：
- TextMeshPro（内置）
- IronSource SDK（官方下载）
- DOTween（Asset Store免费版）
```

### 3. IronSource配置
- 在AdManager.cs中填入 appKey
- 配置激励视频placement：
  - "revive"：失败后复活
  - "win_double"：胜利后双倍奖励

### 4. 广告变现点位
| 触发时机 | 广告类型 | 预期收入占比 |
|---------|---------|------------|
| 游戏失败后 | 激励视频（复活） | 40% |
| 胜利后 | 激励视频（双倍） | 30% |
| 每3关结束 | 插页广告 | 30% |

## 📱 Android 合规配置
```
Player Settings:
- Min API Level: Android 8.0 (API 26)
- Target API Level: Android 14 (API 35)
- Scripting Backend: IL2CPP
- Target Architecture: ARM64
- Build Format: .aab（App Bundle）
```

## 💰 预期数据
- CPI目标：< $0.5
- 次日留存目标：> 40%
- 广告ARPDAU目标：> $0.08
- 回本周期：30~45天

## 📅 开发进度
- [x] 核心棋盘逻辑
- [x] 广告SDK框架
- [x] 分数系统
- [x] 触摸输入
- [ ] Unity场景搭建（UI布局）
- [ ] 美术改皮（从Asset Store购买）
- [ ] Google Play Billing接入
- [ ] 测试包导出
