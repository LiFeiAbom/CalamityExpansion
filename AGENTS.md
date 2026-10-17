# CalamityExpansion — 工程记忆（新会话先读这份）

## 0. 交接摘要（最新：2026-10-17 下半场；下次继续从这里读）

**这次（2026-10-17 下半场）做完的**（全部已提交并推送，HEAD = `6b1ed3f`）：

1. **文案**：两件近战饰品的 tooltip 不再列出破晓EX / 燃烧IV 的数值，只保留「近战命中会造成X」；
   减益数值现在只存在于代码与本文档（见 §5.4 文案条）。
2. **游侠徽章EX**（§5.5）：贴图按星旋盔甲色系染好（灰盘不动、绿环 + 青色能量箭）；
   属性 = **+18% 远程伤害 / +18% 远程暴击率 / +18% 弓箭伤害 / +18% 弹药飞行速度 / 82% 概率不消耗弹药**；
   **远程攻速已按用户要求删掉**（别再当成漏做，也别和「弹药飞行速度」混）。
3. **狙击镜EX**（§5.6）：贴图按 Crossbow_Scope 的**复合造型**重绘（三处金圈），镜身染成星旋胸甲的
   「深紫底 + 灰板件 + 青色高光 + 深绿暗部」；属性 = +22% 远程伤害/暴击率/箭矢伤害/弹幕飞行速度、
   +50 远程穿透、40% 省弹、-400 仇恨、熔岩箭袋 + 潜行者箭袋效果（箭伤各 +10%，仇恨只算一次）、
   可见时侦察镜缩放、50% 额外夜明箭/弹；**星璇火箭已删**（太卡）。
4. **配方改档**：战士徽章EX / 游侠徽章EX = 对应徽章 + **神圣锭 ×15** + 五种魂各 25 @ **秘银砧**
   （石巨人之眼与月亮碎片已去掉）；狙击镜EX = 侦察镜 + 潜行者箭袋 + 熔岩箭袋 + 五种魂各 25 + 星旋碎片 50 @ 远古操纵机。
5. **清理**：四张参考图（Crossbow_Scope / Recon_Scope / Ranger_Emblem / Vortex_armor）与根目录 Daybroken.png 已删；
   BeetleGlove.png 也已不在。

**当前状态**：C# 编译 **0 警告 0 错误**；CheckImplicitTextures.ps1 **18 个类**全命中、
CheckResources.ps1 **27 条**全命中；.tmod 已打包（2026-10-17 23:12）；工作区干净、master 与远端同步。

**新素材（还没处理）**：Content/Items/Accessories/JobAcc/Magic/Sorcerer_Emblem.webp（巫士徽章原图，
用户 2026-10-17 放的，已被 git add -A 一并提交）。**还没有染色、没有 .cs、没有规格**——用户下次大概率会点名魔法系饰品。

**待实测（下次进游戏优先看）**：
- 游侠徽章EX：远程三围与 +18% 弓箭伤害是否如数、82% 省弹手感、弹速是否明显更快。
- 狙击镜EX：22% 箭伤 + 两个箭袋的 +20% 箭伤叠起来是否过强、50% 双发夜明箭/弹、按 R 看省弹、
  缩放是否**只在饰品可见时**生效、命中时确认**不再**有火箭。
- 老账：烈火手套EX / 燃烧IV 的溶解视觉与战士EX 档仍未进游戏实测（见 §7「第四批待实测/待定」）。

**待用户点名**：狙击镜EX 的价值（现 15 金）与英文名（现 `Sniper Scope EX`）；
两个箭袋的箭伤是否要砍成只算一次（`SniperScopeEX.QuiverArrowDamageBonus`）；
游侠徽章EX 的价值（现 15 金）；巫士徽章那张图要怎么做。

**下次开工的顺序**：`dotnet build -v q -nologo` → `Tools/CheckImplicitTextures.ps1` → `Tools/CheckResources.ps1`
（沙箱内构建会在打包那步报 TML001，要出可进游戏的 `.tmod` 得提权跑构建）；改完照例 `git add -A` → commit → push。

更早两天的明细（第三批战士徽章EX/破晓EX、第四批烈火手套EX/燃烧IV、燃烧IV 的加载期 NRE 与 IL 坑）
都还在下面 §3.2–§3.4 与 §5.3–§5.4 里，需要时按节查。

## 1. 工程是什么

**灾厄扩展 mod，定位是「为灾厄新增内容」**（用户 2026-10-15 明确：描述文案不能写成"回归"，要写成"新增"）。
tModLoader 1.4.4 / net8.0，显示名 `CalamityExpansion`，作者 LF。
软依赖：`weakReferences = CalamityMod, CalamityModClassicPreTrailer`（见 `build.txt`）；
代码里一律用 `ModLoader.TryGetMod` / `TryFind<ModItem|ModTile|ModBuff>` 按名字取内容，不写死类型引用。

工程于 2026-10-15 新建（空白模板），同一天开始内容移植，当天落了两批：**中子之刃整链**与**中子脉冲整链**；
目前全部内容为「中子」链。
姊妹工程 `CalamityDemutation` 的规则是「只收灾厄本体与 CI 来源、不收 CWR（灾厄大修）与 CE（灾厄：熵）」，
**本工程不受该限制**（这正是它存在的理由之一），但两工程的换算/命名/工作流口径仍然通用。

## 2. 内容来源与取回方法

中子之刃整链在 `CalamityDemutation` 的提交 `19ad483`（「删除: CWR（灾厄大修）与 CE（灾厄：熵）来源的内容（第一批）」）
里被删除，**最后存在的版本就是它，即 `19ad483^`**；第二批清理是 `f8c48ab`。

取回方式（二进制资产也走同一条路，cmd 重定向按字节写出；别用 PowerShell 的 `>` 否则贴图会坏）：

```powershell
git -C ..\CalamityDemutation cat-file blob '19ad483^:Content/Items/Weapons/Melee/NeutronGlaive.cs' > <本工程路径>
git -C ..\CalamityDemutation cat-file blob 'HEAD:Utilities/ProjUtil.cs' > <本工程路径>      # 通用工具取 HEAD（未删）
git -C ..\CalamityDemutation cat-file -s '19ad483^:<path>'                                  # 核对字节数
```

即：**中子链本体取 `19ad483^`，通用基础设施（工具类、粒子系统、扭曲管线）取 `HEAD`**。
取回后统一做 `CalamityDemutation` → `CalamityExpansion` 整词替换，再逐项裁剪（第 4 节）。

## 3. 本次移植清单（2026-10-15，中子之刃整链）

来源 `19ad483^`（中子链本体）：

| 类别 | 文件 |
|---|---|
| 武器 | `Content/Items/Weapons/Melee/NeutronGlaive.cs` + `.png`（显示名「中子之刃」，月后稀有度 16 品红） |
| 材料 | `Content/Items/Materials/NeutronBar.cs` + `.png`（显示名「中子锭」，原类名 `BlackMatterStick`，见第 4 节改名口径） |
| 弹幕 | `Content/Projectiles/Melee/{NeutronGlaiveBeam, NeutronExplode, CENeutronExplode, NeutronGlaiveHeld, NeutronsOrb}.cs`（+ `NeutronGlaiveBeam.png`） |
| 基类 | `Content/Projectiles/BaseProjectiles/BaseHeldProjCE.cs`（右键蓄力手持弹幕基类，原 `BaseHeldProjCO`） |
| 减益 | `Content/Buffs/NegativeBuffs/NeutronEntropy.cs` + `.png`（显示名「中子熵变」，原 `VoidErosion`） |
| 贴图 | `Assets/Masking/DiffusionCircle.png`（扭曲遮罩）、`Assets/UIs/NeutronsBar{,2,Top,Top2}.png`（充能条） |

来源 `HEAD`（通用基础设施）：

| 类别 | 文件 |
|---|---|
| 粒子 | `Content/Particles/{DRK_Spark, DRK_HeavenfallStar}.cs`、`Content/Particles/Core/{BaseParticle, DRKLoader}.cs`、`Assets/Particles/StarProj.png` |
| 扭曲管线 | `Systems/Graphic/IDrawWarp.cs`、`Common/Effects/{EffectsSystem, EffectLoader}.cs`、`Effects/WarpShader.fx` + `.fxc` |
| 工具 | `Utilities/{CEUtil_Swing, ProjUtil, WeaponUtil}.cs`（合起来就是 `CEUtil` 静态类的各 partial，原类名 `CDUtil`） |
| 占位贴图 | `Content/Projectiles/InvisibleProj.png`（无贴图弹幕用） |

本工程新写（不是搬来的）：

- `CalamityExpansion.cs` —— 单例 / `FlashEffectStrength` / `ScreenShakeAmp` / `gravityDontFlipScreen` 软依赖 /
  加载期请求着色器 / 卸载期清静态引用（含 `NeutronGlaiveHeld.UnloadTextures()` 与 `NeutronGlaiveBeam.warpTex`）。
- `Common/CalamityExpansionConstant.cs` —— 只保留 `Masking` / `UI` / `MaxParticleCount` / `noEffects`（旧工程的 `ColorBar` 未带）。
- `NPCs/CalamityExpansionGlobalNPC.cs` —— 只承载中子熵变：`ResetEffects` 复位标记、`UpdateLifeRegen`、
  `DrawEffects`（红通道压到 100 + 玩家附近喷星屑）。
- ~~`Content/Items/CalamityExpansionGlobalItem.cs`~~ —— 只承载月后稀有度 `postMoonLordRarity`，
  **2026-10-15 按用户要求整体删除**（见第 5.1 节：它会把档号存进物品存档，读回来时反过来盖掉新稀有度）。
- `Content/Projectiles/CalamityExpansionGlobalProjectile.cs` —— 只保留 `defExtraUpdates` 缓存字段（`ProjUtil.HomeInOnNPC` 要用）。
- `Systems/ConfigSystem.cs` —— 只保留「性能模式」（`ProjUtil.DrawAfterimages` / `DrawBeam` 的开关）。
- `Localization/en-US_…` 与 `Localization/zh-Hans_Mods.CalamityExpansion.hjson` —— 中子锭 / 中子之刃（四行 tooltip 照抄旧文案）、
  中子熵变（名称 + 描述）、以及中子链五枚弹幕的显示名。
- `Tools/CheckResources.ps1`、`Tools/CheckImplicitTextures.ps1` —— 从 `CalamityDemutation\Tools` 拷来并把脚本内模组名改成 `CalamityExpansion`。

