using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.LunaisProjectiles;

namespace Timespinner.GameObjects.Heroes.Orbs;

internal sealed class LunaisBladeOrb : LunaisOrb
{
	private const int OrbTrailLength = 75;

	private const int MorphOffsetX = -9;

	private const int MorphOffsetY = -23;

	private const int SlashOffsetX = 18;

	private const int SlashOffsetY = 0;

	private const int SlashWidth = 3;

	private const int SlashHeight = 3;

	private const int RecoilOffsetX = 18;

	private const int RecoilOffsetY = 0;

	private const float MorphAnimationSpeed = 0.035f;

	private const float TimeForMorph = 0.14f;

	private const float TimeForSlash = 0.14f;

	private const float TimeForRecoil = 0.29000002f;

	private const float TimeToStartRecoil = 0.28f;

	private const float TimeForEntireAttack = 0.57f;

	private static readonly Point OrbBboxOffset = new Point(1, 1);

	private static readonly Point OrbBboxDimensions = new Point(8, 8);

	private static readonly Vector2 OrbDrawOrigin = new Vector2(5f, 5f);

	private static readonly Color WhiteTrailColor = new Color(0.95f, 0.95f, 1f, 0.8f);

	private static readonly Color ThrowTrailColor = new Color(0.4f, 0.8f, 0.4f, 0.3f);

	private Point _morphAnchorOffset = new Point(1, 0);

	private Point _recoilAnchorOffset = new Point(-17, -1);

	public static EInventoryOrbType DefaultOrbColor => EInventoryOrbType.Blade;

	public override Point AnchorPosition => Position;

	public LunaisBladeOrb(Level inLevel, LunaisObj parentLunais, Point inPosition, SpriteSheet inSprite, bool isFrontPlane)
		: base(inLevel, parentLunais, inPosition, inSprite, DefaultOrbColor, EOrbState.Idle, isFrontPlane)
	{
		_trailColor = base.IdleTrailColor;
		_trailLength = 75;
		_doesDrawBrushTrail = true;
		_bbox = new Rectangle(0, 0, OrbBboxDimensions.X, OrbBboxDimensions.Y);
		_bboxOffset = OrbBboxOffset;
		DrawOrigin = OrbDrawOrigin;
		_meleeAnimationSpeed = 0.07f;
		ChangeAnimation(12, 10, 0.05f, EAnimationType.Cycle);
		Update(0f);
	}

	public override void StartMeleeAttack(int whichAttack)
	{
		base.StartMeleeAttack(whichAttack);
		base.State = EOrbState.Melee;
		_trailColor = ThrowTrailColor;
		_level.PlayCue(ESFX.LunaisOrbSlashLight, Position);
	}

	protected override void UpdateMeleeAttack(float delta)
	{
		float currentThrowTime = _currentThrowTime;
		_currentThrowTime += delta;
		if (_currentThrowTime > 0.57f)
		{
			_currentThrowTime = 0f;
			_currentRecoilTime = 0f;
			base.State = EOrbState.Idle;
			_trailColor = base.IdleTrailColor;
		}
		else if (_currentThrowTime < 0.14f)
		{
			float num = (float)Math.Sin(_currentThrowTime / 0.14f * ((float)Math.PI / 2f));
			float num2 = -9f * (float)Math.Sin(num * ((float)Math.PI / 2f)) * (float)((!base.IsThrowingLeft) ? 1 : (-1));
			float num3 = -23f * (float)Math.Sin(num * ((float)Math.PI / 2f));
			float scaleFactor = 1f - num;
			Position = Vector2.Add(Vector2.Multiply(_baseOrbitPosition.ToVector2(), scaleFactor), new Vector2(((float)base.CurrentTarget.X + num2) * num, ((float)base.CurrentTarget.Y + num3) * num)).ToPoint();
			if (currentThrowTime <= 0f)
			{
				_trailColor = WhiteTrailColor;
				Point anchorOffset = (base.IsThrowingLeft ? _morphAnchorOffset : new Point(_morphAnchorOffset.X * -1, _morphAnchorOffset.Y));
				_battleAnimations.Add(new BattleAnimation(_level.GCM.SpOrbMeleeBlade, Position, _level)
				{
					TeamSide = _defaultTeam,
					IsFacingLeft = !base.IsThrowingLeft,
					AnchorObject = this,
					AnchorOffset = anchorOffset,
					AnimationSpeed = 0.035f,
					AnimationStart = 0,
					AnimationLength = 4
				});
			}
		}
		else if (_currentThrowTime < 0.28f)
		{
			if (currentThrowTime < 0.14f)
			{
				BladeOrbMeleeDamageArea newProjectile = new BladeOrbMeleeDamageArea(_level, Bbox.Center, ETeamSide.Heroes, this, !base.IsThrowingLeft, base.OrbDamage, this);
				_level.AddProjectile(newProjectile);
				Point anchorOffset2 = (base.IsThrowingLeft ? _recoilAnchorOffset : new Point(_recoilAnchorOffset.X * -1, -1));
				_battleAnimations.Add(new BattleAnimation(_level.GCM.SpOrbMeleeBlade, Position, _level)
				{
					TeamSide = _defaultTeam,
					IsFacingLeft = !base.IsThrowingLeft,
					AnchorObject = this,
					AnchorOffset = anchorOffset2,
					AnimationSpeed = 0.14f,
					AnimationStart = 8,
					AnimationLength = 1
				});
				_doesDrawTrail = false;
				base.IsAtAttackApex = true;
			}
			float num4 = (_currentThrowTime - 0.14f) / 0.14f;
			float num5 = (float)Math.Cos(num4 * ((float)Math.PI / 2f));
			int num6 = (int)((0f - num5) * 3f);
			int num7 = (int)((0f - num5) * 3f);
			int num8 = (18 + num6) * ((!base.IsThrowingLeft) ? 1 : (-1));
			int num9 = num7;
			Position = new Point(base.CurrentTarget.X + num8, base.CurrentTarget.Y + num9);
		}
		else
		{
			if (currentThrowTime < 0.28f)
			{
				Point anchorOffset3 = (base.IsThrowingLeft ? _recoilAnchorOffset : new Point(_recoilAnchorOffset.X * -1, -1));
				_battleAnimations.Add(new BattleAnimation(_level.GCM.SpOrbMeleeBlade, Position, _level)
				{
					TeamSide = _defaultTeam,
					IsFacingLeft = !base.IsThrowingLeft,
					AnchorObject = this,
					AnchorOffset = anchorOffset3,
					DrawColor = Color.White * 0.8f,
					AnimationSpeed = 0.035f,
					AnimationStart = 8,
					AnimationLength = 4
				});
				_doesDrawTrail = true;
				_trailColor = base.IdleTrailColor;
				ClearTrailHistory();
			}
			float num10 = (_currentThrowTime - 0.28f) / 0.29000002f;
			float num11 = (float)Math.Cos(num10 * ((float)Math.PI / 2f));
			float scaleFactor2 = 1f - num11;
			int num12 = 18 * ((!base.IsThrowingLeft) ? 1 : (-1));
			Position = Vector2.Add(Vector2.Multiply(_baseOrbitPosition.ToVector2(), scaleFactor2), new Vector2((float)(base.CurrentTarget.X + num12) * num11, (float)base.CurrentTarget.Y * num11)).ToPoint();
		}
	}
}
