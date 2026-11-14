# 幻想冒险：异世界之旅

Unity 6000.0.40f1 + C# 的 Android 横版动作 RPG。当前 0.3.0 扩展为完整单机流程：主菜单、三档存档、46 地区主线路线、21 个探索/挑战地区、99 条任务记录、四类武器及品质形态、技能与被动成长、商店、结局和通关后地下挑战。

这是参考《小红帽穿越记》制作的改编实现。地区和任务名称依据用户提供的安装包整理；地图布局、任务触发、对白及数值由本项目重建，不代表原作逐项复刻。实际完成度与验证边界见 [版本说明](Docs/CAMPAIGN_030.md)。

## 开发与构建

1. 安装 Unity 6000.0.40f1 和 Android Build Support。
2. 打开仓库，选择 Fantasy Adventure → Build Demo Level，生成包含整个单机流程的启动场景。此菜单保留旧名称以兼容原有构建入口。
3. Android 构建入口为 FantasyAndroidBuild.Build。图集切片、主角动作合并与场景引用在构建时自动生成。

[Android APK Build](https://github.com/2507155811/FantasyAdventure/actions/workflows/android-apk.yml) 在 main 分支变更后自动运行。成功运行底部下载 FantasyAdventure-Android-APK，解压取得 APK。构建所需 Unity 账户和许可证只保存在 Actions Secrets。

## 操作

左下四向方向盘：左右移动，上下爬梯；上键与居民交谈或进入出口；下 + 跳跃穿过单向平台。右侧为跳跃、攻击、技能，支持多指同时操作。装备菜单内更换武器，战斗时只装备一种。键盘支持方向键、空格、J 攻击、K 技能。

界面跟随横屏安全区域，兼容不同长宽比。三档存档隔离，自动记录任务、检查点、金币、物品与成长；旧版存档仅迁移金币、经验与武器成长。

## 验证

Tests/run_input_contracts.py 使用 Unity 附带的 .NET 编译器运行输入、攻击时机及屏幕布局契约。设置 CAMPAIGN_CONTRACTS=1 时运行所有地区的任务数据与存档契约。静态契约通过不等于手机实机通关，触感、音量和视觉接缝仍需设备验收。