### 3.1 第二批（2026-10-15，中子脉冲整链）

来源 `19ad483^`（中子脉冲链本体）：

| 类别 | 文件 |
|---|---|
| 武器 | `Content/Items/Weapons/Ranged/NeutronGun.cs` + `.png`（显示名「中子脉冲」/ en `Neutron Pulse`）+ `NeutronGun2.png`（右键重击时的替换贴图） |
| 弹幕 | `Content/Projectiles/Ranged/{NeutronGunHoldout, NeutronBullet, NeutronLaser, NeutronExplosionRanged, CENeutronExplosionRanged}.cs`（+ `NeutronBullet.png`，32×256 等离子长矛） |
| 基类 | `Content/Projectiles/BaseProjectiles/BaseGunHoldoutCE.cs`（枪械式手持弹幕基类，原 `BaseGunHoldoutCO`，移植自灾厄 `BaseGunHoldoutProjectile`） |
| 扭曲管线 | `Common/Effects/NeutronWarpHelper.cs`（第一次带进来的 helper）+ `Effects/NeutronWarp.{fx,fxc}`。着色器里有五支技法（GravitationalVortex / ShockwaveRing / RelativisticJet / GravitationalLens / KamuiLine），本批只消费 GravitationalLens（中子弹）与 ShockwaveRing（两枚爆点） |
| 贴图 | `Assets/ExtraTextures/{white,StarTexture}.png` —— 1×1 白像素（扭曲着色器铺满用）、326×326 星形遮罩（两枚爆点的贴图键） |
| 音效 | `Sounds/Item/{Gun_AWP_Shoot,Pecharge}.ogg` + 本工程新写的 `Sounds/CalamityExpansionSounds.cs`（只带这两条；旧工程那张音效表有 40 多条，按"只带真消费的"逐批补） |

第二批顺带新写 / 补齐的基础设施（不是搬来的）：

- `Utilities/CEUtil_Laser.cs` —— **只带了 `CantUseHoldout`**（枪械持握弹幕的去留判据，原在旧工程 `Utilities/CDUtil_Laser.cs`）；
  那支文件里的 `DrawAfterimagesCentered` 本工程已有等价物（`CEUtil.DrawAfterimages`），
  `PreciseDistanceToTileCollisionHit` / `AnyProjectiles` 目前没有消费方。
- `Common/CalamityExpansionConstant.cs` 新增 `ExtraTextures` 前缀。
- `Common/Effects/EffectLoader.cs` 新增 `NeutronWarp` 句柄（本工程第二支着色器）。
- 本地化：中子脉冲物品（名字 + 四行 tooltip）与五枚弹幕的显示名，中英都补。

### 3.2 第三批（2026-10-16，战士徽章EX + 破晓EX）

本批**不是从旧工程搬来的**，是用户给素材 + 口述规格、本工程新写的：

| 类别 | 文件 |
|---|---|
| 饰品 | `Content/Items/Accessories/JobAcc/Melee/WarriorEmblemEX.cs` + `.png`（显示名「战士徽章EX」/ en `Warrior Emblem EX`） |
| 减益 | `Content/Buffs/NegativeBuffs/DaybreakEX.cs` + `.png`（显示名「破晓EX」/ en `Daybreak EX`） |
| ModPlayer | `Players/CalamityExpansionPlayer.cs`（**本工程第一个 ModPlayer**：破晓EX 的施加（敌怪路）与玩家侧结算） |
| 全局物品 | `Content/Items/CalamityExpansionGlobalItem.cs`（破晓EX 的 PvP 施加·物品路；**无实例字段**，故不需要 InstancePerEntity） |
| 侦察工具 | `Tools/ReadVanillaIL.ps1`（Mono.Cecil 读 tModLoader.dll，dump 原版方法 IL / 列 API 成员；本批的原版破晓口径就是靠它实查的） |

- **贴图口径**：`WarriorEmblemEX.png`（28×28）是原版战士徽章的染色版。用户给的参考 `Warrior_Emblem.webp` 28×28
  与 `Solar_Flare_Breastplate.webp` 38×28 都是**精确 2× 放大图**（14×14 / 19×14 的像素块翻倍）；
  本工程既有素材 `BeetleGlove.png` 26×30 也是同一口径（13×15 翻倍），故本批照用户素材同口径出图：
  **不缩放、只逐像素换色**，成品 28×28。
- **染色映射**（色板取自日曜胸甲）：银环白 `223,223,223`→`249,75,7`、环中间调 `161,161,161`→`200,44,18`、
  剑身高光 `255,249,209`→`253,169,49`、剑身黄 `236,214,51`→`254,158,35`、金 `231,153,28`→`246,131,14`、
  `224,139,15`→`249,75,7`、暗金 `175,106,17`→`200,44,18`、暗棕 `96,57,9`→`102,34,34`、
  最深棕 `51,33,5`→`76,26,26`（胸甲深栗再压暗一档，保住剑身最深轮廓）。
  **四个中性色原样保留**（`32,32,32` / `64,64,64` / `84,84,84` / 透明）——于是仍是「深灰圆盘 + 橙红环 + 橙色剑」的层次。
  临时产出的 `WarriorEmblem.png`（同一张图、旧名）已随本批删除，工程里只留 `WarriorEmblemEX.png`。
- **破晓EX 图标**：用户给的 `Daybroken.png`（32×32 = 16×16 的 2× 放大，正好是 buff 贴图标准尺寸）原样入库为
  `Content/Buffs/NegativeBuffs/DaybreakEX.png`；工程根目录那份参考图**没删**（用户没点名，留作备查）。
- **EX 徽章改风格（2026-10-17，用户两次纠正后定稿）**：最终口径 =「**原先的灰色部分不染色；已经染过的那些
  （外圈白环 + 中心金剑）改成手套底座那种渲染**」。做法：暗盘 3 个灰阶 `(32,32,32)/(64,64,64)/(84,84,84)`
  原样保留，其余不透明像素按感知亮度 L（`0.299R+0.587G+0.114B`）映射到手套那套暖色阶
  （`106,10,4 → 245,30,18 → 255,127,39 → 255,169,74 → 255,207,106 → 255,247,83`，L 区间拉伸到全档）：
  `(76,26,26)→(106,10,4)`、`(102,34,34)→(173,20,11)`、`(200,44,18)→(252,95,32)`、`(249,75,7)→(255,161,67)`、
  `(246,131,14)→(255,206,105)`、`(254,158,35)→(255,236,89)`、`(253,169,49)→(255,247,83)`。
  ⚠️ 走的弯路：先误把「暗盘（灰）」当成底座染了暖色，方向正好反了，用户当天纠正为「灰的不动、染过的那些学这个风格」。
  ⚠️ 用户原给的 `Warrior_Emblem.webp` 已从工程根目录和回收站都找不到；本次是从中间产物
  `%TEMP%\ce_emblem_work\WarriorEmblemEX_solar.png` 反推的（太阳色版，映射时有两档与原剑色重合，不影响成品）。

### 3.3 第二张染色（2026-10-16，烈火手套EX 的贴图）

- **对象**：`Content/Items/Accessories/JobAcc/Melee/FireGauntletEX.png`（26×30 = 13×15 的 **2×**，与
  `BeetleGlove.png` / `WarriorEmblemEX.png` 同口径）。用户口径：「不修改任何样式，全部涂改成日曜那种烈红色」——
  即**不重绘、不改尺寸**，把整张图（含灰色金属件）统一进一个红色系。
  ⚠️ 中途改过口径：先按「日曜那种烈红」染了一版（色阶 `76,26,26 → 255,114,87`），用户 2026-10-16 看过之后说
  日曜红不如**原图那个烈火红**好看，于是以原图自带的 `(245,30,18)`（色相 3.2°、饱和 0.93）为准重染；
  重染一律从**原件**重新算，不在上一版上叠色。
  **最终口径（用户 2026-10-17 澄清后确定）**：「1 里手指那些个颜色不动，底座的灰色染成手指那种风格」——
  即手指/火焰部分**一个像素都不改**，只把底座的 5 个灰阶按「手指自带的暖色阶」重映射（把灰阶范围拉伸到暖色阶全档）：
  `(50,50,50)→(106,10,4)`、`(68,69,64)→(220,26,15)`、`(100,101,94)→(255,135,46)`、
  `(136,137,127)→(255,197,98)`、`(163,166,155)→(255,247,83)`（暖色阶取自手指本身：
  暗红 → 红 → 橙 → 亮橙 → 更亮橙 → 黄）。
  中途误做过「整只纯色 `(245,30,18)`」一版，用户同一天澄清后作废（那版留档见下）。
- **做法**（可复用）：每个非透明像素取 `t = (V + 感知亮度L) / 2`（V = 通道最大值，L = 0.299R+0.587G+0.114B），
  再走五档**烈火红阶**插值（中间版本，已备份）：`0.00 (51,4,3)` → `0.35 (115,10,6)` → `0.70 (245,30,18)`
  → `0.88 (255,80,50)` → `1.00 (255,115,95)`（整条色阶锁在原图烈火红的色相上）；alpha 原样保留。
  换色系时只改这五个色阶即可，混合口径不用动。
- ⚠️ **踩过的坑**：① 只用亮度 L 会把饱和红压暗（(245,30,18) 的 L 只有 0.29），火焰比灰件还暗；
  ② 只用 V 会把四档亮焰色（V 全是 1.0）并成同一个颜色，层次全丢；③ 纯色相锁定（保 S/V、只换 H）
  会把灰色件（S=0）留在灰——因为用户要「全部」变红，必须抬饱和度。故最终取 (V+L)/2。
  ④ 用 PIL 覆盖同一个文件时，若还留着 `Image.open` 的句柄（懒加载不会自动关），`save` 会抛
  `OSError: [Errno 22] Invalid argument`——先把参照图 `load()`+`copy()` 再写。
