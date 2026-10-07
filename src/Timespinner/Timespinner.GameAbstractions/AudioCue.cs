namespace Timespinner.GameAbstractions;

public abstract class AudioCue
{
	private readonly string _filename;

	public string Filename => _filename;

	protected AudioCue(string filename)
	{
		_filename = filename;
	}

	public abstract void Play(float volume);
}
