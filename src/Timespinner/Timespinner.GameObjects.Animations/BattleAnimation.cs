using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Animations;

public class BattleAnimation
{
	private const float DefaultAnimationSpeed = 0.05f;

	private readonly SpriteSheet _sprite;

	private readonly Level _level;

	private bool _hasInitialized;

	private bool _isFrozen;

	protected bool _isDoneAnimating;

	private int _previousAnimationIndex = -1;

	private float _animationCounter;

	private float _delayTimer;

	private Vector2 _drawOrigin;

	private Color _baseDrawColor;

	private Color _effectiveDrawColor;

	private Rectangle _frameSource;

	public EDrawPlane DrawPlane { get; set; }

	public ETeamSide TeamSide { get; set; }

	public bool IsDead { get; set; }

	public bool IsFacingLeft { get; set; }

	public bool IsRenderedBehind { get; set; }

	public bool IsBelowParent { get; set; }

	public bool DoesRepeat { get; set; }

	public bool IsAnimationInReverse { get; set; }

	internal bool DoesDrawParticles { get; set; }

	internal bool DoesFadeOut { get; set; }

	internal bool IsFlippedVertically { get; set; }

	public int AnimationIndex { get; set; }

	public int AnimationStart { get; set; }

	public int AnimationLength { get; set; }

	public float AnimationSpeed { get; set; }

	public float InitialDelay
	{
		set
		{
			_delayTimer = value;
		}
	}

	public float Rotation { get; set; }

	internal float Scale { get; set; }

	public Point Position { get; set; }

	public Point AnchorOffset { get; set; }

	public Color DrawColor
	{
		get
		{
			return _baseDrawColor;
		}
		set
		{
			_baseDrawColor = value;
		}
	}

	internal Rectangle FrameSource => _frameSource;

	public GameObject AnchorObject { get; set; }

	public ParticleSystem ParticleSystem { get; set; }

	public Level Level => _level;

	internal virtual bool IsFinished
	{
		get
		{
			if (ParticleSystem != null)
			{
				if (_isDoneAnimating)
				{
					return ParticleSystem.AreParticlesDone;
				}
				return false;
			}
			return _isDoneAnimating;
		}
	}

	public BattleAnimation(SpriteSheet inSprite, Point inPosition, Level inLevel)
	{
		_sprite = inSprite;
		_level = inLevel;
		Position = inPosition;
		TeamSide = ETeamSide.Neutral;
		AnimationLength = ((_sprite != null) ? _sprite.FrameCount : 0);
		AnimationSpeed = 0.05f;
		_baseDrawColor = Color.White;
		Scale = 1f;
		DoesDrawParticles = true;
	}

	internal void Initialize()
	{
		_hasInitialized = true;
		if (_sprite == null)
		{
			_isDoneAnimating = true;
		}
		else
		{
			_frameSource = _sprite.GetFrameSource(AnimationStart);
			bool flag = Rotation == 0f;
			float num = (float)_frameSource.Width / 2f;
			float num2 = (float)_frameSource.Height / 2f;
			_drawOrigin = new Vector2(flag ? ((float)(int)Math.Ceiling(num)) : num, flag ? ((float)(int)Math.Ceiling(num2)) : num2);
			if (IsAnimationInReverse)
			{
				AnimationIndex = AnimationLength - 1;
			}
		}
		if (ParticleSystem != null)
		{
			ParticleSystem.AddParticles(Position.ToVector2());
		}
		Update(0f);
	}

