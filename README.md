# 幻想冒险：异世界之旅

面向 Android 的原创 2D 横版动作 RPG，使用 Unity 6000.0.40f1 + C# 开发。

当前开发内容为第一大关「森林净化」：森林入口、林间古道、守卫遗迹，包含角色动作、森林美术、普通敌人与首领、检查点、单武器装备及品质升级、药瓶式生命/魔法 HUD 和本地进度保存。

## 开发

1. 安装 Unity 6000.0.40f1 和 Android Build Support（SDK、NDK、OpenJDK）。
2. 用 Unity 打开仓库。
3. 选择 Fantasy Adventure → Build Demo Level 生成第一大关场景。
4. 运行场景；Android 构建入口为 FantasyAndroidBuild.Build。

操作及内容见 [第一大关说明](Docs/FIRST_LEVEL_DEMO_GUIDE.md)。美术规则见 [ART_DIRECTION](Docs/ART_DIRECTION.md)。

## Android APK

[Android APK Build](https://github.com/2507155811/FantasyAdventure/actions/workflows/android-apk.yml) 在 main 分支相关文件变更时自动运行，也可手动触发。

构建依赖仓库 Actions Secrets 中的 UNITY_LICENSE、UNITY_EMAIL、UNITY_PASSWORD。成功后，在对应运行页底部 Artifacts 下载 FantasyAdventure-Android-APK，解压取得 APK。

目前完整打包结果以对应提交的 Actions 结果为准；安卓设备上的视觉、动作和操作效果仍需真机检查。请勿把密码或许可证内容写进代码、日志或聊天。
