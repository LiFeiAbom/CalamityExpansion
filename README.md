# CalamityExpansion

**为灾厄新增内容。**
New content for Calamity — expanding what the mod has to offer.

一个灾厄扩展模组（tModLoader 1.4.4 / net8.0）：本工程做的是**往灾厄里加新东西**，不是旧版内容的回归。
首批内容是「中子」链 —— 中子之刃（Neutron Blade）与它的材料、弹幕、减益与专属稀有度。
软依赖 `CalamityMod` 与 `CalamityModClassicPreTrailer`（见 `build.txt` 的 `weakReferences`）；代码里一律用
`ModLoader.TryGetMod` / `TryFind<ModItem|ModTile|ModBuff>` 按名字取内容，不写死类型引用，
两版灾厄各自注册一条配方、名字对不上时那条分支静默不注册。

## 内容来源

与姊妹工程 `CalamityDemutation` 的分工不同：那个工程只收灾厄本体与 CI 来源，**本工程允许收录 CWR
（灾厄大修 = CalamityOverhaul）一系的内容**——首批的中子链就是从那份世系里取回来的（原实现在
`CalamityDemutation` 的 `19ad483^` 中可查）。

整合进来的移植件按本工程口径统一改名：`CD` 前缀改 `CE`、大修一系的前缀（`CO` / `EX` 等）也改 `CE`，
粒子系统的 `DRK` 一系保持原名。

## 当前内容

- **中子之刃 Neutron Blade**（`NeutronGlaive`）—— 月后近战巨剑；左键挥出中子之刃光束，右键蓄力喷追踪中子球、
  充满松手引爆中子大爆点（伤害为面板 ×10）。面板基础 855，配置「数值膨胀」开启后 4275。
- **中子锭 Neutron Bar**（`NeutronBar`）—— 中子之刃的合成材料（×12）。
  现代版灾厄配方 = 魔影锭 + 奇迹物质 @ 嘉登熔炉；经典版灾厄配方 = 七味经典材料 + 魔影锭 @ 嘉登熔炉。
- **中子熵变 Neutron Entropy**（`NeutronEntropy`）—— 弹幕命中附加的减益，每秒 7500 掉血、持续 20 秒，附带染色与星屑。
- **中子稀有度 NeutronRarity** —— 中子系列的专属档位，颜色在纯黑与浅黑之间反复渐变。

## 开发

- 构建：`dotnet build`。
  注意：该命令末尾会把 `.tmod` 写进 `…\tModLoader\Mods\`；若在工作区受限的环境（如 Codex 沙箱）里跑，
  这一步会因无写权限报 `TML001`——看编译器那两行的 **0 警告 0 错误**即可，要出 `.tmod` 请在普通 shell 里跑。
- 进游戏前跑一遍自查脚本（这两类问题**编译不报、只在加载期炸，并且会禁用整个模组**）：
  - `Tools/CheckImplicitTextures.ps1` —— 类同名隐式贴图（`类名.png`）。
  - `Tools/CheckResources.ps1` —— 显式资源路径（`"CalamityExpansion/..."` 这类字符串与常量拼接）。
  脚本覆盖不到的两类要手工看：`类名 + "Glow"` 这类后缀拼接、以及 `.fx` 是否有同名 `.fxc` 背书。
- `.fx` 着色器不会被构建自动编译，新增后要手动产出同名 `.fxc` 并随工程提交。
- 本地化：`Localization/` 下中英各一份扁平 hjson，玩家可见文本（物品 / 弹幕 / 增益减益 / 配置项）**两边都要补**。
  tML 在加载后会自动重写这两个文件：英文补英文默认值、中文补 `//` 注释占位——遇到就照它生成的键位把值填回去。
- **完整的工程口径、踩过的坑与当前状态都在 [AGENTS.md](AGENTS.md)**——动手前先读它。

## 致谢

内容源自 Calamity Mod 及其旧版世系（含 CalamityOverhaul / 灾厄大修 与 CalamityInheritance），
版权归灾厄模组原作者与各世系的维护者；本工程做的是内容移植与适配。
