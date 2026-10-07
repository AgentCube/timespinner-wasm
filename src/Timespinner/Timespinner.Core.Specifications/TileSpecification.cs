using System;
using System.Collections.Generic;

namespace Timespinner.Core.Specifications;

[Serializable]
public class TileSpecification
{
	public const int InvisibleSolidTileIndex = 512;

	public const int EventTiles_SolidIndex = 53;

	public const int TilesetAndSolidInvisibleTileSize = 523;

	public bool IsFlippedHorizontally { get; set; }

	public bool IsFlippedVertically { get; set; }

	public ETileLayerType Layer { get; set; }

	public int ID { get; set; }

	public int X { get; set; }

	public int Y { get; set; }

	public int Argument { get; set; }

	public List<SwitchSpecification> Switches { get; set; }

	public TileSpecification()
	{
		Switches = new List<SwitchSpecification>();
	}

	public virtual TileSpecification Duplicate()
	{
		TileSpecification tileSpecification = new TileSpecification();
		tileSpecification.Layer = Layer;
		tileSpecification.X = X;
		tileSpecification.Y = Y;
		tileSpecification.ID = ID;
		tileSpecification.IsFlippedHorizontally = IsFlippedHorizontally;
		tileSpecification.IsFlippedVertically = IsFlippedVertically;
		TileSpecification tileSpecification2 = tileSpecification;
		foreach (SwitchSpecification @switch in Switches)
		{
			tileSpecification2.Switches.Add(@switch.Duplicate());
		}
		return tileSpecification2;
	}
}
