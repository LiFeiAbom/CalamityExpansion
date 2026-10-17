using System.IO;
using CalamityExpansion.Content.Items.Accessories.JobAcc.Ranged;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityExpansion
{
	/// <summary>
	/// 模组主类：缓存单例、在加载期请求着色器资源，并在卸载期清空全部静态资源引用。
	/// 本工程的口径与「已删内容取回」的做法，见仓库根的 AGENTS.md。
	/// </summary>
	public class CalamityExpansion : Mod
	{
		/// <summary>
		/// 本模组的封包类型（每个字节一个类型，写在包体最前面）
		/// </summary>
		internal enum CeMessage : byte
		{
			/// <summary>
			/// PvP 命中请求生成一枚星璇火箭：客户端 → 服务端。
			/// 包体：owner(byte) + 位置(x,y: single) + 速度(x,y: single) + 伤害(int)
			/// </summary>
			SniperScopePvPRocket = 1,
		}		/// <summary>
		/// 模组单例（由 Load 填充），供工程内静态代码取 <c>Assets</c> 等实例成员
		/// </summary>
		public static CalamityExpansion Instance;
		/// <summary>
		/// 全屏闪白强度（移植自 CE 的 CalamityEntropy.FlashEffectStrength，值越大闪得越亮越大）：
		/// 任意效果置一个正值即可，由 EffectsSystem 每帧衰减 0.02，
		/// 并在上屏阶段以屏幕中心为轴叠 16 层逐级放大的画面形成白闪。
		/// 本工程目前没有内容设置它，作为扭曲管线系统的一部分保留。
		/// </summary>
		public static float FlashEffectStrength;
		/// <summary>
		/// 屏幕震动强度（移植自 CE 的 CalamityEntropy.Instance.screenShakeAmp）：
		/// 任意效果置一个正值即可，由 EffectsSystem 每帧衰减 0.5；
		/// 旧工程里的消费方是玩家类的 ModifyScreenPosition（横轴抖幅是纵轴的 8 倍），本工程尚未移植玩家类。
		/// </summary>
		public static float ScreenShakeAmp;
		/// <summary>
		/// GravityDontFlipScreen 模组的软依赖引用（未安装时为 null）。
		/// 供 BaseHeldProjCE 在重力翻转时判断是否需要修正持握弹幕的朝向
		/// </summary>
		internal Mod gravityDontFlipScreen = null;
		/// <summary>
		/// 模组加载钩子：缓存单例、探测软依赖，并在客户端请求 .fx 着色器资源。
		/// 着色器只在客户端需要，专用服务器上跳过（与工程既有写法一致）
		/// </summary>
		public override void Load()
		{
			Instance = this;
			FindMod();
			if (!Main.dedServ)
			{
				Common.Effects.EffectLoader.LoadEffects();
			}
		}
		/// <summary>
		/// 模组卸载钩子：清空静态引用（避免热重载后仍旧引用已卸载的模组实例与资源）
		/// </summary>
		public override void Unload()
		{
			Instance = null;
			gravityDontFlipScreen = null;
			Common.Effects.EffectLoader.UnLoad();
			// 弹幕侧懒加载的静态贴图引用同样要清空，否则热重载后指向已释放的资源
			Content.Projectiles.Melee.NeutronGlaiveHeld.UnloadTextures();
			Content.Projectiles.Melee.NeutronGlaiveBeam.warpTex = null;
		}
		/// <summary>
		/// 探测 GravityDontFlipScreen（重力翻转时保持屏幕不翻转的辅助模组）；
		/// 该模组只影响持握弹幕的朝向修正，未安装时相关修正自动跳过
		/// </summary>
		private void FindMod()
		{
			ModLoader.TryGetMod("GravityDontFlipScreen", out gravityDontFlipScreen);
		}
		/// <summary>
		/// 封包入口。目前只有一条：PvP 命中时的星璇火箭请求。
		/// <para>
		/// 为什么 PvP 需要走服务端：狙击镜EX 的命中钩子（<c>GlobalProjectile.OnHitPlayer</c>）跑在
		/// **被打中那名玩家的客户端**（<c>Player.Hurt</c> 只对本机玩家调用），而原版只在
		/// 「弹幕 owner == 本机」的客户端才同步新生成的弹幕——在那个客户端直接生成，别的玩家看不到。
		/// 所以由被打方客户端发一条请求给服务端，服务端校验后生成，原版会广播给所有客户端。
		/// </para>
		/// </summary>
		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			CeMessage message = (CeMessage)reader.ReadByte();
			switch (message)
			{
				case CeMessage.SniperScopePvPRocket:
					HandleSniperScopePvPRocket(reader, whoAmI);
					break;
			}
		}
		/// <summary>
		/// 服务端处理「PvP 星璇火箭」请求：校验被指定的 owner，然后生成弹幕（原版会自动同步给所有客户端）。
		/// 客户端收到同一条包不会有动作（服务端生成后本来就会广播真正的生成包）。
		/// </summary>
		private void HandleSniperScopePvPRocket(BinaryReader reader, int whoAmI)
		{
			if (Main.netMode != NetmodeID.Server)
			{
				return;
			}
			int ownerIndex = reader.ReadByte();
			Vector2 position = new Vector2(reader.ReadSingle(), reader.ReadSingle());
			Vector2 velocity = new Vector2(reader.ReadSingle(), reader.ReadSingle());
			int damage = reader.ReadInt32();
			if (ownerIndex < 0 || ownerIndex >= Main.maxPlayers || whoAmI < 0 || whoAmI >= Main.maxPlayers)
			{
				return;
			}
			// owner 必须真的戴着狙击镜EX——按饰品槽查，因为专用服务器不跑 UpdateAccessory，
			// ModPlayer 上的 sniperScopeEX 标记在服务端不可靠
			Player attacker = Main.player[ownerIndex];
			if (!attacker.active || !HasSniperScopeEquipped(attacker))
			{
				return;
			}
			if (damage <= 0 || damage > 1000000)
			{
				return;   // 客户端传来的伤害做个上界保护
			}
			Projectile.NewProjectile(new EntitySource_Misc("CalamityExpansion:SniperScopeEXPvPRocket")
				, position, velocity, ProjectileID.VortexBeaterRocket, damage, 0f, ownerIndex);
		}
		/// <summary>
		/// 该玩家身上是否戴着狙击镜EX（饰品槽 armor[3..9]；不依赖 ModPlayer 标记，服务端也能用）
		/// </summary>
		internal static bool HasSniperScopeEquipped(Player player)
		{
			int scopeType = ModContent.ItemType<SniperScopeEX>();
			for (int i = 3; i <= 9; i++)
			{
				if (player.armor[i].type == scopeType)
				{
					return true;
				}
			}
			return false;
		}
	}
}
