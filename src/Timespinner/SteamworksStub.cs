// SteamworksStub.cs — Managed no-op stub for Steamworks.NET types used by Timespinner.
// Replaces Steamworks.NET.dll to allow pure managed execution on WebAssembly.

using System;

#pragma warning disable CS0067  // Event never used
#pragma warning disable CS0169  // Field never used
#pragma warning disable CS8625  // Cannot convert null literal to non-nullable reference type

namespace Steamworks
{
    public struct AppId_t
    {
        public uint m_AppId;
        public AppId_t(uint value) { m_AppId = value; }
        public static implicit operator uint(AppId_t a) => a.m_AppId;
        public static explicit operator AppId_t(uint value) => new AppId_t(value);
    }

    public struct CGameID
    {
        public ulong m_GameID;
        public CGameID(AppId_t appId) { m_GameID = (ulong)appId.m_AppId; }
        public CGameID(ulong value) { m_GameID = value; }
        public static explicit operator ulong(CGameID that) => that.m_GameID;
        public static implicit operator CGameID(ulong value) => new CGameID(value);
    }

    public enum EResult
    {
        k_EResultOK = 1,
        k_EResultFail = 2
    }

    public struct UserStatsReceived_t
    {
        public ulong m_nGameID;
        public EResult m_eResult;
    }

    public struct UserAchievementStored_t
    {
        public ulong m_nGameID;
        public string m_rgchAchievementName;
        public uint m_nCurProgress;
        public uint m_nMaxProgress;
    }

    public struct UserStatsStored_t
    {
        public ulong m_nGameID;
        public EResult m_eResult;
    }

    public sealed class Callback<T> : IDisposable
    {
        public static Callback<T> Create(Action<T> func) => new Callback<T>();
        public void Dispose() { }
    }

    public static class SteamAPI
    {
        public static bool RestartAppIfNecessary(AppId_t unOwnAppID) => false;
        public static bool Init() => false;
        public static void Shutdown() { }
        public static void RunCallbacks() { }
        public static bool IsSteamRunning() => false;
    }

    public static class SteamUtils
    {
        public static AppId_t GetAppID() => new AppId_t(368620);
    }

    public static class SteamApps
    {
        public static string GetCurrentGameLanguage() => "english";
    }

    public static class SteamUserStats
    {
        public static bool RequestCurrentStats() => false;
        public static bool SetAchievement(string pchName) => false;
        public static bool StoreStats() => false;
    }
}
