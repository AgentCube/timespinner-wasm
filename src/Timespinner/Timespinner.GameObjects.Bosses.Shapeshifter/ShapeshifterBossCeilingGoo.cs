using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Shapeshifter;

internal class ShapeshifterBossCeilingGoo : GameEvent
{
	private enum EShapeshifterCeilingGooState
	{
		Invisible,
		GoingToCeiling,
		StretchingOnCeiling,
		StretchingOnGround
	}

	private const int WidthHeight = 16;

	private const int HalfWidth = 8;

	private const int CeilingY = 40;

	private const int DistanceToMoveOnCeiling = 112;

	private const int DistanceToStretch = 112;

	private const float TimeToStretchOnCeiling = 0.66f;

	private const float TimeToThrowToCeiling = 0.66f;

	private bool _isGooingToTheLeft;

	private EShapeshifterCeilingGooState _gooState;

	private int _distanceToCeiling;

	private int _stretchWidth;

	private float _gooTimer;

	private Point _throwStartPosition;

	private SFXCueInstance _gooCueLoopInstance;

	public ShapeshifterBossCeilingGoo(Level inLevel, Point inPosition, SpriteSheet sprite, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, -1, objectSpec)
	{
		_sprite = sprite;
		Bbox = new Rectangle(0, 0, 16, 16);
		ChangeAnimation(68, 3, 0.1f, EAnimationType.Cycle);
		_gooState = EShapeshifterCeilingGooState.Invisible;
		_isSolid = false;
		_isFlying = true;
		_isAffectedByGravity = false;
		base.IsAffectedByTime = true;
		base.CanBeTriggered = false;
		base.DoesCollideWithTiles = false;
		base.DoesCollideWithProjectiles = false;
		_defaultTeam = ETeamSide.Enemies;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			bool flag = true;
			switch (_gooState)
			{
			case EShapeshifterCeilingGooState.GoingToCeiling:
				if (_gooTimer >= 0.66f)
				{
					_gooState = EShapeshifterCeilingGooState.StretchingOnCeiling;
					_gooTimer = 0f;
					_stretchWidth = 16;
					Position = new Point(_throwStartPosition.X, 40);
					if (_gooCueLoopInstance == null)
					{
						_gooCueLoopInstance = CreateCue(ESFX.BossShapeshifterGoopLoop, Position, isLooped: true);
						if (_gooCueLoopInstance != null)
						{
							_gooCueLoopInstance.Anchor = this;
							_gooCueLoopInstance.UpdateType = SFXCueInstance.ECueInstanceUpdateType.Anchor;
							_gooCueLoopInstance.Play();
						}
					}
					else
					{
						_gooCueLoopInstance.Resume();
					}
				}
				else
				{
					float num3 = _gooTimer / 0.66f;
					int num4 = (int)((1.0 - Math.Sin(num3 * ((float)Math.PI / 2f))) * (double)_distanceToCeiling);
					Position = new Point(_throwStartPosition.X, num4 + 40);
				}
				break;
			case EShapeshifterCeilingGooState.StretchingOnCeiling:
				if (_gooTimer >= 0.66f)
				{
					_gooTimer = 0f;
					_gooState = EShapeshifterCeilingGooState.Invisible;
					if (_gooCueLoopInstance != null)
					{
						_gooCueLoopInstance.Pause(0.1f);
					}
				}
				else
				{
					float num = _gooTimer / 0.66f;
					num = (float)(1.0 - Math.Cos(num * ((float)Math.PI / 2f)));
					_stretchWidth = (int)(num * 112f) + 16;
					int num2 = (int)(num * 112f);
					Position = new Point(_throwStartPosition.X + (_isGooingToTheLeft ? (-num2) : num2), 40);
				}
				break;
			default:
				base.DoesDrawBaseSprite = false;
				flag = false;
				break;
			}
			if (flag)
			{
				_gooTimer += delta;
			}
		}
		base.Update(delta);
	}

	protected override void DrawBaseSprite(SpriteBatch spriteBatch, SpriteSheet sprite, Vector2 drawPos, Rectangle source, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float depth)
	{
		if (_gooState == EShapeshifterCeilingGooState.StretchingOnCeiling)
		{
			spriteBatch.Draw(destinationRectangle: new Rectangle((int)(drawPos.X - (float)(_stretchWidth / 2) + 8f), (int)drawPos.Y, _stretchWidth, 16), texture: sprite.Texture, sourceRectangle: source, color: color, rotation: rotation, origin: origin, effects: effects, layerDepth: depth);
			return;
		}
		base.DrawBaseSprite(spriteBatch, sprite, drawPos, source, color, rotation, origin, scale, effects, depth);
	}

	internal void ThrowToCeiling(Point position, bool isGooingToTheLeft)
	{
		Position = position;
		_isGooingToTheLeft = isGooingToTheLeft;
		_throwStartPosition = position;
		_distanceToCeiling = position.Y - 40;
		_gooTimer = 0f;
		_gooState = EShapeshifterCeilingGooState.GoingToCeiling;
		base.DoesDrawBaseSprite = true;
		Update(0f);
	}
}
