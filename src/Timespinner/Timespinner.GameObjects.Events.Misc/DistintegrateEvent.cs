using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.StatusParticleEffects;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Misc;

internal sealed class DistintegrateEvent : GameEvent
{
	private const float TimeForEntireSequence = 0.75f;

	private readonly EDisintegrateType _disintegrateType;

	private readonly bool _hasBurningParticles;

	private readonly int _sequenceTop;

	private readonly int _sequenceBottom;

	private readonly int _sequenceTotalHeight;

	private readonly int _halfFlameParticleHeight;

	private readonly Animate _hostCorpse;

	private readonly DisintegrateAshParticleSystem _ashParticleSystem;

	private readonly DisintegrateFireParticleSystem _burningParticleSystem;

	private bool _isFinished;

	private int _sequencePosition;

	private int _currentLeft;

	private int _currentRight;

	private float _sequenceTimer;

	internal DistintegrateEvent(Animate target, SpriteSheet sprite, EDisintegrateType disintegrateType, int particleStart, int particleLength, Vector4 ashColor)
		: base(target.Level, target.Position, -1, new ObjectTileSpecification())
	{
		_sprite = sprite;
		_disintegrateType = disintegrateType;
		_hostCorpse = target;
		_isAffectedByGravity = true;
		_isAffectedByLevelBounds = false;
		base.DoesCollideWithTiles = false;
		_isAffectedByTime = true;
		base.CanBeTriggered = false;
		_ashParticleSystem = new DisintegrateAshParticleSystem(_level.GCM.TxParticleEnergy, 2)
		{
			BaseColor = ashColor
		};
		_particleSystems.Add(_ashParticleSystem);
		_hasBurningParticles = particleStart >= 0;
		if (_hasBurningParticles)
		{
			_burningParticleSystem = new DisintegrateFireParticleSystem(_sprite, particleStart, particleLength, 20);
			_particleSystems.Add(_burningParticleSystem);
		}
		_halfFlameParticleHeight = (int)Math.Ceiling((float)_sprite.GetFrameSource(particleStart).Height * 0.5f);
		_currentLeft = Position.X;
		_currentRight = Position.X;
		int num = int.MaxValue;
		int num2 = int.MinValue;
		for (int i = -1; i < _hostCorpse.Appendages.Count; i++)
		{
			Animate animate = null;
			if (i == -1)
			{
				if (_hostCorpse.DoesDrawBaseSprite)
				{
					animate = _hostCorpse;
				}
			}
			else
			{
				animate = _hostCorpse.Appendages[i];
			}
			if (animate != null && animate.AnimationStart != -1)
			{
				int height = animate.FrameSource.Height;
				int y = animate.DrawPosition.Y;
				int num3 = (int)animate.DrawOrigin.Y;
				int num4 = y + (height - num3);
				int num5 = y - num3;
				if (num5 < num)
				{
					num = num5;
				}
				if (num4 > num2)
				{
					num2 = num4;
				}
			}
		}
		_sequenceBottom = num2;
		_sequenceTop = num;
		_sequenceTotalHeight = _sequenceBottom - _sequenceTop;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (!_isFinished)
			{
				if (_sequenceTimer <= 0f)
				{
					EDisintegrateType disintegrateType = _disintegrateType;
					if (disintegrateType == EDisintegrateType.Chaos)
					{
						_level.PlayCue(ESFX.EnemyDemonDeathFire, Position, isLooped: false);
					}
				}
				_sequenceTimer += delta;
				if (_sequenceTimer > 0.75f)
				{
					_isFinished = true;
				}
				else
				{
					float num = _sequenceTimer / 0.75f;
					_sequencePosition = _sequenceBottom - (int)Math.Ceiling(num * (float)_sequenceTotalHeight);
					if (_hasBurningParticles)
					{
						_burningParticleSystem.AddParticles(_currentLeft, _currentRight, _sequencePosition - _halfFlameParticleHeight);
					}
					_ashParticleSystem.AddParticles(_currentLeft, _currentRight, _sequencePosition);
				}
			}
			else if ((!_hasBurningParticles || _burningParticleSystem.AreParticlesDone) && _ashParticleSystem.AreParticlesDone)
			{
				Kill();
			}
		}
		base.Update(delta);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (!_isFinished)
		{
			_currentLeft = Position.X;
			_currentRight = Position.X;
			DrawAppendagePieces(spriteBatch, drawUnder: true);
			if (_hostCorpse.DoesDrawBaseSprite)
			{
				DrawPiece(spriteBatch, _hostCorpse);
			}
			DrawAppendagePieces(spriteBatch, drawUnder: false);
		}
		if (_hasBurningParticles)
		{
			_burningParticleSystem.Draw(spriteBatch, _level.LevelRenderCenter, _level.CameraPosition, _level.CameraZoom);
		}
		_ashParticleSystem.Draw(spriteBatch, _level.LevelRenderCenter, _level.CameraPosition, _level.CameraZoom);
	}

	private void DrawAppendagePieces(SpriteBatch spriteBatch, bool drawUnder)
	{
		foreach (Appendage appendage in _hostCorpse.Appendages)
		{
			if ((drawUnder && appendage.DrawPriority <= 0) || (!drawUnder && appendage.DrawPriority > 0))
			{
				DrawPiece(spriteBatch, appendage);
			}
		}
	}

	private void DrawPiece(SpriteBatch spriteBatch, Animate piece)
	{
		SpriteEffects spriteEffects = piece.SpriteEffects;
		if (!piece.IsImageFacingLeft)
		{
			spriteEffects = SpriteEffects.FlipHorizontally | spriteEffects;
		}
		Vector2 drawOrigin = piece.DrawOrigin;
		int height = piece.FrameSource.Height;
		Point drawPosition = piece.DrawPosition;
		int num = (int)((float)drawPosition.Y + ((float)height - drawOrigin.Y));
		int num2 = (int)((float)drawPosition.Y - drawOrigin.Y);
		if (_sequencePosition > num2)
		{
			int width = piece.FrameSource.Width;
			int num3 = (int)((float)drawPosition.X - drawOrigin.X);
			int num4 = (int)((float)drawPosition.X + ((float)width - drawOrigin.X));
			if (num3 < _currentLeft)
			{
				_currentLeft = num3;
			}
			if (num4 > _currentRight)
			{
				_currentRight = num4;
			}
			Vector2 value = CameraizePoint(piece.DrawPosition);
			value = Vector2.Subtract(_level.LevelRenderCenter, value);
			Rectangle frameSource = piece.FrameSource;
			if (num > _sequencePosition)
			{
				frameSource.Height -= num - _sequencePosition;
			}
			spriteBatch.Draw(destinationRectangle: new Rectangle((int)(value.X - drawOrigin.X), (int)(value.Y - drawOrigin.Y), frameSource.Width, frameSource.Height), texture: _sprite.Texture, sourceRectangle: frameSource, color: Color.White, rotation: 0f, origin: drawOrigin, effects: spriteEffects, layerDepth: 0f);
		}
	}
}
