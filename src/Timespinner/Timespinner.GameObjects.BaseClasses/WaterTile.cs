using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.Events;

namespace Timespinner.GameObjects.BaseClasses;

public class WaterTile : Tile
{
	private const int SplashAnimationLength = 10;

	private const float SplashAnimationSpeed = 0.035f;

	private const float EmitInterval = 1f;

	private const float BaseColorTransparency = 0.65f;

	private const float GlowFrequency = (float)Math.PI;

	private const float GlowAmplitude = 0.2f;

	private readonly bool _doesGlow;

	private readonly bool _isAnimated;

	private readonly int _maxAnimationFrames = 3;

	private readonly float _animationSpeed = 0.066f;

	private bool _isOccupied;

	private int _animationIndex;

	private int _lastAnimationIndex;

	private float _animationCounter;

	private float _glowTimer;

	private float _emitTimer;

	private Vector2 _lastDepth;

	private Vector2 _lastVelocity;

	public EWaterTileType CurrentWaterType { get; protected set; }

	public Point Position16 { get; protected set; }

	public WaterTile(Point inPosition, Point inPosition16, Level inLevel, SpriteSheet inSprite, WaterFillerEvent.EWaterType inWaterType)
		: base(inPosition, inLevel, inSprite, ((inWaterType == WaterFillerEvent.EWaterType.Top) ? inLevel.NextRandomInt(1, 6) : 0) + ((inLevel.ID == 6) ? 8 : 0), ETileType.Water)
	{
		Position16 = inPosition16;
		_drawColor = Color.White * 0.65f;
		switch (inWaterType)
		{
		case WaterFillerEvent.EWaterType.Top:
			CurrentWaterType = EWaterTileType.Top;
			_isAnimated = true;
			_animationSpeed = 0.2f;
			_maxAnimationFrames = 3;
			_tileIndex = ((_level.NextRandomInt(0, 1) == 0) ? 1 : 4);
			base.IsFlippedHorizontally = _level.NextRandomInt(0, 3) == 0;
			break;
		case WaterFillerEvent.EWaterType.Bottom:
			CurrentWaterType = EWaterTileType.Bottom;
			break;
		}
		switch (_level.ID)
		{
		case 2:
		case 3:
			_tileIndex += 24;
			break;
		case 7:
			_tileIndex += 17;
			break;
		case 9:
			_tileIndex += 31;
			break;
		case 11:
			_tileIndex += 7;
			_doesGlow = true;
			break;
		}
		_drawSource = _sprite.GetFrameSource(_tileIndex);
	}

