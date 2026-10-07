using System;
using Microsoft.Xna.Framework;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Emperor;

internal sealed class EmperorFireSpellDamageArea : DamageArea
{
	private const int MaxFireProjectiles = 128;

	private const int FlameDirections = 3;

	private const float FlameDirectionRadialOffset = (float)Math.PI * 2f / 3f;

	private const float TimeBetweenFlameEmission = 0.05f;

	private const float EmissionRotationFrequency = 8f;

	private const float EmissionVelocity = 100f;

	private const float MaxLife = 2f;

	private readonly int _baseDamage;

	private readonly EmperorFireSpellProjectile[] _fireProjectiles = new EmperorFireSpellProjectile[128];

	private int _flamesEmitted;

	private float _emissionTimer;

	private float _emissionRotation;

	public EmperorFireSpellDamageArea(Level inLevel, Point inPosition, int baseDamage)
		: base(inLevel, inPosition, ETeamSide.Enemies, -1, null)
	{
		_power = 0;
		Bbox = new Rectangle(0, 0, 16, 16);
		_life = 2f;
		_isAffectedByFriction = false;
		_isFlying = true;
		_isAffectedByGravity = false;
		_maxMoveSpeed = 500f;
		_baseDamage = baseDamage;
		_sprite = _level.GCM.SpOrbMeleeFire;
		ChangeAnimation(-1);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			_emissionRotation += 8f * delta;
			if (_emissionRotation >= (float)Math.PI * 2f)
			{
				_emissionRotation -= (float)Math.PI * 2f;
			}
			if (!_isFading)
			{
				_emissionTimer += delta;
				if (_emissionTimer >= 0.05f)
				{
					_emissionTimer -= 0.05f;
					Point center = Bbox.Center;
					for (int i = 0; i < 3; i++)
					{
						float num = _emissionRotation + (float)i * ((float)Math.PI * 2f / 3f);
						Vector2 iV = new Vector2((float)Math.Cos(num), (float)Math.Sin(num)) * 100f;
						EmitFlame(center, iV);
					}
				}
			}
		}
		base.Update(delta);
	}

	private void EmitFlame(Point startPoint, Vector2 iV)
	{
		EmperorFireSpellProjectile emperorFireSpellProjectile = null;
		if (_flamesEmitted < 128)
		{
			emperorFireSpellProjectile = new EmperorFireSpellProjectile(_level, startPoint, iV, ETeamSide.Enemies, _sprite, _baseDamage);
			_fireProjectiles[_flamesEmitted] = emperorFireSpellProjectile;
			_flamesEmitted++;
		}
		else
		{
			EmperorFireSpellProjectile[] fireProjectiles = _fireProjectiles;
			foreach (EmperorFireSpellProjectile emperorFireSpellProjectile2 in fireProjectiles)
			{
				if (emperorFireSpellProjectile2.IsFinished)
				{
					emperorFireSpellProjectile2.Reset(startPoint, iV);
					emperorFireSpellProjectile = emperorFireSpellProjectile2;
					break;
				}
			}
		}
		if (emperorFireSpellProjectile != null)
		{
			_level.RequestAddObject(emperorFireSpellProjectile);
		}
	}

	internal void Reset(Point position, Vector2 iV)
	{
		Position = position;
		_initialVector = iV;
		_velocity = iV;
		SnapBboxToPosition();
		base.ID = -1;
		_isFading = false;
		_life = 2f;
		_emissionTimer = 0f;
		_emissionRotation = 0f;
		PlayCue(ESFX.BossVolTerrilisFireSpellCast);
		PlayCue(ESFX.BossVolTerrilisFireSpellFlames);
	}
}
