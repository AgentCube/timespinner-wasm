using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Relics;

internal abstract class RelicItemBase : GameEvent
{
	private const float GlowFrequency = 1f;

	private const float GlowAmplitude = 1f;

	private const float GlowOffset = 2f;

	private const float FloatHeight = 4f;

	private const float FloatFrequency = 1f;

	private const float TimeToFade = 0.5f;

	private static readonly Color BaseAuraColor = new Color(1f, 0.75f, 0.75f, 0.3f);

	private readonly Point _startingPoint;

	private readonly HaloRingAnimation _haloAnimation;

	private readonly TimespinnerSpindleLeakParticleSystem _tinySparkles;

	private float _glowDelta;

	private float _floatDelta;

	private float _fadeTimer;

	internal bool IsFading { get; private set; }

	internal float FadePercentage { get; private set; }

	internal RelicItemBase(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, SpriteSheet sprite)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_startingPoint = inPosition;
		_sprite = sprite;
		Bbox = new Rectangle(0, 0, 21, 21);
		_isRepeatedTrigger = false;
		_isAffectedByGravity = false;
		_isFlying = true;
		_isAffectedByTime = false;
		_isGlowing = true;
		_glowBase = 1f;
		_glowColor = new Color(1f, 0.9f, 0.9f, 0.5f);
		base.DoesDrawAura = true;
		base.AuraColor = BaseAuraColor;
		base.AuraSize = 0.05f;
		base.AuraOffset = new Vector2(1f, 1f);
		base.AuraFrequency = 8f;
		_tinySparkles = new TimespinnerSpindleLeakParticleSystem(_level.GCM.TxParticleEnergy, 10);
		_particleSystems.Add(_tinySparkles);
		_haloAnimation = new HaloRingAnimation(_level)
		{
			BaseDrawColor = new Color(0.8f, 0.8f, 0.9f, 0.8f),
			Diameter = 80
		};
	}

	public override void Update(float delta)
	{
		if (!IsFading)
		{
			_tinySparkles.AddParticles(Bbox.Center.ToVector2());
			_glowDelta += delta;
			if (_glowDelta > 614f)
			{
				_glowDelta -= 614f;
			}
			_glowBase = 2f + (float)Math.Sin((double)_glowDelta * Math.PI * 1.0) * 1f;
			_floatDelta += delta;
			if (_floatDelta > 614f)
			{
				_floatDelta -= 614f;
			}
			Position = _startingPoint.Add(0, (int)(Math.Sin((double)_floatDelta * Math.PI * 1.0) * 4.0));
		}
		else
		{
			_fadeTimer += delta;
			_isGlowing = false;
			if (_fadeTimer >= 0.5f)
			{
				SilentKill();
			}
			else
			{
				base.AuraColor = MathEx.SineInterpolate(amount: FadePercentage = _fadeTimer / 0.5f, start: BaseAuraColor, end: Color.Transparent);
				_haloAnimation.Center = Bbox.Center;
				_haloAnimation.Update(delta);
			}
		}
		base.Update(delta);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (IsFading)
		{
			_haloAnimation.Draw(spriteBatch);
		}
		base.Draw(spriteBatch);
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		if (!IsFading && !_isTriggered)
		{
			StartCutscene();
		}
		return base.TriggerEvent(who, depth);
	}

	private void StartCutscene()
	{
		OnPickedUp();
		_isGlowing = false;
		_doesDrawBaseSprite = false;
		IsFading = true;
		Update(0f);
	}

	internal virtual void OnPickedUp()
	{
	}
}
