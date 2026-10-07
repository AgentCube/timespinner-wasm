using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameStateManagement.ScreenManager;

namespace Timespinner.GameStateManagement.Saving;

internal static class XnaSaveLoad
{
	private enum EXnaSaveResultType
	{
		Ok,
		FailDefault,
		FailUnauthorized,
		FailXml
	}

	private static string _xnaSaveExceptionMessage;

	internal static int XnaSaveGame(GameSave save, string filename, StorageDevice storageDevice)
	{
		//IL_00c7: Expected O, but got Unknown
		string file = filename + ".tmp";
		_xnaSaveExceptionMessage = "";
		IAsyncResult asyncResult = storageDevice.BeginOpenContainer("Timespinner", null, null);
		StorageContainer storageContainer = storageDevice.EndOpenContainer(asyncResult);
		asyncResult.AsyncWaitHandle.Close();
		try
		{
			if (storageContainer.FileExists(file))
			{
				storageContainer.DeleteFile(file);
			}
		}
		catch (UnauthorizedAccessException)
		{
			storageContainer.Dispose();
			return 2;
		}
		catch (Exception ex2)
		{
			storageContainer.Dispose();
			_xnaSaveExceptionMessage = ex2.Message;
			return 1;
		}
		Stream stream;
		try
		{
			stream = storageContainer.CreateFile(file);
		}
		catch (UnauthorizedAccessException)
		{
			storageContainer.Dispose();
			return 2;
		}
		catch (Exception ex4)
		{
			storageContainer.Dispose();
			_xnaSaveExceptionMessage = ex4.Message;
			return 1;
		}
		try
		{
			save.FileWriteTime = DateTime.Now;
			save.Save(stream);
		}
		catch (XmlException val)
		{
			XmlException val2 = val;
			stream.Close();
			storageContainer.Dispose();
			_xnaSaveExceptionMessage = ((Exception)(object)val2).Message;
			return 3;
		}
		catch (Exception ex5)
		{
			stream.Close();
			storageContainer.Dispose();
			_xnaSaveExceptionMessage = ex5.Message;
			return 1;
		}
		stream.Close();
		try
		{
			stream = storageContainer.OpenFile(file, FileMode.Open);
		}
		catch (Exception ex6)
		{
			storageContainer.Dispose();
			_xnaSaveExceptionMessage = ex6.Message;
			return 1;
		}
		try
		{
			if (storageContainer.FileExists(filename))
			{
				storageContainer.DeleteFile(filename);
			}
		}
		catch (UnauthorizedAccessException)
		{
			stream.Close();
			storageContainer.Dispose();
			return 2;
		}
		catch (Exception ex8)
		{
			stream.Close();
			storageContainer.Dispose();
			_xnaSaveExceptionMessage = ex8.Message;
			return 1;
		}
		Stream stream2;
		try
		{
			stream2 = storageContainer.CreateFile(filename);
		}
		catch (UnauthorizedAccessException)
		{
			stream.Close();
			storageContainer.Dispose();
			return 2;
		}
		catch (Exception ex10)
		{
			stream.Close();
			storageContainer.Dispose();
			_xnaSaveExceptionMessage = ex10.Message;
			return 1;
		}
		try
		{
			stream.CopyTo(stream2);
		}
		catch (Exception ex11)
		{
			stream.Close();
			stream2.Close();
			storageContainer.Dispose();
			_xnaSaveExceptionMessage = ex11.Message;
			return 1;
		}
		stream.Close();
		try
		{
			storageContainer.DeleteFile(file);
		}
		catch (Exception ex12)
		{
			Console.Write("Failed to delete temp file! " + ex12.Message);
		}
		stream2.Close();
		storageContainer.Dispose();
		return 0;
	}

	internal static int XnaSaveConfig(GameConfigSave save, string filename, StorageDevice storageDevice)
	{
		IAsyncResult asyncResult = storageDevice.BeginOpenContainer("Timespinner", null, null);
		StorageContainer storageContainer = storageDevice.EndOpenContainer(asyncResult);
		asyncResult.AsyncWaitHandle.Close();
		try
		{
			if (storageContainer.FileExists(filename))
			{
				storageContainer.DeleteFile(filename);
			}
		}
		catch (UnauthorizedAccessException)
		{
			storageContainer.Dispose();
			return 2;
		}
		catch (Exception)
		{
			storageContainer.Dispose();
			return 1;
		}
		Stream stream;
		try
		{
			stream = storageContainer.CreateFile(filename);
		}
		catch (UnauthorizedAccessException)
		{
			storageContainer.Dispose();
			return 2;
		}
		catch (Exception)
		{
			storageContainer.Dispose();
			return 1;
		}
		try
		{
			save.SaveXml(stream);
		}
		catch (XmlException)
		{
			stream.Close();
			storageContainer.Dispose();
			return 3;
		}
		catch (Exception)
		{
			stream.Close();
			storageContainer.Dispose();
			return 1;
		}
		stream.Close();
		storageContainer.Dispose();
		return 0;
	}

