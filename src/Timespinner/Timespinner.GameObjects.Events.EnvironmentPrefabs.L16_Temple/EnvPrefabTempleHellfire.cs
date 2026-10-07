using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L16_Temple;

internal class EnvPrefabTempleHellfire : EnvironmentPrefabBase
{
	private const int InitialHellLightWidth = 1;

	private const int InitialHellLightHeight = 1;

	private const int HellLightHeight = 288;

	private const int InitialHellfireWidth = 1;

	private const int FinalHellfireWidth = 256;

	private const int HellfireHeight = 300;

	private const int HellfireOffsetY = 30;

	private const float HellfireOverlapPercentageX = 0.1f;

	private const float TimeForOpeningPhase1 = 0f;

	private const float TimeForOpeningPhase2 = 0.25f;

	private const float TimeForOpeningPhase3 = 0.75f;

	private const float TimeForOpeningPhase4 = 0.5f;

	private const float TimeForOpeningPhase5 = 0f;

	private const float TimeBeforeOpeningPhase3 = 0.25f;

	private const float TimeBeforeOpeningPhase4 = 1f;

	private const float TimeBeforeOpeningPhase5 = 1.5f;

	internal const float TimeForEntireOpeningSequence = 1.5f;

	private const float TimeForClosingPhase1 = 1f;

	private static readonly Color WhiteOutColor = new Color(92, 128, 64);

	private static readonly Color HellLightStartColor = Color.White;

	private static readonly Color HellLightMidColor = new Color(64, 48, 80, 160);

	private static readonly Color HellLightFinalColor = new Color(48, 64, 32, 128);

	private readonly bool _isOpening;

	private readonly int _roomCenterX;

	private readonly Background _levelFadeForeground;

	private readonly HellLightAppendage _leftHellLight;

	private readonly HellLightAppendage _rightHellLight;

	private readonly HellfireAppendage _leftHellfire;

	private readonly HellfireAppendage _rightHellfire;

	private float _sequenceTimer;

	private float _lastSequenceTimer;

