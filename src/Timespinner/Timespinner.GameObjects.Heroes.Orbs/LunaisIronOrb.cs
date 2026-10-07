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

internal sealed class LunaisIronOrb : LunaisOrb
{
	private const int OrbTrailLength = 75;

	private const int MorphOffsetX = -22;

	private const int MorphOffsetY = -25;

	private const int SlashOffsetX = 33;

	private const int SlashOffsetY = 0;

	private const int SlashWidth = 2;

	private const int SlashHeight = 4;

	private const int RecoilOffsetX = 34;

	private const int RecoilOffsetY = 3;

	private const float MorphAnimationSpeed = 0.04f;

	private const float TimeForMorph = 0.16f;

	private const float TimeForSlash = 0.29000002f;

	private const float TimeForRecoil = 0.36f;

	private const float TimeToStartRecoil = 0.45f;

	private const float TimeForEntireAttack = 0.81f;

	private static readonly Point MorphAnchorOffset = new Point(1, 0);

	private static readonly Point HammerAnchorOffset = new Point(-2, -3);

	private static readonly Point OrbBboxOffset = new Point(1, 1);

	private static readonly Point OrbBboxDimensions = new Point(8, 8);

	private static readonly Vector2 OrbDrawOrigin = new Vector2(5f, 5f);

	private static readonly Color WhiteTrailColor = new Color(0.95f, 0.95f, 0.95f, 0.8f);

	private static readonly Color ThrowTrailColor = new Color(0.7f, 0.7f, 0.7f, 0.3f);

	public static EInventoryOrbType DefaultOrbColor => EInventoryOrbType.Iron;

	public override Point AnchorPosition => Position;

	public LunaisIronOrb(Level inLevel, LunaisObj parentLunais, Point inPosition, SpriteSheet inSprite, bool isFrontPlane)
		: base(inLevel, parentLunais, inPosition, inSprite, DefaultOrbColor, EOrbState.Idle, isFrontPlane)
	{
		_trailColor = base.IdleTrailColor;
		_trailLength = 75;
		_doesDrawBrushTrail = true;
		_bbox = new Rectangle(0, 0, OrbBboxDimensions.X, OrbBboxDimensions.Y);
		_bboxOffset = OrbBboxOffset;
		DrawOrigin = OrbDrawOrigin;
		_meleeAnimationSpeed = 0.07f;
		ChangeAnimation(15, 10, 0.05f, EAnimationType.Cycle);
		Update(0f);
	}

	public override void StartMeleeAttack(int whichAttack)
	{
		base.StartMeleeAttack(whichAttack);
		base.State = EOrbState.Melee;
		_trailColor = ThrowTrailColor;
		_level.PlayCue(ESFX.LunaisOrbSlashHeavy, Position);
	}

