using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.LevelEffects;

internal class ForestFirefliesLevelEffect : LevelEffect
{
	private const int MaxTilePickAttempts = 5;

	private const float TimeBetweenEmission = 0.066f;

	private static readonly Vector4 FireflyColor = new Vector4(1f, 0.45f, 0.7f, 0.75f);

	private readonly int _maxX16;

	private readonly int _maxY16;

	private readonly ForestFireflyParticleSystem _nearFireflyParticles;

	private readonly ForestFireflyParticleSystem _farFireflyParticles;

	private float _emissionTimer;

	public ForestFirefliesLevelEffect(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_maxX16 = _level.CurrentRoom.Width - 1;
		_maxY16 = _level.CurrentRoom.Height - 1;
		int howManyEffects = _maxX16 * _maxY16 / 50;
		base.DrawPlane = EDrawPlane.Parralax;
		_nearFireflyParticles = new ForestFireflyParticleSystem(_level.GCM.TxParticleEnergy, howManyEffects, isNear: true)
		{
			BaseColor = FireflyColor
		};
		_farFireflyParticles = new ForestFireflyParticleSystem(_level.GCM.TxParticleEnergy, howManyEffects, isNear: false)
		{
			BaseColor = FireflyColor * 0.75f
		};
		_particleSystems.Add(_nearFireflyParticles);
		_particleSystems.Add(_farFireflyParticles);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			_emissionTimer += delta;
			if (_emissionTimer >= 0.066f)
			{
				_emissionTimer -= 0.066f;
				int i = 0;
				Point a = new Point(-1, -1);
				for (; i < 5; i++)
				{
					Point point = new Point(_level.NextRandomInt(0, _maxX16), _level.NextRandomInt(0, _maxY16));
					if (!_level.BackgroundTiles.ContainsKey(point) && !_level.SolidTiles.ContainsKey(point) && !_level.WaterTiles.ContainsKey(point))
					{
						a = point;
						break;
					}
				}
				if (a.X != -1)
				{
					if (_level.NextRandomInt(0, 1) == 0)
					{
						_nearFireflyParticles.AddParticles(a.Multiply(14f).ToVector2());
					}
					else
					{
						_farFireflyParticles.AddParticles(a.Multiply(10f).ToVector2());
					}
				}
			}
		}
		base.Update(delta);
	}
}
