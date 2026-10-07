using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.HUD;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Misc;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L00_Prologue;

internal sealed class EnvPrefabProDummy : EnvironmentPrefabBase
{
	private const int MaxHP = 21;

	private const int PushVelocity = 500;

	private static readonly Color FlashFrame1Color = new Color(1f, 1f, 0.8f, 0.65f);

	private static readonly Color FlashFrame2Color = new Color(1f, 0.3f, 0.1f, 0.8f);

	private bool _isDead;

	private int _hp;

	private int _flashFrame;

	private float _damageTimeoutTimer;

	internal bool CanBeDamaged { get; set; }

	internal bool IsDestroyed { get; private set; }

	public EnvPrefabProDummy(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		_sprite = _level.GCM.SpPlatforms;
		Bbox = new Rectangle(inPosition.X, inPosition.Y, 16, 16);
		ChangeAnimation(-1);
		IsFacingLeft = objectSpec == null || !objectSpec.IsFlippedHorizontally;
		_doAppendagesInheritDrawColor = true;
		_doAppendagesMatchImageFacing = true;
		base.DoesCollideWithProjectiles = true;
		base.DrawPlane = EDrawPlane.Back;
		base.CanBeTriggered = true;
		_hp = 21;
	}

	public override void Update(float delta)
	{
		base.Update(delta);
		if (base.IsGlowing && _flashFrame > -1)
		{
			base.IsGlowing = true;
			base.GlowBase = 1f;
			base.GlowColor = ((_flashFrame % 3 == 0) ? FlashFrame1Color : FlashFrame2Color);
			_flashFrame--;
			if (_flashFrame <= -1)
			{
				base.IsGlowing = false;
				base.DrawColor = Color.White;
			}
		}
		if (_damageTimeoutTimer > 0f)
		{
			_damageTimeoutTimer -= delta;
			if (_damageTimeoutTimer < 0f)
			{
				_damageTimeoutTimer = 0f;
			}
		}
		if (_hp <= 0 && !_isDead)
		{
			_isDead = true;
			_appendages.Clear();
		}
	}

	public override bool ProjectileTriggerEvent(Projectile projectile, Vector2 depth)
	{
		if (!base.IsFrozen && CanBeDamaged)
		{
			bool flag = false;
			Point point = projectile.Bbox.Center;
			if (_damageTimeoutTimer <= 0f && _hp > 0)
			{
				Rectangle collidingRectangle = GetCollidingRectangle(projectile);
				if (collidingRectangle != Rectangle.Empty)
				{
					flag = true;
					point = projectile.FindDeathPoint(this, collidingRectangle);
					base.IsGlowing = true;
					_flashFrame = 4;
					int effectiveDamage = projectile.EffectiveDamage;
					_level.AddNumber(effectiveDamage, point, ENumberColor.White);
					_hp -= effectiveDamage;
					_damageTimeoutTimer = projectile.DamageTimeout;
					PlayCue(ESFX.LunaisOrbImpact);
					if (_hp <= 0)
					{
						DoDeathSequence(projectile);
					}
				}
			}
			if (flag && projectile.DoesDieOnImpact)
			{
				projectile.Kill(useAnimation: true, point, deathFromInvulnerable: false);
			}
		}
		return true;
	}

	private void DoDeathSequence(Projectile projectile)
	{
		bool flag = _level.GetPlayerPosition().X < Position.X;
		PlayCue(ESFX.EnemyEngineerLogBreak);
		DebrisEvent.CreateFromObject(this, new Vector2(flag ? 500 : (-500), 0f), projectile.Bbox.Center, _sprite, DebrisEvent.EDebrisDeathType.Dust);
		IsDestroyed = true;
	}
}
