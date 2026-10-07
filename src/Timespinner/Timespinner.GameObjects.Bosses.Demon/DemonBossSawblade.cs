using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Demon;

internal sealed class DemonBossSawblade : DamageArea
{
	private const int SpikeHeight = 24;

	private const int SparkRadius = 34;

	private const float TimeForSpikeToStab = 0.25f;

	private const float TimeForSpikeToLinger = 1f;

	private const float TimeForSpikeToStartRetracting = 1.25f;

	private const float TimeForSpikeToRetract = 0.25f;

	private const float TimeForEntireSpikeSequence = 1.5f;

	private const float StabPercentageBeforeCanDamage = 0.15f;

	private readonly Appendage _parentSpotlight;

	private readonly DemonBossSparksParticleSystem _lazerSparkParticles;

	private float _sleepTimer;

	private float _stabTimer;

	private int _floorHeight;

	private Point _initialPosition;

	private SFXCueInstance _sawLoopCueInstance;

	internal bool IsFinished { get; private set; }

	public DemonBossSawblade(Level inLevel, Appendage parentSpotlight, SpriteSheet inSprite, int baseDamage)
		: base(inLevel, parentSpotlight.Position, ETeamSide.Enemies, -1, null)
	{
		_sprite = inSprite;
		_parentSpotlight = parentSpotlight;
		Reset(Position);
		base.DrawPlane = EDrawPlane.Front;
		_power = (int)Math.Ceiling((float)baseDamage * 1.15f);
		_bboxOffset = new Point(8, 8);
		Bbox = new Rectangle(_position.X, _position.Y, 48, 24);
		_doesRotateBasedOnVelocity = false;
		_doesCollideWithFloors = false;
		_isAffectedByGravity = false;
		base.DoesCollideWithTiles = false;
		_doesDieOnTiles = false;
		_canDamageThings = false;
		ChangeAnimation(28, 3, 0.05f, EAnimationType.Cycle);
		_lazerSparkParticles = new DemonBossSparksParticleSystem(_level.GCM.TxParticleEnergy, 3);
		_particleSystems.Add(_lazerSparkParticles);
		_doesAutomaticallyEmitParticles = false;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (_sleepTimer > 0f)
			{
				_canDamageThings = false;
				_doesDrawBaseSprite = false;
				_sleepTimer -= delta;
			}
			else
			{
				if (_stabTimer <= 0f)
				{
					_doesDrawBaseSprite = true;
					PlayCue(ESFX.BossDemonSawStart, _initialPosition);
					_lazerSparkParticles.ResetLastTetherBase(new Point(Position.X + 34, _floorHeight));
					if (_sawLoopCueInstance == null)
					{
						_sawLoopCueInstance = PlayCue(ESFX.BossDemonSawLoop, isLooped: true);
					}
					else
					{
						_sawLoopCueInstance.Resume();
					}
				}
				_stabTimer += delta;
				float num;
				if (_stabTimer < 1.25f)
				{
					num = _stabTimer / 0.25f;
					if (num > 0.15f)
					{
						_canDamageThings = true;
					}
					_lazerSparkParticles.AddParticles(new Vector2(Position.X + 34, _floorHeight));
				}
				else
				{
					num = 1f - (_stabTimer - 1.25f) / 0.25f;
				}
				num = ((num > 1f) ? 1f : ((float)Math.Sin(num * ((float)Math.PI / 2f))));
				int num2 = -(int)(num * 24f - 12f);
				Position = new Point(_parentSpotlight.Position.X, _initialPosition.Y + num2);
			}
		}
		base.Update(delta);
		if (_fadeTimer >= _timeToFade)
		{
			IsFinished = true;
			if (_sawLoopCueInstance != null)
			{
				_sawLoopCueInstance.Pause(0.15f);
			}
		}
	}

	internal void Reset(Point position)
	{
		_stabTimer = 0f;
		_isFading = false;
		base.DrawColor = Color.White;
		_fadeTimer = 0f;
		IsFinished = false;
		IsFacingLeft = true;
		Position = position;
		_initialPosition = position;
		_floorHeight = _initialPosition.Y;
		SnapBboxToPosition();
		_life = 1.5f + _sleepTimer;
		if (_lazerSparkParticles != null)
		{
			_lazerSparkParticles.KillOffParticles(0f);
		}
	}

	internal void CleanUp()
	{
		if (_sawLoopCueInstance != null && !_sawLoopCueInstance.IsPaused && !_sawLoopCueInstance.IsFinished)
		{
			_sawLoopCueInstance.Pause();
		}
	}
}
