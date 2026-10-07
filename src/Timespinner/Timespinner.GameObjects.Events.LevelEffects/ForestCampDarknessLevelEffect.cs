using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.LevelEffects;

internal sealed class ForestCampDarknessLevelEffect : LevelEffect
{
	private const int StartOffsetX = 8;

	private const int StartOffsetY = 118;

	private static readonly Color DarknessColor = new Color(0f, 0.01f, 0.05f, 0.4f);

	public ForestCampDarknessLevelEffect(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.DrawColor = DarknessColor;
		Position = new Point(Position.X + 8, Position.Y + 118);
		base.DrawPlane = EDrawPlane.Front;
		base.DoesCollideWithTiles = false;
		base.DoesCollideWithProjectiles = false;
		Bbox = new Rectangle(Position.X, Position.Y, 256, 256);
		SnapBboxToPosition();
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.Draw(_level.GCM.TxBlankSquare, new Rectangle((int)(_level.LevelRenderCenter.X - (_level.CameraPosition.X - (float)_bbox.X)), (int)(_level.LevelRenderCenter.Y - (_level.CameraPosition.Y - (float)_bbox.Y)), _bbox.Width, _bbox.Height), null, base.DrawColor);
	}
}
