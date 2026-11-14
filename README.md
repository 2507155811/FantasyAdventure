# 幻想冒险：异世界之旅

一款面向 Android 的原创 2D 横版动作 RPG，使用 Unity + C# 开发。

## 项目目标

- 单机离线游玩，横屏，适配不同手机屏幕比例
- 精致高清 2D 卡通美术，原创角色、怪物、场景与 UI
- 左侧虚拟摇杆移动，右侧攻击、技能、跳跃与闪避
- 双武器切换，不同武器拥有独立动作与技能
- 主线章节、支线任务、隐藏区域、精英怪与 BOSS
- 装备掉落、随机词条、强化、背包、图鉴
- 自动存档、手动存档与多个本地存档槽
- 不包含解谜玩法，不包含宠物系统

## 当前开发阶段

**阶段 0：项目规划与基础代码骨架**

目前仓库包含游戏设计文档、美术规范和 Unity 脚本起步代码。场景、角色素材、动画、音效与 Android APK 需要在 Unity 编辑器中继续组装和测试。

## 使用方式

1. 在电脑安装 Unity Hub。
2. 安装 Unity 6 LTS，并勾选 Android Build Support、Android SDK & NDK Tools、OpenJDK。
3. 将本仓库克隆到电脑，使用 Unity Hub 打开项目目录（待补齐 Unity 项目设置与首个场景后）。
4. 按 `Docs/DEVELOPMENT_ROADMAP.md` 的顺序开发和验证。

## 目录

- `Assets/Scripts/`：游戏 C# 脚本
- `Docs/GAME_DESIGN.md`：玩法与系统设计
- `Docs/ART_GUIDE.md`：AI 美术素材规范
- `Docs/DEVELOPMENT_ROADMAP.md`：开发路线与验收标准

## 重要说明

本项目是原创作品，仅参考经典横版动作 RPG 的操作体验与信息布局，不使用其他游戏的角色立绘、地图、音乐、图标或代码。
