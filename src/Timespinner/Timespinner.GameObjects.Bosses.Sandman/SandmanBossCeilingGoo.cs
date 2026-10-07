using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Sandman;

internal class SandmanBossCeilingGoo : GameEvent
{
	private enum ESandmanCeilingGooState
	{
		Invisible,
		GoingToCeiling
	}

	private const int WidthHeight = 16;

	private const int CeilingY = 24;

	private const float TimeToThrowToCeiling = 0.66f;

	private readonly Vector2 _sandTextureRatio = new Vector2(2f, 2f);

	private readonly SandDrawHelper _sandDrawHelper;

	private bool _isDrawingSand;

	private ESandmanCeilingGooState _gooState;

	private int _distanceToCeiling;

	private float _gooTimer;

	private Point _throwStartPosition;

	public SandmanBossCeilingGoo(Level inLevel, Point inPosition, SpriteSheet sprite, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, -1, objectSpec)
	{
		_sprite = sprite;
		Bbox = new Rectangle(0, 0, 16, 16);
		ChangeAnimation(34, 3, 0.1f, EAnimationType.Cycle);
		_gooState = ESandmanCeilingGooState.Invisible;
		_isSolid = false;
		_isFlying = true;
		_isAffectedByGravity = false;
		base.IsAffectedByTime = true;
		base.CanBeTriggered = false;
		base.DoesCollideWithTiles = false;
		base.DoesCollideWithProjectiles = false;
		_defaultTeam = ETeamSide.Enemies;
		_doesDrawTrail = true;
		_trailLength = 6;
		_trailFadeRate = 2f;
		_sandDrawHelper = new SandDrawHelper(this);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			_sandDrawHelper.Update(delta);
			bool flag = true;
			ESandmanCeilingGooState gooState = _gooState;
			if (gooState == ESandmanCeilingGooState.GoingToCeiling)
			{
				if (_gooTimer >= 0.66f)
				{
					_gooState = ESandmanCeilingGooState.Invisible;
					_gooTimer = 0f;
					Position = new Point(_throwStartPosition.X, 24);
					base.DoesDrawBaseSprite = false;
					_doesDrawTrail = false;
				}
				else
				{
					float num = _gooTimer / 0.66f;
					int num2 = (int)((1.0 - Math.Sin(num * ((float)Math.PI / 2f))) * (double)_distanceToCeiling);
					Position = new Point(_throwStartPosition.X, num2 + 24);
				}
			}
			else
			{
				base.DoesDrawBaseSprite = false;
				flag = false;
			}
			if (flag)
			{
				_gooTimer += delta;
			}
		}
		base.Update(delta);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (!_isDrawingSand)
		{
			_isDrawingSand = true;
			_sandDrawHelper.Draw(spriteBatch, this, _sandTextureRatio);
			_isDrawingSand = false;
		}
		else
		{
			base.Draw(spriteBatch);
		}
	}

	internal void ThrowToCeiling(Point position)
	{
		Position = position;
		SnapBboxToPosition();
		_throwStartPosition = position;
		_distanceToCeiling = position.Y - 24;
		_gooTimer = 0f;
		_gooState = ESandmanCeilingGooState.GoingToCeiling;
		base.DoesDrawBaseSprite = true;
		_doesDrawTrail = true;
		Update(0f);
	}
}
