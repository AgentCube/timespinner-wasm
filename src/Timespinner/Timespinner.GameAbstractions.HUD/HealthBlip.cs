namespace Timespinner.GameAbstractions.HUD;

internal class HealthBlip
{
	private readonly int _originalWidth;

	private readonly float _startingX;

	public float MaxLife => 0.1f;

	public float Life { get; set; }

	public float StartingX => _startingX;

	public int Width => _originalWidth;

	public float LifePercentage => Life / MaxLife;

	public HealthBlip(float startX, int width)
	{
		_originalWidth = width;
		_startingX = startX;
	}
}
