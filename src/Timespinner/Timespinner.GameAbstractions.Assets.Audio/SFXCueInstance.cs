using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameStateManagement.Screens.InGame;

namespace Timespinner.GameAbstractions.Assets.Audio;

public class SFXCueInstance
{
	private enum EFadeType
	{
		PauseFade,
		StopFade
	}

	public enum ECueInstanceUpdateType
	{
		None,
		Stationary,
		Anchor,
		Protagonist
	}

	private const int PanScreenWidth = 800;

	private const float VolumeRampStart = 0.5f;

	private const float VolumeRampEnd = 0.9f;

	private const float ProtagonistVolumeRampDistance = 256f;

	private readonly bool _originalIsLooped;

	private readonly bool _isVoice;

	private readonly SoundEffectInstance _soundEffectInstance;

	private readonly SFXCue _sfxCue;

	private bool _hasStartedPlaying;

	private bool _isFinished;

	private bool _isFadingOut;

	private bool _isFadingIn;

	private bool _isWaitingToPlayWhenInRange;

	private bool _isScreenPaused;

	private EFadeType _fadeOutType;

	private float _baseCategoryVolume;

	private float _timeToFadeOut;

	private float _fadeOutTimer;

	private float _fadeInTimer;

	private float _timeToFadeIn;

	private float _fadeVolumeOutMultiplier = 1f;

	private float _fadeVolumeInMultiplier = 1f;

	private float _pan;

	private float _pitch;

	private float _volumeMultiplier = 1f;

	private float _rangeMultiplier = 1f;

	public bool IsFadingOut => _isFadingOut;

	public bool IsFadingIn => _isFadingIn;

	public bool IsPaused
	{
		get
		{
			if (!_isFinished && !_soundEffectInstance.IsDisposed)
			{
				return _soundEffectInstance.State == SoundState.Paused;
			}
			return false;
		}
	}

	public bool IsFinished
	{
		get
		{
			if (!_isFinished && !_soundEffectInstance.IsDisposed)
			{
				if (_soundEffectInstance.State == SoundState.Stopped)
				{
					return _hasStartedPlaying;
				}
				return false;
			}
			return true;
		}
	}

	public bool IsManuallyPaused { get; private set; }

	public bool IsFrozenPaused { get; private set; }

	internal bool IsVoice => _isVoice;

	internal float RangeMultiplier
	{
		get
		{
			return _rangeMultiplier;
		}
		set
		{
			if (value > 0f)
			{
				_rangeMultiplier = value;
			}
		}
	}

	public float Pitch
	{
		get
		{
			return _pitch;
		}
		set
		{
			if (_soundEffectInstance != null && !_soundEffectInstance.IsDisposed && (double)Math.Abs(value - _pitch) > 0.1)
			{
				_pitch = value;
				_soundEffectInstance.Pitch = value;
			}
		}
	}

	public float Pan
	{
		get
		{
			return _pan;
		}
		set
		{
			if (_soundEffectInstance != null && !_soundEffectInstance.IsDisposed && (double)Math.Abs(value - _pan) > 0.1)
			{
				_pan = value;
				_soundEffectInstance.Pan = value;
			}
		}
	}

	public float VolumeMultiplier
	{
		get
		{
			return _volumeMultiplier;
		}
		set
		{
			_volumeMultiplier = value;
			if (_soundEffectInstance != null && !_soundEffectInstance.IsDisposed)
			{
				_soundEffectInstance.Volume = _baseCategoryVolume * _fadeVolumeOutMultiplier * _fadeVolumeInMultiplier * _volumeMultiplier;
			}
		}
	}

	internal Point PositionOffset { get; set; }

	public string Name => _sfxCue.FileName;

	public ECueInstanceUpdateType UpdateType { get; set; }

	public Point SourcePosition { get; set; }

	public Camera2D Camera { get; set; }

	public Mobile Anchor { get; set; }

	public Mobile Protagonist { get; set; }

	public SFXCueInstance(SFXCue sourceCue, float categoryVolume, bool isLooped, bool isVoice)
	{
		_sfxCue = sourceCue;
		_baseCategoryVolume = categoryVolume;
		_originalIsLooped = isLooped;
		_isVoice = isVoice;
		_soundEffectInstance = _sfxCue.CreateInstance();
		_soundEffectInstance.Volume = _baseCategoryVolume;
		if (_originalIsLooped)
		{
			_soundEffectInstance.IsLooped = true;
		}
	}

