using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Timespinner.Core;
using Timespinner.GameAbstractions.Assets;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Saving;

namespace Timespinner.GameAbstractions;

public class Jukebox
{
	private const float GlobalVolumeMultiplier = 0.5f;

	private const float MusicVolumeMultiplier = 1f;

	private const float VoiceVolumeMultiplier = 0.5f;

	private const float EffectsVolumeMultiplier = 1f;

	private const float DefaultVariantPitchMin = -0.15f;

	private const float DefaultVariantPitchMax = 0.15f;

	private const float DefaultVariantVolumeMin = 0.85f;

	private const float DefaultVariantVolumeMax = 1.15f;

	private const float TimeToAdjustMusicVolume = 2f;

	private const float PlaySongFadeInTime = 1f;

	private const string ContentRoot = "Content";

	private readonly AudioEngine _audioEngine;

	private readonly WaveBank _bgmBank;

	private readonly SoundBank _soundBank;

	private readonly ContentManager _sfxContent;

	private readonly ContentManager _uiSfxContent;

	private readonly HashSet<int> _cuesPlayedThisFrame = new HashSet<int>();

	private readonly List<SFXCueInstance> _sfxCueInstances = new List<SFXCueInstance>();

	private readonly Dictionary<string, SFXCue> _sfxCues = new Dictionary<string, SFXCue>();

	private static float _pitchRangeMin;

	private static float _pitchRangeMax;

	private static float _volumeRangeMin = 1f;

	private static float _volumeRangeMax = 1f;

	private float _masterVolume = 2f;

	private float _globalMusicVolume = 1f;

	private float _globalVoiceVolume = 1f;

	private float _globalEffectsVolume = 1f;

	private EBGM _currentSongEnum;

	private AudioCategory _musicCategory;

	private Cue _currentSong;

	private bool _isMusicFading;

	private float _musicVolume = 1f;

	private float _targetFadeVolume;

	private float _startingFadeVolume;

	private float _fadeOutTime;

	private float _fadeOutTimer;

	private static Random _random;

	public bool DoesNotPlaySounds { get; private set; }

	public EBGM CurrentSongEnum => _currentSongEnum;

	public float MasterVolume => _masterVolume * 0.5f;

	public float EffectsVolume => 0.5f * MasterVolume * _globalEffectsVolume * 1f;

	public float VoiceVolume => 0.5f * MasterVolume * _globalVoiceVolume * 0.5f;

	public float MusicVolume => 0.5f * MasterVolume * _globalMusicVolume * _musicVolume * 1f;

	public Jukebox(bool shouldLoad, GameServiceContainer services)
	{
		_random = new Random();
		DoesNotPlaySounds = !shouldLoad;
		if (shouldLoad)
		{
			_uiSfxContent = new ContentManager(services, "Content");
			_sfxContent = new ContentManager(services, "Content");
			PopulateAndLoadUICues();
			PopulateSFXCues();
			_audioEngine = new AudioEngine("Content/Audio/TimespinnerAudio.xgs");
			_soundBank = new SoundBank(_audioEngine, "Content/Audio/SoundBank.xsb");
			_bgmBank = new WaveBank(_audioEngine, "Content/Audio/BGM.xwb", 0, 8);
			_musicCategory = _audioEngine.GetCategory("Music");
		}
	}

	internal void UnloadContent()
	{
		if (_audioEngine != null)
		{
			_audioEngine.Dispose();
		}
		if (_uiSfxContent != null)
		{
			_uiSfxContent.Unload();
		}
		if (_sfxContent != null)
		{
			_sfxContent.Unload();
		}
	}

	private void PopulateAndLoadUICues()
	{
		ESFX[] array = new ESFX[14]
		{
			ESFX.MenuBuy,
			ESFX.MenuCancel,
			ESFX.MenuEquip,
			ESFX.MenuError,
			ESFX.MenuHeal,
			ESFX.MenuMove,
			ESFX.MenuSelect,
			ESFX.MenuSell,
			ESFX.DialogClose,
			ESFX.DialogOpen,
			ESFX.DialogTic,
			ESFX.CharacterLevelUp,
			ESFX.CharacterStatUp,
			ESFX.AreaTitle
		};
		ESFX[] array2 = array;
		foreach (ESFX cue in array2)
		{
			AddCuesFromCueType(cue, _uiSfxContent, shouldLoad: true);
		}
	}

	private void PopulateSFXCues()
	{
		ESFX[] enumValues = EnumExtensions.GetEnumValues<ESFX>();
		foreach (ESFX eSFX in enumValues)
		{
			if (eSFX != 0)
			{
				AddCuesFromCueType(eSFX, _sfxContent, shouldLoad: false);
			}
		}
	}

	private void AddCuesFromCueType(ESFX cue, ContentManager content, bool shouldLoad)
	{
		List<string> sFXNamesFromEnum = GetSFXNamesFromEnum(cue);
		foreach (string item in sFXNamesFromEnum)
		{
			if (item != null && item != "Unknown" && !_sfxCues.ContainsKey(item))
			{
				SFXCue sFXCue = new SFXCue(item, content);
				_sfxCues[item] = sFXCue;
				if (shouldLoad)
				{
					sFXCue.Load();
				}
			}
		}
	}

	internal void LoadAllUnloadedCues()
	{
		if (OperatingSystem.IsBrowser())
		{
			return; // SFXCue lazily loads on demand when played or instantiated
		}
		foreach (SFXCue value in _sfxCues.Values)
		{
			if (!value.IsLoaded)
			{
				value.Load();
			}
		}
	}

	public void PlayCue(ESFX sound)
	{
		if (DoesNotPlaySounds)
		{
			return;
		}
		if (_cuesPlayedThisFrame.Contains((int)sound))
		{
			return;
		}
		_cuesPlayedThisFrame.Add((int)sound);
		try
		{
			string sFXNameFromEnum = GetSFXNameFromEnum(sound);
			if (sFXNameFromEnum != null && _sfxCues.ContainsKey(sFXNameFromEnum))
			{
				float pitch = 0f;
				SFXCue sFXCue = _sfxCues[sFXNameFromEnum];
				if (Math.Abs(_pitchRangeMin) > 0.001f && Math.Abs(_pitchRangeMax) > 0.001f)
				{
					pitch = MathHelper.Lerp(_pitchRangeMin, _pitchRangeMax, (float)_random.NextDouble());
				}
				float num = 1f;
				if (Math.Abs(_volumeRangeMin - 1f) > 0.001f || Math.Abs(_volumeRangeMin - 1f) > 0.001f)
				{
					num = MathHelper.Lerp(_volumeRangeMin, _volumeRangeMax, (float)_random.NextDouble());
				}
				float volume = MathHelper.Clamp((sFXCue.IsVoice ? VoiceVolume : EffectsVolume) * num, 0f, 1f);
				sFXCue.Play(volume, 0f, pitch);
			}
		}
		catch
		{
		}
	}

	internal SFXCueInstance CreateCue(ESFX sound, bool isLooped, bool shouldPlay, Point source, Camera2D camera)
	{
		SFXCueInstance sFXCueInstance = null;
		if (!DoesNotPlaySounds)
		{
			if (!shouldPlay || isLooped || !_cuesPlayedThisFrame.Contains((int)sound))
			{
				try
				{
					string sFXNameFromEnum = GetSFXNameFromEnum(sound);
					if (sFXNameFromEnum != null && _sfxCues.ContainsKey(sFXNameFromEnum))
					{
						SFXCue sFXCue = _sfxCues[sFXNameFromEnum];
						float volume = (sFXCue.IsVoice ? VoiceVolume : EffectsVolume);
						sFXCueInstance = (shouldPlay ? sFXCue.CreateAndPlay(volume, isLooped, source, camera, sFXCue.IsVoice) : sFXCue.Create(volume, isLooped, sFXCue.IsVoice));
						if (sFXCueInstance != null)
						{
							_sfxCueInstances.Add(sFXCueInstance);
							sFXCueInstance.SourcePosition = source;
							sFXCueInstance.Camera = camera;
							if (shouldPlay && !isLooped)
							{
								_cuesPlayedThisFrame.Add((int)sound);
							}
						}
					}
				}
				catch
				{
				}
			}
		}
		return sFXCueInstance;
	}

	public void PlaySong(EBGM song)
	{
		PlaySong(song, shouldForceRestart: false, shouldImmediatelyStopPreviousSong: false);
	}

	public void PlaySong(EBGM song, bool shouldForceRestart, bool shouldImmediatelyStopPreviousSong)
	{
		bool flag = _musicVolume <= 0f || _targetFadeVolume <= 0f;
		shouldImmediatelyStopPreviousSong = shouldImmediatelyStopPreviousSong || flag;
		if ((_isMusicFading || _musicVolume < 1f) && !shouldImmediatelyStopPreviousSong)
		{
			_isMusicFading = true;
			_targetFadeVolume = 1f;
			_startingFadeVolume = _musicVolume;
			_fadeOutTime = 1f;
			_fadeOutTimer = 0f;
		}
		else
		{
			_isMusicFading = false;
			_musicVolume = 1f;
		}
		if (DoesNotPlaySounds)
		{
			return;
		}
		if (song == EBGM.None)
		{
			if (_currentSongEnum != 0 && _currentSong != null)
			{
				StopSong();
			}
			return;
		}
		_musicCategory.SetVolume(MusicVolume * _musicVolume);
		if (_currentSongEnum != song)
		{
			string bGMNameFromEnum = GetBGMNameFromEnum(song);
			if (bGMNameFromEnum != null)
			{
				if (shouldImmediatelyStopPreviousSong)
				{
					StopSong();
				}
				_currentSongEnum = song;
				_currentSong = _soundBank.GetCue(bGMNameFromEnum);
				_currentSong.Play();
			}
		}
		else
		{
			if (_currentSong == null)
			{
				return;
			}
			if (shouldForceRestart)
			{
				string bGMNameFromEnum2 = GetBGMNameFromEnum(song);
				if (bGMNameFromEnum2 != null)
				{
					_currentSongEnum = song;
					_currentSong = _soundBank.GetCue(bGMNameFromEnum2);
					_currentSong.Play();
				}
			}
			else if (_currentSong.IsPaused)
			{
				_currentSong.Resume();
			}
			else if (_currentSong.IsStopped)
			{
				_currentSong.Play();
			}
		}
	}

	public void PauseSong()
	{
		if (_currentSong != null && _currentSong.IsPlaying)
		{
			_currentSong.Pause();
		}
	}

	public void ResumeSong()
	{
		if (_currentSong != null && _currentSong.IsPaused)
		{
			_currentSong.Resume();
		}
	}

