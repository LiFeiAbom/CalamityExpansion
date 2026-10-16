using System.Collections.Generic;
using CalamityExpansion.Common.Effects;
using CalamityExpansion.Common.Graphics;
using CalamityExpansion.Common.Graphics.Buffers;
using CalamityExpansion.Content.Particles;
using CalamityExpansion.Content.Particles.Core;
using CalamityExpansion.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityExpansion.Content.Buffs.NegativeBuffs
{
    /// <summary>
    /// 燃烧IV（HellfireIV） debuff：由「烈火手套EX」的近战命中附加（物品 / 弹幕 / PvP 三路，
    /// 与战士徽章EX 的破晓EX 同一套施加口，见 <see cref="CalamityExpansion.Players.CalamityExpansionPlayer"/>）。
    /// 数值：每秒 900 生命（<c>lifeRegen -= 1800</c>）、防御 -75、受到的伤害 +15%——
    /// 敌怪侧在 <see cref="CalamityExpansionGlobalNPC"/>，玩家侧在 CalamityExpansionPlayer。
    /// 视觉：**完全复刻灾厄现代版 MiracleBlight（超位崩解）**，只把颜色换成燃烧橙红——
    /// ① 火花层：照 MiracleBlight.DrawEffects（1/4 概率一枚火尘、1/3 概率一枚火花粒子，四选一色改为橙红族）；
    /// ② 溶解层：本类在 <c>Load</c> 里钩住 <c>Main.DrawNPC</c>，把带标记的敌怪单独画进屏幕空间渲染目标，
    ///    再用 <see cref="EffectLoader.HellfireIVShaderKey"/> 这支着色器画回来（噪声超阈值抹掉、略低一档换成发光色）。
    /// 多人：增益靠 AddBuff 同步；溶解与火花都是纯客户端表现，各端各画；服务端不跑绘制钩子。
    /// </summary>
    internal class HellfireIV : ModBuff
    {
        // ── 数值口径（单一来源：GlobalNPC 与 ModPlayer 都读这里） ──
        /// <summary>掉血档：<c>lifeRegen -= 1800</c>，敌怪与玩家同一套换算（lifeRegen ÷ 2 = 每秒），即每秒 900</summary>
        public const int BurnLifeRegen = 1800;
        /// <summary>防御降低量（Flat）：敌怪走 <c>HitModifiers.Defense</c>，玩家走 <c>Player.statDefense</c></summary>
        public const float DefenseLoss = 75f;
        /// <summary>受到伤害的倍率（1.15 = 额外 +15%）</summary>
        public const float DamageTakenMultiplier = 1.15f;
        /// <summary>本减益附加的默认时长（帧）：与破晓EX 同口径先用 5 秒（用户未点名）</summary>
        public const int Duration = 300;
        // ── 溶解着色器参数 ──
        /// <summary>溶解阈值基准（原版 MiracleBlight 用 0.7，越大抹掉得越多）</summary>
        private const float DissolveOpacity = 0.7f;
        /// <summary>发光边颜色：燃烧橙红（用户 2026-10-17 指定；原版 MiracleBlight 是荧光绿）</summary>
        public static readonly Color GlowColor = new Color(255, 84, 16);
        /// <summary>溶解用的噪声贴图（自灾厄 ExtraTextures/GreyscaleGradients/Neurons 取来入库）</summary>
        public static Asset<Texture2D> NoiseTexture;
        // ── 排除名单 ──
        /// <summary>不参与溶解绘制的敌怪（见 BuildExclusionList 的注释）</summary>
        private static readonly HashSet<int> excludedNPCs = new HashSet<int>();
        /// <summary>排除名单是否已经构建（懒构建：加载期其它模组的 NPC 还没就绪）</summary>
        private static bool exclusionListBuilt;
        /// <summary>
        /// 加载钩子：请求噪声贴图、挂 Main.DrawNPC 钩子（照灾厄 MiracleBlight 的做法）
        /// ⚠️ 贴图请求走 <c>ModContent.Request</c>，**不能**走工程自持的 <c>CalamityExpansion.Instance.Assets</c>——
        /// IL 实查本版 tML 的每模组加载顺序是 <c>Autoload()</c>（所有内容的 Load 在这一步）→ <c>Load()</c>
        /// （<c>Instance = this</c> 在这一步），即内容 Load 期间 <c>Instance</c> 仍为 null，那样写会 NRE 把整个模组禁用；
        /// 而资产仓库由更早的 <c>Mod.PrepareAssets()</c> 建好，所以 Load 期直接用 ModContent/Mod.Assets 取图是安全的。
        /// </summary>
        public override void Load()
        {
            NoiseTexture = ModContent.Request<Texture2D>(CalamityExpansionConstant.ExtraTextures + "Neurons");
            On_Main.DrawNPC += DrawNPCWithHellfireIV;
        }
        /// <summary>
        /// 卸载钩子：摘钩子、清静态引用、把屏幕空间渲染目标池一并释放（热重载必需）
        /// </summary>
        public override void Unload()
        {
            On_Main.DrawNPC -= DrawNPCWithHellfireIV;
            NoiseTexture = null;
            excludedNPCs.Clear();
            exclusionListBuilt = false;
            ScreenspaceTargetPool.Shared.Dispose();
        }
        /// <summary>
        /// 注册为减益：参与 PvP、不随存档保存，专家/大师下延长持续时间
        /// </summary>
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.LongerExpertDebuff[Type] = true;
        }
        /// <summary>
        /// NPC 版 Update：置位 GlobalNPC 标记（供掉血/减防/增伤结算与溶解绘制读取），并按 MiracleBlight 的频次喷火花
        /// </summary>
        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<CalamityExpansionGlobalNPC>().hellfireIV = true;
            SpanSparks(npc);
        }
        /// <summary>
        /// 玩家版 Update：玩家侧的火花表现（数值结算在 CalamityExpansionPlayer 的三个钩子里）
        /// </summary>
        public override void Update(Player player, ref int buffIndex) => SpanSparks(player);
        /// <summary>
        /// 火花层：复刻 <c>MiracleBlight.DrawEffects</c> 的两条——
        /// ① 1/4 概率一枚原版火尘（速度取随机外向小速度），noGravity，scale 0.7~0.85 并随体积微增，颜色覆写成橙红；
        /// ② 1/3 概率一枚火花粒子（本工程用 DRK_Spark 承担原版 GlowSparkParticle 的角色），
        ///    位移带上 <c>entity.velocity * 3</c>，速度取反乘 0.25，寿命 4 帧。
        /// 原版的速度助手 <c>CalamityUtils.RandomVelocity</c> 在工程里没有等价物，这里用「随机方向 × 随机大小」近似。
        /// </summary>
        public static void SpanSparks(Entity entity)
        {
            // 与原版一致：在实体矩形内随机取点，而不是固定中心
            Vector2 spawn = entity.Center + new Vector2(
                Main.rand.NextFloat(-entity.width / 2f, entity.width / 2f),
                Main.rand.NextFloat(-entity.height / 2f, entity.height / 2f));
            Color sparkColor = RandomSparkColor();
            if (Main.rand.NextBool(4))
            {
                Dust dust = Dust.NewDustPerfect(spawn, DustID.OrangeTorch, RandomSpawnVelocity());
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.7f, 0.85f) + 0.0000007f * entity.width * entity.height;
                dust.color = sparkColor;
            }
            if (Main.rand.NextBool(3))
            {
                Vector2 velOffset = RandomSpawnVelocity() * (Main.rand.NextFloat(5f, 9f) + 0.0002f * entity.width * entity.height);
                BaseParticle spark = new DRK_Spark(entity.Center + entity.velocity * 3f + velOffset * 1.5f
                    , -velOffset * 0.25f, false, 4, 0.6f, sparkColor);
                DRKLoader.AddParticle(spark);
            }
        }
        /// <summary>
        /// 火花取色：原版是 Red / MediumTurquoise / Orange / LawnGreen 四选一（彩虹感），
        /// 本工程按用户口径换成燃烧橙红族四选一
        /// </summary>
        private static Color RandomSparkColor() => Main.rand.Next(4) switch
        {
            0 => new Color(255, 46, 0),     // 烈红
            1 => new Color(255, 96, 10),    // 橙红
            2 => new Color(255, 140, 28),   // 橙
            _ => new Color(255, 190, 62),   // 亮橙金
        };
        /// <summary>随机外向小速度（近似原版 CalamityUtils.RandomVelocity(100f, 70f, 150f, 0.04f) 的量级）</summary>
        private static Vector2 RandomSpawnVelocity() => Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1.4f, 3f);
        /// <summary>
        /// 绘制钩子：带燃烧IV 标记的敌怪走"画进渲染目标 → 用 HellfireIV 着色器画回来"，
        /// 其余（含排除名单）直接交给原版。参数与灾厄 MiracleBlight.DrawForNPC 完全一致。
        /// </summary>
        private static void DrawNPCWithHellfireIV(On_Main.orig_DrawNPC orig, Main self, int iNPCIndex, bool behindTiles)
        {
            NPC npc = Main.npc[iNPCIndex];
            if (!npc.GetGlobalNPC<CalamityExpansionGlobalNPC>().hellfireIV || IsExcludedFromDissolve(npc))
            {
                orig(self, iNPCIndex, behindTiles);
                return;
            }
            // 钩子入口批次是开着的（原版 Main.DrawNPC 自己不开批次），故先用 Scope 接管、离场时还原
            using (Main.spriteBatch.Scope())
            {
                using RenderTargetLease lease = ScreenspaceTargetPool.Shared.Rent(Main.instance.GraphicsDevice);
                using (lease.Scope(clearColor: Color.Transparent))
                {
                    // 第一趟：把这只敌怪单独画进屏幕空间目标（不带其它实体）
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp
                        , DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
                    orig(self, iNPCIndex, behindTiles);
                    Main.spriteBatch.End();
                }
                // 第二趟：把目标内容按"溶解后的样子"画回屏幕（屏幕空间，故不带变换）
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp
                    , DepthStencilState.None, RasterizerState.CullCounterClockwise);
                MiscShaderData dissolveShader = GameShaders.Misc[EffectLoader.HellfireIVShaderKey];
                dissolveShader.UseImage1(NoiseTexture);
                dissolveShader.UseColor(GlowColor);
                dissolveShader.UseOpacity(DissolveOpacity);
                dissolveShader.Apply();
                Main.spriteBatch.Draw(lease.Target, Vector2.Zero, Color.White);
                Main.spriteBatch.End();
            }
        }
        /// <summary>
        /// 该敌怪是否跳过溶解绘制。名单与原版 MiracleBlight 一致，两处原因照抄原注释：
        /// ① Ares 的各部件——那套粒子组会和这副视觉互相打架；
        /// ② 月亮领主——本体/手/头分件绘制，先画进目标再画回来会让头跑到脖子后面去。
        /// 灾厄是软依赖，故按名字 TryFind，未装灾厄时只跳过月亮领主。
        /// </summary>
        private static bool IsExcludedFromDissolve(NPC npc)
        {
            if (!exclusionListBuilt)
            {
                BuildExclusionList();
            }
            return excludedNPCs.Contains(npc.type);
        }
        /// <summary>懒构建排除名单（加载期其它模组的 NPC 未必就绪，故推迟到第一次绘制）</summary>
        private static void BuildExclusionList()
        {
            exclusionListBuilt = true;
            excludedNPCs.Add(NPCID.MoonLordCore);
            excludedNPCs.Add(NPCID.MoonLordHand);
            excludedNPCs.Add(NPCID.MoonLordHead);
            if (!ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                return;
            }
            string[] aresParts = { "AresBody", "AresGaussNuke", "AresLaserCannon", "AresPlasmaFlamethrower", "AresTeslaCannon" };
            foreach (string name in aresParts)
            {
                if (calamity.TryFind<ModNPC>(name, out ModNPC aresPart))
                {
                    excludedNPCs.Add(aresPart.Type);
                }
            }
        }
    }
}
