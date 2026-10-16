using CalamityExpansion.Players;
using Terraria;
using Terraria.ModLoader;
namespace CalamityExpansion.Content.Items
{
    /// <summary>
    /// 全局物品类：目前只承载「战士徽章EX」在 PvP 里对玩家的施加（物品打玩家那条路）。
    /// 弹幕打玩家那条路在 <c>CalamityExpansionGlobalProjectile.OnHitPlayer</c>，两边共用
    /// <see cref="CalamityExpansionPlayer.TryApplyMeleeHitDebuffsOnPvP"/>（战士徽章EX → 破晓EX、烈火手套EX → 燃烧IV）。
    /// 本类**没有实例字段**，所以不需要覆盖 InstancePerEntity（工程记忆第 6 节那条坑只对带实例字段的全局类成立）。
    /// </summary>
    internal class CalamityExpansionGlobalItem : GlobalItem
    {
        /// <summary>
        /// 物品命中玩家（PvP）：交给 ModPlayer 按「装备了战士徽章EX + 这一击算近战」判据施加破晓EX
        /// </summary>
        public override void OnHitPvp(Item item, Player player, Player target, Player.HurtInfo hurtInfo)
        {
            CalamityExpansionPlayer.TryApplyMeleeHitDebuffsOnPvP(player, target, item.DamageType);
        }
    }
}
