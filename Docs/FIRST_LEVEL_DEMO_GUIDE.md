# 第一关演示版搭建说明
## 当前阶段
这是第一关可运行原型框架。Unity 编辑器菜单会生成测试场景和临时占位图形。高清角色逐帧动画、精细地图、音效仍需继续制作；概念设定图不是完整游戏动画。
## 环境
- Unity 6 LTS，2D Core 项目
- Android Build Support（SDK、NDK、OpenJDK）
## 导入与生成
1. 克隆仓库到电脑。
2. Unity Hub → Add → 选择项目根目录。
3. 等待 Unity 导入脚本。
4. 顶部菜单选择 Fantasy Adventure → Build Demo Level。
5. 自动生成 Assets/Scenes/ForestStage01.unity。
6. 打开场景并点击 Play。
## 键盘操作
- A/D 或方向键：移动
- Space：跳跃
- Left Shift：闪避
- J：普通攻击
- K：范围技能（消耗 15 魔法）
- H：红药水
- M：蓝药水
## 手机触控
场景内置基础触控按钮布局。正式安卓版本还需替换为适配安全区域和不同屏幕比例的 Canvas UI，并在真机验证。
## 第一关内容
森林入口、平台地形、普通怪物、森林守卫首领、生命/魔法、药水、金币奖励和通关提示。
## 后续工作
角色动作帧与 Animator、怪物独立外观与攻击动作、技能特效、分层背景、掉落物实体、音效、存档和完整安卓适配。