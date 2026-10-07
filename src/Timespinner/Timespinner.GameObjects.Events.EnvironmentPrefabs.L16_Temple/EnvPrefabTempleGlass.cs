using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Bosses.Sandman;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L16_Temple;

internal sealed class EnvPrefabTempleGlass : EnvironmentPrefabBase
{
	private readonly Point _spawnPoint;

	private readonly SandmanBossHourglassManager _hourglassManager;

	private float _movementTimer;

	public EnvPrefabTempleGlass(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		_spawnPoint = new Point(Position.X, Position.Y - 24);
		_sprite = _level.GCM.SpSandmanBoss;
		ChangeAnimation(-1);
		Bbox = new Rectangle(0, 0, 16, 16);
		_isAffectedByGravity = false;
		_isFlying = true;
		_isSolid = true;
		_doesUseAppendageCollision = true;
		base.CannotBeGrabbed = true;
		_hourglassManager = new SandmanBossHourglassManager(this, _sprite);
		_appendages.Add(_hourglassManager.TopGlass);
		_appendages.Add(_hourglassManager.BottomGlass);
		_appendages.Add(_hourglassManager.PowerSphere);
		_appendages.Add(_hourglassManager.CollisionAppendage);
		_particleSystems.Add(_hourglassManager.ChargeParticles);
		SetDoesDrawAppendageTrails(value: true, isHost: true, 12, 4f);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			_movementTimer += delta * 1f;
			if (_movementTimer >= (float)Math.PI * 2f)
			{
				_movementTimer -= (float)Math.PI * 2f;
			}
			int num = (int)Math.Round(Math.Cos(_movementTimer) * 16.0);
			int num2 = (int)Math.Round(Math.Sin(_movementTimer) * 16.0);
			Position = new Point(_spawnPoint.X + num, _spawnPoint.Y + num2);
			_hourglassManager.Update(delta);
		}
		base.Update(delta);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		_hourglassManager.SandSwirl.Draw(spriteBatch, isOver: false);
		_hourglassManager.GlowTexture.Draw(spriteBatch);
		base.Draw(spriteBatch);
		_hourglassManager.SandSwirl.Draw(spriteBatch, isOver: true);
	}
}
