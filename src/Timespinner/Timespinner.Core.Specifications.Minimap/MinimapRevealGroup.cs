using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Timespinner.Core.Specifications.Minimap;

public class MinimapRevealGroup
{
	private readonly List<int> _rooms = new List<int>();

	public int ID { get; set; }

	public List<int> Rooms => _rooms;

	public void AddRoom(int level, int room)
	{
		int num = room;
		num <<= 6;
		num |= level;
		_rooms.Add(num);
	}

	public List<Point> GetRooms()
	{
		List<Point> list = new List<Point>();
		foreach (int room in _rooms)
		{
			int x = room & 0x3F;
			int y = room >> 6;
			list.Add(new Point(x, y));
		}
		return list;
	}

	public string GetSaveString()
	{
		return string.Join(",", _rooms);
	}

	public void LoadFromString(string saveString)
	{
		_rooms.Clear();
		string[] array = saveString.Split(new char[1] { ',' });
		string[] array2 = array;
		foreach (string s in array2)
		{
			if (int.TryParse(s, out var result))
			{
				_rooms.Add(result);
			}
		}
	}
}
