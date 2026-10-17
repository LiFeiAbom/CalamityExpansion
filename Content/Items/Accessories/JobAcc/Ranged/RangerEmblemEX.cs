using CalamityExpansion.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityExpansion.Content.Rarities;
namespace CalamityExpansion.Content.Items.Accessories.JobAcc.Ranged
{
    /// <summary>
    /// 游侠徽章EX（RangerEmblemEX） - 月后远程饰品。贴图是原版游侠徽章按**星旋盔甲**色系染成的
    /// 「灰盘 + 绿环 + 青色能量箭」（2026-10-17，做法见 AGENTS.md §3.3 第三条）。
    /// 属性（用户 2026-10-17 指定）：+18% 远程伤害、+18% 远程暴击率、+18% 远程攻击速度、
    /// +18% 弹药飞行速度、**18% 概率不消耗弹药**。
    /// <para>
    /// 落点：三围直接在本类的 <see cref="UpdateAccessory"/> 里加；「省弹」与「弹速」要到真正消耗/射出那一刻
    /// 才有对象可改，故写在 <see cref="CalamityExpansionPlayer"/> 的 <c>CanConsumeAmmo</c> /
    /// <c>ModifyShootStats</c> 里，靠本饰品每帧置位的标记（<c>rangerEmblemEX</c>）触发。
    /// </para>
    /// <para>
    /// 配方与稀有度都照战士徽章EX 的规格换职业：材料把「战士徽章 → 游侠徽章」「日曜碎片 → 星旋碎片」，
    /// 档位换成同构的 <see cref="RangerEXRarity"/>（星旋绿青渐变字色）。
    /// </para>
    /// </summary>
    internal class RangerEmblemEX : ModItem
    {
        /// <summary>远程伤害与远程攻击速度的加成幅度（按比例加，用户 2026-10-17 指定 18%）</summary>
        public const float RangedBonus = 0.18f;
        /// <summary>远程暴击率加成（按百分点加，18 = +18%）</summary>
        public const float RangedCritBonus = 18f;
        /// <summary>弹药飞行速度加成（比例，射出的弹幕初速乘 1.18）</summary>
        public const float AmmoVelocityBonus = 0.18f;
        /// <summary>不消耗弹药的概率（0.18 = 18%，判定在 CanConsumeAmmo）</summary>
        public const float AmmoSaveChance = 0.18f;
        /// <summary>
        /// 物品基础属性：28×28 的饰品贴图（与用户素材同口径）。稀有度走本工程自带的
        /// <see cref="RangerEXRarity"/>（星旋盔甲色系：深绿 → 绿 → 亮绿 → 青 的渐变字色），
        /// 与战士徽章EX 用战士EX 档完全对称。
        /// </summary>
        public override void SetDefaults()
        {
            Item.width = 28;    // 贴图宽（像素）
            Item.height = 28;   // 贴图高（像素）
            Item.accessory = true;  // 饰品位
            Item.value = Item.sellPrice(gold: 15);   // 价值 15 金（沿用战士徽章EX 的口径）
            Item.rare = ModContent.RarityType<RangerEXRarity>();   // 稀有度：游侠EX 档（星旋绿青渐变字色）
        }
        /// <summary>
        /// 装备时生效：远程三围加成，并把「已装备」标记交给 ModPlayer 供省弹与弹速两个钩子读取
        /// </summary>
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Ranged) += RangedBonus;
            player.GetCritChance(DamageClass.Ranged) += RangedCritBonus;
            player.GetAttackSpeed(DamageClass.Ranged) += RangedBonus;
            player.GetModPlayer<CalamityExpansionPlayer>().rangerEmblemEX = true;
        }
        /// <summary>
        /// 配方（照战士徽章EX 的八味结构换职业）：游侠徽章 + 石巨人之眼 + 五种魂各 25 + **星旋碎片 50**，
        /// 在远古操纵机（原版 LunarCraftingStation）合成。材料与站台全是原版内容，不需要软引用。
        /// </summary>
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.RangerEmblem)
                .AddIngredient(ItemID.EyeoftheGolem)   // 石巨人之眼（原版字段名是小写 the）
                .AddIngredient(ItemID.SoulofLight, 25)
                .AddIngredient(ItemID.SoulofNight, 25)
                .AddIngredient(ItemID.SoulofFright, 25)
                .AddIngredient(ItemID.SoulofMight, 25)
                .AddIngredient(ItemID.SoulofSight, 25)
                .AddIngredient(ItemID.FragmentVortex, 50)   // 星旋碎片（战士徽章EX 用的是日曜碎片）
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
