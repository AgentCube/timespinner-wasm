using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies._16_Temple;

internal sealed class TempleZealIceUnit : Appendage
{
	private const int FinalBboxHeight = 56;

	private const int Anim_SpikeStart = 10;

	internal const float TimeToLive = 1f;

	private const float TimeToRise = 0.15f;

	private const float TimeToFall = 0.2f;

	private const float TimeToLinger = 0.65f;

	private const float TimeBeforeFalling = 0.8f;

	private const int PieceCount = 12;

	private const int ZoneOffsetY = 8;

	private const float TimeBetweenAddingPieces = 0.1f;

	private readonly TempleZealZonePiece[] _pieces = new TempleZealZonePiece[12];

	private readonly Rectangle _baseFrameSource;

	private bool _isUpsideDown;

	private bool _isGoingLeft;

	private bool _isSkinny;

	private float _lifeTimer;

	private float _addTimer;

	private Point _startingPoint;

	internal bool IsActive { get; set; }

	internal bool IsFinished { get; private set; }

	public TempleZealIceUnit(Animate parent, Point bboxDimensions, Point inBboxOffset, Level inLevel, SpriteSheet inSprite)
		: base(parent, bboxDimensions, inBboxOffset, inLevel, inSprite)
	{
		_baseFrameSource = _sprite.GetFrameSource(10);
		ChangeAnimation(10);
		_doAppendagesInheritDrawColor = false;
		_doesUseAppendageCollision = false;
		_doAppendagesMatchImageFacing = false;
	}

	internal void Reset(Point position, bool isGoingLeft, bool isUpsideDown, bool isSkinny)
	{
		_lifeTimer = 0f;
		_isUpsideDown = isUpsideDown;
		_isGoingLeft = isGoingLeft;
		_isSkinny = isSkinny;
		_addTimer = 0f;
		IsFlippedVertically = isUpsideDown;
		_startingPoint = position;
		Position = position;
		IsActive = true;
		IsFinished = false;
		base.DoesDrawAura = true;
		base.AuraColor = new Color(16, 64, 200, 16);
		base.AuraSize = 0f;
		base.AuraFrequency = 12f;
		base.AuraCount = 4f;
		_doesDrawBaseSprite = false;
		base.DoesDrawBoundingBox = false;
		IsFacingLeft = _level.NextRandomDouble() < 0.5;
		Update(0f);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && !IsFinished)
		{
			_lifeTimer += delta;
			float num = 1f;
			if (_lifeTimer < 0.15f)
			{
				num = _lifeTimer / 0.15f;
				base.DoesCollideWithAnything = false;
			}
			else if (_lifeTimer < 0.8f)
			{
				base.DoesCollideWithAnything = true;
			}
			else if (_lifeTimer < 1f)
			{
				num = 1f - (_lifeTimer - 0.8f) / 0.2f;
				base.DoesCollideWithAnything = false;
			}
			else
			{
				base.DoesCollideWithAnything = false;
				IsFinished = true;
				num = 0f;
			}
			double num2 = 1.0 - Math.Cos((float)Math.PI / 2f * num);
			int num3 = (int)(num2 * 56.0);
			int height = (int)(num2 * (double)_baseFrameSource.Height);
			Bbox = new Rectangle(Position.X, Position.Y, _baseFrameSource.Width, num3);
			_frameSource = new Rectangle(_baseFrameSource.X, _baseFrameSource.Y, _baseFrameSource.Width, height);
			if (_isUpsideDown)
			{
				Position = new Point(_startingPoint.X, _startingPoint.Y + num3);
			}
			else
			{
				Position = _startingPoint;
			}
			SnapBboxToPosition();
		}
		if (!base.IsFrozen && IsActive && _lifeTimer < 0.8f)
		{
			_addTimer -= delta;
			if (_addTimer <= 0f)
			{
				_addTimer += 0.1f;
				AddPiece();
			}
		}
		base.Update(delta);
	}

	private void AddPiece()
	{
		Point origin = (_isUpsideDown ? new Point(Position.X, Bbox.Top - 8) : new Point(Position.X, Position.Y + 8));
		for (int i = 0; i < 12; i++)
		{
			if (_pieces[i] == null)
			{
				TempleZealZonePiece templeZealZonePiece = new TempleZealZonePiece(this, _level, _sprite, _isUpsideDown);
				templeZealZonePiece.Reset(origin, !_isGoingLeft, _isSkinny);
				_pieces[i] = templeZealZonePiece;
				_appendages.Add(templeZealZonePiece);
				break;
			}
			if (!_pieces[i].IsActive)
			{
				_pieces[i].Reset(origin, !_isGoingLeft, _isSkinny);
				break;
			}
		}
	}
}
