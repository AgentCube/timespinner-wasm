using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameAbstractions.Assets;

public class SFXCue : AudioCue
{
	private const int MaxInstances = 5;

	private readonly bool _isVoice;

	private readonly string _filename;

	private readonly ContentManager _content;

	private bool _isLoaded;

	private int _activeInstanceCount;

	private SoundEffect _soundEffect;

	internal bool IsLoaded => _isLoaded;

	public bool CanCreateInstance => _activeInstanceCount < 5;

	public bool IsVoice => _isVoice;

	public string FileName => _filename;

	public SFXCue(string filename, ContentManager content)
		: base(filename)
	{
		_filename = filename;
		_isVoice = filename.Contains("vo_");
		_content = content;
	}

	~SFXCue()
	{
		if (_isLoaded && _soundEffect != null)
		{
			_soundEffect.Dispose();
		}
	}

	internal void Load()
	{
		string text = (_isVoice ? "Audio/VO/" : "Audio/SFX/");
		_soundEffect = _content.Load<SoundEffect>(text + base.Filename);
		_isLoaded = true;
	}

	internal void MarkAsUnloaded()
	{
		_isLoaded = false;
		_soundEffect = null;
		_activeInstanceCount = 0;
	}

	public override void Play(float volume)
	{
		Play(volume, 0f, 0f);
	}

	public void Play(float volume, float pan, float pitch)
	{
		if (!_isLoaded)
		{
			Load();
		}
		if (_soundEffect != null)
		{
			try
			{
				_soundEffect.Play(volume, pitch, pan);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to play sound {0}, {1}", base.Filename, ex.Message);
			}
		}
	}

	public SoundEffectInstance CreateInstance()
	{
		if (!_isLoaded)
		{
			Load();
		}
		_activeInstanceCount++;
		return _soundEffect.CreateInstance();
	}

	public SFXCueInstance CreateAndPlay(float volume, bool isLooped, Point source, Camera2D camera, bool isVoice)
	{
		if (!_isLoaded)
		{
			Load();
		}
		SFXCueInstance sFXCueInstance = null;
		if (isLooped || CanCreateInstance)
		{
			bool flag = source != Point.Zero;
			SFXCueInstance sFXCueInstance2 = new SFXCueInstance(this, volume, isLooped, isVoice);
			sFXCueInstance2.SourcePosition = source;
			sFXCueInstance2.Camera = camera;
			sFXCueInstance2.UpdateType = (flag ? SFXCueInstance.ECueInstanceUpdateType.Stationary : SFXCueInstance.ECueInstanceUpdateType.None);
			sFXCueInstance = sFXCueInstance2;
			if (flag)
			{
				sFXCueInstance.Update(0f);
			}
			sFXCueInstance.Play();
		}
		return sFXCueInstance;
	}

	public SFXCueInstance Create(float volume, bool isLooped, bool isVoice)
	{
		return new SFXCueInstance(this, volume, isLooped, isVoice);
	}

	public void DecrementActiveInstances()
	{
		_activeInstanceCount--;
	}
}
