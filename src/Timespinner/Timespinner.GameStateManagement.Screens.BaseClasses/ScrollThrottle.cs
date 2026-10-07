namespace Timespinner.GameStateManagement.Screens.BaseClasses;

internal class ScrollThrottle
{
	private const int InitialMinScrollRate = 5;

	private const int InitialMaxScrollRate = 20;

	private const int InitialScrollGrowthRate = 5;

	private int _scrollCounter;

	private int _scrollRate;

	public int MinScrollRate { get; set; }

	public int MaxScrollRate { get; set; }

	public int ScrollGrowthRate { get; set; }

	public ScrollThrottle()
	{
		MinScrollRate = 5;
		MaxScrollRate = 20;
		ScrollGrowthRate = 5;
		Reset();
	}

	public bool IsScrollReady()
	{
		bool result = false;
		_scrollCounter--;
		if (_scrollCounter <= 0)
		{
			_scrollRate -= ScrollGrowthRate;
			if (_scrollRate < MinScrollRate)
			{
				_scrollRate = MinScrollRate;
			}
			_scrollCounter = _scrollRate;
			result = true;
		}
		return result;
	}

	public void Reset()
	{
		_scrollRate = MaxScrollRate;
		_scrollCounter = _scrollRate;
	}

	internal void Ready()
	{
		_scrollCounter = 0;
	}
}
