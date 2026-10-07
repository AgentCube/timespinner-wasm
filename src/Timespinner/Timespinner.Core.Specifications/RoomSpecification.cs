using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Timespinner.Core.Specifications;

public class RoomSpecification
{
	private readonly Dictionary<Point, TileSpecification> _middleTiles = new Dictionary<Point, TileSpecification>();

	private readonly Dictionary<Point, List<TileSpecification>> _bottomTiles = new Dictionary<Point, List<TileSpecification>>();

	private readonly Dictionary<Point, List<TileSpecification>> _topTiles = new Dictionary<Point, List<TileSpecification>>();

	private readonly Dictionary<Point, List<ObjectTileSpecification>> _objectTiles = new Dictionary<Point, List<ObjectTileSpecification>>();

	private readonly Dictionary<Point, TileSwathSpecification> _tileSwaths = new Dictionary<Point, TileSwathSpecification>();

	private readonly List<BackgroundSpecification> _backgrounds = new List<BackgroundSpecification>();

	public ETilesetType Tileset { get; set; }

	public int ID { get; set; }

	public int Index { get; set; }

	public int Width { get; set; }

	public int Height { get; set; }

	public string Name { get; set; }

	public Color BackgroundWipeColor { get; set; }

	public Dictionary<Point, TileSpecification> MiddleTiles => _middleTiles;

	public Dictionary<Point, List<TileSpecification>> BottomTiles => _bottomTiles;

	public Dictionary<Point, List<TileSpecification>> TopTiles => _topTiles;

	public Dictionary<Point, List<ObjectTileSpecification>> ObjectTiles => _objectTiles;

	public Dictionary<Point, TileSwathSpecification> TileSwaths => _tileSwaths;

	public List<BackgroundSpecification> Backgrounds => _backgrounds;

	public bool DoesTileExist(Point position, ETileLayerType layer)
	{
		bool result = false;
		switch (layer)
		{
		case ETileLayerType.Bottom:
			result = BottomTiles.ContainsKey(position);
			break;
		case ETileLayerType.Middle:
			result = MiddleTiles.ContainsKey(position);
			break;
		case ETileLayerType.Top:
			result = TopTiles.ContainsKey(position);
			break;
		case ETileLayerType.Objects:
			result = ObjectTiles.ContainsKey(position);
			break;
		}
		return result;
	}

	public IEnumerable<TileSpecification> GetTiles(Point position, ETileLayerType layer)
	{
		List<TileSpecification> list = new List<TileSpecification>();
		switch (layer)
		{
		case ETileLayerType.Bottom:
			if (BottomTiles.ContainsKey(position))
			{
				list.AddRange(BottomTiles[position]);
			}
			break;
		case ETileLayerType.Middle:
			if (MiddleTiles.ContainsKey(position))
			{
				list.Add(MiddleTiles[position]);
			}
			break;
		case ETileLayerType.Top:
			if (TopTiles.ContainsKey(position))
			{
				list.AddRange(TopTiles[position]);
			}
			break;
		case ETileLayerType.Objects:
			if (ObjectTiles.ContainsKey(position))
			{
				list.AddRange(((IEnumerable<ObjectTileSpecification>)ObjectTiles[position]).Select((Func<ObjectTileSpecification, TileSpecification>)((ObjectTileSpecification a) => a)).ToList());
			}
			break;
		case ETileLayerType.Default:
			if (BottomTiles.ContainsKey(position))
			{
				list.AddRange(BottomTiles[position]);
			}
			if (MiddleTiles.ContainsKey(position))
			{
				list.Add(MiddleTiles[position]);
			}
			if (TopTiles.ContainsKey(position))
			{
				list.AddRange(TopTiles[position]);
			}
			if (ObjectTiles.ContainsKey(position))
			{
				list.AddRange(((IEnumerable<ObjectTileSpecification>)ObjectTiles[position]).Select((Func<ObjectTileSpecification, TileSpecification>)((ObjectTileSpecification a) => a)).ToList());
			}
			break;
		}
		return list;
	}

	public void AddTile(Point point, TileSpecification tile)
	{
		switch (tile.Layer)
		{
		case ETileLayerType.Bottom:
			AddStackedTile(point, tile, ETileLayerType.Bottom);
			break;
		case ETileLayerType.Middle:
			MiddleTiles[point] = tile;
			break;
		case ETileLayerType.Top:
			AddStackedTile(point, tile, ETileLayerType.Top);
			break;
		case ETileLayerType.Objects:
			AddObjectTile(point, tile as ObjectTileSpecification);
			break;
		}
	}

	public void AddStackedTile(Point point, TileSpecification tile, ETileLayerType layer)
	{
		Dictionary<Point, List<TileSpecification>> dictionary = null;
		switch (layer)
		{
		case ETileLayerType.Bottom:
			dictionary = BottomTiles;
			break;
		case ETileLayerType.Top:
			dictionary = TopTiles;
			break;
		}
		if (dictionary == null)
		{
			return;
		}
		if (dictionary.ContainsKey(point))
		{
			if (tile.ID == -1)
			{
				dictionary[point].Clear();
				dictionary.Remove(point);
			}
			else if (dictionary[point].All((TileSpecification ink) => ink.ID != tile.ID))
			{
				dictionary[point].Add(tile);
			}
		}
		else
		{
			dictionary[point] = new List<TileSpecification> { tile };
		}
	}

