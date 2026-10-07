using System.IO;

namespace Timespinner.Core.Definitions;

public static class StreamEx
{
	private const int DummyStreamBufferSize = 81920;

	public static void CopyDummyStream(this Stream source, Stream destination)
	{
		byte[] array = new byte[81920];
		int count;
		while ((count = source.Read(array, 0, array.Length)) != 0)
		{
			destination.Write(array, 0, count);
		}
	}
}
