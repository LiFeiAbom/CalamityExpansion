using CalamityExpansion.Content.Buffs.NegativeBuffs;
using Terraria;
using Terraria.ModLoader;
using CalamityExpansion.Content.Items.Accessories.JobAcc.Melee;
using Terraria.ID;
using CalamityExpansion.Content.Items.Accessories.JobAcc.Ranged;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
namespace CalamityExpansion.Players
{
    /// <summary>
    /// 本工程的 ModPlayer：承载「战士徽章EX」的施加与「破晓EX」的玩家侧结算。
    /// 施加：敌怪侧写在 OnHitNPCWithItem / OnHitNPCWithProj（物品与弹幕两条路）；
    /// 玩家侧（PvP）由 <see cref="TryApplyDaybreakEXOnPvP"/> 统一处理，调用方是
    /// <c>CalamityExpansionGlobalItem.OnHitPvp</c>（物品打玩家）与
    /// <c>CalamityExpansionGlobalProjectile.OnHitPlayer</c>（弹幕打玩家）。
    /// 结算（玩家身上带着破晓EX 时）：掉血 UpdateBadLifeRegen、减防 PostUpdateEquips、
    /// 受伤加成 ModifyHurt；粒子与照明在 <see cref="DaybreakEX.Update(Player, ref int)"/> 里出。
    /// </summary>
    internal class CalamityExpansionPlayer:ModPlayer
    {
        /// <summary>
        /// 玩家当前是否装备着战士徽章EX（每帧由饰品置位，换代即失效）
        /// </summary>
        public bool warriorEmblemEX = false;
        /// <summary>
        /// 玩家当前是否装备着烈火手套EX（每帧由饰品置位，换代即失效）
        /// </summary>
        public bool fireGauntletEX = false;
        /// <summary>
        /// 玩家当前是否装备着游侠徽章EX（每帧由饰品置位，换代即失效）——
        /// 供省弹（<see cref="CanConsumeAmmo"/>）与弹药弹速（<see cref="ModifyShootStats"/>）两个钩子读取
        /// </summary>
        public bool rangerEmblemEX = false;
        /// <summary>
        /// 玩家当前是否装备着狙击镜EX（每帧由饰品置位，换代即失效）——
        /// 供省弹、弹速、额外夜明弹药、命中发射星璇火箭四个钩子读取
        /// </summary>
        public bool sniperScopeEX = false;
        /// <summary>
        /// 破晓EX 的附加时长（帧）：照原版破晓武器给的 5 秒
        /// </summary>
        public const int DaybreakEXDuration = 300;
        /// <summary>
        /// 本玩家当前是否带着破晓EX（每帧读 buff 表；buff 本身由原版同步，多人下各端都能读到）
        /// </summary>
        private bool HasDaybreakEX => Player.HasBuff<DaybreakEX>();
        /// <summary>
        /// 本玩家当前是否带着燃烧IV（同上，读 buff 表）
        /// </summary>
        private bool HasHellfireIV => Player.HasBuff<HellfireIV>();
        /// <summary>
        /// tModLoader 的 ResetEffects 钩子：每帧把饰品标记复位，随后由饰品自身的 UpdateAccessory 重新置位
        /// </summary>
        public override void ResetEffects()
        {
            warriorEmblemEX = false;
            fireGauntletEX = false;
            rangerEmblemEX = false;
            sniperScopeEX = false;
        }
        /// <summary>
        /// 物品（近战挥砍 / 近战类物品命中）结算：命中后按近战分类附加本模组的近战减益
        /// </summary>
        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            ApplyMeleeHitDebuffs(target, hit.DamageType);
        }
        /// <summary>
        /// 弹幕（含近战弹幕，如中子之刃光束）结算：与物品路走同一套判据
        /// </summary>
        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            ApplyMeleeHitDebuffs(target, hit.DamageType);
            SpawnVortexRocketOnRangedHit(proj, target, hit);
        }

        /// <summary>
        /// 远程弹幕每命中目标一次，发射一枚**原版**星璇火箭（<c>ProjectileID.VortexBeaterRocket</c> = 616，
        /// 即星璇机枪那枚火箭；本工程不新增弹幕）。
        /// <para>
        /// 判据：装备了狙击镜EX + 这一击算远程 + 不是火箭自己打出来的（否则火箭命中会再生成火箭，无限链）。
        /// 只在弹幕拥有者的客户端生成（<c>Main.myPlayer == Player.whoAmI</c>），原版会负责同步。
        /// 伤害取**发射它的那枚弹幕**的 <c>damage</c>（不按实际打出的 damageDone，免得暴击/减伤把它放大）。
        /// </para>
        /// </summary>
        private void SpawnVortexRocketOnRangedHit(Projectile proj, NPC target, NPC.HitInfo hit)
        {
            if (!sniperScopeEX || !hit.DamageType.CountsAsClass(DamageClass.Ranged))
            {
                return;
            }
            if (proj.type == ProjectileID.VortexBeaterRocket || Player.whoAmI != Main.myPlayer)
            {
                return;
            }
            Vector2 direction = (target.Center - Player.Center).SafeNormalize(-Vector2.UnitY);
            Vector2 spawnPosition = Player.Center + direction * 24f;
            Projectile.NewProjectile(Player.GetSource_OnHit(target), spawnPosition, direction * 14f
                , ProjectileID.VortexBeaterRocket, proj.damage, 0f, Player.whoAmI);
        }
        /// <summary>
        /// 敌怪路的施加口：只有近战命中才附加（按 <c>DamageClass.Melee</c> 过滤命中类型，
        /// 远程/魔法/召唤/无职业命中一律不加）。两个饰品各管自己的减益——
        /// 战士徽章EX → 破晓EX，烈火手套EX → 燃烧IV。
        /// </summary>
        private void ApplyMeleeHitDebuffs(NPC target, DamageClass damageType)
        {
            if (!damageType.CountsAsClass(DamageClass.Melee))
            {
                return;
            }
            if (warriorEmblemEX)
            {
                target.AddBuff(ModContent.BuffType<DaybreakEX>(), DaybreakEXDuration);
            }
            if (fireGauntletEX)
            {
                target.AddBuff(ModContent.BuffType<HellfireIV>(), HellfireIV.Duration);
            }
        }
        /// <summary>
        /// PvP 路的施加口：攻击方玩家装备着本模组的近战饰品、且这一击算近战时，给目标玩家也上对应的减益。
        /// 物品路与弹幕路共用本方法；过滤口径与打敌怪那条完全一致（只看命中类型，不看武器种类）。
        /// 注意：玩家侧的 <c>Player.HurtInfo</c> **没有 DamageType 字段**（与 NPC 的 <c>HitInfo</c> 不同），
        /// 所以命中类型由调用方从 Item / Projectile 上取（<c>item.DamageType</c> / <c>projectile.DamageType</c>）。
        /// 多人口径：玩家受伤由「被击中的那名玩家的客户端」结算，所以本方法在那边跑一次即可——
        /// 玩家增益由原版 AddBuff 同步出去（非 quiet），不会因为多端各跑一遍而叠出额外伤害。
        /// </summary>
        public static void TryApplyMeleeHitDebuffsOnPvP(Player attacker, Player target, DamageClass damageType)
        {
            if (attacker is null || !attacker.active || !target.active)
            {
                return;
            }
            if (!damageType.CountsAsClass(DamageClass.Melee))
            {
                return;
            }
            CalamityExpansionPlayer attackerPlayer = attacker.GetModPlayer<CalamityExpansionPlayer>();
            if (attackerPlayer.warriorEmblemEX)
            {
                target.AddBuff(ModContent.BuffType<DaybreakEX>(), DaybreakEXDuration);
            }
            if (attackerPlayer.fireGauntletEX)
            {
                target.AddBuff(ModContent.BuffType<HellfireIV>(), HellfireIV.Duration);
            }
        }
        /// <summary>
        /// 玩家侧掉血：与敌怪侧同一档（<see cref="DaybreakEX.BurnLifeRegen"/> = 1500，即每秒 750）。
        /// 照原版减益 DoT 的写法：先清掉正回复、清掉回复计时，再压 lifeRegen。
        /// 多人：本钩子在每个客户端都会为每名玩家跑一遍，但玩家生命以本人客户端为准（原版同步），
        /// 所以不会出现多端重复扣血。
        /// </summary>
        public override void UpdateBadLifeRegen()
        {
            if (!HasDaybreakEX && !HasHellfireIV)
            {
                return;
            }
            if (Player.lifeRegen > 0)
            {
                Player.lifeRegen = 0;
            }
            Player.lifeRegenTime = 0;
            if (HasDaybreakEX)
            {
                Player.lifeRegen -= DaybreakEX.BurnLifeRegen;
            }
            if (HasHellfireIV)
            {
                Player.lifeRegen -= HellfireIV.BurnLifeRegen;
            }
        }
        /// <summary>
        /// 玩家侧减防：在 PostUpdateEquips（原版已经算完 statDefense 之后）按各自档位扣点
        /// </summary>
        public override void PostUpdateEquips()
        {
            if (HasDaybreakEX)
            {
                Player.statDefense -= (int)DaybreakEX.DefenseLoss;
            }
            if (HasHellfireIV)
            {
                Player.statDefense -= (int)HellfireIV.DefenseLoss;
            }
        }
        /// <summary>
        /// 玩家侧「受到伤害增加」：与原版灵液/破晓在敌怪侧用的是同一套 HitModifiers 口径；
        /// 两个减益同时挂上时倍率相乘
        /// </summary>
        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (HasDaybreakEX)
            {
                modifiers.FinalDamage *= DaybreakEX.DamageTakenMultiplier;
            }
            if (HasHellfireIV)
            {
                modifiers.FinalDamage *= HellfireIV.DamageTakenMultiplier;
            }
        }
        /// <summary>
        /// 近战击退 ×2（烈火手套EX）：原版近战击退没有专用字段，走 tModLoader 的
        /// <c>ModifyWeaponKnockback</c>；只对近战类武器生效，其它伤害职业不动。
        /// </summary>
        public override void ModifyWeaponKnockback(Item item, ref StatModifier knockback)
        {
            if (fireGauntletEX && item.CountsAsClass(DamageClass.Melee))
            {
                knockback *= FireGauntletEX.KnockbackMultiplier;
            }
        }
        /// <summary>
        /// 近战武器大小 +20%（烈火手套EX）：走 tModLoader 的 <c>ModifyItemScale</c>（<c>scale</c> 是倍率，1 = 原大小），
        /// 同样只对近战类生效。
        /// </summary>
        public override void ModifyItemScale(Item item, ref float scale)
        {
            if (fireGauntletEX && item.CountsAsClass(DamageClass.Melee))
            {
                scale += FireGauntletEX.MeleeScaleBonus;
            }
        }
        /// <summary>
        /// 18% 概率不消耗弹药（游侠徽章EX）：原版没有「任意百分比省弹」的字段（只有 20%/25% 那几个
        /// 固定档的布尔量），故走 tModLoader 的 <c>CanConsumeAmmo</c>——返回 false 即这一发不扣弹药。
        /// 判定只在射击方客户端跑（<c>PickAmmo</c> 本身是客户端逻辑），多人下不会双重触发。
        /// </summary>
        public override bool CanConsumeAmmo(Item weapon, Item ammo)
        {
            if (rangerEmblemEX && Main.rand.NextFloat() < RangerEmblemEX.AmmoSaveChance)
            {
                return false;
            }
            // 狙击镜EX：40% 概率不消耗（两件同时装备时各自独立掷一次，任一命中就省下这一发）
            if (sniperScopeEX && Main.rand.NextFloat() < SniperScopeEX.AmmoSaveChance)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 弹药飞行速度 +18%（游侠徽章EX）：<c>ModifyShootStats</c> 是射出前最后一次能改初速的钩子，
        /// 直接乘在 velocity 上（与原版「魔法箭袋」对箭矢的处理同一个位置的效果）。
        /// 只对**消耗弹药的远程武器**生效——非远程职业、以及不吃弹药的远程武器（<c>useAmmo = None</c>）不动。
        /// </summary>
        public override void ModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (item.useAmmo == AmmoID.None || !item.CountsAsClass(DamageClass.Ranged))
            {
                return;
            }
            if (rangerEmblemEX)
            {
                velocity *= 1f + RangerEmblemEX.AmmoVelocityBonus;
            }
            // 狙击镜EX：远程弹幕飞行速度 +22%（同样是初速，不是攻速；两件叠加时乘算）
            if (sniperScopeEX)
            {
                velocity *= 1f + SniperScopeEX.ProjectileVelocityBonus;
            }
        }
        /// <summary>
        /// 射击瞬间：狙击镜EX 有 50% 概率**额外**射出一发原版夜明箭 / 夜明弹
        /// （用户 2026-10-17 口径：是「多射一发」，不是把原来的弹药换成夜明弹）。
        /// <para>
        /// 这里能拿到本次实际使用的弹药（<c>source.AmmoItemIdUsed</c>），所以按弹药的
        /// <c>ammo</c> 分类判断：箭类 → <c>ProjectileID.MoonlordArrow</c>(639)，
        /// 子弹类 → <c>ProjectileID.MoonlordBullet</c>(638)。两次都用原版弹幕，不新增类型。
        /// 额外那发在垂直方向错开 8 像素、初速同向，避免和原弹完全重叠。
        /// </para>
        /// </summary>
        /// <remarks>
        /// ⚠️ 这个钩子**返回 bool**：返回 false 会把这一发原版弹幕整个拦掉，所以这里永远 <c>return true</c>，
        /// 额外那发是用 <c>Projectile.NewProjectile</c> 自己补的（只加不减）。
        /// </remarks>
        public override bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (!sniperScopeEX || Main.rand.NextFloat() >= SniperScopeEX.ExtraLuminiteChance)
            {
                return true;
            }
            int ammoType = source.AmmoItemIdUsed;
            if (ammoType <= ItemID.None || ammoType >= ItemLoader.ItemCount)
            {
                return true;
            }
            int ammoClass = ContentSamples.ItemsByType[ammoType].ammo;
            int extraType;
            if (ammoClass == AmmoID.Arrow)
            {
                extraType = ProjectileID.MoonlordArrow;
            }
            else if (ammoClass == AmmoID.Bullet)
            {
                extraType = ProjectileID.MoonlordBullet;
            }
            else
            {
                return true;   // 火箭 / 飞镖 / 溶液等其它弹药不参与
            }
            Vector2 direction = velocity.SafeNormalize(Vector2.UnitX);
            Vector2 offset = new Vector2(-direction.Y, direction.X) * 8f;
            Projectile.NewProjectile(source, position + offset, velocity, extraType, damage, knockback, Player.whoAmI);
            return true;
        }
    }
}
