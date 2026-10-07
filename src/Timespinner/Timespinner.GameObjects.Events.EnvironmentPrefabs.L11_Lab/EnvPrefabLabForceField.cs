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

internal sealed class EnvPrefabLabForceField : EnvironmentPrefabBase
{
	private const float TimeToFade = 0.75f;

	private static readonly Color BaseAuraColor = new Color(200, 150, 60, 200);

	private readonly ParticleSystem _trailParticles;

	private bool _isFading;

	private float _fadeTimer;

	public EnvPrefabLabForceField(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		_sprite = _level.GCM.SpMiscLab;
		ChangeAnimation(34);
		Position = new Point(Position.X + 1, Position.Y);
		Bbox = new Rectangle(0, 0, 6, 80);
		_doesDrawBaseSprite = false;
		base.DoesDrawAura = true;
		base.AuraColor = BaseAuraColor;
		_auraCount = 4f;
		base.AuraSize = 1.5f;
		base.AuraFrequency = 8f;
		_isAffectedByGravity = false;
		_isFlying = true;
		_isSolid = true;
		base.IsTriggerableByMonsters = true;
		base.CanBeTriggered = true;
		base.DoesCollideWithTiles = false;
		_trailParticles = new LabForceFieldStreamParticleSystem(_level.GCM.TxParticleEnergy, 5);
		_particleSystems.Add(_trailParticles);
	}

	public override void Initialize()
	{
		if (_level.GameSave.GetSaveBool("11_LabPower"))
		{
			SilentKill();
		}
		base.Initialize();
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (!_isFading)
			{
				Vector2 where = Position.ToVector2();
				_trailParticles.AddParticles(where, new Vector2(0f, -1f));
				if (_level.IsPowerOff)
				{
					_isFading = true;
					_isSolid = false;
				}
			}
			else
			{
				_fadeTimer += delta;
				if (_fadeTimer > 0.75f)
				{
					SilentKill();
				}
				else
				{
					float num = _fadeTimer / 0.75f;
					base.AuraColor = BaseAuraColor * (1f - num);
				}
			}
		}
		base.Update(delta);
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		bool flag = base.TriggerEvent(who, depth);
		if (flag)
		{
			who.ManageDamage(1, Vector2.Zero, Bbox.Center, Bbox, EDamageType.Projectile, EDamageElement.None, doesKnockBack: true);
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
