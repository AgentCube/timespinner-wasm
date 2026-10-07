using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Sandman;

internal sealed class SandmanBossCrusherProjectile : Projectile
{
	internal enum ESandmanCrusherType
	{
		Left,
		Center,
		Right
	}

	private const int Anim_BoneFrameIndex = 17;

	private const int BboxWidth = 128;

	private const int BboxHeight = 48;

	private const int PieceWidth = 64;

	private const int StartPointOffsetY = 32;

	private const int FloorY = 224;

	private const float TimeToMeltFromCeiling = 0.5f;

	private const float TimeToWaitBeforeMeltingAfterLanding = 0.35f;

	private const float TimeToMeltIntoGround = 0.5f;

	private const float CeilingWaitTime = 0.58f;

	private const float MaxLife = 3f;

	private static readonly Color MeltBaseColor = new Color(200, 128, 176, 128);

	private readonly Vector2 _sandTextureRatio = new Vector2(2f, 2f);

	private readonly SandDrawHelper _sandDrawHelper;

	private readonly SandmanCrusherDustParticleSystem _dustParticles;

	private readonly SandmanBossCeilingGoo _ceilingGoo;

	private bool _hasTouchedGround;

	private bool _hasStartedFalling;

	private bool _isWaitingToFall;

	private bool _isDrawingSand;

	private ESandmanCrusherType _crusherType;

	private float _meltTimer;

	private float _floorWaitTimer;

	private float _ceilingWaitTimer;

	private Point _floorPosition;

	private Point _ceilingPosition;

	internal bool IsFinished { get; private set; }

