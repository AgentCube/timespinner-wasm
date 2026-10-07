using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions.Base;

public abstract class AnimatedParticleSystem : ParticleSystem
{
	private readonly SpriteSheet _sprite;

	protected EAnimationType _animationType;

	protected bool _isStartIndexRandom;

	protected int _animationStart;

	protected int _animationLength;

	protected float _animationSpeed;

	protected Vector2 _spriteOrigin;

	public SpriteSheet Sprite => _sprite;

	protected AnimatedParticleSystem(SpriteSheet inSprite, int howManyEffects, int animationStart, int animationLength, float speed, EAnimationType type)
		: base(inSprite.Texture, howManyEffects)
	{
		_sprite = inSprite;
		Point point = _sprite.FrameSize;
		if (animationStart >= 0 && animationStart < _sprite.FrameCount)
		{
			Rectangle frameSource = _sprite.GetFrameSource(animationStart);
			point = new Point(frameSource.Width, frameSource.Height);
		}
		_spriteOrigin = new Vector2((float)point.X / 2f, (float)point.Y / 2f);
		_animationStart = animationStart;
		_animationLength = animationLength;
		_animationSpeed = speed;
		_animationType = type;
	}

	protected override void Initialize()
	{
		_isAnimatedParticleSystem = true;
		base.Initialize();
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		if (p is AnimatedParticle animatedParticle)
		{
			animatedParticle.Sprite = _sprite;
			animatedParticle.AnimationSpeed = _animationSpeed;
			animatedParticle.AnimationStart = _animationStart;
			animatedParticle.AnimationLength = _animationLength;
			animatedParticle.AnimationType = _animationType;
			if (_isStartIndexRandom && _animationLength > 1)
			{
				animatedParticle.AnimationIndex = (int)Math.Round(ParticleSystem.RandomBetween(0f, _animationLength - 1));
			}
		}
		base.InitializeParticle(p, where);
	}

	public override void Draw(SpriteBatch spriteBatch, Vector2 screenCenter, Vector2 cameraPosition, float cameraZoom)
	{
		Particle[] particles = _particles;
		foreach (Particle particle in particles)
		{
			if (particle.IsActive && particle is AnimatedParticle animatedParticle)
			{
				float num = particle.TimeSinceStart / particle.Lifetime;
				float num2 = (base.DoParticleUseLifeForAlpha ? (4f * num * (1f - num)) : 1f);
				Vector4 baseColor = particle.BaseColor;
				baseColor.W *= particle.Brightness;
				Color color = new Color(baseColor) * num2 * baseColor.W;
				SpriteEffects spriteEffects = (animatedParticle.IsFacingLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
				if (animatedParticle.IsFlippedVertically)
				{
					spriteEffects |= SpriteEffects.FlipVertically;
				}
				Vector2 value = Vector2.Subtract(cameraPosition, new Vector2(particle.Position.X, particle.Position.Y));
				value = Vector2.Multiply(value, cameraZoom);
				spriteBatch.Draw(_texture, Vector2.Subtract(screenCenter, value), animatedParticle.FrameSource, color, particle.Rotation, _spriteOrigin, cameraZoom * particle.Scale, spriteEffects, 0f);
			}
		}
	}
}
