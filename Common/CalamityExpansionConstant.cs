namespace CalamityExpansion
{
    /// <summary>
    /// 本模组资源路径常量与通用常量
    /// </summary>
    public static class CalamityExpansionConstant
    {
        /// <summary>
        /// 遮罩贴图完整路径前缀（配 ModContent.Request 使用）
        /// </summary>
        public const string Masking = "CalamityExpansion/Assets/Masking/";
        /// <summary>
        /// UI 贴图完整路径前缀（配 ModContent.Request 使用）
        /// </summary>
        public const string UI = "CalamityExpansion/Assets/UIs/";
        /// <summary>
        /// 杂项贴图完整路径前缀（配 ModContent.Request 使用）：
        /// 目前是中子脉冲链带来的两支——1×1 白像素（扭曲着色器铺满用）与星形遮罩（大小爆点的贴图键）
        /// </summary>
        public const string ExtraTextures = "CalamityExpansion/Assets/ExtraTextures/";
        /// <summary>
        /// 粒子上限（DRK 粒子系统使用）
        /// </summary>
        public const int MaxParticleCount = 10000;
        /// <summary>
        /// Effect 资源相对路径（配 Mod.Assets.Request 使用，不含模组名前缀）
        /// </summary>
        public const string noEffects = "Effects/";
    }
}
