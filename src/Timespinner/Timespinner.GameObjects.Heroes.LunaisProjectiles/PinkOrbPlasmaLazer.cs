using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Lunais.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Heroes.LunaisProjectiles;

internal sealed class PinkOrbPlasmaLazer : LunaisBaseOrbDamageArea
{
	internal const int DamageWidthShort = 400;

	private const int DamageWidthWide = 500;

	internal const int DamageHeight = 32;

	internal const float MaxLife = 1.25f;

	private const float StartEndCutOffPercentage = 0.1f;

	private const float EmissionAnimationBaseAlpha = 0.85f;

	private readonly ParticleSystem _trailParticles;

	private readonly BattleAnimation _emissionAnimation;

	private float _particleEmissionTimer;

	public PinkOrbPlasmaLazer(Level inLevel, Point inPosition, Vector2 inIV, ETeamSide inSide, Mobile inAnchor, int baseOrbDamage, LunaisOrbAbility parentOrb, bool isWide)
		: base(inLevel, inPosition, inSide, -1, inAnchor, parentOrb)
	{
		IsFacingLeft = inIV.X < 0f;
		int x = (isWide ? 500 : 400);
		_sprite = _level.GCM.SpOrbPlasma;
		_doesDrawSpriteAndAppendages = false;
		base.DamageDimensions = new Point(x, 32);
		SnapBboxToPosition();
		base.DoesDrawAura = true;
		base.AuraColor = Color.White * 0.9f;
		_auraCount = 4f;
		base.AuraSize = 1.25f;
		base.AuraFrequency = 9f;
		_power = baseOrbDamage;
		_force = 4;
		_life = 1.25f;
		_canDamageThings = false;
		base.DamageTimeoutTime = 0.2f;
		_damageElement = EDamageElement.Plasma;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_isFlying = true;
		_doesCollideWithSlopes = false;
		_doesDieOnTiles = false;
		_trailParticles = new LunaisLazerStreamParticleSystem(_level.GCM.TxParticleEnergy, 10);
		_particleSystems.Add(_trailParticles);
		_doesAutomaticallyEmitParticles = false;
		_emissionAnimation = new BattleAnimation(_sprite, inPosition.Add(IsFacingLeft ? 1 : 0, 0), _level)
		{
			AnimationStart = 2,
			AnimationLength = 4,
			DoesRepeat = true,
			DrawColor = Color.White * 0f,
			IsFacingLeft = IsFacingLeft
		};
	}

	public override void Update(float delta)
	{
		float num = _life / 1.25f;
		if (num < 0.1f)
		{
			float num2 = num / 0.1f;
			_bbox.Height = (int)(MathHelper.SmoothStep(0f, 1f, num2) * (float)_damageDimensions.Y);
			_trailParticles.KillOffParticles(0.1f);
			_emissionAnimation.DrawColor = Color.White * num2 * 0.85f;
		}
		else if (_life > 0.9f)
		{
			float num3 = (1f - num) / 0.1f;
			_bbox.Height = (int)(MathHelper.SmoothStep(0f, 1f, num3) * (float)_damageDimensions.Y);
			if (num3 > 0.5f)
			{
				AddParticles(delta);
			}
			_emissionAnimation.DrawColor = Color.White * num3 * 0.85f;
		}
		else
		{
			AddParticles(delta);
			_bbox.Height = _damageDimensions.Y;
			_emissionAnimation.DrawColor = Color.White * 0.85f;
		}
		if (num <= 0.95f && !_canDamageThings)
		{
			_canDamageThings = true;
		}
		if (!base.IsFrozen)
		{
			_emissionAnimation.Update(delta);
		}
		base.Update(delta);
	}

	private void AddParticles(float delta)
	{
		_particleEmissionTimer -= delta;
		if (_particleEmissionTimer <= 0f)
		{
			_particleEmissionTimer += 1f / 45f;
			Vector2 where = Position.ToVector2();
			_trailParticles.AddParticles(where, new Vector2((!IsFacingLeft) ? 1 : (-1), 0f));
		}
	}

