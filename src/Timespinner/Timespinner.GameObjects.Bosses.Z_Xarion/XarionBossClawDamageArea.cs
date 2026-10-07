using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Z_Xarion;

internal class XarionBossClawDamageArea : DamageArea
{
	private const int Anim_ClawFrame = 27;

	private const int ClawWidth = 64;

	private const int ClawHeight = 48;

	internal const int HalfWidth = 32;

	internal const int HalfHeight = 24;

	private readonly int _baseDamage;

	private readonly LandingDustParticleSystem _landingDustParticles;

	private readonly XarionWallDustParticleSystem _wallDustParticles;

	public XarionBossClawDamageArea(Level inLevel, Point inPosition, SpriteSheet sprite, int baseDamage)
		: base(inLevel, inPosition, ETeamSide.Enemies, -1, null)
	{
		_sprite = sprite;
		_baseDamage = baseDamage;
		_power = _baseDamage;
		_bbox = new Rectangle(0, 0, 64, 48);
		_life = 1000f;
		base.HasInfiniteLife = true;
		ChangeAnimation(27);
		base.DoesDrawBoundingBox = true;
		base.CanDamageThings = false;
		_doesAutomaticallyEmitParticles = false;
		_landingDustParticles = new LandingDustParticleSystem(_level.GCM.TxParticleDust, 1, _level.ID, 100);
		_wallDustParticles = new XarionWallDustParticleSystem(_level.GCM.TxParticleDust, 1, _level.ID, 100);
		_particleSystems.Add(_landingDustParticles);
		_particleSystems.Add(_wallDustParticles);
	}

	internal void Activate()
	{
		base.CanDamageThings = true;
	}

	internal void Deactivate()
	{
		base.CanDamageThings = false;
		base.DoesDrawBoundingBox = false;
	}

	internal void OnGroundSlam()
	{
		_landingDustParticles.AddParticles(new Vector2(Position.X, Bbox.Bottom), 200f);
	}

	internal void OnWallSlam()
	{
		_wallDustParticles.AddParticles(new Vector2(Bbox.Left, Bbox.Center.Y), 200f);
	}
}
