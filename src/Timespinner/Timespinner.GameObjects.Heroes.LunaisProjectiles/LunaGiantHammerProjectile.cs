using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;
using Timespinner.GameObjects.Heroes.Spells;

namespace Timespinner.GameObjects.Heroes.LunaisProjectiles;

internal sealed class LunaGiantHammerProjectile : LunaisBaseOrbDamageArea
{
	private const int HammerRotationHeight = 40;

	private const int HammerRotationWidth = 30;

	public const float MaxLife = 0.6f;

	private const int FramesToFadeIn = 7;

	private const float TimeBetweenFrames = 0.033f;

	private const float BaseAlpha = 0.5f;

	private readonly int _directionRotationMultiplier;

	private static readonly List<float> HammerRotations = new List<float>
	{
		4.08407f,
		4.3982296f,
		4.790929f,
		5.4977875f,
		6.047566f,
		(float)Math.PI * 2f
	};

	private readonly List<Point> _swordRotationOffsets = new List<Point>();

	private readonly List<GiantHammerFrame> _swordFrames = new List<GiantHammerFrame>();

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

	public LunaGiantHammerProjectile(Level inLevel, Point inPosition, bool isFacingLeft, ETeamSide inSide, Mobile inAnchor, int baseDamage, LunaisSpell parentSpell)
		: base(inLevel, inPosition, inSide, -1, inAnchor, parentSpell)
	{
		IsFacingLeft = isFacingLeft;
		_directionRotationMultiplier = ((!IsFacingLeft) ? 1 : (-1));
		_sprite = _level.GCM.SpMiscProjectiles;
		_damageDimensions = new Point(112, 36);
		_bboxOffset = new Point(60, 0);
		_bbox = new Rectangle(inPosition.X, inPosition.Y, 8, 36);
		base.AnchorOffset = new Point(-60, -24);
		SnapBboxToPosition();
		ChangeAnimation(new AnimationSpec[1]
		{
			new AnimationSpec
			{
				Start = 4,
				Length = 4,
				IsInReverse = true,
				Speed = 0.05f,
				Type = EAnimationType.Once
			}
		});
		DrawOrigin = new Vector2(IsFacingLeft ? 118f : 10f, 32f);
		_doesRotateBasedOnVelocity = false;
		_power = baseDamage;
		_force = 4;
		_life = 0.6f;
		base.DamageTimeoutTime = 1f;
		_canDamageThings = false;
		_damageElement = EDamageElement.Blunt;
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
		foreach (float hammerRotation in HammerRotations)
		{
			float num = hammerRotation;
			_swordRotationOffsets.Add(new Point((int)((0.0 - Math.Cos(num)) * 30.0), (int)(Math.Sin(num) * 40.0)));
		}
	}

