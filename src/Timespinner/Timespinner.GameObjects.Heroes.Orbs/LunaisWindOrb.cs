using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.LunaisProjectiles;

namespace Timespinner.GameObjects.Heroes.Orbs;

internal sealed class LunaisWindOrb : LunaisOrb
{
	private const int OrbTrailLength = 75;

	private const float TimeForDamageToAppear = 0.033f;

	private const float ThrowTimeToAttack = 0.07f;

	private const float ThrowTimeToReturn = 0.1f;

	private const float ThrowRadius = 32f;

	private static readonly Point OrbBboxOffset = new Point(1, 1);

	private static readonly Point OrbBboxDimensions = new Point(8, 8);

	private static readonly Vector2 OrbDrawOrigin = new Vector2(5f, 5f);

	private WindOrbMeleeDamageArea _damageArea;

	public static EInventoryOrbType DefaultOrbColor => EInventoryOrbType.Wind;

	public override Point AnchorPosition => Position;

	public LunaisWindOrb(Level inLevel, LunaisObj parentLunais, Point inPosition, SpriteSheet inSprite, bool isFrontPlane)
		: base(inLevel, parentLunais, inPosition, inSprite, DefaultOrbColor, EOrbState.Idle, isFrontPlane)
	{
		_trailColor = base.IdleTrailColor;
		_trailLength = 75;
		_doesDrawBrushTrail = true;
		_bbox = new Rectangle(0, 0, OrbBboxDimensions.X, OrbBboxDimensions.Y);
		_bboxOffset = OrbBboxOffset;
		DrawOrigin = OrbDrawOrigin;
		ChangeAnimation(6, 10, 0.05f, EAnimationType.Cycle);
		Update(0f);
	}

	public override void StartMeleeAttack(int whichAttack)
	{
		base.StartMeleeAttack(whichAttack);
		base.State = EOrbState.Melee;
		_level.PlayCue(ESFX.LunaisOrbSlashWind, Position);
	}

	protected override void UpdateMeleeAttack(float delta)
	{
		UpdateThrowAttack(delta, 0.07f, 0.1f, UpdateThrow);
	}

	private Vector2 UpdateThrow(float delta)
	{
		float currentThrowTime = _currentThrowTime;
		_currentThrowTime += delta;
		if (_currentThrowTime > 0.17f)
		{
			base.State = EOrbState.Idle;
			return _baseOrbitPosition.ToVector2();
		}
		if (_currentThrowTime >= 0.033f && currentThrowTime < 0.033f)
		{
			_damageArea = new WindOrbMeleeDamageArea(_level, Bbox.Center, ETeamSide.Heroes, this, base.IsThrowingLeft, base.OrbDamage, this);
			_level.AddProjectile(_damageArea);
		}
		if (_currentThrowTime >= 0.07f && currentThrowTime < 0.07f)
		{
			base.IsAtAttackApex = true;
			if (_damageArea != null)
			{
				_damageArea.ReleaseDamageArea();
			}
		}
		float num = ((_currentThrowTime <= 0.07f) ? (_currentThrowTime / 0.14f) : (0.5f + (_currentThrowTime - 0.07f) / 0.2f));
		float num2 = 32f * (float)Math.Sin((double)num * Math.PI);
		_orbXShift = (float)((!base.IsThrowingLeft) ? 1 : (-1)) * num2;
		return new Vector2((int)Math.Round((float)base.CurrentTarget.X + _orbXShift), base.CurrentTarget.Y);
	}
}
