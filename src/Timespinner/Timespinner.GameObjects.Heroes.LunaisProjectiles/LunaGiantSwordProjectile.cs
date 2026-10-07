using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Lunais.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Heroes.LunaisProjectiles;

internal sealed class LunaGiantSwordProjectile : LunaisBaseOrbDamageArea
{
	private const int SwordRotationRadius = 33;

	private const float RotationOffsetMultiplierY = 1.2f;

	public const float MaxLife = 0.6f;

	private const int FramesToFadeIn = 7;

	private const float TimeBetweenFrames = 0.033f;

	private const float BaseAlpha = 0.5f;

	private const int ArcWidth = 136;

	private const int ArcHeight = 124;

	private readonly int _directionRotationMultiplier;

	private readonly Point _arcOffset = new Point(0, 0);

	private static readonly List<float> SwordRotations = new List<float>
	{
		4.08407f,
		4.3982296f,
		4.790929f,
		5.4977875f,
		6.047566f,
		(float)Math.PI * 2f
	};

	private readonly List<Point> _swordRotationOffsets = new List<Point>();

	private readonly List<GiantSwordFrame> _swordFrames = new List<GiantSwordFrame>();

	private int _swordFrameIndex;

	private float _swordFrameTimer;

	private Point _damageBboxOffset;

	private Point _arcDirectionalOffset = new Point(-60, -20);

	private Rectangle _damageBbox;

	public override Rectangle DamageBbox => _damageBbox;

	public Point DamageBboxOffset
	{
		get
		{
			if (!IsFacingLeft)
			{
				return _damageBboxOffset;
			}
			return new Point(-_damageBboxOffset.X, _damageBboxOffset.Y);
		}
		set
		{
			_damageBboxOffset = value;
		}
	}

	public Point ArcOffset
	{
		get
		{
			if (!IsFacingLeft)
			{
				return _arcDirectionalOffset;
			}
			return new Point(-_arcDirectionalOffset.X, _arcDirectionalOffset.Y);
		}
	}

	public override Vector2 BrushOrigin => Vector2.Zero;

	public LunaGiantSwordProjectile(Level inLevel, Point inPosition, bool isFacingLeft, ETeamSide inSide, Mobile inAnchor, int baseOrbDamage, LunaisOrbAbility parentOrb)
		: base(inLevel, inPosition, inSide, -1, inAnchor, parentOrb)
	{
		IsFacingLeft = isFacingLeft;
		_directionRotationMultiplier = ((!IsFacingLeft) ? 1 : (-1));
		_sprite = _level.GCM.SpMiscProjectiles;
		_damageDimensions = new Point(112, 36);
		_bboxOffset = new Point(60, 0);
		_bbox = new Rectangle(inPosition.X, inPosition.Y, 8, 36);
		base.AnchorOffset = new Point(-54, -14);
		SnapBboxToPosition();
		ChangeAnimation(new AnimationSpec[1]
		{
			new AnimationSpec
			{
				Start = 0,
				Length = 4,
				IsInReverse = true,
				Speed = 0.05f,
				Type = EAnimationType.Once
			}
		});
		DrawOrigin = new Vector2(IsFacingLeft ? 118f : 10f, 23.5f);
		_doesRotateBasedOnVelocity = false;
		_power = baseOrbDamage;
		_force = 4;
		_life = 0.6f;
		base.DamageTimeoutTime = 1f;
		_canDamageThings = false;
		_damageElement = EDamageElement.Sharp;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_isFlying = true;
		_doesCollideWithSlopes = false;
		_doesDieOnTiles = false;
		_doesDrawBrushTrail = true;
		_doesOverrideUpdateTrail = true;
		_isTrailLengthAffectedByTime = false;
		_brushTrailSize = 48;
		_trailFadeRate = 1f;
		_trailInterpolationAmount = 20;
		_trailShrinkRate = 0.00125f;
		_trailLength = 150;
		_trailColor = Color.White * 0.95f;
		PopulateRotationOffsets();
	}

	private void PopulateRotationOffsets()
	{
		foreach (float swordRotation in SwordRotations)
		{
			float num = swordRotation;
			_swordRotationOffsets.Add(new Point((int)((0.0 - Math.Cos(num)) * 33.0), (int)(Math.Sin(num) * 33.0 * 1.2000000476837158)));
		}
	}

