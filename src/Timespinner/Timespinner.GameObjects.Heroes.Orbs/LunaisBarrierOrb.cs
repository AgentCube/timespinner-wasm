using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Lunais.LunaisProjectiles;

namespace Timespinner.GameObjects.Heroes.Orbs;

internal sealed class LunaisBarrierOrb : LunaisOrb
{
	private const int OrbTrailLength = 75;

	private const float AttackCooldown = 1f;

	private static readonly Point OrbBboxOffset = new Point(1, 1);

	private static readonly Point OrbBboxDimensions = new Point(8, 8);

	private static readonly Vector2 OrbDrawOrigin = new Vector2(5f, 5f);

	private float _attackTimer;

	private BarrierOrbMeleeDamageArea _shockwaveDamageArea;

	public static EInventoryOrbType DefaultOrbColor => EInventoryOrbType.Barrier;

	public LunaisBarrierOrb(Level inLevel, LunaisObj parentLunais, Point inPosition, SpriteSheet inSprite, bool isFrontPlane)
		: base(inLevel, parentLunais, inPosition, inSprite, DefaultOrbColor, EOrbState.Idle, isFrontPlane)
	{
		_trailColor = base.IdleTrailColor;
		_trailLength = 75;
		_doesDrawBrushTrail = true;
		_bbox = new Rectangle(0, 0, OrbBboxDimensions.X, OrbBboxDimensions.Y);
		_bboxOffset = OrbBboxOffset;
		DrawOrigin = OrbDrawOrigin;
		ChangeAnimation(0, 10, 0.05f, EAnimationType.Cycle);
		Update(0f);
	}

	public override void StartMeleeAttack(int whichAttack)
	{
		base.StartMeleeAttack(whichAttack);
		base.State = EOrbState.Melee;
		_battleAnimations.Add(new BattleAnimation(_sprite, Bbox.Center, _level)
		{
			TeamSide = ETeamSide.Heroes,
			AnimationStart = 19,
			AnimationLength = 5,
			DrawColor = Color.White * 0.75f,
			IsFacingLeft = base.IsOrbFacingLeft
		});
		_shockwaveDamageArea = new BarrierOrbMeleeDamageArea(inPosition: new Point(Position.X + 4, Position.Y + 4), inLevel: _level, inSide: ETeamSide.Heroes, inDamage: base.OrbDamage, parentOrb: this, sprite: _sprite);
		_level.AddProjectile(_shockwaveDamageArea);
		PlayCue(ESFX.LunaisOrbRadiantMelee);
		_attackTimer = 0f;
	}

	protected override void UpdateMeleeAttack(float delta)
	{
		base.IsAtAttackApex = _attackTimer <= 0f;
		Position = _baseOrbitPosition;
		_attackTimer += delta;
		if (_attackTimer >= 1f)
		{
			FinishAttack();
		}
	}

	private void FinishAttack()
	{
		base.State = EOrbState.Idle;
		_shockwaveDamageArea = null;
	}

	public override void DisposeOrb()
	{
		if (_shockwaveDamageArea != null)
		{
			_shockwaveDamageArea.SilentKill();
		}
		base.DisposeOrb();
	}

	public override void ChangeRoom()
	{
		if (_shockwaveDamageArea != null)
		{
			FinishAttack();
		}
		base.ChangeRoom();
	}
}
