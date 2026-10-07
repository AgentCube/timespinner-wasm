using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L16_Temple;

internal class HellfireAppendage : Appendage
{
	private const float TimeToScroll = 0.25f;

	private const float TrigTimeMultiplier = 5f;

	private const float TrigOffsetMultiplier = 1f;

	private const float UnderScrollOffset = 0.25f;

	private const float UnderTrigTimerOffset = (float)Math.PI;

	private static readonly Color BaseColor = new Color(32, 64, 16, 224);

	private static readonly Color BaseUnderColor = new Color(24, 8, 20);

	private float _scrollTimer;

	private float _scrollPercentage;

	private float _underScrollPercentage;

	private float _trigTimer;

	private float _trigOffsetX;

	private Color _underColor;

	public HellfireAppendage(Animate parent, Point bboxDimensions, Point inBboxOffset, Level inLevel, SpriteSheet inSprite)
		: base(parent, bboxDimensions, inBboxOffset, inLevel, inSprite)
	{
		base.DoesInheritDrawColor = false;
		ChangeAnimation(0);
		base.DrawPriority = -1;
	}

	internal void SetColorMultiplier(float multiplier)
	{
		base.DrawColor = BaseColor * multiplier;
		_underColor = BaseUnderColor * multiplier;
	}

	public override void Update(float delta)
	{
		UpdateShader(delta);
		base.Update(delta);
	}

	private void UpdateShader(float delta)
	{
		_scrollTimer += delta;
		_scrollPercentage = (_scrollTimer / 0.25f).Mod(1f);
		_underScrollPercentage = (_scrollPercentage + 0.25f).Mod(1f);
		_trigTimer += delta * 5f;
		_trigOffsetX += delta * 1f;
	}

	private void ApplySineShaderValues(float scroll, float time, float offsetX)
	{
		Vector4 value = new Vector4(-1f, scroll, time, offsetX);
		_level.GCM.EfScrollingDeform.Parameters["ShaderValues"].SetValue(value);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.End();
		ApplySineShaderValues(_underScrollPercentage, _trigTimer + (float)Math.PI, _trigOffsetX);
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, _level.GCM.EfScrollingDeform);
		Color drawColor = base.DrawColor;
		base.DrawColor = _underColor;
		base.Draw(spriteBatch);
		spriteBatch.End();
		ApplySineShaderValues(_scrollPercentage, _trigTimer, _trigOffsetX);
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, _level.GCM.EfScrollingDeform);
		base.DrawColor = drawColor;
		base.Draw(spriteBatch);
		spriteBatch.End();
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null);
	}

	protected override void DrawBaseSprite(SpriteBatch spriteBatch, SpriteSheet sprite, Vector2 drawPos, Rectangle source, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float depth)
	{
		spriteBatch.Draw(_sprite.Texture, new Rectangle((int)(_level.LevelRenderCenter.X - (_level.CameraPosition.X - (float)_bbox.X)), (int)(_level.LevelRenderCenter.Y - (_level.CameraPosition.Y - (float)_bbox.Y)), _bbox.Width, _bbox.Height), _frameSource, base.DrawColor, 0f, Vector2.Zero, _spriteEffects, 0f);
	}
}
