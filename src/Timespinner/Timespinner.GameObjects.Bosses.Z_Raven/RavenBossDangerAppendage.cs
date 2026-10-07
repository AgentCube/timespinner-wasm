using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Z_Raven;

internal sealed class RavenBossDangerAppendage : Appendage
{
	private const int Anim_BlankFrame = 25;

	private const int HistoryCount = 32;

	private readonly Animate _parent;

	internal bool IsDrawingCircleTrail { get; set; }

	internal float ColorMultiplier { get; set; }

	public RavenBossDangerAppendage(Animate parent, Level inLevel, SpriteSheet inSprite)
		: base(parent, new Point(16, 16), Point.Zero, inLevel, inSprite)
	{
		_parent = parent;
		ChangeAnimation(25);
		GetFrameSource(shouldForce: true);
		base.FollowType = EAppendageFollowType.AnchorLocked;
		base.AnchorObject = _parent;
		_doesDrawTrail = true;
		_trailLength = 64;
		_doesDrawBrushTrail = true;
		_brushTrailSize = 16;
		_trailFadeRate = 1f;
		_trailInterpolationAmount = 4;
		_trailShrinkRate = 0f;
		base.TrailColor = new Color(128, 0, 0, 32);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			base.TrailColor = (IsDrawingCircleTrail ? new Color(128, 0, 0, 12) : new Color(128, 0, 0, 32));
			if (IsDrawingCircleTrail)
			{
				_brushTrailSize = (int)Math.Round(64f * ColorMultiplier);
			}
		}
		base.Update(delta);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		_trailShrinkRate = 0.00033f;
		float num = 1f;
		int height = _parent.OuterBbox.Height;
		int num2 = height / 2;
		for (int i = 0; i < _drawHistories.Count; i++)
		{
			DrawHistory drawHistory = _drawHistories[i];
			if (_trailFadeRate >= 0f)
			{
				num = (float)i / (float)_drawHistories.Count / _trailFadeRate;
				if (num < 0f)
				{
					num = 0f;
				}
			}
			Color color = drawHistory.DrawColor * ColorMultiplier;
			color = new Color(color.R, color.G, color.B) * num * ((float)(int)color.A / 255f);
			if (IsDrawingCircleTrail)
			{
				float num3 = 0f;
				if (_trailShrinkRate > 0f)
				{
					num3 = _trailShrinkRate * (float)(_drawHistories.Count - i) * 2f;
					if (num3 < 0f)
					{
						num3 = 0f;
					}
					if (num3 > _scale)
					{
						num3 = _scale;
					}
				}
				Vector2 value = CameraizePoint(drawHistory.DrawPosition, new Vector2(8f, 16f));
				float num4 = ((float)_brushTrailSize * _scale - 2.5f) / 256f;
				spriteBatch.Draw(_level.GCM.TxLargeCircle, Vector2.Subtract(_level.LevelRenderCenter, value), null, color, 0f, new Vector2(128f, 128f), num4 - num3, SpriteEffects.None, 0f);
			}
			else
			{
				Point point = new Point(drawHistory.DrawPosition.X, drawHistory.DrawPosition.Y - num2);
				spriteBatch.Draw(_sprite.Texture, new Rectangle((int)(_level.LevelRenderCenter.X - (_level.CameraPosition.X - (float)point.X)), (int)(_level.LevelRenderCenter.Y - (_level.CameraPosition.Y - (float)point.Y)), 8, height), _frameSource, color);
			}
		}
	}
}
