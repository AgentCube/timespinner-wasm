using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Misc;

internal sealed class SoulStreamEvent : GameEvent
{
	private const int StreamUnitCount = 3;

	private const int DefaultStreamWidth = 128;

	private const int UnitOffsetX = 64;

	private const int UnitOffsetY = 64;

	private const int Unit1Frequency = 12;

	private const int Unit2Frequency = 10;

	private const int Unit3Frequency = 8;

	private const float Unit1Amplitude = 0.075f;

	private const float Unit2Amplitude = 0.065f;

	private const float Unit3Amplitude = 0.1f;

	private const float Unit1ShiftSpeed = -0.5f;

	private const float Unit2ShiftSpeed = 0.4f;

	private const float Unit3ShiftSpeed = -0.45f;

	private const float TimeToFade = 0.25f;

	private const float TimeBetweenAddingParticles = 0.05f;

	private const float TimeToBreak = 2f;

	private const float BrokenFrequency = 2f;

	private const float BrokenAmplitude = 0.1f;

	private const float BrokenShiftXSpeed = 4f;

	private const float BrokenTimeMultiplier = 4f;

	private const float BaseMultiplier = 1f;

	private static readonly Color SoulStreamColor = Color.White * 0.25f;

	private static readonly Color SoulBreakColor1 = new Color(0.3f, 0.05f, 0.1f, 0.3f);

	private static readonly Color SoulBreakColor2 = new Color(1f, 0.9f, 0.95f, 0.5f);

	private readonly bool _isAnchoredToPlayer;

	private readonly SoulStreamSwirlParticleSystem _soulSwirlParticles;

	private readonly SpriteSheet _soulStreamSprite;

	private readonly SoulStreamUnit[] _streamUnits = new SoulStreamUnit[3];

	private bool _isFading;

	private bool _isFadingOut;

	private bool _isBreaking;

	private bool _isFirstSoulBreak;

	private bool _isRemovedAfterFading;

	private float _breakTimer;

	private float _fadeTimer;

	private float _particleAddTimer;

	private Point _anchorPointA;

	private Point _anchorPointB;

	private Color _baseSoulColor;

	private Mobile _dynamicAnchor;

	internal Mobile AlternateStreamAnchor { get; set; }

	public SoulStreamEvent(Level inLevel, Point inPosition, bool isAnchoredToPlayer)
		: base(inLevel, inPosition, -1, new ObjectTileSpecification())
	{
		_isAnchoredToPlayer = isAnchoredToPlayer;
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
		_anchorPointB = Position;
		_soulSwirlParticles = new SoulStreamSwirlParticleSystem(_level.GCM.SpTimeGateAnimation, 16);
		_particleSystems.Add(_soulSwirlParticles);
		for (int i = 0; i < 3; i++)
		{
			float baseAmplitude = 1f;
			float baseFrequency = 1f;
			float baseShiftXSpeed = 1f;
			switch (i)
			{
			case 0:
				baseAmplitude = 0.075f;
				baseFrequency = 12f;
				baseShiftXSpeed = -0.5f;
				break;
			case 1:
				baseAmplitude = 0.065f;
				baseFrequency = 10f;
				baseShiftXSpeed = 0.4f;
				break;
			case 2:
				baseAmplitude = 0.1f;
				baseFrequency = 8f;
				baseShiftXSpeed = -0.45f;
				break;
			}
			SoulStreamUnit soulStreamUnit = new SoulStreamUnit(this, _level, _soulStreamSprite, baseFrequency, baseAmplitude, baseShiftXSpeed)
			{
				DrawOrigin = new Vector2(0f, 64f)
			};
			_streamUnits[i] = soulStreamUnit;
			_appendages.Add(soulStreamUnit);
		}
	}

	internal void StartSoulBreak(bool isFirstBreak)
	{
		_isBreaking = true;
		_breakTimer = 0f;
		_isFirstSoulBreak = isFirstBreak;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			UpdateLinkMatching();
			UpdateStreamParticles(delta);
			UpdateStreamBreak(delta);
			UpdateFading(delta);
		}
		base.Update(delta);
	}

	private void UpdateLinkMatching()
	{
		if (_dynamicAnchor == null)
		{
			_dynamicAnchor = (_isAnchoredToPlayer ? _level.MainHero : AlternateStreamAnchor);
		}
		if (_dynamicAnchor == null)
		{
			return;
		}
		Point anchorPointA = _anchorPointA;
		_anchorPointA = _dynamicAnchor.Bbox.Center;
		if (anchorPointA != _anchorPointA)
		{
			int num = _anchorPointA.X - _anchorPointB.X;
			int num2 = _anchorPointA.Y - _anchorPointB.Y;
			float rotation = (float)Math.Atan2(num2, num) + (float)Math.PI;
			float num3 = (float)Math.Sqrt(num * num + num2 * num2) + 1f;
			Point position = new Point(_anchorPointA.X + 64, _anchorPointA.Y + 64);
			float scale = num3 / 128f;
			SoulStreamUnit[] streamUnits = _streamUnits;
			foreach (SoulStreamUnit soulStreamUnit in streamUnits)
			{
				soulStreamUnit.Position = position;
				soulStreamUnit.Rotation = rotation;
				soulStreamUnit.Scale = scale;
			}
		}
	}

	private void UpdateStreamParticles(float delta)
	{
		_soulSwirlParticles.BaseColor = new Vector4(0.2f, 0.15f, 0.3f, 0.25f);
		if (!_isFadingOut)
		{
			_particleAddTimer += delta;
			if (_particleAddTimer >= 0.05f)
			{
				_particleAddTimer -= 0.05f;
				_soulSwirlParticles.AddParticles(_anchorPointA.ToVector2());
				_soulSwirlParticles.AddParticles(_anchorPointB.ToVector2());
			}
		}
	}

	private void UpdateStreamBreak(float delta)
	{
		if (_isBreaking)
		{
			_breakTimer += delta;
			if (_breakTimer >= 2f)
			{
				_isBreaking = false;
				_breakTimer = 2f;
			}
			float amount = _breakTimer / 2f;
			float num = (_isFirstSoulBreak ? 1f : MathHelper.Lerp(1f, 2f, amount));
			float waveFrequencyMultiplier = MathHelper.Lerp(1f, 2f, amount) * num;
			float waveAmplitudeMultiplier = MathHelper.Lerp(1f, 0.1f, amount) * num;
			float shiftXSpeedMultiplier = MathHelper.Lerp(1f, 4f, amount) * num;
			float timeMultiplierMultiplier = MathHelper.Lerp(1f, 4f, amount) * num;
			Color start = (_isFirstSoulBreak ? SoulStreamColor : SoulBreakColor1);
			Color end = (_isFirstSoulBreak ? SoulBreakColor1 : SoulBreakColor2);
			_baseSoulColor = start.Lerp(end, amount);
			SoulStreamUnit[] streamUnits = _streamUnits;
			foreach (SoulStreamUnit soulStreamUnit in streamUnits)
			{
				soulStreamUnit.WaveFrequencyMultiplier = waveFrequencyMultiplier;
				soulStreamUnit.WaveAmplitudeMultiplier = waveAmplitudeMultiplier;
				soulStreamUnit.ShiftXSpeedMultiplier = shiftXSpeedMultiplier;
				soulStreamUnit.TimeMultiplierMultiplier = timeMultiplierMultiplier;
			}
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
}
