using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.LevelEffects;

internal class NightmareLevelEffect : LevelEffect
{
	private const int SnowEmissionRateRatio = 180;

	private readonly int _maxX16;

	private readonly int _levelBottomY;

	private readonly float _timeBetweenEmission = 0.066f;

	private readonly NightmareDripParticleSystem _nearDripParticles;

	private readonly NightmareDripParticleSystem _farDripParticles;

	private static readonly Vector4 DripColor = new Vector4(0.6f, 0.4f, 0.2f, 0.9f);

	private bool _hasPopulatedRoomWithParticlesOnEntry;

	private float _emissionTimer;

	public NightmareLevelEffect(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_levelBottomY = _level.CurrentRoom.Height * 16;
		_maxX16 = _level.CurrentRoom.Width - 1;
		int maxX = _maxX16;
		if (_maxX16 > 0)
		{
			int num = 180 / _maxX16;
			_timeBetweenEmission = 0.066f * (float)num;
		}
		base.DrawPlane = EDrawPlane.Parralax;
		_nearDripParticles = new NightmareDripParticleSystem(_level.GCM.TxParticleEnergy, maxX, isNear: true)
		{
			BaseColor = DripColor
		};
		_farDripParticles = new NightmareDripParticleSystem(_level.GCM.TxParticleEnergy, maxX, isNear: false)
		{
			BaseColor = DripColor * 0.75f
		};
		_particleSystems.Add(_nearDripParticles);
		_particleSystems.Add(_farDripParticles);
	}

	public override void Update(float delta)
	{
		if (!_hasPopulatedRoomWithParticlesOnEntry)
		{
			_hasPopulatedRoomWithParticlesOnEntry = true;
			int num = (int)((float)_maxX16 * 0.25f);
			for (int i = 0; i < num; i++)
			{
				EmitDrip(shouldBeAboveScreen: false);
			}
			_nearDripParticles.Update(1f);
			_farDripParticles.Update(3f);
			_level.PermanentlyFreezeTime();
		}
		_isFrozen = false;
		_emissionTimer -= delta;
		if (_emissionTimer <= 0f)
		{
			_emissionTimer += _timeBetweenEmission;
			EmitDrip(shouldBeAboveScreen: true);
		}
		base.Update(delta);
	}

	private void EmitDrip(bool shouldBeAboveScreen)
	{
		Point point = new Point(_level.NextRandomInt(0, _maxX16), 0);
		if (point.X != -1)
		{
			int num = (shouldBeAboveScreen ? _levelBottomY : (_levelBottomY - _level.NextRandomInt(0, 64))) - 32;
			Vector2 where = new Vector2(point.X * 16, num);
			if (_level.NextRandomInt(0, 1) == 0)
			{
				_nearDripParticles.AddParticles(where);
			}
			else
			{
				_farDripParticles.AddParticles(where);
			}
		}
	}
}
