using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Timespinner.Core.Constants;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

namespace Timespinner.GameStateManagement.Screens.MainMenu;

internal class LocLoadingScreen : MenuScreen
{
	private readonly string _messageText;

	private readonly LocSelectionScreen _locSelectionScreen;

	private bool _hasFinishedLoading;

	private int _zoom;

	private Task _loadingTask;

	public LocLoadingScreen(GameConfigSave config, GCM gcm, bool isFirstTimeShowing)
		: base("")
	{
		_doesUseBlackGradientBox = false;
		_doesUseCursor = false;
		_messageText = Loc.Get("Loading");
		_locSelectionScreen = new LocSelectionScreen(config, gcm, isFirstTimeShowing);
		MenuEntry item = new MenuEntry(_messageText)
		{
			DoesDrawLargeShadow = true,
			DoesConfirmationPlaySound = false
		};
		base.MenuEntries.Add(item);
	}

	public override void LoadContent()
	{
		base.LoadContent();
		if (OperatingSystem.IsBrowser())
		{
			base.ScreenManager.PreloadScreen(_locSelectionScreen, base.ControllingPlayer);
			_hasFinishedLoading = true;
		}
		else
		{
			_loadingTask = new Task(delegate
			{
				base.ScreenManager.PreloadScreen(_locSelectionScreen, base.ControllingPlayer);
			});
			_loadingTask.Start();
		}
		RefreshSizes();
	}

	internal override void RefreshSizes()
	{
		_zoom = Constants.InGameZoom;
		base.RefreshSizes();
		Vector2 vector = base.ScreenManager.MenuFont.MeasureString(_messageText) * _zoom;
		base.MenuOffset = new Point(-(int)(vector.X / 2f), (int)vector.Y);
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
		if (_hasFinishedLoading || (_loadingTask != null && _loadingTask.IsCompleted))
		{
			base.ScreenManager.AddPreloadedScreen(_locSelectionScreen);
			base.ScreenManager.RemoveScreen(this);
		}
	}

	protected override void OnCancel(PlayerIndex playerIndex)
	{
	}
}