	public override void Update(float delta)
	{
		if (base.IsFrozen)
		{
			return;
		}
		_swordFrameTimer -= delta;
		if (_swordFrameTimer <= 0f)
		{
			_swordFrameTimer += 0.033f;
			_swordFrames.Clear();
			if (_swordFrameIndex < 7)
			{
				_swordFrames.Add(new GiantHammerFrame
				{
					Alpha = 1f,
					Offset = _swordRotationOffsets[0],
					Rotation = HammerRotations[0]
				});
			}
			else
			{
				int num = _swordFrameIndex - 7;
				_canDamageThings = true;
				switch (num)
				{
				case 0:
					_swordFrames.Add(new GiantHammerFrame
					{
						Alpha = 1f,
						Offset = _swordRotationOffsets[0],
						Rotation = HammerRotations[0]
					});
					_swordFrames.Add(new GiantHammerFrame
					{
						Alpha = 0.5f,
						Offset = _swordRotationOffsets[1],
						Rotation = HammerRotations[1]
					});
					_swordFrames.Add(new GiantHammerFrame
					{
						Alpha = 0.5f,
						Offset = _swordRotationOffsets[2],
						Rotation = HammerRotations[2]
					});
					_damageBbox = new Rectangle(0, 0, 130, 66);
					DamageBboxOffset = new Point(-26, -132);
					_brushTrailSize = 16;
					_trailShrinkRate = 0.00035f;
					break;
				case 1:
					_swordFrames.Add(new GiantHammerFrame
					{
						Alpha = 1f / 3f,
						Offset = _swordRotationOffsets[0],
						Rotation = HammerRotations[0]
					});
					_swordFrames.Add(new GiantHammerFrame
					{
						Alpha = 0.5f,
						Offset = _swordRotationOffsets[1],
						Rotation = HammerRotations[1]
					});
					_swordFrames.Add(new GiantHammerFrame
					{
						Alpha = 0.5f,
						Offset = _swordRotationOffsets[2],
						Rotation = HammerRotations[2]
					});
					_swordFrames.Add(new GiantHammerFrame
					{
						Alpha = 0.5f,
						Offset = _swordRotationOffsets[3],
						Rotation = HammerRotations[3]
					});
					_damageBbox = new Rectangle(0, 0, 188, 84);
					DamageBboxOffset = new Point(31, -118);
					_brushTrailSize = 32;
					_trailShrinkRate = 0.0008f;
					break;
				case 2:
					_swordFrames.Add(new GiantHammerFrame
					{
						Alpha = 0.125f,
						Offset = _swordRotationOffsets[0],
						Rotation = HammerRotations[0]
					});
					_swordFrames.Add(new GiantHammerFrame
					{
						Alpha = 1f / 6f,
						Offset = _swordRotationOffsets[1],
						Rotation = HammerRotations[1]
					});
					_swordFrames.Add(new GiantHammerFrame
					{
						Alpha = 0.25f,
						Offset = _swordRotationOffsets[2],
						Rotation = HammerRotations[2]
					});
					_swordFrames.Add(new GiantHammerFrame
					{
						Alpha = 1f / 3f,
						Offset = _swordRotationOffsets[3],
						Rotation = HammerRotations[3]
					});
					_swordFrames.Add(new GiantHammerFrame
					{
						Alpha = 0.5f,
						Offset = _swordRotationOffsets[4],
						Rotation = HammerRotations[4]
					});
					_swordFrames.Add(new GiantHammerFrame
					{
						Alpha = 0.5f,
						Offset = _swordRotationOffsets[5],
						Rotation = HammerRotations[5]
					});
					_damageBbox = new Rectangle(0, 0, 68, 101);
					DamageBboxOffset = new Point(118, -37);
					_brushTrailSize = 48;
					_trailShrinkRate = 0.0012f;
					break;
				default:
				{
					float num2 = (float)(num + 2) - 3f;
					_swordFrames.Add(new GiantHammerFrame
					{
						Alpha = 0.5f / (4f + num2),
						Offset = _swordRotationOffsets[0],
						Rotation = HammerRotations[0]
					});
					_swordFrames.Add(new GiantHammerFrame
					{
						Alpha = 0.5f / (3f + num2),
						Offset = _swordRotationOffsets[1],
						Rotation = HammerRotations[1]
					});
					_swordFrames.Add(new GiantHammerFrame
					{
						Alpha = 0.5f / (2f + num2),
						Offset = _swordRotationOffsets[2],
						Rotation = HammerRotations[2]
					});
					_swordFrames.Add(new GiantHammerFrame
					{
						Alpha = 0.5f / (1.5f + num2),
						Offset = _swordRotationOffsets[3],
						Rotation = HammerRotations[3]
					});
					_swordFrames.Add(new GiantHammerFrame
					{
						Alpha = 0.5f / (1f + num2),
						Offset = _swordRotationOffsets[4],
						Rotation = HammerRotations[4]
					});
					_swordFrames.Add(new GiantHammerFrame
					{
						Alpha = 1f,
						Offset = _swordRotationOffsets[5],
						Rotation = HammerRotations[5]
					});
					_trailColor.A = (byte)((float)(int)_trailColor.A * 0.5f);
					_damageBbox = new Rectangle(0, 0, 66, 53);
					DamageBboxOffset = new Point(119, -12);
					break;
				}
				}
			}
			_swordFrameIndex++;
		}
		base.Update(delta);
		_damageBbox.Location = _anchorObject.AnchorPosition.Add(DamageBboxOffset).Add(new Point(-_damageBbox.Width / 2, -_damageBbox.Height / 2));
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		DrawTrail(spriteBatch);
		Point drawPosition = _drawPosition;
		foreach (GiantHammerFrame swordFrame in _swordFrames)
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
		_level.PlayCue(ESFX.LunaisOrbImpactBlunt, intersectionCenter);
	}

	protected override Point FindDamagePoint(Alive target, Rectangle collisionRectangle)
	{
		return target.Bbox.Center;
	}
}