	public virtual void Update(float delta)
	{
		if (!_hasInitialized)
		{
			Initialize();
		}
		if (_isFrozen)
		{
			return;
		}
		if (AnchorObject != null)
		{
			Position = new Point(AnchorObject.Position.X + AnchorOffset.X, AnchorObject.Position.Y + AnchorOffset.Y);
		}
		if (_delayTimer > 0f)
		{
			_delayTimer -= delta;
			if (_delayTimer >= 0f)
			{
				return;
			}
			_delayTimer = 0f;
		}
		if (!DoesFadeOut || AnimationIndex == 0)
		{
			_effectiveDrawColor = _baseDrawColor;
		}
		if (!_isDoneAnimating)
		{
			_animationCounter += delta;
			if (_animationCounter >= AnimationSpeed)
			{
				_animationCounter = ((AnimationSpeed < delta) ? 0f : (_animationCounter - AnimationSpeed));
				AnimationIndex += ((!IsAnimationInReverse) ? 1 : (-1));
				if (!IsAnimationInReverse && AnimationIndex >= AnimationLength)
				{
					if (!DoesRepeat)
					{
						_isDoneAnimating = true;
					}
					else
					{
						AnimationIndex = 0;
					}
				}
				else if (IsAnimationInReverse && AnimationIndex < 0)
				{
					if (!DoesRepeat)
					{
						_isDoneAnimating = true;
					}
					else
					{
						AnimationIndex = AnimationLength - 1;
					}
				}
				if (DoesFadeOut && AnimationLength > 0)
				{
					float num = (float)AnimationIndex / (float)AnimationLength;
					float num2 = (float)Math.Sin(num * ((float)Math.PI / 2f));
					_effectiveDrawColor = _baseDrawColor * (1f - num2);
				}
			}
		}
		if (ParticleSystem != null)
		{
			ParticleSystem.Update(delta);
		}
		if (IsFinished)
		{
			Kill();
		}
	}

