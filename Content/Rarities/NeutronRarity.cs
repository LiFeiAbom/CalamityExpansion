using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace CalamityExpansion.Content.Rarities
{
    /// <summary>
    /// 中子稀有度（NeutronRarity）—— 中子系列的专属档位，颜色是**黑 ↔ 浅黑 的反复渐变**。
    /// <para>
    /// 取色依据：中子之刃的刀身本体就是**黑色**（贴图 = 黑刃 + 青蓝描边），故档位色直接沿用刀身的黑，
    /// 在纯黑与浅黑之间来回脉动（用户 2026-10-15 拍板）。
    /// </para>
    /// <para>
    /// 侦察备查（2026-10-15，详见 AGENTS.md 第 5.1 节）：大修 0.4.0.1.3 **自己并不定义稀有度**
    /// （源码树里 `ModRarity` 匹配数 = 0），它引用的是灾厄本体那 7 个自定义档；
    /// 中子系在大修那边只有中子星枪口制退器挂了自定义档（灾厄的 `Turquoise` = 青绿 (0, 255, 200)），
    /// 中子之刃 / 中子脉冲 / 洛希之弦 / 中子法杖都还是原版红。本工程不沿用那一味青绿，
    /// 改按刀身本色做黑档（渐变脉动的写法照 CWR main 里 `BrutalRelicRarity` 的 `Color.Lerp + MathF.Sin` 套路）。
    /// </para>
    /// </summary>
    internal class NeutronRarity : ModRarity
    {
        // ── 常量 ──
        /// <summary>渐变暗端：纯黑</summary>
        private static readonly Color DeepBlack = new Color(0, 0, 0);
        /// <summary>渐变亮端：浅黑（暗灰）。觉得名字太暗看不清时，把这个值往上提即可（例：110,110,110）</summary>
        private static readonly Color LightBlack = new Color(70, 70, 70);
        /// <summary>脉动速度：越大闪得越快（一个完整来回约 π/速度 秒）</summary>
        private const float PulseSpeed = 2f;
        // ── 属性 ──
        /// <summary>
        /// 当前脉动色：用 <see cref="Main.GlobalTimeWrappedHourly"/> 取正弦，在纯黑与浅黑之间来回插值。
        /// 该属性每帧被读取，所以物品名/稀有度描边会持续地"反复渐变"。
        /// </summary>
        private static Color PulseColor => Color.Lerp(DeepBlack, LightBlack, MathF.Sin(Main.GlobalTimeWrappedHourly * PulseSpeed) * 0.5f + 0.5f);
        /// <summary>
        /// 中子档配色：黑 ↔ 浅黑脉动。
        /// </summary>
        public override Color RarityColor => PulseColor;
        // 说明：灾厄的 Turquoise 另带一条前缀链（-2 → 红 / -1 → 紫 / +1 → PureGreen / +2 → DarkBlue）。
        // 本工程目前没有与中子档同级的兄弟档可挂，故不覆盖 GetPrefixedRarity（默认恒返回本档），
        // 等后续档位落进来再补这条链。
    }
}
