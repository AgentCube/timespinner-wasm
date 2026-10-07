using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Sandman;

internal class SandDrawHelper
{
	private const int SandDrawIterationCount = 2;

	private const float SandDrawIterationOffset = 0.5f;

	private const float SandDrawR = 0.125f;

	private const float SandDrawG = 0.078f;

	private const float SandDrawB = 0.047f;

	private const float SandDrawA = 0.5f;

	private readonly Animate _parent;

	private readonly Level _level;

	private float _parentAlpha;

	private Vector2 _sandOffset;

	internal float SandScrollSpeedX { get; set; }

	internal float SandScrollSpeedY { get; set; }

	internal float SandJitterSpeed { get; set; }

	internal SandDrawHelper(Animate parent)
	{
		_parent = parent;
		_level = _parent.Level;
	}

	internal void Update(float delta)
	{
		SandScrollSpeedY = 0.1f;
		SandJitterSpeed = 0.1f;
		_sandOffset.X += SandScrollSpeedX * delta;
		_sandOffset.Y += SandScrollSpeedY * delta;
		if (_sandOffset.X > 1f)
		{
			_sandOffset.X -= 1f;
		}
		if (_sandOffset.Y > 1f)
		{
			_sandOffset.Y -= 1f;
		}
		_parentAlpha = (float)(int)_parent.DrawColor.A / 255f;
	}

	internal void Draw(SpriteBatch spriteBatch, Animate target, Vector2 textureRatio)
	{
		foreach (Appendage appendage in target.Appendages)
		{
			if (appendage.DrawPriority <= 0)
			{
				if (appendage.DoesInheritDrawColor)
				{
					appendage.DrawColor = target.DrawColor;
					Draw(spriteBatch, appendage, textureRatio);
				}
				else
				{
					appendage.Draw(spriteBatch);
				}
			}
		}
		Color drawColor = target.DrawColor;
		bool doesDrawAppendages = target.DoesDrawAppendages;
		target.DrawColor = new Color(0.125f * _parentAlpha, 0.078f * _parentAlpha, 0.047f * _parentAlpha, 0.5f * _parentAlpha);
		target.DoesDrawAppendages = false;
		target.Draw(spriteBatch);
		spriteBatch.End();
		_level.GCM.EfSandDraw.Parameters["SizeRatio"].SetValue(textureRatio);
		spriteBatch.GraphicsDevice.Textures[1] = _level.GCM.SpSandTexture.Texture;
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, _level.GCM.EfSandDraw);
		for (int i = 0; i < 2; i++)
		{
			target.DrawColor = new Color(_sandOffset.X, _sandOffset.Y, 0.5f * (float)i, _parentAlpha * 0.5f);
			target.Draw(spriteBatch);
		}
		spriteBatch.End();
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null);
		target.DrawColor = drawColor;
		target.DoesDrawAppendages = doesDrawAppendages;
		foreach (Appendage appendage2 in target.Appendages)
		{
			if (appendage2.DrawPriority > 0)
			{
				if (appendage2.DoesInheritDrawColor)
				{
					appendage2.DrawColor = target.DrawColor;
					Draw(spriteBatch, appendage2, textureRatio);
				}
				else
				{
					appendage2.Draw(spriteBatch);
				}
			}
		}
	}
}
