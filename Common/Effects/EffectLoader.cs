using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
namespace CalamityExpansion.Common.Effects
{
    /// <summary>
    /// 着色器加载器：请求并托管本模组的 .fx 资源。
    /// 本工程目前带进来两支——EffectsSystem 合成 IDrawWarp 弹幕时用的屏幕扭曲
    /// <see cref="WarpShader"/>，与中子脉冲链带来的中子星扭曲 <see cref="NeutronWarp"/>
    /// （前者负责"把位移场应用成屏幕扭曲"，后者负责"程序化生成位移场"）；
    /// 外加燃烧IV 的精灵溶解 <see cref="HellfireIV"/>（移植自灾厄现代版 Effects/MiracleBlight.fx，本工程改名并改为燃烧橙红）。
    /// 旧工程里的刀光 / 变形球 / 深渊裂隙等着色器属于别的 CWR/CE 内容，未随移植。
    /// </summary>
    /// <remarks>
    /// ⚠️ 本工程的 .fx 不是由 tModLoader 在构建时编译的：每新增一个 .fx，都必须先用 tModLoader 自带的 fxc
    /// 在 Effects/ 目录里就地编译出同名 .fxc，构建才会把它打进 .tmod：
    /// <c>FXC\fxc.exe /nologo /T fx_2_0 /Fo 名字.fxc 名字.fx</c>（会刷一条 X4717 "Effects deprecated" 警告，属正常）。
    /// 少编译这一步的后果不是"贴图不显示"，而是运行期 <c>Mod.Assets.Request&lt;Effect&gt;</c> 直接抛
    /// MissingResourceException、整个模组被 tModLoader 禁用——所以新增着色器后务必确认 Effects/ 下同名的 .fxc 已生成。
    /// </remarks>
    public class EffectLoader
    {
        /// <summary>
        /// 屏幕扭曲着色器，EffectsSystem 合成 IDrawWarp 弹幕的扭曲效果时使用
        /// </summary>
        public static Asset<Effect> WarpShader;
        /// <summary>
        /// 中子星扭曲着色器（移植自 CWR 的 NeutronWarp）：由 NeutronWarpHelper 在
        /// <c>IDrawWarp.Warp()</c> 里按技法（GravitationalLens / ShockwaveRing 等）程序化生成位移场，
        /// 取代旧版"CPU 侧叠几十层遮罩贴图"的写法
        /// </summary>
        public static Asset<Effect> NeutronWarp;
        /// <summary>
        /// 燃烧IV 的精灵溶解着色器（移植自灾厄现代版 Effects/MiracleBlight.fx）：
        /// 由 HellfireIV 减益在它的 DrawNPC 钩子里以 Misc 着色器身份使用，发光边颜色由调用方 UseColor 传燃烧橙红
        /// </summary>
        public static Asset<Effect> HellfireIV;
        /// <summary>
        /// 燃烧IV 溶解着色器在 <c>GameShaders.Misc</c> 里的键（配合 <see cref="HellfireIVShaderPass"/> 使用）
        /// </summary>
        public const string HellfireIVShaderKey = "CalamityExpansion:HellfireIV";
        /// <summary>
        /// 燃烧IV 溶解着色器的 pass 名（Effect/HellfireIV.fx 里那个 pass 的名字）
        /// </summary>
        public const string HellfireIVShaderPass = "HellfireIVPass";
        /// <summary>
        /// 请求加载全部 .fx 着色器资源（异步）：路径前缀取 CalamityExpansionConstant.noEffects（"Effects/"）。
        /// 只持有 Asset 句柄，不在此处取 .Value，真正取值推迟到绘制期
        /// </summary>
        public static void LoadEffects()
        {
            // 用默认的异步加载：ImmediateLoad 会在模组加载期阻塞(日志里的 "blocking on asset loading" 警告)，
            // 改为只持有 Asset，在真正使用(绘制期)时才取 .Value。
            // ⚠️ Effects/WarpShader.fx 由 Effects/WarpShader.fxc 背书（.fxc 不由构建自动生成，需手动编译并随工程提交）
            var assets = CalamityExpansion.Instance.Assets;
            WarpShader = assets.Request<Effect>(CalamityExpansionConstant.noEffects + "WarpShader");
            // ⚠️ 同样需要 Effects/NeutronWarp.fxc 背书（随工程提交的预编译产物，构建不会自动生成）
            NeutronWarp = assets.Request<Effect>(CalamityExpansionConstant.noEffects + "NeutronWarp");
            // ⚠️ 同样需要 Effects/HellfireIV.fxc 背书；编译命令见本类 remarks
            HellfireIV = assets.Request<Effect>(CalamityExpansionConstant.noEffects + "HellfireIV");
            // 注册成 Misc 着色器（照灾厄把 MiracleBlight 注册进 GameShaders.Misc 的做法）：
            // 由 HellfireIV 减益在绘制期取用——UseImage1 传噪声贴图、UseColor 传燃烧橙红、UseOpacity 传阈值基准
            GameShaders.Misc[HellfireIVShaderKey] = new MiscShaderData(HellfireIV, HellfireIVShaderPass);
        }
        /// <summary>
        /// 卸载时把所有着色器 Asset 引用置空，便于热重载回收（注意方法名按既有约定写作 UnLoad）
        /// </summary>
        public static void UnLoad()
        {
            WarpShader = null;
            NeutronWarp = null;
            HellfireIV = null;
        }
    }
}