	protected override void UpdateMeleeAttack(float delta)
	{
		float currentThrowTime = _currentThrowTime;
		_currentThrowTime += delta;
		if (_currentThrowTime > 0.81f)
		{
			_currentThrowTime = 0f;
			_currentRecoilTime = 0f;
			base.State = EOrbState.Idle;
			_trailColor = base.IdleTrailColor;
		}
		else if (_currentThrowTime < 0.16f)
		{
			float num = (float)Math.Sin(_currentThrowTime / 0.16f * ((float)Math.PI / 2f));
			float num2 = -22f * (float)Math.Sin(num * ((float)Math.PI / 2f)) * (float)((!base.IsThrowingLeft) ? 1 : (-1));
			float num3 = -25f * (float)Math.Sin(num * ((float)Math.PI / 2f));
			float scaleFactor = 1f - num;
			Position = Vector2.Add(Vector2.Multiply(_baseOrbitPosition.ToVector2(), scaleFactor), new Vector2(((float)base.CurrentTarget.X + num2) * num, ((float)base.CurrentTarget.Y + num3) * num)).ToPoint();
			if (currentThrowTime == 0f)
			{
				_trailColor = WhiteTrailColor;
				Point anchorOffset = (base.IsThrowingLeft ? MorphAnchorOffset : new Point(MorphAnchorOffset.X * -1, MorphAnchorOffset.Y));
				_battleAnimations.Add(new BattleAnimation(_level.GCM.SpOrbMeleeIron, Position, _level)
				{
					TeamSide = _defaultTeam,
					IsFacingLeft = !base.IsThrowingLeft,
					AnchorObject = this,
					AnchorOffset = anchorOffset,
					AnimationSpeed = 0.04f,
					AnimationStart = 0,
					AnimationLength = 4
				});
			}
		}
		else if (_currentThrowTime < 0.45f)
		{
			if (currentThrowTime < 0.16f)
			{
				IronOrbMeleeDamageArea newProjectile = new IronOrbMeleeDamageArea(_level, Bbox.Center, ETeamSide.Heroes, this, !base.IsThrowingLeft, base.OrbDamage, this);
				_level.AddProjectile(newProjectile);
				Point anchorOffset2 = (base.IsThrowingLeft ? HammerAnchorOffset : new Point(HammerAnchorOffset.X * -1, HammerAnchorOffset.Y));
				BattleAnimation battleAnimation = new BattleAnimation(_level.GCM.SpOrbMeleeIron, Position, _level);
				battleAnimation.TeamSide = _defaultTeam;
				battleAnimation.IsFacingLeft = !base.IsThrowingLeft;
				battleAnimation.AnchorObject = this;
				battleAnimation.AnchorOffset = anchorOffset2;
				battleAnimation.AnimationSpeed = 0.32500002f;
				battleAnimation.AnimationStart = 8;
				battleAnimation.AnimationLength = 1;
				battleAnimation.DoesFadeOut = false;
				BattleAnimation battleAnimation2 = battleAnimation;
				battleAnimation2.Update(0f);
				_battleAnimations.Add(battleAnimation2);
				_doesDrawTrail = false;
				base.IsAtAttackApex = true;
			}
			float num4 = (_currentThrowTime - 0.16f) / 0.29000002f;
			float num5 = (float)Math.Sin(num4 * ((float)Math.PI / 2f));
			int num6 = (int)(num5 * 2f);
			int num7 = (int)(num5 * 4f);
			int num8 = (33 + num6) * ((!base.IsThrowingLeft) ? 1 : (-1));
			int num9 = num7;
			Position = new Point(base.CurrentTarget.X + num8, base.CurrentTarget.Y + num9);
		}
		else
		{
			if (currentThrowTime < 0.45f)
			{
				Point anchorOffset3 = (base.IsThrowingLeft ? HammerAnchorOffset : new Point(HammerAnchorOffset.X * -1, HammerAnchorOffset.Y));
				BattleAnimation battleAnimation3 = new BattleAnimation(_level.GCM.SpOrbMeleeIron, Position, _level);
				battleAnimation3.TeamSide = _defaultTeam;
				battleAnimation3.IsFacingLeft = !base.IsThrowingLeft;
				battleAnimation3.AnchorObject = this;
				battleAnimation3.AnchorOffset = anchorOffset3;
				battleAnimation3.DrawColor = Color.White * 0.8f;
				battleAnimation3.AnimationSpeed = 0.04f;
				battleAnimation3.AnimationStart = 8;
				battleAnimation3.AnimationLength = 4;
				BattleAnimation battleAnimation4 = battleAnimation3;
				battleAnimation4.Update(0f);
				_battleAnimations.Add(battleAnimation4);
				_doesDrawTrail = true;
				_trailColor = base.IdleTrailColor;
				ClearTrailHistory();
			}
			float num10 = (_currentThrowTime - 0.45f) / 0.36f;
			float num11 = (float)Math.Cos(num10 * ((float)Math.PI / 2f));
			float scaleFactor2 = 1f - num11;
			int num12 = 34 * ((!base.IsThrowingLeft) ? 1 : (-1));
			Position = Vector2.Add(Vector2.Multiply(_baseOrbitPosition.ToVector2(), scaleFactor2), new Vector2((float)(base.CurrentTarget.X + num12) * num11, (float)(base.CurrentTarget.Y + 3) * num11)).ToPoint();
		}
	}
}
