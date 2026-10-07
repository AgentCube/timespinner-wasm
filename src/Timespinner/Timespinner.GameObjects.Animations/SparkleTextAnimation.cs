using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Animations;

internal class SparkleTextAnimation : TextPopupAnimation
{
	private readonly OrbLevelUpSparkleParticleSystem _sparkleParticles;

	public SparkleTextAnimation(string text, SpriteSheet inSprite, Point inPosition, Level inLevel, Color color)
		: base(text, inSprite, inPosition, inLevel, color)
	{
		base.TeamSide = ETeamSide.Neutral;
		base.DrawPlane = EDrawPlane.Front;
		base.DrawColor = color;
		base.ParticleSystem = new OrbLevelUpParticleSystem(base.Level.GCM.TxParticleEnergy, 1, color);
		_sparkleParticles = new OrbLevelUpSparkleParticleSystem(base.Level.GCM.SpEffectsSmall, 1, color);
		base.AnimationStart = 31;
		base.AnimationLength = 5;
		base.AnimationSpeed = 0.05f;
		_sparkleParticles.AddParticles(base.Position.ToVector2());
	}

	public override void Update(float delta)
	{
		_sparkleParticles.Update(delta);
		base.Update(delta);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);
		_sparkleParticles.Draw(spriteBatch, base.Level.LevelRenderCenter, base.Level.CameraPosition, base.Level.CameraZoom);
	}
}
