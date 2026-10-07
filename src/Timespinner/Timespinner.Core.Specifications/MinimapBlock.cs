using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Timespinner.Core.Specifications;

public class MinimapBlock
{
	private readonly MinimapRoom _parentRoom;

	private readonly bool[] _walls = new bool[4];

	private readonly bool[] _doors = new bool[4];

	private readonly bool[] _secretDoors = new bool[4];

	public bool IsKnown { get; set; }

	public bool IsVisited { get; set; }

	public bool IsBoss { get; set; }

	public bool IsTimespinner { get; set; }

	public bool IsTransition { get; set; }

	public bool IsCheckpoint { get; set; }

	public bool IsSolidWall { get; set; }

	public bool HasSolidWallToNW { get; set; }

	public EMinimapRoomColor RoomColor { get; set; }

	public Point Position { get; set; }

	public MinimapRoom ParentRoom => _parentRoom;

	public bool[] Walls => _walls;

	public bool[] Doors => _doors;

	public bool[] SecretDoors => _secretDoors;

	public MinimapBlock(MinimapRoom parentRoom)
	{
		_parentRoom = parentRoom;
	}

	public void Draw(SpriteBatch spriteBatch, SpriteSheet minimapSprite, Vector2 drawPosition, float zoom, Color baseDrawColor)
	{
		Draw(spriteBatch, minimapSprite, drawPosition, zoom, baseDrawColor, 1f);
	}

	public void Draw(SpriteBatch spriteBatch, SpriteSheet minimapSprite, Vector2 drawPosition, float zoom, Color baseDrawColor, float alphaAmount)
	{
		Color color = baseDrawColor * alphaAmount;
		if (!IsKnown || IsSolidWall)
		{
			return;
		}
		EMinimapRoomColor index = (IsVisited ? RoomColor : EMinimapRoomColor.Grey);
		Rectangle frameSource = minimapSprite.GetFrameSource((int)index);
		spriteBatch.Draw(minimapSprite.Texture, drawPosition, frameSource, color, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
		for (int i = 0; i < 4; i++)
		{
			if (_walls[i])
			{
				bool flag = _doors[i];
				if (flag && _secretDoors[i] && !_parentRoom.IsAdjacentBlockFound(Position, i))
				{
					flag = false;
				}
				int num = (flag ? 4 : 0);
				frameSource = minimapSprite.GetFrameSource(12 + num + i);
				spriteBatch.Draw(minimapSprite.Texture, drawPosition, frameSource, color, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
			}
		}
		if (HasSolidWallToNW)
		{
			frameSource = minimapSprite.GetFrameSource(20);
			spriteBatch.Draw(minimapSprite.Texture, drawPosition, frameSource, color, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
		}
		if (IsVisited)
		{
			if (IsCheckpoint)
			{
				frameSource = minimapSprite.GetFrameSource(5);
				spriteBatch.Draw(minimapSprite.Texture, drawPosition, frameSource, color, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
			}
			else if (IsTransition)
			{
				frameSource = minimapSprite.GetFrameSource((RoomColor == EMinimapRoomColor.Blue) ? 8 : 7);
				spriteBatch.Draw(minimapSprite.Texture, drawPosition, frameSource, color, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
			}
			if (IsTimespinner)
			{
				frameSource = minimapSprite.GetFrameSource(9);
				spriteBatch.Draw(minimapSprite.Texture, drawPosition, frameSource, color, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
			}
		}
	}

	public void SetBoolArrayFromString(bool[] target, string text)
	{
		if (text == null)
		{
			return;
		}
		int length = text.Length;
		for (int i = 0; i < length; i++)
		{
			if (text[i] == '1')
			{
				target[i] = true;
			}
		}
	}
}
