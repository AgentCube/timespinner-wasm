using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameStateManagement.ScreenManager;

namespace Timespinner.GameStateManagement.Saving;

internal class SaveFileManager
{
	internal const string StorageDirectoryName = "Timespinner";

	private const string SaveFileName = "TSSave.sav";

	private const string ConfigFileName = "Config.sav";

	private readonly bool _doesUseStorageDevice;

	private readonly object _saveLoadLock;

	private readonly Timespinner.GameStateManagement.ScreenManager.ScreenManager _screenManager;

	private readonly List<GameSave> _availableSaves = new List<GameSave>();

	private bool _isGameLoadInProgress;

	private bool _isSaveInProgress;

	private bool _isDeleteRequested;

	private bool _isSaveRequested;

	private bool _isSavingConfigFile;

	private bool _isWaitingForDevicePicked;

	private bool _isDevicePromptShowing;

	private bool _doesNeedToShowErrorMessage;

	private bool _doesNeedToShowConfigSaveFailureMessage;

	private bool _doesNeedToShowDeleteErrorMessage;

	private int _saveResult;

	private PlayerIndex? _personWhoPressed;

	private Task _saveThread;

	private Task _loadThread;

	private GameSave _saveToDelete;

	private StorageDevice _storageDevice;

	public bool IsThereNoSaveDevice { get; set; }

	internal bool? WasSaveAvailable { get; private set; }

	internal bool IsFinishedLoading => !_isGameLoadInProgress;

	internal bool IsFinishedSaving => !_isSaveInProgress;

	internal bool IsFinishedCheckingForSaves { get; private set; }

	internal bool WasSaveSuccessful => _saveResult == 0;

	public IAsyncResult StorageAsyncResult { get; set; }

	public GameSave GameSaveToSave { get; set; }

	public GameConfigSave ConfigSave { get; private set; }

	public List<GameSave> AvailableSaves
	{
		get
		{
			if (!OperatingSystem.IsBrowser() && _isGameLoadInProgress && _loadThread != null)
			{
				_loadThread.Wait();
			}
			return _availableSaves;
		}
	}

	internal SaveFileManager(Timespinner.GameStateManagement.ScreenManager.ScreenManager screenManager)
	{
		_screenManager = screenManager;
		_saveLoadLock = new object();
		_doesUseStorageDevice = true;
		if (OperatingSystem.IsBrowser())
		{
			try
			{
				IAsyncResult ar = StorageDevice.BeginShowSelector(null, null);
				_storageDevice = StorageDevice.EndShowSelector(ar);
			}
			catch (Exception ex)
			{
				Console.WriteLine("[SaveFileManager] StorageDevice init failed: " + ex);
			}
		}
	}

