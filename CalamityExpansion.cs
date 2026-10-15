using Terraria;
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
	}
}
