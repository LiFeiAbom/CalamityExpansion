using CalamityExpansion.Content.Items.Materials;
using CalamityExpansion.Content.Projectiles.Ranged;
using CalamityExpansion.Content.Rarities;
using CalamityExpansion.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityExpansion.Content.Items.Weapons.Ranged
{
    /// <summary>
    /// 中子脉冲（NeutronGun） - 月后终局远程武器，由中子锭在灾厄德雷顿熔炉上打造（移植自 CWR 0.5.0.1.7）。
    /// 物品本体既不出图也不挥砍，左右键都只负责把持握弹幕 <see cref="NeutronGunHoldout"/> 拿出来；
    /// 瞄准、开火、蓄力、充能条全由该弹幕自行结算。
    /// 伤害：面板基础值 **580**；配置「数值膨胀」开启时通过 <see cref="ModifyWeaponDamage"/> 回调到
    /// <see cref="InflatedDamage"/>（本工程口径为中子之刃同倍率 ×5，**待用户复核**）。
    /// </summary>
    internal class NeutronGun : ModItem
    {
        /// <summary>
        /// 物品外观动画：按灵魂类物品处理（浮动/发光），贴图竖直滚动 7 帧、每 5 tick 换一帧
        /// </summary>
        public override void SetStaticDefaults()
        {
            ItemID.Sets.AnimatesAsSoul[Type] = true;                          // 按灵魂类物品处理
            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(5, 7));// 垂直动画：7 帧、每 5 tick 换一帧
        }
        /// <summary>
        /// 物品基础属性：伤害 580，其余照搬 CWR（射速 5 帧、暴击 +2%、消耗子弹）。
        /// channel + noUseGraphic + useStyle = Shoot：本体不出图也不挥砍，靠 channel 维持持握弹幕常驻；
        /// useAmmo 供持握弹幕里的 Player.PickAmmo 识别弹药类别（本武器会把任何子弹强制转成中子弹）。
        /// </summary>
        public override void SetDefaults()
        {
            Item.width = Item.height = 34;
            Item.damage = 580;                       // 面板基础伤害（源值）；数值膨胀开启时由 ModifyWeaponDamage 回调
            Item.DamageType = DamageClass.Ranged;
            Item.useAnimation = Item.useTime = 5;
            Item.knockBack = 1.5f;
            Item.shootSpeed = 12f;
            Item.useAmmo = AmmoID.Bullet;
            Item.rare = ModContent.RarityType<NeutronRarity>();   // 中子稀有度（黑 ↔ 浅黑脉动，中子系列专属档）
            Item.value = Item.buyPrice(13, 83, 5, 0);
            Item.crit = 2;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.shoot = ModContent.ProjectileType<NeutronGunHoldout>();
            // 稀有度走本工程自带的 NeutronRarity。旧工程那套 postMoonLordRarity 体系（GlobalItem）
            // 已按用户要求整体删除——它会把档号存进物品存档，读回来时反过来盖掉新稀有度。
        }
        /// <summary>
        /// 数值膨胀后的面板伤害。中子之刃的口径是「源值 → ×5」（855 → 4275），本件按同倍率取 580 → 2900，
        /// **待用户复核**：他若点名别的档位，改这一处即可（武器与右键弹都是按面板放缩的，无需另改）。
        /// </summary>
        private const float InflatedDamage = 2900f;
        /// <summary>
        /// 当前生效的面板基础伤害：膨胀开关开启时用 <see cref="InflatedDamage"/>，否则维持 <c>Item.damage</c> 的源值 580。
        /// </summary>
        private float BaseDamage => ConfigSystem.StatInflationEnabled ? InflatedDamage : Item.damage;
        /// <summary>
        /// 数值膨胀：把面板基础伤害换成 <see cref="BaseDamage"/>。配置项没有 ReloadRequired，
        /// 故这里每帧重算（而不是只在 SetDefaults 里定一次），游戏内切换即时生效。
        /// 左键/右键射出的中子弹与右键大爆点都按 <c>Projectile.damage</c> 派生，会跟着面板自动缩放。
        /// </summary>
        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            damage.Base = BaseDamage;
        }
        /// <summary>
        /// 允许右键副功能：左右键都要能把持握弹幕拿出来（具体打法是弹幕自己按鼠标键区分的）
        /// </summary>
        public override bool AltFunctionUse(Player player) => true;
        /// <summary>
        /// 持握弹幕已在场时不再允许"再次使用"，其后由 channel 维持（与中子之刃同款闸门）
        /// </summary>
        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;
        /// <summary>
        /// 左右键共用同一个持握弹幕，故两侧都在这里手动生成、并拦截默认发射。
        /// 不赌 tML 的 channel 生成机制——本武器左右键共用一件物品，channel 状态不可靠
        /// </summary>
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<NeutronGunHoldout>()] < 1)
                Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<NeutronGunHoldout>(), damage, knockback, player.whoAmI);
            return false;
        }
        /// <summary>
        /// 注册配方：与中子之刃同档 —— 中子锭×12 + 德雷顿熔炉，兼容灾厄现代版与经典版
        /// （两版熔炉同名不同物，故分别注册）
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
