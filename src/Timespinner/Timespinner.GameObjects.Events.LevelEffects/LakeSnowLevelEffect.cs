using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.LevelEffects;

internal class LakeSnowLevelEffect : LevelEffect
{
	private const int SnowEmissionRateRatio = 180;

	private static readonly Vector4 SnowColor = new Vector4(0.9f, 0.8f, 1f, 0.9f);

	private readonly int _maxX16;

	private readonly float _timeBetweenEmission = 0.066f;

	private readonly LakeSnowParticleSystem _nearSnowParticles;

	private readonly LakeSnowParticleSystem _farSnowParticles;

	private bool _hasPopulatedRoomWithSnowOnEntry;

	private float _emissionTimer;

	public LakeSnowLevelEffect(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_maxX16 = _level.CurrentRoom.Width - 1;
		int maxX = _maxX16;
		if (_maxX16 > 0)
		{
			int num = 180 / _maxX16;
			_timeBetweenEmission = 0.066f * (float)num;
		}
		base.DrawPlane = EDrawPlane.Parralax;
		_nearSnowParticles = new LakeSnowParticleSystem(_level.GCM.TxParticleEnergy, maxX, isNear: true)
		{
			BaseColor = SnowColor
		};
		_farSnowParticles = new LakeSnowParticleSystem(_level.GCM.TxParticleEnergy, maxX, isNear: false)
		{
			BaseColor = SnowColor * 0.75f
		};
		_particleSystems.Add(_nearSnowParticles);
		_particleSystems.Add(_farSnowParticles);
	}

	public override void Update(float delta)
	{
		if (!_hasPopulatedRoomWithSnowOnEntry)
		{
			_hasPopulatedRoomWithSnowOnEntry = true;
			int num = (int)((float)_maxX16 * 0.25f);
			for (int i = 0; i < num; i++)
			{
				EmitSnow(shouldBeAboveScreen: false);
			}
			_nearSnowParticles.Update(1f);
			_farSnowParticles.Update(3f);
		}
		if (!base.IsFrozen)
		{
			_emissionTimer -= delta;
			if (_emissionTimer <= 0f)
			{
				_emissionTimer += _timeBetweenEmission;
				EmitSnow(shouldBeAboveScreen: true);
			}
		}
		base.Update(delta);
	}

	private void EmitSnow(bool shouldBeAboveScreen)
	{
		Point point = new Point(_level.NextRandomInt(0, _maxX16), 0);
		if (point.X != -1)
		{
			int num = (shouldBeAboveScreen ? (-32) : _level.NextRandomInt(0, 32));
			Vector2 where = new Vector2(point.X * 16, num);
			if (_level.NextRandomInt(0, 1) == 0)
			{
				_nearSnowParticles.AddParticles(where);
			}
			else
			{
				_farSnowParticles.AddParticles(where);
			}
		}
	}
}