	internal void Update(float delta)
	{
		if (_isSaveRequested || _isDeleteRequested)
		{
			if (!IsThereNoSaveDevice)
			{
				if (_doesUseStorageDevice && _isWaitingForDevicePicked && StorageAsyncResult != null && StorageAsyncResult.IsCompleted)
				{
					if (!DidWeGetStorageDevice())
					{
						IsThereNoSaveDevice = true;
						_storageDevice = null;
					}
					_isWaitingForDevicePicked = false;
				}
				if (!_doesUseStorageDevice || (_storageDevice != null && _storageDevice.IsConnected))
				{
					if (_isSaveRequested)
					{
						if (!_isSaveInProgress)
						{
							_isSaveInProgress = true;
							_saveResult = 0;
							if (OperatingSystem.IsBrowser())
							{
								if (_isSavingConfigFile) SaveConfigFile(); else SaveGameFile();
							}
							else
							{
								_saveThread = (_isSavingConfigFile ? new Task(SaveConfigFile) : new Task(SaveGameFile));
								_saveThread.Start();
							}
						}
						_isSaveRequested = false;
					}
					else if (_isDeleteRequested && !_isSaveInProgress)
					{
						if (_saveToDelete != null)
						{
							_isSaveInProgress = true;
							if (OperatingSystem.IsBrowser())
							{
								DeleteFile(_saveToDelete);
							}
							else
							{
								_saveThread = new Task(delegate
								{
									DeleteFile(_saveToDelete);
								});
								_saveThread.Start();
							}
						}
						_isDeleteRequested = false;
					}
				}
				else if (_doesUseStorageDevice && !_isDevicePromptShowing)
				{
					ShowDisconnectedMessage();
				}
			}
			else if (_isSaveRequested)
			{
				_isSaveRequested = false;
			}
			else if (_isDeleteRequested)
			{
				_isDeleteRequested = false;
			}
		}
		if (_doesNeedToShowErrorMessage)
		{
			_screenManager.AddScreen(new MessageBoxScreen(Loc.Get("SaveFileCorrupt"), shouldIncludeUsageText: false, _screenManager.MenuControllerMapping), _personWhoPressed);
			_doesNeedToShowErrorMessage = false;
		}
		if (_doesNeedToShowConfigSaveFailureMessage)
		{
			_doesNeedToShowConfigSaveFailureMessage = false;
		}
		if (_doesNeedToShowDeleteErrorMessage)
		{
			_screenManager.AddScreen(new MessageBoxScreen(Loc.Get("SaveSelectDeleteFileFail"), shouldIncludeUsageText: false, _screenManager.MenuControllerMapping), _personWhoPressed);
			_doesNeedToShowDeleteErrorMessage = false;
		}
		UpdatePlatformSpecificSaving();
	}

	private void UpdatePlatformSpecificSaving()
	{
	}

	public bool DidWeGetStorageDevice()
	{
		bool result = false;
		if (_storageDevice == null)
		{
			if (OperatingSystem.IsBrowser())
			{
				try
				{
					_storageDevice = StorageDevice.EndShowSelector(StorageDevice.BeginShowSelector(null, null));
					return _storageDevice != null;
				}
				catch { }
			}
			if (StorageAsyncResult != null && StorageAsyncResult.IsCompleted)
			{
				_storageDevice = StorageDevice.EndShowSelector(StorageAsyncResult);
				if (_storageDevice != null)
				{
					result = true;
				}
			}
		}
		else if (_storageDevice.IsConnected)
		{
			result = true;
		}
		return result;
	}

	public bool RequestGameSave(GameSave gameToSave, PlayerIndex? playerIndex)
	{
		GameSaveToSave = gameToSave;
		if (playerIndex.HasValue)
		{
			_personWhoPressed = playerIndex;
			_isSaveRequested = true;
			_isSavingConfigFile = false;
		}
		return IsThereNoSaveDevice;
	}

	public bool RequestGameConfigSave()
	{
		_isSaveRequested = true;
		_isSavingConfigFile = true;
		return IsThereNoSaveDevice;
	}

	public bool RequestGameSaveDelete(GameSave saveToDelete)
	{
		_isDeleteRequested = true;
		_saveToDelete = saveToDelete;
		return IsThereNoSaveDevice;
	}

	private void SaveGameFile()
	{
		lock (_saveLoadLock)
		{
			try
			{
				string fileNameFromSave = GetFileNameFromSave(GameSaveToSave);
				_saveResult = XnaSaveLoad.XnaSaveGame(GameSaveToSave, fileNameFromSave, _storageDevice);
				GameSaveToSave = null;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to save game!" + ex.Message);
				_saveResult = 1;
			}
			_isSaveInProgress = false;
			_isSavingConfigFile = false;
		}
	}

	private void SaveConfigFile()
	{
		lock (_saveLoadLock)
		{
			try
			{
				_saveResult = XnaSaveLoad.XnaSaveConfig(ConfigSave, "Config.sav", _storageDevice);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to save config!" + ex.Message);
				_saveResult = 1;
			}
			if (_saveResult != 0)
			{
				_doesNeedToShowConfigSaveFailureMessage = true;
			}
			_isSaveInProgress = false;
			_isSavingConfigFile = false;
		}
	}

