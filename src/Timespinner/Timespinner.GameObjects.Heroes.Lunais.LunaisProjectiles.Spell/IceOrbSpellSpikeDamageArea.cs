using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;
using Timespinner.GameObjects.Heroes.Spells;

namespace Timespinner.GameObjects.Heroes.Lunais.LunaisProjectiles.Spell;

internal sealed class IceOrbSpellSpikeDamageArea : LunaisBaseOrbDamageArea
{
	private const int MaxGrowHeight = 128;

	private const int MaxGrowWidth = 52;

	private const int DrawWidth = 72;

	private const float TimeToGrow = 0.25f;

	private const float MaxLife = 1f;

	private readonly SnowCloudParticleSystem _snowCloudParticles;

	private bool _isDoneGrowing;

	private float _growthTimer;

	private Rectangle _drawRectangle;

	public IceOrbSpellSpikeDamageArea(Level inLevel, Point inPosition, ETeamSide inSide, int baseDamage, LunaisSpell parentSpell)
		: base(inLevel, inPosition, inSide, -1, null, parentSpell)
	{
		_sprite = _level.GCM.SpOrbMeleeIce;
		_doesDrawSpriteAndAppendages = false;
		_power = baseDamage;
		_force = 4;
		_life = 1f;
		_timeToFade = 0.5f;
		base.DamageTimeoutTime = 0.7f;
		_damageElement = EDamageElement.Ice;
		_doesAutomaticallyEmitParticles = false;
		_snowCloudParticles = new SnowCloudParticleSystem(_sprite, 1);
		_particleSystems.Add(_snowCloudParticles);
		PlayCue(ESFX.LunaisOrbIceSpellEmerge, Position);
	}

	public override void Update(float delta)
	{
		if (!_isDoneGrowing)
		{
			if (_growthTimer <= 0f)
			{
				_snowCloudParticles.AddParticles(Position.ToVector2());
			}
			_growthTimer += delta;
			float num = _growthTimer / 0.25f;
			if (num >= 1f)
			{
				_isDoneGrowing = true;
				num = 1f;
			}
			float num2 = (float)Math.Sin(num * ((float)Math.PI / 2f));
			float num3 = num2;
			base.IsGlowing = num3 < 1f;
			base.GlowColor = (base.IsGlowing ? new Color(1f, 1f, 1f, num3) : Color.White);
			base.GlowBase = 12f;
			int num4 = (int)(num2 * 72f);
			base.DamageDimensions = new Point((int)(num2 * 52f), (int)(num2 * 128f));
			_drawRectangle = new Rectangle(Position.X - num4 / 2, Position.Y - base.DamageDimensions.Y, num4, base.DamageDimensions.Y);
		}
		base.Update(delta);
	}

	public override void SnapBboxToPosition()
	{
		_bbox.Location = new Point(_position.X - _bbox.Width / 2, _position.Y - _bbox.Height);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (_isGlowing)
		{
			spriteBatch.End();
			_level.GCM.EfBrighten.Parameters["shinyAmount"].SetValue(_glowBase);
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, _level.GCM.EfBrighten);
			base.DrawColor = _glowColor;
		}
		Rectangle frameSource = _sprite.GetFrameSource(16);
		spriteBatch.Draw(_sprite.Texture, new Rectangle((int)(_level.LevelRenderCenter.X - (_level.CameraPosition.X - (float)_drawRectangle.X)), (int)(_level.LevelRenderCenter.Y - (_level.CameraPosition.Y - (float)_drawRectangle.Y)), _drawRectangle.Width, _drawRectangle.Height), frameSource, base.DrawColor, 0f, Vector2.Zero, SpriteEffects.None, 0f);
		if (_isGlowing)
		{
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null);
		}
		base.Draw(spriteBatch);
	}
}