- **现状**：贴图最终版 = **手指原样 + 底座暖色化**（就地覆盖，10 种颜色：原 12 色里有两档映射后与手指色重合）。
  三个版本都在 `%TEMP%\ce_gauntlet_work\`：`FireGauntletEX_original.png`（原件）、
  `FireGauntletEX_shaded_red.png`（整只渐变烈火红的中间版）、`FireGauntletEX_flat_red.png`（整只纯色，已作废）、
  `compare_base.png`（本轮三联对比）。临时目录可能被系统清理，要长期留档得说一声搬进工程。
  该贴图目前**没有对应 .cs**，与（已被用户删掉的）`BeetleGlove.png` 同为待规格的 WIP 素材。

**第三张染色（2026-10-17，游侠徽章 RangerEmblemEX）**

- **素材**：`Content/Items/Accessories/JobAcc/Ranged/Ranger_Emblem.webp`（28×28，与战士徽章同样 2× 口径）
  + 配色参考 `Ranged/Vortex_armor.webp`（30×44 星旋胸甲）。输出 `Ranged/RangerEmblemEX.png`（28×28，RGBA，11 色）。
  两张 .webp **留在原位当参考图**（用户没点名删，构建对它们无警告）。2026-10-17 当天贴图已配上 .cs，
  落地成 `RangerEmblemEX`（见 §5.5）。
- **星旋盔甲取色**（从 `Vortex_armor.webp` 直接统计）：底 `(39,24,46)` 深紫、深绿 `(24,93,66)`、
  绿 `(47,184,131)`、亮绿 `(34,221,151)`、青 `(67,255,255)`、白高光 `(255,255,255)`、
  板件灰 `(68,68,68)/(108,108,108)/(134,134,134)/(152,152,152)/(176,176,176)`。
- **口径**：与战士徽章同一条——**3 个灰盘色阶 `(32,32,32)/(64,64,64)/(84,84,84)` 一个像素都不动**（输出里三档数量
  160/156/48 与原件完全一致），其余 7 个有彩色重映射。映射表：
  `(51,33,5)→(26,18,34)`（暗部走星旋底色的深紫）、`(96,57,9)→(20,92,64)`、
  `(161,161,161)→(47,184,131)`（银环主体=盔甲绿）、`(223,223,223)→(130,245,200)`（银环高光=薄荷绿）、
  `(231,153,28)→(34,221,151)`、`(236,214,51)→(67,255,255)`、`(255,249,209)→(180,255,255)`
  ——即箭身走「深绿 → 亮绿 → 青 → 淡青」的发光阶，与星旋"绿色甲片 + 青色能量"的角色分工一致。
- ⚠️ **走过的弯路（别再机械按亮度排）**：先试过「7 个有彩色按感知亮度排序 → 依次塞进 7 档星旋色阶」（版本 A/B/D），
  结果银环的高光段（223）拿到了最亮档，抢掉了箭头的层次，箭身与环又同为绿、在小尺寸下糊成一团。
  改成**按"环 / 箭"分色**（H 版：环=绿+薄荷，箭=绿→青）后箭头才立得住。对比图在
  `%TEMP%\ce_ranged_work\`（`compare.png` / `compare2.png` / `compare3.png` / `style_check.png`）。

**第四张：狙击镜 ReconScope（2026-10-17，按 Crossbow_Scope 的复合造型重绘）**

- **素材**：`Ranged/Recon_Scope.webp`（30×30 = 15×15 @2×，用户给的原图：深紫描边 + 金环 + 绿镜片，16 色，
  **用户要求原图配色一寸不改**）；造型参考 `Ranged/Crossbow_Scope.png`（34×36 = 17×18 @2×，黑描边 + 灰金属 + 红镜片）。
- **用户口径（2026-10-17，纠正过一次）**：`Crossbow_Scope` 是**几支小镜筒 / 支架拼起来的复合体**
  （左上一条长管 + 中间并排的第二条 + 两个小红镜窗 + 右下大物镜）。要的是**把这支狙击镜也画成那种复合造型**，
  **不是**把狙击镜染成灰红。⚠️ 上一版把 Recon 重映射成「黑描边 + 灰金属 + 红镜片」被用户否掉（`12adc7e` 那版），别再那样做。
- **输出**：`Ranged/SniperScopeEX.png`（34×36 @2×，与 Crossbow_Scope 同画布/同造型，配色是狙击镜自己那套）。
  2026-10-17 起这张图已配上 `SniperScopeEX.cs`（§5.6），文件名也跟着物品改成了 `SniperScopeEX.png`。
  ⚠️ 两张参考图（`Crossbow_Scope.png` / `Recon_Scope.webp`）与另外两张染色参考图
  （`Ranger_Emblem.webp` / `Vortex_armor.webp`）、根目录的 `Daybroken.png` 已在 2026-10-17 按用户要求删除，
  需要时从 git 历史（`21b02cf` 及以前）里取回。
- **做法**：取 `Crossbow_Scope` 的**整个像素网格（含 18 色灰阶结构）**，把它的色板按「明度对应」换成狙击镜的色板——
  近黑/描边 `14~31` → 深紫描边 `(19,4,36)`；灰阶 `51/66/106/159/209` → 紫灰 `(45,52,83)/(81,75,96)/(120,115,144)/(155,150,176)/(177,174,196)`；
  红镜片 `94/128/255/196,55,55/255,128,128` → 绿镜片 `(39,74,42)/(39,74,42)/(95,135,85)/(139,182,128)/(213,224,169)`。
  然后给**内圈**补金色卡圈（上左 `(185,164,23)`、下右 `(110,72,0)`，像素级的 8 邻域，不覆盖描边）：
  右下大物镜 + **两个小镜窗都有**（用户 2026-10-17 追加「两个小镜窗也带一点金色卡圈」），
  因为原图狙击镜的身份色就是那圈金环。
- ⚠️ **走过的弯路**：① 先试过从头用距离场程序化生成一支新镜子（v1–v9，`%TEMP%\ce_scope_work\`），物镜糊、镜筒发细；
  ② 又试过把原图重映射成 Crossbow 的灰红配色（被用户否掉）。正确解法是**反过来**：拿 Crossbow 的造型、灌狙击镜的配色。
- **对比图**：`%TEMP%\ce_scope_work\cmp_shape4.png`（Crossbow 原图 / A 无金环 / C2 / C3 柔和金 / 狙击镜原图）、
  `cmp_goldring2.png`（Crossbow / 只有大镜金圈 / 三处都有金圈）。
- **镜身染星璇色（2026-10-17 用户追加「把狙击镜EX 的镜子身躯染成星璇色系」）**：
  只动**身躯那五档紫灰**，镜片（绿）与金色卡圈（金）原样保留。描边 `(19,4,36)` 不动。
  ⚠️ **第一版（全绿 A）被用户否掉**：整条镜身推成「深紫→深绿→绿→亮绿→青」太绿了，和星旋胸甲不像。
  **星旋胸甲的真实构成**（2026-10-17 重新统计）：主体是深紫 `(39,24,46)`（408 像素，占绝对多数）+ **灰色板件**
  `(68,68,68)/(72,70,79)/(108,108,108)/(110,106,120)/(134,134,134)/(142,139,152)/(152,152,152)/(176,176,176)`
  + 绿色描边点缀 `(47,184,131)`×92 / `(34,221,151)` / `(24,93,66)` + 青色发光核心 `(67,255,255)` 与白高光。
  **最终映射（V6）**：`(45,52,83)→(39,24,46)` 深紫底、`(81,75,96)→(24,93,66)` 深绿、
  `(120,115,144)→(108,108,108)` 灰板、`(155,150,176)→(176,176,176)` 亮灰板、`(177,174,196)→(67,255,255)` 青色高光。
  对比图：`cmp_vortex.png`（全绿 A / B）、`cmp_vortex2.png`（V1 纯灰板 / V3 灰板+青 / V4 灰+绿描边）、
  `cmp_vortex3.png`（V3 / V4 / V5 / V6）、`cmp_final_vortex.png`（成品 vs 星旋胸甲并排）。

### 3.4 第四批（2026-10-17，烈火手套EX + 燃烧IV）

同第三批：**不是从旧工程搬来的**，是用户给素材 + 口述规格、本工程新写的。

| 类别 | 文件 |
|---|---|
| 饰品 | `Content/Items/Accessories/JobAcc/Melee/FireGauntletEX.cs` + `.png`（显示名「烈火手套EX」/ en `Fire Gauntlet EX`） |
| 减益 | `Content/Buffs/NegativeBuffs/HellfireIV.cs` + `.png`（显示名「燃烧IV」/ en `Hellfire IV`；图标取灾厄仓库里那份原版 Hellfire 图标） |
| 着色器 | `Effects/HellfireIV.fx` + `.fxc`（移植自灾厄现代版 `Effects/MiracleBlight.fx`，改名并改为燃烧橙红） |
| 贴图 | `Assets/ExtraTextures/Neurons.png`（溶解噪声贴图，自灾厄 `ExtraTextures/GreyscaleGradients/Neurons` 取来，512×512） |
| 渲染基建 | `Common/Graphics/Buffers/{RenderTargetPool, ScreenspaceTargetPool, RenderTargetScope, RenderTargetPreserver}.cs`（自姊妹工程 `Graphics/Buffers/` 原样搬来，只改命名空间）+ `Common/Graphics/SpriteBatchScope.cs`（本工程新写） |
| 稀有度 | `Content/Rarities/WarriorEXRarity.cs`（战士EX 档：名字字色**橙红渐变**，战士徽章EX 与烈火手套EX 共用；见 §5.3/§5.4） |

- **着色器口径**：新增 .fx 必须现场编译出同名 .fxc（`..\..\FXC\fxc.exe /nologo /T fx_2_0 /Fo 名字.fxc 名字.fx`，
  会刷一条 X4717 属正常），否则运行期 `Assets.Request<Effect>` 抛异常、整个模组被禁用 —— 命令原文也写在 `EffectLoader` 的 remarks 里。
  发光边颜色**不再硬编码**（原版是荧光绿 `0.4902,1,0`），改为读 `uColor`，由调用方 `UseColor(燃烧橙红)` 传入，
  换风格不用重编译；pass 名 `HellfireIVPass` 必须与 `EffectLoader.HellfireIVShaderPass` 一致（MiscShaderData 按 pass 名取）。
- **染色（用户 2026-10-17 两轮改口径后定稿）**：手指/火焰部分**原样不动**，底座灰件按手指那套暖色阶渲染
  （映射见 §3.3 那条同款做法：`(50,50,50)→(106,10,4)`…）。临时对比图见 `%TEMP%\ce_gauntlet_work\`。

⚠️ 补一条：§4 末尾那句「本工程自此**没有 GlobalItem**」在 2026-10-16 之后**不再成立**——本批为破晓EX 的
PvP 施加加了 `Content/Items/CalamityExpansionGlobalItem.cs`（**无实例字段**，与旧那套稀有度体系无关，别混为一谈）。

## 4. 命名口径与裁剪（重要，别当成漏做）

- **改名口径（用户 2026-10-15 定）**：工程名叫 `CalamityExpansion`，所以
  **CD 前缀 → CE、CWR 一系的移植件前缀 → CE**。已执行的：
  `CDUtil` → `CEUtil`（文件 `CDUtil_Swing.cs` → `CEUtil_Swing.cs`）、`BaseHeldProjCO` → `BaseHeldProjCE`、
  `EXNeutronExplode` → `CENeutronExplode`（`EX` 是大修那套前缀），
  `VoidErosion` → `NeutronEntropy`（显示名「中子熵变」）、`BlackMatterStick` → `NeutronBar`（显示名「中子锭」/ en `Neutron Bar`）；
  第二批又落了两个：`BaseGunHoldoutCO` → `BaseGunHoldoutCE`、`EXNeutronExplosionRanged` → `CENeutronExplosionRanged`。
  **例外（用户明确）**：`DRK` 一系的粒子名（`DRKLoader` / `DRK_Spark` / `DRK_HeavenfallStar`）不用管，保持原样。
  **译名口径（用户 2026-10-15）**：「中子长戟」是**大修（CWR）的译名**，本工程一律不用——
  中英显示名统一为中英对齐的**「中子之刃」/ `Neutron Blade`**：
  物品与手持弹幕都用这一对，光束弹幕取「中子之刃光束」/ `Neutron Blade Beam`；
  `description.txt` / `description_workshop.txt` 里的英文名也一并同步。
  注释里提到旧译名时只作为"大修原译"的说明保留。**类名未动**：
  `NeutronGlaive` / `NeutronGlaiveHeld` / `NeutronGlaiveBeam` 仍是原名（含本地化键与贴图路径里的 `NeutronGlaive`），
  用户若要连类名一起改成 `NeutronBlade*` 再议。
  尚未动、但同属"移植件脏名字"的：`IDrawWarp.costomDraw`（大修原版拼错的接口成员名）与 `NeutronGlaiveHeld` 里的
  `canatcck / canatcck2 / canatcck3 / maxatcck / uiframe / sengs` 等局部命名——用户未点名，暂留。
- **命名空间与资源路径**：整词替换。`IDrawWarp` 旧工程在 `Content/Projectiles/IDrawWarp.cs`（靠命名空间上溯解析），
  本工程按现状放在 `Systems/Graphic/IDrawWarp.cs`，故三枚实现它的弹幕显式 `using CalamityExpansion.Systems.Graphic;`。
- **`EffectsSystem` 整支搬入**（扭曲桶 + 蓝移/无蓝移两路 + 透明滤镜技巧）；`FlashEffectStrength`（闪白）与
  `ScreenShakeAmp`（屏幕震动）本工程没有产出方，与旧工程一致只保留字段与每帧衰减。
- **`EffectLoader` 现在两支**：`WarpShader`（把位移场合成为屏幕扭曲）与第二批带进来的 `NeutronWarp`
  （程序化生成位移场，由 `NeutronWarpHelper` 驱动）。旧工程另外四支（KnifeRendering / KnifeDistortion / Metaball ×2）
  属于别的 CWR/CE 内容，没随行；**留着不删会在加载期抛 `MissingResourceException` 并禁用整个模组**
  （第一批就是被 `CheckResources.ps1` 抓出来的）。两支 .fx 都由同名 .fxc 背书，取的是旧仓库里已提交的那份。
- **`DRKLoader` 只注册两枚粒子**（DRK_Spark / DRK_HeavenfallStar）：其余五枚属于别的内容，没搬。
- **`CEUtil_Swing` 去掉了 `player.CD()` / `projectile.CD()` 两个扩展**（那需要本工程的 ModPlayer / GlobalProjectile 数据类，中子之刃用不到）。
- **`WeaponUtil` / `ProjUtil` 保留完整**（`ExoDebuffs`、`ProjectileRain` 等通用工具没删，本工程后续移植会用到）。
- **稀有度不走旧体系**：旧工程那 1000 行的 `GlobalItem` 里含「月后稀有度 `postMoonLordRarity`」+「回退灾厄对原版的削弱」
  + 套装增伤等大块内容；本工程当初只搬了稀有度部分，**2026-10-15 连这一部分也删了**（改用 `Content/Rarities/NeutronRarity.cs`
  真稀有度）。本工程自此**没有 GlobalItem**，NPC / 弹幕两个全局类仍在。

## 5. 中子链的关键数值（2026-10-15 用户拍板后的现值）

- **中子之刃**：稀有度 = 本工程自带的 `NeutronRarity`（**黑 ↔ 浅黑 反复渐变**，中子系列专属档，见下）。
  伤害**基础 855 / 膨胀档 4275**（用户 2026-10-15 定：855 回归源值，4275 挂到配置的「数值膨胀」开关上）。
  实现照工程既有口径——`Item.damage = 855`（源值），武器自己写 `InflatedDamage = 4275f`、`BaseDamage` 与
  `ModifyWeaponDamage`（每帧读 `ConfigSystem.StatInflationEnabled`，故开关无需重载、切换即时生效）；
  右键大爆点按面板 ×10 生成，会跟着一起放缩。
  其余：近战、14 帧出手、击退 7.5、暴击 +8、`useTurn`、自动连发、价值 1 铂 50 金 75 银 10 铜、
  `shoot = NeutronGlaiveBeam`、初速 18。

### 5.1 中子稀有度（NeutronRarity）与它的依据

`Content/Rarities/NeutronRarity.cs`：`RarityColor => Color.Lerp(纯黑, 浅黑, sin 脉动)` —— **黑 ↔ 浅黑反复渐变**。
用户 2026-10-15 拍板：**中子之刃的刀身就是黑色的**（贴图 = 黑刃 + 青蓝描边），所以档位色取刀身本色，
在纯黑 `(0,0,0)` 与浅黑 `(70,70,70)` 之间来回脉动（两端色值与速度都是文件顶部的常量，改一处即可；
脉动写法照 CWR main 的 `BrutalRelicRarity`：`Color.Lerp + MathF.Sin(Main.GlobalTimeWrappedHourly * n)`）。

- **大修 0.4.0.1.3 自己并不定义稀有度**：整棵源码树 `ModRarity` 匹配数 = 0。
  它是**直接引用灾厄本体的自定义档**：中子系里挂自定义档的只有中子星枪口制退器 `NeutronStarMuzzleBrake`
  （`ModContent.RarityType<Turquoise>()`）；中子之刃 / 中子脉冲 / 洛希之弦 / 中子法杖在那边**都是原版红**
  （`ItemRarityID.Red`），中子镰刀是 `LightPurple`。
- **灾厄本体的自定义档共 7 个**（`CalamityModPublic-2.0.4\Rarities\`）：
  CalamityRed `(163,25,26)`、DarkBlue `(43,96,222)`、DarkOrange `(204,71,35)`、HotPink `(255,0,255)`、
  PureGreen `(0,255,0)`、Turquoise `(0,255,200)`（= Rarity 12）、Violet `(108,45,199)`。
  旧工程 `postMoonLordRarity` 的 12~16 档就是照这套色板做的：12 青绿 / 13 荧光绿 / 14 蓝 / 15 紫 / 16 品红。
- **曾走过一次弯路**：最初按"大修给中子件挂的是灾厄 Turquoise"把档位色取成青绿 `(0,255,200)`，
  用户当场纠正——中子之刃是黑色的，青绿只是描边/特效色，不作为档位色。**别再照 Turquoise 取色。**
- 中子系的贴图与特效配色（备查，不是档位色）：物品贴图 = 黑刃 + 青蓝描边；弹幕特效主色 `BlueViolet`，
  辅以 `CadetBlue`/`LightBlue`。
- **未写** `GetPrefixedRarity` 前缀链（灾厄 Turquoise 那条是 -2 红 / -1 紫 / +1 PureGreen / +2 DarkBlue）——
  目前没有同级兄弟档可挂，等后续档位落进来再补。
- 中子之刃原先借旧工程的 `postMoonLordRarity = 16`（品红），2026-10-15 换成 `NeutronRarity`；
  **那套 `postMoonLordRarity` 体系（GlobalItem）同日整体删除**。删它的直接原因是实测踩坑：
  该体系通过 `SaveData` / `LoadData` 把档号存进**物品存档**，世界里已存在的物件读回来仍带
  `postMoonLordRarity = 16`，于是它的 `ModifyTooltips` 继续把名字刷成品红，**盖掉了新的 `NeutronRarity`**——
  表现为"换了稀有度但游戏里看着没变"。以后要调稀有度，直接改 `Rarities/NeutronRarity.cs`，别再引入按物品存自定义档号的体系。
- 左键：射出 `NeutronGlaiveBeam`（近战、32×32、存活 120 帧、MaxUpdates 3），撞墙以 -0.6 倍速度反弹；
  命中附加 1200 帧中子熵变；消亡时以 300 半径做一次范围伤害并生成 `NeutronExplode`。
- 右键：`NeutronGlaiveHeld`（充能上限 80 帧），蓄力期间每 5 帧喷一枚追踪球 `NeutronsOrb`；
  松手时若充能满则在朝向 255 像素处生成 `CENeutronExplode`，**伤害 = 武器伤害 ×10**（用户 2026-09-15 指定的口径）。
- **中子熵变**（原虚空侵蚀）：`npc.lifeRegen -= 15000`、`damage` 下限 **7500**
  （用户 2026-10-15 由 -10000 / 5000 提升，血条上显示的即每秒 7500）；命中附加 1200 帧，染色 + 星屑见 GlobalNPC。
- **中子之刃配方**：中子锭 ×12 @ 德雷顿熔炉（`DraedonsForge`），现代/经典各一条，均走 `TryGetMod` 软引用。
- **中子锭配方**（用户 2026-10-15 拍板，替换掉旧工程那套"四柱碎片 + 全部矿锭"）：
  - 现代版灾厄 = 魔影锭 `ShadowspecBar` ×1 + 奇迹物质 `MiracleMatter` ×1 @ 德雷顿熔炉；
  - 经典版灾厄 = **七味经典材料 + 魔影锭 ×1** @ 德雷顿熔炉：梦魇燃料 `NightmareFuel`×5、
    吸热能量 `EndothermicEnergy`×5、夜魇锭 `CosmiliteBar`×5、暗日碎片 `DarksunFragment`×5、
    狱火碎片 `HellcasterFragment`×5、幻魂 `Phantoplasm`×5、圣金源矿 `AuricOre`×25，外加魔影锭 `ShadowspecBar`×1。
    **材料块来源**：照本工程星流之刃 `ExoBlade` 的经典分支（现代侧的 1 个奇迹物质在那边被换成上述七味），
    本条再补一味魔影锭；**狱火碎片取 5**（ExoBlade 处取 3，用户 2026-10-15 指定）。八味材料 + 站台均已核对存在于 cal-1.4.2.101。
  - **命名对照（别再搞混）**：星流之刃 = `ExoBlade`（en `Exoblade`）；`StellarStriker` 在本工程叫**月炎之锋**
    （它是 `ExoBlade` 的六把下位剑之一，配方 = 彗星圣锤 `CometQuasher` + 月亮锭 ×10 @ 月亮工作台，与金源无关）。
  - **备查**：`ExoBlade` 的六把自持下位剑 = 泰拉巨刃 `Terratomere`、混乱之刃 `AnarchyBlade`、霜火之刃 `FlarefrostBlade`、
    熵之舞 `EntropicClaymore`、凤凰之刃 `PhoenixBlade`、月炎之锋 `StellarStriker`。
  - 中子锭本体：堆叠 99、**中子稀有度**（黑 ↔ 浅黑脉动，用户 2026-10-15 定"中子系全部改掉"，原为原版青柠）、
    售价 1 铂；英文名按用户 2026-10-15 要求由 `Black Matter Stick` 修正为 `Neutron Bar`。

### 5.2 中子脉冲（NeutronGun，第二批）

- **中子脉冲**：稀有度 = 中子系共用的 `NeutronRarity`；伤害**基础 580 / 膨胀档 2900**
  （580 是 CWR 源值；2900 按中子之刃同倍率 ×5 推，**用户 2026-10-15 实测后确认收下**）。实现照工程既有口径——
  `Item.damage = 580` + `InflatedDamage = 2900f` + `BaseDamage` + `ModifyWeaponDamage`（每帧读开关，切换即时生效）。
  其余：远程、射速 5 帧、击退 1.5、暴击 +2、消耗子弹类弹药、`channel` + `noUseGraphic` + `useStyle = Shoot`、
  价值 13 铂 83 金 5 银、`shoot = NeutronGunHoldout`、初速 12。
- **持握弹幕 `NeutronGunHoldout`**（108×56，贴图 7 帧）：左右键共用同一条弹幕，物品只负责把它拿出来。
  去留判据是「玩家已无法持械 / 手里不是中子脉冲 / 两个鼠标键都松开」三者任一，**过载回落期间例外**
  （否则充能条会随枪一起断掉；CWR main 版为此专门加了 StayAlive）。
  左键速射：射速 4 帧、**每第 3 发放慢到 12 帧**，每发消耗一发弹药射出一颗中子弹；
  右键重击：射速 45 帧、伤害 **×5.6**，每发 +10 蓄力，满 80 那发在**鼠标位置**炸出 `CENeutronExplosionRanged`
  并把后续伤害抬到 **×15.6**，其后充能逐帧回落、归零时解除过载（两个倍率取 CWR main 版口径，旧版是 2.6 / 5.6）。
  右键姿态把手持物前伸距离从 35 涨到 65 像素并换成 `NeutronGun2` 贴图；
  头顶充能条直接复用 `NeutronGlaiveHeld.DrawBar`（两者上限同为 80）。
- **中子弹 `NeutronBullet`**（32×256 等离子长矛、14×14 判定、MaxUpdates 6、存活 160 帧、无限穿透、
  同敌人只结算一次）：飞行时走 `IDrawWarp` 的**引力透镜**（80×80 场、强度 0.3、进度恒 1、UV 半径 0.4）；
  命中原地炸小爆点 `NeutronExplosionRanged`，并从目标四周朝内落 3 道 `NeutronLaser`
  （出生点 = 圆心 + 随机单位方向 ×16~17 ×10 倍、速度取反即朝内飞）。
  注：生成时传的 `ai[0]`（0 普通 / 1 蓄力）在现版**没有分支**——中子弹每发命中都落光束。
- **中子光束 `NeutronLaser`**：5×5 判定、不绘制（贴图走 `InvisibleProj` 占位），视觉全靠每帧一颗蓝紫 `DRK_Spark`；
  80 点护甲穿透、命中把该敌人的受击无敌帧清零（所以能连续多段命中）。
- **两枚爆点**：小爆点 `NeutronExplosionRanged`（100×100、20 帧、同敌人 6 帧冷却）与大爆点
  `CENeutronExplosionRanged`（200×200、30 帧、80 点护甲穿透）都不绘制本体，只往扭曲管线里画
  **冲击波环**（300×300 / 强度 ×0.65 与 500×500 / ×0.8、大爆点 UV 半径 0.48）。
- **配方**：中子锭 ×12 @ 德雷顿熔炉，现代/经典各一条（与中子之刃同档，走 `TryGetMod` 软引用）。
- **音效**：开火 `Gun_AWP_Shoot`（左右键音高音量不同）、充能满 `Pecharge`，两条都来自 CWR，落在本工程新建的
  `Sounds/CalamityExpansionSounds.cs` 里。

### 5.3 战士徽章EX（WarriorEmblemEX）与破晓EX（DaybreakEX，2026-10-16 第三批）

- **战士徽章EX**：近战饰品（JobAcc/Melee）。属性（用户 2026-10-16 口述）：**+18% 近战伤害 / +18% 近战暴击率 /
  +18% 近战攻击速度**；近战命中——**物品挥砍与近战弹幕两条路都算**——附加破晓EX。命中判定写在
  `CalamityExpansionPlayer.OnHitNPCWithItem` / `OnHitNPCWithProj`，按 `hit.DamageType.CountsAsClass(DamageClass.Melee)` 过滤，
  远程/魔法/召唤/无职业一律不加。附加时长照**原版破晓武器**的 5 秒（300 帧；用户未点名时长，这条是我照原版补的）。
  稀有度：**2026-10-17 起改为本工程自带的 `Warrior EXRarity`**（`Content/Rarities/WarriorEXRarity.cs`，
  与烈火手套EX 共用）——用户当天指定「名字字样要显示橙红渐变」，于是从原版红档（`ItemRarityID.Red`）
  换成这支自定义档：四档橙红循环流动（暗红 `200,44,18` → 烈火红 `245,30,18` → 橙红 `255,96,16` → 亮橙 `255,160,50`，
  全部取自这两件饰品的贴图配色），写法与中子档同源（每帧读 `GlobalTimeWrappedHourly`，只是从两色脉动升级为多色循环，
  循环插值照灾厄 MulticolorLerp 口径）。贴图 28×28。价值 15 金
  （**我按 JobAcc 饰品量级暂定，用户未点名**）。
- **配方（2026-10-17 用户改过）**：战士徽章 + **神圣锭 `HallowedBar` ×15** + 光明/暗影/恐惧/力量/视域之魂各 25
  @ **秘银砧**（`TileID.MythrilAnvil`）。原来的「石巨人之眼 + 日曜碎片 50」已被 15 个神圣锭取代，站台也从远古操纵机改了。
  （`ItemID.WarriorEmblem` / `ItemID.EyeoftheGolem` / `SoulofLight|Night|Fright|Might|Sight` / `ItemID.FragmentSolar`，
  站台 `TileID.LunarCraftingStation` = 412）。八味材料全是原版，故**不走 TryGetMod 软引用**。
  ⚠️ 用户原话写的是「战术徽章」，本机全盘查无此物（原版、灾厄、大修、本地化都没有），按「战士徽章」落地。
  ⚠️ 字段名坑：石巨人之眼是 `ItemID.EyeoftheGolem`（**小写 the**），写 `EyeofTheGolem` 编译直接报 CS0117。
- **破晓EX**：**敌怪与玩家都生效**的减益，置位在本类；敌怪侧数值与表现在 `CalamityExpansionGlobalNPC`，
  玩家侧在 `CalamityExpansionPlayer`：
  - `UpdateLifeRegen`：`lifeRegen -= 1500`（血条上显示每秒 **750**；本工程换算与中子熵变一致：lifeRegen ÷ 2 = 每秒），
    `damage` 下限 750。
  - `ModifyIncomingHit`：`modifiers.Defense.Flat -= 50` 与 `modifiers.FinalDamage *= 1.1f`（即「-50 防御 + 受到伤害 +10%」）。
    走的是 1.4.4 的 `HitModifiers`——**与原版灵液/减防同一路数**，不是去改 `npc.defense`。
  - `DrawEffects`：**粒子照原版破晓**（IL 实查 `NPC.UpdateNPC_BuffApplyVFX` 的 daybreak 支）：3/4 概率喷一枚
    `DustID.OrangeTorch`（158）、scale 3.5、noGravity、速度 ×2.8 再 Y-0.5、其中 1/4 概率改为受重力并 scale×0.5；
    外加 `Lighting.AddLight(npc.Center, 1f, 0.3f, 0.1f)` 的橙色照明。原版破晓不改敌怪染色，本移植也不改。
  - 备查：原版 `BuffID.Daybreak` = 189、`ProjectileID.Daybreak` = 636。原版破晓的 DoT 按「扎在身上的投矛数量」叠，
    本移植按用户口径改成**固定 750/s**。
  - **死亡表现（2026-10-16 用户点名复刻，已落地）**：`GlobalNPC.HitEffect` 里判 `daybreakEX && npc.life <= 0`，
    调 `DaybreakEX.DeathEffect`，照抄 `NPC.VanillaHitEffect` 的 daybreak 支：
    ① 以 `npc.Hitbox.TopLeft()` 为原点、按本体宽高喷 **20 枚**尘，尘 ID 三选一 = `DustID.Torch`(6) /
    `DustID.SolarFlare`(259) / `DustID.OrangeTorch`(158)（原版把这三个 ID 压在 `PrivateImplementationDetails`
    的静态数组里，已用 Cecil 读出初始值 `[6, 259, 158]`），每枚 `alpha = 200`、`velocity *= 1.4`、
    `scale += Main.rand.NextFloat()`，初速固定 `(0, -2.5)`；
    ② `Main.netMode != MultiplayerClient` 时把破晓EX 传染给 **100 像素内**、未免疫、非 dontTakeDamage、
    `lifeMax > 5`、非 friendly / townNPC 的敌怪（300 帧）——判据与原版逐条对齐。

  - **玩家侧效果（2026-10-16 用户点名补齐）**：`CalamityExpansionPlayer` 里——`UpdateBadLifeRegen` 压
    `Player.lifeRegen -= 1500`（同样每秒 750，换算与敌怪侧一致：lifeRegen ÷ 2）、`PostUpdateEquips` 扣
    `Player.statDefense -= 50`、`ModifyHurt` 里 `modifiers.FinalDamage *= 1.1f`；玩家身上的粒子与照明在
    `DaybreakEX.Update(Player)` 里出（每端各画自己那份，与原版对远程玩家的处理一致）。
  - **PvP 施加路径（2026-10-16 补齐）**：物品打玩家 → `Content/Items/CalamityExpansionGlobalItem.cs` 的 `OnHitPvp`；
    弹幕打玩家 → `CalamityExpansionGlobalProjectile.OnHitPlayer`（先挡 `hostile` 与 owner 越界）；两者共用
    `CalamityExpansionPlayer.TryApplyDaybreakEXOnPvP(attacker, target, damageType)`（先判饰品，再判近战分类）。
    ⚠️ **`Player.HurtInfo` 没有 `DamageType` 字段**（与 NPC 的 `HitInfo` 不同），命中类型只能从 `Item.DamageType` /
    `Projectile.DamageType` 取——本批唯一的编译报错（CS1061）就出在这里。
  - **多人口径（2026-10-16 适配）**：增益本身靠原版 `AddBuff`（非 quiet）同步，本工程不自己发 NetMessage；
    数值各端本地算，但玩家生命以本人客户端为准，不会重复扣血；死亡爆尘是纯视觉（各端各画），死亡传染只在
    `Main.netMode != MultiplayerClient` 端做一次（照原版判据）；PvP 施加落在「被击中那名玩家的客户端」那一次结算。
  - **文案（2026-10-16 用户指定，中英同步）**：破晓EX 描述 = 「来自烈阳的光芒肆意灼烧」/
    `Rays of the blazing sun burn without restraint`；中子熵变描述 = 「中子共振的能量击碎现实」/
    `The energy of neutron resonance shatters reality`（原「熵变正在吞噬你」/ `Entropy is eating you away` 作废）；
    徽章 tooltip 第 4 行改成「近战命中会造成破晓EX（对敌怪与玩家都生效）」/ 英文同步加了括号说明。

### 5.4 烈火手套EX（FireGauntletEX）与燃烧IV（HellfireIV，2026-10-17 第四批）

- **烈火手套EX**：近战饰品（JobAcc/Melee）。属性（用户口述）：**防御 +8**、**近战伤害/暴击率/攻速各 +22%**、
  **仇恨 +400**、**近战击退 ×2**、**近战武器大小 +20%**、**允许自动挥舞近战武器与鞭子**。
  落点：防御/仇恨/自动挥舞直接写玩家字段（`statDefense`、`aggro`、原版 `autoReuseAllWeapons`，后者被
  `CanAutoReuseItem` 读取，鞭子也在覆盖内）；三围走 `GetDamage/GetCritChance/GetAttackSpeed(Melee)`；
  击退与大小走 `CalamityExpansionPlayer.ModifyWeaponKnockback` / `ModifyItemScale`（都按 `DamageClass.Melee` 过滤）。
- **配方**（用户口述）：烈火手套 + 狂战士手套 + 毁灭者徽章 + 日曜碎片 ×99 + 地狱锭 ×99 @ 远古操纵机
  （`ItemID.FireGauntlet` / `BerserkerGlove` / `DestroyerEmblem` / `FragmentSolar` / `HellstoneBar`，
  站台 `TileID.LunarCraftingStation`）。全是原版材料，不需要软引用。
  贴图 26×30；价值 15 金（**用户未点名，沿用徽章口径**）；稀有度与战士徽章EX 共用 `WarriorEXRarity`（橙红渐变字色）。
- **燃烧IV**：敌怪与玩家都生效；**每秒 900 生命**（`lifeRegen -= 1800`，与既有换算一致：lifeRegen ÷ 2 = 每秒）、
  **防御 -75**（敌怪 `modifiers.Defense.Flat`，玩家 `statDefense`）、**受到伤害 +15%**（`FinalDamage *= 1.15f`）。
  施加口与破晓EX **共用**：`CalamityExpansionPlayer.ApplyMeleeHitDebuffs`（物品/弹幕两路）+
  `TryApplyMeleeHitDebuffsOnPvP`（PvP，由 `CalamityExpansionGlobalItem.OnHitPvp` 与
  `CalamityExpansionGlobalProjectile.OnHitPlayer` 调用）。时长沿用破晓EX 的 300 帧（用户未点名）。
- **视觉：完全复刻灾厄现代版 MiracleBlight（超位崩解），颜色改燃烧橙红**——
  - 火花层：照 `MiracleBlight.DrawEffects`——1/4 概率一枚尘（`DustID.OrangeTorch`，noGravity，scale 0.7~0.85 并随体积微增）、
    1/3 概率一枚火花粒子（原版是他们的 GlowSparkParticle，本工程用 `DRK_Spark` 承担），
    取色从原版的 Red/MediumTurquoise/Orange/LawnGreen 换成燃烧橙红族四选一。
  - 溶解层：`HellfireIV.Load()` 里 `On_Main.DrawNPC += ...`，把带标记的敌怪单独画进屏幕空间渲染目标
    （`ScreenspaceTargetPool.Shared`，自姊妹工程/RenderTargetPool 那套），再用 Misc 着色器画回来；
    噪声漂移 + `uOpacity 0.7` + 三项 cos/sin 抖动阈值，超过就抹掉、略低一档换成 `uColor`。
  - 排除名单照原版：月亮领主（本体/手/头）与灾厄 Ares 五个部件；灾厄是软依赖，按名字 `TryFind` 懒构建。
  - **文案（用户 2026-10-17 指定）**：燃烧IV 描述 = 「永不熄灭，万物成灰」/ `Never extinguished; all things turn to ash`。
  - **饰品 tooltip 不写减益的数值说明（用户 2026-10-17 追加口径）**：两件近战饰品的 tooltip 只保留
    「近战命中会造成破晓EX（对敌怪与玩家都生效）」/「近战命中会造成燃烧IV」这一句，**不再**列出减益的
    掉血 / 减防 / 增伤数值——那两行 2026-10-17 已从中英两个语言文件里删掉。
    ⚠️ 于是减益数值目前**只存在于代码与这份文档里**，游戏内不可见（`HellfireIV` 顶部的常量
    `BurnLifeRegen` / `DefenseLoss` / `DamageTakenMultiplier`，以及破晓EX 的 750/s、-50、×1.1）；
    若哪天用户要"数值回到游戏里"，写进减益的 `Description` 而不是饰品的 tooltip。
- ⚠️ **踩过的坑（重要）**：
  ① 灾厄 2.1 那套 `SpriteBatchScope`（靠读 FNA 的 `beginCalled/sortMode/blendState…` 保存并还原调用方批次参数）
  **在我们这里编译不过**——那些成员在随 tModLoader 分发的 FNA 参考程序集里是 internal；改用灾厄 **2.0.4** 渲染器的做法：
  `End()` 之后用自己的 Begin/End，离场时按原版实体绘制那套标准参数重新 Begin
  （Deferred + AlphaBlend + `Main.DefaultSamplerState` + `Main.Rasterizer` + `Main.GameViewMatrix.TransformationMatrix`），
  这就是 `Common/Graphics/SpriteBatchScope.cs` 现在的实现与理由。
  ② 原版 `Main.DrawNPC` 自身**不开批次**（IL 实查：方法体内没有 SpriteBatch.Begin/End），批次由调用方开着，
  所以钩子入口可以直接 `End()`。
  ③ 玩家侧 `Player.HurtInfo` 没有 `DamageType`（见 §5.3），命中类型仍由调用方从 Item/Projectile 取。
  ④ **加载顺序：内容的 `Load()` 早于 `Mod.Load()`**（2026-10-17 踩点，IL 实查）。每个模组的流程是
  `AutoloadConfig()` → `PrepareAssets()` → **`Autoload()`（本模组所有 ModType / ModSystem 的 Load 都在这一步）** →
  **`Load()`（`Instance = this` 在这一步）** → `SystemLoader.OnModLoad()`。所以**任何 ModType 的 `Load()` 里都不能碰
  `CalamityExpansion.Instance`**：内容加载期它还是 null，一碰就 NRE、整个模组被禁用，而且 `Mod.Load()` 根本不会跑
  （连 `EffectLoader` 的着色器注册都到不了）。取资产直接 `ModContent.Request` / `Mod.Assets` —— 资产仓库由更早的
  `PrepareAssets()` 建好，Load 期可用（灾厄 `MiracleBlight` 那套懒取 `field ??= ModContent.Request<Texture2D>(...)` 也行）。
  工程内其余 `Instance` 用法已逐处复核安全：`EffectLoader`（在 `Mod.Load` 里调）、`BaseHeldProjCE`（运行期）、
  `ConfigSystem.Instance?`（`AutoloadConfig` 阶段已赋值，且全走 `?.`）。

### 5.5 游侠徽章EX（RangerEmblemEX，2026-10-17 第五批）

- **游侠徽章EX**：远程饰品（JobAcc/Ranged）。贴图 = §3.3 第三条那张按星旋盔甲色系染好的
  `RangerEmblemEX.png`（28×28，灰盘不动、银环走盔甲绿、箭身走绿色→青色发光阶）。
  属性（用户 2026-10-17 口述）：**+18% 远程伤害 / +18% 远程暴击率 / +18% 弓箭伤害 /
  +18% 弹药飞行速度 / **82% 概率不消耗弹药**（2026-10-17 当天由 18% 改过来——用户先要 18%，
  后改口「算了改成82%概率不消耗弹药」；旋钮就是 `RangerEmblemEX.AmmoSaveChance`）。
- **落点**（与战士徽章EX 同一套骨架）：
  - 两围直接写 `player.GetDamage / GetCritChance(DamageClass.Ranged)`；
    弓箭伤害走原版 `player.arrowDamage += 0.18f`（`StatModifier`，每帧由 `Player.ResetEffects` 复位）——
    IL 实查 `Player.PickAmmo`：只有 `AmmoID.Sets.IsArrow[ammo.ammo]` 时才会 `CombineWith(arrowDamage)`，
    所以子弹/火箭**不吃**这条；常量是 `RangerEmblemEX.ArrowDamageBonus`。
    并给 `CalamityExpansionPlayer.rangerEmblemEX` 置位（`ResetEffects` 每帧复位）。
    ⚠️ **2026-10-17 用户点名删掉了「远程攻击速度」**（`GetAttackSpeed` 那行已移除、tooltip 那行也删了）——
    注意区分：`弹药飞行速度`（弹幕初速，走 `ModifyShootStats`）保留，`远程射击速度`（攻速）不要。
  - **省弹**走 `ModPlayer.CanConsumeAmmo`（返回 false = 这一发不扣弹药）：原版只有 20%/25% 那几个
    固定档的布尔量（`ammoCost80` / `ammoCost75` / `ammoCost99`），没有任意百分比的口子，所以只能走钩子。
    IL 实查：调用点在 `Player.IsAmmoFreeThisShot`，判定天然只在射击方客户端跑。
  - **弹速**走 `ModPlayer.ModifyShootStats`（`velocity *= 1.18`）。IL 实查：调用点在
    `Player.ItemCheck_Shoot`，位置在初速算好之后、`Projectile.NewProjectile` 之前，正是弹药弹速该改的地方。
    判据 = 装备了本饰品 + `item.useAmmo != AmmoID.None` + `CountsAsClass(DamageClass.Ranged)`
    ——即**只抬消耗弹药的远程武器**，不吃弹药的远程武器与非远程职业不动。
- **配方（2026-10-17 用户改过）**：游侠徽章 `RangerEmblem` + **神圣锭 `HallowedBar` ×15**
  + 光明/暗影/恐惧/力量/视域之魂各 25 @ **秘银砧**（`TileID.MythrilAnvil`）。
  原来的「石巨人之眼 + 星旋碎片 50」已被 15 个神圣锭取代，站台也从远古操纵机改了。
  全是原版材料，不需要软引用。
- **稀有度**：`Content/Rarities/RangerEXRarity.cs`（用户 2026-10-17 指定「稀有度颜色和星旋盔甲的色系对应」）——
  四档全取自带进工程的星旋胸甲像素：深绿 `(24,93,66)` → 绿 `(47,184,131)` → 亮绿 `(34,221,151)` → 青 `(67,255,255)`，
  每帧循环插值，`CycleSpeed = 0.35` 与战士EX 档同节奏。档位显示名 `Rarities.RangerEXRarity.DisplayName`
  （中「游侠EX」/ 英 `Ranger EX`）。
- 价值 15 金（沿用战士徽章EX 口径）；英文名 `Ranger Emblem EX`（沿用 `Warrior Emblem EX` 的命名法）。

### 5.6 狙击镜EX（SniperScopeEX，2026-10-17 第六批）

- **狙击镜EX**：远程饰品（JobAcc/Ranged）。贴图 `SniperScopeEX.png`（34×36 = 17×18 @2×，见 §3.3 第四条：
  按 Crossbow_Scope 的复合造型重绘、配色保持狙击镜原图那套紫灰 + 金环 + 绿镜片）。
  属性（用户 2026-10-17 口述）：**+22% 远程伤害 / +22% 远程暴击率 / +22% 箭矢伤害 /
  +22% 远程弹幕飞行速度 / 40% 概率不消耗弹药 / +50 点远程护甲穿透 / -400 仇恨**；另外三条特殊效果见下。
  其中「箭矢伤害」走原版 `player.arrowDamage += 0.22f`（同游侠徽章EX 那条，只吃箭类弹药）。
- **特殊效果与落点**（全部用**原版弹幕**，本工程不新增任何弹幕——用户 2026-10-17 明确）：
  - **熔岩箭袋 + 潜行者箭袋**（2026-10-17 用户把「魔法箭袋」换成这两个）：
    IL 实查 `Player.ApplyEquipFunctional` 的两个分支——熔岩箭袋 = `magicQuiver = true` +
    `arrowDamage += 0.1f` + `hasMoltenQuiver = true`（`PickAmmo` 读它把箭头点着）；
    潜行者箭袋 = `magicQuiver = true` + `arrowDamage += 0.1f` + `aggro -= 400`。
    本饰品按这两个效果落，**但仇恨只算一次 -400**（否则两个箭袋叠起来是 -800）；
    箭矢伤害两者都吃（合计 +0.2，与同时戴两个箭袋一致）。旋钮：`QuiverArrowDamageBonus` / `AggroReduction`。
  - **侦察镜的缩放**：只在 **`hideVisual == false`**（饰品可见）时 `player.scope = true`。
    `Terraria.Player.scope` 是原版 Rifle/Sniper/Recon Scope 共用的那个缩放开关（已用 Cecil 实查确认字段存在）。
  - **额外夜明箭/弹**：`ModPlayer.Shoot`（**返回 bool，返回 false 会把这一发整个拦掉，所以永远 return true**），
    读 `source.AmmoItemIdUsed` → `ContentSamples.ItemsByType[...].ammo`：
    箭类 50% 额外生成 `ProjectileID.MoonlordArrow`(639)，子弹类 50% 额外生成 `ProjectileID.MoonlordBullet`(638)；
    额外那发与原弹垂直错开 8 像素，避免完全重叠。**是「多射一发」，不是把原弹药换成夜明弹药**
    （用户 2026-10-17 先说要替换、随后改成额外射出一发）。
  - ~~**命中发射星璇火箭**~~ **已于 2026-10-17 按用户要求整体删除**：用户实测「频繁的火箭爆炸太卡了」。
    随之删除的还有：`OnHitNPCWithProj` 里的生成调用、`CalamityExpansionPlayer` 里的
    `SpawnVortexRocketOnRangedHit` / `TrySpawnVortexRocketOnPvP` / `SpawnVortexRocket` 三个方法、
    `CalamityExpansion` 里的整套封包（`CeMessage` / `HandlePacket` / `HandleSniperScopePvPRocket` /
    `HasSniperScopeEquipped`）、`GlobalProjectile.OnHitPlayer` 里那一行调用，以及 tooltip 的那一行。
    **本工程目前没有 `ModPacket` 线路**，`CalamityExpansion.cs` 回到「单例 + 着色器加载」的原始形态。
- **省弹 / 弹速**：与游侠徽章EX 共用 `CalamityExpansionPlayer.CanConsumeAmmo` / `ModifyShootStats`
  （两件同时装备时省弹各自独立掷一次、弹速乘算叠加）。
- **PvP / 多人口径（火箭删除后）**：剩下所有效果都只依赖射击方本地计算——
  省弹、弹速、额外夜明箭/弹、三围加成都在射击方客户端算，额外那发的 owner 就是自己、正常同步；
  魔法箭袋与 `player.scope` 只影响本机手感，设给远端玩家无副作用。
  供以后参考的两条 IL 结论仍然有效：① `Projectile.Damage` 里 `owner != Main.myPlayer` 会跳过整段 NPC 伤害；
  ② `Projectile.NewProjectile` 只在 `owner == Main.myPlayer` 的客户端发包同步；
  ③ `GlobalProjectile.OnHitPlayer` 跑在被打中那名玩家的客户端（`Player.Hurt` 只对本机玩家跑）。
  —— 以后若再要在 PvP 里生成弹幕，仍需走「被打方发请求、服务端生成」那条路。
- **稀有度**：沿用 `RangerEXRarity`（星旋绿青渐变字色）。
- **配方（2026-10-17 用户改过）**：侦察镜 `ReconScope` + **潜行者箭袋 `StalkersQuiver` + 熔岩箭袋 `MoltenQuiver`**
  + 五种魂各 25 + 星旋碎片 50 @ 远古操纵机。
  （用户原话是「魔法箭袋改成潜行者箭袋 + 熔岩箭袋」，而这条配方里本来就没有魔法箭袋这一味，故把两个箭袋直接加进去。）
  ⚠️ **石巨人之眼后来被用户点名删除**——它本身就是侦察镜的配方材料，这条已经有成品侦察镜，再要一次属重复。
- ⚠️ **待用户点名**：① 价值 15 金、英文名 `Sniper Scope EX`；
  ② 额外夜明弹药的伤害沿用本次射击的伤害（没有单独加成）。
- **待实测（2026-10-17）**：① 单人：夜明箭/弹是否 50% 概率双发、缩放是否只有可见时生效、命中时是否不再有火箭；
  ② 多人：额外夜明弹在其它客户端是否可见、卡顿是否消失。

## 6. 工作流

- 编译：`dotnet build -v q -nologo`。**C# 编译本身应为 0 警告 0 错误**（工程警告基线是 0）。
- ⚠️ 该命令末尾还会调 `tModLoader.dll -server -build` 把 `.tmod` 写进 `…\tModLoader\Mods\`；
  该目录在工作区之外，**沙箱内必报 `TML001 Access to the path … denied`，需提权运行**
  （Codex 沙箱把它算作沙箱审批，粒度策略下会被直接拒；要拿到 .tmod 得在非沙箱 shell 里跑这一条）。
  所以判断"代码写对不对"看的是编译器那两行的 0 警告 0 错误，而不是整体退出码。
- **原版行为实查**：`powershell -NoProfile -ExecutionPolicy Bypass -File .\Tools\ReadVanillaIL.ps1 -Type <类型> [-ListMethods|-ListFields|-Method <方法名> -Needle <串>]`
  ——用 Mono.Cecil 读 `tModLoader.dll`（内含合并进来的原版类）。列 API 成员、确认钩子名、看原版某分支的 IL 都靠它，
  比翻源码快且是权威口径。脚本里 tModLoader.dll 路径写死（`D:\Game\Steam\steamapps\common\tModLoader\`），换机器要改；
  文件必须带 **BOM**（PS 5.1 按 ANSI 读无 BOM 的 UTF-8 时，中文注释会把下一行换行吃掉，脚本直接废）。
- 资源自查（**编译不报、只有进游戏才炸，且会禁用整个模组**）：
  - `powershell -ExecutionPolicy Bypass -File .\Tools\CheckImplicitTextures.ps1`（类同名隐式贴图）
  - `powershell -ExecutionPolicy Bypass -File .\Tools\CheckResources.ps1`（`"CalamityExpansion/…"` 字面量与常量拼接两类显式路径）
  两道都应为退出码 0。脚本覆盖不到两类要手工看：`类名 + "Glow"` 这类**后缀拼接**、以及 `.fx` 是否有同名 `.fxc` 背书。
- ⚠️ **加载期坑（2026-10-15 实测）**：`GlobalItem` / `GlobalNPC` / `GlobalProjectile` / `GlobalBuff` 这类
  **全局类只要带实例字段，就必须把 `InstancePerEntity` 覆盖为 true**（`GlobalItem` 另需 `CloneNewInstances => true`）。
  漏了不会编译报错，而是 tML 在 `GlobalType.ValidateType()` 里抛
  `instance fields but InstancePerEntity returns false`，**直接禁用整个模组**并写进 `client.log`
  （本次就是 `CalamityExpansionGlobalProjectile` 的 `defExtraUpdates` 踩到）。
  本工程现有两个全局类（GlobalNPC / GlobalProjectile）都已开 per-entity；
  曾经那个 `GlobalItem`（稀有度体系）已于 2026-10-15 删除（见第 4 节）。
- 本地化：每个语言一份扁平 hjson，顶层 `Buffs` / `Items` / `Projectiles` / `Configs`（另加 tML 自己写的
  `Rarities.X.DisplayName` 这类根级点号键）。玩家可见文本改动
  （物品/弹幕/减益的名称与描述、配置项）**必须同步补两个语言文件**；物品/弹幕默认分类即 "Items"/"Projectiles"。
  **行尾**：`.cs` / `.md` / `Tools\*.ps1` 一律 **LF**（与旧工程 CalamityDemutation 的 CRLF 口径不同，别照旧习惯往回整）。
  例外只有三处：**tML 自己重写的这两份 hjson（它会写成 CRLF，别手动改回去）**、以及手写的 `CalamityExpansion.cs` /
  `build.txt`（CRLF + BOM）。⚠️ 用 apply_patch 改文件时它会把**整个文件**写成 LF —— 改到那几个 CRLF 文件时留神；
  反过来，把 LF 文件整成 CRLF 也是错（本工程 `.cs` 的主流是 LF + BOM）。仓库 `core.autocrlf=true`，
  **行尾变化进不了 `git diff`**，只看 diff 是发现不了的（自查：`[IO.File]::ReadAllText` 里数 `\r\n` 与裸 `\n`）。
  ⚠️ **tML 会重写这两个文件**（2026-10-15 实测）：新增内容/配置项后的那次加载，tML 会把新键补进去——
  **en-US 补的是英文默认值、zh-Hans 补的是 `//` 注释占位**，而且**会把该段已有的手写值一起冲掉**
  （本次 `Configs` 段的中文就被清成了注释占位，进游戏看配置全是英文默认值）。
  处理办法：别改键位结构，只按 tML 生成的那套键（`Configs.<配置类>.<选项>.Label` / `.Tooltip`、
  `Configs.<配置类>.DisplayName`）**把值重新填回去**；填完别再手改行尾。
  **配置项的键**在 `Configs` 段下：`Configs.ConfigSystem.DisplayName` 是配置界面标题，
  每个选项一组 `Configs.ConfigSystem.<选项名>.Label` / `.Tooltip`（本工程现有 `PerformanceMode` 与 `StatInflation` 两项，
  中英都要填）。忘补时 `dotnet build` 只会自动追加英文默认值，中文那边追加的是 `//` 注释占位。
