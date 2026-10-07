using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L10_Hangar;

internal sealed class EnvPrefabHangarBulwarkForceField : EnvironmentPrefabBase
{
	private const int BboxHeight = 192;

	private const int LazerWidth = 12;

	private const int BboxWidth = 32;

	private const int PositionOffsetX = 8;

	private const int ParticleOffsetX = -6;

	private const int DrawOffsetX = 3;

	private readonly ParticleSystem _trailParticles;

	private readonly HangarBulwarkSparksParticleSystem _sparkParticles;

	private readonly EBulwarkGemType _gemType;

	public EnvPrefabHangarBulwarkForceField(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		_sprite = _level.GCM.SpMiscHangar2;
		_sparkParticles = new HangarBulwarkSparksParticleSystem(_level.GCM.TxParticleEnergy, 1);
		switch (prefabType)
		{
		case EEnvironmentPrefabType.L10_BulwarkForceFieldP:
			_gemType = EBulwarkGemType.Plasma;
			base.AuraColor = new Color(250, 148, 148, 200);
			ChangeAnimation(10);
			break;
		case EEnvironmentPrefabType.L10_BulwarkForceFieldC:
			_gemType = EBulwarkGemType.Chaos;
			base.AuraColor = new Color(200, 160, 100, 200);
			ChangeAnimation(11);
			break;
		default:
			_gemType = EBulwarkGemType.Blood;
			base.AuraColor = new Color(250, 80, 80, 200);
			ChangeAnimation(12);
			break;
		}
		Position = new Point(Position.X + 8, Position.Y);
		Bbox = new Rectangle(0, 0, 32, 192);
		_doesDrawBaseSprite = false;
		base.DrawPlane = EDrawPlane.Back;
		base.DoesDrawAura = true;
		_auraCount = 4f;
		base.AuraSize = 1.5f;
		base.AuraFrequency = 8f;
		_isAffectedByGravity = false;
		_isFlying = true;
		_isSolid = true;
		base.IsTriggerableByMonsters = true;
		base.CanBeTriggered = true;
		base.DoesCollideWithTiles = false;
		_trailParticles = new HangarBulwarkForceFieldParticleSystem(_level.GCM.TxParticleEnergy, 5)
		{
			BaseColor = base.AuraColor.ToVector4()
		};
		_particleSystems.Add(_trailParticles);
		_particleSystems.Add(_sparkParticles);
	}

	public override void Initialize()
	{
		string key = "";
		switch (_gemType)
		{
		case EBulwarkGemType.Plasma:
			key = BossClass.GetSaveKeyByBossType(EBossType.Sorceress);
			break;
		case EBulwarkGemType.Chaos:
			key = BossClass.GetSaveKeyByBossType(EBossType.Demon);
			break;
		case EBulwarkGemType.Blood:
			key = BossClass.GetSaveKeyByBossType(EBossType.Maw);
			break;
		}
		if (_level.GameSave.GetSaveBool(key))
		{
			SilentKill();
		}
		base.Initialize();
	}

	public override void Update(float delta)
	{
		switch (_gemType)
		{
		case EBulwarkGemType.Plasma:
			base.AuraColor = new Color(220, 160, 180, 200);
			break;
		case EBulwarkGemType.Chaos:
			base.AuraColor = new Color(220, 180, 140, 216);
			break;
		default:
			base.AuraColor = new Color(200, 128, 128, 200);
			break;
		}
		if (!base.IsFrozen)
		{
			Vector2 where = new Vector2(Position.X + -6, Position.Y);
			_trailParticles.AddParticles(where, new Vector2(0f, -1f));
		}
		base.Update(delta);
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		bool flag = base.TriggerEvent(who, depth);
		if (flag)
		{
			Point point = new Point(Bbox.Left, who.Bbox.Top);
			who.ManageDamage(1, Vector2.Zero, point, Bbox, EDamageType.Projectile, EDamageElement.None, doesKnockBack: true);
			PlayCue(ESFX.EnvMilitaryLazerTouch, point);
			Point point2 = new Point(Bbox.Left + 4, who.Bbox.Center.Y);
			_sparkParticles.AddParticles(point2.ToVector2());
			_level.AddAnimation(EBattleAnimationType.MediumHitYellow, point2, ETeamSide.Heroes, who.IsFacingLeft);
		}
		return flag;
	}

	public override void DrawAura(SpriteBatch spriteBatch, SpriteEffects toFlip)
	{
		Color auraColor = base.AuraColor;
		Vector2 vector = new Vector2(Bbox.X + 3, Bbox.Y);
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
			float num2 = 12f * num - 12f;
			Rectangle rectangle = new Rectangle((int)(vector.X - num2 / 2f), (int)vector.Y, (int)(12f + num2), _bbox.Height);
			spriteBatch.Draw(_sprite.Texture, new Rectangle((int)(_level.LevelRenderCenter.X - (_level.CameraPosition.X - (float)rectangle.X)), (int)(_level.LevelRenderCenter.Y - (_level.CameraPosition.Y - (float)rectangle.Y)), rectangle.Width, rectangle.Height), _frameSource, auraColor);
		}
	}
}