	public override void Update(float delta)
	{
		if (base.IsFrozen)
		{
			return;
		}
		bool flag = false;
		float startAngle = SwordRotations[0] - 0.22f;
		float endAngle = 0f;
		_swordFrameTimer -= delta;
		if (_swordFrameTimer <= 0f)
		{
			_swordFrameTimer += 0.033f;
			_swordFrames.Clear();
			if (_swordFrameIndex < 7)
			{
				_swordFrames.Add(new GiantSwordFrame
				{
					Alpha = 1f,
					Offset = _swordRotationOffsets[0],
					Rotation = SwordRotations[0]
				});
			}
			else
			{
				int num = _swordFrameIndex - 7;
				_canDamageThings = true;
				switch (num)
				{
				case 0:
					_swordFrames.Add(new GiantSwordFrame
					{
						Alpha = 1f,
						Offset = _swordRotationOffsets[0],
						Rotation = SwordRotations[0]
					});
					_swordFrames.Add(new GiantSwordFrame
					{
						Alpha = 0.5f,
						Offset = _swordRotationOffsets[1],
						Rotation = SwordRotations[1]
					});
					_swordFrames.Add(new GiantSwordFrame
					{
						Alpha = 0.5f,
						Offset = _swordRotationOffsets[2],
						Rotation = SwordRotations[2]
					});
					_damageBbox = new Rectangle(0, 0, 120, 142);
					DamageBboxOffset = new Point(-28, -94);
					flag = true;
					endAngle = SwordRotations[2] - 0.11f;
					_brushTrailSize = 16;
					_trailShrinkRate = 0.00035f;
					break;
				case 1:
					_swordFrames.Add(new GiantSwordFrame
					{
						Alpha = 1f / 3f,
						Offset = _swordRotationOffsets[0],
						Rotation = SwordRotations[0]
					});
					_swordFrames.Add(new GiantSwordFrame
					{
						Alpha = 0.5f,
						Offset = _swordRotationOffsets[1],
						Rotation = SwordRotations[1]
					});
					_swordFrames.Add(new GiantSwordFrame
					{
						Alpha = 0.5f,
						Offset = _swordRotationOffsets[2],
						Rotation = SwordRotations[2]
					});
					_swordFrames.Add(new GiantSwordFrame
					{
						Alpha = 0.5f,
						Offset = _swordRotationOffsets[3],
						Rotation = SwordRotations[3]
					});
					_damageBbox = new Rectangle(0, 0, 172, 123);
					DamageBboxOffset = new Point(21, -99);
					flag = true;
					endAngle = SwordRotations[3] - 0.11f;
					_brushTrailSize = 32;
					_trailShrinkRate = 0.0008f;
					break;
				case 2:
					_swordFrames.Add(new GiantSwordFrame
					{
						Alpha = 0.125f,
						Offset = _swordRotationOffsets[0],
						Rotation = SwordRotations[0]
					});
					_swordFrames.Add(new GiantSwordFrame
					{
						Alpha = 1f / 6f,
						Offset = _swordRotationOffsets[1],
						Rotation = SwordRotations[1]
					});
					_swordFrames.Add(new GiantSwordFrame
					{
						Alpha = 0.25f,
						Offset = _swordRotationOffsets[2],
						Rotation = SwordRotations[2]
					});
					_swordFrames.Add(new GiantSwordFrame
					{
						Alpha = 1f / 3f,
						Offset = _swordRotationOffsets[3],
						Rotation = SwordRotations[3]
					});
					_swordFrames.Add(new GiantSwordFrame
					{
						Alpha = 0.5f,
						Offset = _swordRotationOffsets[4],
						Rotation = SwordRotations[4]
					});
					_swordFrames.Add(new GiantSwordFrame
					{
						Alpha = 0.5f,
						Offset = _swordRotationOffsets[5],
						Rotation = SwordRotations[5]
					});
					_damageBbox = new Rectangle(0, 0, 133, 95);
					DamageBboxOffset = new Point(86, -40);
					flag = true;
					endAngle = SwordRotations[5];
					_brushTrailSize = 48;
					_trailShrinkRate = 0.0012f;
					break;
				default:
				{
					float num2 = (float)(num + 2) - 3f;
					_swordFrames.Add(new GiantSwordFrame
					{
						Alpha = 0.5f / (4f + num2),
						Offset = _swordRotationOffsets[0],
						Rotation = SwordRotations[0]
					});
					_swordFrames.Add(new GiantSwordFrame
					{
						Alpha = 0.5f / (3f + num2),
						Offset = _swordRotationOffsets[1],
						Rotation = SwordRotations[1]
					});
					_swordFrames.Add(new GiantSwordFrame
					{
						Alpha = 0.5f / (2f + num2),
						Offset = _swordRotationOffsets[2],
						Rotation = SwordRotations[2]
					});
					_swordFrames.Add(new GiantSwordFrame
					{
						Alpha = 0.5f / (1.5f + num2),
						Offset = _swordRotationOffsets[3],
						Rotation = SwordRotations[3]
					});
					_swordFrames.Add(new GiantSwordFrame
					{
						Alpha = 0.5f / (1f + num2),
						Offset = _swordRotationOffsets[4],
						Rotation = SwordRotations[4]
					});
					_swordFrames.Add(new GiantSwordFrame
					{
						Alpha = 1f,
						Offset = _swordRotationOffsets[5],
						Rotation = SwordRotations[5]
					});
					_trailColor.A = (byte)((float)(int)_trailColor.A * 0.5f);
					for (int num3 = _drawHistories.Count - 1; num3 >= 0; num3--)
					{
						DrawHistory drawHistory = _drawHistories[num3];
						drawHistory.DrawColor = _trailColor;
					}
					_damageBbox = new Rectangle(0, 0, 128, 35);
					DamageBboxOffset = new Point(88, -8);
					break;
				}
				}
			}
			_swordFrameIndex++;
		}
		if (flag)
		{
			AddCircularArc(startAngle, endAngle);
		}
		base.Update(delta);
		_damageBbox.Location = _anchorObject.AnchorPosition.Add(DamageBboxOffset).Add(new Point(-_damageBbox.Width / 2, -_damageBbox.Height / 2));
	}

