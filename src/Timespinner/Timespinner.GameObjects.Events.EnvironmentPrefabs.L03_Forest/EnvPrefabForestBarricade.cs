using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.StatusParticleEffects;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Misc;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L03_Forest;

internal sealed class EnvPrefabForestBarricade : EnvironmentPrefabBase
{
	private const string SaveKey = "3_Barricade";

	private static readonly Vector4 AshColor = new Vector4(0.8f, 0.6f, 0.4f, 1f);

	private readonly DisintegrateAshParticleSystem _ashParticleSystem;

	private readonly DisintegrateFireParticleSystem _burningParticleSystem;

	private bool _isBurning;

	private float _burningScriptTimer;

	public EnvPrefabForestBarricade(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		IsFacingLeft = true;
		_doAppendagesInheritDrawColor = true;
		_doAppendagesMatchImageFacing = true;
		_sprite = _level.GCM.SpPetrifiedVines;
		ChangeAnimation(-1);
		Bbox = new Rectangle(inPosition.X, inPosition.Y, 16, 64);
		base.DoesCollideWithTiles = false;
		base.IsAffectedByTime = true;
		_isAffectedByGravity = false;
		_isFlying = true;
		_isSolid = true;
		base.IsTriggerableByMonsters = true;
		base.DoesCollideWithProjectiles = true;
		_ashParticleSystem = new DisintegrateAshParticleSystem(_level.GCM.TxParticleEnergy, 12)
		{
			BaseColor = AshColor
		};
		_burningParticleSystem = new DisintegrateFireParticleSystem(_sprite, 23, 4, 64);
		_particleSystems.Add(_ashParticleSystem);
		_particleSystems.Add(_burningParticleSystem);
	}

	public override bool ProjectileTriggerEvent(Projectile projectile, Vector2 depth)
	{
		bool flag = false;
		if (!base.IsFrozen && !_isBurning)
		{
			flag = base.ProjectileTriggerEvent(projectile, depth);
			if (flag && projectile.DamageElement == EDamageElement.Fire)
			{
				_isBurning = true;
				_level.GameSave.SetValue("3_Barricade", value: true);
				PlayCue(ESFX.EnvVinesBurning);
			}
		}
		return flag;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && _isBurning)
		{
			UpdateDeathScript(delta);
		}
		base.Update(delta);
	}

	private void UpdateDeathScript(float delta)
	{
		Color end = new Color(1f, 0.6f, 0.6f, 1f);
		_burningScriptTimer += delta;
		if (_burningScriptTimer < 2f)
		{
			float num = 1f;
			if (_burningScriptTimer < 1f)
			{
				num = _burningScriptTimer / 1f;
				base.DrawColor = Color.White.Lerp(end, num);
			}
			int num2 = -64;
			int bottom = (int)(_level.NextRandomDouble() * (double)num2) + Position.Y;
			int left = Bbox.Left - 16;
			int right = Bbox.Right + 16;
			_burningParticleSystem.AddParticles(left, right, bottom);
			_ashParticleSystem.AddParticles(left, right, bottom);
		}
		else
		{
			Vector2 force = new Vector2(0f, 100f);
			Point origin = new Point(Position.X, Position.Y - 80);
			Appendage appendage = _appendages[0];
			DebrisEvent.CreateFromAppendages(appendage.Appendages, force, origin, _sprite, DebrisEvent.EDebrisDeathType.Dust, Point.Zero);
			_level.JukeBox.PlayCue(ESFX.EnemyEngineerLogBreak);
			SilentKill();
		}
	}
}