	internal static List<GameSave> XnaLoadAllGameSaves(string baseFileName, StorageDevice storageDevice, Action onLoadFailAction)
	{
		List<GameSave> list = new List<GameSave>();
		IAsyncResult asyncResult;
		StorageContainer storageContainer;
		try
		{
			asyncResult = storageDevice.BeginOpenContainer("Timespinner", null, null);
			asyncResult.AsyncWaitHandle.WaitOne();
			storageContainer = storageDevice.EndOpenContainer(asyncResult);
		}
		catch
		{
			return list;
		}
		asyncResult.AsyncWaitHandle.Close();
		bool flag = false;
		for (int i = 0; i < 8; i++)
		{
			string filename = ((i == 0) ? baseFileName : (baseFileName + i));
			GameSave gameSave = LoadGameSaveFile(storageContainer, filename);
			if (gameSave != null)
			{
				SaveFileManager.CleanOldSaveFile(gameSave);
				list.Add(gameSave);
				if (gameSave.IsCorrupt)
				{
					gameSave.SaveFileIndex = i;
					flag = true;
				}
			}
		}
		storageContainer.Dispose();
		if (flag)
		{
			onLoadFailAction();
		}
		return list;
	}

	private static GameSave LoadGameSaveFile(StorageContainer container, string filename)
	{
		GameSave result = null;
		if (container.FileExists(filename))
		{
			try
			{
				using Stream stream = container.OpenFile(filename, FileMode.Open);
				result = GameSave.Load(stream);
			}
			catch (Exception)
			{
				result = GameSave.CorruptSave;
			}
		}
		return result;
	}

	internal static GameConfigSave XnaLoadGameConfig(string filename, StorageDevice storageDevice)
	{
		IAsyncResult asyncResult;
		StorageContainer storageContainer;
		try
		{
			asyncResult = storageDevice.BeginOpenContainer("Timespinner", null, null);
			asyncResult.AsyncWaitHandle.WaitOne();
			storageContainer = storageDevice.EndOpenContainer(asyncResult);
		}
		catch
		{
			return null;
		}
		asyncResult.AsyncWaitHandle.Close();
		GameConfigSave result = LoadConfigSaveFile(storageContainer, filename);
		storageContainer.Dispose();
		return result;
	}

	private static GameConfigSave LoadConfigSaveFile(StorageContainer container, string filename)
	{
		GameConfigSave result = null;
		if (container.FileExists(filename))
		{
			using Stream stream = container.OpenFile(filename, FileMode.Open);
			result = GameConfigSave.LoadXml(stream);
		}
		return result;
	}

	internal static bool? XnaCheckForAnySaveFile(string baseFileName, StorageDevice storageDevice)
	{
		bool? flag = null;
		bool flag2 = false;
		bool flag3 = false;
		IAsyncResult asyncResult = null;
		StorageContainer storageContainer = null;
		try
		{
			asyncResult = storageDevice.BeginOpenContainer("Timespinner", null, null);
			asyncResult.AsyncWaitHandle.WaitOne();
			storageContainer = storageDevice.EndOpenContainer(asyncResult);
			flag3 = true;
		}
		catch
		{
			flag2 = true;
		}
		if (flag3 && storageContainer != null)
		{
			asyncResult.AsyncWaitHandle.Close();
			try
			{
				for (int i = 0; i < 8; i++)
				{
					string file = ((i == 0) ? baseFileName : (baseFileName + i));
					if (storageContainer.FileExists(file))
					{
						flag = true;
						break;
					}
				}
			}
			catch
			{
				flag2 = true;
			}
			storageContainer.Dispose();
		}
		if (flag != true && !flag2)
		{
			flag = false;
		}
		return flag;
	}

	internal static bool DeleteFile(string filename, StorageDevice storageDevice)
	{
		bool result = true;
		IAsyncResult asyncResult = storageDevice.BeginOpenContainer("Timespinner", null, null);
		StorageContainer storageContainer = storageDevice.EndOpenContainer(asyncResult);
		asyncResult.AsyncWaitHandle.Close();
		try
		{
			if (storageContainer.FileExists(filename))
			{
				storageContainer.DeleteFile(filename);
			}
		}
		catch
		{
			storageContainer.Dispose();
			result = false;
		}
		return result;
	}

	internal static void XnaNotifyPlayerOfSaveValidationFailure(Action<bool> onSaveValidationFinished, Timespinner.GameStateManagement.ScreenManager.ScreenManager screenManager, PlayerIndex? playerIndex)
	{
		MessageBoxScreen screen = new MessageBoxScreen(Loc.Get("SaveValidationError"), shouldIncludeUsageText: false, screenManager.MenuControllerMapping);
		screenManager.AddScreen(screen, playerIndex);
		onSaveValidationFinished(obj: true);
	}

	internal static void XnaNotifyPlayerOfSaveFailure(int saveResult, Timespinner.GameStateManagement.ScreenManager.ScreenManager screenManager, PlayerIndex? playerIndex)
	{
		screenManager.AddScreen((EXnaSaveResultType)saveResult switch
		{
			EXnaSaveResultType.FailUnauthorized => new MessageBoxScreen(Loc.Get("SaveGameFailAuthorization"), shouldIncludeUsageText: false, screenManager.MenuControllerMapping), 
			EXnaSaveResultType.FailXml => new MessageBoxScreen(string.Format(Loc.Get("SaveGameFailXml"), _xnaSaveExceptionMessage), shouldIncludeUsageText: false, screenManager.MenuControllerMapping), 
			_ => new MessageBoxScreen(string.Format(Loc.Get("SaveGameFailDefault"), _xnaSaveExceptionMessage), shouldIncludeUsageText: false, screenManager.MenuControllerMapping), 
		}, playerIndex);
	}
}
