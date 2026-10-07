using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.MonsterParticleEffects;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class CeilingStarDamageArea : DamageArea
{
	private const float MaxLife = 0.05f;

	private const float TimeBetweenParticleEmission = 0.3f;

	private readonly CeilingStarZapParticleSystem _zapParticleSystem;

	private bool _canSpawnParticles = true;

	private float _emissionTimer;

	public CeilingStarDamageArea(Level inLevel, Point inPosition, ETeamSide inSide, Mobile inAnchor, SpriteSheet sprite, int baseDamage)
		: base(inLevel, inPosition, inSide, -1, inAnchor)
	{
		_doesDrawSpriteAndAppendages = false;
		base.DamageDimensions = new Point(48, 48);
		SnapBboxToPosition();
		_zapParticleSystem = new CeilingStarZapParticleSystem(sprite, 5, 7);
		_particleSystems.Add(_zapParticleSystem);
		_power = (int)Math.Ceiling((float)baseDamage * 1.25f);
		_force = 1;
		_life = 0.05f;
		base.DamageTimeoutTime = 0.2f;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_isFlying = true;
		_doesCollideWithSlopes = false;
		_doesDieOnTiles = false;
	}

	public void RefreshLife()
	{
		_life = 0.05f;
	}

	public void KillParticles()
	{
		_zapParticleSystem.KillOffParticles(0f);
		_canSpawnParticles = false;
	}

	public override void Update(float delta)
	{
		_emissionTimer -= delta;
		if (_emissionTimer <= 0f && _canSpawnParticles)
		{
			_zapParticleSystem.AddParticles(new Vector2(Bbox.Center.X, Bbox.Center.Y));
			_emissionTimer = 0.3f;
		}
		base.Update(delta);
	}
}
