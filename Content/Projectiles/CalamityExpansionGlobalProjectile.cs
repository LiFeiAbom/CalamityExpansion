using Terraria.ModLoader;

namespace CalamityExpansion.Content.Projectiles
{
    /// <summary>
    /// 全局弹幕类：随本次移植一并带过来的只有「原始 extraUpdates 缓存」这一条 ——
    /// <see cref="CalamityExpansion.Utilities.CEUtil.HomeInOnNPC"/> 会在追踪期间临时抬高弹幕的
    /// extraUpdates 让转向更顺滑，需要先在这里记下原值、追踪结束后写回。
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
    }
}
