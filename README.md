# JobBars CN

为 [XIVQuickLauncher](https://github.com/goatcorp/FFXIVQuickLauncher) 提供的额外职业量谱、队友增益计时、减伤监控等功能的 **简体中文汉化版**。

基于 [ocealot/JobBars](https://github.com/0ceal0t/JobBars) 全面汉化，图标作者 [PAPACHIN](https://www.xivmodarchive.com/user/192152)。

## 功能

- 增益期间可打出的 GCD 数量（战逃反应、解放）
- DoT 计时（毒菌、煞气）
- 触发显示（火苗/雷云 预备）
- 充能数量（飞弹、回转飞弹）
- 层数显示（毁坏 IV）
- 图标替换（DoT 剩余时间、增益剩余时间）
- 队友减伤/增益冷却
- 减伤监控
- 光标显示（咏唱时间、GCD 计时、跳蓝、DoT 跳伤）

## 汉化说明

- 设置窗口、量谱/增益/冷却/图标/光标各页面、下拉框选项、提示文字、自定义冷却对话框、颜色名称全部为中文。
- 量谱、增益、冷却、图标的名称（例如「原初的解放」「战逃反应」）来自游戏数据表，国服客户端自动显示中文。
- 语言可选 **自动（跟随客户端）** / **简体中文** / **English**，默认跟随客户端；设置窗口底部可切换。
- 配置项内部仍以英文名保存，因此升级或切换语言不会丢失已有配置。

## 安装

1. 游戏内输入 `/xlsettings` 打开 Dalamud 设置。
2. 进入 **实验性功能 → 自定义插件仓库**，添加：

```
https://raw.githubusercontent.com/lllluqu/MyDalamudRepo/main/pluginmaster.json
```

3. 在 **插件安装器** 中搜索 `JobBars CN` 安装。
4. 使用 `/jobbars` 打开设置窗口。

## 命令

| 命令 | 说明 |
| --- | --- |
| `/jobbars` | 打开/关闭设置窗口 |

## 从源码构建

```powershell
dotnet build JobBars.sln -c Release -p:DalamudLibPath="<你的 Dalamud dev 目录>"
```

需要 .NET 10 SDK 与 Dalamud API 15 开发库（`XIVLauncher\addon\Hooks\dev`）。

## 许可

AGPL-3.0，与原项目一致。汉化部分同样以 AGPL-3.0 发布。
