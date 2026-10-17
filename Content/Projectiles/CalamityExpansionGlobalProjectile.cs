using Terraria.ModLoader;

using CalamityExpansion.Players;
using Terraria;
namespace CalamityExpansion.Content.Projectiles
{
    /// <summary>
    /// 全局弹幕类：随本次移植一并带过来的只有「原始 extraUpdates 缓存」这一条 ——
    /// <see cref="CalamityExpansion.Utilities.CEUtil.HomeInOnNPC"/> 会在追踪期间临时抬高弹幕的
    /// extraUpdates 让转向更顺滑，需要先在这里记下原值、追踪结束后写回。
    /// 另有「破晓EX 的 PvP 施加」一条：玩家持有的近战弹幕打中玩家时，按战士徽章EX 的判据附加破晓EX。
    /// </summary>
    internal class CalamityExpansionGlobalProjectile : GlobalProjectile
    {
        // ── 实例字段 ──
        /// <summary>
        /// 弹幕"原始 extraUpdates"缓存：-1 表示尚未记录。
        /// ProjUtil 需要临时改动 extraUpdates 时会先缓存原值，用完再写回，避免叠加修改后无法还原。
        /// </summary>
        public int defExtraUpdates = -1;
        // ── 属性 ──
        /// <summary>
        /// 弹幕按实例保存状态，故开启 per-entity。
        /// ⚠️ 本类是实例字段，**必须**返回 true：漏了会在加载期被
        /// <c>GlobalType.ValidateType()</c> 判为非法（"instance fields but InstancePerEntity returns false"）
        /// 并直接禁用整个模组——2026-10-15 实测踩过。
        /// </summary>
        public override bool InstancePerEntity
        {
            get
            {
                return true;
            }
        }
        /// <summary>
        /// 弹幕命中玩家（PvP）：先挡掉敌方弹幕（<c>hostile</c>）与 owner 越界的情况，
        /// 再把施加交给 <see cref="CalamityExpansionPlayer.TryApplyDaybreakEXOnPvP"/>
        /// （内含近战分类过滤与「装备了战士徽章EX」判据）。
        /// 随后交给 <see cref="CalamityExpansionPlayer.TrySpawnVortexRocketOnPvP"/>：
        /// 狙击镜EX 的「远程命中发射星璇火箭」在 PvP 下走同一条判据。
        /// <para>
        /// ⚠️ 这条钩子的调用链是 <c>Player.Hurt</c> → <c>CombinedHooks.OnHitByProjectile</c> →
        /// <c>ProjectileLoader.OnHitPlayer</c>（2026-10-17 IL 实查），即**在被打中那名玩家的客户端**结算；
        /// 而 <c>Player.Hurt</c> 只对本机玩家跑，所以每次 PvP 命中只会有一个客户端生成火箭，不会重复。
        /// </para>
        /// </summary>
        public override void OnHitPlayer(Projectile projectile, Player target, Player.HurtInfo info)
        {
            if (projectile.hostile || projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
            {
                return;
            }
            Player attacker = Main.player[projectile.owner];
            CalamityExpansionPlayer.TryApplyMeleeHitDebuffsOnPvP(attacker, target, projectile.DamageType);
            CalamityExpansionPlayer.TrySpawnVortexRocketOnPvP(attacker, target, projectile);
        }
    }
}