	private void AddCircularArc(float startAngle, float endAngle)
	{
		_drawHistories.Clear();
		float num = startAngle - endAngle;
		Point a = AnchorPosition.Add(_arcOffset).Add(ArcOffset);
		float num2 = startAngle + (float)Math.PI / 2f;
		int num3 = (IsFacingLeft ? 1 : (-1));
		for (int i = 0; i <= _trailLength; i++)
		{
			float num4 = (float)i / (float)_trailLength;
			float num5 = num4 * num;
			float num6 = num2 + num5;
			Point b = new Point((int)(Math.Cos(num6) * 136.0 * (double)num3), (int)(Math.Sin(num6) * 124.0));
			_drawHistories.Add(new DrawHistory
			{
				DrawPosition = a.Add(b),
				DrawColor = _trailColor
			});
		}
		while (_drawHistories.Count > _trailLength)
		{
			_drawHistories.RemoveAt(0);
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		DrawTrail(spriteBatch);
		Point drawPosition = _drawPosition;
		foreach (GiantSwordFrame swordFrame in _swordFrames)
		{
			if (swordFrame.Alpha > 0.1f)
			{
				base.Rotation = swordFrame.Rotation * (float)_directionRotationMultiplier;
				base.DrawColor = Color.White * swordFrame.Alpha;
				Point b = new Point(swordFrame.Offset.X * (IsFacingLeft ? 1 : (-1)), swordFrame.Offset.Y);
				_drawPosition = drawPosition.Add(b);
				base.Draw(spriteBatch);
			}
		}
		_drawPosition = drawPosition;
	}

	protected override void DoDamageAnimation(Alive target, Point intersectionCenter)
	{
		_level.AddAnimation(EBattleAnimationType.MediumHitYellow, intersectionCenter, base.DefaultTeam, isFacingRight: true, doesPlaySFX: false);
		_level.PlayCue(ESFX.LunaisOrbImpactSharp, intersectionCenter);
	}

	protected override Point FindDamagePoint(Alive target, Rectangle collisionRectangle)
	{
		return target.Bbox.Center;
	}
}