	public override void SnapBboxToPosition()
	{
		_bbox.Location = new Point(_position.X - (IsFacingLeft ? _bbox.Width : 0), _position.Y - _bbox.Height / 2);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		_emissionAnimation.Draw(spriteBatch);
		base.Draw(spriteBatch);
	}

	public override void DrawAura(SpriteBatch spriteBatch, SpriteEffects toFlip)
	{
		Color auraColor = base.AuraColor;
		Vector2 vector = new Vector2(_bbox.X, _bbox.Y);
		float num = 1f;
		auraColor *= 0.5f;
		for (int i = 0; (float)i < _auraCount; i++)
		{
			num += (float)((Math.Cos((double)base.AuraFrequency * ((double)_auraTimer + 0.4 * (double)i)) + 1.0) / 20.0);
			if (num > base.AuraSize)
			{
				num = 0.9f;
			}
			auraColor *= 0.9f;
			float num2 = (float)_bbox.Height * num - (float)_bbox.Height;
			DrawLazer(spriteBatch, auraColor, new Rectangle((int)vector.X, (int)(vector.Y - num2 / 2f), _bbox.Width, (int)((float)_bbox.Height + num2)));
		}
	}

	private void DrawLazer(SpriteBatch spriteBatch, Color lazerColor, Rectangle drawRect)
	{
		Rectangle frameSource = _sprite.GetFrameSource(1);
		spriteBatch.Draw(_sprite.Texture, new Rectangle((int)(_level.LevelRenderCenter.X - (_level.CameraPosition.X - (float)drawRect.X - 16f)), (int)(_level.LevelRenderCenter.Y - (_level.CameraPosition.Y - (float)drawRect.Y)), drawRect.Width - 32, drawRect.Height), frameSource, lazerColor);
		frameSource = _sprite.GetFrameSource(0);
		spriteBatch.Draw(_sprite.Texture, new Rectangle((int)(_level.LevelRenderCenter.X - (_level.CameraPosition.X - (float)drawRect.X - (float)(IsFacingLeft ? (drawRect.Width - 16) : 0))), (int)(_level.LevelRenderCenter.Y - (_level.CameraPosition.Y - (float)drawRect.Y)), 16, drawRect.Height), frameSource, lazerColor, 0f, Vector2.Zero, IsFacingLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
		spriteBatch.Draw(_sprite.Texture, new Rectangle((int)(_level.LevelRenderCenter.X - (_level.CameraPosition.X - (float)drawRect.X - (float)((!IsFacingLeft) ? (drawRect.Width - 16) : 0))), (int)(_level.LevelRenderCenter.Y - (_level.CameraPosition.Y - (float)drawRect.Y)), 16, drawRect.Height), frameSource, lazerColor, 0f, Vector2.Zero, (!IsFacingLeft) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
	}

	public void Reset(Point startPoint, Vector2 iV, LunaisObj parentLunais, int spellDamage)
	{
		_life = 1.25f;
		_power = spellDamage;
		_canDamageThings = false;
		_initialVector = iV;
		_isFading = false;
		_fadeTimer = 0f;
		base.ID = -1;
		IsFacingLeft = _initialVector.X < 0f;
		Position = startPoint;
		SnapBboxToPosition();
		_emissionAnimation.AnchorObject = parentLunais;
		_emissionAnimation.IsFacingLeft = IsFacingLeft;
		_emissionAnimation.AnchorOffset = new Point(20 * ((!IsFacingLeft) ? 1 : (-1)), -23);
	}

	internal void Cancel()
	{
		float num = 0.1f;
		if (_life > 0.9f)
		{
			num = 1f - _life / 1.25f;
		}
		if (_life > 0.1f)
		{
			_life = num * 1.25f;
		}
	}
}
