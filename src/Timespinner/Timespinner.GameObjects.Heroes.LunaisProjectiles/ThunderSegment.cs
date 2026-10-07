using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Heroes.LunaisProjectiles;

internal sealed class ThunderSegment : Appendage
{
	private const int BboxBufferThickness = 4;

	private const int FrameStart = 7;

	private const int FrameCount = 7;

	private int _thickness;

	private int _hypotenuseLength;

	private float _hypotenuseAngle;

	private Point _intervalStart;

	private Point _intervalEnd;

	private Rectangle _topFrameSource;

	private Rectangle _bottomFrameSource;

	private SpriteEffects _topEffect;

	private SpriteEffects _bottomEffect;

	internal Point IntervalStart => _intervalStart;

	internal Point IntervalEnd => _intervalEnd;

	public ThunderSegment(Animate parent, Vector4 interval, Level inLevel, SpriteSheet inSprite)
		: base(parent, new Point(16, 16), Point.Zero, inLevel, inSprite)
	{
		_doesDrawBaseSprite = false;
		Reset(interval);
	}

	public override void SnapBboxToPosition()
	{
		_bbox.Location = new Point(Math.Min(_intervalStart.X, _intervalEnd.X) - 2, Math.Min(_intervalStart.Y, _intervalEnd.Y) - 2);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		Vector2 value = CameraizePoint(_intervalStart);
		Vector2 start = Vector2.Subtract(_level.LevelRenderCenter, value);
		DrawingEx.DrawLine(spriteBatch, _sprite.Texture, _bottomFrameSource, start, _hypotenuseLength, _thickness, _hypotenuseAngle, base.DrawColor * 0.2f, _bottomEffect);
		DrawingEx.DrawLine(spriteBatch, _sprite.Texture, _topFrameSource, start, _hypotenuseLength, _thickness, _hypotenuseAngle, base.DrawColor, _topEffect);
		base.Draw(spriteBatch);
	}

	internal void Reset(Vector4 interval)
	{
		_intervalStart = new Point((int)interval.X, (int)interval.Y);
		_intervalEnd = new Point((int)interval.Z, (int)interval.W);
		int num = _intervalStart.X - _intervalEnd.X;
		int num2 = _intervalStart.Y - _intervalEnd.Y;
		_hypotenuseAngle = (float)Math.Atan2(num2, num) + (float)Math.PI;
		_hypotenuseLength = (int)Math.Sqrt(num * num + num2 * num2) + 1;
		_thickness = _level.NextRandomInt(12, 16);
		Position = _intervalStart;
		Bbox = new Rectangle(_intervalStart.X, _intervalStart.Y, Math.Abs(num) + 4, Math.Abs(num2) + 4);
		_topFrameSource = _sprite.GetFrameSource(7 + _level.NextRandomInt(0, 7));
		_bottomFrameSource = _sprite.GetFrameSource(7 + _level.NextRandomInt(0, 7));
		_topEffect = ((_thickness % 2 == 0) ? SpriteEffects.FlipVertically : SpriteEffects.None);
		_bottomEffect = ((_thickness % 4 != 0) ? SpriteEffects.FlipVertically : SpriteEffects.None);
		SnapBboxToPosition();
	}
}