	public bool CheckForRemoval()
	{
		bool result = false;
		if (IsFinished)
		{
			try
			{
				if (_soundEffectInstance != null && !_soundEffectInstance.IsDisposed && _soundEffectInstance.State != SoundState.Stopped)
				{
					_soundEffectInstance.Stop();
				}
				else
				{
					if (_soundEffectInstance != null)
					{
						_soundEffectInstance.Dispose();
					}
					result = true;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to stop or dispose cue when finished! {0}, {1}", Name, ex.Message);
			}
		}
		return result;
	}

	public void Play()
	{
		_fadeVolumeOutMultiplier = 1f;
		VolumeMultiplier = VolumeMultiplier;
		_soundEffectInstance.Play();
		_hasStartedPlaying = true;
	}

	public void Freeze()
	{
		if (_soundEffectInstance.State == SoundState.Playing)
		{
			_soundEffectInstance.Pause();
			IsFrozenPaused = true;
		}
	}

	public void Unfreeze()
	{
		if (IsFrozenPaused)
		{
			_soundEffectInstance.Resume();
			IsFrozenPaused = false;
		}
	}

	public void Pause()
	{
		if (_soundEffectInstance != null && !_soundEffectInstance.IsDisposed && _soundEffectInstance.State != SoundState.Stopped)
		{
			_soundEffectInstance.Pause();
			IsManuallyPaused = true;
		}
		_isWaitingToPlayWhenInRange = false;
	}

	public void Pause(float fadeTime)
	{
		_isFadingOut = true;
		_fadeOutType = EFadeType.PauseFade;
		_timeToFadeOut = fadeTime;
		_fadeOutTimer = 0f;
		_isWaitingToPlayWhenInRange = false;
	}

	public void Resume()
	{
		_fadeVolumeOutMultiplier = 1f;
		_soundEffectInstance.Resume();
		IsManuallyPaused = false;
		float num = 1f;
		if (_isFadingIn)
		{
			num = 0f;
		}
		_isFadingOut = false;
		_soundEffectInstance.Volume = _baseCategoryVolume * _volumeMultiplier * num;
	}

	public void FadeIn(float timeToFadeIn)
	{
		_isFadingIn = true;
		_fadeInTimer = 0f;
		_timeToFadeIn = timeToFadeIn;
		_fadeVolumeInMultiplier = 0f;
	}

	public void ScreenPause()
	{
		_soundEffectInstance.Pause();
		_isScreenPaused = true;
	}

	public void ScreenResume()
	{
		if (_hasStartedPlaying && _isScreenPaused && !IsFrozenPaused && !IsManuallyPaused)
		{
			_soundEffectInstance.Resume();
			_isScreenPaused = false;
		}
	}

	public void Stop()
	{
		_isWaitingToPlayWhenInRange = false;
		_isFinished = true;
		_isFadingIn = false;
		if (_soundEffectInstance != null && !_soundEffectInstance.IsDisposed)
		{
			try
			{
				_soundEffectInstance.Stop(immediate: true);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to immediately stop cue! {0}, {1}", Name, ex.Message);
			}
		}
	}

	public void Stop(float fadeTime)
	{
		if (_isWaitingToPlayWhenInRange)
		{
			Stop();
			return;
		}
		_isFadingOut = true;
		_fadeOutType = EFadeType.StopFade;
		_timeToFadeOut = fadeTime;
		_fadeOutTimer = 0f;
	}

	public void DecrementCueCount()
	{
		_sfxCue.DecrementActiveInstances();
	}

	public void Update(float delta)
	{
		if (_soundEffectInstance == null || _soundEffectInstance.IsDisposed)
		{
			return;
		}
		if (_isFadingOut)
		{
			_fadeOutTimer += delta;
			if (_fadeOutTimer >= _timeToFadeOut)
			{
				if (_fadeOutType == EFadeType.PauseFade)
				{
					Pause();
				}
				else
				{
					Stop();
				}
				_isFadingOut = false;
			}
			else
			{
				float num = _fadeOutTimer / _timeToFadeOut;
				_fadeVolumeOutMultiplier = 1f - num;
				VolumeMultiplier = VolumeMultiplier;
			}
		}
		if (_isFadingIn)
		{
			_fadeInTimer += delta;
			if (_fadeInTimer >= _timeToFadeIn)
			{
				_isFadingIn = false;
				_fadeVolumeInMultiplier = 1f;
				VolumeMultiplier = VolumeMultiplier;
			}
			else
			{
				float fadeVolumeInMultiplier = _fadeInTimer / _timeToFadeIn;
				_fadeVolumeInMultiplier = fadeVolumeInMultiplier;
				VolumeMultiplier = VolumeMultiplier;
			}
		}
		if (UpdateType == ECueInstanceUpdateType.None)
		{
			return;
		}
		if (Anchor != null && (UpdateType == ECueInstanceUpdateType.Anchor || UpdateType == ECueInstanceUpdateType.Protagonist))
		{
			SourcePosition = Anchor.Position;
		}
		if (SourcePosition != Point.Zero && Camera != null)
		{
			UpdateVolumeByCamera();
			if (UpdateType == ECueInstanceUpdateType.Protagonist && Protagonist != null)
			{
				float num2 = Math.Max((float)Math.Abs(SourcePosition.X - Protagonist.Position.X) / 256f, (float)Math.Abs(SourcePosition.Y - Protagonist.Position.Y) / 256f);
				VolumeMultiplier = MathHelper.Clamp(1f - num2, 0.1f, VolumeMultiplier);
			}
			bool flag = Anchor != null && Anchor.IsFrozen;
			if (_isWaitingToPlayWhenInRange && VolumeMultiplier > 0f && !IsFadingOut && !flag)
			{
				_isWaitingToPlayWhenInRange = false;
				Play();
			}
		}
	}

	internal void UpdateVolumeByCamera()
	{
		int num = SourcePosition.X + PositionOffset.X;
		int num2 = num - Camera.Position.X;
		float num3 = 400f * RangeMultiplier;
		float num4 = 800f * RangeMultiplier;
		float value = (float)num2 / num3;
		float value2 = (float)num2 / num4;
		float num5 = MathHelper.Clamp(value2, -1f, 1f);
		float pan = (float)Math.Cos((float)Math.PI / 2f * num5 - (float)Math.PI / 2f);
		Pan = pan;
		int num6 = SourcePosition.Y + PositionOffset.Y;
		float num7 = (float)GameplayScreen.SmallScreenSize.Y * RangeMultiplier;
		float value3 = (float)(num6 - Camera.Position.Y) / num7;
		float num8 = Math.Max(Math.Abs(value), Math.Abs(value3));
		if (num8 > 0.5f)
		{
			if (num8 >= 0.9f)
			{
				VolumeMultiplier = 0f;
			}
			else
			{
				VolumeMultiplier = (float)Math.Sin((float)Math.PI / 2f * (0.9f - num8) / 0.5f);
			}
		}
		else
		{
			VolumeMultiplier = 1f;
		}
	}

	internal static bool IsSourceAudible(Camera2D camera, Point source)
	{
		float value = (float)(source.X - camera.Position.X) / (float)GameplayScreen.SmallScreenSize.X;
		float value2 = (float)(source.Y - camera.Position.Y) / (float)GameplayScreen.SmallScreenSize.Y;
		return Math.Max(Math.Abs(value), Math.Abs(value2)) < 0.9f;
	}

	public void PlayWhenInRange()
	{
		_isWaitingToPlayWhenInRange = true;
		if (!_isFadingIn)
		{
			_isFadingIn = true;
			_fadeInTimer = 0f;
			_fadeVolumeInMultiplier = 0f;
			if (_timeToFadeIn <= 0f)
			{
				_timeToFadeIn = 0.1f;
			}
		}
	}

	internal void RefreshCategoryVolume(float voiceVolume, float sfxVolume)
	{
		_baseCategoryVolume = (IsVoice ? voiceVolume : sfxVolume);
		VolumeMultiplier = VolumeMultiplier;
	}

	internal static bool IsSFXAudible(Point emitPosition, Vector2 cameraPosition)
	{
		bool result = false;
		float value = (float)emitPosition.X - cameraPosition.X;
		float value2 = (float)emitPosition.Y - cameraPosition.Y;
		if (Math.Abs(value) < (float)GameplayScreen.SmallScreenSize.X && Math.Abs(value2) < (float)GameplayScreen.SmallScreenSize.Y)
		{
			result = true;
		}
		return result;
	}
}