- git：仓库已初始化，远端 `origin = https://github.com/LiFeiAbom/CalamityExpansion`，`master` 跟踪 `origin/master`；
  两批内容都是按 `git add -A` → `git commit` → `git push origin master` 落地的。
  ⚠️ git 的写操作在沙箱内会报 `index.lock: Permission denied`，必须提权执行。

## 7. 未移植 / 待办

- **待用户复核（第一批遗留）**：① 中子锭经典配方的量级——中子之刃要 12 个中子锭，等于一把中子之刃要吃
  12×(七味) + 魔影锭×12（例：金源矿 300）；② 现代分支仍是 `ShadowspecBar`×1 + `MiracleMatter`×1，
  是否也要按经典版那样加码 / 对称。
- **第二批口径（用户 2026-10-15 实测后回「没有问题」，四项全部收下、已结清）**：
  ① 膨胀档 580 → 2900（中子之刃同倍率 ×5）；② 面板源值 580、右键倍率 5.6 / 15.6（CWR main 版口径照收）；
  ③ 配方「中子锭 ×12 @ 德雷顿熔炉」（与中子之刃同档）；④ tooltip 按现实现改的两处措辞也要着
  （CWR 原文的"狙击形态"在本移植里没有模式开关；"伽马射线"原文只挂在蓄力弹上，实际每发命中都落 3 道光束）。
  另：基类把 `Owner` 改成惰性解析那处偏离源实现，也按现状保留（防出生当帧绘制时空引用）。
