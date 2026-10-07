using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Animations;

internal class ShockwaveAnimation : BattleAnimation
{
	private const int StartFrameIndex = 10;

	private const int FlashAnimationLength = 9;

	private const float FlashAnimationSpeed = 0.033f;

	internal const float AnimationDuration = 0.297f;

	public ShockwaveAnimation(SpriteSheet inSprite, Point inPosition, Level inLevel, Color tintColor)
		: base(inSprite, inPosition, inLevel)
	{
		base.AnimationStart = 10;
		base.AnimationLength = 9;
		base.AnimationSpeed = 0.033f;
		base.DrawColor = tintColor;
		base.DoesFadeOut = true;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		Point position = base.Position;
		base.IsFlippedVertically = true;
		base.IsFacingLeft = true;
		base.Draw(spriteBatch);
		int width = base.FrameSource.Width;
		base.IsFacingLeft = false;
		base.Position = new Point(base.Position.X - width, base.Position.Y);
		base.Draw(spriteBatch);
		base.IsFlippedVertically = false;
		base.Position = new Point(base.Position.X, base.Position.Y - width);
		base.Draw(spriteBatch);
		base.IsFacingLeft = true;
		base.Position = new Point(base.Position.X + width, base.Position.Y);
		base.Draw(spriteBatch);
		base.Position = position;
	}
}
