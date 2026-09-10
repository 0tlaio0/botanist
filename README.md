# 亓官夙（Qgs）

这是一个基于 BaseLib 的《杀戮尖塔 2》角色 Mod 起始框架，当前按 BaseLib 3.4.5 和 .NET 9 配置。

## 使用

1. 打开 `qgs.csproj`，把 `Sts2Dir` 改为本机游戏安装目录。
2. 安装与游戏版本匹配的 BaseLib，并确认 `mods/BaseLib/BaseLib.dll` 存在。
3. 使用 Godot 4.5.1 Mono 导入本项目，添加 Windows Desktop 导出预设并导出 `qgs.pck`。
4. 执行 `dotnet build` 编译 DLL。构建脚本会把 DLL 和 `qgs.json` 复制到游戏的 `mods/qgs/`。
5. 确认 `qgs.pck` 也位于 `mods/qgs/`，启动游戏后在角色选择界面测试。

当前代码使用占位卡牌、遗物和视觉资源，适合先验证加载流程。实际游戏版本或 BaseLib API 发生变化时，应以本机 DLL 和模板为准调整签名。
