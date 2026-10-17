using CalamityExpansion.Content.Rarities;
using CalamityExpansion.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityExpansion.Content.Items.Accessories.JobAcc.Ranged
{
    /// <summary>
    /// 狙击镜EX（SniperScopeEX） - 远程饰品。贴图是把狙击镜（Recon Scope）按 Crossbow_Scope 的复合造型重绘的
    /// `SniperScopeEX.png`（34×36，紫灰镜筒 + 金环 + 绿镜片，见 AGENTS.md §3.3 第四条）。
    /// 属性（用户 2026-10-17 口述）：**+22% 远程伤害 / +22% 远程暴击率 / +22% 远程弹幕飞行速度 /
    /// 40% 概率不消耗弹药 / +50 点远程护甲穿透**；额外：
    /// ① 魔法箭袋效果；② **饰品可见时**获得侦察镜（Recon Scope）的缩放能力；
    /// ③ 射出箭矢时 50% 概率**额外**射出一发夜明箭，射出子弹时 50% 概率**额外**射出一发夜明弹；
    /// ④ 远程弹幕每命中目标一次，发射一枚星璇火箭。
    /// <para>
    /// 全部用**原版弹幕**，本工程不新增弹幕：夜明箭 <c>ProjectileID.MoonlordArrow</c>(639)、
    /// 夜明弹 <c>ProjectileID.MoonlordBullet</c>(638)、星璇火箭 <c>ProjectileID.VortexBeaterRocket</c>(616)。
    /// </para>
    /// <para>
    /// 落点：三围、魔法箭袋、缩放直接在 <see cref="UpdateAccessory"/>；省弹 / 弹速 / 额外弹药 / 命中火箭
    /// 分别落在 <see cref="CalamityExpansionPlayer"/> 的 <c>CanConsumeAmmo</c> / <c>ModifyShootStats</c> /
    /// <c>Shoot</c> / <c>OnHitNPCWithProj</c>，靠每帧置位的 <c>sniperScopeEX</c> 标记触发。
    /// </para>
    /// </summary>
    internal class SniperScopeEX : ModItem
    {
        /// <summary>远程伤害加成（按比例加，用户 2026-10-17 指定 22%）</summary>
        public const float RangedBonus = 0.22f;
        /// <summary>远程暴击率加成（按百分点加，22 = +22%）</summary>
        public const float RangedCritBonus = 22f;
        /// <summary>远程弹幕飞行速度加成（比例，初速乘 1.22；**不是攻击速度**）</summary>
        public const float ProjectileVelocityBonus = 0.22f;
        /// <summary>不消耗弹药的概率（0.40 = 40%）</summary>
        public const float AmmoSaveChance = 0.40f;
        /// <summary>远程护甲穿透（点，用户 2026-10-17 指定 50）</summary>
        public const float ArmorPenetration = 50f;
        /// <summary>额外射出一发夜明箭 / 夜明弹的概率（0.5 = 50%）</summary>
        public const float ExtraLuminiteChance = 0.5f;
        /// <summary>
        /// 物品基础属性：34×36 的饰品贴图（与 Crossbow_Scope 同画布）。
        /// 稀有度沿用远程系 EX 档 <see cref="RangerEXRarity"/>（星旋绿青渐变字色）。
        /// </summary>
        public override void SetDefaults()
        {
            Item.width = 34;    // 贴图宽（像素）
            Item.height = 36;   // 贴图高（像素）
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 15);   // 沿用 EX 饰品的量级（用户未点名）
            Item.rare = ModContent.RarityType<RangerEXRarity>();
        }
        /// <summary>
        /// 装备时生效：远程三围 + 护甲穿透 + 魔法箭袋 + 可见时的侦察镜缩放，
        /// 并把「已装备」标记交给 ModPlayer 供四个运行时钩子读取
        /// </summary>
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Ranged) += RangedBonus;
            player.GetCritChance(DamageClass.Ranged) += RangedCritBonus;
            player.GetArmorPenetration(DamageClass.Ranged) += ArmorPenetration;
            // 魔法箭袋：原版字段，箭矢伤害/速度加成与箭矢回收都挂在它身上
            player.magicQuiver = true;
            // 侦察镜的缩放能力（原版 Player.scope）：只在饰品显示（未隐藏外观）时给，用户 2026-10-17 指定
            if (!hideVisual)
            {
                player.scope = true;
            }
            player.GetModPlayer<CalamityExpansionPlayer>().sniperScopeEX = true;
        }
        /// <summary>
        /// 配方（照 EX 饰品那套八味结构，底件换成侦察镜 ReconScope）：
        /// 侦察镜 + 石巨人之眼 + 五种魂各 25 + 星旋碎片 50 @ 远古操纵机。
        /// ⚠️ 用户 2026-10-17 没点名配方，这条是按游侠徽章EX 的口径推的，等点名再改。
        /// </summary>
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.ReconScope)      // 侦察镜（原版）
                .AddIngredient(ItemID.EyeoftheGolem)   // 石巨人之眼（原版字段名是小写 the）
                .AddIngredient(ItemID.SoulofLight, 25)
                .AddIngredient(ItemID.SoulofNight, 25)
                .AddIngredient(ItemID.SoulofFright, 25)
                .AddIngredient(ItemID.SoulofMight, 25)
                .AddIngredient(ItemID.SoulofSight, 25)
                .AddIngredient(ItemID.FragmentVortex, 50)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
