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

internal sealed class TempleConviction : Monster
{
	private const int ShieldChargeAnchorOffsetX = -28;

	private const int ShieldBashAnchorOffsetX = -180;

	private const int ShieldGlowBase = 4;

	private const int TilesToMove = 6;

	private const int TileSize = 16;

	private const int MaxSearchCount = 32;

	private const int MoveOffsetY = 24;

	private const int RunAwayDistanceThreshold = 80;

	private const float OscillationFrequency = 2f;

	private const float OscillationRadius = 5f;

	private const float TimeToMove = 1.5f;

	private const float TimeToCharge = 0.5f;

	private const float TimeToCast = 0.35f;

	private const float TimeToWaitAfterAttacking = 1.2f;

	private const float TimeBeforeCasting = 2f;

	private const float TimeBeforeWaiting = 2.35f;

	private const float TotalActionTime = 3.55f;

	private const float TimeForDeath = 0.35f;

	private const float DeathTimeMultiplier = 0.5f;

	private static readonly Color BaseAuraColor = new Color(0.7f, 0.5f, 0.2f, 0.1f);

	private static readonly Color AscendedColor = new Color(0.7f, 0.5f, 0.2f, 0.25f);

	private static readonly Color ShieldGlowColor = new Color(1f, 0.9f, 0.6f, 0.75f);

	private readonly SFXCueInstance _ascendedLoopCueInstance;

	private readonly TempleConvictionShield _shield;

	private readonly AscendedDrawHelper _ascendedDrawHelper;

	private float _zealTimer;

	private float _oscillationTimer;

	private Point _movementStart;

	private Point _movementDestination;

	private Point _movementPosition;

	public TempleConviction(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		Bbox = new Rectangle(0, 0, 16, 16);
		ChangeAnimation(-1);
		_currentAI = EAIStrategy.CustomScriptAI;
		_currentAction = EAIAction.Custom;
		_nonAggroAction = EAIAction.Custom;
		_canLoseAggro = false;
		_doAppendagesMatchImageFacing = true;
		_doAppendagesInheritDrawColor = true;
		_isAffectedByGravity = false;
		_isFlying = true;
		_shield = new TempleConvictionShield(_level, Position, this, _sprite);
		_level.RequestAddObject(_shield);
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
			_shield.ForceImageFacing(IsFacingLeft);
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
				bool flag = num <= 80;
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
			else if (_zealTimer < 2f)
			{
				if (zealTimer <= 1.5f)
				{
					_shield.PlayCue(ESFX.EnemyTempleConvictionBash);
				}
				float percentage = (_zealTimer - 1.5f) / 0.5f;
				int x = (int)MathEx.SineInterpolate(-40f, -28f, percentage);
				_shield.AnchorOffset = new Point(x, -8);
				_shield.IsGlowing = true;
				_shield.GlowColor = ShieldGlowColor;
				_shield.GlowBase = MathEx.SineInterpolate(1f, 4f, percentage);
			}
			else if (_zealTimer < 2.35f)
			{
				_ = 2f;
				float num2 = (_zealTimer - 2f) / 0.35f;
				int x2 = (int)MathEx.CosInterpolate(-28f, -180f, num2);
				_shield.AnchorOffset = new Point(x2, -8);
				_shield.IsGlowing = true;
				_shield.GlowColor = ShieldGlowColor;
				_shield.GlowBase = MathEx.SineInterpolate(1f, 4f, 1f - num2);
			}
			else if (_zealTimer < 3.55f)
			{
				if (zealTimer < 2.35f)
				{
					_shield.IsGlowing = false;
					_shield.DrawColor = Color.White;
				}
				float percentage2 = (_zealTimer - 2.35f) / 1.2f;
				int x3 = (int)MathEx.SineInterpolate(-180f, -40f, percentage2);
				_shield.AnchorOffset = new Point(x3, -8);
			}
			else
			{
				_zealTimer = 0f;
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
		int x4 = (int)Math.Round(Math.Cos(_oscillationTimer) * 5.0);
		int y = (int)Math.Round(Math.Sin(_oscillationTimer) * 5.0);
		Position = _movementPosition.Add(x4, y);
	}

	private static Point PickNextDestination(Level level, Point startingPoint, bool isGoingLeft)
	{
		Point point = startingPoint;
		Point inPosition = point.Add(isGoingLeft ? (-16) : 16, 0);
		for (int i = 0; i < 6; i++)
		{
			Tile nearestSolidTile = level.GetNearestSolidTile(inPosition, EDirection.North, 32);
			Tile nearestSolidTile2 = level.GetNearestSolidTile(inPosition, EDirection.South, 32);
			if (nearestSolidTile == null || nearestSolidTile2 == null)
			{
				break;
			}
			point = new Point(inPosition.X, (int)((float)(nearestSolidTile.Position.Y + nearestSolidTile2.Position.Y) * 0.65f));
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
		}
		base.Update(delta);
	}

	protected override void UpdateDeathScript(float delta)
	{
		if (_deathScriptTimer <= 0f)
		{
			DropLoot();
			_ascendedDrawHelper.ScatterSparkles();
			_ascendedDrawHelper.AddShockwave();
			_shield.Power = 0f;
			_shield.IsGlowing = false;
			_shield.ForceImageFacing(IsFacingLeft);
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
			UpdateCharacterSequences(delta * 0.5f);
			_shield.DrawColor = base.DrawColor;
			_shield.AuraColor = base.AuraColor;
		}
		else
		{
			RemoveInstance();
		}
		_deathScriptTimer += delta;
	}

	public override void SilentKill()
	{
		if (_shield != null)
		{
			_shield.SilentKill();
		}
		base.SilentKill();
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		_ascendedDrawHelper.Draw(spriteBatch, isAbove: false);
		base.Draw(spriteBatch);
		_ascendedDrawHelper.Draw(spriteBatch, isAbove: true);
	}
}
