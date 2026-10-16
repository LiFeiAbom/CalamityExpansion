using CalamityExpansion.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityExpansion.Content.Buffs.NegativeBuffs
{
    /// <summary>
    /// 破晓EX（DaybreakEX） debuff：由「战士徽章EX」的近战命中（物品挥砍与近战弹幕都算）附加，
    /// **敌怪与玩家都生效**（玩家侧由 PvP 命中施加，见 <see cref="CalamityExpansion.Players.CalamityExpansionPlayer"/>）。
    /// 结算分工：置位在本类；敌怪侧数值与表现放在 <see cref="CalamityExpansionGlobalNPC"/>
    /// （掉血、-50 防御、×1.1 最终伤害、灼烧粒子、死亡爆尘与传染）；
    /// 玩家侧数值放在 <c>CalamityExpansionPlayer</c>（UpdateBadLifeRegen / PostUpdateEquips / ModifyHurt），
    /// 玩家侧的粒子与照明在本类的 Update(Player) 里出。
    /// 粒子照原版破晓（BuffID.Daybreak = 189）那一支：DustID.OrangeTorch（158）、3/4 概率、
    /// 速度 ×2.8 再 Y-0.5、scale 3.5、noGravity，外加橙色照明 AddLight(1, 0.3, 0.1)。
    /// 原版口径由 IL 实查得到（NPC.UpdateNPC_BuffApplyVFX / NPC.UpdateNPC_BuffApplyDOTs / NPC.VanillaHitEffect）。
    /// 多人：增益本身靠 AddBuff 的同步（非 quiet）走服务器，数值各自在本地算（玩家生命以本人客户端为准），
    /// 死亡传染只在服务端做一次（照原版 <c>Main.netMode != MultiplayerClient</c> 的判据）。
    /// </summary>
    internal class DaybreakEX:ModBuff
    {
        // ── 数值口径（单一来源：GlobalNPC 与 ModPlayer 都读这里） ──
        /// <summary>
        /// 掉血档：<c>lifeRegen -= 1500</c>；敌怪与玩家同一套换算（lifeRegen ÷ 2 = 每秒），即每秒 750
        /// </summary>
        public const int BurnLifeRegen = 1500;
        /// <summary>
        /// 防御降低量（Flat）：敌怪走 <c>HitModifiers.Defense</c>，玩家走 <c>Player.statDefense</c>
        /// </summary>
        public const float DefenseLoss = 50f;
        /// <summary>
        /// 受到伤害的倍率（1.1 = 额外 +10%）
        /// </summary>
        public const float DamageTakenMultiplier = 1.1f;
        /// <summary>
        /// 死亡传染半径（像素，原版 100）
        /// </summary>
        public const float SpreadRadius = 100f;
        /// <summary>
        /// 死亡传染给附近敌怪的时长（帧，原版 300）
        /// </summary>
        public const int SpreadDuration = 300;
        /// <summary>
        /// 注册为减益：参与 PvP、不随存档保存，且专家/大师模式下延长持续时间
        /// </summary>
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.LongerExpertDebuff[Type] = true;
        }
        /// <summary>
        /// NPC 版 Update：每帧置位 GlobalNPC 标记，供掉血、减防与粒子结算读取
        /// </summary>
        public override void Update(NPC npc, ref int buffIndex) => npc.GetGlobalNPC<CalamityExpansionGlobalNPC>().daybreakEX = true;
        /// <summary>
        /// 玩家版 Update：多人下本类在每个客户端、对每名玩家各跑一遍（与原版灼烧类减益同口径），
        /// 故粒子与照明直接在这里出（每个客户端各画自己那一份，与原版对远程玩家的处理一致）；
        /// 数值结算不在这里做——掉血在 <c>CalamityExpansionPlayer.UpdateBadLifeRegen</c>，
        /// 减防在 <c>PostUpdateEquips</c>，受伤加成在 <c>ModifyHurt</c>。
        /// </summary>
        public override void Update(Player player, ref int buffIndex)
        {
            SpawnBurnDust(player);
            Lighting.AddLight(player.Center, 1f, 0.3f, 0.1f);
        }
        /// <summary>
        /// 原版破晓的灼烧粒子：照抄 NPC.UpdateNPC_BuffApplyVFX 里 daybreak 那一支（3/4 概率喷一枚橙火尘，
        /// 速度乘 2.8 再抬高，另有 1/4 概率让它受重力并缩小一半）。
        /// 参数取 <see cref="Entity"/>，敌怪（GlobalNPC.DrawEffects）与玩家（本类 Update(Player)）共用同一支。
        /// </summary>
        public static void SpawnBurnDust(Entity target)
        {
            if (Main.rand.Next(4) >= 3)
            {
                return;
            }
            Dust dust = Dust.NewDustDirect(new Vector2(target.position.X - 2f, target.position.Y - 2f)
                , target.width + 4, target.height + 4, DustID.OrangeTorch
                , target.velocity.X * 0.4f, target.velocity.Y * 0.4f, 100, default, 3.5f);
            dust.noGravity = true;
            dust.velocity *= 2.8f;
            dust.velocity.Y -= 0.5f;
            if (Main.rand.Next(4) == 0)
            {
                dust.noGravity = false;
                dust.scale *= 0.5f;
            }
        }
        /// <summary>
        /// 原版破晓的死亡表现：照抄 NPC.VanillaHitEffect 里 daybreak 那一支。
        /// 先在死亡位置喷 20 枚随机尘（Torch 6 / SolarFlare 259 / OrangeTorch 158 三选一，
        /// alpha 200、速度 ×1.4、scale 再加一枚 0~1 的随机数），
        /// 再由服务端把破晓传染给 100 像素内、未免疫的敌怪（300 帧）——判据与原版逐条对齐
        /// （active、未免疫、距离 < 100、非 dontTakeDamage、lifeMax > 5、非 friendly、非 townNPC）。
        /// </summary>
        public static void DeathEffect(NPC npc)
        {
            // 原版用 npc.Hitbox.TopLeft() 作为喷尘原点，宽高取 NPC 本体；速度固定 (0, -2.5)
            Rectangle hitbox = npc.Hitbox;
            for (int i = 0; i < 20; i++)
            {
                int dustType = Utils.SelectRandom(Main.rand, DustID.Torch, DustID.SolarFlare, DustID.OrangeTorch);
                Dust dust = Main.dust[Dust.NewDust(hitbox.TopLeft(), npc.width, npc.height, dustType, 0f, -2.5f, 0, default, 1f)];
                dust.alpha = 200;
                dust.velocity *= 1.4f;
                dust.scale += Main.rand.NextFloat();
            }
            // 传染只在服务端（或单机）做一次，客户端不做，避免离屏 NPC 的增益不同步
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                return;
            }
            int buffType = ModContent.BuffType<DaybreakEX>();
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC other = Main.npc[i];
                if (other.active && !other.buffImmune[buffType] && npc.Distance(other.Center) < SpreadRadius
                    && !other.dontTakeDamage && other.lifeMax > 5 && !other.friendly && !other.townNPC)
                {
                    other.AddBuff(buffType, SpreadDuration);
                }
            }
        }
    }
}
