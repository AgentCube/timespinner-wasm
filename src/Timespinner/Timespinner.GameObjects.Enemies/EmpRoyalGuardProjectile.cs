using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.StatusEffects;

namespace Timespinner.GameObjects.Enemies;

internal sealed class EmpRoyalGuardProjectile : DamageArea
{
	private const int BboxWidth = 48;

	private const int BboxHeight = 48;

	private const int PieceCount = 4;

	private const int SpirographTicks = 24;

	private const float SpirographRotationIncrement = 4f;

	private const float SpiroGraphRotationSpeed = 5f;

	private const float PieceRotationOffset = (float)Math.PI / 2f;

	private const float TopRateOfRotation = 2.5f;

	private const float ReverseRateOfRotation = 1.25f;

	private const float MaxLife = 3f;

	private const float BackGlowColorMultiplier = 0.1f;

	private const float TimeBetweenUpdatingTrackingTarget = 0.25f;

	private const float FollowVelocityMagnitude = 100f;

	private static readonly Vector4 GradientColor1 = new Vector4(0.4f, 0.3f, 0.4f, 1f);

	private static readonly Vector4 GradientColor2 = new Vector4(0.6f, 0.3f, 0.3f, 1f);

	private static readonly Color LightEvilDrawColor = new Color(0.3f, 0.05f, 0.4f, 0.15f);

	private static readonly Color DarkEvilDrawColor = new Color(0.3f, 0.05f, 0.1f, 0.15f);

	private static readonly Color BehindDrawColor = Color.White * 0.5f;

	private readonly GlowTexture _backGlowTexture;

	private readonly HaloRingAnimation _startingRingAnimation;

	private readonly Appendage[] _behindGlowAppendages = new Appendage[4];

	private readonly Appendage[] _darkEvilAppendages = new Appendage[4];

	private readonly Appendage[] _lightEvilAppendages = new Appendage[4];

	private float _fadePercentage;

	private float _spirographTimer;

	private float _spirographRotation;

	private float _spirographGradientPercentage;

	private float _evilRotation;

	private float _reverseRotation;

	private float _followUpdateTimer;

	private Vector2 _startingVelocity;

	private Vector2 _targetVelocity;

	public EmpRoyalGuardProjectile(Level inLevel, Point inPosition, Vector2 iV, ETeamSide inSide, SpriteSheet sprite, int baseDamage)
		: base(inLevel, inPosition, inSide, -1, null)
	{
		_initialVector = iV;
		_sprite = sprite;
		_bbox = new Rectangle(inPosition.X - 24, inPosition.Y - 24, 48, 48);
		_bboxOffset = new Point((iV.X < 0f) ? (-21) : (-19), 8);
		DrawOrigin = new Vector2(4f, 32f);
		_power = (int)Math.Ceiling((float)baseDamage * 1.25f);
		_force = 0;
		_life = 3f;
		base.DamageTimeoutTime = 1f;
		_doAppendagesInheritDrawColor = false;
		_doAppendagesMatchImageFacing = false;
		_doesUseAppendageCollision = false;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_isFlying = true;
		base.DoesDieOnImpact = false;
		base.DoesCollideWithTiles = false;
		base.DoesKnockBack = true;
		ChangeAnimation(31);
		_startingRingAnimation = new HaloRingAnimation(_level)
		{
			Diameter = 128,
			Center = Position
		};
		for (int i = 0; i < 4; i++)
		{
			Appendage appendage = new Appendage(this, new Point(28, 28), Point.Zero, _level, _sprite)
			{
				FollowType = EAppendageFollowType.AnchorLocked,
				AnchorOffset = new Point(-14, 0),
				DrawColor = BehindDrawColor,
				DrawOrigin = new Vector2(28f, 28f),
				Rotation = (float)i * ((float)Math.PI / 2f)
			};
			appendage.ChangeAnimation(29);
			_behindGlowAppendages[i] = appendage;
		}
		_backGlowTexture = new GlowTexture(_level)
		{
			GlowColorMultiplier = 0.1f
		};
		Point anchorOffset = new Point(-32, -32);
		for (int j = 0; j < 4; j++)
		{
			Appendage appendage2 = new Appendage(this, new Point(1, 1), Point.Zero, _level, _sprite)
			{
				DrawPriority = 1,
				DrawOrigin = new Vector2(32f, 32f),
				DrawColor = LightEvilDrawColor,
				FollowType = EAppendageFollowType.AnchorLocked,
				AnchorOffset = anchorOffset
			};
			Appendage appendage3 = new Appendage(this, new Point(1, 1), Point.Zero, _level, _sprite)
			{
				DrawPriority = 0,
				DrawOrigin = new Vector2(32f, 32f),
				DrawColor = DarkEvilDrawColor,
				FollowType = EAppendageFollowType.AnchorLocked,
				AnchorOffset = anchorOffset
			};
			appendage2.ChangeAnimation(30);
			appendage3.ChangeAnimation(30);
			_darkEvilAppendages[j] = appendage3;
			_lightEvilAppendages[j] = appendage2;
		}
	}

	protected override void AddImpactAnimation(Alive target, Rectangle collisionRectangle)
	{
		base.AddImpactAnimation(target, collisionRectangle);
	}

