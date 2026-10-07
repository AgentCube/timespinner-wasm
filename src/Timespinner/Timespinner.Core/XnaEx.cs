using Microsoft.Xna.Framework;

namespace Timespinner.Core;

public static class XnaEx
{
	public static int PlayerIndexToInt(PlayerIndex index)
	{
		int result = 1;
		switch (index)
		{
		case PlayerIndex.Two:
			result = 2;
			break;
		case PlayerIndex.Three:
			result = 3;
			break;
		case PlayerIndex.Four:
			result = 4;
			break;
		}
		return result;
	}

	public static int PlayerIndexToInt(PlayerIndex? index)
	{
		int result = 1;
		if (index.HasValue)
		{
			switch (index)
			{
			case PlayerIndex.Two:
				result = 2;
				break;
			case PlayerIndex.Three:
				result = 3;
				break;
			case PlayerIndex.Four:
				result = 4;
				break;
			}
		}
		return result;
	}

	public static PlayerIndex IntToPlayerIndex(int index)
	{
		PlayerIndex result = PlayerIndex.One;
		switch (index)
		{
		case 2:
			result = PlayerIndex.Two;
			break;
		case 3:
			result = PlayerIndex.Three;
			break;
		case 4:
			result = PlayerIndex.Four;
			break;
		}
		return result;
	}

	public static PlayerIndex GetFirstPlayerIndexExcept(PlayerIndex except)
	{
		if (except != 0)
		{
			return PlayerIndex.One;
		}
		return PlayerIndex.Two;
	}
}
