using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L15_DarkForest;

internal sealed class EnvPrefabDarkForestSmoke : EnvironmentPrefabBase
{
	private const int EmissionOffsetX = 8;

	private const int IterationsToInitialize = 30;

	private const float TimeBetweenEmissions = 0.03f;

	private readonly bool _isEmittingLeft;

	private readonly Vector2 _emissionPosition;

	private readonly DoorSmokeParticleSystem _smokeParticles;

	private bool _hasUpdatedYet;

	private float _emissionTimer;

	public EnvPrefabDarkForestSmoke(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		_isEmittingLeft = objectSpec == null || !objectSpec.IsFlippedHorizontally;
		_emissionPosition = new Vector2(Position.X + (_isEmittingLeft ? 8 : (-8)), Position.Y);
		_smokeParticles = new DoorSmokeParticleSystem(_level.GCM.TxParticleSmoke, 12, _isEmittingLeft);
		_particleSystems.Add(_smokeParticles);
	}

	public override void Update(float delta)
	{
		_isFrozen = false;
		if (!_hasUpdatedYet)
		{
			_hasUpdatedYet = true;
			for (int i = 0; i < 30; i++)
			{
				_smokeParticles.AddParticles(_emissionPosition);
				_smokeParticles.Update(0.03f);
			}
		}
		_emissionTimer -= delta;
		if (_emissionTimer <= 0f)
		{
			_emissionTimer += 0.03f;
			_smokeParticles.AddParticles(_emissionPosition);
		}
		base.Update(delta);
	}
}
