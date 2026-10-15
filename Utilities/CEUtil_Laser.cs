using Terraria;

namespace CalamityExpansion.Utilities
{
    /// <summary>
    /// 通用工具类（激光 / 手持弹幕部分）。
    /// 旧工程对应文件是 <c>Utilities/CDUtil_Laser.cs</c>，本工程**只带了中子脉冲链要用到的
    /// <see cref="CantUseHoldout"/> 一个成员**：
    /// 那支文件里的 <c>DrawAfterimagesCentered</c> 在本工程已有等价物（<c>CEUtil.DrawAfterimages</c>），
    /// <c>PreciseDistanceToTileCollisionHit</c> 与 <c>AnyProjectiles</c> 目前没有移植件消费。
    /// 后续哪件内容要用到它们，再照旧工程补进来即可。
    /// </summary>
    internal static partial class CEUtil
    {
        /// <summary>
        /// 玩家是否无法继续使用手持弹幕（移植自灾厄 PlayerUtils.CantUseHoldout）：
        /// 玩家无效/死亡/被控/物品栏被锁，或 <paramref name="needsToHold"/> 为真且已松开使用键时返回 true。
        /// <para>
        /// 左右键共用一件物品的持握弹幕（中子脉冲）不能沿用 <c>player.channel</c> 判据——那种情况下
        /// channel 状态不可靠，故传 <c>needsToHold: false</c>，改由调用方自己看两个鼠标键。
        /// </para>
        /// </summary>
        public static bool CantUseHoldout(this Player player, bool needsToHold = true) => player == null || !player.active || player.dead || (!player.channel && needsToHold) || player.CCed || player.noItems;
    }
}
