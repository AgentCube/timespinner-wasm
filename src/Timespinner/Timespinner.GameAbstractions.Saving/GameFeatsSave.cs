using System.Collections.Generic;

namespace Timespinner.GameAbstractions.Saving;

public class GameFeatsSave
{
	public Dictionary<string, int> FeatsData { get; set; }

	public GameFeatsSave()
	{
		FeatsData = new Dictionary<string, int>();
	}
}
