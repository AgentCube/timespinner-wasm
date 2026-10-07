using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Etc;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Enemies._16_Temple;

internal sealed class TempleZeal : Monster
{
	private const int MoveOffsetY = 24;

	private const int TilesToMove = 4;

	private const int TileSize = 16;

	private const int MaxSearchCount = 32;

	private const int BaseBallRadius = 32;

	private const float RunAwayDistanceThreshold = 80f;

	private const float OscillationFrequency = 2f;

	private const float OscillationRadius = 5f;

	private const float TimeForDeath = 0.35f;

	private const float DeathTimeMultiplier = 0.5f;

	private const int ChargeBallRadius = 24;

	private const int CastBallRadius = 112;

	private const int BallOffsetY = -4;

	private const int DeathBallFallHeight = 64;

	private const float BallOscillationFrequency = 5f;

	private const float TopBallTimeOffset = (float)Math.PI;

	private const float TimeToMove = 1.5f;

	private const float TimeToCharge = 0.75f;

	private const float TimeToCast = 0.2f;

	private const float TimeToWaitAfterAttacking = 2f;

	private const float TimeBeforeCasting = 2.25f;

	private const float TimeBeforeWaiting = 2.45f;

	private const float TotalActionTime = 4.45f;

	private static readonly Color BaseAuraColor = new Color(0.25f, 0.5f, 0.75f, 0.1f);

	private static readonly Color AscendedColor = new Color(0.3f, 0.5f, 0.75f, 0.25f);

	private static readonly Color BaseBallTrailColor = new Color(32, 64, 128, 32);

	private static readonly Color BallChargeTrailColor = new Color(128, 200, 248, 64);

	private readonly SFXCueInstance _ascendedLoopCueInstance;

	private readonly TempleZealIceBall _bottomBall;

	private readonly TempleZealIceBall _topBall;

	private readonly AscendedDrawHelper _ascendedDrawHelper;

	private float _zealTimer;

	private float _oscillationTimer;

	private float _ballOscillationTimer;

	private float _ballRadius;

	private float _ballOscillMultiplier;

	private Point _movementStart;

	private Point _movementDestination;

	private Point _movementPosition;

	private TempleZealIceDamageArea _floorDamageArea;

	private TempleZealIceDamageArea _ceilingDamageArea;

	public TempleZeal(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		Bbox = new Rectangle(0, 0, 16, 16);
		ChangeAnimation(-1);
		_doAppendagesMatchImageFacing = true;
		_doAppendagesInheritDrawColor = true;
		_currentAI = EAIStrategy.CustomScriptAI;
		_currentAction = EAIAction.Custom;
		_nonAggroAction = EAIAction.Custom;
		_canLoseAggro = false;
		_agility = 0.25f;
		_isAffectedByGravity = false;
		_isFlying = true;
		base.DoesTouchDamageKnockback = true;
		_ballOscillMultiplier = 1f;
		_ballRadius = 32f;
		_bottomBall = new TempleZealIceBall(this, new Point(8, 8), Point.Zero, _level, _sprite)
		{
			DrawPriority = 1,
			TrailColor = BaseBallTrailColor
		};
		_topBall = new TempleZealIceBall(this, new Point(8, 8), Point.Zero, _level, _sprite)
		{
			DrawPriority = 1,
			TrailColor = BaseBallTrailColor
		};
		_ascendedDrawHelper = new AscendedDrawHelper(this, 5);
		_ascendedDrawHelper.SetGlowColor(AscendedColor);
		_movementPosition = inPosition;
		base.DoesDrawAura = true;
		base.DoesDrawAppendageAuras = true;
		base.AuraColor = BaseAuraColor;
		base.AuraOffset = new Vector2(1f, 2f);
		base.AuraFrequency = 9f;
		base.AuraSize = 0.075f;
		_auraCount = 5f;
		_ascendedLoopCueInstance = CreateCue(ESFX.EnemyTempleAscendedLoop, Position, isLooped: true);
		if (_ascendedLoopCueInstance != null)
		{
			_ascendedLoopCueInstance.PlayWhenInRange();
		}
	}

	public override void InitializeMob()
	{
		Protagonist mainHero = _level.MainHero;
		if (mainHero != null)
		{
			IsFacingLeft = mainHero.Position.X < Position.X;
		}
		base.InitializeMob();
	}

