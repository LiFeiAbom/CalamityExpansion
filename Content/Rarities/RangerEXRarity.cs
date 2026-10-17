using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
namespace CalamityExpansion.Content.Rarities
{
    /// <summary>
    /// 游侠EX 稀有度（RangerEXRarity）——本工程「远程系 EX 饰品」的专属档位，与战士EX 档（<see cref="WarriorEXRarity"/>）
    /// 一一对应，只是配色从日曜橙红换成**星旋盔甲的绿 → 青**。目前由游侠徽章EX 使用。
    /// <para>
    /// 取色依据（用户 2026-10-17 指定「稀有度颜色和星旋盔甲的色系对应」）：四档全部直接来自
    /// `JobAcc/Ranged/Vortex_armor.webp`（星旋胸甲）的像素——深绿 `(24,93,66)` 是甲片的暗部、
    /// 绿 `(47,184,131)` 是主体甲片色、亮绿 `(34,221,151)` 是甲片高光、青 `(67,255,255)` 是胸口的能量光。
    /// </para>
    /// <para>
    /// 写法与 <see cref="WarriorEXRarity"/>/<see cref="NeutronRarity"/> 同源：每帧读
    /// <see cref="Main.GlobalTimeWrappedHourly"/> 算进度，在四色之间循环插值（口径同灾厄的 MulticolorLerp）。
    /// </para>
    /// </summary>
    internal class RangerEXRarity : ModRarity
    {
        /// <summary>渐变色谱（按顺序循环）：深绿 → 绿 → 亮绿 → 青（全部取自星旋胸甲）</summary>
        private static readonly Color[] GradientColors =
        {
            new Color(24, 93, 66),
            new Color(47, 184, 131),
            new Color(34, 221, 151),
            new Color(67, 255, 255),
        };
        /// <summary>循环速度：与战士EX 档取同一个值，两档流转节奏一致</summary>
        private const float CycleSpeed = 0.35f;
        /// <summary>当前渐变色：按时间取进度，在色谱上循环插值</summary>
        private static Color GradientColor => MulticolorLerp(Main.GlobalTimeWrappedHourly * CycleSpeed % 1f, GradientColors);
        /// <summary>游侠EX 档位配色：星旋绿青渐变（每帧被读取，所以物品名与稀有度描边会持续流动）</summary>
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
        // 说明：与战士EX 档一样不覆盖 GetPrefixedRarity——本工程目前没有与游侠EX 同级的兄弟档可挂。
    }
}
