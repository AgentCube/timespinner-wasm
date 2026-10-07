using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;

namespace Timespinner.GameObjects.Heroes.Lunais.BaseClasses;

internal class LunaisReviveAnimation : BattleAnimation
{
	private const int StartFrameIndex = 10;

	private const int FlashAnimationLength = 9;

	private const int Anim_FeatherStart = 47;

	private const int Anim_FeatherLength = 4;

	private const float FlashAnimationSpeed = 0.033f;

	internal const float AnimationDuration = 0.297f;

	private readonly FeatherReviveParticleSystem _featherParticles;

	public LunaisReviveAnimation(SpriteSheet inSprite, Point inPosition, Level inLevel)
		: base(inSprite, inPosition, inLevel)
	{
		base.AnimationStart = 10;
		base.AnimationLength = 9;
		base.AnimationSpeed = 0.033f;
		Color drawColor = new Color(224, 216, 168, 128);
		base.DrawColor = drawColor;
		base.DoesFadeOut = true;
		base.Scale = 0.5f;
		_featherParticles = new FeatherReviveParticleSystem(inSprite, 1, 47, 4);
		base.ParticleSystem = _featherParticles;
		base.DoesDrawParticles = false;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		Point position = base.Position;
		base.IsFlippedVertically = true;
		base.IsFacingLeft = true;
		base.Draw(spriteBatch);
		int num = (int)((float)base.FrameSource.Width * base.Scale);
		base.IsFacingLeft = false;
		base.Position = new Point(base.Position.X - num, base.Position.Y);
		base.Draw(spriteBatch);
		base.IsFlippedVertically = false;
		base.Position = new Point(base.Position.X, base.Position.Y - num);
		base.Draw(spriteBatch);
		base.IsFacingLeft = true;
		base.Position = new Point(base.Position.X + num, base.Position.Y);
		base.Draw(spriteBatch);
		base.Position = position;
		_featherParticles.Draw(spriteBatch, base.Level.LevelRenderCenter, base.Level.CameraPosition, base.Level.CameraZoom);
	}
}
