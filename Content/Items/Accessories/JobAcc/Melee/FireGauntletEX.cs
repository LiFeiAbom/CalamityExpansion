using CalamityExpansion.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityExpansion.Content.Rarities;
namespace CalamityExpansion.Content.Items.Accessories.JobAcc.Melee
{
    /// <summary>
    /// 烈火手套EX（FireGauntletEX） - 月后近战饰品。贴图是用户给的烈火手套贴图整只染成燃烧橙红
    /// （2026-10-17 两轮改口径后定稿：手指/火焰部分原样、把底座灰件按手指那套暖色阶渲染）。
    /// 属性（用户 2026-10-17 指定）：防御 +8、近战伤害/暴击/攻速各 +22%、仇恨 +400、
    /// 近战击退 ×2、近战武器大小 +20%、允许自动挥舞近战武器与鞭子；
    /// 近战命中（物品 / 弹幕 / PvP 三路，与战士徽章EX 同一套施加口）附加燃烧IV。
    /// 配方：烈火手套 + 狂战士手套 + 毁灭者徽章 + 日曜碎片 ×99 + 地狱锭 ×99 @ 远古操纵机。
    /// </summary>
    internal class FireGauntletEX : ModItem
    {
        /// <summary>三围加成幅度（近战伤害与攻速按比例加）</summary>
        public const float MeleeBonus = 0.22f;
        /// <summary>近战暴击率加成（百分点）</summary>
        public const float MeleeCritBonus = 22f;
        /// <summary>防御加成</summary>
        public const int DefenseBonus = 8;
        /// <summary>仇恨值加成</summary>
        public const int AggroBonus = 400;
        /// <summary>近战击退倍率</summary>
        public const float KnockbackMultiplier = 2f;
        /// <summary>近战武器大小加成（作为倍率叠加，+0.2 = 大 20%）</summary>
        public const float MeleeScaleBonus = 0.2f;
        /// <summary>
        /// 物品基础属性：26×30 的饰品贴图（与用户素材同口径）。稀有度与战士徽章EX 共用
        /// <see cref="Content.Rarities.WarriorEXRarity"/>（用户 2026-10-17 指定：名字字样显示橙红渐变），
        /// 价值 15 金（用户未点名，与战士徽章EX 对齐）。
        /// </summary>
        public override void SetDefaults()
        {
            Item.width = 26;    // 贴图宽（像素）
            Item.height = 30;   // 贴图高（像素）
            Item.accessory = true;  // 饰品位
            Item.value = Item.sellPrice(gold: 15);
            Item.rare = ModContent.RarityType<WarriorEXRarity>();
        }
        /// <summary>
        /// 装备时生效：防御/仇恨/自动挥舞直接写玩家字段；三围走伤害类接口；
        /// 击退与武器大小落在 <see cref="CalamityExpansionPlayer.ModifyWeaponKnockback"/> 与
        /// <see cref="CalamityExpansionPlayer.ModifyItemScale"/>（都只对近战生效）。
        /// 自动挥舞用原版字段 <c>autoReuseAllWeapons</c>（原版 CanAutoReuseItem 读它，鞭子也在覆盖内）。
        /// </summary>
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.statDefense += DefenseBonus;
            player.GetDamage(DamageClass.Melee) += MeleeBonus;
            player.GetCritChance(DamageClass.Melee) += MeleeCritBonus;
            player.GetAttackSpeed(DamageClass.Melee) += MeleeBonus;
            player.aggro += AggroBonus;
            player.autoReuseAllWeapons = true;
            player.GetModPlayer<CalamityExpansionPlayer>().fireGauntletEX = true;
        }
        /// <summary>
        /// 配方：烈火手套 + 狂战士手套 + 毁灭者徽章 + 日曜碎片 ×99 + 地狱锭 ×99，在远古操纵机合成。
        /// 五味材料全是原版内容，故不需要 TryGetMod 软引用。
        /// </summary>
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.FireGauntlet)
                .AddIngredient(ItemID.BerserkerGlove)
                .AddIngredient(ItemID.DestroyerEmblem)
                .AddIngredient(ItemID.FragmentSolar, 99)
                .AddIngredient(ItemID.HellstoneBar, 99)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