	public void StopSong()
	{
		if (_currentSong == null)
		{
			return;
		}
		try
		{
			if (_currentSong.IsPlaying)
			{
				_currentSong.Stop(AudioStopOptions.AsAuthored);
				_currentSongEnum = EBGM.None;
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine("Failed to stop song! " + ex);
		}
	}

	public void FadeOutSong(float howFast)
	{
		_targetFadeVolume = 0f;
		_startingFadeVolume = _musicVolume;
		_fadeOutTime = howFast;
		_fadeOutTimer = 0f;
		_isMusicFading = true;
	}

	public void FadeInSong(float howFast)
	{
		_targetFadeVolume = 1f;
		_startingFadeVolume = _musicVolume;
		_fadeOutTime = howFast;
		_fadeOutTimer = 0f;
		_isMusicFading = true;
	}

	internal void AdjustMusicVolume(float targetVolume)
	{
		if (Math.Abs(_musicVolume - targetVolume) > 0.1f)
		{
			_targetFadeVolume = targetVolume;
			_startingFadeVolume = _musicVolume;
			_fadeOutTime = 2f;
			_fadeOutTimer = 0f;
			_isMusicFading = true;
		}
	}

	public void StopAllSFX()
	{
		foreach (SFXCueInstance sfxCueInstance in _sfxCueInstances)
		{
			sfxCueInstance.Stop();
		}
	}

	public void FadeOutAllSFX(float timeToFade)
	{
		foreach (SFXCueInstance sfxCueInstance in _sfxCueInstances)
		{
			sfxCueInstance.Stop(timeToFade);
		}
	}

	internal void RefreshSFXVolumes()
	{
		foreach (SFXCueInstance sfxCueInstance in _sfxCueInstances)
		{
			sfxCueInstance?.RefreshCategoryVolume(VoiceVolume, EffectsVolume);
		}
	}

	public void AdjustGlobalVolume(float newVolume, EVolumeCategory category)
	{
		switch (category)
		{
		case EVolumeCategory.Master:
			_masterVolume = newVolume;
			_musicCategory.SetVolume(MusicVolume);
			break;
		case EVolumeCategory.Music:
			_globalMusicVolume = newVolume;
			_musicCategory.SetVolume(MusicVolume);
			break;
		case EVolumeCategory.Voice:
			_globalVoiceVolume = newVolume;
			break;
		case EVolumeCategory.SFX:
			_globalEffectsVolume = newVolume;
			break;
		}
	}

	public List<float> GetGlobalVolumes()
	{
		List<float> list = new List<float>();
		list.Add(_masterVolume);
		list.Add(_globalMusicVolume);
		list.Add(_globalVoiceVolume);
		list.Add(_globalEffectsVolume);
		return list;
	}

	public void Update(float delta)
	{
		if (_audioEngine != null)
		{
			_audioEngine.Update();
		}
		if (_isMusicFading)
		{
			_fadeOutTimer += delta;
			if (_fadeOutTimer >= _fadeOutTime)
			{
				_isMusicFading = false;
				_musicVolume = _targetFadeVolume;
			}
			else
			{
				_musicVolume = MathHelper.Lerp(_startingFadeVolume, _targetFadeVolume, _fadeOutTimer / _fadeOutTime);
			}
			_musicCategory.SetVolume(MusicVolume * _musicVolume);
		}
		UpdateSFXCueInstances(delta);
	}

	internal void UnfadeMusicVolume()
	{
		_musicVolume = 1f;
		_musicCategory.SetVolume(MusicVolume * _musicVolume);
	}

	private void UpdateSFXCueInstances(float delta)
	{
		for (int num = _sfxCueInstances.Count - 1; num >= 0; num--)
		{
			SFXCueInstance sFXCueInstance = _sfxCueInstances[num];
			sFXCueInstance.Update(delta);
			if (sFXCueInstance.CheckForRemoval())
			{
				sFXCueInstance.DecrementCueCount();
				_sfxCueInstances.RemoveAt(num);
			}
		}
		if (_cuesPlayedThisFrame.Count > 0)
		{
			_cuesPlayedThisFrame.Clear();
		}
	}

	public void PauseScreenSounds()
	{
		foreach (SFXCueInstance sfxCueInstance in _sfxCueInstances)
		{
			sfxCueInstance.ScreenPause();
		}
	}

	public void UnpauseScreenSounds()
	{
		foreach (SFXCueInstance sfxCueInstance in _sfxCueInstances)
		{
			sfxCueInstance.ScreenResume();
		}
	}

	private static string GetSFXNameFromEnum(ESFX sound)
	{
		string text = "Unknown";
		_pitchRangeMin = 0f;
		_pitchRangeMax = 0f;
		_volumeRangeMin = 1f;
		_volumeRangeMax = 1f;
		List<string> sFXNamesFromEnum = GetSFXNamesFromEnum(sound);
		int count = sFXNamesFromEnum.Count;
		if (count > 0)
		{
			text = ((count > 1) ? sFXNamesFromEnum[_random.Next(count)] : sFXNamesFromEnum[0]);
		}
		if (text == "Unknown")
		{
			text = null;
		}
		return text;
	}

	private static List<string> GetSFXNamesFromEnum(ESFX sound)
	{
		List<string> list = new List<string>();
		bool flag = false;
		switch (sound)
		{
		case ESFX.AmbientConveyorBeltLoop:
			list.Add("Foley/sfx_ambient_conveyor_belt_loop");
			break;
		case ESFX.AmbientCricketsLoop:
			list.Add("Foley/sfx_ambient_crickets_loop");
			break;
		case ESFX.AmbientLazerLoop:
			list.Add("Foley/sfx_ambient_lazer_loop");
			break;
		case ESFX.AmbientTestTubeLoop:
			list.Add("Foley/sfx_ambient_test_tube_loop");
			break;
		case ESFX.AmbientWaterfallLoop:
			list.Add("Foley/sfx_ambient_waterfall_loop");
			break;
		case ESFX.AreaTitle:
			list.Add("UI/sfx_area_title");
			break;
		case ESFX.BossBirdAuraPush:
			list.Add("Boss/03_Bird/sfx_boss_bird_aura_push");
			break;
		case ESFX.BossBirdAuraWallHit:
			list.Add("Boss/03_Bird/sfx_boss_bird_aura_wallhit");
			break;
		case ESFX.BossBirdAuraZone:
			list.Add("Boss/03_Bird/sfx_boss_bird_aura_zone");
			break;
		case ESFX.BossBirdAuraBlastCast:
			list.Add("Boss/03_Bird/sfx_boss_bird_aurablast_cast");
			break;
		case ESFX.BossBirdAuraBlastCharge:
			list.Add("Boss/03_Bird/sfx_boss_bird_aurablast_charge");
			break;
		case ESFX.BossBirdChainAttackA:
			list.Add("Boss/03_Bird/sfx_boss_bird_chain_attack_01");
			flag = true;
			break;
		case ESFX.BossBirdChainAttackB:
			list.Add("Boss/03_Bird/sfx_boss_bird_chain_attack_02");
			flag = true;
			break;
		case ESFX.BossBirdChainSummon:
			list.Add("Boss/03_Bird/sfx_boss_bird_chain_summon");
			break;
		case ESFX.BossBirdDeath:
			list.Add("Boss/03_Bird/sfx_boss_bird_death");
			break;
		case ESFX.BossBirdVomitPrep:
			list.Add("Boss/03_Bird/sfx_boss_bird_vomit_prep");
			break;
		case ESFX.BossCantoranDeath:
			list.Add("Boss/16_Cantoran/sfx_boss_cantoran_death");
			break;
		case ESFX.BossDeathFinalHit:
			list.Add("Boss/00_Misc/sfx_boss_death_finalhit");
			break;
		case ESFX.BossDeathSand:
			list.Add("Boss/00_Misc/sfx_boss_death_sand");
			break;
		case ESFX.BossDeathSandRestore:
			list.Add("Boss/00_Misc/sfx_boss_death_sand_restore");
			break;
		case ESFX.BossDemonCrush:
			list.Add("Boss/04_Demon/sfx_boss_demon_crush");
			break;
		case ESFX.BossDemonDeath:
			list.Add("Boss/04_Demon/sfx_boss_demon_death");
			break;
		case ESFX.BossDemonIncSuccAppear:
			list.Add("Boss/04_Demon/sfx_boss_demon_incsucc_appear");
			break;
		case ESFX.BossDemonIncSuccDisappear:
			list.Add("Boss/04_Demon/sfx_boss_demon_incsucc_disappear");
			break;
		case ESFX.BossDemonIncSuccFinalHit:
			list.Add("Boss/04_Demon/sfx_boss_demon_incsucc_finalhit");
			break;
		case ESFX.BossDemonIncSuccLaughFLoop:
			list.Add("Boss/04_Demon/sfx_boss_demon_incsucc_laugh_f_loop");
			break;
		case ESFX.BossDemonIncSuccLaughFSingle:
			list.Add("Boss/04_Demon/sfx_boss_demon_incsucc_laugh_f_single");
			break;
		case ESFX.BossDemonIncSuccLaughMLoop:
			list.Add("Boss/04_Demon/sfx_boss_demon_incsucc_laugh_m_loop");
			break;
		case ESFX.BossDemonIncSuccLaughMSingle:
			list.Add("Boss/04_Demon/sfx_boss_demon_incsucc_laugh_m_single");
			break;
		case ESFX.BossDemonIncSuccWaveAttack:
			list.Add("Boss/04_Demon/sfx_boss_demon_incsucc_waveattack");
			break;
		case ESFX.BossDemonPhase2Intro:
			list.Add("Boss/04_Demon/sfx_boss_demon_phase2_intro");
			break;
		case ESFX.BossDemonPuppetLeftCharge:
			list.Add("Boss/04_Demon/sfx_boss_demon_puppet_left_charge");
			break;
		case ESFX.BossDemonPuppetLeftDeath:
			list.Add("Boss/04_Demon/sfx_boss_demon_puppet_left_death");
			break;
		case ESFX.BossDemonPuppetLeftRevive:
			list.Add("Boss/04_Demon/sfx_boss_demon_puppet_left_revive");
			break;
		case ESFX.BossDemonPuppetRightCharge:
			list.Add("Boss/04_Demon/sfx_boss_demon_puppet_right_charge");
			break;
		case ESFX.BossDemonPuppetRightDeath:
			list.Add("Boss/04_Demon/sfx_boss_demon_puppet_right_death");
			break;
		case ESFX.BossDemonPuppetRightRevive:
			list.Add("Boss/04_Demon/sfx_boss_demon_puppet_right_revive");
			break;
		case ESFX.BossDemonSawLoop:
			list.Add("Boss/04_Demon/sfx_boss_demon_saw_loop");
			break;
		case ESFX.BossDemonSawStart:
			list.Add("Boss/04_Demon/sfx_boss_demon_saw_start");
			break;
		case ESFX.BossDemonSpikesExtend:
			list.Add("Boss/04_Demon/sfx_boss_demon_spikes_extend");
			break;
		case ESFX.BossDemonSpikesRetract:
			list.Add("Boss/04_Demon/sfx_boss_demon_spikes_retract");
			break;
		case ESFX.BossDemonSpotlight:
			list.Add("Boss/04_Demon/sfx_boss_demon_spotlight");
			break;
		case ESFX.BossDemonSpotlightLoop:
			list.Add("Boss/04_Demon/sfx_boss_demon_spotlight_loop");
			break;
		case ESFX.BossEmperorArmOut:
			list.Add("Boss/08_Emperor/sfx_boss_emperor_arm_out");
			break;
		case ESFX.BossEmperorBladesLoop:
			list.Add("Boss/08_Emperor/sfx_boss_emperor_blades_loop");
			break;
		case ESFX.BossEmperorBladeStart:
			list.Add("Boss/08_Emperor/sfx_boss_emperor_blades_start");
			break;
		case ESFX.BossEmperorBlueMeleeCast:
			list.Add("Boss/08_Emperor/sfx_boss_emperor_bluemelee_cast");
			break;
		case ESFX.BossEmperorBlueMeleeCastRight:
			list.Add("Boss/08_Emperor/sfx_boss_emperor_bluemelee_cast_right");
			break;
		case ESFX.BossEmperorChargeStart:
			list.Add("Boss/08_Emperor/sfx_boss_emperor_attack_charge");
			break;
		case ESFX.BossEmperorChargeLoop:
			list.Add("Boss/08_Emperor/sfx_boss_emperor_attack_charged_loop");
			break;
		case ESFX.BossEmperorChargeEnd:
			list.Add("Boss/08_Emperor/sfx_boss_emperor_charge_shoot");
			break;
		case ESFX.BossEmperorDeath:
			list.Add("Boss/08_Emperor/sfx_boss_emperor_death");
			break;
		case ESFX.BossEmperorDisappear:
			list.Add("Boss/08_Emperor/sfx_boss_emperor_disappear");
			flag = true;
			break;
		case ESFX.BossEmperorEnergyBall:
			list.Add("Boss/08_Emperor/sfx_boss_emperor_energy_ball_01");
			list.Add("Boss/08_Emperor/sfx_boss_emperor_energy_ball_02");
			list.Add("Boss/08_Emperor/sfx_boss_emperor_energy_ball_03");
			flag = true;
			break;
		case ESFX.BossEmperorFightBegin:
			list.Add("Boss/08_Emperor/sfx_boss_emperor_fightbegin");
			break;
		case ESFX.BossEmperorOrbsPreAttack:
			list.Add("Boss/08_Emperor/sfx_boss_emperor_orbs_preattack");
			break;
		case ESFX.BossEmperorOrbsShift:
			list.Add("Boss/08_Emperor/sfx_boss_emperor_orbs_shift");
			break;
		case ESFX.BossEmperorOrbsForm:
			list.Add("Boss/08_Emperor/sfx_boss_emperor_orbsform");
			break;
		case ESFX.BossEmperorOrbsVanish:
			list.Add("Boss/08_Emperor/sfx_boss_emperor_orbs_vanish");
			break;
		case ESFX.BossEmperorReappear:
			list.Add("Boss/08_Emperor/sfx_boss_emperor_reappear");
			flag = true;
			break;
		case ESFX.BossEyeClose:
			list.Add("Boss/02_Eye/sfx_boss_eye_close");
			break;
		case ESFX.BossEyeFlamethrower:
			list.Add("Boss/02_Eye/sfx_boss_eye_flamethrower");
			break;
		case ESFX.BossEyeFlamethrowerEnd:
			list.Add("Boss/02_Eye/sfx_boss_eye_flamethrower_end");
			break;
		case ESFX.BossEyeFlamethrowerStart:
			list.Add("Boss/02_Eye/sfx_boss_eye_flamethrower_start");
			break;
		case ESFX.BossEyeMatch:
			list.Add("Boss/02_Eye/sfx_boss_eye_match");
			break;
		case ESFX.BossEyeOpen:
			list.Add("Boss/02_Eye/sfx_boss_eye_open");
			break;
		case ESFX.BossEyeProjectileDestroy:
			list.Add("Boss/02_Eye/sfx_boss_eye_projectile_destroy");
			break;
		case ESFX.BossEyeProjectileFly:
			list.Add("Boss/02_Eye/sfx_boss_eye_projectile_fly");
			break;
		case ESFX.BossEyePulse:
			list.Add("Boss/02_Eye/sfx_boss_eye_pulse");
			break;
		case ESFX.BossEyeRam:
			list.Add("Boss/02_Eye/sfx_boss_eye_ram");
			break;
		case ESFX.BossEyeRamCrash:
			list.Add("Boss/02_Eye/sfx_boss_eye_ram_crash");
			break;
		case ESFX.BossEyeShoot:
			list.Add("Boss/02_Eye/sfx_boss_eye_shoot");
			flag = true;
			break;
		case ESFX.BossEyeSpin:
			list.Add("Boss/02_Eye/sfx_boss_eye_spin");
			break;
		case ESFX.BossMawBoneBreak:
			list.Add("Boss/06_Maw/sfx_boss_maw_bone_break");
			break;
		case ESFX.BossMawBoneTwitch:
			list.Add("Boss/06_Maw/sfx_boss_maw_bone_twitch");
			break;
		case ESFX.BossMawDeathRoar:
			list.Add("Boss/06_Maw/sfx_boss_maw_death_roar");
			break;
		case ESFX.BossMawLazerLoop:
			list.Add("Boss/06_Maw/sfx_boss_maw_lazer_loop_loop");
			break;
		case ESFX.BossMawLazerStart:
			list.Add("Boss/06_Maw/sfx_boss_maw_lazer_loop_start");
			break;
		case ESFX.BossMawMouthClose:
			list.Add("Boss/06_Maw/sfx_boss_maw_mouth_close");
			break;
		case ESFX.BossMawMouthOpen:
			list.Add("Boss/06_Maw/sfx_boss_maw_mouth_open");
			break;
		case ESFX.BossMawRoar:
			list.Add("Boss/06_Maw/sfx_boss_maw_roar");
			break;
		case ESFX.BossMawSpike:
			list.Add("Boss/06_Maw/sfx_boss_maw_spike_01");
			list.Add("Boss/06_Maw/sfx_boss_maw_spike_02");
			list.Add("Boss/06_Maw/sfx_boss_maw_spike_03");
			list.Add("Boss/06_Maw/sfx_boss_maw_spike_04");
			flag = true;
			break;
		case ESFX.BossMawSpikeTeethCast:
			list.Add("Boss/06_Maw/sfx_boss_maw_spiketeeth_cast");
			break;
		case ESFX.BossMawSpikeWaveFlame:
			list.Add("Boss/06_Maw/sfx_boss_maw_spikewave_flame");
			break;
		case ESFX.BossMawVacuum:
			list.Add("Boss/06_Maw/sfx_boss_maw_vacuum");
			break;
		case ESFX.BossNightmareApocalypse:
			list.Add("Boss/12_Nightmare/sfx_boss_nightmare_apocalypse_01");
			list.Add("Boss/12_Nightmare/sfx_boss_nightmare_apocalypse_02");
			list.Add("Boss/12_Nightmare/sfx_boss_nightmare_apocalypse_03");
			list.Add("Boss/12_Nightmare/sfx_boss_nightmare_apocalypse_04");
			break;
		case ESFX.BossNightmareBombFall:
			list.Add("Boss/12_Nightmare/sfx_boss_nightmare_bomb_fall");
			flag = true;
			break;
		case ESFX.BossNightmareBombShoot:
			list.Add("Boss/12_Nightmare/sfx_boss_nightmare_bomb_shoot");
			flag = true;
			break;
		case ESFX.BossNightmareBreath:
			list.Add("Boss/12_Nightmare/sfx_boss_nightmare_breath");
			flag = true;
			break;
		case ESFX.BossNightmareCackle:
			list.Add("Boss/12_Nightmare/sfx_boss_nightmare_cackle");
			break;
		case ESFX.BossNightmareDeathCry:
			list.Add("Boss/12_Nightmare/sfx_boss_nightmare_death_cry");
			break;
		case ESFX.BossNightmareDeathExplosion:
			list.Add("Boss/12_Nightmare/sfx_boss_nightmare_death_explosion");
			break;
		case ESFX.BossNightmareFightBegin:
			list.Add("Boss/12_Nightmare/sfx_boss_nightmare_fightbegin");
			break;
		case ESFX.BossNightmareHellfire:
			list.Add("Boss/12_Nightmare/sfx_boss_nightmare_hellfire");
			flag = true;
			break;
		case ESFX.BossNightmareHellfireCast:
			list.Add("Boss/12_Nightmare/sfx_boss_nightmare_hellfire_cast");
			break;
		case ESFX.BossNightmareSlash:
			list.Add("Boss/12_Nightmare/sfx_boss_nightmare_slash");
			flag = true;
			break;
		case ESFX.BossRavenAggro:
			list.Add("Boss/13_Raven/sfx_boss_raven_aggro");
			break;
		case ESFX.BossRavenBirdDeath:
			list.Add("Boss/13_Raven/sfx_boss_raven_bird_death_01");
			list.Add("Boss/13_Raven/sfx_boss_raven_bird_death_02");
			list.Add("Boss/13_Raven/sfx_boss_raven_bird_death_03");
			flag = true;
			break;
		case ESFX.BossRavenCroak:
			list.Add("Boss/13_Raven/sfx_boss_raven_croak_01");
			list.Add("Boss/13_Raven/sfx_boss_raven_croak_02");
			list.Add("Boss/13_Raven/sfx_boss_raven_croak_03");
			list.Add("Boss/13_Raven/sfx_boss_raven_croak_04");
			list.Add("Boss/13_Raven/sfx_boss_raven_croak_05");
			flag = true;
			break;
		case ESFX.BossRavenDeath:
			list.Add("Boss/13_Raven/sfx_boss_raven_death");
			break;
		case ESFX.BossRavenFlockBash:
			list.Add("Boss/13_Raven/sfx_boss_raven_flockbash");
			break;
		case ESFX.BossRavenFlockLoop:
			list.Add("Boss/13_Raven/sfx_boss_raven_flock_loop");
			break;
		case ESFX.BossRavenVanish:
			list.Add("Boss/13_Raven/sfx_boss_raven_vanish");
			break;
		case ESFX.BossRavenWindyLoop:
			list.Add("Boss/13_Raven/sfx_boss_raven_windy_loop");
			break;
		case ESFX.BossRavenWindThrowLeft:
			list.Add("Boss/13_Raven/sfx_boss_raven_windthrow_left");
			break;
		case ESFX.BossRavenWindThrowRight:
			list.Add("Boss/13_Raven/sfx_boss_raven_windthrow_right");
			break;
		case ESFX.BossRoboKittyClaw:
			list.Add("Boss/01_Kitty/sfx_boss_kitty_claw");
			flag = true;
			break;
		case ESFX.BossRoboKittyClawFar:
			list.Add("Boss/01_Kitty/sfx_boss_kitty_claw_far");
			flag = true;
			break;
		case ESFX.BossRoboKittyGroundPound:
			list.Add("Boss/01_Kitty/sfx_boss_kitty_groundpound");
			break;
		case ESFX.BossRoboKittyFall:
			list.Add("Boss/01_Kitty/sfx_boss_kitty_fall");
			break;
		case ESFX.BossRoboKittyLand:
			list.Add("Boss/01_Kitty/sfx_boss_kitty_impactverb");
			break;
		case ESFX.BossRoboKittyLaunch:
			list.Add("Boss/01_Kitty/sfx_boss_kitty_launch");
			break;
		case ESFX.BossRoboKittyPreGroundPound:
			list.Add("Boss/01_Kitty/sfx_boss_kitty_pregroundpound");
			flag = true;
			break;
		case ESFX.BossRoboKittyPrelaunch:
			list.Add("Boss/01_Kitty/sfx_boss_kitty_prelaunch");
			break;
		case ESFX.BossRoboKittyLazer:
			list.Add("Boss/01_Kitty/sfx_boss_kitty_laser");
			break;
		case ESFX.BossRoboKittyLazerEnd:
			list.Add("Boss/01_Kitty/sfx_boss_kitty_laserverb");
			break;
		case ESFX.BossRoboKittyMeow:
			list.Add("Boss/01_Kitty/sfx_boss_kitty_meow");
			flag = true;
			break;
		case ESFX.BossRoboKittyRibScrapeLoop:
			list.Add("Boss/01_Kitty/sfx_boss_kitty_ribcage_lp");
			break;
		case ESFX.BossRoboKittyLegMoveL:
			list.Add("Boss/01_Kitty/sfx_boss_kitty_legmove_L");
			break;
		case ESFX.BossRoboKittyLegMoveR:
			list.Add("Boss/01_Kitty/sfx_boss_kitty_legmove_R");
			break;
		case ESFX.BossRoboKittyPawDownL:
			list.Add("Boss/01_Kitty/sfx_boss_kitty_pawdown_L");
			break;
		case ESFX.BossRoboKittyPawDownR:
			list.Add("Boss/01_Kitty/sfx_boss_kitty_pawdown_R");
			break;
		case ESFX.BossSandmanAppear:
			list.Add("Boss/11_Sandman/sfx_boss_sandman_appear");
			break;
		case ESFX.BossSandmanBallA:
			list.Add("Boss/11_Sandman/sfx_boss_sandman_ball_01");
			break;
		case ESFX.BossSandmanBallB:
			list.Add("Boss/11_Sandman/sfx_boss_sandman_ball_02");
			break;
		case ESFX.BossSandmanChainA:
			list.Add("Boss/11_Sandman/sfx_boss_sandman_chain_01");
			flag = true;
			break;
		case ESFX.BossSandmanChainB:
			list.Add("Boss/11_Sandman/sfx_boss_sandman_chain_02");
			flag = true;
			break;
		case ESFX.BossSandmanChainCast:
			list.Add("Boss/11_Sandman/sfx_boss_sandman_chain_cast");
			break;
		case ESFX.BossSandmanDeath:
			list.Add("Boss/11_Sandman/sfx_boss_sandman_death");
			break;
		case ESFX.BossSandmanDisappear:
			list.Add("Boss/11_Sandman/sfx_boss_sandman_disappear");
			break;
		case ESFX.BossSandmanDissolve:
			list.Add("Boss/11_Sandman/sfx_boss_sandman_dissolve");
			break;
		case ESFX.BossSandmanGroundPound:
			list.Add("Boss/11_Sandman/sfx_boss_sandman_groundpound");
			break;
		case ESFX.BossSandmanGroundPoundImpact:
			list.Add("Boss/11_Sandman/sfx_boss_sandman_groundpound_impact_01");
			list.Add("Boss/11_Sandman/sfx_boss_sandman_groundpound_impact_02");
			list.Add("Boss/11_Sandman/sfx_boss_sandman_groundpound_impact_03");
			list.Add("Boss/11_Sandman/sfx_boss_sandman_groundpound_impact_04");
			list.Add("Boss/11_Sandman/sfx_boss_sandman_groundpound_impact_05");
			list.Add("Boss/11_Sandman/sfx_boss_sandman_groundpound_impact_06");
			flag = true;
			break;
		case ESFX.BossSandmanPhase2Intro:
			list.Add("Boss/11_Sandman/sfx_boss_sandman_phase2_intro");
			break;
		case ESFX.BossSandmanRam:
			list.Add("Boss/11_Sandman/sfx_boss_sandman_ram");
			break;
		case ESFX.BossSandmanRamCharge:
			list.Add("Boss/11_Sandman/sfx_boss_sandman_ram_charge");
			break;
		case ESFX.BossSandmanRamCrash:
			list.Add("Boss/11_Sandman/sfx_boss_sandman_ram_crash");
			break;
		case ESFX.BossSandmanSpikeCast:
			list.Add("Boss/11_Sandman/sfx_boss_sandman_spike_cast");
			break;
		case ESFX.BossSandmanTeethCast:
			list.Add("Boss/11_Sandman/sfx_boss_sandman_teeth_cast");
			break;
		case ESFX.BossSandmanTeethCenter:
			list.Add("Boss/11_Sandman/sfx_boss_sandman_teeth_center");
			break;
		case ESFX.BossSandmanTeethLeft:
			list.Add("Boss/11_Sandman/sfx_boss_sandman_teeth_left");
			break;
		case ESFX.BossSandmanTeethRight:
			list.Add("Boss/11_Sandman/sfx_boss_sandman_teeth_right");
			break;
		case ESFX.BossShapeshifterCough:
			list.Add("Boss/07_Shapeshifter/sfx_boss_shapeshifter_cough");
			break;
		case ESFX.BossShapeshifterCutsceneShift:
			list.Add("Boss/07_Shapeshifter/sfx_boss_shapeshifter_cutsceneshift");
			break;
		case ESFX.BossShapeshifterDeath:
			list.Add("Boss/07_Shapeshifter/sfx_boss_shapeshifter_death");
			break;
		case ESFX.BossShapeshifterDissipate:
			list.Add("Boss/07_Shapeshifter/sfx_boss_shapeshifter_dissipate");
			break;
		case ESFX.BossShapeshifterGoopLoop:
			list.Add("Boss/07_Shapeshifter/sfx_boss_shapeshifter_goop_loop");
			break;
		case ESFX.BossShapeshifterGoopSpike:
			list.Add("Boss/07_Shapeshifter/sfx_boss_shapeshifter_goop_spike");
			break;
		case ESFX.BossShapeshifterMelt:
			list.Add("Boss/07_Shapeshifter/sfx_boss_shapeshifter_melt");
			break;
		case ESFX.BossShapeshifterSelenLaugh:
			list.Add("Boss/07_Shapeshifter/sfx_boss_shapeshifter_selen_laugh");
			flag = true;
			break;
		case ESFX.BossShapeshifterSpearForm:
			list.Add("Boss/07_Shapeshifter/sfx_boss_shapeshifter_spear_form");
			flag = true;
			break;
		case ESFX.BossShapeshifterSpearImpact:
			list.Add("Boss/07_Shapeshifter/sfx_boss_shapeshifter_spear_impact");
			flag = true;
			break;
		case ESFX.BossShapeshifterSpearThrow:
			list.Add("Boss/07_Shapeshifter/sfx_boss_shapeshifter_spear_throw");
			flag = true;
			break;
		case ESFX.BossShapeshifterStab:
			list.Add("Boss/07_Shapeshifter/sfx_boss_shapeshifter_stab");
			break;
		case ESFX.BossShapeshifterTeeth:
			list.Add("Boss/07_Shapeshifter/sfx_boss_shapeshifter_teeth");
			break;
		case ESFX.BossShapeshifterUnmelt:
			list.Add("Boss/07_Shapeshifter/sfx_boss_shapeshifter_unmelt");
			break;
		case ESFX.BossSorceressDeath:
			list.Add("Boss/05_Sorceress/sfx_boss_sorceress_death");
			break;
		case ESFX.BossSorceressDeathOrbBounce:
			list.Add("Boss/05_Sorceress/sfx_boss_sorceress_death_orb_bounce");
			flag = true;
			break;
		case ESFX.BossSorceressLightningBallStart:
			list.Add("Boss/05_Sorceress/sfx_boss_sorceress_lightning_ball_start");
			flag = true;
			break;
		case ESFX.BossSorceressLightningBallLoop:
			list.Add("Boss/05_Sorceress/sfx_boss_sorceress_lightning_ball_loop");
			break;
		case ESFX.BossSorceressLightningStorm:
			list.Add("Boss/05_Sorceress/sfx_boss_sorceress_lightning_storm_01");
			list.Add("Boss/05_Sorceress/sfx_boss_sorceress_lightning_storm_02");
			list.Add("Boss/05_Sorceress/sfx_boss_sorceress_lightning_storm_03");
			flag = true;
			break;
		case ESFX.BossVolTerrilisFireSpellCast:
			list.Add("Boss/10_VolTerrilis/sfx_boss_volterrilis_fire_cast");
			break;
		case ESFX.BossVolTerrilisFireSpellFlames:
			list.Add("Boss/10_VolTerrilis/sfx_boss_volterrilis_fire_flames");
			break;
		case ESFX.BossVolTerrilisFireMeleeCast:
			list.Add("Boss/10_VolTerrilis/sfx_boss_volterrilis_firemelee_cast");
			break;
		case ESFX.BossVolTerrilisFireMeleeCastRight:
			list.Add("Boss/10_VolTerrilis/sfx_boss_volterrilis_firemelee_cast_right");
			break;
		case ESFX.BossVolTerrilisIronLoop:
			list.Add("Boss/10_VolTerrilis/sfx_boss_volterrilis_iron_loop");
			break;
		case ESFX.BossVolTerrilisIronStart:
			list.Add("Boss/08_Emperor/sfx_boss_emperor_iron_start");
			break;
		case ESFX.BossVolTerrilisLightningCast:
			list.Add("Boss/10_VolTerrilis/sfx_boss_volterrilis_lightning_cast");
			break;
		case ESFX.BossXarionClaw:
			list.Add("Boss/14_Xarion/sfx_boss_xarion_claw");
			break;
		case ESFX.BossXarionClawDeath:
			list.Add("Boss/14_Xarion/sfx_boss_xarion_claw_death");
			break;
		case ESFX.BossXarionDeath:
			list.Add("Boss/14_Xarion/sfx_boss_xarion_death");
			break;
		case ESFX.BossXarionNest:
			list.Add("Boss/14_Xarion/sfx_boss_xarion_nest");
			flag = true;
			break;
		case ESFX.BossXarionSwarm:
			list.Add("Boss/14_Xarion/sfx_boss_xarion_swarm");
			break;
		case ESFX.BossXarionVoidEnd:
			list.Add("Boss/14_Xarion/sfx_boss_xarion_void_end");
			break;
		case ESFX.BossXarionVoidLoop:
			list.Add("Boss/14_Xarion/sfx_boss_xarion_void_loop");
			break;
		case ESFX.BossXarionVoidStart:
			list.Add("Boss/14_Xarion/sfx_boss_xarion_void_start");
			break;
		case ESFX.BossXarionVoidUnit:
			list.Add("Boss/14_Xarion/sfx_boss_xarion_void_unit_01");
			list.Add("Boss/14_Xarion/sfx_boss_xarion_void_unit_02");
			list.Add("Boss/14_Xarion/sfx_boss_xarion_void_unit_03");
			flag = true;
			break;
		case ESFX.BossZelDeath:
			list.Add("Boss/15_Zel/sfx_boss_zel_death");
			break;
		case ESFX.BossZelDisappear:
			list.Add("Boss/15_Zel/sfx_boss_zel_disappear");
			break;
		case ESFX.BossZelFirespellCast:
			list.Add("Boss/15_Zel/sfx_boss_zel_firespell_cast");
			break;
		case ESFX.BossZelHumanCast:
			list.Add("Boss/15_Zel/sfx_boss_zel_human_cast");
			break;
		case ESFX.BossZelHumanDeath:
			list.Add("Boss/15_Zel/sfx_boss_zel_human_death");
			break;
		case ESFX.BossZelMagma:
			list.Add("Boss/15_Zel/sfx_boss_zel_magma");
			flag = true;
			break;
		case ESFX.BossZelReappear:
			list.Add("Boss/15_Zel/sfx_boss_zel_reappear");
			break;
		case ESFX.BossZelSpike:
			list.Add("Boss/15_Zel/sfx_boss_zel_spike_01");
			list.Add("Boss/15_Zel/sfx_boss_zel_spike_02");
			list.Add("Boss/15_Zel/sfx_boss_zel_spike_03");
			flag = true;
			break;
		case ESFX.BossZelStonePillarsCast:
			list.Add("Boss/15_Zel/sfx_boss_zel_stonepillars_cast");
			break;
		case ESFX.BossZelStonePillarsBreak:
			list.Add("Boss/15_Zel/sfx_boss_zel_stonepillar_break_01");
			list.Add("Boss/15_Zel/sfx_boss_zel_stonepillar_break_02");
			list.Add("Boss/15_Zel/sfx_boss_zel_stonepillar_break_03");
			flag = true;
			break;
		case ESFX.BossZelSword:
			list.Add("Boss/15_Zel/sfx_boss_zel_sword");
			flag = true;
			break;
		case ESFX.BossZelTransform:
			list.Add("Boss/15_Zel/sfx_boss_zel_transform");
			break;
		case ESFX.CharacterLevelUp:
			list.Add("UI/sfx_character_levelup");
			break;
		case ESFX.CharacterStatUp:
			list.Add("UI/sfx_character_statup");
			break;
		case ESFX.CrowCaw:
			list.Add("Foley/sfx_crow_caw");
			break;
		case ESFX.CsAscendPhase1Loop:
			list.Add("Cutscene/sfx_cs_ascend_phase_1_loop");
			break;
		case ESFX.CsAscendPhase2Loop:
			list.Add("Cutscene/sfx_cs_ascend_phase_2_loop");
			break;
		case ESFX.CsAscendPhase2Start:
			list.Add("Cutscene/sfx_cs_ascend_phase_2_start");
			break;
		case ESFX.CsAscendPhase3Loop:
			list.Add("Cutscene/sfx_cs_ascend_phase_3_loop");
			break;
		case ESFX.CsAscendPhase3Start:
			list.Add("Cutscene/sfx_cs_ascend_phase_3_start");
			break;
		case ESFX.CsAscendPhase4:
			list.Add("Cutscene/sfx_cs_ascend_phase_4");
			break;
		case ESFX.CsBranchesSplash:
			list.Add("Cutscene/sfx_cs_branches_splash");
			break;
		case ESFX.CsExplosionRubble:
			list.Add("Cutscene/sfx_cs_explosion_rubble");
			break;
		case ESFX.CsFallingWind:
			list.Add("Cutscene/sfx_cs_falling_wind");
			break;
		case ESFX.CsLakeShip:
			list.Add("Cutscene/sfx_cs_lake_ship");
			break;
		case ESFX.CsPrologueExplosion:
			list.Add("Cutscene/sfx_cs_prologue_explosion");
			break;
		case ESFX.CsPrologueRock:
			list.Add("Cutscene/sfx_cs_prologue_rock");
			break;
		case ESFX.CsPrologueRumble:
			list.Add("Cutscene/sfx_cs_prologue_rumble");
			break;
		case ESFX.CsPrologueTableFlip:
			list.Add("Cutscene/sfx_cs_prologue_tableflip");
			break;
		case ESFX.CsRealityShatter:
			list.Add("Cutscene/sfx_cs_reality_shatter");
			break;
		case ESFX.CsTimespinnerAbsorb:
			list.Add("Cutscene/sfx_cs_timespinner_absorb");
			break;
		case ESFX.CsTimespinnerHaywireLoop:
			list.Add("Cutscene/sfx_cs_timespinner_haywire_loop");
			break;
		case ESFX.CsTimespinnerNormalLoop:
			list.Add("Cutscene/sfx_cs_timespinner_normal_loop");
			break;
		case ESFX.CsTimespinnerNormalStart:
			list.Add("Cutscene/sfx_cs_timespinner_normal_start");
			break;
		case ESFX.CsTimespinnerSelenDeath:
			list.Add("Cutscene/sfx_cs_timespinner_selen_death");
			break;
		case ESFX.DialogClose:
			list.Add("UI/sfx_dia_close");
			break;
		case ESFX.DialogOpen:
			list.Add("UI/sfx_dia_open");
			break;
		case ESFX.DialogTic:
			list.Add("UI/sfx_dia_tic_loop");
			break;
		case ESFX.DoorBossClose:
			list.Add("Foley/sfx_door_boss_close");
			break;
		case ESFX.DoorBossClose2D:
			list.Add("Foley/sfx_door_boss_close_2d");
			break;
		case ESFX.DoorBossOpen:
			list.Add("Foley/sfx_door_boss_open");
			break;
		case ESFX.DoorDungeonOpen:
			list.Add("Foley/sfx_door_dungeon_open");
			break;
		case ESFX.DoorKeycardAccessGranted:
			list.Add("Foley/sfx_door_keycard_open_access_granted");
			break;
		case ESFX.DoorKeycardError:
			list.Add("Foley/sfx_door_keycard_error");
			break;
		case ESFX.DoorKeycardLoop:
			list.Add("Foley/sfx_door_keycard_loop");
			break;
		case ESFX.DoorKeycardOpen:
			list.Add("Foley/sfx_door_keycard_open_door_open");
			break;
		case ESFX.DoorTransitionClose:
			list.Add("Foley/sfx_door_transition_close");
			break;
		case ESFX.DoorTransitionOpen:
			list.Add("Foley/sfx_door_transition_open");
			break;
		case ESFX.EnemyArcherArrowImpact:
			list.Add("Enemies/04_Curtain/sfx_en_archer_arrow_impact");
			flag = true;
			break;
		case ESFX.EnemyArcherBowDraw:
			list.Add("Enemies/04_Curtain/sfx_en_archer_bow_draw");
			break;
		case ESFX.EnemyArcherBowShoot:
			list.Add("Enemies/04_Curtain/sfx_en_archer_bow_shoot");
			flag = true;
			break;
		case ESFX.EnemyBabyCheveuxDrop:
			list.Add("Enemies/07_LakeSerene/sfx_en_baby_cheveux_drop");
			break;
		case ESFX.EnemyBabyCheveuxFlap:
			list.Add("Enemies/07_LakeSerene/sfx_en_baby_cheveux_flap");
			break;
		case ESFX.EnemyBabyCheveuxLand:
			list.Add("Enemies/07_LakeSerene/sfx_en_baby_cheveux_land");
			break;
		case ESFX.EnemyBabyCheveuxSquawk:
			list.Add("Enemies/07_LakeSerene/sfx_en_baby_cheveux_squawk_01");
			list.Add("Enemies/07_LakeSerene/sfx_en_baby_cheveux_squawk_02");
			list.Add("Enemies/07_LakeSerene/sfx_en_baby_cheveux_squawk_03");
			flag = true;
			break;
		case ESFX.EnemyBarbedAnemoneDeploy:
			list.Add("Enemies/07_LakeSerene/sfx_en_barbedanemone_deploy");
			flag = true;
			break;
		case ESFX.EnemyBarbedAnemoneRetract:
			list.Add("Enemies/07_LakeSerene/sfx_en_barbedanemone_retract");
			flag = true;
			break;
		case ESFX.EnemyBlossomAutomatonClose:
			list.Add("Enemies/01_LakeDesolation/sfx_en_blossomautomaton_close");
			flag = true;
			break;
		case ESFX.EnemyBlossomAutomatonOpen:
			list.Add("Enemies/01_LakeDesolation/sfx_en_blossomautomaton_open");
			flag = true;
			break;
		case ESFX.EnemyCheveuxTowerSquawk:
			list.Add("Enemies/07_LakeSerene/sfx_en_cheveux_tower_squawk_01");
			list.Add("Enemies/07_LakeSerene/sfx_en_cheveux_tower_squawk_02");
			list.Add("Enemies/07_LakeSerene/sfx_en_cheveux_tower_squawk_03");
			flag = true;
			break;
		case ESFX.EnemyCheveuxTowerVomit:
			list.Add("Enemies/07_LakeSerene/sfx_en_cheveux_tower_vomit");
			flag = true;
			break;
		case ESFX.EnemyCheveuxTowerVomitSplat:
			list.Add("Enemies/07_LakeSerene/sfx_en_cheveux_tower_vomit_splat");
			flag = true;
			break;
		case ESFX.EnemyCheveuxTowerVomitWalk:
			list.Add("Enemies/07_LakeSerene/sfx_en_cheveux_tower_vomit_walk_01");
			list.Add("Enemies/07_LakeSerene/sfx_en_cheveux_tower_vomit_walk_02");
			list.Add("Enemies/07_LakeSerene/sfx_en_cheveux_tower_vomit_walk_03");
			list.Add("Enemies/07_LakeSerene/sfx_en_cheveux_tower_vomit_walk_04");
			flag = true;
			break;
		case ESFX.EnemyCopperWyvernHop:
			list.Add("Enemies/08_CavesPast/sfx_en_copper_wyvern_hop");
			break;
		case ESFX.EnemyCopperWyvernLand:
			list.Add("Enemies/08_CavesPast/sfx_en_copper_wyvern_land");
			break;
		case ESFX.EnemyCopperWyvernPrespit:
			list.Add("Enemies/08_CavesPast/sfx_en_copper_wyvern_prespit");
			flag = true;
			break;
		case ESFX.EnemyCopperWyvernSpit:
			list.Add("Enemies/08_CavesPast/sfx_en_copper_wyvern_spit");
			flag = true;
			break;
		case ESFX.EnemyCopperWyvernSpitImpact:
			list.Add("Enemies/08_CavesPast/sfx_en_copper_wyvern_spit_impact");
			break;
		case ESFX.EnemyDemonCast:
			list.Add("Enemies/05_Keep/sfx_en_demon_cast");
			break;
		case ESFX.EnemyDemonDeathFire:
			list.Add("Enemies/05_Keep/sfx_en_demon_death_fire");
			break;
		case ESFX.EnemyDiscAggro:
			list.Add("Enemies/02_Metropolis/sfx_en_disc_aggro");
			break;
		case ESFX.EnemyDiscMovement:
			list.Add("Enemies/02_Metropolis/sfx_en_disc_movement");
			break;
		case ESFX.EnemySlimeCeilingDrop:
			list.Add("Enemies/08_CavesPast/sfx_en_ectoplasm_ceilingdrop");
			break;
		case ESFX.EnemySlimeLand:
			list.Add("Enemies/08_CavesPast/sfx_en_ectoplasm_land");
			break;
		case ESFX.EnemySlimeLunge:
			list.Add("Enemies/08_CavesPast/sfx_en_ectoplasm_lunge");
			break;
		case ESFX.EnemySlimeMoveLoop:
			list.Add("Enemies/08_CavesPast/sfx_en_ectoplasm_move_loop");
			break;
		case ESFX.EnemyEelDiggingCry:
			list.Add("Enemies/07_LakeSerene/sfx_en_eel_digging_cry_01");
			list.Add("Enemies/07_LakeSerene/sfx_en_eel_digging_cry_02");
			list.Add("Enemies/07_LakeSerene/sfx_en_eel_digging_cry_03");
			break;
		case ESFX.EnemyEelDeathLoop:
			list.Add("Enemies/07_LakeSerene/sfx_en_eel_death_loop");
			break;
		case ESFX.EnemyEelDiggingLoop:
			list.Add("Enemies/07_LakeSerene/sfx_en_eel_digging_loop");
			break;
		case ESFX.EnemyEelDiggingLoopEnd:
			list.Add("Enemies/07_LakeSerene/sfx_en_eel_digging_loop_end");
			break;
		case ESFX.EnemyEelDiggingLoopStart:
			list.Add("Enemies/07_LakeSerene/sfx_en_eel_digging_loop_start");
			break;
		case ESFX.EnemyEelGrowlBig:
			list.Add("Enemies/07_LakeSerene/sfx_en_eel_growl_big_01");
			list.Add("Enemies/07_LakeSerene/sfx_en_eel_growl_big_02");
			break;
		case ESFX.EnemyEelGrowlSmall:
			list.Add("Enemies/07_LakeSerene/sfx_en_eel_growl_small_01");
			list.Add("Enemies/07_LakeSerene/sfx_en_eel_growl_small_02");
			list.Add("Enemies/07_LakeSerene/sfx_en_eel_growl_small_03");
			break;
		case ESFX.EnemyEggHatch:
			list.Add("Enemies/05_Keep/sfx_en_egg_hatch");
			break;
		case ESFX.EnemyEngineerBoulderBreak:
			list.Add("Enemies/04_Curtain/sfx_en_engineer_boulder_break_01");
			list.Add("Enemies/04_Curtain/sfx_en_engineer_boulder_break_02");
			list.Add("Enemies/04_Curtain/sfx_en_engineer_boulder_break_03");
			flag = true;
			break;
		case ESFX.EnemyEngineerLogBounce:
			list.Add("Enemies/04_Curtain/sfx_en_engineer_log_bounce_01");
			list.Add("Enemies/04_Curtain/sfx_en_engineer_log_bounce_02");
			list.Add("Enemies/04_Curtain/sfx_en_engineer_log_bounce_03");
			flag = true;
			break;
		case ESFX.EnemyEngineerLogBreak:
			list.Add("Enemies/04_Curtain/sfx_en_engineer_log_break_01");
			list.Add("Enemies/04_Curtain/sfx_en_engineer_log_break_02");
			list.Add("Enemies/04_Curtain/sfx_en_engineer_log_break_03");
			flag = true;
			break;
		case ESFX.EnemyEngineerLogRollLoop:
			list.Add("Enemies/04_Curtain/sfx_en_engineer_log_roll_loop");
			break;
		case ESFX.EnemyEngineerSurprised:
			list.Add("Enemies/04_Curtain/sfx_en_engineer_surprised");
			break;
		case ESFX.EnemyElectricDown:
			list.Add("Enemies/01_LakeDesolation/sfx_en_ceilingstar_down");
			break;
		case ESFX.EnemyElectricStorm:
			list.Add("Enemies/01_LakeDesolation/sfx_en_ceilingstar_lp");
			break;
		case ESFX.EnemyElectricUp:
			list.Add("Enemies/01_LakeDesolation/sfx_en_ceilingstar_up");
			break;
		case ESFX.EnemyExplodeHumanoid:
			list.Add("Enemies/00_Misc/sfx_en_gen_explode_humanoid");
			break;
		case ESFX.EnemyFangedAnemoneFangShot:
			list.Add("Enemies/09_CavesPresent/sfx_en_fangedanemone_fangshot");
			break;
		case ESFX.EnemyFireMageFireCast:
			list.Add("Enemies/05_Keep/sfx_en_fire_mage_fire_cast_01");
			list.Add("Enemies/05_Keep/sfx_en_fire_mage_fire_cast_02");
			list.Add("Enemies/05_Keep/sfx_en_fire_mage_fire_cast_03");
			list.Add("Enemies/05_Keep/sfx_en_fire_mage_fire_cast_04");
			list.Add("Enemies/05_Keep/sfx_en_fire_mage_fire_cast_05");
			flag = true;
			break;
		case ESFX.EnemyFledglingWarbirdCharge:
			list.Add("Enemies/05_Keep/sfx_en_fledglingwarbird_chargeattack");
			break;
		case ESFX.EnemyFortressEngineerBomb:
			list.Add("Enemies/10_Hangar/sfx_en_fortress_engineer_bomb_01");
			list.Add("Enemies/10_Hangar/sfx_en_fortress_engineer_bomb_02");
			flag = true;
			break;
		case ESFX.EnemyFortressKnightAbsorb:
			list.Add("Enemies/10_Hangar/sfx_en_fortress_knight_absorb");
			flag = true;
			break;
		case ESFX.EnemyFortressKnightEnergyLoop:
			list.Add("Enemies/10_Hangar/sfx_en_fortress_knight_energy_loop");
			break;
		case ESFX.EnemyFortressKnightShoot:
			list.Add("Enemies/10_Hangar/sfx_en_fortress_knight_shoot");
			break;
		case ESFX.EnemyFortressKnightSword:
			list.Add("Enemies/10_Hangar/sfx_en_fortress_knight_sword");
			break;
		case ESFX.EnemyFortressLargeSoldierShockwave:
			list.Add("Enemies/10_Hangar/sfx_en_fortress_large_soldier_shockwave");
			flag = true;
			break;
		case ESFX.EnemyFortressSniperAim:
			list.Add("Enemies/10_Hangar/sfx_en_fortress_sniper_aim");
			flag = true;
			break;
		case ESFX.EnemyFortressSniperFire:
			list.Add("Enemies/10_Hangar/sfx_en_fortress_sniper_fire");
			flag = true;
			break;
		case ESFX.EnemyFlyLoop:
			list.Add("Enemies/07_LakeSerene/sfx_en_fly_loop");
			break;
		case ESFX.EnemyFlyingCheveuxIdle:
			list.Add("Enemies/02_Metropolis/sfx_en_cheveux_flying_idle");
			break;
		case ESFX.EnemyGalacticSageSpellCast:
			list.Add("Enemies/12_EmpTower/sfx_en_galacticsage_spell_cast");
			flag = true;
			break;
		case ESFX.EnemyGalacticSageSpellGlyph:
			list.Add("Enemies/12_EmpTower/sfx_en_galacticsage_spell_glyph");
			flag = true;
			break;
		case ESFX.EnemyGyreRyshiaDeath:
			list.Add("Enemies/13_Gyre/sfx_en_gyre_ryshia_death");
			break;
		case ESFX.EnemyGyreRyshiaDeath2D:
			list.Add("Enemies/13_Gyre/sfx_en_gyre_ryshia_death_2d");
			break;
		case ESFX.EnemyGyreRyshiaSummon:
			list.Add("Enemies/13_Gyre/sfx_en_gyre_ryshia_summon");
			break;
		case ESFX.EnemyJusticeBashImpact:
			list.Add("Enemies/14_Temple/sfx_en_justice_bash_impact");
			break;
		case ESFX.EnemyJusticeBashMove:
			list.Add("Enemies/14_Temple/sfx_en_justice_bash_move");
			break;
		case ESFX.EnemyKainAttackPrep:
			list.Add("Enemies/13_Gyre/sfx_en_kain_attack_prep");
			break;
		case ESFX.EnemyKainAttackThrow:
			list.Add("Enemies/13_Gyre/sfx_en_kain_attack_throw");
			break;
		case ESFX.EnemyIceMageGlyph:
			list.Add("Enemies/06_Tower/sfx_en_ice_mage_glyph");
			break;
		case ESFX.EnemyIceMageIceCast:
			list.Add("Enemies/06_Tower/sfx_en_ice_mage_ice_cast");
			break;
		case ESFX.EnemyLabAdultGlassSlash:
			list.Add("Enemies/11_Lab/sfx_en_lab_adult_glass_slash_01");
			list.Add("Enemies/11_Lab/sfx_en_lab_adult_glass_slash_02");
			list.Add("Enemies/11_Lab/sfx_en_lab_adult_glass_slash_03");
			list.Add("Enemies/11_Lab/sfx_en_lab_adult_glass_slash_04");
			flag = true;
			break;
		case ESFX.EnemyLabAdultGroan:
			list.Add("Enemies/11_Lab/sfx_en_lab_adult_groan");
			break;
		case ESFX.EnemyLabChildMoveLoop:
			list.Add("Enemies/11_Lab/sfx_en_lab_child_move_loop");
			break;
		case ESFX.EnemyLabChildShriek:
			list.Add("Enemies/11_Lab/sfx_en_lab_child_shriek");
			flag = true;
			break;
		case ESFX.EnemyLabTurretCharge:
			list.Add("Enemies/11_Lab/sfx_en_lab_turret_charge");
			break;
		case ESFX.EnemyLabTurretShoot:
			list.Add("Enemies/11_Lab/sfx_en_lab_turret_shoot");
			flag = true;
			break;
		case ESFX.EnemyLargeSoldierHammer:
			list.Add("Enemies/04_Curtain/sfx_en_large_soldier_hammer");
			break;
		case ESFX.EnemyLargeSoldierReset:
			list.Add("Enemies/04_Curtain/sfx_en_large_soldier_reset");
			break;
		case ESFX.EnemyMageSpellGlyph:
			list.Add("Enemies/05_Keep/sfx_en_mage_spell_glyph");
			break;
		case ESFX.EnemyMeteorSparrowAimLocked:
			list.Add("Enemies/01_LakeDesolation/sfx_en_meteorsparrow_aim_locked");
			flag = true;
			break;
		case ESFX.EnemyMeteorSparrowAimStart:
			list.Add("Enemies/01_LakeDesolation/sfx_en_meteorsparrow_aim_start");
			flag = true;
			break;
		case ESFX.EnemyMeteorSparrowDashImpact:
			list.Add("Enemies/01_LakeDesolation/sfx_en_meteorsparrow_dash_impact");
			flag = true;
			break;
		case ESFX.EnemyMeteorSparrowDashImpact2D:
			list.Add("Enemies/01_LakeDesolation/sfx_en_meteorsparrow_dash_impact_2d");
			break;
		case ESFX.EnemyMeteorSparrowDashStart:
			list.Add("Enemies/01_LakeDesolation/sfx_en_meteorsparrow_dash_start");
			flag = true;
			break;
		case ESFX.EnemyMushroomTowerDeath:
			list.Add("Enemies/08_CavesPast/sfx_en_mushroom_tower_death");
			break;
		case ESFX.EnemyMushroomTowerEmit:
			list.Add("Enemies/08_CavesPast/sfx_en_mushroom_tower_emit");
			break;
		case ESFX.EnemyMushroomTowerSporeHit:
			list.Add("Enemies/08_CavesPast/sfx_en_mushroom_tower_spore_hit");
			break;
		case ESFX.EnemyNethershadeAggro:
			list.Add("Enemies/13_Gyre/sfx_en_nethershade_aggro");
			flag = true;
			break;
		case ESFX.EnemyNethershadeDeath:
			list.Add("Enemies/13_Gyre/sfx_en_nethershade_death");
			break;
		case ESFX.EnemyNethershadeLoop:
			list.Add("Enemies/13_Gyre/sfx_en_nethershade_loop");
			break;
		case ESFX.EnemyNethershadeLoop2D:
			list.Add("Enemies/13_Gyre/sfx_en_nethershade_loop_2d");
			break;
		case ESFX.EnemyNethershadeUnAggro:
			list.Add("Enemies/13_Gyre/sfx_en_nethershade_unaggro");
			flag = true;
			break;
		case ESFX.EnemyOrganicShoot:
			list.Add("Enemies/03_Forest/sfx_en_wormflower_spit");
			break;
		case ESFX.EnemyOrnagyRutBark:
			list.Add("Enemies/13_Gyre/sfx_en_ornagyrut_bark");
			flag = true;
			break;
		case ESFX.EnemyOrnagyRutDeath:
			list.Add("Enemies/13_Gyre/sfx_en_ornagyrut_death");
			break;
		case ESFX.EnemyLand:
			list.Add("Enemies/01_LakeDesolation/sfx_en_cheveux_land");
			break;
		case ESFX.EnemyPlantBatDrop:
			list.Add("Enemies/07_LakeSerene/sfx_en_plantbat_drop");
			flag = true;
			break;
		case ESFX.EnemyPlantBatGnaw:
			list.Add("Enemies/07_LakeSerene/sfx_en_plant_bat_gnaw_01");
			list.Add("Enemies/07_LakeSerene/sfx_en_plant_bat_gnaw_02");
			list.Add("Enemies/07_LakeSerene/sfx_en_plant_bat_gnaw_03");
			break;
		case ESFX.EnemyPlantBatWingFlap:
			list.Add("Enemies/07_LakeSerene/sfx_en_plantbat_wingflap_01");
			list.Add("Enemies/07_LakeSerene/sfx_en_plantbat_wingflap_02");
			list.Add("Enemies/07_LakeSerene/sfx_en_plantbat_wingflap_03");
			list.Add("Enemies/07_LakeSerene/sfx_en_plantbat_wingflap_04");
			flag = true;
			break;
		case ESFX.EnemyPlasmaPodAggro:
			list.Add("Enemies/06_Tower/sfx_en_plasmapod_aggro");
			break;
		case ESFX.EnemyPlasmaPodAttackCast:
			list.Add("Enemies/06_Tower/sfx_en_plasmapod_attack_cast");
			break;
		case ESFX.EnemyPlasmaPodAttackStart:
			list.Add("Enemies/06_Tower/sfx_en_plasmapod_attack_start");
			break;
		case ESFX.EnemyPoisonMothWingFlap:
			list.Add("Enemies/03_Forest/sfx_en_poisonmoth_wingflap_01");
			list.Add("Enemies/03_Forest/sfx_en_poisonmoth_wingflap_02");
			list.Add("Enemies/03_Forest/sfx_en_poisonmoth_wingflap_03");
			list.Add("Enemies/03_Forest/sfx_en_poisonmoth_wingflap_04");
			flag = true;
			break;
		case ESFX.EnemyRatLunge:
			list.Add("Enemies/03_Forest/sfx_en_rat_lunge_01");
			list.Add("Enemies/03_Forest/sfx_en_rat_lunge_02");
			list.Add("Enemies/03_Forest/sfx_en_rat_lunge_03");
			flag = true;
			break;
		case ESFX.EnemyRatSqueak:
			list.Add("Enemies/03_Forest/sfx_en_rat_squeak_01");
			list.Add("Enemies/03_Forest/sfx_en_rat_squeak_02");
			list.Add("Enemies/03_Forest/sfx_en_rat_squeak_03");
			list.Add("Enemies/03_Forest/sfx_en_rat_squeak_04");
			flag = true;
			break;
		case ESFX.EnemyRoyalGuardAggro:
			list.Add("Enemies/06_Tower/sfx_en_royal_guard_aggro");
			break;
		case ESFX.EnemyRoyalGuardDeathCry:
			list.Add("Enemies/06_Tower/sfx_en_royal_guard_death_cry");
			break;
		case ESFX.EnemyRoyalGuardSpellCast:
			list.Add("Enemies/06_Tower/sfx_en_royal_guard_spell_cast");
			break;
		case ESFX.EnemyRoyalGuardSpellPrep:
			list.Add("Enemies/06_Tower/sfx_en_royal_guard_spell_prep");
			break;
		case ESFX.EnemySecGuardGrenadeBounce:
			list.Add("Enemies/02_Metropolis/sfx_en_secguard_grenade_bounce_01");
			list.Add("Enemies/02_Metropolis/sfx_en_secguard_grenade_bounce_02");
			list.Add("Enemies/02_Metropolis/sfx_en_secguard_grenade_bounce_03");
			list.Add("Enemies/02_Metropolis/sfx_en_secguard_grenade_bounce_04");
			flag = true;
			break;
		case ESFX.EnemySecGuardGrenadePrep:
			list.Add("Enemies/02_Metropolis/sfx_en_secguard_grenade_prep");
			break;
		case ESFX.EnemySecGuardGrenadeToss:
			list.Add("Enemies/02_Metropolis/sfx_en_secguard_grenade_toss");
			break;
		case ESFX.EnemySecGuardSlash:
			list.Add("Enemies/02_Metropolis/sfx_en_secguard_slash");
			break;
		case ESFX.EnemySecGuardSlashPrep:
			list.Add("Enemies/02_Metropolis/sfx_en_secguard_slash_prep");
			break;
		case ESFX.EnemyShieldKnightAttack:
			list.Add("Enemies/04_Curtain/sfx_en_shield_knight_attack");
			flag = true;
			break;
		case ESFX.EnemyShieldKnightBlock:
			list.Add("Enemies/04_Curtain/sfx_en_shield_knight_block");
			flag = true;
			break;
		case ESFX.EnemySirenInkAttack:
			list.Add("Enemies/08_CavesPast/sfx_en_siren_inkattack");
			flag = true;
			break;
		case ESFX.EnemySirenSplashImpact:
			list.Add("Enemies/08_CavesPast/sfx_en_siren_splash_impact_01");
			list.Add("Enemies/08_CavesPast/sfx_en_siren_splash_impact_02");
			list.Add("Enemies/08_CavesPast/sfx_en_siren_splash_impact_03");
			list.Add("Enemies/08_CavesPast/sfx_en_siren_splash_impact_04");
			flag = true;
			break;
		case ESFX.EnemySirenSplashStart:
			list.Add("Enemies/08_CavesPast/sfx_en_siren_splash_start");
			break;
		case ESFX.EnemySirenSplashThrow:
			list.Add("Enemies/08_CavesPast/sfx_en_siren_splash_throw");
			flag = true;
			break;
		case ESFX.EnemySnailDeath:
			list.Add("Enemies/08_CavesPast/sfx_en_snail_death");
			break;
		case ESFX.EnemySnailHarpoon:
			list.Add("Enemies/08_CavesPast/sfx_en_snail_harpoon");
			flag = true;
			break;
		case ESFX.EnemySnailHarpoonPrep:
			list.Add("Enemies/08_CavesPast/sfx_en_snail_harpoon_prep");
			flag = true;
			break;
		case ESFX.EnemySnailHarpoonRetract:
			list.Add("Enemies/08_CavesPast/sfx_en_snail_harpoon_retract");
			flag = true;
			break;
		case ESFX.EnemySnailMoveIn:
			list.Add("Enemies/08_CavesPast/sfx_en_snail_move_in");
			flag = true;
			break;
		case ESFX.EnemySnailMoveOut:
			list.Add("Enemies/08_CavesPast/sfx_en_snail_move_out");
			flag = true;
			break;
		case ESFX.EnemySnailSpit:
			list.Add("Enemies/08_CavesPast/sfx_en_snail_spit");
			flag = true;
			break;
		case ESFX.EnemySnailSpitLand:
			list.Add("Enemies/08_CavesPast/sfx_en_snail_spit_land_01");
			list.Add("Enemies/08_CavesPast/sfx_en_snail_spit_land_02");
			list.Add("Enemies/08_CavesPast/sfx_en_snail_spit_land_03");
			list.Add("Enemies/08_CavesPast/sfx_en_snail_spit_land_04");
			list.Add("Enemies/08_CavesPast/sfx_en_snail_spit_land_05");
			list.Add("Enemies/08_CavesPast/sfx_en_snail_spit_land_06");
			flag = true;
			break;
		case ESFX.EnemySnailSpitPrep:
			list.Add("Enemies/08_CavesPast/sfx_en_snail_spit_prep");
			flag = true;
			break;
		case ESFX.EnemySpiderDrop:
			list.Add("Enemies/02_Metropolis/sfx_en_spider_drop");
			break;
		case ESFX.EnemySpiderWalk:
			list.Add("Enemies/02_Metropolis/sfx_en_spider_walk");
			break;
		case ESFX.EnemySporeVineBreak:
			list.Add("Enemies/08_CavesPast/sfx_en_spore_vine_break");
			flag = true;
			break;
		case ESFX.EnemySporeVineLand:
			list.Add("Enemies/08_CavesPast/sfx_en_spore_vine_land");
			flag = true;
			break;
		case ESFX.EnemySporeVineWhiff:
			list.Add("Enemies/08_CavesPast/sfx_en_spore_vine_whiff_01");
			list.Add("Enemies/08_CavesPast/sfx_en_spore_vine_whiff_02");
			list.Add("Enemies/08_CavesPast/sfx_en_spore_vine_whiff_03");
			list.Add("Enemies/08_CavesPast/sfx_en_spore_vine_whiff_04");
			list.Add("Enemies/08_CavesPast/sfx_en_spore_vine_whiff_05");
			flag = true;
			break;
		case ESFX.EnemySpringJump:
			list.Add("Enemies/01_LakeDesolation/sfx_en_cheveux_jump_01");
			list.Add("Enemies/01_LakeDesolation/sfx_en_cheveux_jump_02");
			list.Add("Enemies/01_LakeDesolation/sfx_en_cheveux_jump_03");
			break;
		case ESFX.EnemyTeenCheveuxAggro:
			list.Add("Enemies/07_LakeSerene/sfx_en_savagecheveur_aggro");
			break;
		case ESFX.EnemyTeenCheveuxAggro2D:
			list.Add("Enemies/07_LakeSerene/sfx_en_savagecheveur_aggro_2d");
			break;
		case ESFX.EnemyTeenCheveuxBreath:
			list.Add("Enemies/07_LakeSerene/sfx_en_teen_cheveux_breath");
			flag = true;
			break;
		case ESFX.EnemyTeenCheveuxFootLeft:
			list.Add("Enemies/07_LakeSerene/sfx_en_savagecheveur_foot_left");
			flag = true;
			break;
		case ESFX.EnemyTeenCheveuxFootRight:
			list.Add("Enemies/07_LakeSerene/sfx_en_savagecheveur_foot_right");
			flag = true;
			break;
		case ESFX.EnemyTempleAscendedDeath:
			list.Add("Enemies/14_Temple/sfx_en_temple_ascended_death");
			flag = true;
			break;
		case ESFX.EnemyTempleAscendedLoop:
			list.Add("Enemies/14_Temple/sfx_en_temple_ascended_loop");
			break;
		case ESFX.EnemyTempleConvictionBash:
			list.Add("Enemies/14_Temple/sfx_en_temple_conviction_bash");
			flag = true;
			break;
		case ESFX.EnemyTempleZealSpell:
			list.Add("Enemies/14_Temple/sfx_en_temple_zeal_spell");
			flag = true;
			break;
		case ESFX.EnemyTreadCharge:
			list.Add("Enemies/01_LakeDesolation/sfx_en_cheveux_tank_charge");
			break;
		case ESFX.EnemyTreadDash:
			list.Add("Enemies/01_LakeDesolation/sfx_en_cheveux_tank_dash");
			break;
		case ESFX.EnemyWildCheveurCharge:
			list.Add("Enemies/07_LakeSerene/sfx_en_wildcheveur_chargeattack");
			break;
		case ESFX.EnemyWormFlowerClose:
			list.Add("Enemies/03_Forest/sfx_en_wormblossom_close");
			break;
		case ESFX.EnemyWormFlowerOpen:
			list.Add("Enemies/03_Forest/sfx_en_wormblossom_open");
			break;
		case ESFX.EnemyWormFlowerSeedBoom:
			list.Add("Enemies/03_Forest/sfx_en_wormflower_seedhit_break");
			break;
		case ESFX.EnemyWormFlowerSeedHitSprout:
			list.Add("Enemies/03_Forest/sfx_en_wormflower_seedhit_sprout");
			break;
		case ESFX.EnvCavesElevatorBreakThru:
			list.Add("Foley/sfx_env_caves_elevator_breakthru");
			break;
		case ESFX.EnvCavesElevatorLoopEnd:
			list.Add("Foley/sfx_env_caves_elevator_loop_end");
			break;
		case ESFX.EnvCavesElevatorLoop:
			list.Add("Foley/sfx_env_caves_elevator_loop");
			break;
		case ESFX.EnvCavesElevatorLoopStart:
			list.Add("Foley/sfx_env_caves_elevator_loop_start");
			break;
		case ESFX.EnvCrusherCrush:
			list.Add("Foley/sfx_env_crusher_crush_01");
			list.Add("Foley/sfx_env_crusher_crush_02");
			list.Add("Foley/sfx_env_crusher_crush_03");
			break;
		case ESFX.EnvConveyorSwap:
			list.Add("Foley/sfx_env_conveyor_swap");
			break;
		case ESFX.EnvCrusherDown:
			list.Add("Foley/sfx_env_crusher_down");
			break;
		case ESFX.EnvCrusherUp:
			list.Add("Foley/sfx_env_crusher_up");
			break;
		case ESFX.EnvElevator:
			list.Add("Foley/sfx_env_elevator");
			break;
		case ESFX.EnvElevatorDoorClose:
			list.Add("Foley/sfx_env_elevator_door_close");
			break;
		case ESFX.EnvElevatorDoorOpen:
			list.Add("Foley/sfx_env_elevator_door_open");
			break;
		case ESFX.EnvLabGlassTubeBreak:
			list.Add("Foley/sfx_env_lab_glass_tube_break");
			break;
		case ESFX.EnvLabPowerDown:
			list.Add("Foley/sfx_env_lab_power_down");
			break;
		case ESFX.EnvMilitaryLazerTouch:
			list.Add("Foley/sfx_env_militarylazer_touch");
			break;
		case ESFX.EnvPlasmaCrystalBreak:
			list.Add("Foley/sfx_env_plasmacrystal_break");
			break;
		case ESFX.EnvVinesBurning:
			list.Add("Foley/sfx_env_vines_burning");
			break;
		case ESFX.FamiliarAfkBegin:
			list.Add("Familiar/sfx_familiar_afk_begin");
			break;
		case ESFX.FamiliarAfkFinish:
			list.Add("Familiar/sfx_familiar_afk_finish");
			break;
		case ESFX.FamiliarCoinSpell:
			list.Add("Familiar/sfx_familiar_coin_spell_01");
			list.Add("Familiar/sfx_familiar_coin_spell_02");
			list.Add("Familiar/sfx_familiar_coin_spell_03");
			list.Add("Familiar/sfx_familiar_coin_spell_04");
			break;
		case ESFX.FamiliarFireBreath:
			list.Add("Familiar/sfx_familiar_fire_breath");
			break;
		case ESFX.FamiliarHealSpell:
			list.Add("Familiar/sfx_familiar_heal_spell");
			break;
		case ESFX.FamiliarKoboRangedAttack:
			list.Add("Familiar/sfx_familiar_kobo_rangedattack");
			break;
		case ESFX.FamiliarPlayer2End:
			list.Add("Familiar/sfx_familiar_player2_end");
			break;
		case ESFX.FamiliarPlayer2Start:
			list.Add("Familiar/sfx_familiar_player2_start");
			break;
		case ESFX.FamiliarPoof:
			list.Add("Familiar/sfx_familiar_poof");
			break;
		case ESFX.FamiliarSwipe:
			list.Add("Familiar/sfx_familiar_swipe");
			break;
		case ESFX.FamiliarWindSpell:
			list.Add("Familiar/sfx_familiar_wind_spell");
			break;
		case ESFX.FoleyBreakableWallBreak:
			list.Add("Foley/sfx_breakable_wall_break");
			break;
		case ESFX.FoleyBreakableWallBreak2D:
			list.Add("Foley/sfx_breakable_wall_break_2d");
			break;
		case ESFX.FoleyBreakableWallHit:
			list.Add("Foley/sfx_breakable_wall_hit_01");
			list.Add("Foley/sfx_breakable_wall_hit_02");
			list.Add("Foley/sfx_breakable_wall_hit_03");
			flag = true;
			break;
		case ESFX.FoleyCrumble:
			list.Add("Foley/sfx_groundbreak_01");
			list.Add("Foley/sfx_groundbreak_02");
			list.Add("Foley/sfx_groundbreak_03");
			list.Add("Foley/sfx_groundbreak_04");
			break;
		case ESFX.FoleyDrawbridgeLockMid:
			list.Add("Foley/sfx_drawbridge_lock_mid");
			break;
		case ESFX.FoleyDrawbridgeLockProper:
			list.Add("Foley/sfx_drawbridge_lock_proper");
			break;
		case ESFX.FoleyDrawbridgeLowerLoop:
			list.Add("Foley/sfx_drawbridge_lower_loop");
			break;
		case ESFX.FoleyDrawbridgeRaiseLoop:
			list.Add("Foley/sfx_drawbridge_raise_loop");
			break;
		case ESFX.FoleyExplosionFeather:
			list.Add("Foley/sfx_en_gen_explode_feathers");
			break;
		case ESFX.FoleyExplosionLarge:
			list.Add("Foley/sfx_en_gen_explode");
			flag = true;
			break;
		case ESFX.FoleyExplosionPoof:
			list.Add("Foley/sfx_en_gen_explode_poof");
			flag = true;
			break;
		case ESFX.FoleyHit:
			list.Add("Foley/sfx_foley_hit");
			break;
		case ESFX.FoleyLanternBrazierBreak:
			list.Add("Foley/sfx_lantern_brazier_break");
			flag = true;
			break;
		case ESFX.FoleyLanternExtinguish:
			list.Add("Foley/sfx_lantern_extinguish");
			flag = true;
			break;
		case ESFX.FoleyLanternFruitBreak:
			list.Add("Foley/sfx_lantern_fruit_break");
			break;
		case ESFX.FoleyLanternGlassBreak:
			list.Add("Foley/sfx_lantern_glass_break");
			break;
		case ESFX.FoleyOrbPedestalBreak:
			list.Add("Foley/sfx_orb_pedestal_break");
			break;
		case ESFX.FoleyOrbPedestalLoop:
			list.Add("Foley/sfx_orb_pedestal_loop");
			break;
		case ESFX.FoleySpikeDamage:
			list.Add("Foley/sfx_spikes_hit");
			break;
		case ESFX.FoleyWarpGyreIn:
			list.Add("Foley/sfx_warp_gyre_in");
			break;
		case ESFX.FoleyWarpGyreOut:
			list.Add("Foley/sfx_warp_gyre_out");
			break;
		case ESFX.FoleyWarpCutsceneActivate:
			list.Add("Foley/sfx_warp_cutscene_activate");
			break;
		case ESFX.FoleyWarpCutsceneExit:
			list.Add("Foley/sfx_warp_cutscene_exit");
			break;
		case ESFX.FoleyWaterSplashShallow:
			list.Add("Foley/sfx_water_shallow_splash_01");
			list.Add("Foley/sfx_water_shallow_splash_02");
			list.Add("Foley/sfx_water_shallow_splash_03");
			list.Add("Foley/sfx_water_shallow_splash_04");
			flag = true;
			break;
		case ESFX.FoleyWaterSplashDeep:
			list.Add("Foley/sfx_water_deep_splash_01");
			list.Add("Foley/sfx_water_deep_splash_02");
			list.Add("Foley/sfx_water_deep_splash_03");
			list.Add("Foley/sfx_water_deep_splash_04");
			flag = true;
			break;
		case ESFX.FoleyWetExplosionLarge:
			list.Add("Foley/sfx_en_gen_explode_wet_large");
			flag = true;
			break;
		case ESFX.FoleyWetExplosionSmall:
			list.Add("Foley/sfx_en_gen_explode_wet_small");
			flag = true;
			break;
		case ESFX.ItemGetDownload:
			list.Add("Foley/sfx_item_download_file");
			break;
		case ESFX.ItemGetMoney:
			list.Add("Foley/sfx_item_money");
			break;
		case ESFX.ItemGetGeneral:
			list.Add("Foley/sfx_item_general");
			break;
		case ESFX.ItemGetOrb:
			list.Add("Foley/sfx_orb_item_get");
			break;
		case ESFX.ItemGetRestore:
			list.Add("Foley/sfx_pickup_small");
			break;
		case ESFX.LunaisBackdash:
			list.Add("Lunais/01_Movement/sfx_lunais_backdash");
			flag = true;
			break;
		case ESFX.LunaisCrouch:
			list.Add("Lunais/01_Movement/sfx_lunais_crouch");
			flag = true;
			break;
		case ESFX.LunaisChargeImpact:
			list.Add("Lunais/03_Spell/sfx_lunais_charge_impact");
			break;
		case ESFX.LunaisFistClasp:
			list.Add("Lunais/00_Misc/sfx_lunais_fistclasp");
			break;
		case ESFX.LunaisJump:
			list.Add("Lunais/01_Movement/sfx_lunais_jump");
			break;
		case ESFX.LunaisJumpDown:
			list.Add("Lunais/01_Movement/sfx_lunais_jumpdown");
			break;
		case ESFX.LunaisLand:
			list.Add("Lunais/01_Movement/sfx_lunais_land");
			break;
		case ESFX.LunaisLedgeGrab:
			list.Add("Lunais/01_Movement/sfx_lunais_ledgegrab");
			break;
		case ESFX.LunaisChargeShoot:
			list.Add("Lunais/03_Spell/sfx_lunais_charge_shoot");
			break;
		case ESFX.LunaisChargeStart:
			list.Add("Lunais/03_Spell/sfx_lunais_charge_st");
			break;
		case ESFX.LunaisChargeLoop:
			list.Add("Lunais/03_Spell/sfx_lunais_charge_lp");
			break;
		case ESFX.LunaisChargeFlashMedium:
			list.Add("Lunais/03_Spell/sfx_lunais_charge_flash_1");
			break;
		case ESFX.LunaisChargeFlashLarge:
			list.Add("Lunais/03_Spell/sfx_lunais_charge_flash_2");
			break;
		case ESFX.LunaisDeathCutscene:
			list.Add("Lunais/00_Misc/sfx_lunais_death_cutscene");
			break;
		case ESFX.LunaisDoubleJump:
			list.Add("Lunais/01_Movement/sfx_lunais_double_jump");
			break;
		case ESFX.LunaisGiantHammerWhiff:
			list.Add("Lunais/03_Spell/sfx_lunais_giant_hammer_whiff");
			break;
		case ESFX.LunaisGiantSwordWhiff:
			list.Add("Lunais/03_Spell/sfx_lunais_giant_sword_whiff");
			break;
		case ESFX.LunaisLargeLazerShoot:
			list.Add("Lunais/03_Spell/sfx_lunais_largelazershoot");
			break;
		case ESFX.LunaisLargeLazerShoot2D:
			list.Add("Lunais/03_Spell/sfx_lunais_largelazershoot_2d");
			break;
		case ESFX.LunaisOrbBloodReturnFinish:
			list.Add("Lunais/02_Melee/sfx_lunais_orb_blood_return_finish");
			flag = true;
			break;
		case ESFX.LunaisOrbBloodReturnLoop:
			list.Add("Lunais/02_Melee/sfx_lunais_orb_blood_return_loop_01");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_blood_return_loop_02");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_blood_return_loop_03");
			break;
		case ESFX.LunaisOrbBloodWhiffLunais:
			list.Add("Lunais/02_Melee/sfx_lunais_orb_blood_whiff_lunais_01");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_blood_whiff_lunais_02");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_blood_whiff_lunais_03");
			flag = true;
			break;
		case ESFX.LunaisOrbBloodWhiffTarget:
			list.Add("Lunais/02_Melee/sfx_lunais_orb_blood_whiff_target_01");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_blood_whiff_target_02");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_blood_whiff_target_03");
			flag = true;
			break;
		case ESFX.LunaisOrbBloodSpell:
			list.Add("Lunais/03_Spell/sfx_lunais_orb_blood_spell");
			flag = true;
			break;
		case ESFX.LunaisOrbBookLoop:
			list.Add("Lunais/02_Melee/sfx_lunais_orb_book_loop_01");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_book_loop_02");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_book_loop_03");
			break;
		case ESFX.LunaisOrbBookMelee:
			list.Add("Lunais/02_Melee/sfx_lunais_orb_book_melee_01");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_book_melee_02");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_book_melee_03");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_book_melee_04");
			flag = true;
			break;
		case ESFX.LunaisOrbBookSpell:
			list.Add("Lunais/03_Spell/sfx_lunais_orb_book_spell");
			flag = true;
			break;
		case ESFX.LunaisOrbBookSpell2D:
			list.Add("Lunais/03_Spell/sfx_lunais_orb_book_spell_2d");
			break;
		case ESFX.LunaisOrbEmpireMelee:
			list.Add("Lunais/02_Melee/sfx_lunais_orb_empire_melee_01");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_empire_melee_02");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_empire_melee_03");
			flag = true;
			break;
		case ESFX.LunaisOrbEmpireSpell:
			list.Add("Lunais/03_Spell/sfx_lunais_orb_empire_spell");
			break;
		case ESFX.LunaisOrbEmpireSpell2D:
			list.Add("Lunais/03_Spell/sfx_lunais_orb_empire_spell_2d");
			break;
		case ESFX.LunaisOrbEyeWhiff:
			list.Add("Lunais/02_Melee/sfx_lunais_orb_eye_whiff_01");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_eye_whiff_02");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_eye_whiff_03");
			flag = true;
			break;
		case ESFX.LunaisOrbEyeSpell:
			list.Add("Lunais/03_Spell/sfx_lunais_orb_eye_spell");
			break;
		case ESFX.LunaisOrbEyeSpell2D:
			list.Add("Lunais/03_Spell/sfx_lunais_orb_eye_spell_2d");
			break;
		case ESFX.LunaisOrbFire:
			list.Add("Lunais/02_Melee/sfx_lunais_orb_fire_whiff_01");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_fire_whiff_02");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_fire_whiff_03");
			flag = true;
			break;
		case ESFX.LunaisOrbFireSpell:
			list.Add("Lunais/03_Spell/sfx_lunais_orb_fire_spell");
			break;
		case ESFX.LunaisOrbGunMelee:
			list.Add("Lunais/02_Melee/sfx_lunais_orb_gun_melee_01");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_gun_melee_02");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_gun_melee_03");
			flag = true;
			break;
		case ESFX.LunaisOrbGunSpell:
			list.Add("Lunais/03_Spell/sfx_lunais_orb_gun_spell");
			break;
		case ESFX.LunaisOrbGunSpell2D:
			list.Add("Lunais/03_Spell/sfx_lunais_orb_gun_spell_2d");
			break;
		case ESFX.LunaisOrbIceGrow:
			list.Add("Lunais/02_Melee/sfx_lunais_orb_ice_grow_01");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_ice_grow_02");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_ice_grow_03");
			flag = true;
			break;
		case ESFX.LunaisOrbIceWhiff:
			list.Add("Lunais/02_Melee/sfx_lunais_orb_ice_whiff_01");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_ice_whiff_02");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_ice_whiff_03");
			flag = true;
			break;
		case ESFX.LunaisOrbIceSpellEmerge:
			list.Add("Lunais/03_Spell/sfx_lunais_orb_ice_spell_emerge_01");
			list.Add("Lunais/03_Spell/sfx_lunais_orb_ice_spell_emerge_02");
			list.Add("Lunais/03_Spell/sfx_lunais_orb_ice_spell_emerge_03");
			break;
		case ESFX.LunaisOrbIceSpellThrow:
			list.Add("Lunais/03_Spell/sfx_lunais_orb_ice_spell_throw");
			break;
		case ESFX.LunaisOrbIceSpellThrow2D:
			list.Add("Lunais/03_Spell/sfx_lunais_orb_ice_spell_throw_2d");
			break;
		case ESFX.LunaisOrbImpact:
			list.Add("Lunais/02_Melee/sfx_lunais_orb_impact");
			flag = true;
			break;
		case ESFX.LunaisOrbImpactBullet:
			list.Add("Lunais/02_Melee/sfx_lunais_orb_impact_bullet");
			flag = true;
			break;
		case ESFX.LunaisOrbImpactBurn:
			list.Add("Lunais/02_Melee/sfx_lunais_orb_impact_burn");
			flag = true;
			break;
		case ESFX.LunaisOrbImpactDark:
			list.Add("Lunais/02_Melee/sfx_lunais_orb_impact_dark");
			flag = true;
			break;
		case ESFX.LunaisOrbImpactEmpire:
			list.Add("Lunais/02_Melee/sfx_lunais_orb_impact_empire");
			flag = true;
			break;
		case ESFX.LunaisOrbImpactIce:
			list.Add("Lunais/02_Melee/sfx_lunais_orb_impact_ice_01");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_impact_ice_02");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_impact_ice_03");
			flag = true;
			break;
		case ESFX.LunaisOrbImpactIceTinyEnemy:
			list.Add("Lunais/04_Passive/sfx_lunais_orb_tiny_icicle_hit_enemy");
			flag = true;
			break;
		case ESFX.LunaisOrbImpactIceTinyOther:
			list.Add("Lunais/04_Passive/sfx_lunais_orb_tiny_icicle_hit_other");
			flag = true;
			break;
		case ESFX.LunaisOrbImpactBlunt:
			list.Add("Lunais/02_Melee/sfx_lunais_orb_impact_blunt");
			flag = true;
			break;
		case ESFX.LunaisOrbImpactLightning:
			list.Add("Lunais/02_Melee/sfx_lunais_orb_lightning_impact_01");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_lightning_impact_02");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_lightning_impact_03");
			flag = true;
			break;
		case ESFX.LunaisOrbImpactSharp:
			list.Add("Lunais/02_Melee/sfx_lunais_orb_impact_sharp");
			flag = true;
			break;
		case ESFX.LunaisOrbLightningThrow:
			list.Add("Lunais/02_Melee/sfx_lunais_orb_lightning_throw_01");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_lightning_throw_02");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_lightning_throw_03");
			flag = true;
			break;
		case ESFX.LunaisOrbNetherWhiff:
			list.Add("Lunais/02_Melee/sfx_lunais_orb_nether_whiff_01");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_nether_whiff_02");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_nether_whiff_03");
			flag = true;
			break;
		case ESFX.LunaisOrbNetherMeleeVoid:
			list.Add("Lunais/02_Melee/sfx_lunais_orb_nether_melee_void");
			flag = true;
			break;
		case ESFX.LunaisOrbNetherSpellCast:
			list.Add("Lunais/03_Spell/sfx_lunais_orb_nether_spell_cast");
			flag = true;
			break;
		case ESFX.LunaisOrbNetherSpellSeek:
			list.Add("Lunais/03_Spell/sfx_lunais_orb_nether_spell_seek");
			flag = true;
			break;
		case ESFX.LunaisOrbRadiantMelee:
			list.Add("Lunais/02_Melee/sfx_lunais_orb_radiant_melee_01");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_radiant_melee_02");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_radiant_melee_03");
			flag = true;
			break;
		case ESFX.LunaisOrbRadiantSpell:
			list.Add("Lunais/03_Spell/sfx_lunais_orb_radiant_spell");
			flag = true;
			break;
		case ESFX.LunaisOrbRadiantSpell2D:
			list.Add("Lunais/03_Spell/sfx_lunais_orb_radiant_spell_2d");
			break;
		case ESFX.LunaisOrbRadiantSpellWallFade:
			list.Add("Lunais/03_Spell/sfx_lunais_orb_radiant_spell_wallfade");
			flag = true;
			break;
		case ESFX.LunaisOrbSlashHeavy:
			list.Add("Lunais/02_Melee/sfx_lunais_orb_slash_heavy_whiff_01");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_slash_heavy_whiff_02");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_slash_heavy_whiff_03");
			flag = true;
			break;
		case ESFX.LunaisOrbShatteredWhiff:
			list.Add("Lunais/02_Melee/sfx_lunais_orb_shattered_whiff_01");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_shattered_whiff_02");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_shattered_whiff_03");
			flag = true;
			break;
		case ESFX.LunaisOrbShatteredSpell:
			list.Add("Lunais/03_Spell/sfx_lunais_orb_shattered_spell");
			flag = true;
			break;
		case ESFX.LunaisOrbShatteredSpell2D:
			list.Add("Lunais/03_Spell/sfx_lunais_orb_shattered_spell_2d");
			flag = true;
			break;
		case ESFX.LunaisOrbSlashLight:
			list.Add("Lunais/02_Melee/sfx_lunais_orb_slash_light_whiff_01");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_slash_light_whiff_02");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_slash_light_whiff_03");
			flag = true;
			break;
		case ESFX.LunaisOrbSlashWind:
			list.Add("Lunais/02_Melee/sfx_lunais_orb_slash_wind_whiff_01");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_slash_wind_whiff_02");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_slash_wind_whiff_03");
			flag = true;
			break;
		case ESFX.LunaisOrbSwitch:
			list.Add("Lunais/00_Misc/sfx_lunais_orb_switch");
			flag = true;
			break;
		case ESFX.LunaisOrbThrow:
			list.Add("Lunais/02_Melee/sfx_lunais_orb_throw_01");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_throw_02");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_throw_03");
			flag = true;
			break;
		case ESFX.LunaisOrbPassiveIce:
			list.Add("Lunais/04_Passive/sfx_lunais_orb_tiny_shoot_01");
			list.Add("Lunais/04_Passive/sfx_lunais_orb_tiny_shoot_02");
			list.Add("Lunais/04_Passive/sfx_lunais_orb_tiny_shoot_03");
			flag = true;
			break;
		case ESFX.LunaisOrbPlasmaWhiff:
			list.Add("Lunais/02_Melee/sfx_lunais_orb_plasma_whiff_01");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_plasma_whiff_02");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_plasma_whiff_03");
			flag = true;
			break;
		case ESFX.LunaisOrbUmbraSpell:
			list.Add("Lunais/03_Spell/sfx_lunais_orb_umbra_spell");
			break;
		case ESFX.LunaisOrbUmbraSpell2D:
			list.Add("Lunais/03_Spell/sfx_lunais_orb_umbra_spell_2d");
			break;
		case ESFX.LunaisOrbUmbraWhiff:
			list.Add("Lunais/02_Melee/sfx_lunais_orb_umbra_whiff_01");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_umbra_whiff_02");
			list.Add("Lunais/02_Melee/sfx_lunais_orb_umbra_whiff_03");
			flag = true;
			break;
		case ESFX.LunaisOrbWindSpell:
			list.Add("Lunais/03_Spell/sfx_lunais_orb_wind_spell");
			break;
		case ESFX.LunaisOrbsAppear:
			list.Add("Lunais/00_Misc/sfx_lunais_orbs_appear");
			flag = true;
			break;
		case ESFX.LunaisOrbsVanish:
			list.Add("Lunais/00_Misc/sfx_lunais_orbs_vanish");
			flag = true;
			break;
		case ESFX.LunaisPiercingHit:
			list.Add("Lunais/02_Melee/sfx_lunais_piercinghit");
			flag = true;
			break;
		case ESFX.LunaisShieldDeflect:
			list.Add("Lunais/04_Passive/sfx_lunais_shield_deflect");
			flag = true;
			break;
		case ESFX.LunaisStandFromCrouch:
			list.Add("Lunais/01_Movement/sfx_lunais_stand_from_crouch");
			flag = true;
			break;
		case ESFX.LunaisStandFromLyingDown:
			list.Add("Lunais/01_Movement/sfx_lunais_stand_from_lyingdown");
			break;
		case ESFX.LunaisSuperJump:
			list.Add("Lunais/01_Movement/sfx_lunais_super_jump");
			flag = true;
			break;
		case ESFX.LunaisSuperJumpImpact:
			list.Add("Lunais/01_Movement/sfx_lunais_super_jump_impact");
			flag = true;
			break;
		case ESFX.LunaisTakeDamage:
			list.Add("Lunais/00_Misc/sfx_lunais_takedamage");
			break;
		case ESFX.LunaisTimeGateWarpin:
			list.Add("Lunais/00_Misc/sfx_lunais_warpin");
			break;
		case ESFX.LunaisTimeStop:
			list.Add("Lunais/00_Misc/sfx_lunais_time_stop");
			break;
		case ESFX.LunaisTimeUnstop:
			list.Add("Lunais/00_Misc/sfx_lunais_time_unstop");
			break;
		case ESFX.MenuBuy:
			list.Add("UI/sfx_menu_buy");
			break;
		case ESFX.MenuCancel:
			list.Add("UI/sfx_menu_cancel");
			break;
		case ESFX.MenuEquip:
			list.Add("UI/sfx_menu_equip");
			break;
		case ESFX.MenuError:
			list.Add("UI/sfx_menu_error");
			break;
		case ESFX.MenuHeal:
			list.Add("UI/sfx_menu_heal");
			break;
		case ESFX.MenuMove:
			list.Add("UI/sfx_menu_move");
			break;
		case ESFX.MenuSelect:
			list.Add("UI/sfx_menu_select");
			break;
		case ESFX.MenuSell:
			list.Add("UI/sfx_menu_sell");
			break;
		case ESFX.MeyefDamaged:
			list.Add("Familiar/sfx_meyef_hurt_01");
			list.Add("Familiar/sfx_meyef_hurt_02");
			break;
		case ESFX.MeyefMeow:
			list.Add("Familiar/sfx_meyef_meow_01");
			list.Add("Familiar/sfx_meyef_meow_02");
			list.Add("Familiar/sfx_meyef_meow_03");
			break;
		case ESFX.MeyefPurr:
			list.Add("Familiar/sfx_meyef_purr_01");
			list.Add("Familiar/sfx_meyef_purr_02");
			break;
		case ESFX.FoleySaveStatueFlash:
			list.Add("Foley/sfx_save_statue_flash");
			break;
		case ESFX.FoleySaveStatueIdle:
			list.Add("Foley/sfx_save_statue_idle");
			break;
		case ESFX.FoleySaveStatueTouchAgain:
			list.Add("Foley/sfx_save_statue_2ndtouch");
			break;
		case ESFX.FoleyTreasureOpen:
			list.Add("Foley/sfx_treasure_open");
			break;
		case ESFX.VO_Duke_BigEffort:
			list.Add("Boss/vo_duke_bigeffort_02");
			list.Add("Boss/vo_duke_bigeffort_03");
			list.Add("Boss/vo_duke_bigeffort_04");
			list.Add("Boss/vo_duke_bigeffort_05");
			break;
		case ESFX.VO_Duke_Hurt:
			list.Add("Boss/vo_duke_hurt_01");
			list.Add("Boss/vo_duke_hurt_02");
			list.Add("Boss/vo_duke_hurt_03");
			list.Add("Boss/vo_duke_hurt_05");
			break;
		case ESFX.VO_Duke_SmallEffort:
			list.Add("Boss/vo_duke_smalleffort_01");
			list.Add("Boss/vo_duke_smalleffort_02");
			list.Add("Boss/vo_duke_smalleffort_03");
			list.Add("Boss/vo_duke_smalleffort_05");
			break;
		case ESFX.VO_Emp_BigEffort:
			list.Add("Boss/vo_emperor_bigeffort_01");
			list.Add("Boss/vo_emperor_bigeffort_03");
			list.Add("Boss/vo_emperor_bigeffort_05");
			list.Add("Boss/vo_emperor_bigeffort_07");
			break;
		case ESFX.VO_Emp_Hurt:
			list.Add("Boss/vo_emperor_hurt_01");
			list.Add("Boss/vo_emperor_hurt_02");
			list.Add("Boss/vo_emperor_hurt_03");
			list.Add("Boss/vo_emperor_hurt_05");
			break;
		case ESFX.VO_Emp_SmallEffort:
			list.Add("Boss/vo_emperor_smalleffort_02");
			list.Add("Boss/vo_emperor_smalleffort_03");
			list.Add("Boss/vo_emperor_smalleffort_04");
			list.Add("Boss/vo_emperor_smalleffort_05");
			break;
		case ESFX.VO_Lun_Charge:
			list.Add("Lunais/lun_vo_charge_01");
			list.Add("Lunais/lun_vo_charge_02");
			list.Add("Lunais/lun_vo_charge_03");
			list.Add("Lunais/lun_vo_charge_04");
			break;
		case ESFX.VO_Lun_ChargeReleaseLarge:
			list.Add("Lunais/lun_vo_chargerelease_lrg_01");
			list.Add("Lunais/lun_vo_chargerelease_lrg_02");
			list.Add("Lunais/lun_vo_chargerelease_lrg_03");
			list.Add("Lunais/lun_vo_chargerelease_lrg_04");
			break;
		case ESFX.VO_Lun_ChargeReleaseMedium:
			list.Add("Lunais/lun_vo_chargerelease_med_01");
			list.Add("Lunais/lun_vo_chargerelease_med_02");
			list.Add("Lunais/lun_vo_chargerelease_med_03");
			list.Add("Lunais/lun_vo_chargerelease_med_04");
			list.Add("Lunais/lun_vo_chargerelease_med_05");
			list.Add("Lunais/lun_vo_chargerelease_med_06");
			break;
		case ESFX.VO_Lun_Jump:
			list.Add("Lunais/lun_vo_jump_01");
			list.Add("Lunais/lun_vo_jump_02");
			list.Add("Lunais/lun_vo_jump_03");
			break;
		case ESFX.VO_Lun_OrbThrow:
			list.Add("Lunais/lun_vo_orbthrow_01");
			list.Add("Lunais/lun_vo_orbthrow_02");
			list.Add("Lunais/lun_vo_orbthrow_03");
			list.Add("Lunais/lun_vo_orbthrow_04");
			list.Add("Lunais/lun_vo_orbthrow_05");
			list.Add("Lunais/lun_vo_orbthrow_06");
			list.Add("Lunais/lun_vo_orbthrow_07");
			list.Add("Lunais/lun_vo_orbthrow_08");
			list.Add("Lunais/lun_vo_orbthrow_09");
			list.Add("Lunais/lun_vo_orbthrow_10");
			break;
		case ESFX.VO_Lun_TakeDamage:
			list.Add("Lunais/lun_vo_takedamage_01");
			list.Add("Lunais/lun_vo_takedamage_02");
			list.Add("Lunais/lun_vo_takedamage_03");
			list.Add("Lunais/lun_vo_takedamage_04");
			list.Add("Lunais/lun_vo_takedamage_05");
			break;
		case ESFX.VO_Lun_TimeStop:
			list.Add("Lunais/lun_vo_timestop_01");
			list.Add("Lunais/lun_vo_timestop_02");
			list.Add("Lunais/lun_vo_timestop_03");
			break;
		default:
			list.Add("Unknown");
			break;
		}
		if (flag)
		{
			_pitchRangeMin = -0.15f;
			_pitchRangeMax = 0.15f;
			_volumeRangeMin = 0.85f;
			_volumeRangeMax = 1.15f;
		}
		return list;
	}

	private static string GetBGMNameFromEnum(EBGM song)
	{
		switch (song)
		{
		case EBGM.Boss01:
			return "BGM_Boss1";
		case EBGM.Boss02:
			return "BGM_Boss2";
		case EBGM.Boss05A:
			return "BGM_Boss5a";
		case EBGM.Boss05B:
			return "BGM_Boss5b";
		case EBGM.Boss06:
			return "BGM_Boss6";
		case EBGM.Boss07:
			return "BGM_Boss7";
		case EBGM.Boss08:
			return "BGM_Boss8";
		case EBGM.Boss11:
			return "BGM_Boss11";
		case EBGM.Boss12:
		case EBGM.Boss13:
			return "BGM_Boss12";
		case EBGM.Boss15:
			return "BGM_Boss15";
		case EBGM.Boss16:
			return "BGM_Boss16";
		case EBGM.Level01:
			return "BGM_Level1";
		case EBGM.Level02:
			return "BGM_Level2";
		case EBGM.Level03:
			return "BGM_Level3";
		case EBGM.Level04:
			return "BGM_Level4";
		case EBGM.Level05:
			return "BGM_Level5";
		case EBGM.Level06:
			return "BGM_Level6";
		case EBGM.Level07:
			return "BGM_Level7";
		case EBGM.Level08:
			return "BGM_Level8";
		case EBGM.Level09:
			return "BGM_Level9";
		case EBGM.Level10:
			return "BGM_Level10";
		case EBGM.Level11:
			return "BGM_Level11";
		case EBGM.Level12:
			return "BGM_Level12";
		case EBGM.Level14:
			return "BGM_Level14";
		case EBGM.Level15:
			return "BGM_Level15";
		case EBGM.Level16:
			return "BGM_Level16";
		case EBGM.TitleScreen:
			return "BGM_Title";
		case EBGM.Sanctuary:
			return "BGM_Sanctuary";
		case EBGM.Library:
			return "BGM_Library";
		case EBGM.Credits:
			return "BGM_Credits";
		case EBGM.CsBirthday:
			return "BGM_CS_Birthday";
		case EBGM.CsSelen:
			return "BGM_CS_Selen";
		case EBGM.CsCrow:
			return "BGM_CS_Crow";
		case EBGM.CsPortal:
			return "BGM_CS_Portal";
		case EBGM.CsPortal2:
			return "BGM_CS_Portal2";
		case EBGM.CsLetters:
			return "BGM_CS_Letters";
		case EBGM.CsLakeDesolation:
			return "BGM_Level1_Pre";
		case EBGM.CsSoldiers:
			return "BGM_CS_Soldiers";
		case EBGM.CsEmperor:
			return "BGM_CS_Emperor";
		default:
			return "BGM_Level1";
		}
	}

	public void SetVolumesBySave(GameConfigSave saveFile)
	{
		if (saveFile != null)
		{
			AdjustGlobalVolume(saveFile.AudioVolumeMaster + 1f, EVolumeCategory.Master);
			AdjustGlobalVolume(saveFile.AudioVolumeSFX + 1f, EVolumeCategory.SFX);
			AdjustGlobalVolume(saveFile.AudioVolumeVO + 1f, EVolumeCategory.Voice);
			AdjustGlobalVolume(saveFile.AudioVolumeMusic + 1f, EVolumeCategory.Music);
		}
	}

	internal void SetUnOwnedCuesToNonUpdate()
	{
		foreach (SFXCueInstance sfxCueInstance in _sfxCueInstances)
		{
			if (sfxCueInstance != null && sfxCueInstance.Anchor == null && sfxCueInstance.UpdateType == SFXCueInstance.ECueInstanceUpdateType.Stationary)
			{
				sfxCueInstance.UpdateType = SFXCueInstance.ECueInstanceUpdateType.None;
			}
		}
	}
}