	public SandmanBossCrusherProjectile(Level inLevel, Point inPosition, SpriteSheet sprite, int baseDamage)
		: base(inLevel, inPosition, Vector2.Zero, ETeamSide.Enemies, -1)
	{
		_ceilingPosition = new Point(inPosition.X, inPosition.Y + 32);
		_sprite = sprite;
		base.Power = (int)Math.Ceiling((float)baseDamage * 1.15f);
		Bbox = new Rectangle(0, 0, 128, 48);
		_isAffectedByGravity = false;
		_doesDieOnTiles = false;
		_doesBounceOnGround = true;
		_doesCollideWithFloors = true;
		_doesRotateBasedOnVelocity = false;
		base.DoesCollideWithTiles = false;
		base.DoesDieOnImpact = false;
		base.DoesKnockBack = true;
		base.IsDamageArea = true;
		_life = 3f;
		ChangeAnimation(-1);
		Position = new Point(_ceilingPosition.X, _ceilingPosition.Y - 48);
		_sandDrawHelper = new SandDrawHelper(this);
		_ceilingGoo = new SandmanBossCeilingGoo(_level, new Point(_ceilingPosition.X, _floorPosition.Y), _sprite, new ObjectTileSpecification());
		_level.RequestAddObject(_ceilingGoo);
		for (int i = 0; i < 2; i++)
		{
			bool isFacingLeft = i == 0;
			Appendage appendage = new Appendage(this, new Point(64, 48), Point.Zero, _level, _sprite)
			{
				FollowType = EAppendageFollowType.AnchorLocked,
				AnchorObject = this,
				AnchorOffset = new Point(-32, 0),
				IsFacingLeft = isFacingLeft
			};
			appendage.ChangeAnimation(17);
			_appendages.Add(appendage);
		}
		_dustParticles = new SandmanCrusherDustParticleSystem(_level.GCM.TxParticleEnergy, 1);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			_dustParticles.Update(delta);
			_sandDrawHelper.Update(delta);
			if (_isWaitingToFall)
			{
				_ceilingWaitTimer += delta;
				if (_ceilingWaitTimer >= 0.58f)
				{
					_isWaitingToFall = false;
				}
			}
			if (!_isWaitingToFall)
			{
				if (!_hasStartedFalling)
				{
					if (_meltTimer <= 0f && delta > 0f)
					{
						switch (_crusherType)
						{
						case ESandmanCrusherType.Left:
							PlayCue2D(ESFX.BossSandmanTeethLeft);
							break;
						case ESandmanCrusherType.Center:
							PlayCue2D(ESFX.BossSandmanTeethCenter);
							break;
						case ESandmanCrusherType.Right:
							PlayCue2D(ESFX.BossSandmanTeethRight);
							break;
						}
					}
					_meltTimer += delta;
					if (_meltTimer <= 0.5f)
					{
						float num = _meltTimer / 0.5f;
						base.DrawColor = MeltBaseColor.SineInterpolate(Color.White, num);
						Position = new Point(_ceilingPosition.X, _ceilingPosition.Y - (int)((1.0 - Math.Sin(num * ((float)Math.PI / 2f))) * 48.0));
					}
					else
					{
						Position = _ceilingPosition;
						_hasStartedFalling = true;
						_meltTimer = 0f;
						_isAffectedByGravity = true;
						base.DoesCollideWithTiles = true;
					}
					SnapBboxToPosition();
				}
				else if (_hasTouchedGround)
				{
					_floorWaitTimer += delta;
					if (_floorWaitTimer >= 0.35f)
					{
						if (_meltTimer <= 0f)
						{
							_floorPosition = Position;
							_isAffectedByGravity = false;
							base.DoesCollideWithTiles = false;
							_velocity = Vector2.Zero;
							base.CanDamageThings = false;
						}
						_meltTimer += delta;
						if (_meltTimer <= 0.5f)
						{
							float num2 = _meltTimer / 0.5f;
							base.DrawColor = Color.White.SineInterpolate(MeltBaseColor, num2);
							Position = new Point(_floorPosition.X, _floorPosition.Y + (int)((1.0 - Math.Cos(num2 * ((float)Math.PI / 2f))) * 48.0));
						}
						else
						{
							SilentKill();
						}
					}
				}
			}
		}
		base.Update(delta);
	}

	public override void SnapBboxToPosition()
	{
		_bbox.Location = new Point(_position.X - _bbox.Width / 2, _position.Y - _bbox.Height);
	}

	protected override void DoBounce(Point impactPoint)
	{
		if (!_hasTouchedGround)
		{
			_level.RequestScreenShake(new Vector2(0f, 5f), 0.4f, 6f, isAffectedByTime: false);
			_dustParticles.AddParticles(new Vector2(Position.X, impactPoint.Y));
		}
		_hasTouchedGround = true;
	}

	public override void SilentKill()
	{
		IsFinished = true;
		base.SilentKill();
	}

	internal void Reset(Point position, ESandmanCrusherType crusherType)
	{
		_crusherType = crusherType;
		_isFading = false;
		base.DrawColor = Color.White;
		_fadeTimer = 0f;
		IsFinished = false;
		_isWaitingToFall = true;
		_ceilingWaitTimer = 0f;
		base.CanDamageThings = true;
		_ceilingPosition = new Point(position.X, position.Y + 32);
		Position = new Point(_ceilingPosition.X, _ceilingPosition.Y - 48);
		SnapBboxToPosition();
		_ceilingGoo.ThrowToCeiling(new Point(position.X, 224));
		_hasTouchedGround = false;
		_hasStartedFalling = false;
		_isDrawingSand = false;
		_meltTimer = 0f;
		_floorWaitTimer = 0f;
		_life = 3f;
		base.ID = -1;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (!_isDrawingSand)
		{
			_isDrawingSand = true;
			_sandDrawHelper.Draw(spriteBatch, this, _sandTextureRatio);
			_isDrawingSand = false;
			_dustParticles.Draw(spriteBatch, _level.LevelRenderCenter, _level.CameraPosition, _level.CameraZoom);
		}
		else
		{
			base.Draw(spriteBatch);
		}
	}
}
