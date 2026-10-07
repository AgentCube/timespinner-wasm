using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Emperor;

internal sealed class EmperorBlueProjectile : Projectile
{
	private const int Anim_TextureStart = 36;

	private const int Anim_PulseStart = 32;

	private const int Anim_PulseLength = 4;

	private const float MaxLife = 0.5f;

	private const float TextureRotationMultiplier = 10f;

	private const float BulletSpeed = 275f;

	private static readonly Color BlueBaseColor = new Color(64, 96, 248, 128);

	private static readonly Color FireBaseColor = new Color(232, 128, 60, 128);

	private readonly bool _isFire;

	private readonly int _baseDamage;

	private readonly Color _baseColor;

	private readonly Appendage _textureAppendage;

	private readonly HaloRingAnimation _halo;

	internal bool IsFinished { get; private set; }

	public EmperorBlueProjectile(Level inLevel, int baseDamage, bool isFire)
		: base(inLevel, Point.Zero, Vector2.Zero, ETeamSide.Enemies, -1)
	{
		_isFire = isFire;
		_sprite = _level.GCM.SpOrbMeleeBarrier;
		_bboxOffset = new Point(2, 2);
		_bbox = new Rectangle(0, 0, 16, 16);
		DrawOrigin = new Vector2(8f, 8f);
		ChangeAnimation(32, 4, 0.06f, EAnimationType.Cycle);
		_textureAppendage = new Appendage(this, new Point(16, 16), new Point(2, 2), _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorObject = this,
			AnchorOffset = new Point(0, 8),
			DrawOrigin = new Vector2(10f, 10f)
		};
		_textureAppendage.ChangeAnimation(36);
		_appendages.Add(_textureAppendage);
		_doAppendagesInheritDrawColor = false;
		_baseDamage = (int)Math.Ceiling((_isFire ? 1f : 1f) * (float)baseDamage);
		_power = _baseDamage;
		_force = 0;
		_life = 0.5f;
		_damageElement = (_isFire ? EDamageElement.Fire : EDamageElement.Aura);
		base.DoesKnockBack = true;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_maxMoveSpeed = 5000f;
		_maxFallSpeed = 5000f;
		_isFlying = true;
		_doesDieOutsideOfVisibleArea = true;
		base.DoesDieOnImpact = false;
		base.DoesCollideWithTiles = false;
		_doesDrawTrail = true;
		_trailFadeRate = 1.35f;
		_normalTrailLength = 2;
		_trailLength = _normalTrailLength;
		_trailShrinkRate = 0.2f;
		_baseColor = (_isFire ? FireBaseColor : BlueBaseColor);
		Color baseDrawColor = (_isFire ? new Color(255, 200, 160, 224) : new Color(160, 200, 255, 224));
		_halo = new HaloRingAnimation(_level)
		{
			BaseDrawColor = baseDrawColor,
			Diameter = 32
		};
		IsFinished = true;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			base.DrawColor = _baseColor;
			_textureAppendage.DrawColor = _baseColor;
			_textureAppendage.Rotation += delta * 10f;
			if (_textureAppendage.Rotation >= (float)Math.PI * 2f)
			{
				_textureAppendage.Rotation -= (float)Math.PI * 2f;
			}
			_halo.Update(delta);
		}
		base.Update(delta);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);
		if (!_halo.IsFinished)
		{
			_halo.Draw(spriteBatch);
		}
	}

	public override void SilentKill()
	{
		IsFinished = true;
		base.SilentKill();
	}

	internal void Reset(Point position, bool isFacingLeft)
	{
		Position = position;
		_initialVector = new Vector2(275f * (float)((!isFacingLeft) ? 1 : (-1)), 0f);
		_velocity = _initialVector;
		SnapBboxToPosition();
		base.ID = -1;
		_power = _baseDamage;
		_isFading = false;
		_life = 1.5f;
		IsFinished = false;
		base.DoesDieOnImpact = false;
		_halo.Center = position;
		_halo.Reset();
		ClearTrailHistory();
	}
}
