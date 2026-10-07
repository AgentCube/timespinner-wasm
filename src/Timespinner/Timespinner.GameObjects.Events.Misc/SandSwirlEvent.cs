using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Misc;

internal sealed class SandSwirlEvent : GameEvent
{
	private const int StreamUnitCount = 2;

	private const int SizeBiggerThanParent = 52;

	private const int DefaultStreamWidth = 128;

	private const int UnitOffsetX = 64;

	private const int UnitOffsetY = 64;

	private const int Unit1Frequency = 6;

	private const int Unit2Frequency = 6;

	private const float Unit1Amplitude = 0.35f;

	private const float Unit2Amplitude = 0.35f;

	private const float Unit1ShiftSpeed = -0.5f;

	private const float Unit2ShiftSpeed = -0.5f;

	private const float TimeToFade = 0.25f;

	private static readonly Color SoulStreamColor = Color.White * 0.25f;

	private readonly Color _baseSoulColor;

	private readonly Animate _anchorObject;

	private readonly SoulStreamSwirlParticleSystem _soulSwirlParticles;

	private readonly SpriteSheet _soulStreamSprite;

	private readonly SandStreamUnit[] _streamUnits = new SandStreamUnit[2];

	private bool _isFading;

	private bool _isFadingOut;

	private bool _isRemovedAfterFading;

	private float _fadeTimer;

	private Vector2 _sandOffset;

	private Point _anchorPointA;

	internal float SandScrollSpeedX { get; set; }

	internal float SandScrollSpeedY { get; set; }

	internal float SandJitterSpeed { get; set; }

	public SandSwirlEvent(Level inLevel, Point inPosition, Animate anchorObject)
		: base(inLevel, inPosition, -1, new ObjectTileSpecification())
	{
		_anchorObject = anchorObject;
		_soulStreamSprite = _level.GCM.SpSoulStream;
		_sprite = _soulStreamSprite;
		Bbox = new Rectangle(Position.X, Position.Y, 16, 16);
		_doesDrawBaseSprite = false;
		_doesUseAppendageCollision = false;
		_doAppendagesInheritDrawColor = true;
		_isAffectedByTime = false;
		ChangeAnimation(-1);
		_isFading = true;
		_isFadingOut = false;
		base.DrawColor = Color.Transparent;
		_baseSoulColor = SoulStreamColor;
		_soulSwirlParticles = new SoulStreamSwirlParticleSystem(_level.GCM.SpTimeGateAnimation, 16);
		_particleSystems.Add(_soulSwirlParticles);
		for (int i = 0; i < 2; i++)
		{
			float baseAmplitude = 1f;
			float baseFrequency = 1f;
			float baseShiftXSpeed = 1f;
			float timeOffset = 0f;
			switch (i)
			{
			case 0:
				baseAmplitude = 0.35f;
				baseFrequency = 6f;
				baseShiftXSpeed = -0.5f;
				break;
			case 1:
				baseAmplitude = 0.35f;
				baseFrequency = 6f;
				baseShiftXSpeed = -0.5f;
				timeOffset = (float)Math.PI;
				break;
			}
			SandStreamUnit sandStreamUnit = new SandStreamUnit(this, _level, _soulStreamSprite, baseFrequency, baseAmplitude, baseShiftXSpeed, timeOffset)
			{
				DrawOrigin = new Vector2(0f, 64f)
			};
			_streamUnits[i] = sandStreamUnit;
			_appendages.Add(sandStreamUnit);
		}
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			UpdateLinkMatching();
			UpdateSand(delta);
			UpdateFading(delta);
		}
		base.Update(delta);
	}

	private void UpdateLinkMatching()
	{
		Point anchorPointA = _anchorPointA;
		_anchorPointA = new Point(_anchorObject.Position.X, _anchorObject.Bbox.Top - 52);
		int num = _anchorObject.Bbox.Height + 104;
		if (anchorPointA != _anchorPointA)
		{
			Point position = new Point(_anchorPointA.X + 64, _anchorPointA.Y + 64);
			float scale = (float)num / 128f;
			SandStreamUnit[] streamUnits = _streamUnits;
			foreach (SandStreamUnit sandStreamUnit in streamUnits)
			{
				sandStreamUnit.Position = position;
				sandStreamUnit.Rotation = (float)Math.PI / 2f;
				sandStreamUnit.Scale = scale;
			}
		}
	}

	private void UpdateSand(float delta)
	{
		SandScrollSpeedY = 0.1f;
		SandJitterSpeed = 0.1f;
		_sandOffset.X += SandScrollSpeedX * delta;
		_sandOffset.Y += SandScrollSpeedY * delta;
		if (_sandOffset.X > 1f)
		{
			_sandOffset.X -= 1f;
		}
		if (_sandOffset.Y > 1f)
		{
			_sandOffset.Y -= 1f;
		}
		Vector3 sandValues = new Vector3(_sandOffset.X, _sandOffset.Y, 0f);
		SandStreamUnit[] streamUnits = _streamUnits;
		foreach (SandStreamUnit sandStreamUnit in streamUnits)
		{
			sandStreamUnit.SandValues = sandValues;
		}
	}

	private void UpdateFading(float delta)
	{
		if (_isFading)
		{
			_fadeTimer += delta;
			Color start = (_isFadingOut ? _baseSoulColor : Color.Transparent);
			Color color = (_isFadingOut ? Color.Transparent : _baseSoulColor);
			if (_fadeTimer >= 0.25f)
			{
				_isFading = false;
				base.DrawColor = color;
				if (_isRemovedAfterFading)
				{
					SilentKill();
				}
			}
			else
			{
				float amount = _fadeTimer / 0.25f;
				base.DrawColor = start.SineInterpolate(color, amount);
			}
		}
		else if (!_isFadingOut)
		{
			base.DrawColor = _baseSoulColor;
		}
	}

	internal void DoFade(bool isFadingOut)
	{
		_isFading = true;
		_isFadingOut = isFadingOut;
		_fadeTimer = 0f;
	}

	internal void End()
	{
		DoFade(isFadingOut: true);
		_isRemovedAfterFading = true;
	}

	public void Draw(SpriteBatch spriteBatch, bool isOver)
	{
		SandStreamUnit[] streamUnits = _streamUnits;
		foreach (SandStreamUnit sandStreamUnit in streamUnits)
		{
			sandStreamUnit.DrawColor = base.DrawColor;
			sandStreamUnit.Draw(spriteBatch, isOver);
		}
	}
}