	public void AddObjectTile(Point point, ObjectTileSpecification toAdd)
	{
		if (ObjectTiles.ContainsKey(point))
		{
			if (toAdd.Category == EObjectTileCategory.None || toAdd.ID == -1)
			{
				ObjectTiles[point].Clear();
				ObjectTiles.Remove(point);
			}
			else if (ObjectTiles[point].All((ObjectTileSpecification obj) => obj.ID != toAdd.ID))
			{
				ObjectTiles[point].Add(toAdd);
			}
		}
		else
		{
			ObjectTiles[point] = new List<ObjectTileSpecification> { toAdd };
		}
	}

	public void RemoveTile(Point position, ETileLayerType layer)
	{
		switch (layer)
		{
		case ETileLayerType.Bottom:
			if (BottomTiles.ContainsKey(position))
			{
				BottomTiles.Remove(position);
			}
			break;
		case ETileLayerType.Middle:
			if (MiddleTiles.ContainsKey(position))
			{
				MiddleTiles.Remove(position);
			}
			break;
		case ETileLayerType.Top:
			if (TopTiles.ContainsKey(position))
			{
				TopTiles.Remove(position);
			}
			break;
		case ETileLayerType.Objects:
			if (ObjectTiles.ContainsKey(position))
			{
				ObjectTiles[position].Clear();
				ObjectTiles.Remove(position);
			}
			break;
		}
	}

	public void AddTileSwath(Point point, TileSwathSpecification swath)
	{
		_tileSwaths[point] = swath;
	}

	public void RemoveTileSwaths(Point point, int width, int height)
	{
		IEnumerable<TileSwathSpecification> swaths = GetSwaths(point, width, height);
		foreach (TileSwathSpecification item in swaths)
		{
			Point key = new Point(item.X, item.Y);
			_tileSwaths.Remove(key);
		}
	}

	public IEnumerable<TileSwathSpecification> GetSwaths(Point topLeft, int width, int height)
	{
		List<TileSwathSpecification> list = new List<TileSwathSpecification>();
		int num = topLeft.X + width;
		int num2 = topLeft.Y + height;
		for (int i = topLeft.X; i < num; i++)
		{
			for (int j = topLeft.Y; j < num2; j++)
			{
				Point key = new Point(i, j);
				if (_tileSwaths.ContainsKey(key))
				{
					list.Add(_tileSwaths[key]);
				}
			}
		}
		return list;
	}

	public void TranslateTilesAtPoint(Point key, bool isColumns, int amount)
	{
		Point offset = new Point(isColumns ? amount : 0, (!isColumns) ? amount : 0);
		TranslateTilesAtPoint(key, offset);
	}

	public void TranslateTilesAtPoint(Point key, Point offset)
	{
		TranslateTileLayer(BottomTiles, new Point(key.X, key.Y), offset);
		TranslateTileLayer(MiddleTiles, new Point(key.X, key.Y), offset);
		TranslateTileLayer(TopTiles, new Point(key.X, key.Y), offset);
		if (!ObjectTiles.ContainsKey(key))
		{
			return;
		}
		List<ObjectTileSpecification> list = ObjectTiles[key];
		ObjectTiles.Remove(key);
		foreach (ObjectTileSpecification item in list)
		{
			item.X += offset.X;
			item.Y += offset.Y;
		}
		key.X += offset.X;
		key.Y += offset.Y;
		ObjectTiles.Add(key, list);
	}

	private void TranslateTileLayer(Dictionary<Point, TileSpecification> layer, Point key, Point offset)
	{
		if (layer.ContainsKey(key))
		{
			TileSpecification tileSpecification = layer[key];
			layer.Remove(key);
			tileSpecification.X += offset.X;
			key.X += offset.X;
			tileSpecification.Y += offset.Y;
			key.Y += offset.Y;
			layer.Add(key, tileSpecification);
		}
	}

	private void TranslateTileLayer(Dictionary<Point, List<TileSpecification>> layer, Point key, Point offset)
	{
		if (!layer.ContainsKey(key))
		{
			return;
		}
		List<TileSpecification> list = layer[key];
		layer.Remove(key);
		foreach (TileSpecification item in list)
		{
			item.X += offset.X;
			item.Y += offset.Y;
		}
		key.X += offset.X;
		key.Y += offset.Y;
		layer.Add(key, list);
	}

	public IEnumerable<ObjectTileSpecification> GetAllMinimapObjects()
	{
		List<ObjectTileSpecification> list = new List<ObjectTileSpecification>();
		foreach (List<ObjectTileSpecification> value in ObjectTiles.Values)
		{
			foreach (ObjectTileSpecification item in value)
			{
				if (item.IsDoor() || item.IsCheckpoint() || item.IsTransition() || item.IsBreakableWall() || item.IsBoss() || item.IsTimespinner())
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	public bool IsAreaSolidWall(Rectangle tilesRectangle)
	{
		bool flag = true;
		for (int i = tilesRectangle.Y; i < tilesRectangle.Y + tilesRectangle.Height; i++)
		{
			for (int j = tilesRectangle.X; j < tilesRectangle.X + tilesRectangle.Width; j++)
			{
				Point point = new Point(j, i);
				if (MiddleTiles.ContainsKey(point) || point.X >= Width || point.Y >= Height)
				{
					continue;
				}
				bool flag2 = false;
				foreach (TileSwathSpecification value in TileSwaths.Values)
				{
					flag2 = new Rectangle(value.X, value.Y, value.Width, value.Height).Contains(point);
					if (flag2)
					{
						break;
					}
				}
				if (!flag2)
				{
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				break;
			}
		}
		return flag;
	}
}
