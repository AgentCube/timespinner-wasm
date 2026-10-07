namespace Timespinner.Core.Specifications;

public class TileSwathSpecification
{
	public int ID { get; set; }

	public int X { get; set; }

	public int Y { get; set; }

	public int Width { get; set; }

	public int Height { get; set; }

	public TileSwathSpecification Duplicate()
	{
		TileSwathSpecification tileSwathSpecification = new TileSwathSpecification();
		tileSwathSpecification.ID = ID;
		tileSwathSpecification.X = X;
		tileSwathSpecification.Y = Y;
		tileSwathSpecification.Width = Width;
		tileSwathSpecification.Height = Height;
		return tileSwathSpecification;
	}
}