	public virtual void Draw(SpriteBatch spriteBatch)
	{
		if (!_hasInitialized)
		{
			Initialize();
		}
		if (_delayTimer > 0f)
		{
			return;
		}
		if (!_isDoneAnimating)
		{
			if (_previousAnimationIndex != AnimationIndex)
			{
				_frameSource = _sprite.GetFrameSource(AnimationIndex + AnimationStart);
			}
			_previousAnimationIndex = AnimationIndex;
			Vector2 value = Vector2.Subtract(_level.CameraPosition, Position.ToVector2());
			value = Vector2.Multiply(value, _level.CameraZoom);
			SpriteEffects spriteEffects = ((!IsFacingLeft) ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
			if (IsFlippedVertically)
			{
				spriteEffects |= SpriteEffects.FlipVertically;
			}
			spriteBatch.Draw(_sprite.Texture, Vector2.Subtract(_level.LevelRenderCenter, value), _frameSource, _effectiveDrawColor, Rotation, _drawOrigin, Scale, spriteEffects, 0f);
		}
		if (ParticleSystem != null && DoesDrawParticles)
		{
			ParticleSystem.Draw(spriteBatch, _level.LevelRenderCenter, _level.CameraPosition, _level.CameraZoom);
		}
	}

	internal void RefreshFacing(EBattleAnimationType animationType, bool isFacingRight)
	{
		switch (animationType)
		{
		case EBattleAnimationType.GlassShatter:
			if (ParticleSystem is GlassShatterParticleSystem glassShatterParticleSystem)
			{
				glassShatterParticleSystem.IsFacingLeft = isFacingRight;
			}
			break;
		case EBattleAnimationType.SmallHit:
		case EBattleAnimationType.SmallGrayHit:
		case EBattleAnimationType.BlueOrbHit:
		case EBattleAnimationType.MediumHitBlue:
			if (ParticleSystem is LunaisOrbHitParticleSystem lunaisOrbHitParticleSystem)
			{
				lunaisOrbHitParticleSystem.IsFacingLeft = !isFacingRight;
			}
			break;
		case EBattleAnimationType.MediumHit:
		case EBattleAnimationType.BigHit:
		case EBattleAnimationType.SmallFail:
			break;
		}
	}

	public void Kill()
	{
		IsDead = true;
		_level.RemoveAnimation(this);
	}

	public void Freeze()
	{
		_isFrozen = true;
	}

	public void Unfreeze()
	{
		_isFrozen = false;
	}

	internal static BattleAnimation Create(EBattleAnimationType animationType, Point position, ETeamSide teamSide, bool imageFacingRight, Level level, bool doesPlaySFX)
	{
		return Create(animationType, position, teamSide, imageFacingRight, level, doesPlaySFX, EElementAnimationColor.None);
	}

	internal static BattleAnimation Create(EBattleAnimationType animationType, Point position, ETeamSide teamSide, bool isFacingLeft, Level level, bool doesPlaySFX, EElementAnimationColor color)
	{
		GCM gCM = level.GCM;
		int iD = level.ID;
		bool flag = false;
		SpriteSheet inSprite = null;
		ParticleSystem particleSystem = null;
		float animationSpeed = 0.06f;
		int animationStart = 0;
		int animationLength = 0;
		float rotation = 0f;
		Color elementColor = GetElementColor(color);
		switch (animationType)
		{
		case EBattleAnimationType.Boom:
			inSprite = gCM.SpBoomAnimation;
			animationLength = 13;
			particleSystem = new SmallShrapnelParticleSystem(gCM.SpAnimatedParticlesSmall, 1);
			animationSpeed = 0.05f;
			break;
		case EBattleAnimationType.SmallBoom:
			inSprite = gCM.SpEffectsSmall;
			animationStart = 98;
			animationLength = 8;
			animationSpeed = 0.05f;
			break;
		case EBattleAnimationType.DustBoom:
			particleSystem = new DustBoomParticleSystem(gCM.TxParticleDust, 1);
			break;
		case EBattleAnimationType.AuraExplosion:
			inSprite = gCM.SpAuraEffects;
			animationStart = 0;
			animationLength = 12;
			animationSpeed = 0.05f;
			break;
		case EBattleAnimationType.Shrapnel:
			particleSystem = new SmallShrapnelParticleSystem(gCM.SpAnimatedParticlesSmall, 1);
			break;
		case EBattleAnimationType.ExtinguishSmoke:
			particleSystem = new ExtinguishSmokeParticleSystem(gCM.TxParticleDust, 1);
			break;
		case EBattleAnimationType.GlassShatter:
			particleSystem = new GlassShatterParticleSystem(gCM.TxParticleEnergy, 1);
			break;
		case EBattleAnimationType.Poof:
			inSprite = gCM.SpBoomAnimation;
			animationStart = 20;
			animationLength = 7;
			animationSpeed = 0.05f;
			break;
		case EBattleAnimationType.SmallHit:
			inSprite = gCM.SpEffectsSmall;
			animationStart = 14;
			animationLength = 3;
			particleSystem = new LunaisOrbHitParticleSystem(gCM.TxParticleEnergy, 1);
			break;
		case EBattleAnimationType.SmallGrayHit:
			inSprite = gCM.SpEffectsSmall;
			animationStart = 125;
			animationLength = 3;
			particleSystem = new LunaisOrbHitParticleSystem(gCM.TxParticleEnergy, 1);
			break;
		case EBattleAnimationType.MediumHit:
			inSprite = gCM.SpEffectsSmall;
			animationStart = 21;
			animationLength = 3;
			animationSpeed = 0.035f;
			break;
		case EBattleAnimationType.BlueOrbHit:
			inSprite = gCM.SpEffectsSmall;
			animationStart = 21;
			animationLength = 3;
			animationSpeed = 0.035f;
			particleSystem = new LunaisOrbHitParticleSystem(gCM.TxParticleEnergy, 1);
			break;
		case EBattleAnimationType.BigHit:
			inSprite = gCM.SpEffectsMedium;
			animationStart = 0;
			animationLength = 4;
			animationSpeed = 0.03f;
			break;
		case EBattleAnimationType.MediumHitBlue:
			inSprite = gCM.SpEffectsMedium;
			animationStart = 11;
			animationLength = 4;
			animationSpeed = 0.03f;
			rotation = (float)level.NextRandomDouble();
			particleSystem = new LunaisOrbHitParticleSystem(gCM.TxParticleEnergy, 1);
			break;
		case EBattleAnimationType.MediumHitYellow:
			inSprite = gCM.SpEffectsMedium;
			animationStart = 15;
			animationLength = 4;
			animationSpeed = 0.03f;
			rotation = (float)level.NextRandomDouble();
			break;
		case EBattleAnimationType.SmallFail:
			inSprite = gCM.SpEffectsSmall;
			animationStart = 106;
			animationLength = 4;
			animationSpeed = 0.06f;
			break;
		case EBattleAnimationType.BigFlash:
			inSprite = gCM.SpEffectsMedium;
			animationStart = 6;
			animationLength = 5;
			animationSpeed = 0.06f;
			break;
		case EBattleAnimationType.BigRipple:
			inSprite = gCM.SpEffectsMedium;
			animationStart = 30;
			animationLength = 5;
			animationSpeed = 0.06f;
			break;
		case EBattleAnimationType.HPSparkles:
			particleSystem = new SparklesParticleSystem(gCM.SpEffectsSmall, 1, 46, 4);
			inSprite = gCM.SpEffectsSmall;
			animationStart = 88;
			animationLength = 5;
			break;
		case EBattleAnimationType.MPSparkles:
			particleSystem = new SparklesParticleSystem(gCM.SpEffectsSmall, 1, 55, 3);
			inSprite = gCM.SpEffectsSmall;
			animationStart = 93;
			animationLength = 5;
			break;
		case EBattleAnimationType.HPCreate:
			inSprite = gCM.SpEffectsSmall;
			animationStart = 88;
			animationLength = 5;
			break;
		case EBattleAnimationType.MPCreate:
			inSprite = gCM.SpEffectsSmall;
			animationStart = 93;
			animationLength = 5;
			break;
		case EBattleAnimationType.WaterSplash:
			particleSystem = new WaterSplashParticleSystem(gCM.TxParticleEnergy, 1);
			break;
		case EBattleAnimationType.AcidSplash:
			particleSystem = new WaterAcidSplashParticleSystem(gCM.TxParticleEnergy, 1);
			break;
		case EBattleAnimationType.Dust:
			particleSystem = new DustParticleSystem(gCM.TxParticleDust, 1, iD);
			break;
		case EBattleAnimationType.CrackingDust:
			particleSystem = new DustCrackingParticleSystem(gCM.TxParticleDust, 1, iD);
			break;
		case EBattleAnimationType.Pebbles:
			particleSystem = new PebblesParticleSystem(gCM.TxBlankSquare, 1, iD);
			break;
		case EBattleAnimationType.MediumRecoilDust:
			inSprite = gCM.SpEffectsMedium;
			animationStart = 25;
			animationLength = 5;
			animationSpeed = 0.06f;
			elementColor.A = 200;
			break;
		case EBattleAnimationType.OrbDisappear:
			inSprite = gCM.SpEffectsSmall;
			animationStart = 73;
			animationLength = 3;
			break;
		case EBattleAnimationType.WetSplashLarge:
			inSprite = gCM.SpEffectsLarge;
			animationStart = 11;
			animationLength = 7;
			animationSpeed = 0.05f;
			break;
		case EBattleAnimationType.WetSplashSmall:
			inSprite = gCM.SpBoomAnimation;
			animationStart = 13;
			animationLength = 7;
			animationSpeed = 0.05f;
			break;
		default:
			flag = true;
			break;
		}
		if (doesPlaySFX && level.IsWithinCameraDistance(position))
		{
			ESFX eSFXFromBattleAnimationType = GetESFXFromBattleAnimationType(animationType);
			if (eSFXFromBattleAnimationType != 0)
			{
				level.PlayCue(eSFXFromBattleAnimationType, position);
			}
		}
		if (particleSystem != null && color != 0)
		{
			particleSystem.BaseColor = elementColor.ToVector4();
		}
		if (!flag)
		{
			BattleAnimation battleAnimation = new BattleAnimation(inSprite, position, level);
			battleAnimation.ParticleSystem = particleSystem;
			battleAnimation.TeamSide = teamSide;
			battleAnimation.AnimationSpeed = animationSpeed;
			battleAnimation.IsFacingLeft = isFacingLeft;
			battleAnimation.DoesRepeat = false;
			battleAnimation.AnimationStart = animationStart;
			battleAnimation.AnimationLength = animationLength;
			battleAnimation.Rotation = rotation;
			battleAnimation.DrawColor = elementColor;
			return battleAnimation;
		}
		return null;
	}

	internal static ESFX GetESFXFromBattleAnimationType(EBattleAnimationType animationType)
	{
		ESFX result = ESFX.Default;
		switch (animationType)
		{
		case EBattleAnimationType.Boom:
			result = ESFX.FoleyExplosionLarge;
			break;
		case EBattleAnimationType.AuraExplosion:
			result = ESFX.EnemyExplodeHumanoid;
			break;
		case EBattleAnimationType.Poof:
			result = ESFX.FoleyExplosionPoof;
			break;
		case EBattleAnimationType.SmallHit:
			result = ESFX.FoleyHit;
			break;
		case EBattleAnimationType.SmallGrayHit:
			result = ESFX.FoleyHit;
			break;
		case EBattleAnimationType.MediumHit:
			result = ESFX.LunaisChargeImpact;
			break;
		case EBattleAnimationType.BlueOrbHit:
			result = ESFX.LunaisChargeImpact;
			break;
		case EBattleAnimationType.BigHit:
			result = ESFX.LunaisChargeImpact;
			break;
		case EBattleAnimationType.MediumHitBlue:
			result = ESFX.FoleyHit;
			break;
		case EBattleAnimationType.MediumHitYellow:
			result = ESFX.FoleyHit;
			break;
		case EBattleAnimationType.SmallFail:
			result = ESFX.EnemyShieldKnightBlock;
			break;
		case EBattleAnimationType.WetSplashLarge:
			result = ESFX.FoleyWetExplosionLarge;
			break;
		case EBattleAnimationType.WetSplashSmall:
			result = ESFX.FoleyWetExplosionSmall;
			break;
		}
		return result;
	}

	public static Color GetElementColor(EElementAnimationColor color)
	{
		Color result = Color.White;
		switch (color)
		{
		case EElementAnimationColor.Blue:
			result = new Color(0.8f, 0.9f, 1f);
			break;
		case EElementAnimationColor.Green:
			result = new Color(0.9f, 1f, 0.8f);
			break;
		case EElementAnimationColor.Red:
			result = new Color(1f, 0.8f, 0.6f);
			break;
		case EElementAnimationColor.Pink:
			result = new Color(1f, 0.6f, 0.8f);
			break;
		case EElementAnimationColor.Purple:
			result = new Color(0.8f, 0.6f, 1f);
			break;
		}
		return result;
	}

	public static Vector4 GetElementColorVector(EElementAnimationColor color)
	{
		return GetElementColor(color).ToVector4();
	}

	public static EElementAnimationColor GetElementFromOrbColor(EInventoryOrbType orb)
	{
		EElementAnimationColor result = EElementAnimationColor.None;
		switch (orb)
		{
		case EInventoryOrbType.Blue:
			result = EElementAnimationColor.Blue;
			break;
		case EInventoryOrbType.Blade:
			result = EElementAnimationColor.Green;
			break;
		case EInventoryOrbType.Flame:
			result = EElementAnimationColor.Red;
			break;
		case EInventoryOrbType.Pink:
			result = EElementAnimationColor.Pink;
			break;
		}
		return result;
	}

	public static Color GetElementColorFromOrb(EInventoryOrbType orb)
	{
		return GetElementColor(GetElementFromOrbColor(orb));
	}

	public static Vector4 GetElementColorVectorFromOrb(EInventoryOrbType orb)
	{
		return GetElementColorVector(GetElementFromOrbColor(orb));
	}

	public void Reset(Point position, bool isFacingLeft)
	{
		AnimationIndex = 0;
		_animationCounter = 0f;
		_isDoneAnimating = _sprite == null;
		IsDead = false;
		Position = position;
		IsFacingLeft = isFacingLeft;
		if (ParticleSystem != null)
		{
			ParticleSystem.AddParticles(Position.ToVector2());
		}
	}
}