- **未移植的同系件**：洛希之弦 / 中子长弓（`NeutronBow` 整链，含 `NeutronArrow` 与它自己的爆点）、
  中子脉冲链里 CWR 的 120 发弹匣与装填系统（`BaseFeederGun`，要连按键与配置系统一起搬，本批刻意跳过）。
  中子锭的旧注释里还写着这几件的用量，搬的时候一起处理。
- **实测状态**：**第二批（中子脉冲整链）已由用户 2026-10-15 进游戏实测通过**（回「没有问题」）——
  覆盖持枪位置与 7 帧动画、右键前伸与 `NeutronGun2` 贴图切换、中子弹的引力透镜与两枚爆点的冲击波环
  （这两样是本工程第一次真跑 `NeutronWarp` 着色器，没被加载期禁用，说明 .fx/.fxc 与 helper 这条线是通的）。
  **第一批（中子之刃）那几项的实测结论用户当天没点名，按"未记录"保留**：面板与手感、右键蓄力条位置、
  三枚 IDrawWarp 弹幕的扭曲表现、中子熵变的每秒 7500 掉血与染色——下次会话若提到就问一句，别当成已验过。
- **文案待用户点名的两处**：中子锭的 tooltip（`万物之重，聚合指尖` / `The weight of all things, gathered at your fingertips`）
  是旧工程带来的 CWR 原味文案；中子熵变的描述被我按新名改成「熵变正在吞噬你」/ `Entropy is eating you away`（原文是"虚无正在吞噬你"）。
