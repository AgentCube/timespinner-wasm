using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Lanterns;

internal sealed class LabCoffeeLanternEvent : BaseLantern
{
	private const int FruitGlowRadius = 32;

	private static readonly Vector4 CoffeeGlowColor = new Vector4(0.8f, 0.4f, 0.6f, 1f);

	private readonly FruitSparkleParticleSystem _sparkles;

	public LabCoffeeLanternEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_sprite = _level.GCM.SpLanterns;
		Position = Position.Add(6, 0);
		IsFacingLeft = true;
		ChangeAnimation(114);
		BboxOffset = new Point(-4, -4);
		Bbox = new Rectangle(0, 0, 16, 12);
		DrawOrigin = new Vector2(4f, 4f);
		_isAffectedByGravity = false;
		_isFlying = true;
		_isAffectedByTime = true;
		base.GlowRadius = 32;
		base.OrbGlowColor = CoffeeGlowColor;
		SnapBboxToPosition();
		SnapFrameToBbox();
		_sparkles = new FruitSparkleParticleSystem(_level.GCM.TxParticleEnergy, 5)
		{
			BaseColor = CoffeeGlowColor
		};
		_particleSystems.Add(_sparkles);
	}

	public override void Update(float delta)
	{
		if (!_isFrozen && !base.IsDormant)
		{
			_sparkles.AddParticles(Bbox.Center.ToVector2());
		}
		base.Update(delta);
	}

	internal override void Explode(Projectile projectile)
	{
		_level.PlayCue(ESFX.FoleyLanternBrazierBreak, Bbox.Center);
		_level.AddAnimation(EBattleAnimationType.GlassShatter, Bbox.Center, ETeamSide.Neutral, projectile.VisibleVelocity.X < 0f);
		base.Explode(projectile);
	}
}
