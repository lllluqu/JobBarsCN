# 上游更新时如何维护汉化版

本仓库是 [0ceal0t/JobBars](https://github.com/0ceal0t/JobBars) 的简体中文汉化分支。
上游发版后，按下面的流程把新代码合进来、补翻译、再发布。

## 0. 仓库结构

```
JobBars/                          ← 插件工程
  Localization/Loc.cs             ← 运行时翻译入口（Loc.T / Loc.Display / Loc.TryGet）
  Localization/zh-CN.json         ← 汉化词典（英文原文 → 中文）
  GlobalUsings.cs                 ← global using JobBars.Localization
  JobBarsCN.json                  ← 插件清单（Name/InternalName/版本/简介）
tools/
  build.ps1                       ← 本地构建
  check-keys.ps1                  ← 词典覆盖率检查（必须 missing=0）
  test-display-names.py           ← 显示名转换测试（职业后缀/触发后缀）
```

汉化改动分两类，冲突处理方式不同：

| 类型 | 文件 | 上游更新时 |
| --- | --- | --- |
| 本地化基础设施 | `Localization/*`、`GlobalUsings.cs`、`JobBarsCN.json`、`tools/`、`README.md` | 保留本仓库版本 |
| 被 `Loc.T(...)` 包裹的业务代码 | 其余 `JobBars/**/*.cs` | **以上游为准**，重新套翻译 |

## 1. 一次性准备

```powershell
git clone --recurse-submodules https://github.com/lllluqu/JobBarsCN.git
cd JobBarsCN
git remote add upstream https://github.com/0ceal0t/JobBars.git

# 构建依赖：.NET 10 SDK + Dalamud 开发库
# Dalamud 开发库默认放在 %AppData%\XIVLauncher\addon\Hooks\dev
# 下载 https://goatcorp.github.io/dalamud-distrib/latest.zip 解压到该目录
```

## 2. 每次上游更新

### 2.1 同步代码

```powershell
git fetch upstream
git merge upstream/main
git submodule update --init --recursive   # 上游可能更新了 KamiToolKit 子模块
```

### 2.2 解决冲突

绝大多数冲突来自我们给字符串加的 `Loc.T(...)` 包装：

```powershell
# 业务代码直接取上游版本
git checkout --theirs -- JobBars/Buffs JobBars/Cooldowns JobBars/Cursors JobBars/Gauges JobBars/Icons JobBars/Manager JobBars/Jobs JobBars/Nodes JobBars/Data JobBars/Helper

# 保留本地化文件
git checkout --ours -- JobBars/Localization JobBars/GlobalUsings.cs JobBars/JobBarsCN.json JobBars/JobBars.csproj
```

> merge 时 `--theirs` = 上游，`--ours` = 当前分支（汉化）。rebase 时相反。

### 2.3 重新套翻译

上游代码取回后，按下面的顺序把 `Loc.T(...)` 套回去（关键点：**只包玩家可见的文字**）。

1. 设置窗口与 Tab 标题：`JobBars.Settings.cs`
2. 各管理器 UI：`*/Manager/*.UI.cs`
3. 共享基类：`Manager/PerJobManager*.cs`、`Gauges/GaugeConfig.cs`、`Data/Configuration*.cs`、`Data/ItemSelector.cs`、`Helper/InfoBox.cs`、`Helper/GenericDialog.cs`
4. 各职业配置里的可调项：`Gauges/**/*Config.cs`、`Buffs/BuffConfig.cs`、`Cooldowns/CooldownConfig.cs`、`Icons/IconReplacer.cs`、`Cursors/Cursor.cs`

规则：

- 纯标签：`ImGui.Checkbox( Loc.T( "Out of combat" ), ... )`
- 带 ImGui ID 的标签：`Loc.T( "Scale" ) + manager.Id`（**不要**把中文放进 `##` 之后）
- 插值：`Loc.T( "Position ({0})", value )`，占位符顺序与原文一致
- 枚举下拉：给枚举成员加 `[Description("English")]`，`Loc.Display(...)` 会查词典
- 名称类（量谱/增益/冷却/图标名）**不要**改内部 `Name` 字段（它是配置键），渲染时用 `UiHelper.DisplayName( Name )`
- 职业名用 `UiHelper.JobDisplay( job )`
- 不翻译：日志、配置键、IPC 名、纹理路径、正则、`##` 之后的 ImGui ID

### 2.4 补新词条

```powershell
pwsh tools/check-keys.ps1     # 看 MISSING 列表
```

把缺失的英文原文加进 `JobBars/Localization/zh-CN.json`，中文用 FF14 国服用语（见下方术语表）。

### 2.5 校验并构建

```powershell
pwsh tools/check-keys.ps1          # 必须 MISSING: none
python tools/test-display-names.py # 必须 failures=0
pwsh tools/build.ps1
```

## 3. 发布

1. 改版本号：`JobBars/JobBars.csproj` 的 `AssemblyVersion`/`FileVersion` 与 `JobBars/JobBarsCN.json` 的 `AssemblyVersion`。
2. 构建后取 `JobBars/bin/x64/Release/JobBarsCN/latest.zip`，重命名为 `JobBarsCN-<版本>.zip`。
3. 在 GitHub 建 release，tag 用 `v<版本>`，上传该 zip。
4. 更新 [MyDalamudRepo](https://github.com/lllluqu/MyDalamudRepo) 的 `pluginmaster.json`：`AssemblyVersion`、三个 `DownloadLink*`、`LastUpdated`（Unix 秒）。

## 4. 术语表

| 英文 | 中文 |
| --- | --- |
| gauge | 量谱 |
| buff | 增益 |
| cooldown | 冷却 |
| icon replacement | 图标替换 |
| cursor | 光标 |
| proc | 触发 |
| GCD | GCD |
| MP tick | 跳蓝 |
| DoT tick | DoT 跳伤 |
| slidecast | 滑步 |
| stacks | 层数 |
| charges | 充能 |
| party member | 队友 |
| out of combat | 非战斗状态 |
| weapon sheathed | 收起武器 |
| position locked | 锁定位置 |
| scale | 缩放 |
| opacity | 不透明度 |
| border | 边框 |
| ring | 圆环 |
| bar | 条状 |
| diamond | 菱形 |
| arrow | 箭头 |
| global / per job / split | 全局 / 按职业 / 分离 |
| completion sound | 完成音效 |
| low warning | 低时间警告 |
| custom cooldown | 自定义冷却 |
| trigger type | 触发类型 |
