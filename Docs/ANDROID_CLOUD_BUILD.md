# Android 云端自动打包

仓库已配置 GitHub Actions 工作流：`.github/workflows/android-apk.yml`。

## 首次使用前必须配置 Unity 许可证

GitHub 云端不能绕过 Unity 授权。仓库管理员需要在 GitHub 仓库：

Settings → Secrets and variables → Actions → New repository secret

添加以下 Actions secrets：

- `UNITY_LICENSE`：Unity Personal/Pro 许可证文件内容（按 GameCI 文档要求提供）。
- `UNITY_EMAIL`：Unity 账号邮箱。
- `UNITY_PASSWORD`：Unity 账号密码或该工作流要求的账号凭据。

请勿把许可证、邮箱或密码写进仓库文件、README、Issue 或聊天消息。建议先阅读 GameCI 官方文档，确认当前 Unity 版本与许可证激活方式：
https://game.ci/docs/github/activation

## 触发构建

配置好 Secrets 后：

1. 打开 GitHub 仓库的 Actions 页面。
2. 选择 **Android APK Build**。
3. 点击 **Run workflow**。
4. 等待任务完成。成功后打开该次运行页面底部的 Artifacts。
5. 下载 `FantasyAdventure-Android-APK`，解压即可取得 APK。

工作流也会在 main 分支相关项目文件更新时自动触发。

## 构建内容与限制

- 使用仓库锁定的 Unity Editor 版本：`6000.0.40f1`。
- 通过 `FantasyAndroidBuild.Build` 生成森林第一关场景并执行 Android APK 构建。
- 输出路径：`build/Android/FantasyAdventure.apk`。
- 该工作流只负责云端构建，不代表 APK 已经在真实安卓设备上完成运行测试。
- 当前第一关仍是程序生成的占位美术原型；高清角色动画、精细场景、完整战斗反馈和设备适配仍需继续开发。
