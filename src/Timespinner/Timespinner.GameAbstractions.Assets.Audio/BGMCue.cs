using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace Timespinner.GameAbstractions.Assets.Audio;

public class BGMCue : AudioCue
{
	private readonly bool _hasIntro;

	private readonly double _introSongDuration;

	private readonly Song _song;

	private readonly SoundEffect _introSong;

	private bool _hasIntroFinishedPlaying;

	private float _introSongTimer;

	private SoundEffectInstance _introSongInstance;

	public bool IsPlaying { get; protected set; }

	public bool IsPaused { get; protected set; }

	public bool IsStopped { get; protected set; }

	internal bool HasIntro => _hasIntro;

	public Song Song => _song;

	public BGMCue(string filename, ContentManager content)
		: this(filename, content, hasIntro: false)
	{
	}

	public BGMCue(string filename, ContentManager content, bool hasIntro)
		: base(filename)
	{
		_hasIntro = hasIntro;
		string text = "Audio/BGM/" + base.Filename;
		if (hasIntro)
		{
			_introSong = content.Load<SoundEffect>(text + "_intro");
			_song = content.Load<Song>(text + "_loop");
			if (_introSong != null)
			{
				_introSongDuration = _introSong.Duration.TotalSeconds;
			}
		}
		else
		{
			_song = content.Load<Song>(text);
		}
	}

	~BGMCue()
	{
		_song.Dispose();
		if (_introSongInstance != null && !_introSongInstance.IsDisposed)
		{
			_introSongInstance.Dispose();
		}
		if (_introSong != null && !_introSong.IsDisposed)
		{
			_introSong.Dispose();
		}
	}

	public override void Play(float volume)
	{
		if (!(_song != null))
		{
			return;
		}
		MediaPlayer.Volume = volume;
		if (_hasIntro && _introSong != null)
		{
			MediaPlayer.Stop();
			_hasIntroFinishedPlaying = false;
			_introSongTimer = 0f;
			if (_introSongInstance != null)
			{
				_introSongInstance.Dispose();
			}
			_introSongInstance = _introSong.CreateInstance();
			_introSongInstance.Volume = volume;
			_introSongInstance.Play();
		}
		else
		{
			MediaPlayer.Play(_song);
			MediaPlayer.IsRepeating = true;
		}
		IsPlaying = true;
		IsPaused = false;
		IsStopped = false;
	}

	public void Pause()
	{
		MediaPlayer.Pause();
		IsPlaying = false;
		IsPaused = true;
		IsStopped = false;
	}

	public void Resume()
	{
		MediaPlayer.Resume();
		IsPlaying = true;
		IsPaused = false;
		IsStopped = false;
	}

	public void Stop()
	{
		MediaPlayer.Stop();
		IsPlaying = false;
		IsPaused = false;
		IsStopped = true;
		if (_introSong != null && !_hasIntroFinishedPlaying && _introSongInstance != null)
		{
			_introSongInstance.Stop();
		}
		_hasIntroFinishedPlaying = false;
	}

	public static void SetVolume(float volume)
	{
		MediaPlayer.Volume = volume;
	}

	public void SetIntroVolume(float volume)
	{
		if (_introSongInstance != null && !_introSongInstance.IsDisposed)
		{
			_introSongInstance.Volume = volume;
		}
	}

	public void Update(float delta)
	{
		if (_hasIntro && !_hasIntroFinishedPlaying && _song != null && _introSong != null)
		{
			_introSongTimer += delta;
			if (_introSongDuration - (double)_introSongTimer < 0.10000000149011612)
			{
				MediaPlayer.Play(_song);
				MediaPlayer.IsRepeating = true;
				_hasIntroFinishedPlaying = true;
			}
		}
	}
}
