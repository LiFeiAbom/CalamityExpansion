using CalamityExpansion.Content.Buffs.NegativeBuffs;
using CalamityExpansion.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityExpansion.NPCs
{
    /// <summary>
    /// 全局 NPC 类：目前只承载「中子熵变（NeutronEntropy）」这一条来自中子之刃弹幕的减益结算。
    /// 该减益的置位方是 <see cref="NeutronEntropy"/>，本类负责掉血与染色/星屑表现。
    /// </summary>
    internal class CalamityExpansionGlobalNPC : GlobalNPC
    {
        // ── 实例字段 ──
        /// <summary>
        /// 中子熵变标记：由 NeutronEntropy debuff 在敌怪侧置位，参与持续伤害与染色/星屑结算
        /// </summary>
        public bool neutronEntropy = false;
        // ── 属性 ──
        /// <summary>
        /// 按实例启用，避免多个 NPC 共享全局状态
        /// </summary>
        public override bool InstancePerEntity => true;
        // ── 生命周期方法 ──
        /// <summary>
        /// tModLoader 的 ResetEffects 钩子：每帧重置 NPC 状态时调用（本类已按实例启用）。
        /// 把中子熵变标记复位，随后由 NeutronEntropy.Update 在同帧重新置位，保证标记不跨帧残留。
        /// </summary>
        public override void ResetEffects(NPC npc)
        {
            neutronEntropy = false;
        }
        /// <summary>
        /// tModLoader 的 UpdateLifeRegen 钩子：每帧结算 NPC 生命回复时调用。
        /// 中子熵变（neutronEntropy）先清掉正回复，再 -15000，damage 下限抬到 7500
        /// （用户 2026-10-15 拍板：由 -10000 / damage 5000 提升，血条上显示的即每秒 7500）。
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
        }
        /// <summary>
        /// tModLoader 的 DrawEffects 钩子：NPC 绘制前调用，可修改 drawColor 并生成粒子。
        /// 中子熵变（neutronEntropy）生效时把红色通道压到 100（整体偏青），并在敌怪四周喷射星屑。
        /// 该标记由 NeutronEntropy debuff 在敌怪侧置位。
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
        }
    }
}
