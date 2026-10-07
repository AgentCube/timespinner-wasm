using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Z_Raven;

internal sealed class RavenBossWindZone : GameEvent
{
	private const int BoxWidth = 480;

	private const int BoxHeight = 200;

	private const int WindSpeedX = 75;

	private const float ParticleEmissionTime = 0.07f;

	private readonly RavenBossWindyParticleSystem _windyParticles;

	private readonly SFXCueInstance _windyLoopCueInstance;

	private float _particleEmissionTimer;

	private Vector2 _particleEmissionPosition;

	internal Vector2 CurrentVector = Vector2.Zero;

	internal bool IsActive { get; private set; }

	public RavenBossWindZone(Level inLevel, Point inPosition, ObjectTileSpecification objectSpec, SpriteSheet sprite)
		: base(inLevel, inPosition, -1, objectSpec)
	{
		_doesDrawBaseSprite = false;
		Bbox = new Rectangle(inPosition.X, inPosition.Y, 480, 200);
		base.IsLostWhenNotTouching = true;
		base.IsLostWhenNotGrounded = false;
		_isSolid = false;
		_doesPersist = true;
		_isAffectedByGravity = false;
		_isFlying = true;
		_isRepeatedTrigger = true;
		IsFacingLeft = true;
		_isAffectedByTime = true;
		base.IsTriggerableByMonsters = false;
		_windyParticles = new RavenBossWindyParticleSystem(sprite, 20, 200);
		_particleSystems.Add(_windyParticles);
		_windyLoopCueInstance = CreateCue(ESFX.BossRavenWindyLoop, Position, isLooped: true);
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		if (depth != Vector2.Zero)
		{
			who.AddMovingPlatform(this);
		}
		return base.TriggerEvent(who, depth);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && IsActive)
		{
			_particleEmissionTimer -= delta;
			if (_particleEmissionTimer < 0f)
			{
				_particleEmissionTimer += 0.07f;
				_windyParticles.AddParticles(_particleEmissionPosition);
			}
		}
		base.Update(delta);
		if (!base.IsFrozen)
		{
			base.AmountMovedLastStep = (IsActive ? new Vector2(CurrentVector.X * delta, 0f) : Vector2.Zero);
		}
	}

	internal void Deactivate()
	{
		IsActive = false;
		if (_windyLoopCueInstance != null && !_windyLoopCueInstance.IsFinished)
		{
			_windyLoopCueInstance.Pause(0.25f);
		}
	}

	internal void Reset(Point position, bool isFacingLeft)
	{
		IsActive = true;
		IsFacingLeft = isFacingLeft;
		CurrentVector = new Vector2(isFacingLeft ? (-75) : 75, 0f);
		int num = Bbox.Width / 2;
		int num2 = Bbox.Height / 2;
		Position = new Point(position.X + (isFacingLeft ? (-num) : num), position.Y + num2);
		_particleEmissionPosition = position.ToVector2();
		_windyParticles.IsBlowingToTheLeft = isFacingLeft;
		_windyParticles.KillOffParticles(0f);
		if (_windyLoopCueInstance != null && !_windyLoopCueInstance.IsFinished)
		{
			_windyLoopCueInstance.FadeIn(0.25f);
			if (_windyLoopCueInstance.IsManuallyPaused)
			{
				_windyLoopCueInstance.Resume();
			}
			else
			{
				_windyLoopCueInstance.PlayWhenInRange();
			}
		}
	}
}
