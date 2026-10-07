using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L02_City;

internal class EnvPrefabCityWaterBubbles : EnvironmentPrefabBase
{
	private const int DistanceToWaterTop = -11;

	private const int DistanceToWaterEmission = -5;

	private const float TimeBetweenEmittingBubbles = 0.05f;

	private readonly Vector2 _emissionPoint;

	private readonly ForcedWaterBubbleParticleSystem _waterBubbles;

	private float _bubbleEmissionTimer;

	public EnvPrefabCityWaterBubbles(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		_waterBubbles = new ForcedWaterBubbleParticleSystem(_level.GCM.TxParticleEnergy, 16)
		{
			WaterTopY = inPosition.Y + -11
		};
		_emissionPoint = new Vector2(inPosition.X, inPosition.Y + -5);
		_particleSystems.Add(_waterBubbles);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			_bubbleEmissionTimer += delta;
			if (_bubbleEmissionTimer >= 0.05f)
			{
				_bubbleEmissionTimer -= 0.05f;
				_waterBubbles.AddParticles(_emissionPoint);
			}
		}
		base.Update(delta);
	}
}