- **第三批待用户复核（2026-10-16）**：① 破晓EX 的「每秒 750」是否就是本意（中子熵变是 7500/s，差 10 倍，别是漏写一个 0）；
  ② 战士徽章EX 的**价值 15 金**是我按 JobAcc 量级暂定的；③ 破晓EX 的**附加时长 300 帧（5 秒）**是我照原版破晓武器补的；
  ④ 原版「破晓敌人死亡时多炸一批尘 + 100 像素内传染」**已于 2026-10-16 按用户点名复刻落地**（见 §5.3）；
  ⑤ 英文名 `Warrior Emblem EX` / `Daybreak EX` 是我拟的。
- **第三批待实测（多人，2026-10-16）**：联机表现用户还没测过——重点看 ① PvP 施加是否只结算一次（物品路/弹幕路不重复加成）；
  ② 玩家侧每秒 750 与 -50 防御在客机上与主机是否一致；③ 死亡传染是否只在主机/单机生效、客户端不重复传染；
  ④ PvP 下破晓EX 的图标与倒计时是否正常。
- **贴图尺寸口径（本批已按用户素材定，别当成漏做）**：用户给的素材（战士徽章 28×28、日曜胸甲 38×28、破晓图标 32×32）
  **全是 2× 图**（每个像素都是 2×2 方块，与 `BeetleGlove.png` 26×30 = 13×15 翻倍同一口径）。
  本批一律「不缩放、只换色」。若哪天发现游戏里物品图标偏大，就是这条口径要改：把素材减半到 1× 再染色。
