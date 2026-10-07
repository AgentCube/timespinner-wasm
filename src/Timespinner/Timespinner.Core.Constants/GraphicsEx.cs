using Microsoft.Xna.Framework;

namespace Timespinner.Core.Constants;

internal static class GraphicsEx
{
	private const int R3840_2160X = 3840;

	private const int R3840_2160Y = 2160;

	private const int R3200_1920X = 3200;

	private const int R3200_1920Y = 1920;

	private const int R2560_1440X = 2560;

	private const int R2560_1440Y = 1440;

	private const int R2000_1200X = 2000;

	private const int R2000_1200Y = 1200;

	private const int R1920_1080X = 1920;

	private const int R1920_1080Y = 1080;

	private const int R1600_960X = 1600;

	private const int R1600_960Y = 960;

	private const int R1280_720X = 1280;

	private const int R1280_720Y = 720;

	private const int R960_544X = 960;

	private const int R960_544Y = 544;

	private const int R800_480X = 800;

	private const int R800_480Y = 480;

	private const int R400_240X = 400;

	private const int R400_240Y = 240;

	private const int DefaultWidth = 1280;

	private const int DefaultHeight = 720;

	private const int DefaultZoom = 3;

	internal static Point GetGameResolutionFromType(EGameResolutionType resolution)
	{
		Point result = new Point(1280, 720);
		switch (resolution)
		{
		case EGameResolutionType.R1280_720:
			result = new Point(1280, 720);
			break;
		case EGameResolutionType.R1920_1080:
			result = new Point(1920, 1080);
			break;
		case EGameResolutionType.R2560_1440:
			result = new Point(2560, 1440);
			break;
		case EGameResolutionType.R2000_1200:
			result = new Point(2000, 1200);
			break;
		case EGameResolutionType.R3200_1920:
			result = new Point(3200, 1920);
			break;
		case EGameResolutionType.R3840_2160:
			result = new Point(3840, 2160);
			break;
		case EGameResolutionType.R1600_960:
			result = new Point(1600, 960);
			break;
		case EGameResolutionType.R960_544:
			result = new Point(960, 544);
			break;
		case EGameResolutionType.R800_480:
			result = new Point(800, 480);
			break;
		case EGameResolutionType.R400_240:
			result = new Point(400, 240);
			break;
		}
		return result;
	}

	internal static EGameResolutionType GetNearestResolutionTypeFromSize(Point size)
	{
		int num = (int)((float)size.X * 1.1f);
		int num2 = (int)((float)size.Y * 1.1f);
		if (num >= 3840 && num2 >= 2160)
		{
			return EGameResolutionType.R3840_2160;
		}
		if (num >= 3200 && num2 >= 1920)
		{
			return EGameResolutionType.R3200_1920;
		}
		if (num >= 2560 && num2 >= 1440)
		{
			return EGameResolutionType.R2560_1440;
		}
		if (num >= 2000 && num2 >= 1200)
		{
			return EGameResolutionType.R2000_1200;
		}
		if (num >= 1920 && num2 >= 1080)
		{
			return EGameResolutionType.R1920_1080;
		}
		if (num >= 1600 && num2 >= 960)
		{
			return EGameResolutionType.R1600_960;
		}
		if (num >= 1280 && num2 >= 720)
		{
			return EGameResolutionType.R1280_720;
		}
		if (num >= 960 && num2 >= 544)
		{
			return EGameResolutionType.R960_544;
		}
		if (num >= 800 && num2 >= 480)
		{
			return EGameResolutionType.R800_480;
		}
		return EGameResolutionType.R400_240;
	}

	internal static int GetGameZoomFromType(EGameResolutionType resolution)
	{
		int result = 3;
		switch (resolution)
		{
		case EGameResolutionType.R1280_720:
			result = 3;
			break;
		case EGameResolutionType.R1920_1080:
			result = 4;
			break;
		case EGameResolutionType.R2560_1440:
			result = 6;
			break;
		case EGameResolutionType.R2000_1200:
			result = 5;
			break;
		case EGameResolutionType.R3200_1920:
			result = 8;
			break;
		case EGameResolutionType.R3840_2160:
			result = 9;
			break;
		case EGameResolutionType.R1600_960:
			result = 4;
			break;
		case EGameResolutionType.R960_544:
			result = 2;
			break;
		case EGameResolutionType.R800_480:
			result = 2;
			break;
		case EGameResolutionType.R400_240:
			result = 1;
			break;
		}
		return result;
	}
}
