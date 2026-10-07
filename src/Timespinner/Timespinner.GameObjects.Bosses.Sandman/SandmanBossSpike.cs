using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Sandman;

internal sealed class SandmanBossSpike : DamageArea
{
	private const int SpikeHeight = 112;

	private const float TimeBeforeSpikeAppears = 1.25f;

	private const float TimeForSpikeToStab = 0.25f;

	private const float TimeForSpikeToLinger = 0.5f;

	private const float TimeForSpikeToStartRetracting = 0.75f;

	private const float TimeForSpikeToRetract = 0.25f;

	private const float TimeForSpikeToBeVisible = 1.25f;

	private const float StabPercentageBeforeCanDamage = 0.15f;

	private readonly Vector2 _sandTextureRatio = new Vector2(2f, 2f);

	private readonly SandmanPortalParticleSystem _portalParticleSystem;

	private readonly SandDrawHelper _sandDrawHelper;

	private bool _isDrawingSand;

	private float _sleepTimer;

	private float _stabTimer;

	private Point _initialPosition;

	private Vector2 _spikeBasePosition;

	internal bool IsFinished { get; private set; }

	public SandmanBossSpike(Level inLevel, Point inPosition, SpriteSheet inSprite, int baseDamage)
		: base(inLevel, inPosition, ETeamSide.Enemies, -1, null)
	{
		_sprite = inSprite;
		base.DrawPlane = EDrawPlane.Front;
		_power = (int)Math.Ceiling((float)baseDamage * 1.1f);
		_bboxOffset = new Point(4, 0);
		Bbox = new Rectangle(_position.X, _position.Y, 8, 112);
		_doesRotateBasedOnVelocity = false;
		_doesCollideWithFloors = false;
		_isAffectedByGravity = false;
		base.DoesCollideWithTiles = false;
		_doesDieOnTiles = false;
		_canDamageThings = false;
		ChangeAnimation(15);
		_sandDrawHelper = new SandDrawHelper(this);
		_portalParticleSystem = new SandmanPortalParticleSystem(_level.GCM.TxParticleEnergy, 5, 16, EDirection.North);
		_doesAutomaticallyEmitParticles = false;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			_sandDrawHelper.Update(delta);
			_portalParticleSystem.Update(delta);
			if (_sleepTimer > 0f)
			{
				_canDamageThings = false;
				_doesDrawBaseSprite = false;
				_sleepTimer -= delta;
				if (_sleepTimer <= 1.25f)
				{
					_portalParticleSystem.AddParticles(_spikeBasePosition);
				}
			}
			else
			{
				if (_stabTimer <= 0f)
				{
					_doesDrawBaseSprite = true;
				}
				float stabTimer = _stabTimer;
				_stabTimer += delta;
				float num;
				if (_stabTimer < 0.75f)
				{
					num = _stabTimer / 0.25f;
					_portalParticleSystem.AddParticles(_spikeBasePosition);
					if (num > 0.15f)
					{
						_canDamageThings = true;
					}
				}
				else
				{
					if (stabTimer < 0.75f)
					{
						_portalParticleSystem.KillOffParticles(0.5f);
					}
					num = 1f - (_stabTimer - 0.75f) / 0.25f;
				}
				num = ((num > 1f) ? 1f : ((float)Math.Sin(num * ((float)Math.PI / 2f))));
				int num2 = -((int)(num * 112f) - 56);
				Position = new Point(_initialPosition.X, _initialPosition.Y + num2);
			}
		}
		base.Update(delta);
		if (_fadeTimer >= _timeToFade)
		{
			IsFinished = true;
		}
	}

	internal void Reset(Point position, float sleepTime)
	{
		_stabTimer = 0f;
		_isFading = false;
		base.DrawColor = Color.White;
		_fadeTimer = 0f;
		IsFinished = false;
		IsFacingLeft = position.X % 16 == 0;
		Position = position;
		_initialPosition = position;
		_spikeBasePosition = _initialPosition.ToVector2();
		SnapBboxToPosition();
		_sleepTimer = sleepTime + 1.25f;
		_life = 1.25f + _sleepTimer;
		if (_portalParticleSystem != null)
		{
			_portalParticleSystem.EmissionDirection = EDirection.North;
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (!_isDrawingSand)
		{
			_isDrawingSand = true;
			_sandDrawHelper.Draw(spriteBatch, this, _sandTextureRatio);
			_isDrawingSand = false;
			_portalParticleSystem.Draw(spriteBatch, _level.LevelRenderCenter, _level.CameraPosition, _level.CameraZoom);
		}
		else
		{
			base.Draw(spriteBatch);
		}
	}
}
