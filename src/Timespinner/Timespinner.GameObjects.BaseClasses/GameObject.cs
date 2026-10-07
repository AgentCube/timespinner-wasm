using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.BaseClasses;

public abstract class GameObject
{
	protected readonly Level _level;

	protected bool _doesHarmOnTouch;

	protected int _squishDamage;

	protected Point _position;

	protected Point _drawPosition;

	protected Point _bboxOffset;

	protected Rectangle _bbox;

	protected SpriteSheet _sprite;

	public bool CannotBeGrabbed { get; protected set; }

	public int SquishDamage => _squishDamage;

	public virtual Point Position
	{
		get
		{
			return _position;
		}
		set
		{
			_position = value;
		}
	}

	public virtual Point BboxOffset
	{
		get
		{
			return _bboxOffset;
		}
		set
		{
			_bboxOffset = value;
		}
	}

	public virtual Rectangle Bbox
	{
		get
		{
			return _bbox;
		}
		protected set
		{
			_bbox = value;
		}
	}

	public Level Level => _level;

	protected GameObject(Point inPosition, Level inLevel)
	{
		_position = inPosition;
		_bbox = new Rectangle(_position.X, _position.Y, 0, 0);
		_drawPosition = _position;
		_level = inLevel;
	}

	public abstract void Draw(SpriteBatch spriteBatch);
}
