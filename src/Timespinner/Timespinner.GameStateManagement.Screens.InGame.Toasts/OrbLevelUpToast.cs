using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;

namespace Timespinner.GameStateManagement.Screens.InGame.Toasts;

internal class OrbLevelUpToast : BaseToastPopup
{
	private const int OrbFrameIndexStart = 78;

	private const int TextFrameIndexStart = 77;

	private const int TopFrameIndexStart = 75;

	private const int BottomFrameIndexStart = 76;

	private const int DisplayOffsetY = -16;

	private static readonly Vector2 OrbOffset = new Vector2(-8f, -29f);

	private static readonly Vector2 TextOffset = new Vector2(-34f, -3f);

	private static readonly Vector2 TopFrameOffset = new Vector2(-44f, -30f);

	private static readonly Vector2 BottomFrameOffset = new Vector2(-28f, 10f);

	private readonly int _orbIndex;

	private readonly int _orbFrameIndex;

	private readonly Vector2 _drawPosition;

	public OrbLevelUpToast(SpriteSheet sprite, GCM gcm, int orbIndex, Rectangle titleSafe, float zoom)
		: base(sprite, doesFreezeGameplay: false, gcm)
	{
		_orbIndex = orbIndex;
		_orbFrameIndex = 78 + _orbIndex;
		Vector2 drawPosition = new Vector2((float)titleSafe.Center.X / zoom, (float)titleSafe.Bottom / zoom + -16f);
		_drawPosition = drawPosition;
	}

	internal override void DrawToastContent(SpriteBatch spriteBatch, Color drawColor, float zoom)
	{
		Vector2 value = _drawPosition * zoom;
		Rectangle frameSource = base.Sprite.GetFrameSource(_orbFrameIndex);
		Vector2 position = Vector2.Add(value, OrbOffset * zoom);
		spriteBatch.Draw(base.Sprite.Texture, position, frameSource, drawColor, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
		frameSource = base.Sprite.GetFrameSource(77);
		position = Vector2.Add(value, TextOffset * zoom);
		spriteBatch.Draw(base.Sprite.Texture, position, frameSource, drawColor, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
		frameSource = base.Sprite.GetFrameSource(75);
		position = Vector2.Add(value, TopFrameOffset * zoom);
		spriteBatch.Draw(base.Sprite.Texture, position, frameSource, drawColor, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
		position.X += (0f - TopFrameOffset.X) * zoom;
		spriteBatch.Draw(base.Sprite.Texture, position, frameSource, drawColor, 0f, Vector2.Zero, zoom, SpriteEffects.FlipHorizontally, 0f);
		frameSource = base.Sprite.GetFrameSource(76);
		position = Vector2.Add(value, BottomFrameOffset * zoom);
		spriteBatch.Draw(base.Sprite.Texture, position, frameSource, drawColor, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
		position.X += (0f - BottomFrameOffset.X) * zoom;
		spriteBatch.Draw(base.Sprite.Texture, position, frameSource, drawColor, 0f, Vector2.Zero, zoom, SpriteEffects.FlipHorizontally, 0f);
	}
}
