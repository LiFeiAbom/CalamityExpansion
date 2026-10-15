# CalamityExpansion — 工程记忆（新会话先读这份）

> 本文件是给后续会话/协作者的项目记忆：工程口径、本次移植的清单与裁剪、踩过的坑、当前状态。
> 改动工程时请同步更新它。内容来源（旧工程）是 `..\CalamityDemutation`，它的 `AGENTS.md` 里有更完整的
> 换算表、命名规则与灾厄源码侦察路线（本文件的很多口径都继承自那里）。

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
  （580 是 CWR 源值；2900 是按中子之刃同倍率 ×5 推的，**待用户复核**）。实现照工程既有口径——
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

## 6. 工作流

- 编译：`dotnet build -v q -nologo`。**C# 编译本身应为 0 警告 0 错误**（工程警告基线是 0）。
- ⚠️ 该命令末尾还会调 `tModLoader.dll -server -build` 把 `.tmod` 写进 `…\tModLoader\Mods\`；
  该目录在工作区之外，**沙箱内必报 `TML001 Access to the path … denied`，需提权运行**
  （Codex 沙箱把它算作沙箱审批，粒度策略下会被直接拒；要拿到 .tmod 得在非沙箱 shell 里跑这一条）。
  所以判断"代码写对不对"看的是编译器那两行的 0 警告 0 错误，而不是整体退出码。
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
  **行尾**：本工程全部文件是 **LF**（提取出来的与新建的都一致，与旧工程 CalamityDemutation 的 CRLF 口径不同），
  别照着旧工程的习惯往回整成 CRLF。
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
- **待用户复核（第二批，用户尚未点名）**：
  ① 中子脉冲的膨胀档——按中子之刃同倍率 ×5 暂取 **580 → 2900**，改 `NeutronGun.InflatedDamage` 一处即可；
  ② 面板源值 580、右键两个倍率 5.6 / 15.6（CWR main 版口径）是否照收；
  ③ 配方沿用「中子锭 ×12 @ 德雷顿熔炉」（与中子之刃同档）；
  ④ tooltip 我按现实现改了两处措辞——CWR 原文的"右键切换为狙击形态"（本移植里没有模式开关，只有射速与伤害变化）
  与"伽马射线"只挂在蓄力弹上的说法（中子弹其实**每发**命中都落 3 道光束）。要原味文案的话，
  旧工程 `19ad483^` 的 en-US / zh-Hans hjson 里有原文可对照。
- **未移植的同系件**：洛希之弦 / 中子长弓（`NeutronBow` 整链，含 `NeutronArrow` 与它自己的爆点）、
  中子脉冲链里 CWR 的 120 发弹匣与装填系统（`BaseFeederGun`，要连按键与配置系统一起搬，本批刻意跳过）。
  中子锭的旧注释里还写着这几件的用量，搬的时候一起处理。
- **中子链未进游戏实测**（本机沙箱出不了 .tmod）：待实测项 = 中子之刃面板与手感、右键蓄力条位置、
  三枚 IDrawWarp 弹幕的扭曲表现、中子熵变的每秒 7500 掉血与染色；第二批再加：持枪位置与 7 帧动画、
  右键前伸与 `NeutronGun2` 贴图切换、中子弹的引力透镜与两枚爆点的冲击波环
  （**这两样是本工程第一次真跑 `NeutronWarp` 着色器**）。
- **文案待用户点名的两处**：中子锭的 tooltip（`万物之重，聚合指尖` / `The weight of all things, gathered at your fingertips`）
  是旧工程带来的 CWR 原味文案；中子熵变的描述被我按新名改成「熵变正在吞噬你」/ `Entropy is eating you away`（原文是"虚无正在吞噬你"）。
- **图标**（`icon*.png`）仍是 tModLoader 模板图。