	private static string GetFileNameFromSave(GameSave gameSave)
	{
		return "TSSave.sav" + ((gameSave.SaveFileIndex == 0) ? "" : gameSave.SaveFileIndex.ToString(CultureInfo.InvariantCulture));
	}

	internal void UseDefaultConfigFile()
	{
		ConfigSave = new GameConfigSave
		{
			AudioVolumeMaster = 1f,
			AudioVolumeMusic = 1f,
			AudioVolumeSFX = 1f,
			AudioVolumeVO = 1f,
			PlayerControllerMapping = 
			{
				DoesUseControllerRumble = true
			}
		};
	}

	internal void LoadConfigFile()
	{
		lock (_saveLoadLock)
		{
			_isGameLoadInProgress = true;
			GameConfigSave gameConfigSave = null;
			try
			{
				gameConfigSave = XnaSaveLoad.XnaLoadGameConfig("Config.sav", _storageDevice);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to load config!" + ex.Message);
			}
			_isGameLoadInProgress = false;
			if (gameConfigSave == null)
			{
				UseDefaultConfigFile();
			}
			else
			{
				ConfigSave = gameConfigSave;
			}
		}
	}

	private void LoadAllSaveFiles()
	{
		lock (_saveLoadLock)
		{
			_isGameLoadInProgress = true;
			_availableSaves.Clear();
			List<GameSave> collection = XnaSaveLoad.XnaLoadAllGameSaves("TSSave.sav", _storageDevice, ShowLoadingErrorMessage);
			_availableSaves.AddRange(collection);
			_isGameLoadInProgress = false;
		}
	}

	internal static void CleanOldSaveFile(GameSave save)
	{
		if (save.Inventory.OrbSets == null)
		{
			save.Inventory.OrbSets = new List<OrbSet>();
		}
		if (save.Inventory.OrbSets.Count < 3)
		{
			save.Inventory.OrbSets.Add(new OrbSet());
			save.Inventory.OrbSets.Add(new OrbSet());
			save.Inventory.OrbSets.Add(new OrbSet());
		}
	}

	internal void PassiveCheckForGameSaveFile()
	{
		if (OperatingSystem.IsBrowser() && _storageDevice == null)
		{
			try
			{
				_storageDevice = StorageDevice.EndShowSelector(StorageDevice.BeginShowSelector(null, null));
			}
			catch { }
		}
		IsFinishedCheckingForSaves = false;
		WasSaveAvailable = CheckForGameSaveFile();
		IsFinishedCheckingForSaves = true;
	}

	public bool? CheckForGameSaveFile()
	{
		bool? result = null;
		if (_storageDevice != null || !_doesUseStorageDevice)
		{
			return XnaSaveLoad.XnaCheckForAnySaveFile("TSSave.sav", _storageDevice);
		}
		return result;
	}

	public void ReleaseDevice()
	{
		if (!OperatingSystem.IsBrowser())
		{
			_storageDevice = null;
		}
		StorageAsyncResult = null;
	}

	private void ShowDisconnectedMessage()
	{
		if (OperatingSystem.IsBrowser())
		{
			return;
		}
		MessageBoxScreen messageBoxScreen = new MessageBoxScreen(Loc.Get("DisconnectedStorageDevice"), _screenManager.MenuControllerMapping);
		messageBoxScreen.Accepted += ConfirmNoDeviceBoxAccepted;
		messageBoxScreen.Cancelled += ConfirmNoDeviceBoxCanceled;
		_screenManager.AddScreen(messageBoxScreen, _personWhoPressed);
		_isDevicePromptShowing = true;
	}

	private void ShowLoadingErrorMessage()
	{
		_doesNeedToShowErrorMessage = true;
	}

