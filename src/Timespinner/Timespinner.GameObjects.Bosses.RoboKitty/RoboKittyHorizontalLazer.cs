using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.RoboKitty;

public sealed class RoboKittyHorizontalLazer : DamageArea
{
	public const float MaxLife = 1.25f;

	private readonly ParticleSystem _streamParticles;

	private float _lastLifePercentage;

	public RoboKittyHorizontalLazer(Level inLevel, Point inPosition, Vector2 inIV, ETeamSide inSide, Mobile inAnchor, Point anchorOffset, int baseDamage)
		: base(inLevel, inPosition, inSide, -1, inAnchor)
	{
		base.AnchorOffset = anchorOffset;
		IsFacingLeft = inIV.X < 0f;
		_sprite = _level.GCM.SpRoboKitty;
		_doesDrawSpriteAndAppendages = false;
		base.DamageDimensions = new Point(360, 16);
		SnapBboxToPosition();
		base.DoesDrawAura = true;
		base.AuraColor = new Color(0.25f, 1f, 0.4f) * 0.9f;
		_auraCount = 4f;
		base.AuraSize = 1.25f;
		base.AuraFrequency = 9f;
		base.DamageTimeoutTime = 0.2f;
		_power = (int)Math.Ceiling((float)baseDamage * 1.4f);
		_force = 4;
		_life = 1.25f;
		_canDamageThings = false;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_isFlying = true;
		_doesCollideWithSlopes = false;
		_doesDieOnTiles = false;
		_streamParticles = new LunaisLazerStreamParticleSystem(_level.GCM.TxParticleEnergy, 10)
		{
			BaseColor = new Vector4(0.5f, 0.9f, 0.75f, 0.9f)
		};
		_particleSystems.Add(_streamParticles);
		_doesAutomaticallyEmitParticles = false;
	}

	public override void Update(float delta)
	{
		float num = _life / 1.25f;
		Vector2 where = new Vector2(_anchorObject.Position.X + base.AnchorOffset.X, _anchorObject.Position.Y + base.AnchorOffset.Y - 4);
		if (num < 0.1f)
		{
			_power = 0;
			_bbox.Height = (int)(MathHelper.SmoothStep(0f, 1f, num / 0.1f) * (float)_damageDimensions.Y);
			_streamParticles.KillOffParticles(0.1f);
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
				_streamParticles.AddParticles(where, new Vector2((!IsFacingLeft) ? 1 : (-1), 0f));
			}
		}
		else
		{
			_streamParticles.AddParticles(where, new Vector2((!IsFacingLeft) ? 1 : (-1), 0f));
			_bbox.Height = _damageDimensions.Y;
		}
		if (num <= 0.95f && !_canDamageThings)
		{
			_canDamageThings = true;
		}
		base.Update(delta);
		_lastLifePercentage = num;
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
		Rectangle frameSource = _sprite.GetFrameSource(21);
		spriteBatch.Draw(_sprite.Texture, new Rectangle((int)(_level.LevelRenderCenter.X - (_level.CameraPosition.X - (float)drawRect.X - 16f)), (int)(_level.LevelRenderCenter.Y - (_level.CameraPosition.Y - (float)drawRect.Y)), drawRect.Width - 32, drawRect.Height), frameSource, lazerColor);
		frameSource = _sprite.GetFrameSource(20);
		spriteBatch.Draw(_sprite.Texture, new Rectangle((int)(_level.LevelRenderCenter.X - (_level.CameraPosition.X - (float)drawRect.X - (float)(IsFacingLeft ? (drawRect.Width - 16) : 0))), (int)(_level.LevelRenderCenter.Y - (_level.CameraPosition.Y - (float)drawRect.Y)), 16, drawRect.Height), frameSource, lazerColor, 0f, Vector2.Zero, IsFacingLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
		spriteBatch.Draw(_sprite.Texture, new Rectangle((int)(_level.LevelRenderCenter.X - (_level.CameraPosition.X - (float)drawRect.X - (float)((!IsFacingLeft) ? (drawRect.Width - 16) : 0))), (int)(_level.LevelRenderCenter.Y - (_level.CameraPosition.Y - (float)drawRect.Y)), 16, drawRect.Height), frameSource, lazerColor, 0f, Vector2.Zero, (!IsFacingLeft) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
	}
}
