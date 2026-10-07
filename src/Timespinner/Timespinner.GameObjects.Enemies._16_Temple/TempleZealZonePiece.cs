using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies._16_Temple;

internal sealed class TempleZealZonePiece : Appendage
{
	private const int SummonFrameIndex = 10;

	private const int Radius = 8;

	private const int SizeGainX = 8;

	private static readonly Color BaseGlyphColor = new Color(0.1f, 0.15f, 0.3f, 0.1f);

	private static readonly Color BaseGlyphColor2 = new Color(0.1f, 0.2f, 0.35f, 0.1f);

	private readonly bool _isUpsideDown;

	private readonly Rectangle _summonFrameSource;

	private bool _isSkinny;

	private int _height;

	private int _offsetX;

	private int _targetHeight;

	private float _timer;

	private float _timeToLive;

	private Point _origin;

	private Color _baseDrawColor;

	internal bool IsActive { get; private set; }

	public TempleZealZonePiece(Animate parent, Level inLevel, SpriteSheet inSprite, bool isUpsideDown)
		: base(parent, new Point(16, 16), Point.Zero, inLevel, inSprite)
	{
		_isUpsideDown = isUpsideDown;
		if (_isUpsideDown)
		{
			IsFlippedVertically = true;
		}
		base.DoesInheritDrawColor = false;
		base.IsFacingLocked = true;
		base.DoesCollideWithAnything = false;
		_summonFrameSource = _sprite.GetFrameSource(10);
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
				_offsetX = (int)Math.Round(MathHelper.Lerp(0f, 8f, num));
				if (IsFacingLeft)
				{
					_offsetX = -_offsetX;
				}
				int num3 = ((!_isSkinny) ? 8 : 0);
				Position = new Point(_origin.X + _offsetX, _isUpsideDown ? (_origin.Y + _height) : _origin.Y);
				Bbox = new Rectangle(Position.X, Position.Y, _summonFrameSource.Width + num3, _height);
				SnapBboxToPosition();
				SnapFrameToBbox();
			}
		}
		base.Update(delta);
	}

	internal void Reset(Point origin, bool isGoingLeft, bool isSkinny)
	{
		_origin = origin;
		_isSkinny = isSkinny;
		IsActive = true;
		_timer = 0f;
		_offsetX = 0;
		_height = 0;
		base.DrawColor = Color.Transparent;
		_baseDrawColor = BaseGlyphColor.Lerp(BaseGlyphColor2, (float)_level.NextRandomDouble());
		_timeToLive = RandomBetween(0.45f, 0.6f);
		_targetHeight = RandomBetween(80, 104);
		IsFacingLeft = isGoingLeft;
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
			spriteBatch.Draw(_sprite.Texture, new Rectangle((int)(_level.LevelRenderCenter.X - (_level.CameraPosition.X - (float)Bbox.X)), (int)(_level.LevelRenderCenter.Y - (_level.CameraPosition.Y - (float)Bbox.Y)), Bbox.Width, Bbox.Height), _summonFrameSource, base.DrawColor, 0f, Vector2.Zero, _spriteEffects, 0f);
		}
	}
}
