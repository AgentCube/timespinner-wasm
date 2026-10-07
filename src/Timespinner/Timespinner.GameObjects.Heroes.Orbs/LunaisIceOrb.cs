using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.LunaisProjectiles;

namespace Timespinner.GameObjects.Heroes.Orbs;

internal sealed class LunaisIceOrb : LunaisOrb
{
	private const int OrbTrailLength = 75;

	private const float TimeToWaitAfterThrowing = 0.45f;

	private static readonly Point OrbBboxOffset = new Point(1, 1);

	private static readonly Point OrbBboxDimensions = new Point(8, 8);

	private static readonly Vector2 OrbDrawOrigin = new Vector2(5f, 5f);

	private IceOrbMeleeSnowProjectile _snowProjectile;

	public static EInventoryOrbType DefaultOrbColor => EInventoryOrbType.Ice;

	public LunaisIceOrb(Level inLevel, LunaisObj parentLunais, Point inPosition, SpriteSheet inSprite, bool isFrontPlane)
		: base(inLevel, parentLunais, inPosition, inSprite, DefaultOrbColor, EOrbState.Idle, isFrontPlane)
	{
		_trailColor = base.IdleTrailColor;
		_trailLength = 75;
		_doesDrawBrushTrail = true;
		_bbox = new Rectangle(0, 0, OrbBboxDimensions.X, OrbBboxDimensions.Y);
		_bboxOffset = OrbBboxOffset;
		DrawOrigin = OrbDrawOrigin;
		ChangeAnimation(18, 10, 0.05f, EAnimationType.Cycle);
		Update(0f);
	}

	public override void StartMeleeAttack(int whichAttack)
	{
		base.StartMeleeAttack(whichAttack);
		base.State = EOrbState.Melee;
		_battleAnimations.Add(new BattleAnimation(_level.GCM.SpEffectsSmall, new Point(Bbox.Center.X - 3, Bbox.Center.Y), _level)
		{
			TeamSide = ETeamSide.Heroes,
			AnimationStart = 69,
			AnimationLength = 4,
			DrawColor = Color.White * 0.75f,
			IsFacingLeft = base.IsOrbFacingLeft
		});
		Vector2 iV = new Vector2(((!base.IsThrowingLeft) ? 1 : (-1)) * 3000, -150f);
		_snowProjectile = new IceOrbMeleeSnowProjectile(_level, Bbox.Center, iV, ETeamSide.Heroes, base.OrbDamage, this);
		_level.AddProjectile(_snowProjectile);
		_level.PlayCue(ESFX.LunaisOrbIceWhiff, Position);
	}

	protected override void UpdateMeleeAttack(float delta)
	{
		Position = _baseOrbitPosition;
		if (_currentThrowTime <= 0f && delta > 0f)
		{
			base.IsAtAttackApex = true;
		}
		_currentThrowTime += delta;
		if (_currentThrowTime >= 0.45f)
		{
			FinishAttack();
		}
	}

	private void FinishAttack()
	{
		base.State = EOrbState.Idle;
		_doesDrawBaseSprite = true;
		Position = base.CurrentTarget;
		ClearTrailHistory();
		_snowProjectile = null;
	}

	public override void DisposeOrb()
	{
		if (_snowProjectile != null)
		{
			_snowProjectile.SilentKill();
		}
		base.DisposeOrb();
	}

	public override void ChangeRoom()
	{
		if (_snowProjectile != null)
		{
			FinishAttack();
		}
		base.ChangeRoom();
	}
}
