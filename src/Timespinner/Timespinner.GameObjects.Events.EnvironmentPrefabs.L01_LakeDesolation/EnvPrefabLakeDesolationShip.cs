using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L01_LakeDesolation;

internal sealed class EnvPrefabLakeDesolationShip : EnvironmentPrefabBase
{
	private const int Anim_ShipIndex = 17;

	private const float TimeForFlight = 5.5f;

	private const float TimeToFadeCue = 1f;

	private const float TimeBeforeFadingCue = 4.5f;

	private readonly Appendage _particleAppendage;

	private readonly ShipEngineParticleSystem _engineParticles;

	private bool _isDoingFlightSequence;

	private float _flightTimer;

	private Point _flightStartPoint;

	private Point _flightEndPoint;

	private SFXCueInstance _flightCue;

	public EnvPrefabLakeDesolationShip(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		_sprite = _level.GCM.SpMiscHangar;
		ChangeAnimation(17);
		IsFacingLeft = false;
		_bboxOffset = new Point(0, 0);
		Bbox = new Rectangle(0, 0, 70, 31);
		SnapBboxToPosition();
		_doAppendagesMatchImageFacing = true;
		_isSolid = false;
		_isFlying = true;
		_isAffectedByGravity = false;
		base.DrawPlane = EDrawPlane.Parralax;
		if (base.Appendages.Count > 1)
		{
			_particleAppendage = base.Appendages[0];
			_engineParticles = new ShipEngineParticleSystem(_level.GCM.TxParticleEnergy, 16);
			_particleAppendage.AddParticleSystem(_engineParticles);
		}
		SetCharacterSequenceByName("Idle");
	}

	internal void StartFlight(Point startPoint, Point endPoint)
	{
		_flightStartPoint = startPoint;
		_flightEndPoint = endPoint;
		_flightTimer = 0f;
		_isDoingFlightSequence = true;
		_flightCue = CreateCue(ESFX.CsLakeShip, Position, isLooped: true);
		if (_flightCue != null)
		{
			_flightCue.FadeIn(1f);
			_flightCue.PlayWhenInRange();
		}
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (_isDoingFlightSequence)
			{
				if (_flightTimer < 5.5f)
				{
					float flightTimer = _flightTimer;
					_flightTimer += delta;
					float amount = _flightTimer / 5.5f;
					Position = _flightStartPoint.Lerp(_flightEndPoint, amount);
					if (_flightTimer >= 4.5f && flightTimer < 4.5f && _flightCue != null)
					{
						_flightCue.Stop(1f);
					}
				}
				else
				{
					Position = _flightEndPoint;
					_isFlying = false;
				}
			}
			if (_engineParticles != null)
			{
				_engineParticles.BaseColor = (Color.Teal * 0.5f).ToVector4();
				_engineParticles.AddParticles(_particleAppendage.Position.ToVector2());
			}
		}
		base.Update(delta);
	}

	internal void Remove()
	{
		SilentKill();
	}
}
