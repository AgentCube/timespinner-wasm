using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L16_Temple;

internal class HellLightAppendage : Appendage
{
	private const int SizeGrowthX = 2;

	private const int SizeGrowthY = -8;

	private const int GlowCopyCount = 4;

	private const float CopyColorMultiplier = 0.35f;

	public HellLightAppendage(Animate parent, Point bboxDimensions, Point inBboxOffset, Level inLevel, SpriteSheet inSprite)
		: base(parent, bboxDimensions, inBboxOffset, inLevel, inSprite)
	{
		ChangeAnimation(13);
		base.DrawPriority = 1;
		base.DoesInheritDrawColor = false;
	}

	protected override void DrawBaseSprite(SpriteBatch spriteBatch, SpriteSheet sprite, Vector2 drawPos, Rectangle source, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float depth)
	{
		int num = _bbox.Width;
		int num2 = _bbox.Height;
		Color drawColor = base.DrawColor;
		int num3 = 0;
		int num4 = 0;
		for (int i = 0; i < 4; i++)
		{
			num += 2;
			num2 += -8;
			drawColor *= 0.35f;
			num3++;
			num4 += -4;
			spriteBatch.Draw(_sprite.Texture, new Rectangle((int)(_level.LevelRenderCenter.X - (_level.CameraPosition.X - (float)_bbox.X + (float)num3)), (int)(_level.LevelRenderCenter.Y - (_level.CameraPosition.Y - (float)_bbox.Y + (float)num4)), num, num2), _frameSource, drawColor, 0f, Vector2.Zero, _spriteEffects, 0f);
		}
		spriteBatch.Draw(_sprite.Texture, new Rectangle((int)(_level.LevelRenderCenter.X - (_level.CameraPosition.X - (float)_bbox.X)), (int)(_level.LevelRenderCenter.Y - (_level.CameraPosition.Y - (float)_bbox.Y)), _bbox.Width, _bbox.Height), _frameSource, base.DrawColor, 0f, Vector2.Zero, _spriteEffects, 0f);
	}
}
