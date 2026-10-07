using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Shapeshifter;

internal sealed class ShapeshifterCrusherProjectile : Projectile
{
	private const int BboxWidth = 128;

	private const int BboxHeight = 48;

	private const float TimeToMeltFromCeiling = 0.5f;

	private const float TimeToWaitBeforeMeltingAfterLanding = 0.35f;

	private const float TimeToMeltIntoGround = 0.5f;

	private static readonly Color MeltBaseColor = new Color(200, 128, 176);

	private readonly Point _ceilingPosition;

	private readonly LandingDustParticleSystem _dustParticles;

	private bool _hasTouchedGround;

	private bool _hasStartedFalling;

	private float _meltTimer;

	private float _floorWaitTimer;

	private Point _floorPosition;

	public ShapeshifterCrusherProjectile(Level inLevel, Point inPosition, SpriteSheet sprite, int baseDamage)
		: base(inLevel, inPosition, Vector2.Zero, ETeamSide.Enemies, -1)
	{
		_ceilingPosition = new Point(inPosition.X, inPosition.Y + 32);
		_sprite = sprite;
		base.Power = (int)Math.Ceiling((float)baseDamage * 1.15f);
		Bbox = new Rectangle(0, 0, 128, 48);
		_isAffectedByGravity = false;
		_doesDieOnTiles = false;
		_doesBounceOnGround = true;
		_doesCollideWithFloors = true;
		_doesRotateBasedOnVelocity = false;
		base.DoesCollideWithTiles = false;
		base.DoesDieOnImpact = false;
		base.DoesKnockBack = true;
		base.IsDamageArea = true;
		_life = 3f;
		ChangeAnimation(42);
		Position = new Point(_ceilingPosition.X, _ceilingPosition.Y - 48);
		_doesAutomaticallyEmitParticles = false;
		_dustParticles = new LandingDustParticleSystem(_level.GCM.TxParticleDust, 10, _level.ID, 100);
		_particleSystems.Add(_dustParticles);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (!_hasStartedFalling)
			{
				if (_meltTimer <= 0f && delta > 0f)
				{
					PlayCue(ESFX.BossShapeshifterTeeth);
				}
				_meltTimer += delta;
				if (_meltTimer <= 0.5f)
				{
					float num = _meltTimer / 0.5f;
					base.DrawColor = MeltBaseColor.SineInterpolate(Color.White, num);
					Position = new Point(_ceilingPosition.X, _ceilingPosition.Y - (int)((1.0 - Math.Sin(num * ((float)Math.PI / 2f))) * 48.0));
				}
				else
				{
					Position = _ceilingPosition;
					_hasStartedFalling = true;
					_meltTimer = 0f;
					_isAffectedByGravity = true;
					base.DoesCollideWithTiles = true;
				}
				SnapBboxToPosition();
			}
			else if (_hasTouchedGround)
			{
				_floorWaitTimer += delta;
				if (_floorWaitTimer >= 0.35f)
				{
					if (_meltTimer <= 0f)
					{
						_floorPosition = Position;
						_isAffectedByGravity = false;
						base.DoesCollideWithTiles = false;
						_velocity = Vector2.Zero;
						base.CanDamageThings = false;
					}
					_meltTimer += delta;
					if (_meltTimer <= 0.5f)
					{
						float num2 = _meltTimer / 0.5f;
						base.DrawColor = Color.White.SineInterpolate(MeltBaseColor, num2);
						Position = new Point(_floorPosition.X, _floorPosition.Y + (int)((1.0 - Math.Cos(num2 * ((float)Math.PI / 2f))) * 48.0));
					}
					else
					{
						SilentKill();
					}
				}
			}
		}
		base.Update(delta);
	}

	public override void SnapBboxToPosition()
	{
		_bbox.Location = new Point(_position.X - _bbox.Width / 2, _position.Y - _bbox.Height);
	}

	protected override void DoBounce(Point impactPoint)
	{
		if (!_hasTouchedGround)
		{
			_level.RequestScreenShake(new Vector2(0f, 5f), 0.4f, 6f, isAffectedByTime: true);
			_dustParticles.AddParticles(new Vector2(Position.X, impactPoint.Y), 100f);
		}
		_hasTouchedGround = true;
	}
}
