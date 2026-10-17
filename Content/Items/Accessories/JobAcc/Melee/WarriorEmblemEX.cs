using CalamityExpansion.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityExpansion.Content.Rarities;
namespace CalamityExpansion.Content.Items.Accessories.JobAcc.Melee
{
    /// <summary>
    /// 战士徽章EX（WarriorEmblemEX） - 月后近战饰品。贴图是原版战士徽章染成日曜橙红
    /// （用户 2026-10-16 指定：外圈银环与中心金剑一起换成日曜套装的橙红，取自日曜胸甲色板）。
    /// 属性（用户 2026-10-16 指定）：+18% 近战伤害、+18% 近战暴击率、+18% 近战攻击速度；
    /// 近战命中（物品挥砍或近战弹幕）附加破晓EX，判定在 <see cref="CalamityExpansionPlayer"/>。
    /// 配方：战士徽章 + 石巨人之眼 + 光明/暗影/恐惧/力量/视域之魂各 25 + 日曜碎片 50 @ 远古操纵机。
    /// </summary>
    internal class WarriorEmblemEX:ModItem
    {
        /// <summary>
        /// 三围加成幅度：近战伤害与攻速按比例加，暴击率按百分点加（用户 2026-10-16 指定均为 18）
        /// </summary>
        public const float MeleeBonus = 0.18f;
        /// <summary>
        /// 近战暴击率加成（百分点）
        /// </summary>
        public const float MeleeCritBonus = 18f;
        /// <summary>
        /// 物品基础属性：28×28 的饰品贴图（与用户素材同口径）。稀有度走本工程自带的
        /// <see cref="Content.Rarities.WarriorEXRarity"/>（用户 2026-10-17 指定：名字字样要显示橙红渐变）——
        /// 该档与烈火手套EX 共用，字色在四档橙红之间循环流动，配色取自这两件饰品的贴图。
        /// </summary>
        public override void SetDefaults()
        {
            Item.width = 28;    // 贴图宽（像素）
            Item.height = 28;   // 贴图高（像素）
            Item.accessory = true;  // 饰品位
            Item.value = Item.sellPrice(gold: 15);   // 价值 15 金（照本工程 JobAcc 饰品的量级）
            Item.rare = ModContent.RarityType<WarriorEXRarity>();   // 稀有度：战士EX 档（橙红渐变字色）
        }
        /// <summary>
        /// 装备时生效：近战三围加成，并把「已装备」标记交给 ModPlayer 供命中结算读取
        /// </summary>
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Melee) += MeleeBonus;
            player.GetCritChance(DamageClass.Melee) += MeleeCritBonus;
            player.GetAttackSpeed(DamageClass.Melee) += MeleeBonus;
            player.GetModPlayer<CalamityExpansionPlayer>().warriorEmblemEX = true;
        }
        /// <summary>
        /// 配方（2026-10-17 用户改过）：战士徽章 + **神圣锭 15** + 五种魂各 25，在**秘银砧**（原版 MythrilAnvil）合成。
        /// （原来的「石巨人之眼 + 日曜碎片 50」被 15 个神圣锭取代，站台也从远古操纵机改成秘银砧。）
        /// 材料与站台全是原版内容，故不需要走 TryGetMod 软引用。
        /// </summary>
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.WarriorEmblem)
                .AddIngredient(ItemID.HallowedBar, 15)   // 神圣锭 ×15（取代原来的石巨人之眼 + 日曜碎片 50）
                .AddIngredient(ItemID.SoulofLight, 25)
                .AddIngredient(ItemID.SoulofNight, 25)
                .AddIngredient(ItemID.SoulofFright, 25)
                .AddIngredient(ItemID.SoulofMight, 25)
                .AddIngredient(ItemID.SoulofSight, 25)
                .AddTile(TileID.MythrilAnvil)   // 秘银砧（原为远古操纵机）
                .Register();
        }
    }
}
