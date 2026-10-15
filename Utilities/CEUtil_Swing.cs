using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
namespace CalamityExpansion.Utilities
{
    /// <summary>
    /// 通用工具类（挥砍武器部分）：提供手持/挥砍弹幕所需的纹理、帧动画与向量扩展。
    /// </summary>
    internal static partial class CEUtil
    {
        /// <summary>
        /// 获取玩家当前“实际持有”的物品（对应 CWR 的 Player.ActiveItem()，即灾厄的
        /// GeneralExtensionUtils.ActiveItem）：鼠标上拿着东西时返回鼠标物品，否则返回手持物品。
        /// </summary>
        public static Item ActiveItem(this Player player) => Main.mouseItem.IsAir ? player.HeldItem : Main.mouseItem;
        /// <summary>
        /// 检测玩家是否存活活跃（对应 CWR 的 Alives）
        /// </summary>
        public static bool Alives(this Player player) => player != null && player.active && !player.dead;
        /// <summary>
        /// 整帧间隔切换动画帧，到达最大帧后回绕
        /// </summary>
        public static void ClockFrame(ref int frameCounter, int intervalFrame, int maxFrame)
        {
            if (Main.GameUpdateCount % intervalFrame == 0)
            {
                frameCounter++;
            }
            if (frameCounter > maxFrame)
            {
                frameCounter = 0;
            }
        }
        // 说明：CalamityDemutation 时代的 CEUtil 里还有 player.CD() / projectile.CD() 两个扩展
        // （返回本模组 ModPlayer / GlobalProjectile）。本工程尚未移植那套玩家与弹幕全局数据类，
        // 中子之刃也不需要它们，故随本次移植一并省去。
        /// <summary>
        /// 返回向量的法线（垂直）向量
        /// </summary>
        public static Vector2 GetNormalVector(this Vector2 vr)
        {
            Vector2 nVr = new(vr.Y, -vr.X);
            return Vector2.Normalize(nVr);
        }
        /// <summary>
        /// 按总帧数计算单帧缩放中心（对应 CWR 的 GetOrig）
        /// </summary>
        public static Vector2 GetOrig(Texture2D value, int frameCounterMax = 1)
        {
            float singleFrameY = value.Height / frameCounterMax;
            return new Vector2(value.Width * 0.5f, singleFrameY / 2);
        }
        /// <summary>
        /// 获取玩家稳定中心（对应 CWR 的 GetPlayerStabilityCenter）
        /// </summary>
        public static Vector2 GetPlayerStabilityCenter(this Player player) => player.MountedCenter.Floor() + new Vector2(0, player.gfxOffY);
        /// <summary>
        /// 按帧数与当前帧计算竖直切片矩形（用于多帧贴图取帧）
        /// </summary>
        public static Rectangle GetRec(Texture2D value, int frame, int frameCounterMax = 1)
        {
            int singleFrameY = value.Height / frameCounterMax;
            return new Rectangle(0, singleFrameY * frame, value.Width, singleFrameY);
        }
        /// <summary>
        /// 按资源路径加载并返回一张 Texture2D；asyncLoad 为 true 时改用异步加载（默认同步立即加载，
        /// 以保证调用方拿到返回值时贴图已就绪）
        /// </summary>
        public static Texture2D GetT2DValue(string texture, bool asyncLoad = false)
        {
            return ModContent.Request<Texture2D>(texture, asyncLoad ? AssetRequestMode.AsyncLoad : AssetRequestMode.ImmediateLoad).Value;
        }
        /// <summary>
        /// 从贴图提取调色板：遍历全部像素，收集"非透明且非纯黑非纯白"的颜色，按像素顺序返回。
        /// 供弹幕按寿命在做多段颜色插值时使用（配合 <see cref="MulticolorLerp"/>）。
        /// </summary>
        public static Color[] GetColorDate(Texture2D tex)
        {
            Color[] colors = new Color[tex.Width * tex.Height];
            tex.GetData(colors);
            List<Color> nonTransparentColors = new List<Color>();
            foreach (Color color in colors)
            {
                if ((color.A > 0 || color.R > 0 || color.G > 0 || color.B > 0) && color != Color.White && color != Color.Black)
                {
                    nonTransparentColors.Add(color);
                }
            }
            return nonTransparentColors.ToArray();
        }
        /// <summary>
        /// 检测玩家是否按下左/右键（对应 CWR 的 PressKey，netCed 为 true 时仅在本地玩家生效）
        /// </summary>
        public static bool PressKey(this Player player, bool leftCed = true, bool netCed = true) => (!netCed || Main.myPlayer == player.whoAmI) && (leftCed ? PlayerInput.Triggers.Current.MouseLeft : PlayerInput.Triggers.Current.MouseRight);
        /// <summary>
        /// 随机方向单位向量乘以 0~max 随机长度（对应 CWR 的 randVr）
        /// </summary>
        public static Vector2 randVr(int max)
        {
            return Main.rand.NextVector2Unit() * Main.rand.Next(0, max);
        }
        /// <summary>
        /// 随机方向单位向量乘以 min~max 随机长度（对应 CEUtils 的同名双参重载）
        /// </summary>
        public static Vector2 randVr(int min, int max)
        {
            return Main.rand.NextVector2Unit() * Main.rand.Next(min, max);
        }
        /// <summary>
        /// 把二维向量提升为 Z 分量为 0 的三维向量
        /// </summary>
        public static Vector3 Vec3(this Vector2 vector) => new Vector3(vector.X, vector.Y, 0);
    }
}
