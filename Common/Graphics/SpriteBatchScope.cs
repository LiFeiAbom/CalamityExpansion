using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
namespace CalamityExpansion.Common.Graphics
{
    /// <summary>
    /// 精灵批次作用域：进入时 End 掉当前批次，离开 using 时**按原版实体绘制的标准参数**重新 Begin。
    /// 专供「拦截某只敌怪的绘制、先把它画进渲染目标再画回来」这种必须自己控制 Begin/End 的场合
    /// （灾厄 MiracleBlight 与我们的 HellfireIV 都是这套做法）。
    /// </summary>
    /// <remarks>
    /// ⚠️ 刻意**不**做「保存调用方那套 Begin 参数再原样还原」：那需要读 SpriteBatch 的
    /// <c>beginCalled / sortMode / blendState ...</c> 等成员，而它们在随 tModLoader 分发的 FNA 参考程序集里是
    /// internal（灾厄 2.1 的 Daybreak 工具链是对着他们自己的 FNA 分支写的，我们这边编译期拿不到）。
    /// 改为照灾厄 2.0.4 渲染器的做法：End 之后用原版画实体那套标准参数重新 Begin
    /// （Deferred + AlphaBlend + <c>Main.DefaultSamplerState</c> + <c>Main.Rasterizer</c> +
    /// <c>Main.GameViewMatrix.TransformationMatrix</c>）。
    /// 使用前提：进入作用域时批次是打开的——原版 <c>Main.DrawNPC</c> 自身不开批次，由调用方开着，故成立。
    /// </remarks>
    internal readonly struct SpriteBatchScope : IDisposable
    {
        /// <summary>被接管的批次</summary>
        private readonly SpriteBatch spriteBatch;
        /// <summary>进入作用域：结束调用方开着的批次，把舞台让给我们的渲染目标通路</summary>
        public SpriteBatchScope(SpriteBatch spriteBatch)
        {
            this.spriteBatch = spriteBatch;
            spriteBatch.End();
        }
        /// <summary>离开作用域：按原版实体绘制的标准参数重新开批次，后续敌怪接着正常画</summary>
        public void Dispose()
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState
                , DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
    /// <summary>精灵批次作用域的取用扩展：<c>using (Main.spriteBatch.Scope()) { ... }</c></summary>
    internal static class SpriteBatchScopeExtensions
    {
        /// <summary>按 <see cref="SpriteBatchScope"/> 的语义接管批次</summary>
        public static SpriteBatchScope Scope(this SpriteBatch spriteBatch) => new(spriteBatch);
    }
}