	public EnvPrefabTempleHellfire(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType, bool isOpening)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		_isOpening = isOpening;
		base.DrawPlane = EDrawPlane.Front;
		_sprite = _level.GCM.BgEndingBackdrops1;
		_doesDrawBaseSprite = false;
		_roomCenterX = _level.RoomSize.X / 2 - 2;
		Point bboxDimensions = new Point(1, 1);
		_leftHellLight = new HellLightAppendage(this, bboxDimensions, Point.Zero, _level, _sprite);
		_rightHellLight = new HellLightAppendage(this, bboxDimensions, Point.Zero, _level, _sprite);
		SpriteSheet spDifferenceCloud = _level.GCM.SpDifferenceCloud;
		Point bboxDimensions2 = (_isOpening ? new Point(1, 300) : new Point(256, 300));
		_leftHellfire = new HellfireAppendage(this, bboxDimensions2, Point.Zero, _level, spDifferenceCloud)
		{
			SpriteEffects = SpriteEffects.FlipVertically
		};
		_rightHellfire = new HellfireAppendage(this, bboxDimensions2, Point.Zero, _level, spDifferenceCloud);
		if (_isOpening)
		{
			_appendages.Add(_leftHellLight);
			_appendages.Add(_rightHellLight);
		}
		else
		{
			_appendages.Add(_leftHellfire);
			_appendages.Add(_rightHellfire);
		}
		_levelFadeForeground = new Background(new BackgroundSpecification
		{
			IsForeground = true,
			DoesTileEast = true,
			DoesTileWest = true,
			DoesTileNorth = true,
			DoesTileSouth = true,
			DrawColor = Color.White,
			TextureType = EBackgroundTextureType.EndingBackdrops1,
			FrameIndex = 13
		}, _level)
		{
			DrawColor = Color.Transparent
		};
		if (_isOpening)
		{
			_level.Foregrounds.Add(_levelFadeForeground);
		}
	}

	public override void Update(float delta)
	{
		if (_isOpening)
		{
			if (!(_sequenceTimer < 0f))
			{
				if (_sequenceTimer < 0.25f)
				{
					float num = (_sequenceTimer - 0f) / 0.25f;
					int y = (int)Math.Ceiling(MathEx.CosInterpolate(1f, 288f, num));
					Point newDimensions = new Point(1, y);
					_leftHellLight.ChangeBboxDimensions(newDimensions, Point.Zero);
					_rightHellLight.ChangeBboxDimensions(newDimensions, Point.Zero);
					int y2 = (int)Math.Ceiling(MathEx.CosInterpolate(Position.Y, 288f, num));
					Point position = new Point(_roomCenterX, y2);
					_leftHellLight.Position = position;
					_rightHellLight.Position = position;
					Color drawColor = HellLightStartColor.Lerp(HellLightMidColor, num);
					_leftHellLight.DrawColor = drawColor;
					_rightHellLight.DrawColor = drawColor;
				}
				else if (_sequenceTimer < 1f)
				{
					if (_lastSequenceTimer < 0.25f)
					{
						_leftHellfire.SetColorMultiplier(1f);
						_rightHellfire.SetColorMultiplier(1f);
						_appendages.Add(_leftHellfire);
						_appendages.Add(_rightHellfire);
						Point newDimensions2 = new Point(1, 288);
						_leftHellLight.ChangeBboxDimensions(newDimensions2, Point.Zero);
						_rightHellLight.ChangeBboxDimensions(newDimensions2, Point.Zero);
						Point position2 = new Point(_roomCenterX, 288);
						_leftHellLight.Position = position2;
						_rightHellLight.Position = position2;
					}
					float num2 = (_sequenceTimer - 0.25f) / 0.75f;
					int num3 = (int)Math.Ceiling(MathEx.CosInterpolate(1f, 256f, num2));
					int num4 = num3 / 2;
					int num5 = (int)Math.Ceiling((float)num3 * 0.1f);
					Point newDimensions3 = new Point(num3, 300);
					_leftHellfire.ChangeBboxDimensions(newDimensions3, Point.Zero);
					_rightHellfire.ChangeBboxDimensions(newDimensions3, Point.Zero);
					_leftHellfire.Position = new Point(_roomCenterX - num4 + num5, 270);
					_rightHellfire.Position = new Point(_roomCenterX + num4 - num5, 270);
					_leftHellLight.Position = new Point(_leftHellfire.Bbox.Left, 288);
					_rightHellLight.Position = new Point(_rightHellfire.Bbox.Right, 288);
					Color drawColor2 = HellLightMidColor.Lerp(HellLightFinalColor, num2);
					_leftHellLight.DrawColor = drawColor2;
					_rightHellLight.DrawColor = drawColor2;
				}
				else if (_sequenceTimer < 1.5f)
				{
					float amount = (_sequenceTimer - 1f) / 0.5f;
					_levelFadeForeground.DrawColor = Color.Transparent.CosInterpolate(WhiteOutColor, amount);
				}
				else if (_sequenceTimer < 1.5f)
				{
					float amount2 = (_sequenceTimer - 1.5f) / 0f;
					_levelFadeForeground.DrawColor = WhiteOutColor.SineInterpolate(Color.White, amount2);
				}
			}
		}
		else if (_sequenceTimer < 1f)
		{
			if (_lastSequenceTimer <= 0f)
			{
				int num6 = (int)Math.Ceiling(25.600000381469727);
				Point newDimensions4 = new Point(256, 300);
				_leftHellfire.ChangeBboxDimensions(newDimensions4, Point.Zero);
				_rightHellfire.ChangeBboxDimensions(newDimensions4, Point.Zero);
				_leftHellfire.Position = new Point(_roomCenterX - 128 + num6, 270);
				_rightHellfire.Position = new Point(_roomCenterX + 128 - num6, 270);
			}
			float num7 = _sequenceTimer / 1f;
			_leftHellfire.SetColorMultiplier(1f - num7);
			_rightHellfire.SetColorMultiplier(1f - num7);
		}
		else
		{
			SilentKill();
		}
		_lastSequenceTimer = _sequenceTimer;
		_sequenceTimer += delta;
		base.Update(delta);
	}

	internal void Reset()
	{
		_sequenceTimer = 0f;
		_lastSequenceTimer = -1f;
		_levelFadeForeground.DrawColor = Color.Transparent;
		Point newDimensions = new Point(1, 300);
		_leftHellfire.ChangeBboxDimensions(newDimensions, Point.Zero);
		_rightHellfire.ChangeBboxDimensions(newDimensions, Point.Zero);
		Point position = new Point(_roomCenterX, 270);
		_leftHellfire.Position = position;
		_rightHellfire.Position = position;
	}
}
