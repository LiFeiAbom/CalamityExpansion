using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
namespace CalamityExpansion.Content.Rarities
{
    /// <summary>
    /// 战士EX 稀有度（WarriorEXRarity）——本工程「战士系 EX 饰品」的专属档位：
    /// 战士徽章EX 与烈火手套EX 共用，名字字色是**橙红渐变**（在四档橙红之间循环流动）。
    /// <para>
    /// 取色依据（用户 2026-10-17 指定「名字字样要像颜色对应显示橙红色渐变」）：四档全取自这两件饰品的贴图配色——
    /// 烈火红 `(245,30,18)` 是烈火手套EX 那根手指/剑身的原色，橙红 `(255,96,16)` 是燃烧IV 的发光色，
    /// 暗红 `(200,44,18)` 与亮橙 `(255,160,50)` 分别是两端的暗部与高光。
    /// </para>
    /// <para>
    /// 写法与中子档（<see cref="NeutronRarity"/>）同源：每帧读 <see cref="Main.GlobalTimeWrappedHourly"/> 算进度，
    /// 只是从「两色来回脉动」升级成「多色循环」，循环插值的写法照灾厄的 MulticolorLerp 口径。
    /// </para>
    /// </summary>
    internal class WarriorEXRarity : ModRarity
    {
        /// <summary>渐变色谱（按顺序循环）：暗红 → 烈火红 → 橙红 → 亮橙</summary>
        private static readonly Color[] GradientColors =
        {
            new Color(200, 44, 18),
            new Color(245, 30, 18),
            new Color(255, 96, 16),
            new Color(255, 160, 50),
        };
        /// <summary>循环速度：越大流转越快（一个完整循环约 1/速度 秒）</summary>
        private const float CycleSpeed = 0.35f;
        /// <summary>当前渐变色：按时间取进度，在色谱上循环插值</summary>
        private static Color GradientColor => MulticolorLerp(Main.GlobalTimeWrappedHourly * CycleSpeed % 1f, GradientColors);
        /// <summary>战士EX 档位配色：橙红渐变（每帧被读取，所以物品名与稀有度描边会持续流动）</summary>
        public override Color RarityColor => GradientColor;
        /// <summary>
        /// 多色循环插值（口径同灾厄的 MulticolorLerp）：进度 0~1 映射到整条色谱上，
        /// 落在相邻两色之间做线性插值，末端回到首色形成无缝循环。
        /// </summary>
        private static Color MulticolorLerp(float progress, Color[] colors)
        {
            float scaled = progress * colors.Length;
            int index = (int)scaled % colors.Length;
            int next = (index + 1) % colors.Length;
            return Color.Lerp(colors[index], colors[next], scaled - (int)scaled);
        }
        // 说明：与中子档一样不覆盖 GetPrefixedRarity——本工程目前没有与战士EX 同级的兄弟档可挂，
        // 等后续档位落进来再补那条前缀链。
    }
}