- **WIP（未点名，别当成漏做）**：`Content/Items/Accessories/JobAcc/Magic/Sorcerer_Emblem.webp`
  （巫士徽章原图，用户 2026-10-17 放下）**没有对应 .cs、没有染色、没有规格**——下次点名魔法系饰品时就是它。
  四个饰品目录现在的状态：`Melee/` 战士徽章EX + 烈火手套EX（都已完成），
  `Ranged/` 游侠徽章EX + 狙击镜EX（都已完成），`Magic/` 只有这张原图，`Rogue/` `Summon/` 为空。
  `BeetleGlove.png` 与四张参考图（`Crossbow_Scope.png` / `Recon_Scope.webp` / `Ranger_Emblem.webp` /
  `Vortex_armor.webp`）、根目录 `Daybroken.png` 都已在 2026-10-17 按用户要求删掉（要找回从 git 历史取）。
  JobAcc 目录结构沿用姊妹工程 `CalamityDemutation`（Melee / Magic / Ranged / Rogue / Summon）。
- **工程根目录的 `Daybroken.png`**：破晓EX 图标的参考副本，已入库为 `Content/Buffs/NegativeBuffs/DaybreakEX.png`；
  根目录那份用户没点名要删，暂时留着（要清理说一声即可）。
- **图标**（`icon*.png`）仍是 tModLoader 模板图。
- **第四批待实测/待定（2026-10-17）**：① 燃烧IV 的溶解视觉**尚未进游戏实测**——重点看：批次接管是否与其它模组的
  绘制钩子冲突（进不了画面/报 "Begin must be called" 就是这里）、溶解强度是否合适（`uOpacity 0.7` 可调）、
  发光橙红是否够"燃烧"、火花密度是否过密；② 燃烧IV 的时长（现用 300 帧）与英文名 `Hellfire IV`；
  ③ 烈火手套EX 的价值 15 金（沿用徽章口径）；④ PvP 与多人实测（施加只算一次、客机表现一致）。
- **战士EX 档（稀有度）配色可调**：`Content/Rarities/WarriorEXRarity.cs` 顶部的 `GradientColors` 四色与 `CycleSpeed`
  就是全部旋钮（觉得流得太快/太暗就改这两处）；档位显示名在两个语言文件的 `Rarities.WarriorEXRarity.DisplayName`。
- **第五批待复核（2026-10-17，游侠徽章EX）**：① 未进游戏实测——重点看远程三围是否如数生效、
  弹药初速是否真的 ×1.18（视觉上"打得更直更快"）、省弹手感是否**约 5 发里 4 发不扣**（可按 `R` 键看弹药数）；
  ② 「弹药消耗」现实现成 **82% 概率不消耗**（原版没有按比例扣减的口子，只能整发判定）；
  ③ 价值 15 金、英文名 `Ranger Emblem EX`、
  稀有度四色与 `CycleSpeed` 都是沿用/暂定项（`RangerEXRarity` 顶部同样是旋钮）。
