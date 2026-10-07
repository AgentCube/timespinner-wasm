using System;
using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.NPCs;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L00_Prologue;

internal class EnvPrefabProRock : EnvironmentPrefabBase
{
	private const int RiseHeight = 19;

	private const int WindupWidth = 4;

	internal const float TimeToRise = 1f;

	private const float TimeToWindup = 0.1f;

	private const float TimeToShoot = 0.3f;

	internal const float TimeBeforeRecoiling = 0.4f;

	private static readonly Color OrbAuraColor = new Color(0.25f, 0.25f, 0.75f, 0.5f);

	private bool _isRising;

	private bool _isShooting;

	private int _shootDistanceX;

	private float _stateTimer;

	private Point _startPoint;

	internal YorneNPC TargetYorne { get; set; }

	public EnvPrefabProRock(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		_sprite = _level.GCM.SpPlatforms;
		Bbox = new Rectangle(inPosition.X, inPosition.Y, 4, 4);
		ChangeAnimation(51);
	}

	public override void Update(float delta)
	{
		if (_isRising)
		{
			UpdateRising(delta);
		}
		else if (_isShooting)
		{
			UpdateShooting(delta);
		}
		base.Update(delta);
	}

	private void UpdateRising(float delta)
	{
		if (_stateTimer < 1f)
		{
			_stateTimer += delta;
			float num = 1f - _stateTimer / 1f;
			int num2 = (int)Math.Round(Math.Cos(num * ((float)Math.PI / 2f)) * 19.0);
			Position = new Point(_startPoint.X, _startPoint.Y - num2);
		}
	}

	private void UpdateShooting(float delta)
	{
		_stateTimer += delta;
		if (_stateTimer < 0.4f)
		{
			if (_stateTimer <= 0.1f)
			{
				float num = _stateTimer / 0.1f;
				int num2 = (int)Math.Round(Math.Sin(num * ((float)Math.PI / 2f)) * 4.0);
				Position = new Point(_startPoint.X + num2, _startPoint.Y);
			}
			else
			{
				float num3 = 1f - (_stateTimer - 0.1f) / 0.3f;
				int num4 = (int)Math.Round(Math.Cos(num3 * ((float)Math.PI / 2f)) * (double)_shootDistanceX);
				Position = new Point(_startPoint.X + 4 - num4, _startPoint.Y);
			}
			return;
		}
		if (TargetYorne != null)
		{
			TargetYorne.Recoil();
			_level.AddAnimation(EBattleAnimationType.SmallHit, Bbox.Center, ETeamSide.Neutral, isFacingRight: false, doesPlaySFX: false);
		}
		base.DoesDrawAura = false;
		_isShooting = false;
		_isAffectedByGravity = true;
		_isFlying = false;
		base.DoesCollideWithTiles = true;
		_doesBounceOnGround = true;
		_velocity = new Vector2(500f, 0f);
	}

	internal void Levitate()
	{
		_isRising = true;
		_isShooting = false;
		_stateTimer = 0f;
		_isAffectedByGravity = false;
		_isFlying = true;
		base.DoesDrawAura = true;
		base.AuraFrequency = 9f;
		base.AuraCount = 5f;
		base.AuraColor = OrbAuraColor;
		base.AuraSize = 1f;
		base.AuraOffset = new Vector2(2f, 2.5f);
		_startPoint = Position;
	}

	internal void Shoot()
	{
		_isRising = false;
		_isShooting = true;
		_stateTimer = 0f;
		_startPoint = Position;
		_shootDistanceX = _startPoint.X - ((TargetYorne == null) ? 64 : TargetYorne.Position.X);
	}
}
