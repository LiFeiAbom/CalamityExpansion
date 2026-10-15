using CalamityExpansion.Content.Rarities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityExpansion.Content.Items.Materials
{
    /// <summary>
    /// 中子锭（NeutronBar）—— 中子系列武器的高级材料。
    /// 配方按**现代版 / 经典版各一条**（用户 2026-10-15 拍板，见 <c>AddRecipes</c>）：
    /// <para>
    /// 现代版灾厄 = 魔影锭 `ShadowspecBar` ×1 + 奇迹物质 `MiracleMatter` ×1 @ 德雷顿熔炉；
    /// 经典版灾厄 = 整套经典版材料（照本工程星流之刃 `ExoBlade` 的经典分支，其中狱火碎片取 5）
    /// + 魔影锭 `ShadowspecBar` ×1 @ 德雷顿熔炉。
    /// </para>
    /// 目前中子之刃（NeutronGlaive）需要它 ×12；贴图使用灵魂式的垂直逐帧动画。
    /// </summary>
    internal class NeutronBar : ModItem
    {
        /// <summary>
        /// 静态属性：研究解锁数量、以及动画/灵魂类物品标记，使贴图逐帧播放并像"魂"一样浮动。
        /// </summary>
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 9999;                                   // 研究所解锁所需数量
            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(5, 18));// 垂直动画：18 帧、每 5 tick 换一帧
            ItemID.Sets.AnimatesAsSoul[Type] = true;                           // 按灵魂类物品处理（浮动/发光表现）
        }
        /// <summary>
        /// 基础属性：尺寸 25、堆叠 99、中子稀有度（黑 ↔ 浅黑脉动）、售价 1 铂金；设置了挥击使用动画（本身无使用效果）。
        /// </summary>
        public override void SetDefaults()
        {
            Item.width = Item.height = 25;                       // 贴图宽高（像素）
            Item.maxStack = 99;                                  // 最大堆叠
            Item.rare = ModContent.RarityType<NeutronRarity>();  // 稀有度：中子档（黑 ↔ 浅黑脉动，用户 2026-10-15 定：中子系全部改掉）
            Item.value = Terraria.Item.sellPrice(platinum: 1);   // 售价 1 铂金
            Item.useAnimation = Item.useTime = 15;               // 使用动画/间隔（tick）
            Item.useStyle = ItemUseStyleID.Swing;                // 使用样式：挥击
        }
        /// <summary>
        /// 注册合成配方（用户 2026-10-15 拍板）：两版灾厄各注册一条，均在德雷顿熔炉（DraedonsForge）上合成。
        /// 每条分支都要求「本版灾厄已加载 + 材料与站台全部 TryFind 命中」才注册（沿用工程既有写法）。
        /// <para>
        /// 经典分支的材料块取自本工程星流之刃 `ExoBlade` 的经典分支（现代版的 1 个奇迹物质在那边被换成
        /// 梦魇燃料 / 吸热能量 / 夜魇锭 / 暗日碎片 / 狱火碎片 / 幻魂 / 圣金源矿 七味），
        /// 本条再照用户要求补一味魔影锭；其中**狱火碎片取 5**（ExoBlade 处取 3）。
        /// 八味材料与站台均已核对存在于 cal-1.4.2.101 源码。
        /// </para>
        /// </summary>
        public override void AddRecipes()
        {
            // 现代版灾厄：魔影锭 ×1 + 奇迹物质 ×1 @ 德雷顿熔炉
            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity)
                && calamity.TryFind<ModItem>("ShadowspecBar", out ModItem modernShadowspecBar)
                && calamity.TryFind<ModItem>("MiracleMatter", out ModItem modernMiracleMatter)
                && calamity.TryFind<ModTile>("DraedonsForge", out ModTile modernForge))
            {
                Recipe recipe = CreateRecipe();
                recipe.AddIngredient(modernShadowspecBar.Type, 1);   // 魔影锭
                recipe.AddIngredient(modernMiracleMatter.Type, 1);   // 奇迹物质
                recipe.AddTile(modernForge.Type);
                recipe.Register();
            }
            // 经典版灾厄：七味经典材料 + 魔影锭 ×1 @ 德雷顿熔炉
            if (ModLoader.TryGetMod("CalamityModClassicPreTrailer", out Mod classic)
                && classic.TryFind<ModItem>("NightmareFuel", out ModItem classicNightmareFuel)
                && classic.TryFind<ModItem>("EndothermicEnergy", out ModItem classicEndothermicEnergy)
                && classic.TryFind<ModItem>("CosmiliteBar", out ModItem classicCosmiliteBar)
                && classic.TryFind<ModItem>("DarksunFragment", out ModItem classicDarksunFragment)
                && classic.TryFind<ModItem>("HellcasterFragment", out ModItem classicHellcasterFragment)
                && classic.TryFind<ModItem>("Phantoplasm", out ModItem classicPhantoplasm)
                && classic.TryFind<ModItem>("AuricOre", out ModItem classicAuricOre)
                && classic.TryFind<ModItem>("ShadowspecBar", out ModItem classicShadowspecBar)
                && classic.TryFind<ModTile>("DraedonsForge", out ModTile classicForge))
            {
                Recipe recipeClassic = CreateRecipe();
                recipeClassic.AddIngredient(classicNightmareFuel.Type, 5);          // 梦魇燃料
                recipeClassic.AddIngredient(classicEndothermicEnergy.Type, 5);      // 吸热能量
                recipeClassic.AddIngredient(classicCosmiliteBar.Type, 5);           // 夜魇锭
                recipeClassic.AddIngredient(classicDarksunFragment.Type, 5);        // 暗日碎片
                recipeClassic.AddIngredient(classicHellcasterFragment.Type, 5);     // 狱火碎片（ExoBlade 处是 3，本条按用户要求取 5）
                recipeClassic.AddIngredient(classicPhantoplasm.Type, 5);            // 幻魂
                recipeClassic.AddIngredient(classicAuricOre.Type, 25);              // 圣金源矿
                recipeClassic.AddIngredient(classicShadowspecBar.Type, 1);          // 魔影锭
                recipeClassic.AddTile(classicForge.Type);
                recipeClassic.Register();
            }
        }
    }
}
