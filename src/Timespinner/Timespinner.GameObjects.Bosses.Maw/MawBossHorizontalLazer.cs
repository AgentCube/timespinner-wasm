using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Maw;

public sealed class MawBossHorizontalLazer : DamageArea
{
	private const int Height = 66;

	private const float TimeBetweenFlowParticleEmission = 0.066f;

	public const float MaxLife = 1.25f;

	private readonly int _originalPower;

	private readonly Vector2 _streamVector;

	private readonly ParticleSystem _streamParticles;

	private readonly PlasmaEmissionParticleSystem _emissionParticles;

	private float _lastLifePercentage;

	private float _flowEmissionTimer;

	private float _currentMaxLife;

	internal bool IsClosing { get; private set; }

	internal bool WasClosing { get; private set; }

	public MawBossHorizontalLazer(Level inLevel, Point inPosition, Vector2 inIV, ETeamSide inSide, Mobile inAnchor, Point anchorOffset, SpriteSheet sprite, int baseDamage)
		: base(inLevel, inPosition, inSide, -1, inAnchor)
	{
		base.AnchorOffset = anchorOffset;
		IsFacingLeft = inIV.X < 0f;
		_streamVector = inIV;
		_sprite = sprite;
		_doesDrawSpriteAndAppendages = false;
		base.DamageDimensions = new Point(560, 66);
		SnapBboxToPosition();
		base.DoesDrawAura = true;
		base.AuraColor = Color.White * 0.9f;
		_auraCount = 4f;
		base.AuraSize = 1.25f;
		base.AuraFrequency = 9f;
		base.DamageTimeoutTime = 0.2f;
		_originalPower = (int)Math.Ceiling((float)baseDamage * 1.2f);
		_currentMaxLife = 1.25f;
		_power = _originalPower;
		_force = 4;
		_life = _currentMaxLife;
		_canDamageThings = false;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_isFlying = true;
		_doesCollideWithSlopes = false;
		_doesDieOnTiles = false;
		_doesAutomaticallyEmitParticles = false;
		_streamParticles = new LunaisLazerStreamParticleSystem(_level.GCM.TxParticleEnergy, 10)
		{
			BaseColor = new Vector4(0.5f, 0.9f, 0.75f, 0.9f)
		};
		_particleSystems.Add(_streamParticles);
		_emissionParticles = new PlasmaEmissionParticleSystem(_level.GCM.SpOrbPlasma, 5, base.DamageDimensions, (!IsFacingLeft) ? EDirection.East : EDirection.West);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			float num = _life / _currentMaxLife;
			Vector2 where = new Vector2(IsFacingLeft ? Bbox.Right : Bbox.Left, Bbox.Center.Y);
			WasClosing = IsClosing;
			IsClosing = false;
			if (num < 0.1f)
			{
				IsClosing = true;
				_power = 0;
				_bbox.Height = (int)(MathHelper.SmoothStep(0f, 1f, num / 0.1f) * (float)_damageDimensions.Y);
				_streamParticles.KillOffParticles(0.1f);
				_emissionParticles.KillOffParticles(0.05f);
				if (_lastLifePercentage > 0f && num < 0f)
				{
					_level.PlayCue(ESFX.BossRoboKittyLazerEnd, Position);
				}
			}
			else if (_life > 0.9f)
			{
				float num2 = (1f - num) / 0.1f;
				_bbox.Height = (int)(MathHelper.SmoothStep(0f, 1f, num2) * (float)_damageDimensions.Y);
				if (num2 > 0.5f)
				{
					_streamParticles.AddParticles(where, _streamVector);
					EmitFlowParticle(where, delta);
				}
			}
			else
			{
				_streamParticles.AddParticles(where, _streamVector);
				EmitFlowParticle(where, delta);
				_bbox.Height = _damageDimensions.Y;
			}
			if (num <= 0.95f && !_canDamageThings)
			{
				_canDamageThings = true;
			}
			_lastLifePercentage = num;
		}
		base.Update(delta);
	}

	private void EmitFlowParticle(Vector2 where, float delta)
	{
		_flowEmissionTimer += delta;
		if (_flowEmissionTimer >= 0.066f)
		{
			_emissionParticles.AddParticles(where);
			_flowEmissionTimer = 0f;
		}
	}

	public override void SnapBboxToPosition()
	{
		_bbox.Location = new Point(_position.X - (IsFacingLeft ? _bbox.Width : 0) + base.AnchorOffset.X, _position.Y - _bbox.Height / 2 + base.AnchorOffset.Y);
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
		Rectangle frameSource = _sprite.GetFrameSource(26);
		spriteBatch.Draw(_sprite.Texture, new Rectangle((int)(_level.LevelRenderCenter.X - (_level.CameraPosition.X - (float)drawRect.X - 16f)), (int)(_level.LevelRenderCenter.Y - (_level.CameraPosition.Y - (float)drawRect.Y)), drawRect.Width - 32, drawRect.Height), frameSource, lazerColor);
		frameSource = _sprite.GetFrameSource(25);
		spriteBatch.Draw(_sprite.Texture, new Rectangle((int)(_level.LevelRenderCenter.X - (_level.CameraPosition.X - (float)drawRect.X - (float)(IsFacingLeft ? (drawRect.Width - 16) : 0))), (int)(_level.LevelRenderCenter.Y - (_level.CameraPosition.Y - (float)drawRect.Y)), 16, drawRect.Height), frameSource, lazerColor, 0f, Vector2.Zero, IsFacingLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
	}

	internal void Reset(float lifetime)
	{
		_life = lifetime;
		_currentMaxLife = lifetime;
		_isFading = false;
		_fadeTimer = 0f;
		_lastLifePercentage = 0f;
		_flowEmissionTimer = 0f;
		_power = _originalPower;
		IsClosing = false;
		WasClosing = false;
	}
}
