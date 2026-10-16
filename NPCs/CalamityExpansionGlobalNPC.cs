using CalamityExpansion.Content.Buffs.NegativeBuffs;
using CalamityExpansion.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityExpansion.NPCs
{
    /// <summary>
    /// 全局 NPC 类：承载本模组在敌怪侧的减益结算，目前两条——
    /// ① 中子熵变（NeutronEntropy，中子之刃弹幕附加）：掉血与染色/星屑；
    /// ② 破晓EX（DaybreakEX，战士徽章EX 的近战命中附加）：掉血、-50 防御、受到伤害 ×1.1，粒子照原版破晓；
    /// ③ 燃烧IV（HellfireIV，烈火手套EX 的近战命中附加）：掉血、-75 防御、受到伤害 ×1.15，视觉复刻灾厄 MiracleBlight。
    /// 两条减益的置位方是各自的 debuff 类，本类只做数值与表现。
    /// 多人：本类的事件在各端都会跑——增益本身靠原版 AddBuff 同步；灼烧粒子各端各画一份（纯视觉）；
    /// 死亡传染在 <see cref="DaybreakEX.DeathEffect"/> 里按原版口径只在非 MultiplayerClient 端做一次。
    /// </summary>
    internal class CalamityExpansionGlobalNPC : GlobalNPC
    {
        // ── 实例字段 ──
        /// <summary>
        /// 中子熵变标记：由 NeutronEntropy debuff 在敌怪侧置位，参与持续伤害与染色/星屑结算
        /// </summary>
        public bool neutronEntropy = false;
        /// <summary>
        /// 破晓EX 标记：由 DaybreakEX debuff 在敌怪侧置位，参与持续伤害、减防/增伤与灼烧粒子结算
        /// </summary>
        public bool daybreakEX = false;
        /// <summary>
        /// 燃烧IV 标记：由 HellfireIV debuff 在敌怪侧置位，参与持续伤害、减防/增伤与溶解绘制（溶解在 HellfireIV 的绘制钩子里）
        /// </summary>
        public bool hellfireIV = false;
        // ── 属性 ──
        /// <summary>
        /// 按实例启用，避免多个 NPC 共享全局状态
        /// </summary>
        public override bool InstancePerEntity => true;
        // ── 生命周期方法 ──
        /// <summary>
        /// tModLoader 的 ResetEffects 钩子：每帧重置 NPC 状态时调用（本类已按实例启用）。
        /// 把两条减益的标记复位，随后由各自的 debuff.Update 在同帧重新置位，保证标记不跨帧残留。
        /// </summary>
        public override void ResetEffects(NPC npc)
        {
            neutronEntropy = false;
            daybreakEX = false;
            hellfireIV = false;
        }
        /// <summary>
        /// tModLoader 的 UpdateLifeRegen 钩子：每帧结算 NPC 生命回复时调用。
        /// 中子熵变（neutronEntropy）先清掉正回复，再 -15000，damage 下限抬到 7500
        /// （用户 2026-10-15 拍板：由 -10000 / damage 5000 提升，血条上显示的即每秒 7500）。
        /// 破晓EX（daybreakEX）同一套换算：-1500 / damage 下限 750（即每秒 750，用户 2026-10-16 指定）。
        /// 燃烧IV（hellfireIV）同样：-1800 / damage 下限 900（即每秒 900，用户 2026-10-17 指定）。
        /// </summary>
        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (neutronEntropy)
            {
                if (npc.lifeRegen > 0)
                {
                    npc.lifeRegen = 0;
                }
                npc.lifeRegen -= 15000;
                if (damage < 7500)
                {
                    damage = 7500;
                }
            }
            if (daybreakEX)
            {
                if (npc.lifeRegen > 0)
                {
                    npc.lifeRegen = 0;
                }
                npc.lifeRegen -= DaybreakEX.BurnLifeRegen;
                if (damage < DaybreakEX.BurnLifeRegen / 2)
                {
                    damage = DaybreakEX.BurnLifeRegen / 2;
                }
            }
            if (hellfireIV)
            {
                if (npc.lifeRegen > 0)
                {
                    npc.lifeRegen = 0;
                }
                npc.lifeRegen -= HellfireIV.BurnLifeRegen;
                if (damage < HellfireIV.BurnLifeRegen / 2)
                {
                    damage = HellfireIV.BurnLifeRegen / 2;
                }
            }
        }
        /// <summary>
        /// tModLoader 的 ModifyIncomingHit 钩子：结算敌怪这次挨打时调用（1.4.4 的破甲走这里——
        /// 原版灵液的 -15 防御就是改 <c>HitModifiers.Defense</c>，不是去改 npc.defense）。
        /// 破晓EX 生效时：防御按 Flat 减 50，最终伤害 ×1.1（即「受到伤害额外增加 10%」）；
        /// 燃烧IV 生效时：防御按 Flat 减 75，最终伤害 ×1.15（即「受到伤害增加 15%」）。
        /// </summary>
        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (daybreakEX)
            {
                modifiers.Defense.Flat -= DaybreakEX.DefenseLoss;
                modifiers.FinalDamage *= DaybreakEX.DamageTakenMultiplier;
            }
            if (hellfireIV)
            {
                modifiers.Defense.Flat -= HellfireIV.DefenseLoss;
                modifiers.FinalDamage *= HellfireIV.DamageTakenMultiplier;
            }
        }
        /// <summary>
        /// tModLoader 的 HitEffect 钩子：敌怪每次挨打时调用（原版的受击特效也走这一路）。
        /// 破晓EX 生效、且这次挨打把敌怪打死（life &lt;= 0）时，照原版破晓炸一批尘并让破晓EX 传染给附近敌怪。
        /// </summary>
        public override void HitEffect(NPC npc, NPC.HitInfo hit)
        {
            if (daybreakEX && npc.life <= 0)
            {
                DaybreakEX.DeathEffect(npc);
            }
        }
        /// <summary>
        /// tModLoader 的 DrawEffects 钩子：NPC 绘制前调用，可修改 drawColor 并生成粒子。
        /// 中子熵变（neutronEntropy）生效时把红色通道压到 100（整体偏青），并在敌怪四周喷射星屑。
        /// 该标记由 NeutronEntropy debuff 在敌怪侧置位。
        /// 破晓EX（daybreakEX）生效时照原版破晓喷橙火尘并加橙色照明，不改 drawColor。
        /// </summary>
        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (neutronEntropy)
            {
                drawColor.R = 100;
                // 仅在本地玩家附近喷射星屑：远处敌人本就不可见，避免多只被侵蚀敌怪挤占全局粒子上限
                if (Vector2.DistanceSquared(npc.Center, Main.LocalPlayer.Center) < 1600f * 1600f)
                    NeutronEntropy.SpanStar(npc, CEUtil.randVr(npc.width / 2));
            }
            if (daybreakEX)
            {
                DaybreakEX.SpawnBurnDust(npc);
                Lighting.AddLight(npc.Center, 1f, 0.3f, 0.1f);
            }
        }
    }
}