	protected override void UpdateCustomScriptAIAction(float delta)
	{
		if (_isAggroed)
		{
			if (_zealTimer <= 0f)
			{
				Point playerPosition = _level.GetPlayerPosition();
				IsFacingLeft = playerPosition.X <= Position.X;
				_movementStart = _movementPosition;
				int num = Math.Abs(playerPosition.X - Position.X);
				bool flag = (float)num <= 80f;
				bool isGoingLeft = (IsFacingLeft && !flag) || (!IsFacingLeft && flag);
				_movementDestination = PickNextDestination(_level, Bbox.Center, isGoingLeft).Add(0, 24);
			}
			float zealTimer = _zealTimer;
			_zealTimer += delta;
			if (_zealTimer <= 1.5f)
			{
				float amount = _zealTimer / 1.5f;
				_movementPosition = _movementStart.WaveInterpolate(_movementDestination, amount);
			}
			else if (_zealTimer < 2.25f)
			{
				bool flag2 = true;
				if (zealTimer <= 1.5f)
				{
					int num2 = Math.Abs(_level.GetPlayerPosition().X - Position.X);
					if (num2 > 240)
					{
						flag2 = false;
						_zealTimer = 0f;
					}
				}
				if (flag2)
				{
					float num3 = (_zealTimer - 1.5f) / 0.75f;
					_ballRadius = MathEx.SineInterpolate(32f, 24f, num3);
					_ballOscillMultiplier = MathEx.SineInterpolate(1f, 0f, num3);
					Color trailColor = BaseBallTrailColor.SineInterpolate(BallChargeTrailColor, num3);
					_bottomBall.TrailColor = trailColor;
					_topBall.TrailColor = trailColor;
				}
			}
			else if (_zealTimer < 2.45f)
			{
				if (zealTimer < 2.25f)
				{
					_topBall.AddFlash();
					_bottomBall.AddFlash();
				}
				float num4 = (_zealTimer - 2.25f) / 0.2f;
				_ballRadius = MathEx.CosInterpolate(24f, 112f, num4);
				_ballOscillMultiplier = 0f;
				Color drawColor = Color.White.CosInterpolate(Color.Transparent, num4);
				_bottomBall.DrawColor = drawColor;
				_topBall.DrawColor = drawColor;
				Color trailColor2 = BallChargeTrailColor.CosInterpolate(Color.Transparent, num4);
				_bottomBall.TrailColor = trailColor2;
				_topBall.TrailColor = trailColor2;
			}
			else if (_zealTimer < 4.45f)
			{
				if (zealTimer < 2.45f)
				{
					CastSpell();
					_ballOscillMultiplier = 1f;
					_ballRadius = 32f;
					_topBall.ClearTrailHistory();
					_bottomBall.ClearTrailHistory();
				}
				float amount2 = (_zealTimer - 2.45f) / 2f;
				Color drawColor2 = Color.Transparent.SineInterpolate(Color.White, amount2);
				_bottomBall.DrawColor = drawColor2;
				_topBall.DrawColor = drawColor2;
				Color trailColor3 = Color.Transparent.SineInterpolate(BaseBallTrailColor, amount2);
				_bottomBall.TrailColor = trailColor3;
				_topBall.TrailColor = trailColor3;
			}
			else
			{
				_zealTimer = 0f;
				_ballOscillMultiplier = 1f;
				_ballRadius = 32f;
				_bottomBall.TrailColor = BaseBallTrailColor;
				_topBall.TrailColor = BaseBallTrailColor;
				_bottomBall.DrawColor = Color.White;
				_topBall.DrawColor = Color.White;
			}
		}
		else
		{
			_zealTimer = 0f;
		}
		_oscillationTimer += delta * 2f;
		if (_oscillationTimer >= (float)Math.PI * 2f)
		{
			_oscillationTimer -= (float)Math.PI * 2f;
		}
		int x = (int)Math.Round(Math.Cos(_oscillationTimer) * 5.0);
		int y = (int)Math.Round(Math.Sin(_oscillationTimer) * 5.0);
		Position = _movementPosition.Add(x, y);
	}

	private static Point PickNextDestination(Level level, Point startingPoint, bool isGoingLeft)
	{
		Point point = startingPoint;
		Point inPosition = point.Add(isGoingLeft ? (-16) : 16, 0);
		for (int i = 0; i < 4; i++)
		{
			Tile nearestSolidTile = level.GetNearestSolidTile(inPosition, EDirection.North, 32);
			Tile nearestSolidTile2 = level.GetNearestSolidTile(inPosition, EDirection.South, 32);
			if (nearestSolidTile == null || nearestSolidTile2 == null)
			{
				break;
			}
			point = new Point(inPosition.X, (nearestSolidTile.Position.Y + nearestSolidTile2.Position.Y) / 2);
			inPosition = point.Add(isGoingLeft ? (-16) : 16, 0);
		}
		return point;
	}

	public override void Update(float delta)
	{
		if (_isFrozen)
		{
			_isFrozen = false;
		}
		if (!base.IsFrozen)
		{
			_ascendedDrawHelper.Update(delta);
			UpdateIceBalls(delta);
		}
		base.Update(delta);
	}

