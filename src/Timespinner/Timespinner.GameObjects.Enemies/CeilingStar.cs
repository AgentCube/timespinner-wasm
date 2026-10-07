using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class CeilingStar : Monster
{
	private const int DefaultFallLength = 64;

	private const float FallLengthRatio = 0.75f;

	private const float TimeToFall = 0.3f;

	private const float TimeToRetract = 0.5f;

	private static readonly Point ClosedBboxOffset = new Point(2, 1);

	private static readonly Point OpenBboxOffset = new Point(6, 5);

	private readonly Appendage _rootAppendage;

	private int _fallLength;

	private float _fallRetractTimer = 0.5f;

	private Point _rootBaseLocation;

	private Point _floorTilePosition;

	private Point _targetFallPosition;

	private CeilingStarDamageArea _damageArea;

	private SFXCueInstance _electricLoopCue;

	public CeilingStar(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_currentAI = EAIStrategy.None;
		_paceLength = 5f;
		_agility = 0.15f;
		_bboxOffset = ClosedBboxOffset;
		Bbox = new Rectangle(_position.X, _position.Y, 16, 18);
		_doesAggroOnTakingDamage = false;
		base.AggroBboxDimensions = new Point(150, 200);
		base.DeaggroBboxDimensions = new Point(250, 350);
		ChangeAnimation(2, 1, 1f, EAnimationType.None);
		_isAffectedByGravity = false;
		_doesBounceOffWall = true;
		base.CannotBeGrabbed = true;
		_rootBaseLocation = new Point(Position.X, Position.Y - 12);
		_rootAppendage = new Appendage(this, new Rectangle(_rootBaseLocation.X, _rootBaseLocation.Y, 10, 5), new Point(3, 0), _level, _sprite)
		{
			OscillAmplitude = 2f,
			OscillFrequency = 2f,
			OscillIncrement = 0.25f,
			OscillDelta = _random.Next(6),
			OscillSpeed = 2f,
			SpriteFrameOffset = base.SpriteFrameOffset
		};
		_appendages.Add(_rootAppendage);
		Point offset = new Point(0, 4);
		_rootAppendage.ChangeAnimation(1, 1, 1f, EAnimationType.None);
		_rootAppendage.AddLinks(16, EAppendageFollowType.TrigoInterpolate, new Rectangle(_rootBaseLocation.X, _rootBaseLocation.Y, 6, 6), new Point(1, 1), base.SpriteFrameOffset, offset);
		Position = _rootBaseLocation;
		SnapBboxToPosition();
	}

	public override void InitializeMob()
	{
		Tile tile = _level.FindFirstSolidTileInDirection(Position, EDirection.South);
		_floorTilePosition = ((tile != null) ? new Point(tile.Bbox.Center.X, tile.Bbox.Top) : new Point(Position.X, Position.Y + 64));
		_targetFallPosition = new Point(_rootBaseLocation.X, _rootBaseLocation.Y + (int)((float)(_floorTilePosition.Y - _rootBaseLocation.Y - Bbox.Height) * 0.75f));
		_fallLength = _targetFallPosition.Y - _rootBaseLocation.Y;
		base.InitializeMob();
		UpdateAggroFall(0f);
		Update(0f);
	}

	public override void Update(float delta)
	{
		if (!_isFrozen)
		{
			UpdateAggroFall(delta);
		}
		int num = base.AnimationStart + base.AnimationIndex;
		if (num == 2 || num == 3)
		{
			_bboxOffset = ClosedBboxOffset;
		}
		else
		{
			_bboxOffset = OpenBboxOffset;
		}
		base.Update(delta);
	}

	private void UpdateAggroFall(float delta)
	{
		if (_isAggroed)
		{
			if (!_wasAggroed)
			{
				ChangeAnimation(new AnimationSpec[2]
				{
					new AnimationSpec
					{
						Start = 2,
						Length = 4,
						Speed = 0.1f
					},
					new AnimationSpec
					{
						Start = 5,
						Length = 2,
						Speed = 0.04f,
						Type = EAnimationType.Cycle
					}
				});
				_fallRetractTimer = 0f;
				PlayCue(ESFX.EnemyElectricDown, Position);
			}
			if (_fallRetractTimer < 0.3f)
			{
				_fallRetractTimer += delta;
				if (_fallRetractTimer > 0.3f)
				{
					_fallRetractTimer = 0.3f;
				}
				float num = _fallRetractTimer / 0.3f;
				_floatPosition.Y = (float)_rootBaseLocation.Y + (float)(Math.Pow(num, 2.0) * (double)_fallLength);
			}
			else if (_damageArea == null)
			{
				if (_electricLoopCue == null)
				{
					_electricLoopCue = PlayCue(ESFX.EnemyElectricStorm, isLooped: true);
				}
				else
				{
					_electricLoopCue.Resume();
				}
				_damageArea = new CeilingStarDamageArea(_level, Bbox.Center, base.DefaultTeam, null, _sprite, base.Damage);
				_level.AddProjectile(_damageArea);
			}
			else
			{
				_damageArea.RefreshLife();
			}
			return;
		}
		if (_wasAggroed)
		{
			PlayCue(ESFX.EnemyElectricUp, Position);
			if (_electricLoopCue != null)
			{
				_electricLoopCue.Pause();
			}
			ChangeAnimation(new AnimationSpec[1]
			{
				new AnimationSpec
				{
					Start = 2,
					Length = 4,
					IsInReverse = true,
					Speed = 0.1f
				}
			});
			_fallRetractTimer = 0f;
			_wasAggroed = false;
		}
		if (_fallRetractTimer < 0.5f)
		{
			_fallRetractTimer += delta;
			if (_fallRetractTimer > 0.5f)
			{
				_fallRetractTimer = 0.5f;
			}
			float num2 = 1f - _fallRetractTimer / 0.5f;
			_floatPosition.Y = (float)_rootBaseLocation.Y + (float)(Math.Pow(num2, 2.0) * (double)_fallLength);
			if (_damageArea != null)
			{
				_damageArea.KillParticles();
				_damageArea = null;
			}
		}
	}

	public override void SnapBboxToPosition()
	{
		_bbox.Location = new Point(_position.X - _bbox.Width / 2, _position.Y);
		SnapAggroBboxToBbox();
	}

	public override bool ManageDamage(int damage, Vector2 velocity, Point where, Rectangle sourceRectangle, EDamageType type, EDamageElement element, bool doesKnockBack)
	{
		bool result = false;
		if (base.OuterBbox.Intersects(sourceRectangle))
		{
			if (!Bbox.Intersects(sourceRectangle))
			{
				List<Rectangle> list = FindIntersectingBoundingBoxes(sourceRectangle, 1);
				if (list != null && list.Count > 0)
				{
					Rectangle rectangle = list.First();
					if (rectangle != _rootAppendage.Bbox)
					{
						result = base.ManageDamage(damage * 2, velocity, where, sourceRectangle, type, element, doesKnockBack);
					}
				}
			}
			else
			{
				result = base.ManageDamage(damage, velocity, where, sourceRectangle, type, element, doesKnockBack);
			}
		}
		return result;
	}
}
