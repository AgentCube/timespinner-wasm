using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies._14_Gyre;

internal sealed class SummoningZonePiece : Appendage
{
	private const int SummonFrameIndex = 23;

	private const int Radius = 16;

	private readonly Rectangle _summonFrameSource;

	private static readonly Color BaseGlyphColor = new Color(0.1f, 0.2f, 0.3f, 0.1f);

	private static readonly Color BaseGlyphColor2 = new Color(0.1f, 0.25f, 0.25f, 0.1f);

	private int _height;

	private int _offsetX;

	private int _targetHeight;

	private float _timer;

	private float _timeToLive;

	private Point _origin;

	private Color _baseDrawColor;

	internal bool IsActive { get; private set; }

	public SummoningZonePiece(Animate parent, Level inLevel, SpriteSheet inSprite)
		: base(parent, new Point(16, 16), Point.Zero, inLevel, inSprite)
	{
		base.DoesInheritDrawColor = false;
		base.IsFacingLocked = true;
		_summonFrameSource = _sprite.GetFrameSource(23);
		Reset(parent.Position);
	}

	public override void Update(float delta)
	{
		if (IsActive)
		{
			_timer += delta;
			if (_timer >= _timeToLive)
			{
				IsActive = false;
			}
			else
			{
				float num = _timer / _timeToLive;
				float num2 = MathEx.SineInterpolate(0f, 1f, num * 2f);
				_height = (int)Math.Round(num2 * (float)_targetHeight);
				base.DrawColor = Color.Transparent.SineInterpolate(_baseDrawColor, num * 2f);
				_offsetX = (int)Math.Round(MathHelper.Lerp(0f, 16f, num));
				if (IsFacingLeft)
				{
					_offsetX = -_offsetX;
				}
				Position = new Point(_origin.X + _offsetX, _origin.Y);
				Bbox = new Rectangle(Position.X, Position.Y, _summonFrameSource.Width, _height);
				SnapBboxToPosition();
				SnapFrameToBbox();
			}
		}
		base.Update(delta);
	}

	internal void Reset(Point origin)
	{
		_origin = origin;
		IsActive = true;
		_timer = 0f;
		_offsetX = 0;
		_height = 0;
		base.DrawColor = Color.Transparent;
		_baseDrawColor = BaseGlyphColor.Lerp(BaseGlyphColor2, (float)_level.NextRandomDouble());
		_timeToLive = RandomBetween(0.25f, 0.5f);
		_targetHeight = RandomBetween(16, 28);
		IsFacingLeft = _level.NextRandomDouble() < 0.5;
	}

	private int RandomBetween(int start, int end)
	{
		return (int)Math.Round(_level.NextRandomDouble() * (double)(end - start)) + start;
	}

	private float RandomBetween(float start, float end)
	{
		return (float)(_level.NextRandomDouble() * (double)(end - start)) + start;
	}

	protected override void DrawBaseSprite(SpriteBatch spriteBatch, SpriteSheet sprite, Vector2 drawPos, Rectangle source, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float depth)
	{
		if (IsActive && _height > 0)
		{
			spriteBatch.Draw(_sprite.Texture, new Rectangle((int)(_level.LevelRenderCenter.X - (_level.CameraPosition.X - (float)Bbox.X)), (int)(_level.LevelRenderCenter.Y - (_level.CameraPosition.Y - (float)Bbox.Y)), Bbox.Width, Bbox.Height), _summonFrameSource, base.DrawColor, 0f, Vector2.Zero, SpriteEffects.None, 0f);
		}
	}
}
