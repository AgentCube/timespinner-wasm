using System;
using Steamworks;
using Timespinner.Core.Localization;

namespace Timespinner;

public class PlatformHelper
{
	private const string PresenceLevelKeyFormat = "level_{0:D2}";

	private const string DefaultPresenceKey = "default";

	private const int SteamAppID = 368620;

	private bool _hasCalledForStats;

	private CGameID _gameID;

	private Callback<UserStatsReceived_t> _onUserStatsReceived;

	private Callback<UserAchievementStored_t> _onUserAchievementStored;

	private Callback<UserStatsStored_t> _onUserStatsStored;

	private int _lastPresenceID = -999;

	internal bool DoesNeedToExit { get; private set; }

	internal bool IsSteamRunning { get; private set; }

	internal PlatformHelper()
	{
		try
		{
			if (SteamAPI.RestartAppIfNecessary((AppId_t)368620u))
			{
				Console.Out.WriteLine("Game wasn't started by Steam-client. Restarting.");
				DoesNeedToExit = true;
			}
		}
		catch (DllNotFoundException ex)
		{
			Console.Out.WriteLine("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + ex);
			DoesNeedToExit = true;
		}
	}

	internal void Initialize()
	{
		try
		{
			if (!SteamAPI.Init())
			{
				Console.WriteLine("SteamAPI.Init() failed!");
				return;
			}
			IsSteamRunning = true;
			_gameID = new CGameID(SteamUtils.GetAppID());
			_onUserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
			_onUserAchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);
			_onUserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
		}
		catch (DllNotFoundException value)
		{
			Console.WriteLine(value);
		}
	}

	internal void Update()
	{
		if (IsSteamRunning)
		{
			SteamAPI.RunCallbacks();
			if (!_hasCalledForStats)
			{
				_hasCalledForStats = SteamUserStats.RequestCurrentStats();
			}
		}
	}

	internal void Shutdown()
	{
		SteamAPI.Shutdown();
	}

	internal ELanguageLocale GuessLocale()
	{
		ELanguageLocale result = ELanguageLocale.EN;
		if (IsSteamRunning)
		{
			switch (SteamApps.GetCurrentGameLanguage())
			{
			case "schinese":
			case "tchinese":
				result = ELanguageLocale.CN;
				break;
			case "czech":
			case "russian":
				result = ELanguageLocale.RU;
				break;
			case "french":
				result = ELanguageLocale.FR;
				break;
			case "german":
				result = ELanguageLocale.DE;
				break;
			case "japanese":
				result = ELanguageLocale.JP;
				break;
			case "portuguese":
			case "brazilian":
				result = ELanguageLocale.BP;
				break;
			case "spanish":
			case "latam":
				result = ELanguageLocale.ES;
				break;
			}
		}
		return result;
	}

	private void OnUserStatsReceived(UserStatsReceived_t pCallback)
	{
		if (IsSteamRunning && (ulong)_gameID == pCallback.m_nGameID)
		{
			if (EResult.k_EResultOK == pCallback.m_eResult)
			{
				Console.WriteLine("Received stats and achievements from Steam\n");
			}
			else
			{
				Console.WriteLine("RequestStats - failed, " + pCallback.m_eResult);
			}
		}
	}

	private void OnAchievementStored(UserAchievementStored_t pCallback)
	{
		_ = (ulong)_gameID;
		_ = pCallback.m_nGameID;
	}

	private void OnUserStatsStored(UserStatsStored_t pCallback)
	{
		_ = (ulong)_gameID;
		_ = pCallback.m_nGameID;
	}

	internal void UpdateRichPresence(int presenceID)
	{
	}
}
