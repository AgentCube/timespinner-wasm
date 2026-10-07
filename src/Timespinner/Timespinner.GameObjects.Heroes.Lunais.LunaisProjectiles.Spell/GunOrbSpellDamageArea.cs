using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Lunais.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Heroes.Lunais.LunaisProjectiles.Spell;

internal sealed class GunOrbSpellDamageArea : LunaisBaseOrbDamageArea
{
	internal const int DamageWidth = 400;

	internal const int DamageHeight = 4;

	internal const float MaxLife = 0.25f;

	private const float StartEndCutOffPercentage = 0.33f;

	private readonly Rectangle _lazerFrameSource;

	public GunOrbSpellDamageArea(Level inLevel, Point inPosition, ETeamSide inSide, Mobile inAnchor, LunaisOrbAbility parentOrb, SpriteSheet sprite, int baseOrbDamage, bool isFacingLeft)
		: base(inLevel, inPosition, inSide, -1, inAnchor, parentOrb)
	{
		IsFacingLeft = isFacingLeft;
		_sprite = sprite;
		_lazerFrameSource = _sprite.GetFrameSource(27);
		_doesDrawSpriteAndAppendages = false;
		base.DamageDimensions = new Point(400, 4);
		SnapBboxToPosition();
		base.DoesDrawAura = true;
		base.AuraColor = Color.White * 0.9f;
		_auraCount = 4f;
		base.AuraSize = 1.25f;
		base.AuraFrequency = 9f;
		_power = baseOrbDamage * 3;
		_force = 4;
		_life = 0.25f;
		base.DamageTimeoutTime = 0.25f;
		_canDamageThings = false;
		_damageElement = EDamageElement.Sharp;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_isFlying = true;
		_doesCollideWithSlopes = false;
		_doesDieOnTiles = false;
	}

	public override void Update(float delta)
	{
		float num = _life / 0.25f;
		_canDamageThings = false;
		if (num < 0.33f)
		{
			float amount = num / 0.33f;
			_bbox.Height = (int)(MathHelper.SmoothStep(0f, 1f, amount) * (float)_damageDimensions.Y);
		}
		else if (num > 0.66999996f)
		{
			float amount2 = (1f - num) / 0.33f;
			_bbox.Height = (int)(MathHelper.SmoothStep(0f, 1f, amount2) * (float)_damageDimensions.Y);
		}
		else
		{
			_bbox.Height = _damageDimensions.Y;
			_canDamageThings = true;
		}
		base.Update(delta);
	}

	public override void SnapBboxToPosition()
	{
		_bbox.Location = new Point(_position.X - (IsFacingLeft ? _bbox.Width : 0), _position.Y - _bbox.Height / 2);
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
		spriteBatch.Draw(_sprite.Texture, new Rectangle((int)(_level.LevelRenderCenter.X - (_level.CameraPosition.X - (float)drawRect.X)), (int)(_level.LevelRenderCenter.Y - (_level.CameraPosition.Y - (float)drawRect.Y)), drawRect.Width, drawRect.Height), _lazerFrameSource, lazerColor);
	}

	public void Reset(Point startPoint, int spellDamage, bool isFacingLeft)
	{
		_life = 0.25f;
		_power = spellDamage * 3;
		_canDamageThings = false;
		_isFading = false;
		_fadeTimer = 0f;
		base.ID = -1;
		IsFacingLeft = isFacingLeft;
		Position = startPoint;
		SnapBboxToPosition();
	}

	internal void Cancel()
	{
		float num = 0.33f;
		if (_life > 0.66999996f)
		{
			num = 1f - _life / 0.25f;
		}
		if (_life > 0.33f)
		{
			_life = num * 0.25f;
		}
	}
}
