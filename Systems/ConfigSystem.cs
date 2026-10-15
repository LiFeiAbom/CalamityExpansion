using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace CalamityExpansion.Systems
{
    /// <summary>
    /// 模组配置类（客户端侧）。两项开关：
    /// <list type="bullet">
    /// <item>性能模式：<c>CEUtil.DrawAfterimages</c> 等绘制工具的开关（随第一批内容一并带过来）。</item>
    /// <item>数值膨胀：把本模组武器的面板回调到用户点名的"膨胀档"（目前消费方是中子之刃）。</item>
    /// </list>
    /// </summary>
    internal class ConfigSystem : ModConfig
    {
        /// <summary>
        /// 当前生效的配置单例，由 OnLoaded 在配置载入后填充，供全局读取
        /// </summary>
        public static ConfigSystem Instance;
        /// <summary>
        /// 配置作用域：客户端侧，仅影响本机显示/性能类选项，不参与多人同步
        /// </summary>
        public override ConfigScope Mode => ConfigScope.ClientSide;
        /// <summary>
        /// 性能模式开关：启用后禁用弹幕残影/拖尾渲染，
        /// 在弹幕数量庞大时可显著降低绘制开销（详见 DrawAfterimages 的实现）。
        /// </summary>
        [BackgroundColor(211, 211, 211, 192)]
        [DefaultValue(false)]
        public bool PerformanceMode { get; set; }
        /// <summary>
        /// 数值膨胀开关：启用后，本模组把**已接入的武器**回调到用户逐一点名的"膨胀档"数值。
        /// 消费方是武器自己在代码里读 <see cref="StatInflationEnabled"/> 决定面板回调多少
        /// （中子之刃：源值 855 → 膨胀档 4275，见其 <c>ModifyWeaponDamage</c>）。
        /// 本项没有 `ReloadRequired`，游戏内切换即时生效。
        /// </summary>
        [BackgroundColor(211, 211, 211, 192)]
        [DefaultValue(false)]
        public bool StatInflation { get; set; }
        /// <summary>
        /// 数值膨胀是否启用的静态便捷入口：武器侧统一读这一处，省掉每处判空
        /// （配置尚未载入时按"未启用"处理）。
        /// </summary>
        public static bool StatInflationEnabled => Instance?.StatInflation ?? false;
        /// <summary>
        /// 配置加载完成钩子：由 tModLoader 在配置读入后自动调用，缓存配置单例
        /// </summary>
        public override void OnLoaded()
        {
            Instance = this;
        }
    }
}
