using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L10_Hangar;

internal sealed class EnvPrefabHangarCrashedShip : EnvironmentPrefabBase
{
	private const int SmokeFramesToPopulate = 32;

	private const float TimeBetweenSmokeEmission = 0.033f;

	private readonly Appendage _smokeEmissionAppendage;

	private readonly SmokePlumeParticleSystem _smokeParticleSystem;

	private float _smokeEmissionTimer;

	public EnvPrefabHangarCrashedShip(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		_sprite = _level.GCM.SpMiscHangar;
		ChangeAnimation(0);
		IsFacingLeft = true;
		_bboxOffset = new Point(0, 0);
		Bbox = new Rectangle(0, 0, 240, 179);
		_doAppendagesMatchImageFacing = true;
		_isSolid = false;
		_isAffectedByGravity = false;
		base.DrawPlane = EDrawPlane.Back;
		if (base.Appendages.Count > 0)
		{
			_smokeParticleSystem = new SmokePlumeParticleSystem(_level.GCM.TxParticleSmoke, 12);
			_smokeEmissionAppendage = base.Appendages[0];
			_smokeEmissionAppendage.AddParticleSystem(_smokeParticleSystem);
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		for (int i = 0; i < 32; i++)
		{
			EmitSmoke();
			_smokeParticleSystem.Update(0.033f);
		}
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && _smokeParticleSystem != null)
		{
			_smokeEmissionTimer -= delta;
			if (_smokeEmissionTimer <= 0f)
			{
				_smokeEmissionTimer += 0.033f;
				EmitSmoke();
			}
		}
		base.Update(delta);
	}

	private void EmitSmoke()
	{
		Point position = _smokeEmissionAppendage.Position;
		_smokeParticleSystem.AddParticles(position.ToVector2());
	}
}
