using Terraria.Audio;

namespace CalamityExpansion.Sounds
{
    /// <summary>
    /// 本模组的自定义音效表（对应旧工程的 <c>Sounds/CalamityDemutationSounds.cs</c>）。
    /// 旧工程那张表有 40 多条，本工程按"只带移植件真正消费的"口径逐批补——中子之刃链一个都没用到，
    /// 中子脉冲链带来两条，故目前只有这两条。
    /// </summary>
    internal static class CalamityExpansionSounds
    {
        /// <summary>中子脉冲开火（CWR 原名 AWP_Shoot）：NeutronGunHoldout 每次射出中子弹时播放，音高音量按左右键区分</summary>
        public static readonly SoundStyle Gun_AWP_Shoot = new("CalamityExpansion/Sounds/Item/Gun_AWP_Shoot");
        /// <summary>蓄力完成（CWR 原名 Pecharge）：中子脉冲充能满 80 触发大爆点时播放</summary>
        public static readonly SoundStyle Pecharge = new("CalamityExpansion/Sounds/Item/Pecharge");
    }
}
