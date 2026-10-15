using CalamityExpansion.Content.Items.Materials;
using CalamityExpansion.Content.Projectiles.Melee;
using CalamityExpansion.Content.Rarities;
using CalamityExpansion.Systems;
using CalamityExpansion.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityExpansion.Content.Items.Weapons.Melee
{
    /// <summary>
    /// 中子之刃（NeutronGlaive） - 月后终局近战挥砍武器，由中子锭在德雷顿熔炉（DraedonsForge）上打造。
    /// 命名说明：显示名「中子之刃」为本工程口径；大修原版把它译作「中子长戟」，本工程一律不用那个译名。
    /// 每次挥击都会射出 NeutronGlaiveBeam 弹幕，弹幕消亡时再生成 NeutronExplode 扭曲爆点，
    /// 形成「挥砍→光束→中子爆裂」的联动伤害链。
    /// 伤害：面板基础值 **855**；配置「数值膨胀」开启时通过 <see cref="ModifyWeaponDamage"/> 回调到
    /// <see cref="InflatedDamage"/>（**4275**，用户 2026-10-15 指定的膨胀档）。
    /// </summary>
    internal class NeutronGlaive:ModItem
    {
        /// <summary>
        /// 物品外观动画：设置为灵魂类物品，贴图逐帧竖直滚动（6 帧、每 5 tick 换一帧）。
        /// </summary>
        public override void SetStaticDefaults()
        {
            ItemID.Sets.AnimatesAsSoul[Type] = true;                          // 按灵魂类物品处理（浮动/发光表现）
            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(5, 6));// 垂直动画：6 帧、每 5 tick 换一帧
        }
        /// <summary>
        /// 物品基础属性：巨型挥砍武器，14 帧出手、必定发射中子光束弹幕，自定义月后稀有度 16。
        /// 面板基础伤害 855（源值）；膨胀档 4275 由 <see cref="ModifyWeaponDamage"/> 在开关开启时生效。
        /// </summary>
        public override void SetDefaults()
        {
            Item.height = 154;   // 贴图高（像素）
            Item.width = 154;    // 贴图宽（像素）
            Item.damage = 855;   // 面板基础伤害（源值）；数值膨胀开启时由 ModifyWeaponDamage 回调到 4275
            Item.DamageType = DamageClass.Melee;   // 归属近战伤害
            Item.useAnimation = Item.useTime = 14; // 使用动画与冷却同为 14 帧（很快）
            Item.useTurn = true;                   // 挥砍时可转向
            Item.useStyle = ItemUseStyleID.Swing;  // 挥砍式使用
            Item.noMelee = true;                   // 无近战挥砍判定（右键为手持蓄力模式）
            Item.knockBack = 7.5f;                 // 击退力
            Item.UseSound = SoundID.Item60;        // 挥砍音效
            Item.autoReuse = true;                 // 长按可自动连续挥砍
            Item.value = Item.buyPrice(1, 50, 75, 10);   // 价值 1 铂金 50 金 75 银 10 铜
            Item.rare = ModContent.RarityType<NeutronRarity>();   // 中子稀有度（黑 ↔ 浅黑脉动，中子系列专属档）
            Item.crit = 8;                               // 额外暴击率 +8%
            Item.shoot = ModContent.ProjectileType<NeutronGlaiveBeam>();  // 挥砍时发射的弹幕
            Item.shootSpeed = 18f;                       // 弹幕初速度
            // 稀有度走本工程自带的 NeutronRarity（黑 ↔ 浅黑脉动）。
            // 旧工程那套 postMoonLordRarity 体系（GlobalItem）已按用户 2026-10-15 要求**整体删除**——
            // 它会把档号存进物品存档，读回来时用 ModifyTooltips 刷名字颜色，反过来盖掉本工程的新稀有度。
        }
        /// <summary>
        /// 数值膨胀后的面板伤害（用户 2026-10-15 指定：中子之刃 855 → 4275）。
        /// </summary>
        private const float InflatedDamage = 4275f;
        /// <summary>
        /// 当前生效的面板基础伤害：膨胀开关开启时用 <see cref="InflatedDamage"/>，否则维持 <c>Item.damage</c> 的源值 855。
        /// </summary>
        private float BaseDamage => ConfigSystem.StatInflationEnabled ? InflatedDamage : Item.damage;
        /// <summary>
        /// 数值膨胀：把面板基础伤害换成 <see cref="BaseDamage"/>。配置项没有 ReloadRequired，
        /// 故这里每帧重算（而不是只在 SetDefaults 里定一次），游戏内切换即时生效。
        /// 命中派生的右键大爆点（手持弹幕按 <c>Projectile.damage ×10</c> 生成）会跟随面板自动缩放。
        /// </summary>
        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            damage.Base = BaseDamage;
        }
        /// <summary>
        /// 使用条件：左键正常挥砍；右键切换到手持蓄力模式（改音效与无近战/无使用贴图），
        /// 并限制场上只能同时存在一把 NeutronGlaiveHeld
        /// </summary>
        public override bool CanUseItem(Player player)
        {
            Item.noMelee = false;
            Item.noUseGraphic = false;
            Item.UseSound = SoundID.Item60;
            if (player.altFunctionUse == 2)
            {
                Item.noMelee = true;
                Item.noUseGraphic = true;
                Item.UseSound = SoundID.AbigailAttack;
            }
            return player.ownedProjectileCounts[ModContent.ProjectileType<NeutronGlaiveHeld>()] == 0;
        }
        /// <summary>
        /// 允许右键副功能（altFunctionUse == 2）
        /// </summary>
        public override bool AltFunctionUse(Player player)
        {
            return true;
        }
        /// <summary>
        /// 发射逻辑：右键（altFunctionUse == 2）生成手持弹幕 NeutronGlaiveHeld 并拦截默认发射；
        /// 左键走基类默认发射 NeutronGlaiveBeam
        /// </summary>
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<NeutronGlaiveHeld>(), damage, knockback, player.whoAmI);
                return false;
            }
            return base.Shoot(player, source, position, velocity, type, damage, knockback);
        }
        /// <summary>
        /// 近战挥舞特效：仅调用 CEUtil.BetterSwing 修正武器挥舞位置，使巨剑类挥舞更贴合视觉。
        /// </summary>
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            CEUtil.BetterSwing(player);
        }
        /// <summary>
        /// 注册配方：兼容灾厄现代版与经典版，两版所需工作台名字一致但是具体对应不同，故分别注册配方。
        /// 两版均消耗 12 个中子锭。
        /// </summary>
        public override void AddRecipes()
        {
            // 现代版灾厄：德雷顿熔炉（DraedonsForge）
            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                if (calamity.TryFind<ModTile>("DraedonsForge", out ModTile draedonsForge))
                {
                    Recipe recipe = CreateRecipe();
                    recipe.AddIngredient<NeutronBar>(12);
                    recipe.AddTile(draedonsForge.Type);
                    recipe.Register();
                }
            }
            // 经典版灾厄：德雷顿熔炉（DraedonsForge）
            if (ModLoader.TryGetMod("CalamityModClassicPreTrailer", out Mod classic))
            {
                if (classic.TryFind<ModTile>("DraedonsForge", out ModTile classicDraedonsForge))
                {
                    Recipe recipeClassic = CreateRecipe();
                    recipeClassic.AddIngredient<NeutronBar>(12);
                    recipeClassic.AddTile(classicDraedonsForge.Type);
                    recipeClassic.Register();
                }
            }
        }
    }
}
