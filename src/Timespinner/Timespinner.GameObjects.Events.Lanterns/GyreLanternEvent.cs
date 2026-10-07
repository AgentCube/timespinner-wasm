using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Lanterns;

internal sealed class GyreLanternEvent : BaseLantern
{
	private const int FlameGlowRadius = 33;

	private const int FlameGlowCircleCount = 6;

	private static readonly Vector4 FlameGlowColor = new Vector4(0.8f, 0.4f, 0.7f, 0.7f);

	private readonly ExtinguishSmokeParticleSystem _smokeParticles;

	private readonly CharacterSequenceSpecification _deathSequence;

	private readonly CharacterSequenceSpecification _reviveSequence;

	public GyreLanternEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_sprite = _level.GCM.SpLanterns;
		Bbox = new Rectangle(0, 0, 16, 16);
		DrawOrigin = new Vector2(0f, 0f);
		_doAppendagesMatchImageFacing = true;
		IsFacingLeft = objectSpec.IsFlippedHorizontally;
		_isAffectedByGravity = false;
		_isFlying = true;
		_isAffectedByTime = true;
		base.DoesRegenerate = !_level.IsHardMode;
		base.AreAppendagesVisibleWhenDormant = true;
		base.DoesFlicker = true;
		base.GlowRadius = 33;
		base.GlowCircleCount = 6;
		base.OrbGlowColor = FlameGlowColor;
		base.LanternGlowOffset = new Point(0, 5);
		SnapBboxToPosition();
		SnapFrameToBbox();
		_smokeParticles = new ExtinguishSmokeParticleSystem(_level.GCM.TxParticleDust, 1);
		_particleSystems.Add(_smokeParticles);
		if (base.CharacterSpecification != null && base.CharacterSpecification.Sequences.Count > 0)
		{
			SetCharacterSequence(base.CharacterSpecification.Sequences[0]);
			_deathSequence = GetCharacterSequenceByName("Death");
			_reviveSequence = GetCharacterSequenceByName("Revive");
		}
	}

	internal override void Explode(Projectile projectile)
	{
		PlayCue(ESFX.FoleyLanternExtinguish, Position);
		if (base.DoesRegenerate)
		{
			_smokeParticles.AddParticles(Position.ToVector2());
		}
		else
		{
			BattleAnimation battleAnimation = new BattleAnimation(null, Position, _level);
			battleAnimation.ParticleSystem = _smokeParticles;
			BattleAnimation newAnimation = battleAnimation;
			_level.AddAnimation(newAnimation);
		}
		SetCharacterSequence(_deathSequence);
		base.IsGlowing = false;
		base.Explode(projectile);
	}

	internal override void Revive()
	{
		SetCharacterSequence(_reviveSequence);
	}
}
