using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Lanterns;

internal sealed class ForestCampfireLanternEvent : BaseLantern
{
	private const int FlameGlowRadius = 48;

	private const int FlameGlowCircleCount = 6;

	private const int StartOffsetX = 8;

	private const int StartOffsetY = 1;

	private const float TimeBetweenSmokeParticles = 0.033f;

	private static readonly Vector4 FlameGlowColor = new Vector4(0.9f, 0.6f, 0.1f, 1f);

	private readonly CampfireSmokeParticleSystem _smokeParticles;

	private float _particleTimer;

	public ForestCampfireLanternEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		Position = new Point(Position.X + 8, Position.Y + 1);
		_sprite = _level.GCM.SpLanterns;
		ChangeAnimation(14, 4, 0.1f, EAnimationType.Cycle);
		BboxOffset = new Point(0, 0);
		Bbox = new Rectangle(0, 0, 16, 16);
		DrawOrigin = new Vector2(8f, 8f);
		Position = Position.Add(0, -4);
		_isAffectedByGravity = false;
		_isFlying = true;
		_isAffectedByTime = true;
		base.DoesRegenerate = false;
		base.IsInvulnerable = true;
		base.DoesFlicker = true;
		base.GlowRadius = 48;
		base.GlowCircleCount = 6;
		base.OrbGlowColor = FlameGlowColor;
		SnapBboxToPosition();
		SnapFrameToBbox();
		SetCharacterSequenceByName("Idle");
		_smokeParticles = new CampfireSmokeParticleSystem(_level.GCM.TxParticleDust, 32);
		_particleSystems.Add(_smokeParticles);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			_particleTimer -= delta;
			if (_particleTimer <= 0f)
			{
				_smokeParticles.AddParticles(Position.ToVector2());
				_particleTimer = 0.033f;
			}
		}
		base.Update(delta);
	}
}
