using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L11_Lab;

internal sealed class EnvPrefabHangarForceField : EnvironmentPrefabBase
{
	private const int SmokeFramesToPopulate = 32;

	private const float TimeBetweenSmokeEmission = 0.033f;

	private readonly bool _isDemonDead;

	private readonly bool _isMawDead;

	private readonly bool _isAelanaDead;

	private readonly bool _isPastCleared;

	private readonly ParticleSystem _trailParticles;

	private readonly SmallSmokePlumeParticleSystem _smokeParticleSystem;

	private bool _isDead;

	public EnvPrefabHangarForceField(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		_sprite = _level.GCM.SpMiscHangar;
		ChangeAnimation(5);
		Position = new Point(Position.X + 1, Position.Y);
		Bbox = new Rectangle(0, 0, 36, 192);
		_doesDrawBaseSprite = false;
		base.DoesDrawAura = true;
		base.AuraColor = new Color(60, 150, 200, 200);
		_auraCount = 4f;
		base.AuraSize = 1.5f;
		base.AuraFrequency = 8f;
		_isAffectedByGravity = false;
		_isFlying = true;
		_isSolid = true;
		base.IsTriggerableByMonsters = true;
		base.CanBeTriggered = true;
		base.DoesCollideWithTiles = false;
		_trailParticles = new HangarForceFieldStreamParticleSystem(_level.GCM.TxParticleEnergy, 5);
		_smokeParticleSystem = new SmallSmokePlumeParticleSystem(_level.GCM.TxParticleSmoke, 8);
		_particleSystems.Add(_trailParticles);
		_isDemonDead = _level.GameSave.GetSaveBool(BossClass.GetSaveKeyByBossType(EBossType.Demon));
		_isMawDead = _level.GameSave.GetSaveBool(BossClass.GetSaveKeyByBossType(EBossType.Maw));
		_isAelanaDead = _level.GameSave.GetSaveBool(BossClass.GetSaveKeyByBossType(EBossType.Sorceress));
		_isPastCleared = _level.GameSave.GetSaveBool("IsPastCleared");
	}

	public override void Initialize()
	{
		if (!_isPastCleared)
		{
			switch (base.PrefabType)
			{
			case EEnvironmentPrefabType.L10_ForceFieldA:
				_isDead = _isDemonDead;
				base.AuraColor = new Color(200, 160, 100, 200);
				break;
			case EEnvironmentPrefabType.L10_ForceFieldB:
				_isDead = _isAelanaDead;
				base.AuraColor = new Color(250, 148, 148, 200);
				break;
			case EEnvironmentPrefabType.L10_ForceFieldC:
				_isDead = _isMawDead;
				base.AuraColor = new Color(250, 80, 80, 200);
				break;
			}
		}
		else
		{
			_isDead = true;
		}
		if (_isDead)
		{
			_particleSystems.Add(_smokeParticleSystem);
			base.DoesDrawAura = false;
			Vector2 where = Position.ToVector2();
			for (int i = 0; i < 32; i++)
			{
				_smokeParticleSystem.AddParticles(where);
				_smokeParticleSystem.Update(0.033f);
			}
		}
		else
		{
			_trailParticles.BaseColor = base.AuraColor.ToVector4();
			_particleSystems.Add(_trailParticles);
		}
		base.Initialize();
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			Vector2 where = Position.ToVector2();
			if (_isDead)
			{
				_smokeParticleSystem.AddParticles(where);
			}
			else
			{
				_trailParticles.AddParticles(where, new Vector2(0f, -1f));
			}
		}
		base.Update(delta);
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		bool flag = false;
		if (!_isDead)
		{
			flag = base.TriggerEvent(who, depth);
			if (flag)
			{
				who.ManageDamage(1, Vector2.Zero, Bbox.Center, Bbox, EDamageType.Projectile, EDamageElement.Fire, doesKnockBack: true);
			}
		}
		return flag;
	}

	public override void DrawAura(SpriteBatch spriteBatch, SpriteEffects toFlip)
	{
		Color auraColor = base.AuraColor;
		Vector2 vector = new Vector2(_bbox.X, _bbox.Y);
		float num = 1f;
		auraColor *= 0.5f;
		for (int i = 0; (float)i < _auraCount; i++)
		{
			num += (float)((Math.Cos((double)base.AuraFrequency * ((double)_auraTimer + 0.4 * (double)i)) + 1.0) / 20.0);
			if (num > base.AuraSize)
			{
				num = 0.9f;
			}
			auraColor *= 0.9f;
			float num2 = (float)_bbox.Width * num - (float)_bbox.Width;
			Rectangle rectangle = new Rectangle((int)(vector.X - num2 / 2f), (int)vector.Y, (int)((float)_bbox.Width + num2), _bbox.Height);
			spriteBatch.Draw(_sprite.Texture, new Rectangle((int)(_level.LevelRenderCenter.X - (_level.CameraPosition.X - (float)rectangle.X)), (int)(_level.LevelRenderCenter.Y - (_level.CameraPosition.Y - (float)rectangle.Y)), rectangle.Width, rectangle.Height), _frameSource, auraColor);
		}
	}
}