	private void UpdateIceBalls(float delta)
	{
		_ballOscillationTimer += delta * 5f;
		if (_ballOscillationTimer >= (float)Math.PI * 2f)
		{
			_ballOscillationTimer -= (float)Math.PI * 2f;
		}
		float num = 1f - _ballOscillMultiplier;
		SetBallOscillation(this, _bottomBall, _ballOscillationTimer, _ballRadius, _ballOscillMultiplier, num);
		SetBallOscillation(this, _topBall, _ballOscillationTimer + (float)Math.PI, _ballRadius, _ballOscillMultiplier, 0f - num);
		_topBall.Update(delta);
		_bottomBall.Update(delta);
	}

	private static void SetBallOscillation(Animate parent, TempleZealIceBall ball, float time, float radius, float idleMult, float attackMult)
	{
		float num = (float)Math.Cos(time);
		float num2 = (float)Math.Sin(time);
		int x = (int)Math.Round(num * radius * idleMult);
		int num3 = (int)Math.Round(num2 * radius * idleMult);
		int num4 = (int)Math.Round(radius * attackMult);
		Point position = parent.Position;
		Point position2 = position.Add(x, num3 + -4 + num4);
		ball.Position = position2;
		ball.DoesDrawBoundingBox = false;
	}

	private void CastSpell()
	{
		SFXCueInstance sFXCueInstance = PlayCue(ESFX.EnemyTempleZealSpell);
		if (sFXCueInstance != null)
		{
			sFXCueInstance.RangeMultiplier = 2f;
		}
		bool isTravelingLeft = IsFacingLeft;
		Protagonist mainHero = _level.MainHero;
		if (mainHero != null)
		{
			isTravelingLeft = mainHero.Position.X <= Position.X;
		}
		Tile tile = _level.FindFirstSolidTileInDirection(_movementPosition, EDirection.South);
		if (tile != null)
		{
			Point point = new Point(tile.Bbox.Center.X, tile.Bbox.Bottom);
			if (_floorDamageArea == null)
			{
				_floorDamageArea = new TempleZealIceDamageArea(_level, point, base.Damage, _sprite);
			}
			_floorDamageArea.Reset(point, isTravelingLeft, isOnFloor: true);
			_level.AddProjectile(_floorDamageArea);
		}
		tile = _level.FindFirstSolidTileInDirection(Bbox.Center, EDirection.North);
		if (tile != null)
		{
			Point point2 = new Point(tile.Bbox.Center.X, tile.Bbox.Top);
			if (_ceilingDamageArea == null)
			{
				_ceilingDamageArea = new TempleZealIceDamageArea(_level, point2, base.Damage, _sprite);
			}
			_ceilingDamageArea.Reset(point2, isTravelingLeft, isOnFloor: false);
			_level.AddProjectile(_ceilingDamageArea);
		}
	}

	protected override void UpdateDeathScript(float delta)
	{
		if (_deathScriptTimer <= 0f)
		{
			DropLoot();
			_ascendedDrawHelper.ScatterSparkles();
			_ascendedDrawHelper.AddShockwave();
			_topBall.DeathPoint = _topBall.Position;
			_bottomBall.DeathPoint = _bottomBall.Position;
			_topBall.DeathColor = _topBall.DrawColor;
			_bottomBall.DeathColor = _bottomBall.DrawColor;
			_topBall.DeathTrailColor = _topBall.TrailColor;
			_bottomBall.DeathTrailColor = _bottomBall.TrailColor;
			if (_ascendedLoopCueInstance != null)
			{
				_ascendedLoopCueInstance.Stop();
			}
			PlayCue(ESFX.EnemyTempleAscendedDeath);
		}
		if (_deathScriptTimer < 0.35f)
		{
			float num = (float)Math.Cos(_deathScriptTimer / 0.35f * ((float)Math.PI / 2f));
			base.DrawColor = Color.White * num;
			base.AuraColor = BaseAuraColor * num;
			_ascendedDrawHelper.SetGlowColor(AscendedColor * num);
			num = 1f - num;
			Color drawColor = _topBall.DeathColor.Lerp(Color.Transparent, num);
			Color trailColor = _topBall.DeathTrailColor.Lerp(Color.Transparent, num);
			int y = (int)Math.Ceiling(MathEx.CosInterpolate(0f, 64f, num));
			_topBall.DrawColor = drawColor;
			_bottomBall.DrawColor = drawColor;
			_topBall.TrailColor = trailColor;
			_bottomBall.TrailColor = trailColor;
			_topBall.Position = _topBall.DeathPoint.Add(0, y);
			_bottomBall.Position = _topBall.DeathPoint.Add(0, y);
			_topBall.Update(delta);
			_bottomBall.Update(delta);
			UpdateCharacterSequences(delta * 0.5f);
		}
		else
		{
			RemoveInstance();
		}
		_deathScriptTimer += delta;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		_ascendedDrawHelper.Draw(spriteBatch, isAbove: false);
		base.Draw(spriteBatch);
		_topBall.Draw(spriteBatch);
		_bottomBall.Draw(spriteBatch);
		_ascendedDrawHelper.Draw(spriteBatch, isAbove: true);
	}
}