	public override bool DetermineDamage(Alive target, Rectangle collidingBbox)
	{
		bool flag = base.DetermineDamage(target, collidingBbox);
		if (flag)
		{
			target.GiveStatusEffect(EStatusEffectType.Chaos, 0);
		}
		return flag;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (_isFading)
			{
				_fadePercentage = _fadeTimer / _timeToFade;
				Color drawColor = Color.White * 0.5f * (1f - _fadePercentage);
				Appendage[] behindGlowAppendages = _behindGlowAppendages;
				foreach (Appendage appendage in behindGlowAppendages)
				{
					appendage.DrawColor = drawColor;
				}
			}
			if (!_startingRingAnimation.IsFinished)
			{
				_startingRingAnimation.Update(delta);
			}
			_followUpdateTimer -= delta;
			if (_followUpdateTimer <= 0f)
			{
				Point nearestProtagonistPosition = _level.GetNearestProtagonistPosition(Position);
				Vector2 vector = new Vector2(nearestProtagonistPosition.X - Position.X, nearestProtagonistPosition.Y - Bbox.Center.Y);
				vector.Normalize();
				_targetVelocity = vector * 100f;
				_startingVelocity = _velocity;
				_followUpdateTimer = 0.25f;
			}
			float amount = 1f - _followUpdateTimer / 0.25f;
			_velocity = _startingVelocity.SineInterpolate(_targetVelocity, amount);
			UpdateSpirograph(delta);
			Appendage[] behindGlowAppendages2 = _behindGlowAppendages;
			foreach (Appendage appendage2 in behindGlowAppendages2)
			{
				appendage2.Update(delta);
			}
			_backGlowTexture.Center = Bbox.Center;
			_backGlowTexture.Update(delta);
			_evilRotation += 2.5f * delta;
			_reverseRotation -= 1.25f * delta;
			if (_evilRotation > (float)Math.PI * 2f)
			{
				_evilRotation -= (float)Math.PI * 2f;
			}
			if (_reverseRotation < 0f)
			{
				_reverseRotation += (float)Math.PI * 2f;
			}
			for (int k = 0; k < 4; k++)
			{
				float num = (float)k * ((float)Math.PI / 2f);
				_lightEvilAppendages[k].Rotation = _evilRotation + num;
				_darkEvilAppendages[k].Rotation = _reverseRotation + num;
				_lightEvilAppendages[k].Update(delta);
				_darkEvilAppendages[k].Update(delta);
				if (_isFading)
				{
					float num2 = 1f - _fadePercentage;
					_lightEvilAppendages[k].DrawColor = LightEvilDrawColor * num2;
					_darkEvilAppendages[k].DrawColor = DarkEvilDrawColor * num2;
				}
			}
		}
		base.Update(delta);
	}

	private void UpdateSpirograph(float delta)
	{
		_spirographRotation += delta * 5f;
		_spirographTimer += delta;
		if (_spirographTimer > (float)Math.PI * 2f)
		{
			_spirographTimer -= (float)Math.PI * 2f;
		}
		_spirographGradientPercentage = ((float)Math.Sin(_spirographTimer) + 1f) / 2f;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		Appendage[] behindGlowAppendages = _behindGlowAppendages;
		foreach (Appendage appendage in behindGlowAppendages)
		{
			appendage.Draw(spriteBatch);
		}
		_backGlowTexture.Draw(spriteBatch);
		DrawSpirograph(spriteBatch);
		Appendage[] darkEvilAppendages = _darkEvilAppendages;
		foreach (Appendage appendage2 in darkEvilAppendages)
		{
			appendage2.Draw(spriteBatch);
		}
		Appendage[] lightEvilAppendages = _lightEvilAppendages;
		foreach (Appendage appendage3 in lightEvilAppendages)
		{
			appendage3.Draw(spriteBatch);
		}
		if (!_startingRingAnimation.IsFinished)
		{
			_startingRingAnimation.Draw(spriteBatch);
		}
	}

	private void DrawSpirograph(SpriteBatch spriteBatch)
	{
		if (!_isFading)
		{
			spriteBatch.End();
			_level.GCM.EfSlidingGradient.Parameters["colorOne"].SetValue(GradientColor1);
			_level.GCM.EfSlidingGradient.Parameters["colorTwo"].SetValue(GradientColor2);
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, _level.GCM.EfSlidingGradient);
			base.DrawColor = new Color(_spirographGradientPercentage, 0f, 0f, 0f);
		}
		else
		{
			base.DrawColor = Color.Purple * 0.5f * (1f - _fadePercentage);
		}
		for (int i = 0; i < 24; i++)
		{
			base.Rotation = _spirographRotation + (float)i * 4f;
			base.Draw(spriteBatch);
		}
		if (!_isFading)
		{
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null);
		}
	}

	public void Reset(Point startPoint, Vector2 iV)
	{
		_isFading = false;
		_fadeTimer = 0f;
		_life = 3f;
		_spirographTimer = 0f;
		Position = startPoint;
		_initialVector = iV;
		base.Velocity = iV;
		_startingVelocity = iV;
		_targetVelocity = iV;
		_followUpdateTimer = 0f;
		SnapBboxToPosition();
		Appendage[] behindGlowAppendages = _behindGlowAppendages;
		foreach (Appendage appendage in behindGlowAppendages)
		{
			appendage.DrawColor = BehindDrawColor;
		}
		Appendage[] darkEvilAppendages = _darkEvilAppendages;
		foreach (Appendage appendage2 in darkEvilAppendages)
		{
			appendage2.DrawColor = DarkEvilDrawColor;
		}
		Appendage[] lightEvilAppendages = _lightEvilAppendages;
		foreach (Appendage appendage3 in lightEvilAppendages)
		{
			appendage3.DrawColor = LightEvilDrawColor;
		}
		_startingRingAnimation.Center = startPoint;
		_startingRingAnimation.Reset();
		Update(0f);
	}
}