	private void ConfirmNoDeviceBoxAccepted(object sender, PlayerIndexEventArgs e)
	{
		_storageDevice = null;
		_isWaitingForDevicePicked = true;
		_personWhoPressed = e.PlayerIndex;
		StorageAsyncResult = StorageDevice.BeginShowSelector(null, null);
		_isDevicePromptShowing = false;
	}

	private void ConfirmNoDeviceBoxCanceled(object sender, PlayerIndexEventArgs e)
	{
		IsThereNoSaveDevice = true;
		_storageDevice = null;
		_isDevicePromptShowing = false;
	}

	internal bool TryGetStorageDevice()
	{
		bool result = true;
		if (StorageAsyncResult != null && _doesUseStorageDevice && StorageAsyncResult.IsCompleted)
		{
			result = DidWeGetStorageDevice();
		}
		return result;
	}

	internal void StartLoadAllSaves()
	{
		if (!_isGameLoadInProgress)
		{
			if (OperatingSystem.IsBrowser())
			{
				LoadAllSaveFiles();
			}
			else
			{
				_loadThread = new Task(LoadAllSaveFiles);
				_loadThread.Start();
			}
		}
	}

	internal void ReloadAllSaves()
	{
		if (!OperatingSystem.IsBrowser() && _isGameLoadInProgress && _loadThread != null)
		{
			_loadThread.Wait();
		}
		StartLoadAllSaves();
	}

	internal GameSave GetNewestSave()
	{
		GameSave gameSave = null;
		if (!OperatingSystem.IsBrowser() && _isGameLoadInProgress && _loadThread != null)
		{
			_loadThread.Wait();
		}
		DateTime dateTime = DateTime.MinValue;
		foreach (GameSave availableSafe in _availableSaves)
		{
			if (gameSave == null || availableSafe.FileWriteTime > dateTime)
			{
				gameSave = availableSafe;
				dateTime = availableSafe.FileWriteTime;
			}
		}
		return gameSave;
	}

	internal bool AreSaveFilesFull()
	{
		return _availableSaves.Count >= 8;
	}

	internal int GetNextSaveIndex()
	{
		HashSet<int> hashSet = new HashSet<int>();
		foreach (GameSave availableSafe in _availableSaves)
		{
			if (!hashSet.Contains(availableSafe.SaveFileIndex))
			{
				hashSet.Add(availableSafe.SaveFileIndex);
			}
		}
		int result = 7;
		for (int i = 0; i < 8; i++)
		{
			if (!hashSet.Contains(i))
			{
				result = i;
				break;
			}
		}
		return result;
	}

	internal bool DeleteFile(GameSave saveToDelete)
	{
		lock (_saveLoadLock)
		{
			string filename = "TSSave.sav" + ((saveToDelete.SaveFileIndex == 0) ? "" : saveToDelete.SaveFileIndex.ToString(CultureInfo.InvariantCulture));
			bool flag = XnaSaveLoad.DeleteFile(filename, _storageDevice);
			_availableSaves.Remove(saveToDelete);
			_doesNeedToShowDeleteErrorMessage = !flag;
			_isDeleteRequested = false;
			_isSaveInProgress = false;
			return flag;
		}
	}

	internal GameSave GetFreshSaveFromOld(GameSave gameSave)
	{
		GameSave result = null;
		if (gameSave != null)
		{
			if (_isGameLoadInProgress && _loadThread != null)
			{
				_loadThread.Wait();
			}
			foreach (GameSave availableSafe in _availableSaves)
			{
				if (availableSafe.SaveFileIndex == gameSave.SaveFileIndex)
				{
					result = availableSafe;
					break;
				}
			}
		}
		return result;
	}

	internal void NotifyPlayerOfSaveFailure(PlayerIndex? controllingPlayer)
	{
		XnaSaveLoad.XnaNotifyPlayerOfSaveFailure(_saveResult, _screenManager, controllingPlayer);
	}
}