	public void MobileCollided(Mobile who, Vector2 depth)
	{
		if (CurrentWaterType == EWaterTileType.Bottom)
		{
			Point pointFromDirection = Level.GetPointFromDirection(Position16, EDirection.North);
			bool flag = false;
			if (!_level.WaterTiles.ContainsKey(pointFromDirection))
			{
				flag = true;
			}
			else
			{
				WaterTile waterTile = _level.WaterTiles[pointFromDirection];
				if (waterTile.CurrentWaterType == EWaterTileType.Bottom)
				{
					flag = true;
				}
				else if (depth.Y > 0f || depth.Y < -9f)
				{
					flag = true;
				}
			}
			if (flag)
			{
				who.IsInWater = true;
				who.IsHittingOnHeadOnCeiling = false;
			}
		}
		else
		{
			if (CurrentWaterType != 0)
			{
				return;
			}
			if (!_level.IsTimeFrozen && !who.DoesNotMakeSplashesInWater && _emitTimer <= 0f && (!who.WasGrounded || who.Position.X != who.LastPosition.X) && (!_level.SolidTiles.ContainsKey(Position16) || _level.SolidTiles[Position16].Type == ETileType.Slope))
			{
				float num = _lastDepth.X - depth.X + (_lastDepth.Y - depth.Y);
				if ((num > 5f || num < -5f || _lastVelocity.Y >= 0f != who.Velocity.Y >= 0f || (_lastVelocity.X >= 0f != who.Velocity.X >= 0f && Math.Abs(who.Velocity.X) > 10f && Math.Abs(_lastVelocity.X) > 10f)) && depth.Y <= -1f)
				{
					Point position = new Point(who.Bbox.Center.X, _bbox.Top + 10);
					if (_level.ID != 6)
					{
						_level.AddAnimation(EBattleAnimationType.WaterSplash, position, ETeamSide.Neutral);
						if (who.Velocity.Y > 250f)
						{
							Color drawColor = Color.White * 0.65f;
							BattleAnimation battleAnimation = new BattleAnimation(_level.GCM.SpEffectsMedium, new Point(position.X, position.Y - 22), _level);
							battleAnimation.AnimationSpeed = 0.035f;
							battleAnimation.AnimationStart = 44;
							battleAnimation.AnimationLength = 10;
							battleAnimation.DrawColor = drawColor;
							battleAnimation.IsFacingLeft = true;
							BattleAnimation newAnimation = battleAnimation;
							_level.AddAnimation(newAnimation);
							BattleAnimation battleAnimation2 = new BattleAnimation(_level.GCM.SpEffectsMedium, new Point(position.X, position.Y + 10), _level);
							battleAnimation2.AnimationSpeed = 0.035f;
							battleAnimation2.AnimationStart = 54;
							battleAnimation2.AnimationLength = 10;
							battleAnimation2.DrawColor = drawColor;
							battleAnimation2.IsFacingLeft = true;
							BattleAnimation newAnimation2 = battleAnimation2;
							_level.AddAnimation(newAnimation2);
							if (_level.IsWithinCameraDistance(position))
							{
								_level.PlayCue(ESFX.FoleyWaterSplashDeep, position);
							}
						}
						else if (_level.IsWithinCameraDistance(position))
						{
							_level.PlayCue(ESFX.FoleyWaterSplashShallow, position);
						}
					}
					else
					{
						_level.AddAnimation(EBattleAnimationType.AcidSplash, new Point(who.Bbox.Center.X, _bbox.Top + 9), ETeamSide.Neutral);
					}
					_emitTimer = 1f;
				}
				_isOccupied = true;
				_lastDepth = depth;
				_lastVelocity = who.Velocity;
			}
			if (_level.ID == 6 && depth.Y <= -8f)
			{
				int num2 = ((depth.Y > 0f) ? 1 : (-1));
				int num3 = (who.IsFacingLeft ? 1 : (-1));
				who.ManageDamage(1, new Vector2(0.5f * (float)num3, 90 * num2), Bbox.Center, Bbox, EDamageType.Spike, EDamageElement.Dark, doesKnockBack: false);
				who.IsInWater = true;
			}
		}
	}

	public void Update(float delta)
	{
		if (CurrentWaterType == EWaterTileType.Top)
		{
			if (_emitTimer > 0f)
			{
				_emitTimer -= delta;
				if (_emitTimer < 0f)
				{
					_emitTimer = 0f;
				}
			}
			if (!_isOccupied)
			{
				_lastDepth = Vector2.Zero;
			}
			_isOccupied = false;
		}
		if (_doesGlow)
		{
			_glowTimer += delta;
			if (_glowTimer > 19.73921f)
			{
				_glowTimer -= 19.73921f;
			}
			float num = (float)(Math.Sin(_glowTimer * (float)Math.PI) * 0.5 + 1.0);
			_drawColor = Color.White * (0.65f + num * 0.2f);
		}
		if (!_isAnimated)
		{
			return;
		}
		_lastAnimationIndex = _animationIndex;
		_animationCounter += delta;
		if (_animationCounter > _animationSpeed)
		{
			_animationCounter = 0f;
			_animationIndex++;
			if (_animationIndex >= _maxAnimationFrames)
			{
				_animationIndex = 0;
			}
		}
		if (_animationIndex != _lastAnimationIndex)
		{
			_drawSource = _sprite.GetFrameSource(_tileIndex + _animationIndex);
		}
	}
}
