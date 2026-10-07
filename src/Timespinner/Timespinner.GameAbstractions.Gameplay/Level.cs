using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.Core.Definitions;
using Timespinner.Core.Localization;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.HUD;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameObjects;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Enemies;
using Timespinner.GameObjects.Enemies._04_Ramparts;
using Timespinner.GameObjects.Enemies._10_Fortress;
using Timespinner.GameObjects.Enemies._16_Temple;
using Timespinner.GameObjects.Events;
using Timespinner.GameObjects.Events.Cutscene;
using Timespinner.GameObjects.Events.Doors;
using Timespinner.GameObjects.Events.EnvironmentPrefabs;
using Timespinner.GameObjects.Events.Lanterns;
using Timespinner.GameObjects.Events.LevelEffects;
using Timespinner.GameObjects.Events.Misc;
using Timespinner.GameObjects.Events.Platforms;
using Timespinner.GameObjects.Events.Relics;
using Timespinner.GameObjects.Events.Treasure;
using Timespinner.GameObjects.Heroes;
using Timespinner.GameObjects.Heroes.Familiars;
using Timespinner.GameObjects.Items;
using Timespinner.GameObjects.NPCs;
using Timespinner.GameStateManagement.ScreenManager;
using Timespinner.GameStateManagement.Screens.InGame;

namespace Timespinner.GameAbstractions.Gameplay;

public sealed class Level : IDisposable
{
	public const int LevelCount = 18;

	public const int NexusLevelID = 17;

	private const int MaxQueueDialogues = 20;

	private const int BackerTerminalCount = 4;

	private const int BackerBustCount = 9;

	private const int BackerPaintingStart = 13;

	private const int LabPowerOffRedOffset = 48;

	private const int LabPowerOffGreenOffset = 16;

	private const int LabPowerOffDarknessAmplitude = 16;

	private const int LabPowerOffDarknessMidValue = 180;

	private const float ObjectVisibleAreaSizeMultiplier = 1.5f;

	private const float LabPowerOffDarknessFrequency = 2f;

	private const float TimeBeforeShowingAreaTitleAfterEnteringRoom = 0.15f;

	private const float CutsceneSkipFadeTime = 0.25f;

	private const string HasAreaTitleBeenShownKey = "AreaTitleShown{0}";

	private readonly bool _isEasyMode;

	private readonly bool _isHardMode;

	private readonly EEraType _era;

	private readonly int _id;

	private readonly Camera2D _camera;

	private readonly Random _random = new Random();

	private readonly LevelSpecification _levelSpecification;

	private readonly MinimapSpecification _minimapSpecification;

	private readonly Jukebox _jukebox;

	private readonly GCM _gcm;

	private readonly List<HudNumber> _hudNumberQueue = new List<HudNumber>();

	private readonly Dictionary<int, IEnumerable<BackgroundSpecification>> _knownWarpBackgrounds;

	private readonly TicketIDDispenser _objectTicketIDDispenser;

	private readonly TicketIDDispenser _projectileTicketIDDispenser;

	private readonly Dictionary<Point, Tile> _solidTiles = new Dictionary<Point, Tile>();

	private readonly Dictionary<Point, List<Tile>> _backgroundTiles = new Dictionary<Point, List<Tile>>();

	private readonly Dictionary<Point, List<Tile>> _foregroundTiles = new Dictionary<Point, List<Tile>>();

	private readonly Dictionary<Point, WaterTile> _waterTiles = new Dictionary<Point, WaterTile>();

	private Tile[] _solidTileGrid;

	private WaterTile[] _waterTileGrid;

	private List<Tile>[] _backgroundTileGrid;

	private List<Tile>[] _foregroundTileGrid;

	public Tile GetSolidTileFast(int x, int y)
	{
		int width = _levelTileSize.X;
		if ((uint)x < (uint)width && (uint)y < (uint)_levelTileSize.Y && _solidTileGrid != null)
		{
			int idx = y * width + x;
			if ((uint)idx < (uint)_solidTileGrid.Length)
			{
				return _solidTileGrid[idx];
			}
		}
		return null;
	}

	public WaterTile GetWaterTileFast(int x, int y)
	{
		int width = _levelTileSize.X;
		if ((uint)x < (uint)width && (uint)y < (uint)_levelTileSize.Y && _waterTileGrid != null)
		{
			int idx = y * width + x;
			if ((uint)idx < (uint)_waterTileGrid.Length)
			{
				return _waterTileGrid[idx];
			}
		}
		return null;
	}

	private readonly Dictionary<int, ConveyorBeltFloorEvent> _conveyorBelts = new Dictionary<int, ConveyorBeltFloorEvent>();

	private readonly List<TileSwath> _tileSwaths = new List<TileSwath>();

	private readonly List<WaterTile> _updatableWaterTiles = new List<WaterTile>();

	private readonly List<WaterFillerEvent> _waterFillerTiles = new List<WaterFillerEvent>();

	private readonly Dictionary<Point, Tile> _cameraBlockerTiles = new Dictionary<Point, Tile>();

	private readonly Dictionary<int, Protagonist> _protagonists = new Dictionary<int, Protagonist>();

	private readonly Dictionary<int, Monster> _enemies = new Dictionary<int, Monster>();

	private readonly Dictionary<int, NPCBase> _npcs = new Dictionary<int, NPCBase>();

	private readonly Dictionary<int, Projectile> _enemyProjectiles = new Dictionary<int, Projectile>();

	private readonly Dictionary<int, Projectile> _heroProjectiles = new Dictionary<int, Projectile>();

	private readonly Dictionary<int, Item> _items = new Dictionary<int, Item>();

	private readonly Dictionary<int, GameEvent> _levelEvents = new Dictionary<int, GameEvent>();

	private readonly List<SaveStatue> _checkpoints = new List<SaveStatue>();

	private readonly List<TeleportEvent> _teleportDoors = new List<TeleportEvent>();

	private readonly List<Background> _backgrounds = new List<Background>();

	private readonly List<Background> _foregrounds = new List<Background>();

	private readonly List<BattleAnimation> _enemyAnimations = new List<BattleAnimation>();

	private readonly List<BattleAnimation> _neutralAnimations = new List<BattleAnimation>();

	private readonly List<BattleAnimation> _heroAnimations = new List<BattleAnimation>();

	private readonly Dictionary<int, List<BattleAnimation>> _cachedAnimations = new Dictionary<int, List<BattleAnimation>>();

	private readonly List<Mobile> _deadObjects = new List<Mobile>();

	private readonly List<Mobile> _newObjects = new List<Mobile>();

	private readonly List<ScriptAction> _activeScripts = new List<ScriptAction>();

	private readonly Queue<ScriptAction> _waitingScripts = new Queue<ScriptAction>();

	private readonly Queue<ScreenEffect> _screenEffectQueue = new Queue<ScreenEffect>();

	private readonly Queue<DialogueBox> _dialogueQueue = new Queue<DialogueBox>();

	private readonly Queue<GameScreen> _screenAddQueue = new Queue<GameScreen>();

	private readonly HashSet<string> _levelSaveBools = new HashSet<string>();

	private readonly Dictionary<string, int> _levelSaveInts = new Dictionary<string, int>();

	private bool _areTeleportExitsUnlocked = true;

	private bool _isRoomReloadRequested;

	private bool _isCameraUpdateDisabled;

	private bool _hasAreaTitleBeenShown;

	private bool _isSkippingCutscene;

	private bool _isCutsceneSkipFading;

	private bool _isOverridingPowerOff;

	private ETeamSide _teamFreezing;

	private float _reloadTimer;

	private float _sepiaPercent = 1f;

	private float _areaTitleShowTimer;

	private float _levelShaderTimer;

	private float _cutsceneSkipFadeTimer;

	private Point _playerStart;

	private Point _levelTileSize;

	private Rectangle _visibleArea;

	private Rectangle _visibleArea16;

	private Rectangle _projectileVisibleArea;

	private Rectangle _objectVisibleArea;

	private Rectangle _levelCameraBounds;

	private TimeGateEvent _timeGateEvent;

	private TransitionWarpEvent _transitionWarpEvent;

	private ElevatorEvent _roomElevator;

	private LevelChangeRequest _levelChangeRequest;

	public bool IsInEditMode { get; set; }

	public bool IsPaused { get; set; }

	internal bool WasLastActive { get; set; }

	internal bool IsPlayerInputBlocked { get; set; }

	internal bool IsActiveScriptUnskippable { get; private set; }

	internal bool IsUIRequestingHide { get; set; }

	internal bool IsPreventingPauseMenuUsage { get; set; }

	internal bool IsLevelChangeRequested { get; set; }

	internal bool IsButtonPromptRequested { get; set; }

	internal bool IsFadeOutRequested { get; private set; }

	internal bool IsUsingWhiteFadeOut { get; private set; }

	internal bool IsRequestingGameOverScreen { get; private set; }

	internal bool IsInBossRoom { get; private set; }

	internal bool IsRequestingToast { get; set; }

	internal bool IsEndGameRequested { get; private set; }

	internal bool IsEndGameRequestingEndScreen { get; private set; }

	internal bool IsRequestingRollCredits { get; private set; }

	internal bool IsDoingPlayerDeathCutscene { get; private set; }

	internal bool IsPowerOff { get; private set; }

	internal bool IsOnVilete { get; set; }

	internal bool IsEasyMode => _isEasyMode;

	internal bool IsHardMode => _isHardMode;

	internal bool HasPlayerBeenDamagedInThisRoom { get; set; }

	internal bool HasPlayerFrozenTimeInThisRoom { get; set; }

	internal bool IsMufflingPlayerSFX { get; set; }

	internal EToastType RequestedToastType { get; private set; }

	internal ScriptAction RequestedToastScript { get; private set; }

	internal int RequestedToastArgument { get; private set; }

	internal bool AreTeleportExitsUnlocked => _areTeleportExitsUnlocked;

	internal bool IsInTransitionRoom => _transitionWarpEvent != null;

	public bool IsRoomChanged { get; set; }

	public bool IsTimeFrozen { get; set; }

	internal bool IsTimePermanentlyFrozen { get; private set; }

	public bool WasTimeFrozen { get; set; }

	public bool IsDialoguePlaying { get; set; }

	internal bool IsUsingGrayscaleEffect { get; set; }

	internal bool IsUsingCutsceneSepiaEffect { get; set; }

	public EDirection RoomEntrance { get; private set; }

	internal EEraType Era => _era;

	internal float GrayscaleFadePercentage { get; set; }

	internal float CutsceneSepiaEffectPercentage { get; set; }

	public int ID => _id;

	public int RoomIndex { get; private set; }

	public int RoomID { get; private set; }

	public int TotalRooms => _levelSpecification.Rooms.Count;

	public int LastRoomIndex { get; private set; }

	public int Player1Controller { get; private set; }

	public int ButtonTutorialDisplayIndex { get; set; }

	public float CameraZoom { get; set; }

	public float BackgroundZoom { get; set; }

	public float ButtonTutorialDisplayAmount { get; set; }

	internal bool IsRequestingItemGetPopup { get; set; }

	internal EInventoryCategoryType ItemGetCategory { get; set; }

	internal int ItemGetValue { get; set; }

	internal string EnemyHitName { get; set; }

	public float FullGrayscaleWipeColor { get; private set; }

	public Color BackgroundWipeColor { get; private set; }

	public Color LevelDrawColor { get; private set; }

	public Vector3 FullSepiaWipeColor { get; private set; }

	public Point RoomSize { get; private set; }

	public Point RoomSize16 => _levelTileSize;

	public Point ButtonPromptPosition { get; set; }

	public Vector2 CameraPosition { get; set; }

	public Vector2 LevelRenderCenter { get; private set; }

	public Vector2 MonitorScreenCenter { get; private set; }

	public Vector2 DisplaySize { get; private set; }

	public Vector2 VisibleSize { get; private set; }

	public Rectangle VisibleArea => _visibleArea;

	public Rectangle VisibleArea16 => _visibleArea16;

	internal Rectangle ObjectVisibleArea => _objectVisibleArea;

	internal AreaTitleBlockerEvent AreaTitleBlocker { get; private set; }

	public LevelChangeRequest LevelChangeRequest { get; set; }

	public SpriteSheet CurrentTileset { get; private set; }

	public Protagonist MainHero { get; private set; }

	public ControllerMapping PlayerControllerMapping => ConfigSave.PlayerControllerMapping;

	public GameSave GameSave { get; private set; }

	public GameConfigSave ConfigSave { get; private set; }

	internal RoomSpecification CurrentRoom { get; private set; }

	public LevelSpecification LevelSpecification => _levelSpecification;

	public MinimapSpecification Minimap => _minimapSpecification;

	public Jukebox JukeBox => _jukebox;

	public GCM GCM => _gcm;

	public List<Background> Backgrounds => _backgrounds;

	public List<Background> Foregrounds => _foregrounds;

	public IEnumerable<RoomSpecification> Rooms => _levelSpecification.Rooms;

	public Queue<ScreenEffect> ScreenEffectQueue => _screenEffectQueue;

	public Queue<GameScreen> ScreenAddQueue => _screenAddQueue;

	public List<HudNumber> HUDNumberQueue => _hudNumberQueue;

	public Dictionary<Point, Tile> SolidTiles => _solidTiles;

	public bool HasPendingRoomChange => _levelChangeRequest != null || IsFadeOutRequested;

	internal Dictionary<Point, Tile> CameraBlockerTiles => _cameraBlockerTiles;

	internal Dictionary<Point, List<Tile>> BackgroundTiles => _backgroundTiles;

	internal Dictionary<Point, List<Tile>> ForegroundTiles => _foregroundTiles;

	public Dictionary<Point, WaterTile> WaterTiles => _waterTiles;

	public Dictionary<int, Protagonist> Heroes => _protagonists;

	internal Dictionary<int, NPCBase> NPCs => _npcs;

	public Dictionary<int, IEnumerable<BackgroundSpecification>> KnownWarpBackgrounds => _knownWarpBackgrounds;

	public bool IsNewDialogueAvailable => _dialogueQueue.Count > 0;

	public int NextObjectTicketID => _objectTicketIDDispenser.GetNext();

	public int NextProjectileTicketID => _projectileTicketIDDispenser.GetNext();

	internal bool IsFamiliarAvailableToPlay
	{
		get
		{
			bool result = false;
			if (MainHero != null && MainHero.IsPrimaryPlayer && GameSave.Inventory.EquippedFamiliar != 0 && MainHero is LunaisObj lunaisObj && lunaisObj.FamiliarManager.IsFamiliarControlledByAI)
			{
				result = true;
			}
			return result;
		}
	}

	public Level(GCM inGCM, LevelSpecification levelSpec, MinimapSpecification minimap, Jukebox inJukebox, Vector2 inScreenCenter, Vector2 inLevelCenter, int inPlayer1Controller, GameSave inSave, GameConfigSave inConfig, LevelChangeRequest levelChangeRequest, Dictionary<int, IEnumerable<BackgroundSpecification>> knownWarpBackgrounds)
	{
		_levelSpecification = levelSpec;
		_gcm = inGCM;
		_minimapSpecification = minimap;
		_jukebox = inJukebox;
		_knownWarpBackgrounds = knownWarpBackgrounds;
		_objectTicketIDDispenser = new TicketIDDispenser();
		_projectileTicketIDDispenser = new TicketIDDispenser();
		_id = levelSpec.ID;
		_era = GetEraByLevelID(_id);
		Player1Controller = inPlayer1Controller;
		GameSave = inSave;
		_isEasyMode = inSave.IsEasyMode;
		_isHardMode = inSave.IsHardMode;
		ConfigSave = inConfig;
		IsPowerOff = GameSave.GetSaveBool("11_LabPower");
		CameraZoom = 1f;
		UpdateScreenCenter(inScreenCenter, inLevelCenter);
		_camera = new Camera2D(LevelRenderCenter, this);
		ResetCameraToInitialPosition();
		LevelDrawColor = Color.White;
		LoadRoom(levelChangeRequest.RoomID);
		LoadProtagonists(levelChangeRequest);
		InitializeMonsters();
		InitializeEvents();
		InitializeNPCs();
		foreach (KeyValuePair<string, bool> levelSaveBool in GameSave.LevelSaveBools)
		{
			if (!_levelSaveBools.Contains(levelSaveBool.Key))
			{
				_levelSaveBools.Add(levelSaveBool.Key);
			}
		}
		GameSave.LevelSaveBools.Clear();
		foreach (KeyValuePair<string, int> levelSaveInt in GameSave.LevelSaveInts)
		{
			_levelSaveInts[levelSaveInt.Key] = levelSaveInt.Value;
		}
		GameSave.LevelSaveInts.Clear();
		_hasAreaTitleBeenShown = levelChangeRequest.IsDebugRequest || GameSave.GetSaveBool($"AreaTitleShown{ID}");
		if (!levelChangeRequest.IsUsingWarp && levelChangeRequest.CutsceneToCall != 0)
		{
			CutsceneBase.CreateAndCallCutscene(levelChangeRequest.CutsceneToCall, this, Point.Zero, isAfterWarp: true);
		}
	}

	public void UpdateScreenCenter(Vector2 inScreenCenter, Vector2 inRenderCenter)
	{
		LevelRenderCenter = inRenderCenter;
		DisplaySize = new Vector2(inScreenCenter.X * 2f, inScreenCenter.Y * 2f);
		MonitorScreenCenter = inScreenCenter;
	}

	public static string GetLevelPathFromID(int levelID, bool isCompressed)
	{
		return "Content/Levels/" + GetLevelFileNameFromID(levelID, isCompressed);
	}

	public static string GetLevelFileNameFromID(int levelID, bool isCompressed)
	{
		string text = (isCompressed ? "dat" : "lvl");
		if (levelID == 17)
		{
			return "Nexus." + text;
		}
		if (levelID < 18)
		{
			return string.Format("Level_{0}{1}.{2}", (levelID < 10) ? "0" : "", levelID, text);
		}
		return "Debug." + text;
	}

	private void LoadRoom(int roomID)
	{
		CurrentRoom = _levelSpecification.GetRoomByID(roomID);
		LastRoomIndex = RoomIndex;
		RoomID = roomID;
		RoomIndex = CurrentRoom.Index;
		_playerStart = Point.Zero;
		CurrentTileset = GCM.GetTileset(CurrentRoom.Tileset);
		int height = CurrentRoom.Height;
		int width = CurrentRoom.Width;
		_levelTileSize = new Point(width, height);
		RoomSize = new Point(width * 16, height * 16);
		_levelCameraBounds = new Rectangle(200, 120, _levelTileSize.X * 16 - 200, _levelTileSize.Y * 16 - 120);
		AllocateSpatialGrids(width, height);
		LoadBackgrounds(roomID);
		foreach (List<TileSpecification> value in CurrentRoom.BottomTiles.Values)
		{
			foreach (TileSpecification item in value)
			{
				PlaceTile(item, shouldInitialize: false);
			}
		}
		foreach (TileSpecification value2 in CurrentRoom.MiddleTiles.Values)
		{
			PlaceTile(value2, shouldInitialize: false);
		}
		foreach (List<TileSpecification> value3 in CurrentRoom.TopTiles.Values)
		{
			foreach (TileSpecification item2 in value3)
			{
				PlaceTile(item2, shouldInitialize: false);
			}
		}
		foreach (List<ObjectTileSpecification> value4 in CurrentRoom.ObjectTiles.Values)
		{
			foreach (ObjectTileSpecification item3 in value4)
			{
				PlaceTile(item3, shouldInitialize: false);
			}
		}
		PlaceLevelBoundaryTiles();
		PlaceWaterTiles();
		foreach (TileSwathSpecification value5 in CurrentRoom.TileSwaths.Values)
		{
			_tileSwaths.Add(new TileSwath(value5, this, CurrentTileset));
		}
		SyncSpatialGrids();
	}

	private void AllocateSpatialGrids(int width, int height)
	{
		int size = width * height;
		if (_solidTileGrid == null || _solidTileGrid.Length != size)
		{
			_solidTileGrid = new Tile[size];
			_waterTileGrid = new WaterTile[size];
			_backgroundTileGrid = new List<Tile>[size];
			_foregroundTileGrid = new List<Tile>[size];
		}
		else
		{
			Array.Clear(_solidTileGrid, 0, size);
			Array.Clear(_waterTileGrid, 0, size);
			Array.Clear(_backgroundTileGrid, 0, size);
			Array.Clear(_foregroundTileGrid, 0, size);
		}
	}

	private void SyncSpatialGrids()
	{
		int width = _levelTileSize.X;
		int height = _levelTileSize.Y;
		int size = width * height;
		if (_solidTileGrid == null || _solidTileGrid.Length != size)
		{
			AllocateSpatialGrids(width, height);
		}
		else
		{
			Array.Clear(_solidTileGrid, 0, size);
			Array.Clear(_waterTileGrid, 0, size);
			Array.Clear(_backgroundTileGrid, 0, size);
			Array.Clear(_foregroundTileGrid, 0, size);
		}

		foreach (KeyValuePair<Point, Tile> kvp in _solidTiles)
		{
			int x = kvp.Key.X;
			int y = kvp.Key.Y;
			if ((uint)x < (uint)width && (uint)y < (uint)height)
			{
				int idx = y * width + x;
				if ((uint)idx < (uint)_solidTileGrid.Length)
				{
					_solidTileGrid[idx] = kvp.Value;
				}
			}
		}
		foreach (KeyValuePair<Point, WaterTile> kvp in _waterTiles)
		{
			int x = kvp.Key.X;
			int y = kvp.Key.Y;
			if ((uint)x < (uint)width && (uint)y < (uint)height)
			{
				int idx = y * width + x;
				if ((uint)idx < (uint)_waterTileGrid.Length)
				{
					_waterTileGrid[idx] = kvp.Value;
				}
			}
		}
		foreach (KeyValuePair<Point, List<Tile>> kvp in _backgroundTiles)
		{
			int x = kvp.Key.X;
			int y = kvp.Key.Y;
			if ((uint)x < (uint)width && (uint)y < (uint)height)
			{
				int idx = y * width + x;
				if ((uint)idx < (uint)_backgroundTileGrid.Length)
				{
					_backgroundTileGrid[idx] = kvp.Value;
				}
			}
		}
		foreach (KeyValuePair<Point, List<Tile>> kvp in _foregroundTiles)
		{
			int x = kvp.Key.X;
			int y = kvp.Key.Y;
			if ((uint)x < (uint)width && (uint)y < (uint)height)
			{
				int idx = y * width + x;
				if ((uint)idx < (uint)_foregroundTileGrid.Length)
				{
					_foregroundTileGrid[idx] = kvp.Value;
				}
			}
		}
	}

	private void LoadBackgrounds(int roomID)
	{
		BackgroundWipeColor = _levelSpecification.GetBackgroundWipeColorForRoom(roomID);
		Vector3 vector = BackgroundWipeColor.ToVector3();
		float x = Vector3.Dot(vector, new Vector3(0.349f, 0.769f, 0.189f));
		float y = Vector3.Dot(vector, new Vector3(0.349f, 0.686f, 0.169f));
		float z = Vector3.Dot(vector, new Vector3(0.272f, 0.534f, 0.131f));
		FullSepiaWipeColor = new Vector3(x, y, z);
		float fullGrayscaleWipeColor = Vector3.Dot(vector, new Vector3(0.3f, 0.59f, 0.11f));
		FullGrayscaleWipeColor = fullGrayscaleWipeColor;
		foreach (BackgroundSpecification item2 in _levelSpecification.GetBackgroundsForRoom(roomID))
		{
			if (item2.TextureType == EBackgroundTextureType.None)
			{
				continue;
			}
			bool flag = true;
			if (item2.Switches.Count > 0)
			{
				foreach (SwitchSpecification @switch in item2.Switches)
				{
					if (!GameSave.CheckSwitch(@switch))
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				Background item = new Background(item2, this);
				if (!item2.IsForeground)
				{
					_backgrounds.Add(item);
				}
				else
				{
					_foregrounds.Add(item);
				}
			}
		}
	}

	public Color GetBackgroundWipeColor()
	{
		Color result = BackgroundWipeColor;
		if (IsTimeFrozen)
		{
			Vector3 vector = BackgroundWipeColor.ToVector3();
			if (IsUsingGrayscaleEffect)
			{
				float num = MathHelper.Lerp(FullGrayscaleWipeColor, 0f, GrayscaleFadePercentage);
				result = new Color(num, num, num);
			}
			else
			{
				float r = MathHelper.Lerp(FullSepiaWipeColor.X, vector.X, _sepiaPercent);
				float g = MathHelper.Lerp(FullSepiaWipeColor.Y, vector.Y, _sepiaPercent);
				float b = MathHelper.Lerp(FullSepiaWipeColor.Z, vector.Z, _sepiaPercent);
				result = new Color(r, g, b);
			}
		}
		return result;
	}

	public void ReloadBackgrounds()
	{
		_backgrounds.Clear();
		_foregrounds.Clear();
		LoadBackgrounds(RoomID);
	}

	public void ChangeRoomByIndex(int index, EDirection whichEntrance)
	{
		if (index >= 0 && index < LevelSpecification.Rooms.Count)
		{
			ChangeRoom(LevelSpecification.Rooms[index].ID, whichEntrance);
		}
	}

	public void ChangeRoom(int whichRoom, EDirection whichEntrance)
	{
		ChangeRoom(new LevelChangeRequest
		{
			LevelID = ID,
			RoomID = whichRoom,
			EnterDirection = whichEntrance
		});
	}

	public void ChangeRoom(LevelChangeRequest request)
	{
		if (request == null)
		{
			return;
		}
		try
		{
			Console.WriteLine($"[Transition ChangeRoom] Room transition started: from Room {RoomID} to Room {request.RoomID}, Entrance: {request.EnterDirection}");
			DisposeAllObjects();
			JukeBox.SetUnOwnedCuesToNonUpdate();
			_objectTicketIDDispenser.Reset();
			_projectileTicketIDDispenser.Reset();
			_areTeleportExitsUnlocked = true;
			List<Protagonist> list = new List<Protagonist>();
			foreach (Protagonist value in _protagonists.Values)
			{
				list.Add(value);
				value.ID = NextObjectTicketID;
			}
			_protagonists.Clear();
			foreach (Protagonist item in list)
			{
				_protagonists.Add(item.ID, item);
			}
			GC.Collect();
			LoadRoom(request.RoomID);
			MainHero.IsBlocked = false;
			IsUIRequestingHide = false;
			IsPlayerInputBlocked = false;
			_isCameraUpdateDisabled = false;
			PlacePlayerAtDoor(request);
			_camera.TargetPoint = MainHero.Position;
			_camera.ChaseRectangleFollowPoint(MainHero.Position);
			_camera.EnforceBoundary(_levelCameraBounds);
			CameraChanged();
			InitializeMonsters();
			InitializeEvents();
			InitializeNPCs();
			_camera.DetectCameraBlockers();
			CameraChanged();
			RefreshBackgroundCamera();
			if (IsTimeFrozen)
			{
				FreezeTime(_teamFreezing, reFreeze: true);
			}
			MainHero.ChangeRoom();
			IsRoomChanged = true;
			RoomEntrance = request.EnterDirection;
			if (request.CutsceneToCall != 0)
			{
				CutsceneBase.CreateAndCallCutscene(request.CutsceneToCall, this, Point.Zero, isAfterWarp: true);
			}
			Console.WriteLine($"[Transition ChangeRoom] Room transition to {request.RoomID} complete. Player at {MainHero?.Position}");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[Transition ChangeRoom ERROR] Failed to change room: {ex}");
			if (MainHero != null)
			{
				MainHero.IsBlocked = false;
				MainHero.StopMovement();
			}
			IsPlayerInputBlocked = false;
			IsUIRequestingHide = false;
			_isCameraUpdateDisabled = false;
			_areTeleportExitsUnlocked = true;
		}
	}

	internal void RefreshBackgroundCamera()
	{
		foreach (Background background in _backgrounds)
		{
			background.RefreshCameraValues();
		}
		foreach (Background foreground in _foregrounds)
		{
			foreground.RefreshCameraValues();
		}
	}

	public void PlacePlayerAtDoor(LevelChangeRequest request)
	{
		if (request == null || MainHero == null)
		{
			return;
		}
		List<TeleportEvent> list = new List<TeleportEvent>();
		foreach (TeleportEvent teleportDoor in _teleportDoors)
		{
			if (teleportDoor.Direction == request.EnterDirection)
			{
				list.Add(teleportDoor);
			}
		}
		int count = list.Count;
		if (count > 0)
		{
			TeleportEvent teleportEvent = list[0];
			if (count > 1)
			{
				List<TeleportEvent> list2 = new List<TeleportEvent>();
				foreach (TeleportEvent item in list)
				{
					Point point = new Point((int)((float)item.Position.X / 400f), (int)((float)item.Position.Y / 320f));
					if (item.Direction == EDirection.South && item.Position.Y % 320 == 0)
					{
						point = point.Add(0, -1);
					}
					if (point == request.TargetBlockKey)
					{
						list2.Add(item);
					}
				}
				if (list2.Count > 0)
				{
					teleportEvent = list2[0];
					Point point2 = new Point(request.TargetBlockKey.X * 400, request.TargetBlockKey.Y * 400);
					if (list2.Count > 1)
					{
						bool flag = request.EnterDirection == EDirection.North || request.EnterDirection == EDirection.South;
						int num = int.MaxValue;
						foreach (TeleportEvent item2 in list2)
						{
							int num2 = Math.Abs(flag ? (item2.Position.X - point2.X - request.EntryPosition.X) : (item2.Position.Y - point2.Y - request.EntryPosition.Y));
							if (num2 < num)
							{
								teleportEvent = item2;
								num = num2;
							}
						}
					}
				}
			}
			MainHero.TeleportToPoint(new Point(teleportEvent.Position.X + request.HeroOffset.X, teleportEvent.Position.Y + request.HeroOffset.Y));
		}
		else if (_playerStart == Point.Zero && _teleportDoors.Count > 0)
		{
			MainHero.TeleportToPoint(_teleportDoors[0].DefaultTeleportPosition);
		}
		else if (_playerStart != Point.Zero)
		{
			MainHero.TeleportToPoint(_playerStart);
		}
		else
		{
			// Safe fallback if room has neither playerStart nor matching door
			Point safePos = new Point(Math.Max(16, RoomSize.X / 2), Math.Max(16, RoomSize.Y / 2));
			MainHero.TeleportToPoint(safePos);
		}
		if (request.EnterDirection == EDirection.West)
		{
			MainHero.IsFacingLeft = false;
		}
		else if (request.EnterDirection == EDirection.East)
		{
			MainHero.IsFacingLeft = true;
		}
		Console.WriteLine($"[Transition PlacePlayerAtDoor] Placed player at {MainHero.Position}, FacingLeft: {MainHero.IsFacingLeft}");
	}

	public void RequestChangeRoom(LevelChangeRequest request)
	{
		_levelChangeRequest = request;
		if (request != null)
		{
			Console.WriteLine($"[Transition RequestChangeRoom] Request to change to Room {request.RoomID}, Level {request.LevelID}, EnterDir: {request.EnterDirection}");
			IsUsingWhiteFadeOut = request.IsUsingWhiteFadeOut;
			if (request.LevelID != ID)
			{
				GameSave.LevelSaveBools.Clear();
				GameSave.LevelSaveInts.Clear();
			}
		}
	}

	public void RequestChangeLevel(int whichLevel)
	{
		RequestChangeLevel(new LevelChangeRequest
		{
			LevelID = whichLevel
		});
	}

	public void RequestChangeLevel(LevelChangeRequest request)
	{
		if (request != null)
		{
			Console.WriteLine($"[Transition RequestChangeLevel] Request to change to Level {request.LevelID}, Room {request.RoomID}, EnterDir: {request.EnterDirection}");
		}
		IsLevelChangeRequested = true;
		LevelChangeRequest = request;
	}

	public void ForceFinalizeTransition()
	{
		Console.WriteLine($"[Transition Watchdog] ForceFinalizeTransition on Level {ID}, Room {RoomID}...");
		if (_levelChangeRequest != null)
		{
			LevelChangeRequest req = _levelChangeRequest;
			_levelChangeRequest = null;
			try
			{
				ChangeRoom(req);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[Transition Watchdog ERROR] ChangeRoom failed during watchdog recovery: {ex}");
			}
		}
		_levelChangeRequest = null;
		IsFadeOutRequested = false;
		IsUsingWhiteFadeOut = false;
		IsPlayerInputBlocked = false;
		IsUIRequestingHide = false;
		_isCameraUpdateDisabled = false;
		_areTeleportExitsUnlocked = true;
		if (MainHero != null)
		{
			MainHero.IsBlocked = false;
			MainHero.StopMovement();
			MainHero.ScriptActionList.Clear();
		}
		_waitingScripts.Clear();
		_activeScripts.Clear();
		_dialogueQueue.Clear();
		if (MainHero != null && _camera != null)
		{
			_camera.TargetPoint = MainHero.Position;
			_camera.ChaseRectangleFollowPoint(MainHero.Position);
			_camera.EnforceBoundary(_levelCameraBounds);
			CameraChanged();
		}
		Console.WriteLine($"[Transition Watchdog] Level recovery complete. Hero pos: {MainHero?.Position}");
	}

	internal void RequestEndGame(bool shouldShowScreen)
	{
		IsEndGameRequested = true;
		IsEndGameRequestingEndScreen = shouldShowScreen;
	}

	internal void RequestRollCredits()
	{
		IsRequestingRollCredits = true;
	}

	internal void ClearAllEnemies()
	{
		foreach (Monster value in _enemies.Values)
		{
			value.SilentKill();
		}
	}

	public void DisposeAllObjects()
	{
		foreach (Monster value in _enemies.Values)
		{
			value.SilentKill();
		}
		foreach (Projectile value2 in _heroProjectiles.Values)
		{
			value2.SilentKill();
		}
		foreach (Projectile value3 in _enemyProjectiles.Values)
		{
			value3.SilentKill();
		}
		foreach (GameEvent value4 in _levelEvents.Values)
		{
			value4.SilentKill();
		}
		foreach (Item value5 in _items.Values)
		{
			value5.SilentKill();
		}
		foreach (Mobile newObject in _newObjects)
		{
			newObject.SilentKill();
		}
		_deadObjects.Clear();
		_newObjects.Clear();
		_solidTiles.Clear();
		_backgroundTiles.Clear();
		_foregroundTiles.Clear();
		_tileSwaths.Clear();
		_waterTiles.Clear();
		_solidTileGrid = null;
		_waterTileGrid = null;
		_backgroundTileGrid = null;
		_foregroundTileGrid = null;
		_waterFillerTiles.Clear();
		_conveyorBelts.Clear();
		_updatableWaterTiles.Clear();
		_cameraBlockerTiles.Clear();
		_enemies.Clear();
		_npcs.Clear();
		_heroProjectiles.Clear();
		_enemyProjectiles.Clear();
		_enemyAnimations.Clear();
		_neutralAnimations.Clear();
		_heroAnimations.Clear();
		_cachedAnimations.Clear();
		_backgrounds.Clear();
		_foregrounds.Clear();
		_checkpoints.Clear();
		_levelEvents.Clear();
		_teleportDoors.Clear();
		_items.Clear();
		HUDNumberQueue.Clear();
		CleanupRoomVariables();
	}

	public void CleanupRoomVariables()
	{
		_roomElevator = null;
		_timeGateEvent = null;
		_transitionWarpEvent = null;
		AreaTitleBlocker = null;
		IsOnVilete = false;
		IsInBossRoom = false;
		_isOverridingPowerOff = false;
		_areaTitleShowTimer = 0f;
		HasPlayerBeenDamagedInThisRoom = false;
		HasPlayerFrozenTimeInThisRoom = false;
		IsMufflingPlayerSFX = false;
	}

	private void PlaceLevelBoundaryTiles()
	{
		Point zero = Point.Zero;
		Point zero2 = Point.Zero;
		zero.Y = 0;
		zero2.Y = _levelTileSize.Y - 1;
		for (int i = 0; i < _levelTileSize.X; i++)
		{
			zero2.X = i;
			zero.X = i;
			if (_solidTiles.ContainsKey(zero))
			{
				Tile tile = _solidTiles[zero];
				if (tile.Type != ETileType.Platform)
				{
					Point key;
					Tile value;
					if (tile.Type != ETileType.Slope)
					{
						key = new Point(zero.X, zero.Y - 1);
						Tile tile2 = new Tile(new Point(key.X * 16, key.Y * 16), this, CurrentTileset, tile.TileIndex, ETileType.Solid);
						tile2.IsFlippedVertically = !tile.IsFlippedVertically;
						tile2.IsFlippedHorizontally = tile.IsFlippedHorizontally;
						value = tile2;
					}
					else
					{
						bool flag = Tile.IsSlopeFacingLeft(tile.Slope);
						key = new Point(zero.X + (flag ? 1 : (-1)), zero.Y - 1);
						Tile tile3 = new Tile(new Point(key.X * 16, key.Y * 16), this, CurrentTileset, tile.TileIndex, ETileType.Slope, tile.Slope);
						tile3.IsFlippedHorizontally = tile.IsFlippedHorizontally;
						tile3.IsFlippedVertically = tile.IsFlippedVertically;
						value = tile3;
					}
					_solidTiles[key] = value;
				}
			}
			else if (_foregroundTiles.ContainsKey(zero))
			{
				Point key2 = new Point(zero.X, zero.Y - 1);
				_solidTiles.Add(key2, new Tile(new Point(key2.X * 16, key2.Y * 16), this, CurrentTileset, 96, ETileType.Solid));
			}
			if (_backgroundTiles.ContainsKey(zero) && _backgroundTiles[zero].Count > 0)
			{
				Tile tile4 = _backgroundTiles[zero][0];
				Point inPosition = new Point(zero.X * 16, (zero.Y - 1) * 16);
				Tile tile5 = new Tile(inPosition, this, CurrentTileset, tile4.TileIndex, ETileType.Passable);
				tile5.IsFlippedHorizontally = tile4.IsFlippedHorizontally;
				tile5.IsFlippedVertically = !tile4.IsFlippedVertically;
				Tile tile6 = tile5;
				PlaceStackableTile(_backgroundTiles, tile6);
			}
			if (_solidTiles.ContainsKey(zero2))
			{
				Tile tile7 = _solidTiles[zero2];
				if (tile7.Type != ETileType.Platform)
				{
					if (tile7.Type != ETileType.Slope)
					{
						Point key3 = new Point(zero2.X, zero2.Y + 1);
						Tile tile8 = new Tile(new Point(key3.X * 16, key3.Y * 16), this, CurrentTileset, tile7.TileIndex, ETileType.Passable);
						tile8.IsFlippedHorizontally = tile7.IsFlippedHorizontally;
						tile8.IsFlippedVertically = !tile7.IsFlippedVertically;
						Tile item = tile8;
						if (_foregroundTiles.ContainsKey(key3))
						{
							_foregroundTiles[key3].Add(item);
						}
						else
						{
							_foregroundTiles[key3] = new List<Tile> { item };
						}
					}
					else
					{
						bool flag2 = Tile.IsSlopeFacingLeft(tile7.Slope);
						Point key4 = new Point(zero.X + ((!flag2) ? 1 : (-1)), zero2.Y + 1);
						if (!_solidTiles.ContainsKey(key4))
						{
							Tile tile9 = new Tile(new Point(key4.X * 16, key4.Y * 16), this, CurrentTileset, tile7.TileIndex, ETileType.Slope, tile7.Slope);
							tile9.IsFlippedHorizontally = tile7.IsFlippedHorizontally;
							tile9.IsFlippedVertically = tile7.IsFlippedVertically;
							Tile value2 = tile9;
							_solidTiles.Add(key4, value2);
						}
					}
				}
			}
			if (_backgroundTiles.ContainsKey(zero2) && _backgroundTiles[zero2].Count > 0)
			{
				Tile tile10 = _backgroundTiles[zero2][0];
				Point inPosition2 = new Point(zero2.X * 16, (zero2.Y + 1) * 16);
				Tile tile11 = new Tile(inPosition2, this, CurrentTileset, tile10.TileIndex, ETileType.Passable);
				tile11.IsFlippedHorizontally = tile10.IsFlippedHorizontally;
				tile11.IsFlippedVertically = !tile10.IsFlippedVertically;
				Tile tile12 = tile11;
				PlaceStackableTile(_backgroundTiles, tile12);
			}
		}
		AddSideBoundaryTiles(isLeftSide: false);
		AddSideBoundaryTiles(isLeftSide: true);
		AddCornerTiles();
	}

	private void AddSideBoundaryTiles(bool isLeftSide)
	{
		int num = ((!isLeftSide) ? (_levelTileSize.X - 1) : 0);
		for (int i = 0; i < _levelTileSize.Y; i++)
		{
			Point key = new Point(num, i);
			Tile tile = null;
			bool flag = true;
			if (_solidTiles.ContainsKey(key))
			{
				tile = _solidTiles[key];
			}
			else if (_backgroundTiles.ContainsKey(key) && _backgroundTiles[key].Count > 0)
			{
				tile = _backgroundTiles[key][0];
				flag = false;
			}
			if (tile == null)
			{
				continue;
			}
			Point key2 = new Point(num + ((!isLeftSide) ? 1 : (-1)), i);
			Point inPosition = new Point(key2.X * 16, key2.Y * 16);
			Tile tile2 = new Tile(inPosition, this, CurrentTileset, tile.TileIndex, flag ? ETileType.Solid : ETileType.Passable);
			tile2.IsFlippedHorizontally = !tile.IsFlippedHorizontally;
			tile2.IsFlippedVertically = tile.IsFlippedVertically;
			Tile tile3 = tile2;
			if (flag)
			{
				if (!_solidTiles.ContainsKey(key2))
				{
					_solidTiles[key2] = tile3;
				}
			}
			else
			{
				PlaceStackableTile(_backgroundTiles, tile3);
			}
		}
	}

	private void AddCornerTiles()
	{
		AddCornerTile(new Point(-1, 0), new Point(-1, -1));
		AddCornerTile(new Point(-1, RoomSize16.Y - 1), new Point(-1, RoomSize16.Y));
		int x = RoomSize16.X;
		AddCornerTile(new Point(x, 0), new Point(x, -1));
		AddCornerTile(new Point(x, RoomSize16.Y - 1), new Point(x, RoomSize16.Y));
	}

	private void AddCornerTile(Point start, Point destination)
	{
		if (_solidTiles.ContainsKey(start) && !_solidTiles.ContainsKey(destination))
		{
			Tile tile = _solidTiles[start];
			Tile tile2 = new Tile(new Point(destination.X * 16, destination.Y * 16), this, CurrentTileset, tile.TileIndex, ETileType.Slope, tile.Slope);
			tile2.IsFlippedHorizontally = tile.IsFlippedHorizontally;
			tile2.IsFlippedVertically = !tile.IsFlippedVertically;
			Tile value = tile2;
			_solidTiles[destination] = value;
			int width = _levelTileSize.X;
			if ((uint)destination.X < (uint)width && (uint)destination.Y < (uint)_levelTileSize.Y && _solidTileGrid != null)
			{
				int idx = destination.Y * width + destination.X;
				if ((uint)idx < (uint)_solidTileGrid.Length)
				{
					_solidTileGrid[idx] = value;
				}
			}
		}
	}

	private void PlaceWaterTiles()
	{
		foreach (WaterFillerEvent waterFillerTile in _waterFillerTiles)
		{
			if (waterFillerTile.CurrentEWaterType == WaterFillerEvent.EWaterType.FillerTopLeft)
			{
				waterFillerTile.FillRoomWithWater(_waterTiles, _waterFillerTiles, _updatableWaterTiles);
			}
		}
	}

	public void DeleteTile(TileSpecification targetTile)
	{
		Point key = new Point(targetTile.X, targetTile.Y);
		int x = key.X;
		int y = key.Y;
		int width = _levelTileSize.X;
		bool inBounds = (uint)x < (uint)width && (uint)y < (uint)_levelTileSize.Y;
		int idx = inBounds ? (y * width + x) : -1;
		switch (targetTile.Layer)
		{
		case ETileLayerType.Bottom:
			if (_backgroundTiles != null && _backgroundTiles.ContainsKey(key))
			{
				_backgroundTiles[key].Clear();
				_backgroundTiles.Remove(key);
				if (inBounds && _backgroundTileGrid != null && (uint)idx < (uint)_backgroundTileGrid.Length)
				{
					_backgroundTileGrid[idx] = null;
				}
			}
			break;
		case ETileLayerType.Middle:
			if (_solidTiles != null && _solidTiles.ContainsKey(key))
			{
				_solidTiles.Remove(key);
				if (inBounds && _solidTileGrid != null && (uint)idx < (uint)_solidTileGrid.Length)
				{
					_solidTileGrid[idx] = null;
				}
			}
			break;
		case ETileLayerType.Top:
			if (_foregroundTiles != null && _foregroundTiles.ContainsKey(key))
			{
				_foregroundTiles[key].Clear();
				_foregroundTiles.Remove(key);
				if (inBounds && _foregroundTileGrid != null && (uint)idx < (uint)_foregroundTileGrid.Length)
				{
					_foregroundTileGrid[idx] = null;
				}
			}
			break;
		}
	}

	public void ReplaceTile(TileSpecification newTile)
	{
		DeleteTile(newTile);
		PlaceTile(newTile, shouldInitialize: true);
	}

	public void PlaceTile(TileSpecification tile, bool shouldInitialize)
	{
		if (tile.ID < 0)
		{
			return;
		}
		bool flag = true;
		if (tile.Switches.Count > 0)
		{
			foreach (SwitchSpecification @switch in tile.Switches)
			{
				if (!GameSave.CheckSwitch(@switch))
				{
					flag = false;
					break;
				}
			}
		}
		if (!flag)
		{
			return;
		}
		if (tile.Layer != ETileLayerType.Objects)
		{
			Tile tile2 = Tile.FromSpecification(tile, this);
			switch (tile.Layer)
			{
			case ETileLayerType.Bottom:
				PlaceStackableTile(_backgroundTiles, tile2);
				break;
			case ETileLayerType.Middle:
				_solidTiles.Add(tile2.DictKey, tile2);
				int x = tile2.DictKey.X;
				int y = tile2.DictKey.Y;
				int width = _levelTileSize.X;
				if ((uint)x < (uint)width && (uint)y < (uint)_levelTileSize.Y && _solidTileGrid != null)
				{
					int idx = y * width + x;
					if ((uint)idx < (uint)_solidTileGrid.Length)
					{
						_solidTileGrid[idx] = tile2;
					}
				}
				if (tile2.Special == ETileSpecialType.CamBlock)
				{
					_cameraBlockerTiles.Add(tile2.DictKey, tile2);
				}
				break;
			case ETileLayerType.Top:
				PlaceStackableTile(_foregroundTiles, tile2);
				break;
			}
		}
		else if (tile is ObjectTileSpecification objectTileSpec)
		{
			PlaceEvent(objectTileSpec, shouldInitialize);
		}
	}

	public Animate PlaceEvent(ObjectTileSpecification objectTileSpec, bool shouldInitialize)
	{
		Animate result = null;
		if (objectTileSpec != null && objectTileSpec.Category == EObjectTileCategory.Enemy)
		{
			int nextObjectTicketID = NextObjectTicketID;
			Point point = new Point(objectTileSpec.X * 16 + 8, objectTileSpec.Y * 16 + 16);
			Monster monster = null;
			switch (objectTileSpec.GetEnemyType())
			{
			case EEnemyTileType.CheveuxTank:
				monster = new CheveuxTank(point, this, GCM.SpCheveuxTank, nextObjectTicketID, objectTileSpec);
				break;
			case EEnemyTileType.RedCheveux:
				monster = new RedCheveux(point, this, GCM.SpCheveuxTank, nextObjectTicketID, objectTileSpec);
				break;
			case EEnemyTileType.FlyingCheveux:
				monster = new FlyingCheveux(point, this, GCM.SpCheveuxFlying, nextObjectTicketID, objectTileSpec);
				break;
			case EEnemyTileType.KickstarterFoe:
				switch (objectTileSpec.Argument)
				{
				case 0:
					monster = new GyreMajorUgly(point, this, GCM.SpGyreMajorUgly, nextObjectTicketID, objectTileSpec);
					break;
				case 1:
					monster = new GyreMeteorSparrow(point, this, GCM.SpGyreMeteorSparrow, nextObjectTicketID, objectTileSpec);
					break;
				case 2:
					monster = new GyreKain(point, this, GCM.SpGyreKain, nextObjectTicketID, objectTileSpec);
					break;
				case 3:
					monster = new GyreNethershade(point, this, GCM.SpGyreNethershade, nextObjectTicketID, objectTileSpec);
					break;
				case 4:
					monster = new GyreRyshia(point, this, GCM.SpGyreRyshia, nextObjectTicketID, objectTileSpec);
					break;
				case 5:
					monster = new GyreZel(point, this, GCM.SpGyreZel, nextObjectTicketID, objectTileSpec);
					break;
				}
				break;
			case EEnemyTileType.JunkSpawner:
			{
				JunkSpawnerEvent junkSpawnerEvent = new JunkSpawnerEvent(this, point, nextObjectTicketID, objectTileSpec);
				result = junkSpawnerEvent;
				_levelEvents.Add(nextObjectTicketID, junkSpawnerEvent);
				break;
			}
			case EEnemyTileType.CavesCopperWyvern:
				monster = ((objectTileSpec.DoesHaveArgument && objectTileSpec.Argument != 0) ? ((Monster)new CursedCopperWyvern(point, this, GCM.SpCursedCopperWyvern, nextObjectTicketID, objectTileSpec)) : ((Monster)new CavesCopperWyvern(point, this, GCM.SpCopperWyvern, nextObjectTicketID, objectTileSpec)));
				break;
			case EEnemyTileType.CavesSiren:
				monster = ((objectTileSpec.DoesHaveArgument && objectTileSpec.Argument != 0) ? ((Monster)new CursedSiren(point, this, GCM.SpCursedSiren, nextObjectTicketID, objectTileSpec)) : ((Monster)new CavesSiren(point, this, GCM.SpSiren, nextObjectTicketID, objectTileSpec)));
				break;
			case EEnemyTileType.CavesSlime:
				monster = ((objectTileSpec.DoesHaveArgument && objectTileSpec.Argument != 0) ? ((Monster)new CursedSlime(point, this, GCM.SpCavesSlime, nextObjectTicketID, objectTileSpec)) : ((Monster)new CavesSlime(point, this, GCM.SpCavesSlime, nextObjectTicketID, objectTileSpec)));
				break;
			case EEnemyTileType.KeepDemon:
				monster = ((objectTileSpec.DoesHaveArgument && objectTileSpec.Argument != 0) ? ((Monster)new EmpDemon(point, this, GCM.SpEmpDemon, nextObjectTicketID, objectTileSpec)) : ((Monster)new KeepDemon(point, this, GCM.SpKeepDemon, nextObjectTicketID, objectTileSpec)));
				break;
			case EEnemyTileType.WormFlower:
				monster = new WormFlower(new Point(point.X, point.Y), this, GCM.SpWormFlower, nextObjectTicketID, objectTileSpec);
				break;
			case EEnemyTileType.WormFlowerWalker:
				monster = new WormFlowerWalker(new Point(point.X, point.Y), this, GCM.SpWormFlower, nextObjectTicketID, objectTileSpec);
				break;
			case EEnemyTileType.CeilingStar:
				monster = new CeilingStar(point, this, GCM.SpCeilingStar, nextObjectTicketID, objectTileSpec);
				break;
			case EEnemyTileType.FleshSpider:
				monster = ((objectTileSpec.DoesHaveArgument && objectTileSpec.Argument != 0) ? ((Monster)new LabSpider(point, this, GCM.SpLabSpider, nextObjectTicketID, objectTileSpec)) : ((Monster)new FleshSpider(point, this, GCM.SpFleshSpider, nextObjectTicketID, objectTileSpec)));
				break;
			case EEnemyTileType.DiscStatue:
				monster = new DiscStatue(point, this, GCM.SpDiscStatue, nextObjectTicketID, objectTileSpec);
				break;
			case EEnemyTileType.CitySecurityGuard:
				monster = new CitySecurityGuard(point, this, GCM.SpCitySecurityGuard, nextObjectTicketID, objectTileSpec);
				break;
			case EEnemyTileType.ForestBabyCheveux:
				monster = new ForestBabyCheveux(point, this, GCM.SpForestBabyCheveux, nextObjectTicketID, objectTileSpec);
				break;
			case EEnemyTileType.ForestMoth:
				monster = ((objectTileSpec.DoesHaveArgument && objectTileSpec.Argument != 0) ? ((Monster)new CursedMoth(point, this, GCM.SpCursedMoth, nextObjectTicketID, objectTileSpec)) : ((Monster)new ForestMoth(point, this, GCM.SpForestMoth, nextObjectTicketID, objectTileSpec)));
				break;
			case EEnemyTileType.ForestPlantBat:
				monster = new ForestPlantBat(point, this, GCM.SpForestPlantBat, nextObjectTicketID, objectTileSpec);
				break;
			case EEnemyTileType.ForestRodent:
				monster = new ForestRodent(point, this, GCM.SpForestRodent, nextObjectTicketID, objectTileSpec);
				break;
			case EEnemyTileType.ForestWormFlower:
				monster = new ForestWormFlower(point, this, GCM.SpForestWormFlower, nextObjectTicketID, objectTileSpec);
				break;
			case EEnemyTileType.CavesMushroomTower:
				monster = ((objectTileSpec.DoesHaveArgument && objectTileSpec.Argument != 0) ? ((Monster)new CursedMushroomTower(point, this, GCM.SpCursedMushroomTower, nextObjectTicketID, objectTileSpec)) : ((Monster)new CavesMushroomTower(point, this, GCM.SpCavesMushroomTower, nextObjectTicketID, objectTileSpec)));
				break;
			case EEnemyTileType.CavesSporeVine:
				point.Y -= 8;
				monster = ((objectTileSpec.DoesHaveArgument && objectTileSpec.Argument != 0) ? ((Monster)new CursedSporeVine(point, this, GCM.SpCursedSporeVine, nextObjectTicketID, objectTileSpec)) : ((Monster)new CavesSporeVine(point, this, GCM.SpCavesSporeVine, nextObjectTicketID, objectTileSpec)));
				break;
			case EEnemyTileType.CavesSnail:
				monster = ((objectTileSpec.DoesHaveArgument && objectTileSpec.Argument != 0) ? ((Monster)new CursedSnail(point, this, GCM.SpCursedSnail, nextObjectTicketID, objectTileSpec)) : ((Monster)new CavesSnail(point, this, GCM.SpCavesSnail, nextObjectTicketID, objectTileSpec)));
				break;
			case EEnemyTileType.CursedAnemone:
				monster = new CursedAnemone(point, this, GCM.SpCursedAnemone, nextObjectTicketID, objectTileSpec);
				break;
			case EEnemyTileType.CastleShieldKnight:
				monster = ((objectTileSpec.DoesHaveArgument && objectTileSpec.Argument != 0) ? ((Monster)new ViletianLancer(point, this, GCM.SpCastleShieldKnight, nextObjectTicketID, objectTileSpec)) : ((Monster)new CastleShieldKnight(point, this, GCM.SpCastleShieldKnight, nextObjectTicketID, objectTileSpec)));
				break;
			case EEnemyTileType.CastleArcher:
				monster = new CastleArcher(point, this, GCM.SpCastleArcher, nextObjectTicketID, objectTileSpec);
				break;
			case EEnemyTileType.CastleLargeSoldier:
				monster = new CastleLargeSoldier(point, this, GCM.SpCastleLargeSoldier, nextObjectTicketID, objectTileSpec);
				break;
			case EEnemyTileType.CastleEngineer:
				monster = new CastleEngineer(point, this, GCM.SpCastleEngineer, nextObjectTicketID, objectTileSpec);
				break;
			case EEnemyTileType.KeepWarCheveux:
				monster = new KeepWarCheveux(point, this, GCM.SpKeepWarCheveux, nextObjectTicketID, objectTileSpec);
				break;
			case EEnemyTileType.KeepAristocrat:
				if (!objectTileSpec.DoesHaveArgument || objectTileSpec.Argument == 0)
				{
					monster = new KeepAristocrat(point, this, GCM.SpKeepAristocrat, nextObjectTicketID, objectTileSpec);
					break;
				}
				switch (objectTileSpec.Argument)
				{
				case 1:
					monster = new TowerIceMage(point, this, GCM.SpTowerIceMage, nextObjectTicketID, objectTileSpec);
					break;
				case 2:
					monster = new EmpAristocrat(point, this, GCM.SpEmpAristocrat, nextObjectTicketID, objectTileSpec);
					break;
				}
				break;
			case EEnemyTileType.TowerPlasmaPod:
				monster = new TowerPlasmaPod(point, this, GCM.SpTowerPlasmaPod, nextObjectTicketID, objectTileSpec);
				break;
			case EEnemyTileType.TowerRoyalGuard:
				monster = ((objectTileSpec.DoesHaveArgument && objectTileSpec.Argument != 0) ? ((Monster)new EmpRoyalGuard(point, this, GCM.SpEmpRoyalGuard, nextObjectTicketID, objectTileSpec)) : ((Monster)new TowerRoyalGuard(point, this, GCM.SpTowerDemonMage, nextObjectTicketID, objectTileSpec)));
				break;
			case EEnemyTileType.LakeAnemone:
				monster = new LakeAnemone(point, this, GCM.SpLakeAnemone, nextObjectTicketID, objectTileSpec);
				break;
			case EEnemyTileType.LakeBirdEgg:
				monster = new LakeBirdEgg(point, this, GCM.SpLakeBirdEgg, nextObjectTicketID, objectTileSpec, doesInstantlyAggro: false);
				break;
			case EEnemyTileType.LakeCheveux:
				monster = new LakeCheveux(point, this, GCM.SpLakeCheveux, nextObjectTicketID, objectTileSpec);
				break;
			case EEnemyTileType.LakeEel:
				monster = new LakeEel(point, this, GCM.SpLakeEel, nextObjectTicketID, objectTileSpec);
				break;
			case EEnemyTileType.LakeFly:
				monster = new LakeFly(point, this, GCM.SpLakeFly, nextObjectTicketID, objectTileSpec);
				break;
			case EEnemyTileType.FortressKnight:
				monster = new FortressKnight(point, this, GCM.SpFortressKnight, nextObjectTicketID, objectTileSpec);
				break;
			case EEnemyTileType.FortressGunner:
				monster = new FortressGunner(point, this, GCM.SpFortressGunner, nextObjectTicketID, objectTileSpec);
				break;
			case EEnemyTileType.LabTurret:
				monster = new LabTurret(point, this, GCM.SpLabTurret, nextObjectTicketID, objectTileSpec);
				break;
			case EEnemyTileType.LabChild:
				monster = new LabChild(point, this, GCM.SpLabChild, nextObjectTicketID, objectTileSpec);
				break;
			case EEnemyTileType.LabAdult:
				monster = new LabAdult(point, this, GCM.SpLabAdult, nextObjectTicketID, objectTileSpec);
				break;
			case EEnemyTileType.FortressLargeSoldier:
				monster = new FortressLargeSoldier(point, this, GCM.SpFortressLargeSoldier, nextObjectTicketID, objectTileSpec);
				break;
			case EEnemyTileType.FortressEngineer:
				monster = new FortressEngineer(point, this, GCM.SpFortressEngineer, nextObjectTicketID, objectTileSpec);
				break;
			case EEnemyTileType.TempleFoe:
				monster = objectTileSpec.Argument switch
				{
					1 => new TempleZeal(point, this, GCM.SpTempleConviction, nextObjectTicketID, objectTileSpec), 
					2 => new TempleJustice(point, this, GCM.SpTempleConviction, nextObjectTicketID, objectTileSpec), 
					3 => new TemplePride(point, this, GCM.SpTempleConviction, nextObjectTicketID, objectTileSpec), 
					_ => new TempleConviction(point, this, GCM.SpTempleConviction, nextObjectTicketID, objectTileSpec), 
				};
				break;
			case EEnemyTileType.BirdBoss:
			case EEnemyTileType.RoboKittyBoss:
			case EEnemyTileType.VarndagrothBoss:
			case EEnemyTileType.AelanaBoss:
			case EEnemyTileType.IncubusBoss:
			case EEnemyTileType.MawBoss:
			case EEnemyTileType.ShapeshiftBoss:
			case EEnemyTileType.EmperorBoss:
			case EEnemyTileType.SandmanBoss:
			case EEnemyTileType.NightmareBoss:
			case EEnemyTileType.RavenBoss:
			case EEnemyTileType.XarionBoss:
			case EEnemyTileType.ZelBoss:
			case EEnemyTileType.CantoranBoss:
				monster = BossClass.CreateFromTileType(this, point, _gcm, nextObjectTicketID, objectTileSpec);
				break;
			}
			if (monster != null)
			{
				result = monster;
				_enemies.Add(nextObjectTicketID, monster);
				if (shouldInitialize)
				{
					monster.InitializeMob();
				}
			}
		}
		else if (objectTileSpec != null && objectTileSpec.Category == EObjectTileCategory.Event)
		{
			Point point = new Point(objectTileSpec.X * 16 + 8, objectTileSpec.Y * 16 + 16);
			int nextObjectTicketID = NextObjectTicketID;
			GameEvent gameEvent = null;
			switch (objectTileSpec.GetEventType())
			{
			case EEventTileType.Checkpoint:
			{
				int checkpointID = _checkpoints.Count + 1;
				SaveStatue saveStatue = new SaveStatue(this, GCM.SpSaveStatue, point, checkpointID, inFound: false, nextObjectTicketID, objectTileSpec);
				result = saveStatue;
				_levelEvents.Add(nextObjectTicketID, saveStatue);
				_checkpoints.Add(saveStatue);
				break;
			}
			case EEventTileType.PlayerStart:
				_playerStart = point;
				break;
			case EEventTileType.WestTeleport:
			{
				TeleportEvent teleportEvent = new TeleportEvent(this, point, EDirection.West, nextObjectTicketID, _teleportDoors.Count, objectTileSpec);
				_levelEvents.Add(nextObjectTicketID, teleportEvent);
				_teleportDoors.Add(teleportEvent);
				break;
			}
			case EEventTileType.NorthTeleport:
			{
				TeleportEvent teleportEvent = new TeleportEvent(this, point, EDirection.North, nextObjectTicketID, _teleportDoors.Count, objectTileSpec);
				_levelEvents.Add(nextObjectTicketID, teleportEvent);
				_teleportDoors.Add(teleportEvent);
				break;
			}
			case EEventTileType.EastTeleport:
			{
				TeleportEvent teleportEvent = new TeleportEvent(this, point, EDirection.East, nextObjectTicketID, _teleportDoors.Count, objectTileSpec);
				_levelEvents.Add(nextObjectTicketID, teleportEvent);
				_teleportDoors.Add(teleportEvent);
				break;
			}
			case EEventTileType.SouthTeleport:
			{
				TeleportEvent teleportEvent = new TeleportEvent(this, point, EDirection.South, nextObjectTicketID, _teleportDoors.Count, objectTileSpec);
				_levelEvents.Add(nextObjectTicketID, teleportEvent);
				_teleportDoors.Add(teleportEvent);
				break;
			}
			case EEventTileType.BossDoor:
				_levelEvents.Add(nextObjectTicketID, new BossDoorEvent(this, point, nextObjectTicketID, objectTileSpec));
				break;
			case EEventTileType.DummyUIEvent:
				gameEvent = new DummyUIEvent(this, point, nextObjectTicketID, objectTileSpec);
				break;
			case EEventTileType.JournalEntry:
				gameEvent = BaseJournalEntryEvent.FromSpecification(this, point, nextObjectTicketID, objectTileSpec);
				break;
			case EEventTileType.MovingPlatform:
				_levelEvents.Add(nextObjectTicketID, new MovingPlatformEvent(this, point, nextObjectTicketID, objectTileSpec));
				break;
			case EEventTileType.BlastDoor:
				_levelEvents.Add(nextObjectTicketID, new BlastDoorEvent(this, point, nextObjectTicketID, objectTileSpec));
				break;
			case EEventTileType.CirclePlatform:
				gameEvent = new CirclePlatformEvent(this, point, nextObjectTicketID, objectTileSpec);
				break;
			case EEventTileType.MiniBossDoor:
				gameEvent = new MiniBossDoorEvent(this, point, nextObjectTicketID, objectTileSpec);
				break;
			case EEventTileType.TransitionWarpEvent:
				_transitionWarpEvent = new TransitionWarpEvent(this, point, nextObjectTicketID, objectTileSpec);
				gameEvent = _transitionWarpEvent;
				break;
			case EEventTileType.ConveyorBelt:
			{
				int key = ((!objectTileSpec.IsFlippedHorizontally) ? 1 : (-1)) * point.Y;
				if (_conveyorBelts.ContainsKey(key))
				{
					_conveyorBelts[key].AddUnit(point);
					break;
				}
				ConveyorBeltFloorEvent value = new ConveyorBeltFloorEvent(this, point, nextObjectTicketID, objectTileSpec);
				_conveyorBelts[key] = value;
				_levelEvents.Add(nextObjectTicketID, value);
				break;
			}
			case EEventTileType.PetrifiedVine:
				_levelEvents.Add(nextObjectTicketID, new PetrifiedVineEvent(this, point, nextObjectTicketID, objectTileSpec));
				break;
			case EEventTileType.WaterFillerNW:
				_waterFillerTiles.Add(new WaterFillerEvent(this, point, new Point(objectTileSpec.X, objectTileSpec.Y), nextObjectTicketID, isTopLeft: true, objectTileSpec));
				break;
			case EEventTileType.WaterFillerSE:
				_waterFillerTiles.Add(new WaterFillerEvent(this, point, new Point(objectTileSpec.X, objectTileSpec.Y), nextObjectTicketID, isTopLeft: false, objectTileSpec));
				break;
			case EEventTileType.Elevator:
				if (_roomElevator == null)
				{
					_roomElevator = new ElevatorEvent(this, point, nextObjectTicketID, objectTileSpec);
					gameEvent = _roomElevator;
				}
				else
				{
					_roomElevator.AddNode(point);
				}
				break;
			case EEventTileType.DonutTile:
				_levelEvents.Add(nextObjectTicketID, new DonutPlatformEvent(this, point, nextObjectTicketID, objectTileSpec));
				break;
			case EEventTileType.BreakableWall:
				gameEvent = new BreakableWallEvent(this, point, nextObjectTicketID, objectTileSpec);
				break;
			case EEventTileType.JunkCrusher:
				_levelEvents.Add(nextObjectTicketID, new JunkCrusherEvent(this, point, nextObjectTicketID, objectTileSpec));
				break;
			case EEventTileType.MerchantCrow:
			{
				MerchantCrowNPC merchantCrowNPC = new MerchantCrowNPC(this, point, nextObjectTicketID, objectTileSpec);
				result = merchantCrowNPC;
				_npcs.Add(nextObjectTicketID, merchantCrowNPC);
				break;
			}
			case EEventTileType.Selen:
			{
				SelenNPC selenNPC = new SelenNPC(this, point, nextObjectTicketID);
				selenNPC.IsFacingLeft = !objectTileSpec.IsFlippedHorizontally;
				SelenNPC selenNPC2 = selenNPC;
				result = selenNPC2;
				_npcs.Add(nextObjectTicketID, selenNPC2);
				break;
			}
			case EEventTileType.TheTimespinner:
			{
				TheTimespinner theTimespinner = new TheTimespinner(this, point, nextObjectTicketID, objectTileSpec);
				result = theTimespinner;
				_levelEvents.Add(nextObjectTicketID, theTimespinner);
				break;
			}
			case EEventTileType.OrbPedestal:
				gameEvent = new OrbPedestalEvent(this, point, nextObjectTicketID, objectTileSpec);
				break;
			case EEventTileType.TimeGate:
				_timeGateEvent = new TimeGateEvent(this, point, nextObjectTicketID, objectTileSpec);
				gameEvent = _timeGateEvent;
				break;
			case EEventTileType.TreasureChest:
				gameEvent = new TreasureChestEvent(this, point, nextObjectTicketID, objectTileSpec);
				break;
			case EEventTileType.Doorway:
			{
				DoorwayEvent doorwayEvent = new DoorwayEvent(this, point, nextObjectTicketID, objectTileSpec, _teleportDoors.Count);
				result = doorwayEvent;
				AddEvent(doorwayEvent);
				_teleportDoors.Add(doorwayEvent);
				break;
			}
			case EEventTileType.KeycardDoor:
				gameEvent = new KeycardDoorEvent(this, point, nextObjectTicketID, objectTileSpec);
				break;
			case EEventTileType.Transition:
				gameEvent = new TransitionDoorEvent(this, point, nextObjectTicketID, objectTileSpec);
				break;
			case EEventTileType.Lantern:
				gameEvent = BaseLantern.FromArgumentAndLevel(this, point, nextObjectTicketID, objectTileSpec);
				break;
			case EEventTileType.ForestNPCs:
			{
				NPCBase nPCBase = NPCBase.FromArgumentAndLevel(this, point, nextObjectTicketID, objectTileSpec);
				if (nPCBase != null)
				{
					_npcs.Add(nextObjectTicketID, nPCBase);
					result = nPCBase;
					if (shouldInitialize)
					{
						nPCBase.Initialize();
					}
				}
				break;
			}
			case EEventTileType.CurtainDrawbridge:
				gameEvent = new CurtainDrawbridge(this, point, nextObjectTicketID, objectTileSpec);
				break;
			case EEventTileType.TimespinnerWheelItem:
				switch (objectTileSpec.Argument)
				{
				default:
					gameEvent = new TimespinnerWheelItem(this, point, nextObjectTicketID, objectTileSpec);
					break;
				case 1:
				case 2:
				case 3:
				{
					int gearIndex = objectTileSpec.Argument - 1;
					gameEvent = new TimespinnerGearItem(this, point, nextObjectTicketID, objectTileSpec, GCM.SpTheTimespinner, gearIndex);
					break;
				}
				case 4:
					gameEvent = new PyramidKeys(this, point, nextObjectTicketID, objectTileSpec, GCM.SpTimeGateAnimation);
					break;
				case 5:
					gameEvent = new TalariaAttachment(this, point, nextObjectTicketID, objectTileSpec, GCM.SpMenuIcons);
					break;
				case 6:
					gameEvent = new RelicKeycardC(this, point, nextObjectTicketID, objectTileSpec, GCM.SpMenuIcons);
					break;
				}
				break;
			case EEventTileType.LevelEffect:
				gameEvent = LevelEffect.CreateFromLevel(this, point, nextObjectTicketID, objectTileSpec);
				break;
			case EEventTileType.EnvironmentPrefab:
				gameEvent = EnvironmentPrefabBase.Create(this, point, nextObjectTicketID, objectTileSpec);
				break;
			case EEventTileType.AreaTitleBlocker:
				AreaTitleBlocker = new AreaTitleBlockerEvent(this, point, nextObjectTicketID, objectTileSpec);
				gameEvent = AreaTitleBlocker;
				break;
			case EEventTileType.MapTerminal:
				gameEvent = new MapComputerEvent(this, point, nextObjectTicketID, objectTileSpec);
				break;
			case EEventTileType.BackerPortrait:
				gameEvent = ((objectTileSpec.Argument >= 4) ? ((objectTileSpec.Argument >= 13) ? ((GameEvent)new BackerPaintingEvent(this, point, nextObjectTicketID, objectTileSpec)) : ((GameEvent)new BackerBustEvent(this, point, nextObjectTicketID, objectTileSpec))) : new BackerPortraitEvent(this, point, nextObjectTicketID, objectTileSpec));
				break;
			case EEventTileType.Cutscene:
				gameEvent = CutsceneBase.FromSpecification(this, point, nextObjectTicketID, objectTileSpec);
				break;
			case EEventTileType.MusicFader:
				gameEvent = new MusicFaderEvent(this, point, nextObjectTicketID, objectTileSpec);
				break;
			case EEventTileType.MusicPlayer:
				gameEvent = new MusicPlayerEvent(this, point, nextObjectTicketID, objectTileSpec);
				break;
			case EEventTileType.GyrePortal:
				gameEvent = new GyrePortalEvent(this, point, nextObjectTicketID, objectTileSpec);
				break;
			case EEventTileType.GyreSpawner:
				gameEvent = new GyreSpawnerEvent(this, point, nextObjectTicketID, objectTileSpec);
				break;
			case EEventTileType.LostItem:
				gameEvent = new LostItemEvent(this, point, nextObjectTicketID, objectTileSpec);
				break;
			case EEventTileType.Tutorial:
				gameEvent = new TutorialEvent(this, point, nextObjectTicketID, objectTileSpec);
				break;
			case EEventTileType.RareEnemySpawner:
				gameEvent = new RareEnemySpawnerEvent(this, point, nextObjectTicketID, objectTileSpec);
				break;
			case EEventTileType.EscortMissionManager:
				gameEvent = new EscortMissionManager(this, point, nextObjectTicketID, objectTileSpec);
				break;
			default:
				Console.WriteLine(" Failed to load tile ID: " + objectTileSpec.GetEventType());
				break;
			}
			if (gameEvent != null)
			{
				AddEvent(gameEvent);
				result = gameEvent;
				if (shouldInitialize)
				{
					gameEvent.Initialize();
				}
			}
		}
		return result;
	}

	private void PlaceStackableTile(Dictionary<Point, List<Tile>> layer, Tile tile)
	{
		List<Tile> list;
		if (layer.TryGetValue(tile.DictKey, out list))
		{
			list.Add(tile);
			return;
		}
		list = new List<Tile> { tile };
		layer[tile.DictKey] = list;
		int x = tile.DictKey.X;
		int y = tile.DictKey.Y;
		int width = _levelTileSize.X;
		if ((uint)x < (uint)width && (uint)y < (uint)_levelTileSize.Y)
		{
			int idx = y * width + x;
			if (layer == _backgroundTiles && _backgroundTileGrid != null && (uint)idx < (uint)_backgroundTileGrid.Length)
			{
				_backgroundTileGrid[idx] = list;
			}
			else if (layer == _foregroundTiles && _foregroundTileGrid != null && (uint)idx < (uint)_foregroundTileGrid.Length)
			{
				_foregroundTileGrid[idx] = list;
			}
		}
	}

	private void LoadProtagonists(LevelChangeRequest levelChangeRequest)
	{
		int count = _checkpoints.Count;
		Point inPosition = Point.Zero;
		bool flag = false;
		bool flag2 = false;
		int checkpointID = levelChangeRequest.CheckpointID;
		if (count == 0 || checkpointID == 0)
		{
			if (_timeGateEvent != null && levelChangeRequest.IsUsingWarp)
			{
				inPosition = new Point(_timeGateEvent.WarpInPoint.X, _playerStart.Y);
				flag = true;
				if (levelChangeRequest.PreviousLevelID > 0)
				{
					_timeGateEvent.ChangeTargetLevel(levelChangeRequest.PreviousLevelID);
				}
				if (levelChangeRequest.CutsceneToCall != 0)
				{
					_timeGateEvent.CutsceneToCall = levelChangeRequest.CutsceneToCall;
				}
			}
			else if (_transitionWarpEvent != null && levelChangeRequest.IsUsingWarp)
			{
				if (levelChangeRequest.CutsceneToCall != 0)
				{
					_transitionWarpEvent.CutsceneToCall = levelChangeRequest.CutsceneToCall;
				}
				inPosition = _transitionWarpEvent.SpitOutPlayer(levelChangeRequest);
				flag2 = true;
			}
			else if (_playerStart == Point.Zero)
			{
				EDirection enterDirection = levelChangeRequest.EnterDirection;
				foreach (GameEvent value in _levelEvents.Values)
				{
					EEventTileType eventType = value.EventType;
					if ((eventType == EEventTileType.WestTeleport && enterDirection == EDirection.West) || (eventType == EEventTileType.EastTeleport && enterDirection == EDirection.East) || (eventType == EEventTileType.NorthTeleport && enterDirection == EDirection.North) || (eventType == EEventTileType.SouthTeleport && enterDirection == EDirection.South))
					{
						inPosition = value.Position;
					}
				}
			}
			else
			{
				inPosition = _playerStart;
			}
		}
		else
		{
			SaveStatue saveStatue = ((checkpointID < count) ? _checkpoints[checkpointID - 1] : _checkpoints[0]);
			saveStatue.StopGlowingNow();
			inPosition = saveStatue.Position;
		}
		if (_protagonists.Count == 0)
		{
			int nextObjectTicketID = NextObjectTicketID;
			_protagonists.Add(nextObjectTicketID, new LunaisObj(inPosition, this, GCM.SpLunais, GCM.SpAltLunais, GCM.SpAltLunais2, Player1Controller, nextObjectTicketID));
			MainHero = _protagonists[nextObjectTicketID];
			if (flag)
			{
				MainHero.HideAndBlockInput(shouldBlockAndHide: true, shouldHideFamiliar: true);
				_timeGateEvent.IsActive = true;
			}
			else if (flag2)
			{
				MainHero.HideAndBlockInput(shouldBlockAndHide: true, shouldHideFamiliar: true);
			}
		}
	}

	private void InitializeMonsters()
	{
		foreach (Monster value in _enemies.Values)
		{
			value.InitializeMob();
		}
	}

	private void InitializeEvents()
	{
		foreach (GameEvent value in _levelEvents.Values)
		{
			value.Initialize();
		}
	}

	private void InitializeNPCs()
	{
		foreach (NPCBase value in _npcs.Values)
		{
			value.Initialize();
		}
	}

	public static EBGM GetLevelSong(int levelID, int roomID, GameSave gameSave)
	{
		switch (levelID)
		{
		case 0:
			return EBGM.None;
		case 1:
			if (gameSave.Inventory.RelicInventory.Inventory.ContainsKey(1))
			{
				return EBGM.Level01;
			}
			return CutsceneBase.GetIsCutsceneTriggeredByType(CutsceneBase.ECutsceneType.LakeDesolation1_Entrance, gameSave) ? EBGM.CsLakeDesolation : EBGM.None;
		case 2:
			if (roomID == 6 || roomID == 42)
			{
				return EBGM.Library;
			}
			return EBGM.Level02;
		case 3:
			if (roomID == 6 || roomID == 0 || roomID == 18)
			{
				return EBGM.Sanctuary;
			}
			return EBGM.Level03;
		case 4:
			return EBGM.Level04;
		case 5:
			return EBGM.Level05;
		case 6:
			return EBGM.Level06;
		case 7:
			return EBGM.Level07;
		case 8:
			if (roomID == 33 || roomID == 6 || roomID == 13)
			{
				return EBGM.CsPortal;
			}
			return EBGM.Level08;
		case 9:
			if (roomID == 33 || roomID == 6 || roomID == 13 || roomID == 21)
			{
				return EBGM.CsPortal2;
			}
			return EBGM.Level09;
		case 10:
			return EBGM.Level10;
		case 11:
			return EBGM.Level11;
		case 12:
			return EBGM.Level12;
		case 14:
			return EBGM.None;
		case 15:
			return EBGM.Level15;
		case 16:
			return EBGM.Level16;
		case 17:
			return EBGM.Default;
		default:
			return EBGM.Default;
		}
	}

	public void PlayLevelSong()
	{
		_jukebox.PlaySong(GetLevelSong(ID, RoomID, GameSave), shouldForceRestart: false, shouldImmediatelyStopPreviousSong: true);
	}

	public void Dispose()
	{
		DisposeAllObjects();
	}

	public bool ReloadRoom(float howLong)
	{
		bool result = false;
		if (!_isRoomReloadRequested)
		{
			_reloadTimer = howLong;
			_isRoomReloadRequested = true;
			result = true;
		}
		return result;
	}

	public bool CheckNearby(EDirection where, Point start)
	{
		bool result = false;
		bool flag = true;
		Point key = Point.Zero;
		switch (where)
		{
		case EDirection.East:
			if (start.X >= _levelTileSize.X)
			{
				result = true;
				flag = false;
			}
			else
			{
				key = new Point(start.X + 1, start.Y);
			}
			break;
		case EDirection.SouthEast:
			key = new Point(start.X + 1, start.Y + 1);
			break;
		case EDirection.South:
			key = new Point(start.X, start.Y + 1);
			break;
		case EDirection.SouthWest:
			key = new Point(start.X - 1, start.Y + 1);
			break;
		case EDirection.West:
			if (start.X > 0)
			{
				key = new Point(start.X - 1, start.Y);
			}
			break;
		case EDirection.NorthWest:
			key = new Point(start.X - 1, start.Y - 1);
			break;
		case EDirection.North:
			key = new Point(start.X, start.Y - 1);
			break;
		case EDirection.NorthEast:
			key = new Point(start.X + 1, start.Y - 1);
			break;
		case EDirection.Center:
			key = new Point(start.X, start.Y);
			break;
		default:
			flag = false;
			break;
		}
		if (flag)
		{
			Tile tile = GetSolidTileFast(key.X, key.Y);
			if (tile != null || _solidTiles.ContainsKey(key))
			{
				result = true;
			}
		}
		return result;
	}

	internal bool CheckNearbySolid(EDirection where, Point start)
	{
		bool flag = CheckNearby(where, start);
		if (flag)
		{
			Point pointFromDirection = GetPointFromDirection(start, where);
			Tile tile = GetSolidTileFast(pointFromDirection.X, pointFromDirection.Y);
			if (tile != null && tile.Type == ETileType.Platform)
			{
				flag = false;
			}
			else if (tile == null && _solidTiles.TryGetValue(pointFromDirection, out Tile t) && t.Type == ETileType.Platform)
			{
				flag = false;
			}
		}
		return flag;
	}

	public bool CheckNearbyIgnoreSlopes(EDirection where, Point start)
	{
		bool flag = CheckNearby(where, start);
		if (flag && (where == EDirection.North || where == EDirection.South) && CheckNearbyType(where, start) == ETileType.Slope)
		{
			flag = false;
		}
		return flag;
	}

	public ETileType CheckNearbyType(EDirection where, Point start)
	{
		Point pointFromDirection = GetPointFromDirection(start, where);
		Tile tile = GetSolidTileFast(pointFromDirection.X, pointFromDirection.Y);
		if (tile != null)
		{
			return tile.Type;
		}
		if (_solidTiles.TryGetValue(pointFromDirection, out Tile t))
		{
			return t.Type;
		}
		return ETileType.Passable;
	}

	public static Point GetPointFromDirection(Point start, EDirection where)
	{
		Point result = Point.Zero;
		switch (where)
		{
		case EDirection.East:
			result = new Point(start.X + 1, start.Y);
			break;
		case EDirection.SouthEast:
			result = new Point(start.X + 1, start.Y + 1);
			break;
		case EDirection.South:
			result = new Point(start.X, start.Y + 1);
			break;
		case EDirection.SouthWest:
			result = new Point(start.X - 1, start.Y + 1);
			break;
		case EDirection.West:
			result = new Point(start.X - 1, start.Y);
			break;
		case EDirection.NorthWest:
			result = new Point(start.X - 1, start.Y - 1);
			break;
		case EDirection.North:
			result = new Point(start.X, start.Y - 1);
			break;
		case EDirection.NorthEast:
			result = new Point(start.X + 1, start.Y - 1);
			break;
		case EDirection.Center:
			result = start;
			break;
		}
		return result;
	}

	public Tile FindFirstSolidTileInDirection(Point start, EDirection direction)
	{
		Point point = new Point(start.X / 16, start.Y / 16);
		while (point.X > 0 && point.X < _levelTileSize.X && point.Y > 0 && point.Y < _levelTileSize.Y)
		{
			point = GetPointFromDirection(point, direction);
			Tile tile = GetSolidTileFast(point.X, point.Y);
			if (tile != null)
			{
				return tile;
			}
			if (_solidTiles.TryGetValue(point, out Tile t))
			{
				return t;
			}
		}
		return null;
	}

	internal Point FindOpenTilePosition(Point startPoint)
	{
		Point result = startPoint;
		Point point = new Point(startPoint.X / 16, startPoint.Y / 16);
		if (IsKeyOutsideLevel(point) || _solidTiles.ContainsKey(point))
		{
			EDirection[] array = new EDirection[8]
			{
				EDirection.North,
				EDirection.South,
				EDirection.East,
				EDirection.West,
				EDirection.NorthWest,
				EDirection.NorthEast,
				EDirection.SouthWest,
				EDirection.SouthEast
			};
			for (int i = 1; i < 12; i++)
			{
				bool flag = true;
				EDirection[] array2 = array;
				foreach (EDirection where in array2)
				{
					Point b = GetPointFromDirection(Point.Zero, where).Multiply(i);
					Point key = point.Add(b);
					if (IsKeyOutsideLevel(key))
					{
						flag = false;
						continue;
					}
					if (_solidTiles.ContainsKey(key))
					{
						Tile tile = _solidTiles[key];
						if (tile.Type == ETileType.Solid)
						{
							flag = false;
							continue;
						}
						if (tile.Type == ETileType.Slope)
						{
							int num = tile.LookupTileHeight(result.X);
							int y = (tile.IsFlippedVertically ? (tile.Bbox.Bottom - num) : (tile.Bbox.Top + num));
							result = new Point(tile.Bbox.Center.X, y);
							flag = true;
						}
						else
						{
							flag = true;
							result = tile.Bbox.Center;
						}
						break;
					}
					flag = true;
					result = new Point(key.X * 16 + 8, key.Y * 16 + 8);
					break;
				}
				if (flag)
				{
					break;
				}
			}
		}
		return result;
	}

	private bool IsKeyOutsideLevel(Point key)
	{
		if (key.X >= 0 && key.Y >= 0 && key.X < RoomSize16.X)
		{
			return key.Y >= RoomSize16.Y;
		}
		return true;
	}

	public bool CheckIfHorizontallyAdjacentTilesBlock(EDirection inDirection, Point tileKey, GameObject target, Point whoPosition)
	{
		if (CheckNearby(inDirection, tileKey))
		{
			switch (CheckNearbyType(inDirection, tileKey))
			{
			case ETileType.Platform:
				return false;
			case ETileType.Slope:
				if (target is Tile tile)
				{
					int num = tile.LookupTileHeight(whoPosition.X);
					if (num > 16)
					{
						return false;
					}
					break;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private void DetectCollisions()
	{
		foreach (GameEvent value in _levelEvents.Values)
		{
			if (!value.DoesCollideWithTiles)
			{
				continue;
			}
			foreach (Point intermediatePosition in value.IntermediatePositions)
			{
				value.Position = intermediatePosition;
				value.SnapBboxToPosition();
				if (value.DetectTileCollisions())
				{
					break;
				}
			}
		}
		foreach (NPCBase value2 in _npcs.Values)
		{
			foreach (Point intermediatePosition2 in value2.IntermediatePositions)
			{
				value2.Position = intermediatePosition2;
				value2.SnapBboxToPosition();
				if (value2.DetectTileCollisions())
				{
					break;
				}
			}
		}
		foreach (Protagonist value3 in _protagonists.Values)
		{
			if (!value3.IsPlayable)
			{
				continue;
			}
			List<int> eventKeysList = new List<int>(_levelEvents.Keys);
			List<int> npcKeysList = new List<int>(_npcs.Keys);
			foreach (Point intermediatePosition3 in value3.IntermediatePositions)
			{
				value3.Position = intermediatePosition3;
				value3.SnapBboxToPosition();
				bool flag = value3.DetectFallDeath(RoomSize.Y);
				flag = value3.DetectTileCollisions() || flag;
				foreach (Monster value4 in _enemies.Values)
				{
					flag = value4.CheckObjectCollision(value3) || flag;
				}
				if (!value3.IsInvulnerable)
				{
					foreach (Projectile value5 in _enemyProjectiles.Values)
					{
						if (!value5.IsFrozen && value5.CanDamageThings)
						{
							Rectangle collidingRectangle = value3.GetCollidingRectangle(value5);
							if (collidingRectangle != Rectangle.Empty)
							{
								flag = true;
								value5.DetermineDamage(value3, collidingRectangle);
							}
						}
						else if (value5.IsFrozen && value5.CanBeStoodOnWhenFrozen)
						{
							flag = value5.CheckSolidCollision(value3) || flag;
						}
					}
				}
				foreach (Item value6 in _items.Values)
				{
					Vector2 intersectionDepth = value3.Bbox.GetIntersectionDepth(value6.Bbox);
					if (intersectionDepth != Vector2.Zero)
					{
						value6.GetItem(value3);
					}
				}
				foreach (int item in eventKeysList)
				{
					if (_levelEvents.TryGetValue(item, out GameEvent gameEvent))
					{
						if (value3.IsPrimaryPlayer || gameEvent.CanBeTriggeredByFamiliar)
						{
							Vector2 intersectionDepth2 = value3.Bbox.GetIntersectionDepth(gameEvent.TriggerBbox);
							if (intersectionDepth2 != Vector2.Zero)
							{
								flag = gameEvent.TriggerEvent(value3, intersectionDepth2) || flag;
							}
						}
					}
				}
				if (value3.IsPrimaryPlayer)
				{
					foreach (int item2 in npcKeysList)
					{
						if (_npcs.TryGetValue(item2, out NPCBase nPCBase))
						{
							flag = nPCBase.CheckForHeroCollision(value3) || flag;
						}
					}
				}
				value3.UpdatePreviousPosition();
				if (flag)
				{
					value3.PostCollisionUpdate();
					break;
				}
			}
		}
		foreach (Monster value7 in _enemies.Values)
		{
			if (!value7.DoesCollideWithTiles)
			{
				continue;
			}
			foreach (Point intermediatePosition4 in value7.IntermediatePositions)
			{
				value7.Position = intermediatePosition4;
				value7.SnapBboxToPosition();
				bool flag = value7.DetectTileCollisions();
				value7.UpdatePreviousPosition();
				if (flag)
				{
					value7.PostCollisionUpdate();
					break;
				}
			}
		}
		foreach (GameEvent value8 in _levelEvents.Values)
		{
			if (!value8.IsTriggerableByMonsters)
			{
				continue;
			}
			foreach (Monster value9 in _enemies.Values)
			{
				Vector2 intersectionDepth3 = value9.Bbox.GetIntersectionDepth(value8.TriggerBbox);
				if (intersectionDepth3 != Vector2.Zero)
				{
					value8.TriggerEvent(value9, intersectionDepth3);
					value9.UpdatePreviousPosition();
				}
			}
		}
		List<int> list3 = _heroProjectiles.Keys.ToList();
		for (int num = _heroProjectiles.Count - 1; num >= 0; num--)
		{
			Projectile projectile = _heroProjectiles[list3[num]];
			if (projectile.CanDamageThings && !projectile.IsDormant)
			{
				bool flag = false;
				foreach (Point intermediatePosition5 in projectile.IntermediatePositions)
				{
					projectile.Position = intermediatePosition5;
					projectile.SnapBboxToPosition();
					if (projectile.CanDamageEnemies)
					{
						foreach (Monster value10 in _enemies.Values)
						{
							if (value10.CanBeDamaged)
							{
								Rectangle collidingRectangle2 = value10.GetCollidingRectangle(projectile);
								if (collidingRectangle2 != Rectangle.Empty)
								{
									flag = projectile.DetermineDamage(value10, collidingRectangle2) || flag;
								}
							}
						}
					}
					if (projectile.DoesCollideWithTiles)
					{
						flag = projectile.DetectTileCollisions() || flag;
					}
					if (projectile.CanDamageEnemyProjectiles)
					{
						foreach (Projectile value11 in _enemyProjectiles.Values)
						{
							if (value11.EffectiveDamage > 0 && !value11.IsFrozen && (value11.DoesDieToEnemyProjectiles || (projectile.DoesKillProjectilesOnImpact && !value11.IsDamageArea)))
							{
								Rectangle collidingRectangle3 = value11.GetCollidingRectangle(projectile);
								if (collidingRectangle3 != Rectangle.Empty)
								{
									Vector2 intersectionDepth4 = value11.Bbox.GetIntersectionDepth(collidingRectangle3);
									if (intersectionDepth4 != Vector2.Zero)
									{
										projectile.OnKillOtherProjectile(value11, intersectionDepth4);
										value11.KillOnProjectileImpact(projectile);
									}
								}
							}
							else
							{
								if (!value11.DoesKillProjectilesOnImpact || projectile.IsDamageArea)
								{
									continue;
								}
								Rectangle collidingRectangle4 = value11.GetCollidingRectangle(projectile);
								if (collidingRectangle4 != Rectangle.Empty)
								{
									Vector2 intersectionDepth5 = value11.Bbox.GetIntersectionDepth(collidingRectangle4);
									if (intersectionDepth5 != Vector2.Zero)
									{
										value11.OnKillOtherProjectile(value11, intersectionDepth5);
										projectile.KillOnProjectileImpact(projectile);
									}
								}
							}
						}
					}
					if (projectile.CanDamageEvents)
					{
						List<int> list4 = _levelEvents.Keys.ToList();
						foreach (int item3 in list4)
						{
							GameEvent gameEvent2 = _levelEvents[item3];
							if (!gameEvent2.DoesCollideWithProjectiles)
							{
								continue;
							}
							Rectangle collidingRectangle5 = gameEvent2.GetCollidingRectangle(projectile);
							if (collidingRectangle5 != Rectangle.Empty)
							{
								Vector2 intersectionDepth6 = gameEvent2.TriggerBbox.GetIntersectionDepth(collidingRectangle5);
								if (intersectionDepth6 != Vector2.Zero)
								{
									gameEvent2.ProjectileTriggerEvent(projectile, intersectionDepth6);
								}
							}
						}
					}
					if (flag)
					{
						projectile.PostCollisionUpdate();
						break;
					}
				}
			}
		}
		foreach (Projectile value12 in _enemyProjectiles.Values)
		{
			if (value12.IsDormant || (!value12.DoesCollideWithTiles && value12.TeamSide != ETeamSide.Neutral))
			{
				continue;
			}
			bool flag = false;
			foreach (Point intermediatePosition6 in value12.IntermediatePositions)
			{
				value12.Position = intermediatePosition6;
				value12.SnapBboxToPosition();
				if (value12.DoesCollideWithTiles && value12.DetectTileCollisions())
				{
					value12.PostCollisionUpdate();
					flag = true;
				}
				if (value12.TeamSide == ETeamSide.Neutral && value12.CanDamageEnemies)
				{
					foreach (Monster value13 in _enemies.Values)
					{
						if (value13.CanBeDamaged && value13.DoesCollideWith(value12.DamageBbox, checkAppendages: true))
						{
							flag = true;
							value12.DetermineDamage(value13, value12.DamageBbox);
						}
					}
				}
				if (value12.DoesCollideWithTiles)
				{
					List<int> list5 = _levelEvents.Keys.ToList();
					foreach (int item4 in list5)
					{
						GameEvent gameEvent3 = _levelEvents[item4];
						if (!gameEvent3.DoesCollideWithProjectiles || !gameEvent3.IsTriggerableByMonsters)
						{
							continue;
						}
						Rectangle collidingRectangle6 = gameEvent3.GetCollidingRectangle(value12);
						if (collidingRectangle6 != Rectangle.Empty)
						{
							Vector2 intersectionDepth7 = gameEvent3.TriggerBbox.GetIntersectionDepth(collidingRectangle6);
							if (intersectionDepth7 != Vector2.Zero)
							{
								gameEvent3.ProjectileTriggerEvent(value12, intersectionDepth7);
							}
						}
					}
				}
				if (flag)
				{
					break;
				}
			}
		}
	}

	public void Update(float delta, bool active)
	{
		IsPreventingPauseMenuUsage = false;
		if (_levelChangeRequest != null)
		{
			ChangeRoom(_levelChangeRequest);
			_levelChangeRequest = null;
			IsFadeOutRequested = false;
			IsUsingWhiteFadeOut = false;
		}
		if (_isRoomReloadRequested)
		{
			_reloadTimer -= delta;
			if (_reloadTimer < 0f)
			{
				_reloadTimer = 0f;
				if (_isRoomReloadRequested)
				{
					_isRoomReloadRequested = false;
					ChangeRoom(RoomID, RoomEntrance);
				}
			}
		}
		if (ID != 0 && ID != 13 && ID <= 16 && !_hasAreaTitleBeenShown && !IsInTransitionRoom && AreaTitleBlocker == null)
		{
			_areaTitleShowTimer += delta;
			if (_areaTitleShowTimer >= 0.15f)
			{
				RequestToastPopup(EToastType.AreaTitle, ID);
				_hasAreaTitleBeenShown = true;
				GameSave.SetValue($"AreaTitleShown{ID}", value: true);
			}
		}
		AddNewDelayedObjects();
		RemoveDeadObjects();
		WasLastActive = active;
		WasTimeFrozen = IsTimeFrozen;
		UpdateScripts(delta);
		if (!IsDoingPlayerDeathCutscene)
		{
			UpdateEvents(delta);
		}
		UpdateHeroes(delta);
		if (!IsDoingPlayerDeathCutscene)
		{
			UpdateEnemies(delta);
			UpdateNPCs(delta);
			UpdateProjectiles(delta);
			UpdateItems(delta);
			UpdateWaterTiles(delta);
			DetectCollisions();
			UpdateAnimations(delta);
			UpdateCamera(delta);
			UpdateBackgrounds(delta);
			UpdateNumbers(delta);
			UpdateTilesetShaders(delta);
			if (_levelChangeRequest != null)
			{
				IsFadeOutRequested = true;
			}
		}
		else
		{
			UpdateAnimations(delta);
		}
	}

	private void UpdateHeroes(float delta)
	{
		foreach (Protagonist value in _protagonists.Values)
		{
			value.Update(delta);
		}
	}

	private void UpdateEnemies(float delta)
	{
		foreach (Monster value in _enemies.Values)
		{
			value.Update(delta);
		}
	}

	private void UpdateNPCs(float delta)
	{
		foreach (NPCBase value in _npcs.Values)
		{
			value.Update(delta);
		}
	}

	private void UpdateProjectiles(float delta)
	{
		foreach (Projectile value in _heroProjectiles.Values)
		{
			value.Update(delta);
		}
		foreach (Projectile value2 in _enemyProjectiles.Values)
		{
			value2.Update(delta);
		}
	}

	private void UpdateBackgrounds(float delta)
	{
		delta = (IsTimeFrozen ? 0f : delta);
		foreach (Background background in _backgrounds)
		{
			background.Update(delta);
		}
		foreach (Background foreground in _foregrounds)
		{
			foreground.Update(delta);
		}
	}

	private void UpdateAnimations(float delta)
	{
		for (int num = _heroAnimations.Count - 1; num >= 0; num--)
		{
			BattleAnimation battleAnimation = _heroAnimations[num];
			battleAnimation.Update(delta);
		}
		for (int num2 = _enemyAnimations.Count - 1; num2 >= 0; num2--)
		{
			BattleAnimation battleAnimation2 = _enemyAnimations[num2];
			battleAnimation2.Update(delta);
		}
		for (int num3 = _neutralAnimations.Count - 1; num3 >= 0; num3--)
		{
			BattleAnimation battleAnimation3 = _neutralAnimations[num3];
			battleAnimation3.Update(delta);
		}
	}

	private void UpdateWaterTiles(float delta)
	{
		if (IsTimeFrozen)
		{
			return;
		}
		foreach (WaterTile updatableWaterTile in _updatableWaterTiles)
		{
			updatableWaterTile.Update(delta);
		}
	}

	private void UpdateItems(float delta)
	{
		foreach (Item value in _items.Values)
		{
			value.Update(delta);
		}
	}

	private void UpdateEvents(float delta)
	{
		foreach (GameEvent value in _levelEvents.Values)
		{
			value.Update(delta);
		}
	}

	private void UpdateCamera(float delta)
	{
		if (!_isCameraUpdateDisabled)
		{
			SetCameraToPlayerPosition();
		}
		_camera.Update(delta, _levelCameraBounds);
		if (_camera.HasChanged)
		{
			CameraChanged();
		}
		_camera.ResetChanged();
	}

	internal void InstantUpdateCamera()
	{
		UpdateCamera(0f);
	}

	private void UpdateNumbers(float delta)
	{
		for (int num = HUDNumberQueue.Count - 1; num >= 0; num--)
		{
			HudNumber hudNumber = HUDNumberQueue[num];
			hudNumber.Update(delta);
			if (hudNumber.IsFinished)
			{
				HUDNumberQueue.RemoveAt(num);
			}
		}
	}

	private void UpdateScripts(float delta)
	{
		if (_isCutsceneSkipFading)
		{
			_cutsceneSkipFadeTimer -= delta;
			if (_cutsceneSkipFadeTimer <= 0f)
			{
				_isCutsceneSkipFading = false;
				_isSkippingCutscene = true;
			}
		}
		IsActiveScriptUnskippable = false;
		bool flag = false;
		for (int num = _activeScripts.Count - 1; num >= 0; num--)
		{
			ScriptAction scriptAction = _activeScripts[num];
			scriptAction.Update(delta);
			if (scriptAction.IsFinished)
			{
				_activeScripts.RemoveAt(num);
			}
			else
			{
				switch (scriptAction.ScriptType)
				{
				case EScriptType.CutsceneStart:
					if (_waitingScripts.Count == 0)
					{
						_activeScripts.RemoveAt(num);
						IsPlayerInputBlocked = false;
						IsUIRequestingHide = false;
					}
					else
					{
						IsPlayerInputBlocked = true;
						IsUIRequestingHide = true;
					}
					break;
				case EScriptType.MoveCamera:
					UpdateCameraScript(scriptAction);
					break;
				case EScriptType.SepiaFade:
					UpdateSepiaFadeScript(scriptAction);
					break;
				}
				if (scriptAction.DoesBlockQueue)
				{
					flag = true;
				}
				if (scriptAction.IsUnskippable)
				{
					IsActiveScriptUnskippable = true;
				}
			}
		}
		while ((!flag || (_isSkippingCutscene && !IsActiveScriptUnskippable)) && _waitingScripts.Count > 0)
		{
			ScriptAction scriptAction2 = _waitingScripts.Dequeue();
			flag = scriptAction2.DoesBlockQueue;
			IsActiveScriptUnskippable = scriptAction2.IsUnskippable;
			if (_isSkippingCutscene && !scriptAction2.IsUnskippable)
			{
				scriptAction2.IsBeingSkipped = true;
			}
			InsertScript(scriptAction2);
		}
		if (!_isSkippingCutscene || IsActiveScriptUnskippable)
		{
			return;
		}
		_dialogueQueue.Clear();
		foreach (ScriptAction activeScript in _activeScripts)
		{
			activeScript.Skip();
		}
		_isSkippingCutscene = false;
		Console.WriteLine($"[Level CutsceneSkipped] Complete. activeScripts={_activeScripts.Count}, waitingScripts={_waitingScripts.Count}, IsPlayerInputBlocked={IsPlayerInputBlocked}");
	}

	internal void InsertScript(ScriptAction script)
	{
		script.Update(0f);
		if (script.DoesClearSameType)
		{
			ClearActiveScriptsOfType(script);
		}
		_activeScripts.Insert(0, script);
		switch (script.TargetType)
		{
		case EScriptTargetType.Player1:
			if (MainHero != null)
			{
				MainHero.AddScriptAction(script);
			}
			break;
		case EScriptTargetType.AllPlayers:
			foreach (Protagonist value in _protagonists.Values)
			{
				value.AddScriptAction(script);
			}
			break;
		case EScriptTargetType.Familiar:
			if (MainHero != null)
			{
				LunaisObj lunaisObj = (LunaisObj)MainHero;
				if (lunaisObj != null && lunaisObj.FamiliarManager != null)
				{
					lunaisObj.FamiliarManager.AddScriptAction(script);
				}
			}
			break;
		case EScriptTargetType.Specified:
			if (script.ScriptTarget != null)
			{
				script.ScriptTarget.AddScriptAction(script);
			}
			break;
		}
		if (script.ScriptType == EScriptType.Dialogue && script.Dialogue != null)
		{
			if (_dialogueQueue.Count < 20)
			{
				_dialogueQueue.Enqueue(script.Dialogue);
				script.HasStarted = true;
				if (script.Dialogue.IsGhostDialogue)
				{
					return;
				}
				{
					foreach (ScriptAction waitingScript in _waitingScripts)
					{
						if (waitingScript.ScriptType == EScriptType.Dialogue && waitingScript.Dialogue != null && !waitingScript.Dialogue.IsGhostDialogue)
						{
							waitingScript.Dialogue.ShouldStartOpened = true;
							waitingScript.Dialogue.PreviousPortrait = script.Dialogue.Portrait;
							script.Dialogue.ShouldCloseOnEnd = false;
							break;
						}
						if (waitingScript.DoesBlockQueue)
						{
							break;
						}
					}
					return;
				}
			}
			script.Dialogue.FinishDialogue();
		}
		else if (script.ScriptType == EScriptType.MoveCamera)
		{
			UpdateCameraScript(script);
		}
		else if (script.ScriptType == EScriptType.SepiaFade)
		{
			UpdateSepiaFadeScript(script);
		}
		else if (script.ScriptType == EScriptType.Delegate && script.Delegate != null)
		{
			script.Delegate();
		}
		else if (script.ScriptType == EScriptType.PlayCue && script.Arguments != Vector4.Zero)
		{
			if (!script.IsBeingSkipped)
			{
				int num = (int)script.Arguments.Y;
				int num2 = (int)script.Arguments.Z;
				if (num == -1 && num2 == -1)
				{
					JukeBox.PlayCue((ESFX)script.Arguments.X);
				}
				else
				{
					PlayCue((ESFX)script.Arguments.X, new Point(num, num2));
				}
			}
		}
		else if (script.ScriptType == EScriptType.PlaySong && script.Arguments != Vector4.Zero)
		{
			JukeBox.PlaySong((EBGM)script.Arguments.X, script.Arguments.Y > 0f, script.Arguments.Z > 0f);
		}
		else if (script.ScriptType == EScriptType.FadeOutSong)
		{
			JukeBox.FadeOutSong(script.Arguments.X);
		}
		else if (script.ScriptType == EScriptType.ScreenShake)
		{
			RequestScreenShake(new Vector2(script.Arguments.X, script.Arguments.Y), script.Arguments.Z, script.Arguments.W, isAffectedByTime: true);
		}
		else if (script.ScriptType == EScriptType.GiveItem)
		{
			switch (script.ItemToGiveType)
			{
			case EInventoryCategoryType.UseItem:
			{
				EInventoryUseItemType itemToGive6 = (EInventoryUseItemType)script.ItemToGive;
				GameSave.Inventory.AddItem(itemToGive6, script.ItemToGiveCount);
				RequestItemGetPopup(itemToGive6);
				break;
			}
			case EInventoryCategoryType.Equipment:
			{
				EInventoryEquipmentType itemToGive5 = (EInventoryEquipmentType)script.ItemToGive;
				GameSave.Inventory.AddItem(itemToGive5);
				RequestItemGetPopup(itemToGive5);
				break;
			}
			case EInventoryCategoryType.Orb:
			{
				EInventoryOrbType itemToGive4 = (EInventoryOrbType)script.ItemToGive;
				GameSave.GiveOrb(itemToGive4, EOrbSlot.Melee);
				break;
			}
			case EInventoryCategoryType.Relic:
			{
				EInventoryRelicType itemToGive3 = (EInventoryRelicType)script.ItemToGive;
				GameSave.UnlockRelic(itemToGive3);
				break;
			}
			case EInventoryCategoryType.Familiar:
			{
				EInventoryFamiliarType itemToGive2 = (EInventoryFamiliarType)script.ItemToGive;
				GameSave.GiveFamiliar(itemToGive2);
				break;
			}
			case EInventoryCategoryType.Journal:
			{
				EInventoryJournalType itemToGive = (EInventoryJournalType)script.ItemToGive;
				GameSave.Inventory.JournalCollection.AddItem(script.ItemToGive);
				RequestItemGetPopup(itemToGive);
				break;
			}
			}
		}
		else if (script.ScriptType == EScriptType.RelicOrbGetToast)
		{
			IsRequestingToast = true;
			RequestedToastType = EToastType.RelicOrbGet;
			RequestedToastScript = script;
		}
		else if (script.ScriptType == EScriptType.QuestCompleteToast)
		{
			IsRequestingToast = true;
			RequestedToastType = EToastType.QuestComplete;
			RequestedToastScript = script;
		}
		else if (script.ScriptType == EScriptType.LevelRoomChange)
		{
			LevelChangeRequest levelRoomChangeRequest = script.LevelRoomChangeRequest;
			if (levelRoomChangeRequest != null)
			{
				if (levelRoomChangeRequest.LevelID == ID && (levelRoomChangeRequest.PreviousLevelID == 0 || levelRoomChangeRequest.PreviousLevelID == levelRoomChangeRequest.LevelID))
				{
					RequestChangeRoom(levelRoomChangeRequest);
				}
				else
				{
					RequestChangeLevel(levelRoomChangeRequest);
				}
			}
		}
		else if (script.ScriptType == EScriptType.FadeInFadeOut)
		{
			if (script.Arguments.Y > 0f || script.Arguments.Z > 0f || script.Arguments.W > 0f)
			{
				RequestScreenFadeOut(script.Arguments.X, script.Arguments.Y, script.Arguments.Z, script.Arguments.W);
			}
			else
			{
				RequestScreenFadeOut(script.Arguments.X);
			}
		}
		else if (script.ScriptType == EScriptType.ScreenFlash && !script.IsBeingSkipped)
		{
			RequestScreenFlash(script.Arguments.X, script.Arguments.Y, script.Arguments.Z);
		}
		else if (script.ScriptType == EScriptType.LockUnlockCamera)
		{
			SetCameraUpdateDisable(script.Arguments.X < 1f);
		}
		else if (script.ScriptType == EScriptType.HideShowPlayer && MainHero != null)
		{
			MainHero.HideAndBlockInput(script.Arguments.X < 1f, script.Arguments.Y <= 0f);
		}
	}

	private void UpdateCameraScript(ScriptAction script)
	{
		if (script.Arguments.W <= 0f && script.Arguments.Z <= 0f)
		{
			script.Arguments = new Vector4(script.Arguments.X, script.Arguments.Y, CameraPosition.X, CameraPosition.Y);
		}
		Point targetPoint;
		if (script.ActionTimer > 0f && script.Duration > 0f)
		{
			Point start = new Point((int)script.Arguments.Z, (int)script.Arguments.W);
			Point end = new Point((int)script.Arguments.X, (int)script.Arguments.Y);
			float amount = 1f - script.ActionTimer / script.Duration;
			ECameraScriptPanType eCameraScriptPanType = (ECameraScriptPanType)script.IntArgument;
			if (eCameraScriptPanType == ECameraScriptPanType.Default)
			{
				eCameraScriptPanType = ((!(script.Duration < 2f)) ? ECameraScriptPanType.Linear : ECameraScriptPanType.Sine);
			}
			targetPoint = eCameraScriptPanType switch
			{
				ECameraScriptPanType.Sine => start.SineInterpolate(end, amount), 
				ECameraScriptPanType.Cos => start.CosInterpolate(end, amount), 
				_ => start.Lerp(end, amount), 
			};
		}
		else
		{
			targetPoint = new Point((int)script.Arguments.X, (int)script.Arguments.Y);
		}
		bool doesUseFollowBubble = _camera.DoesUseFollowBubble;
		_camera.DoesUseFollowBubble = false;
		_camera.TargetPoint = targetPoint;
		_camera.Update(0f, _levelCameraBounds);
		_camera.DoesUseFollowBubble = doesUseFollowBubble;
		CameraChanged();
		_camera.ResetChanged();
	}

	private void UpdateSepiaFadeScript(ScriptAction script)
	{
		bool flag = script.Arguments.X == 1f;
		float num = 1f;
		if (script.Duration > 0f)
		{
			num = script.ActionTimer / script.Duration;
		}
		num = (CutsceneSepiaEffectPercentage = (flag ? num : (1f - num)));
		bool flag2 = num > 0f;
		if (flag2 != IsUsingCutsceneSepiaEffect)
		{
			if (flag2)
			{
				FreezeTime(ETeamSide.Heroes, reFreeze: true);
			}
			else
			{
				UnfreezeTime(ETeamSide.Heroes);
			}
		}
		IsUsingCutsceneSepiaEffect = flag2;
	}

	private void ClearActiveScriptsOfType(ScriptAction newScript)
	{
		List<ScriptAction> list = new List<ScriptAction>();
		EScriptType scriptType = newScript.ScriptType;
		EScriptTargetType targetType = newScript.TargetType;
		EScriptActionType actionType = newScript.ActionType;
		foreach (ScriptAction activeScript in _activeScripts)
		{
			if (activeScript.ScriptType == scriptType && activeScript.TargetType == targetType && activeScript.ActionType == actionType)
			{
				list.Add(activeScript);
			}
		}
		foreach (ScriptAction item in list)
		{
			_activeScripts.Remove(item);
		}
	}

	private void UpdateTilesetShaders(float delta)
	{
		IsPowerOff = false;
		if (ID != 11)
		{
			return;
		}
		if (!IsInTransitionRoom && !_isOverridingPowerOff && GameSave.GetSaveBool("11_LabPower"))
		{
			IsPowerOff = true;
			_levelShaderTimer += delta * 2f;
			if (_levelShaderTimer >= (float)Math.PI * 2f)
			{
				_levelShaderTimer -= (float)Math.PI * 2f;
			}
			int num = 180 - (int)(Math.Sin(_levelShaderTimer) * 16.0);
			LevelDrawColor = new Color(num + 48, num + 16, num, 255);
		}
		else
		{
			LevelDrawColor = Color.White;
		}
	}

	internal void SetLevelDrawColor(Color color)
	{
		LevelDrawColor = color;
	}

	public void DrawTiles(SpriteBatch spriteBatch)
	{
		int stride = _levelTileSize.X;
		int totalSize = stride * _levelTileSize.Y;
		int left = Math.Max(0, VisibleArea16.Left - 1);
		int right = Math.Min(stride - 1, VisibleArea16.Right + 1);
		int top = Math.Max(0, VisibleArea16.Top - 1);
		int bottom = Math.Min(_levelTileSize.Y - 1, VisibleArea16.Bottom + 1);

		if (_solidTileGrid != null && _waterTileGrid != null && _solidTileGrid.Length == totalSize && _waterTileGrid.Length == totalSize)
		{
			for (int j = top; j <= bottom; j++)
			{
				int rowOffset = j * stride;
				for (int i = left; i <= right; i++)
				{
					int idx = rowOffset + i;
					WaterTile waterTile = _waterTileGrid[idx];
					if (waterTile != null)
					{
						waterTile.Draw(spriteBatch);
					}
					Tile solidTile = _solidTileGrid[idx];
					if (solidTile != null && solidTile.Type != ETileType.Platform)
					{
						solidTile.Draw(spriteBatch);
					}
				}
			}
		}
		else
		{
			Rectangle rectangle = new Rectangle(VisibleArea16.Left - 1, VisibleArea16.Top - 1, VisibleArea16.Width + 2, VisibleArea16.Height + 2);
			for (int i = rectangle.Left; i <= rectangle.Right; i++)
			{
				for (int j = rectangle.Top; j <= rectangle.Bottom; j++)
				{
					Point key = new Point(i, j);
					if (_waterTiles.TryGetValue(key, out WaterTile waterTile))
					{
						waterTile.Draw(spriteBatch);
					}
					if (_solidTiles.TryGetValue(key, out Tile solidTile) && solidTile.Type != ETileType.Platform)
					{
						solidTile.Draw(spriteBatch);
					}
				}
			}
		}

		Rectangle rect = new Rectangle(VisibleArea16.Left - 1, VisibleArea16.Top - 1, VisibleArea16.Width + 2, VisibleArea16.Height + 2);
		foreach (TileSwath tileSwath in _tileSwaths)
		{
			if (tileSwath.Area.Intersects(rect))
			{
				tileSwath.Draw(spriteBatch, rect);
			}
		}
	}

	public void DrawPlatforms(SpriteBatch spriteBatch)
	{
		int stride = _levelTileSize.X;
		int totalSize = stride * _levelTileSize.Y;
		int left = Math.Max(0, VisibleArea16.Left - 1);
		int right = Math.Min(stride - 1, VisibleArea16.Right + 1);
		int top = Math.Max(0, VisibleArea16.Top - 1);
		int bottom = Math.Min(_levelTileSize.Y - 1, VisibleArea16.Bottom + 1);

		if (_solidTileGrid != null && _solidTileGrid.Length == totalSize)
		{
			for (int j = top; j <= bottom; j++)
			{
				int rowOffset = j * stride;
				for (int i = left; i <= right; i++)
				{
					Tile solidTile = _solidTileGrid[rowOffset + i];
					if (solidTile != null && solidTile.Type == ETileType.Platform)
					{
						solidTile.Draw(spriteBatch);
					}
				}
			}
		}
		else
		{
			for (int i = VisibleArea16.Left - 1; i <= VisibleArea16.Right + 1; i++)
			{
				for (int j = VisibleArea16.Top - 1; j <= VisibleArea16.Bottom + 1; j++)
				{
					Point key = new Point(i, j);
					if (_solidTiles.TryGetValue(key, out Tile solidTile) && solidTile.Type == ETileType.Platform)
					{
						solidTile.Draw(spriteBatch);
					}
				}
			}
		}
	}

	public void DrawForegroundTiles(SpriteBatch spriteBatch)
	{
		int stride = _levelTileSize.X;
		int totalSize = stride * _levelTileSize.Y;
		int left = Math.Max(0, VisibleArea16.Left - 1);
		int right = Math.Min(stride - 1, VisibleArea16.Right + 1);
		int top = Math.Max(0, VisibleArea16.Top - 1);
		int bottom = Math.Min(_levelTileSize.Y - 1, VisibleArea16.Bottom + 1);

		if (_foregroundTileGrid != null && _foregroundTileGrid.Length == totalSize)
		{
			for (int j = top; j <= bottom; j++)
			{
				int rowOffset = j * stride;
				for (int i = left; i <= right; i++)
				{
					List<Tile> list = _foregroundTileGrid[rowOffset + i];
					if (list != null)
					{
						for (int k = 0; k < list.Count; k++)
						{
							list[k].Draw(spriteBatch);
						}
					}
				}
			}
		}
		else
		{
			for (int i = VisibleArea16.Left - 1; i <= VisibleArea16.Right + 1; i++)
			{
				for (int j = VisibleArea16.Top - 1; j <= VisibleArea16.Bottom + 1; j++)
				{
					Point key = new Point(i, j);
					if (_foregroundTiles.TryGetValue(key, out List<Tile> list))
					{
						for (int k = 0; k < list.Count; k++)
						{
							list[k].Draw(spriteBatch);
						}
					}
				}
			}
		}
	}

	public void DrawBackgroundTiles(SpriteBatch spriteBatch)
	{
		int stride = _levelTileSize.X;
		int totalSize = stride * _levelTileSize.Y;
		int left = Math.Max(0, VisibleArea16.Left - 1);
		int right = Math.Min(stride - 1, VisibleArea16.Right + 1);
		int top = Math.Max(0, VisibleArea16.Top - 1);
		int bottom = Math.Min(_levelTileSize.Y - 1, VisibleArea16.Bottom + 1);

		if (_backgroundTileGrid != null && _backgroundTileGrid.Length == totalSize)
		{
			for (int j = top; j <= bottom; j++)
			{
				int rowOffset = j * stride;
				for (int i = left; i <= right; i++)
				{
					List<Tile> list = _backgroundTileGrid[rowOffset + i];
					if (list != null)
					{
						for (int k = 0; k < list.Count; k++)
						{
							list[k].Draw(spriteBatch);
						}
					}
				}
			}
		}
		else
		{
			for (int i = VisibleArea16.Left - 1; i <= VisibleArea16.Right + 1; i++)
			{
				for (int j = VisibleArea16.Top - 1; j <= VisibleArea16.Bottom + 1; j++)
				{
					Point key = new Point(i, j);
					if (_backgroundTiles.TryGetValue(key, out List<Tile> list))
					{
						for (int k = 0; k < list.Count; k++)
						{
							list[k].Draw(spriteBatch);
						}
					}
				}
			}
		}
	}

	public void DrawBackgrounds(SpriteBatch spriteBatch, float camZoom)
	{
		foreach (Background background in _backgrounds)
		{
			background.Draw(spriteBatch);
		}
	}

	public void DrawForegrounds(SpriteBatch spriteBatch, float camZoom)
	{
		foreach (Background foreground in _foregrounds)
		{
			foreground.Draw(spriteBatch);
		}
	}

	public void DrawMonsters(SpriteBatch spriteBatch, bool isAffectedByTime)
	{
		foreach (Monster value in _enemies.Values)
		{
			if (value.IsAffectedByTime == isAffectedByTime)
			{
				value.Draw(spriteBatch);
			}
		}
	}

	public void DrawGameEvents(SpriteBatch spriteBatch, ETeamSide teamSide, EDrawPlane drawPlane)
	{
		foreach (GameEvent value in _levelEvents.Values)
		{
			if (value.DrawPlane == drawPlane && !value.IsAffectedByTime == (teamSide == ETeamSide.Heroes))
			{
				value.Draw(spriteBatch);
			}
		}
	}

	public void DrawHeroes(SpriteBatch spriteBatch)
	{
		foreach (Protagonist value in _protagonists.Values)
		{
			value.Draw(spriteBatch);
		}
	}

	public void DrawNPCs(SpriteBatch spriteBatch, EDrawPlane drawPlane)
	{
		foreach (NPCBase value in _npcs.Values)
		{
			if (drawPlane == EDrawPlane.Front && value.DrawPlane == EDrawPlane.Front)
			{
				value.Draw(spriteBatch);
			}
			else if (drawPlane != EDrawPlane.Front && value.DrawPlane != EDrawPlane.Front)
			{
				value.Draw(spriteBatch);
			}
		}
	}

	public void DrawBattleAnimations(SpriteBatch spriteBatch, ETeamSide animationSide, EDrawPlane drawPlane)
	{
		switch (animationSide)
		{
		case ETeamSide.Heroes:
		{
			foreach (BattleAnimation heroAnimation in _heroAnimations)
			{
				if (heroAnimation.DrawPlane == drawPlane)
				{
					heroAnimation.Draw(spriteBatch);
				}
			}
			return;
		}
		case ETeamSide.Enemies:
		{
			foreach (BattleAnimation enemyAnimation in _enemyAnimations)
			{
				if (enemyAnimation.DrawPlane == drawPlane)
				{
					enemyAnimation.Draw(spriteBatch);
				}
			}
			return;
		}
		}
		foreach (BattleAnimation neutralAnimation in _neutralAnimations)
		{
			if (neutralAnimation.DrawPlane == drawPlane)
			{
				neutralAnimation.Draw(spriteBatch);
			}
		}
	}

	public void DrawProjectiles(SpriteBatch spriteBatch, ETeamSide animationSide)
	{
		if (animationSide == ETeamSide.Heroes)
		{
			foreach (Projectile value in _heroProjectiles.Values)
			{
				if (value.BackPane)
				{
					value.Draw(spriteBatch);
				}
			}
			foreach (Projectile value2 in _heroProjectiles.Values)
			{
				if (!value2.BackPane)
				{
					value2.Draw(spriteBatch);
				}
			}
		}
		if (animationSide != ETeamSide.Enemies)
		{
			return;
		}
		foreach (Projectile value3 in _enemyProjectiles.Values)
		{
			value3.Draw(spriteBatch);
		}
	}

	public void DrawItems(SpriteBatch spriteBatch)
	{
		foreach (Item value in _items.Values)
		{
			value.Draw(spriteBatch);
		}
	}

	public void DrawNumbers(SpriteBatch spriteBatch)
	{
		foreach (HudNumber item in HUDNumberQueue)
		{
			item.Draw(spriteBatch, _camera);
		}
	}

	public void DrawVisibleArea(SpriteBatch spriteBatch)
	{
		spriteBatch.Draw(GCM.TxBlankSquare, new Rectangle((int)(LevelRenderCenter.X - (CameraPosition.X - (float)_visibleArea.X)), (int)(LevelRenderCenter.Y - (CameraPosition.Y - (float)_visibleArea.Y)), _visibleArea.Width, _visibleArea.Height), null, new Color(10, 0, 150, 150));
	}

	public void ResetCameraToInitialPosition()
	{
		_camera.Position = Point.Zero;
		_camera.Zoom = 1f;
		VisibleSize = Vector2.Divide(DisplaySize, IsInEditMode ? _camera.Zoom : ((float)Constants.InGameZoom));
		_visibleArea = new Rectangle((int)(CameraPosition.X - VisibleSize.X / 2f), (int)(CameraPosition.Y - VisibleSize.Y / 2f), (int)VisibleSize.X, (int)VisibleSize.Y);
		_visibleArea16 = new Rectangle(VisibleArea.X / 16, VisibleArea.Y / 16, VisibleArea.Width / 16, VisibleArea.Height / 16);
		_objectVisibleArea = new Rectangle(_visibleArea.X, _visibleArea.Y, (int)(VisibleSize.X * 1.5f), (int)(VisibleSize.Y * 1.5f));
		Point point = new Point(0, (int)(VisibleSize.Y * 0.2f));
		_projectileVisibleArea = new Rectangle(VisibleArea.X, VisibleArea.Y, VisibleArea.Width + point.X, VisibleArea.Height + point.Y);
		CameraChanged();
	}

	public void CameraChanged()
	{
		CameraPosition = new Vector2(_camera.Position.X, _camera.Position.Y);
		CameraZoom = _camera.Zoom;
		_visibleArea.Location = new Point((int)(CameraPosition.X - VisibleSize.X / 2f), (int)(CameraPosition.Y - VisibleSize.Y / 2f));
		_visibleArea16.Location = new Point(VisibleArea.X / 16, VisibleArea.Y / 16);
		Point center = _visibleArea.Center;
		_objectVisibleArea.Location = new Point(center.X - _objectVisibleArea.Width / 2, center.Y - _objectVisibleArea.Height / 2);
		_projectileVisibleArea.Location = new Point(center.X - _projectileVisibleArea.Width / 2, center.Y - _projectileVisibleArea.Height / 2);
		_camera.ResetChanged();
	}

	public void SetCameraPosition(Point newPosition)
	{
		_camera.Position = newPosition;
		_camera.TargetPoint = newPosition;
		CameraChanged();
	}

	internal void SetCameraToPlayerPosition()
	{
		_camera.TargetPoint = new Point(MainHero.Position.X, MainHero.Position.Y - 16);
	}

	public void SetCameraUpdateDisable(bool isDisabled)
	{
		_isCameraUpdateDisabled = isDisabled;
	}

	public void ZoomCamera()
	{
		_camera.Zoom -= 0.5f;
		if (_camera.Zoom < 0.5f)
		{
			_camera.Zoom = Constants.InGameZoom;
		}
		_camera.Zoom = 1f;
		int num = (int)(640f / _camera.Zoom);
		int num2 = (int)(360f / _camera.Zoom);
		if (_camera.Zoom < 2f)
		{
			num = 320;
			num2 = 180;
		}
		_levelCameraBounds = new Rectangle(num, 0, _levelTileSize.X * 16 - num, _levelTileSize.Y * 16 - num2);
		CameraChanged();
	}

	public void AddProjectile(Projectile newProjectile)
	{
		if ((_heroProjectiles.ContainsKey(newProjectile.ID) && _heroProjectiles[newProjectile.ID] == newProjectile) || (_enemyProjectiles.ContainsKey(newProjectile.ID) && _enemyProjectiles[newProjectile.ID] == newProjectile))
		{
			return;
		}
		newProjectile.ID = _projectileTicketIDDispenser.GetNext();
		if (newProjectile.TeamSide == ETeamSide.Heroes)
		{
			_heroProjectiles.Add(newProjectile.ID, newProjectile);
			return;
		}
		_enemyProjectiles.Add(newProjectile.ID, newProjectile);
		if (IsTimeFrozen && newProjectile.IsAffectedByTime)
		{
			newProjectile.Update(0f);
			newProjectile.Freeze();
		}
	}

	public void AddAnimation(BattleAnimation newAnimation)
	{
		if (newAnimation.TeamSide == ETeamSide.Heroes)
		{
			_heroAnimations.Add(newAnimation);
		}
		else if (newAnimation.TeamSide == ETeamSide.Enemies)
		{
			_enemyAnimations.Add(newAnimation);
		}
		else
		{
			_neutralAnimations.Add(newAnimation);
		}
	}

	public void AddAnimation(EBattleAnimationType animationType, Point position)
	{
		AddAnimation(animationType, position, ETeamSide.Neutral, isFacingRight: false, doesPlaySFX: true);
	}

	public void AddAnimation(EBattleAnimationType animationType, Point position, ETeamSide side)
	{
		AddAnimation(animationType, position, side, isFacingRight: false, doesPlaySFX: true);
	}

	public void AddAnimation(EBattleAnimationType animationType, Point position, ETeamSide teamSide, bool isFacingRight)
	{
		AddAnimation(animationType, position, teamSide, isFacingRight, doesPlaySFX: true);
	}

	public void AddAnimation(EBattleAnimationType animationType, Point position, ETeamSide teamSide, bool isFacingRight, bool doesPlaySFX)
	{
		BattleAnimation battleAnimation = null;
		if (animationType == EBattleAnimationType.AuraExplosion)
		{
			position = position.Add(0, -19);
		}
		if (_cachedAnimations.ContainsKey((int)animationType))
		{
			List<BattleAnimation> list = _cachedAnimations[(int)animationType];
			foreach (BattleAnimation item in list)
			{
				if (!item.IsFinished)
				{
					continue;
				}
				item.RefreshFacing(animationType, isFacingRight);
				item.Reset(position, isFacingRight);
				item.TeamSide = teamSide;
				battleAnimation = item;
				if (doesPlaySFX && IsWithinCameraDistance(position))
				{
					ESFX eSFXFromBattleAnimationType = BattleAnimation.GetESFXFromBattleAnimationType(animationType);
					if (eSFXFromBattleAnimationType != 0)
					{
						PlayCue(eSFXFromBattleAnimationType, position);
					}
				}
				break;
			}
		}
		if (battleAnimation == null)
		{
			battleAnimation = BattleAnimation.Create(animationType, position, teamSide, isFacingRight, this, doesPlaySFX);
			battleAnimation.RefreshFacing(animationType, isFacingRight);
			if (!_cachedAnimations.ContainsKey((int)animationType))
			{
				_cachedAnimations[(int)animationType] = new List<BattleAnimation>();
			}
			_cachedAnimations[(int)animationType].Add(battleAnimation);
		}
		AddAnimation(battleAnimation);
	}

	public void AddNumber(int amount, Point targetPoint, ENumberColor numberColor)
	{
		_hudNumberQueue.Add(new HudNumber(amount, targetPoint, GCM, numberColor)
		{
			MovementType = ENumberMovementType.FloatUp
		});
	}

	public void AddItem(EItemType type, float amount, Point position, int newObjectID)
	{
		try
		{
			AddItem((type != EItemType.Money) ? new Item(this, position, type, amount, newObjectID) : new GemItem(this, position, amount, newObjectID));
		}
		catch
		{
		}
	}

	public void AddItem(Item item)
	{
		if (item != null)
		{
			if (item.ID == -1)
			{
				item.ID = NextObjectTicketID;
			}
			_items.Add(item.ID, item);
			item.Initialize();
		}
	}

	private void AddEnemy(Monster newMonster, int newId)
	{
		if (newMonster != null)
		{
			if (newId == -1)
			{
				newId = NextObjectTicketID;
				newMonster.ID = newId;
			}
			_enemies.Add(newId, newMonster);
		}
	}

	public void AddEvent(GameEvent newEvent)
	{
		if (newEvent != null)
		{
			if (newEvent.ID == -1)
			{
				newEvent.ID = NextObjectTicketID;
			}
			_levelEvents.Add(newEvent.ID, newEvent);
		}
	}

	public void AddFamiliar(FamiliarBase newFamiliar)
	{
		if (newFamiliar != null)
		{
			newFamiliar.ID = NextObjectTicketID;
			_protagonists.Add(newFamiliar.ID, newFamiliar);
		}
	}

	public void AddNPC(NPCBase newNPC)
	{
		_npcs.Add(newNPC.ID, newNPC);
	}

	private void AddNewDelayedObjects()
	{
		for (int num = _newObjects.Count - 1; num >= 0; num--)
		{
			Mobile mobile = _newObjects[num];
			int num2 = mobile.ID;
			if (num2 == -1)
			{
				num2 = (mobile.ID = ((mobile.BaseType == EGameObjectBaseType.Projectile) ? NextProjectileTicketID : NextObjectTicketID));
			}
			switch (mobile.BaseType)
			{
			case EGameObjectBaseType.Monster:
				if (mobile is Monster newMonster)
				{
					AddEnemy(newMonster, num2);
				}
				break;
			case EGameObjectBaseType.Event:
				if (mobile is GameEvent newEvent)
				{
					AddEvent(newEvent);
				}
				break;
			case EGameObjectBaseType.Item:
				if (mobile is Item item)
				{
					AddItem(item);
				}
				break;
			case EGameObjectBaseType.Projectile:
				if (mobile is Projectile newProjectile)
				{
					AddProjectile(newProjectile);
				}
				break;
			case EGameObjectBaseType.NPC:
				if (mobile is NPCBase newNPC)
				{
					AddNPC(newNPC);
				}
				break;
			case EGameObjectBaseType.Hero:
				if (mobile is FamiliarBase newFamiliar)
				{
					AddFamiliar(newFamiliar);
				}
				break;
			}
		}
		_newObjects.Clear();
	}

	private void RemoveDeadObjects()
	{
		for (int num = _deadObjects.Count - 1; num >= 0; num--)
		{
			Mobile mobile = _deadObjects[num];
			int iD = mobile.ID;
			switch (mobile.BaseType)
			{
			case EGameObjectBaseType.Monster:
				if (_enemies.ContainsKey(iD))
				{
					_enemies.Remove(iD);
					_objectTicketIDDispenser.Recycle(iD);
				}
				break;
			case EGameObjectBaseType.Projectile:
				if (_heroProjectiles.ContainsKey(iD) && _heroProjectiles[iD] == mobile)
				{
					_heroProjectiles.Remove(iD);
					_projectileTicketIDDispenser.Recycle(iD);
				}
				if (_enemyProjectiles.ContainsKey(iD))
				{
					_enemyProjectiles.Remove(iD);
					_projectileTicketIDDispenser.Recycle(iD);
				}
				break;
			case EGameObjectBaseType.Item:
				if (_items.ContainsKey(iD))
				{
					_items.Remove(iD);
					_objectTicketIDDispenser.Recycle(iD);
				}
				break;
			case EGameObjectBaseType.Event:
				if (_levelEvents.ContainsKey(iD))
				{
					_levelEvents.Remove(iD);
					_objectTicketIDDispenser.Recycle(iD);
				}
				break;
			case EGameObjectBaseType.NPC:
				if (_npcs.ContainsKey(iD))
				{
					_npcs.Remove(iD);
					_objectTicketIDDispenser.Recycle(iD);
				}
				break;
			case EGameObjectBaseType.Hero:
				if (_protagonists.ContainsKey(iD))
				{
					Protagonist protagonist = _protagonists[iD];
					if (protagonist is FamiliarBase)
					{
						_protagonists.Remove(iD);
					}
				}
				break;
			}
		}
		_deadObjects.Clear();
	}

	internal void RequestAddObject(Mobile newObject)
	{
		_newObjects.Add(newObject);
	}

	public void RequestRemoveObject(Mobile deadObject)
	{
		_deadObjects.Add(deadObject);
	}

	public void RemoveAnimation(BattleAnimation toDie)
	{
		if (_heroAnimations.Contains(toDie))
		{
			_heroAnimations.Remove(toDie);
		}
		if (_enemyAnimations.Contains(toDie))
		{
			_enemyAnimations.Remove(toDie);
		}
		if (_neutralAnimations.Contains(toDie))
		{
			_neutralAnimations.Remove(toDie);
		}
	}

	public void GiveExperience(int amount, int enemyID, Point position, bool isBossHit)
	{
		if (MainHero != null)
		{
			MainHero.GiveExperience(amount, enemyID, position, isBossHit);
		}
	}

	internal void FullyHealPlayer()
	{
		if (MainHero != null)
		{
			MainHero.FullyHeal();
		}
	}

	internal void UnlockRelic(EInventoryRelicType relicType)
	{
		if (GameSave != null)
		{
			GameSave.UnlockRelic(relicType);
			if (MainHero != null)
			{
				CharacterStats characterStats = GameSave.CharacterStats;
				characterStats.HP = MainHero.HP;
				characterStats.Sand = MainHero.MP;
				characterStats.Aura = MainHero.Aura;
				MainHero.RefreshStats(GameSave);
			}
		}
	}

	internal void PermanentlyFreezeTime()
	{
		if (!IsTimePermanentlyFrozen)
		{
			IsTimePermanentlyFrozen = true;
			FreezeTime(ETeamSide.Heroes, reFreeze: true);
		}
	}

	public void FreezeTime(ETeamSide team, bool reFreeze)
	{
		if (IsTimeFrozen && !reFreeze)
		{
			return;
		}
		IsTimeFrozen = true;
		_teamFreezing = team;
		if (!reFreeze)
		{
			HasPlayerFrozenTimeInThisRoom = true;
			_jukebox.PlayCue(ESFX.LunaisTimeStop);
		}
		if (team != ETeamSide.Heroes)
		{
			return;
		}
		foreach (Monster value in _enemies.Values)
		{
			value.Freeze();
		}
		foreach (NPCBase value2 in _npcs.Values)
		{
			value2.Freeze();
		}
		foreach (Projectile value3 in _enemyProjectiles.Values)
		{
			value3.Freeze();
		}
		foreach (BattleAnimation enemyAnimation in _enemyAnimations)
		{
			enemyAnimation.Freeze();
		}
		foreach (BattleAnimation neutralAnimation in _neutralAnimations)
		{
			neutralAnimation.Freeze();
		}
		foreach (Projectile value4 in _enemyProjectiles.Values)
		{
			value4.Freeze();
		}
		foreach (GameEvent value5 in _levelEvents.Values)
		{
			value5.Freeze();
		}
		foreach (Item value6 in _items.Values)
		{
			value6.Freeze();
		}
	}

	public void UnfreezeTime(ETeamSide team)
	{
		if (!IsTimeFrozen || IsTimePermanentlyFrozen)
		{
			return;
		}
		IsTimeFrozen = false;
		if (!IsUsingCutsceneSepiaEffect)
		{
			_jukebox.PlayCue(ESFX.LunaisTimeUnstop);
		}
		if (team != ETeamSide.Heroes)
		{
			return;
		}
		if (_protagonists.Count > 0)
		{
			MainHero.DoneCasting();
		}
		foreach (Monster value in _enemies.Values)
		{
			value.Unfreeze();
		}
		foreach (NPCBase value2 in _npcs.Values)
		{
			value2.Unfreeze();
		}
		foreach (Projectile value3 in _enemyProjectiles.Values)
		{
			value3.Unfreeze();
		}
		foreach (BattleAnimation enemyAnimation in _enemyAnimations)
		{
			enemyAnimation.Unfreeze();
		}
		foreach (BattleAnimation neutralAnimation in _neutralAnimations)
		{
			neutralAnimation.Unfreeze();
		}
		foreach (Projectile value4 in _enemyProjectiles.Values)
		{
			value4.Unfreeze();
		}
		foreach (GameEvent value5 in _levelEvents.Values)
		{
			value5.Unfreeze();
		}
		foreach (Item value6 in _items.Values)
		{
			value6.Unfreeze();
		}
	}

	public int NextRandomInt(int start, int end)
	{
		return _random.Next(start, end + 1);
	}

	public double NextRandomDouble()
	{
		return _random.NextDouble();
	}

	public NPCBase GetCharacterByType(NPCBase.ENPCType targetType)
	{
		NPCBase result = null;
		foreach (NPCBase value in _npcs.Values)
		{
			if (value.NPCType == targetType)
			{
				result = value;
				break;
			}
		}
		return result;
	}

	public IEnumerable<GameEvent> GetEventAllEventsOfType(EEventTileType targetType)
	{
		List<GameEvent> list = new List<GameEvent>();
		foreach (KeyValuePair<int, GameEvent> levelEvent in _levelEvents)
		{
			if (levelEvent.Value.EventType == targetType)
			{
				list.Add(levelEvent.Value);
			}
		}
		return list;
	}

	internal void TogglePlayerIsInvulnerable(bool isInvulnerable)
	{
		if (MainHero != null)
		{
			MainHero.ToggleInvulnerability(isInvulnerable);
		}
	}

	internal void PreventPlayerFromWarpingOut()
	{
		IsInBossRoom = true;
	}

	public void LockAllBossDoors(float waitTime)
	{
		IsInBossRoom = true;
		foreach (GameEvent value in _levelEvents.Values)
		{
			if ((value.EventType == EEventTileType.BossDoor || value.EventType == EEventTileType.MiniBossDoor) && value is SlidingDoorEvent slidingDoorEvent)
			{
				slidingDoorEvent.LockDoor(waitTime);
			}
		}
	}

	public void OpenAllBossDoors(float waitTime)
	{
		IsInBossRoom = false;
		foreach (GameEvent value in _levelEvents.Values)
		{
			if ((value.EventType == EEventTileType.BossDoor || value.EventType == EEventTileType.MiniBossDoor) && value is SlidingDoorEvent slidingDoorEvent)
			{
				slidingDoorEvent.OpenDoor(waitTime);
			}
		}
	}

	public void ToggleExits(bool isEnabled)
	{
		_areTeleportExitsUnlocked = isEnabled;
	}

	public void AddScript(ScriptAction script)
	{
		_waitingScripts.Enqueue(script);
	}

	public void AddScriptToPlayer1(ScriptAction script)
	{
		if (script != null)
		{
			script.TargetType = EScriptTargetType.Player1;
			AddScript(script);
		}
	}

	public ScriptAction ShowDialogueMessage(string inMessage, string inSource, bool inHideUI, DialogueBox.EDialogueBoxType dialogueType)
	{
		DialogueBox newDialogue = new DialogueBox(inMessage, inSource, inHideUI, _gcm, _jukebox, GameSave, dialogueType, PlayerControllerMapping);
		ScriptAction scriptAction = new ScriptAction(newDialogue);
		AddScript(scriptAction);
		return scriptAction;
	}

	public ScriptAction ShowDialogueMessage(string key)
	{
		StringInstance dialogue = Loc.GetDialogue(key);
		return ShowDialogueMessage(dialogue.Text, dialogue.Speaker, inHideUI: true, DialogueBox.EDialogueBoxType.Default);
	}

	public ScriptAction ShowGhostDialogueMessage(string key)
	{
		StringInstance dialogue = Loc.GetDialogue(key);
		return ShowDialogueMessage(dialogue.Text, dialogue.Speaker, inHideUI: true, DialogueBox.EDialogueBoxType.Ghost);
	}

	public ScriptAction ShowAutoplayGhostDialogueMessage(string key)
	{
		StringInstance dialogue = Loc.GetDialogue(key);
		return ShowDialogueMessage(dialogue.Text, dialogue.Speaker, inHideUI: true, DialogueBox.EDialogueBoxType.AutoplayGhost);
	}

	public ScriptAction ShowMenuDialogueMessage(string key)
	{
		ControllerMapping menuControllerMapping = ConfigSave.MenuControllerMapping;
		StringInstance dialogue = Loc.GetDialogue(key);
		DialogueBox newDialogue = new DialogueBox(dialogue.Text, dialogue.Speaker, inHideUI: true, _gcm, _jukebox, GameSave, DialogueBox.EDialogueBoxType.Default, menuControllerMapping);
		ScriptAction scriptAction = new ScriptAction(newDialogue);
		AddScript(scriptAction);
		return scriptAction;
	}

	public ScriptAction ShowControlAgnosticDialogueMessage(string key)
	{
		ControllerMapping controlAgnosticMapping = ControllerMapping.ControlAgnosticMapping;
		StringInstance dialogue = Loc.GetDialogue(key);
		DialogueBox newDialogue = new DialogueBox(dialogue.Text, dialogue.Speaker, inHideUI: true, _gcm, _jukebox, GameSave, DialogueBox.EDialogueBoxType.Default, controlAgnosticMapping);
		ScriptAction scriptAction = new ScriptAction(newDialogue);
		AddScript(scriptAction);
		return scriptAction;
	}

	public DialogueBox GetNextDialogue()
	{
		DialogueBox dialogueBox = _dialogueQueue.Dequeue();
		dialogueBox.RefreshSizes();
		if (IsNewDialogueAvailable)
		{
			dialogueBox.ShouldCloseOnEnd = false;
			DialogueBox dialogueBox2 = PeekNextDialogue();
			if (dialogueBox2 != null)
			{
				dialogueBox2.ShouldStartOpened = true;
			}
		}
		IsDialoguePlaying = true;
		return dialogueBox;
	}

	public DialogueBox PeekNextDialogue()
	{
		DialogueBox result = null;
		if (_dialogueQueue.Count > 0)
		{
			result = _dialogueQueue.Peek();
		}
		return result;
	}

	public void SetRenderEffectValues()
	{
		if (IsTimeFrozen)
		{
			_sepiaPercent = 0f;
			if (!IsTimePermanentlyFrozen)
			{
				if (!WasTimeFrozen)
				{
					_sepiaPercent = 0.7f;
				}
				else if (Heroes.Count > 0 && MainHero != null)
				{
					switch (MainHero.MP)
					{
					case 20:
						_sepiaPercent = 0.3f;
						break;
					case 15:
						_sepiaPercent = 0.4f;
						break;
					case 10:
						_sepiaPercent = 0.5f;
						break;
					case 6:
						_sepiaPercent = 0.6f;
						break;
					case 4:
						_sepiaPercent = 0.7f;
						break;
					case 2:
						_sepiaPercent = 0.8f;
						break;
					case 1:
						_sepiaPercent = 0.9f;
						break;
					}
				}
			}
		}
		else
		{
			_sepiaPercent = 1f;
		}
		if (IsUsingCutsceneSepiaEffect)
		{
			_sepiaPercent = CutsceneSepiaEffectPercentage;
		}
		GCM.EfSepiaTone.Parameters["sepiaAmount"].SetValue(_sepiaPercent);
		if (IsUsingGrayscaleEffect)
		{
			GCM.EfGrayscale.Parameters["screenFadePercent"].SetValue(GrayscaleFadePercentage);
		}
	}

	public Protagonist GetNearestProtagonist(Point inPosition)
	{
		return MainHero;
	}

	public Point GetNearestProtagonistPosition(Point inPosition)
	{
		return GetNearestProtagonist(inPosition)?.Bbox.Center ?? Point.Zero;
	}

	internal Point GetFamiliarPosition()
	{
		Point result = CameraPosition.ToPoint();
		foreach (KeyValuePair<int, Protagonist> protagonist in _protagonists)
		{
			if (protagonist.Value != MainHero)
			{
				result = protagonist.Value.Position;
				break;
			}
		}
		return result;
	}

	internal bool GetIsProtagonistInArea(Rectangle area)
	{
		bool result = false;
		foreach (Protagonist value in _protagonists.Values)
		{
			if (value.IsPlayable && value.IsPrimaryPlayer && value.Bbox.Intersects(area))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	internal Monster GetNearestVisibleAggroedEnemy(Point inPosition)
	{
		return GetNearestEnemy(inPosition, shouldBeVisible: true, shouldBeAggroed: true);
	}

	internal Monster GetNearestVisibleEnemy(Point inPosition)
	{
		return GetNearestEnemy(inPosition, shouldBeVisible: true, shouldBeAggroed: false);
	}

	internal Monster GetNearestEnemy(Point inPosition, bool shouldBeVisible, bool shouldBeAggroed)
	{
		Monster result = null;
		float num = float.MaxValue;
		int num2 = -1;
		foreach (KeyValuePair<int, Monster> enemy in _enemies)
		{
			Monster value = enemy.Value;
			Point position = value.Position;
			if (enemy.Value.HP > 0 && (!shouldBeVisible || VisibleArea.Contains(position) || VisibleArea.Contains(enemy.Value.OuterBbox.Center)) && (!shouldBeAggroed || (value.IsAggroed && !value.IsDormant)))
			{
				float num3 = (float)(Math.Pow(position.X - inPosition.X, 2.0) + Math.Pow(position.Y - inPosition.Y, 2.0));
				if (num3 < num)
				{
					num = num3;
					num2 = enemy.Key;
				}
			}
		}
		if (num2 >= 0 && _enemies.ContainsKey(num2))
		{
			result = _enemies[num2];
		}
		return result;
	}

	internal List<Monster> GetVisibleEnemies()
	{
		List<Monster> list = new List<Monster>();
		foreach (KeyValuePair<int, Monster> enemy in _enemies)
		{
			Point position = enemy.Value.Position;
			if (enemy.Value.HP > 0 && (VisibleArea.Contains(position) || VisibleArea.Contains(enemy.Value.OuterBbox.Center)))
			{
				list.Add(enemy.Value);
			}
		}
		return list;
	}

	internal IEnumerable<Monster> GetEnemiesWithinRadius(Circle circle)
	{
		List<Monster> list = new List<Monster>();
		foreach (KeyValuePair<int, Monster> enemy in _enemies)
		{
			if (enemy.Value.HP > 0 && circle.Intersects(enemy.Value.OuterBbox))
			{
				list.Add(enemy.Value);
			}
		}
		return list;
	}

	internal IEnumerable<Monster> GetEnemiesOfType(EEnemyTileType enemyType)
	{
		List<Monster> list = new List<Monster>();
		foreach (Monster value in _enemies.Values)
		{
			if (value.EnemyType == enemyType)
			{
				list.Add(value);
			}
		}
		return list;
	}

	internal Monster GetEnemyByID(int enemyID)
	{
		Monster result = null;
		if (_enemies.ContainsKey(enemyID))
		{
			result = _enemies[enemyID];
		}
		return result;
	}

	internal Tile GetNearestSolidTile(Point inPosition, EDirection direction, int maxTileDistance)
	{
		Tile result = null;
		Point point = new Point(inPosition.X / 16, inPosition.Y / 16);
		for (int i = 0; i < maxTileDistance; i++)
		{
			point = GetPointFromDirection(point, direction);
			if (_solidTiles.ContainsKey(point))
			{
				result = _solidTiles[point];
				break;
			}
		}
		return result;
	}

	internal IEnumerable<Projectile> GetCollidingProjectiles(Rectangle bbox, ETeamSide side)
	{
		List<Projectile> list = new List<Projectile>();
		switch (side)
		{
		case ETeamSide.Enemies:
			foreach (Projectile value in _enemyProjectiles.Values)
			{
				if (value.DoesCollideWith(bbox))
				{
					list.Add(value);
				}
			}
			break;
		case ETeamSide.Heroes:
			foreach (Projectile value2 in _heroProjectiles.Values)
			{
				if (value2.DoesCollideWith(bbox))
				{
					list.Add(value2);
				}
			}
			break;
		}
		return list;
	}

	public void SaveTheGame(int whichCheckpoint)
	{
		GameSave.CurrentCheckpoint = (byte)whichCheckpoint;
		GameSave.CurrentRoom = (byte)RoomID;
		GameSave.CurrentLevel = (byte)ID;
		GameSave.DoesNeedSave = true;
		GameSave.LevelSaveBools.Clear();
		GameSave.LevelSaveInts.Clear();
		foreach (string levelSaveBool in _levelSaveBools)
		{
			GameSave.LevelSaveBools.Add(levelSaveBool, value: true);
		}
		foreach (KeyValuePair<string, int> levelSaveInt in _levelSaveInts)
		{
			GameSave.LevelSaveInts.Add(levelSaveInt.Key, levelSaveInt.Value);
		}
	}

	internal void RequestScreenShake(Vector2 inDimensions, float inShakeTime, float inFrequency, bool isAffectedByTime)
	{
		if (_camera != null)
		{
			_camera.RequestScreenShake(inDimensions, inShakeTime, inFrequency, isAffectedByTime);
		}
	}

	public void RequestScreenFlash(float duration, float amplitude, float frequency)
	{
		RequestScreenFlash(new ScreenFlash(duration)
		{
			Amplitude = amplitude,
			Frequency = frequency
		});
	}

	public void RequestScreenFlash(ScreenFlash screenFlash)
	{
		_screenEffectQueue.Enqueue(screenFlash);
	}

	internal void RequestScreenFadeOut(float duration)
	{
		_screenEffectQueue.Enqueue(new ScreenFade(duration));
	}

	internal void RequestScreenFadeOut(float fadeOutTime, float blackTime, float fadeInTime, float fadeColor)
	{
		ScreenFade screenFade = new ScreenFade(fadeOutTime, blackTime, fadeInTime);
		if (fadeColor > 0f)
		{
			screenFade.EffectColor = new Color(fadeColor, fadeColor, fadeColor, 1f);
		}
		_screenEffectQueue.Enqueue(screenFade);
	}

	public void RequestButtonPrompt(int buttonIndex, Point location)
	{
		if (!IsUIRequestingHide)
		{
			IsButtonPromptRequested = true;
			ButtonTutorialDisplayIndex = buttonIndex;
			ButtonPromptPosition = location;
		}
	}

	internal void RequestItemGetPopup(EInventoryUseItemType itemType)
	{
		IsRequestingItemGetPopup = true;
		ItemGetCategory = EInventoryCategoryType.UseItem;
		ItemGetValue = (int)itemType;
	}

	internal void RequestItemGetPopup(EInventoryEquipmentType equipmentType)
	{
		IsRequestingItemGetPopup = true;
		ItemGetCategory = EInventoryCategoryType.Equipment;
		ItemGetValue = (int)equipmentType;
	}

	internal void RequestItemGetPopup(EInventoryJournalType journalType)
	{
		IsRequestingItemGetPopup = true;
		ItemGetCategory = EInventoryCategoryType.Journal;
		ItemGetValue = (int)journalType;
	}

	internal void RequestToastPopup(EToastType type, int argument)
	{
		IsRequestingToast = true;
		RequestedToastType = type;
		RequestedToastArgument = argument;
	}

	public bool IsOutsideVisibleArea(Point position)
	{
		if (position.X >= VisibleArea.Left && position.X <= VisibleArea.Right && position.Y <= VisibleArea.Bottom)
		{
			return position.Y < VisibleArea.Top;
		}
		return true;
	}

	public bool IsOutsideProjectileVisibleArea(Point position)
	{
		if (position.X >= _projectileVisibleArea.Left && position.X <= _projectileVisibleArea.Right && position.Y <= _projectileVisibleArea.Bottom)
		{
			return position.Y < _projectileVisibleArea.Top;
		}
		return true;
	}

	internal bool IsWithinCameraDistance(Point position)
	{
		if (Math.Abs((float)position.X - CameraPosition.X) < (float)GameplayScreen.SmallScreenSize.X)
		{
			return Math.Abs((float)position.Y - CameraPosition.Y) < (float)GameplayScreen.SmallScreenSize.Y;
		}
		return false;
	}

	public void SetPlayerPosition(Point levelCoordinates)
	{
		if (MainHero != null)
		{
			MainHero.Position = levelCoordinates;
		}
	}

	public void RevivePlayer()
	{
		if (MainHero != null)
		{
			MainHero.Revive();
		}
	}

	public Point GetPlayerPosition()
	{
		Point result = Point.Zero;
		if (MainHero != null)
		{
			result = MainHero.Position;
		}
		return result;
	}

	public int GetNextRoomID(int indexModifier)
	{
		int num = RoomIndex + indexModifier;
		if (num < 0)
		{
			num = 0;
		}
		if (num < _levelSpecification.Rooms.Count)
		{
			RoomSpecification roomSpecification = _levelSpecification.Rooms[num];
			num = roomSpecification.ID;
		}
		return num;
	}

	public SFXCueInstance PlayCue(ESFX cue)
	{
		return _jukebox.CreateCue(cue, isLooped: false, shouldPlay: true, Point.Zero, null);
	}

	public SFXCueInstance PlayCue(ESFX cue, Point position)
	{
		return PlayCue(cue, position, isLooped: false);
	}

	public SFXCueInstance PlayCue(ESFX cue, Point position, bool isLooped)
	{
		SFXCueInstance result = null;
		if (isLooped || IsWithinCameraDistance(position))
		{
			result = _jukebox.CreateCue(cue, isLooped, shouldPlay: true, position, _camera);
		}
		return result;
	}

	public SFXCueInstance CreateCue(ESFX cue, Point position, bool isLooped)
	{
		return _jukebox.CreateCue(cue, isLooped, shouldPlay: false, position, _camera);
	}

	public SFXCueInstance CreateCue2D(ESFX cue, bool isLooped)
	{
		return _jukebox.CreateCue(cue, isLooped, shouldPlay: false, Point.Zero, null);
	}

	public void AddKill()
	{
		GameSave.Kills++;
	}

	public IEnumerable<BackgroundSpecification> GetWarpBackgroundsForLevelID(int levelID)
	{
		if (KnownWarpBackgrounds == null || !KnownWarpBackgrounds.ContainsKey(levelID))
		{
			return LevelSpecification.WarpBackgrounds;
		}
		return KnownWarpBackgrounds[levelID];
	}

	public void AddScreen(GameScreen newScreen)
	{
		_screenAddQueue.Enqueue(newScreen);
	}

	internal bool GetLevelSaveBool(string key)
	{
		return _levelSaveBools.Contains(key);
	}

	internal int GetLevelSaveInt(string key)
	{
		if (!_levelSaveInts.ContainsKey(key))
		{
			return 0;
		}
		return _levelSaveInts[key];
	}

	internal void SetLevelSaveBool(string key, bool value)
	{
		if (_levelSaveBools.Contains(key))
		{
			if (!value)
			{
				_levelSaveBools.Remove(key);
			}
		}
		else if (value)
		{
			_levelSaveBools.Add(key);
		}
	}

	internal void SetLevelSaveInt(string key, int value)
	{
		if (_levelSaveInts.ContainsKey(key))
		{
			if (value == 0)
			{
				_levelSaveInts.Remove(key);
			}
			else
			{
				_levelSaveInts[key] = value;
			}
		}
		else
		{
			_levelSaveInts[key] = value;
		}
	}

	internal void ClearLevelSaveData()
	{
		_levelSaveBools.Clear();
		_levelSaveInts.Clear();
	}

	internal void SetIsOverridingPowerOff(bool newValue)
	{
		_isOverridingPowerOff = newValue;
	}

	public CharacterSpecification GetCharacterSpecification(ObjectTileSpecification objectSpec)
	{
		CharacterSpecification result = null;
		string key = CharacterSpecification.KeyFromObjectSpecification(objectSpec);
		if (GCM.CharacterDatabase != null && GCM.CharacterDatabase.CharacterSpecifications.ContainsKey(key))
		{
			result = GCM.CharacterDatabase.CharacterSpecifications[key];
		}
		return result;
	}

	public int GetWaterTopFromBelowWater(Point position)
	{
		int x = position.X / 16;
		int num = position.Y / 16;
		int num2 = 0;
		while (num >= 0 && WaterTiles.ContainsKey(new Point(x, num)))
		{
			num2 = num;
			num--;
		}
		return num2 * 16 + 5;
	}

	internal int GetWaterTopFromAboveWater(Point position)
	{
		int x = position.X / 16;
		int i = position.Y / 16;
		int y = RoomSize16.Y;
		int result = y * 16;
		for (; i < y; i++)
		{
			if (WaterTiles.ContainsKey(new Point(x, i)))
			{
				result = i * 16 + 5;
				break;
			}
		}
		return result;
	}

	public int GetFloorY(Point position)
	{
		int x = position.X / 16;
		int i = position.Y / 16;
		int y = RoomSize16.Y;
		int result = -1;
		for (; i < y; i++)
		{
			if (WaterTiles.ContainsKey(new Point(x, i)))
			{
				result = i * 16 + 5;
				break;
			}
			if (SolidTiles.ContainsKey(new Point(x, i)))
			{
				result = i * 16;
				break;
			}
		}
		return result;
	}

	internal void StartPlayerDeathCutscene()
	{
		ForceFreezeEvents();
		_heroAnimations.Clear();
		_neutralAnimations.Clear();
		_enemyAnimations.Clear();
		_items.Clear();
		LevelDrawColor = Color.White;
		IsDoingPlayerDeathCutscene = true;
		IsPlayerInputBlocked = true;
		RequestScreenFlash(new ScreenFlash(0.1f)
		{
			Frequency = 2f
		});
		FreezeTime(ETeamSide.Heroes, reFreeze: true);
		IsUsingGrayscaleEffect = true;
		IsUIRequestingHide = true;
		JukeBox.FadeOutAllSFX(1f);
		JukeBox.FadeOutSong(1f);
	}

	internal void ForceFreezeEvents()
	{
		foreach (GameEvent value in _levelEvents.Values)
		{
			value.IsAffectedByTime = true;
			value.Freeze();
		}
	}

	internal void SkipCutscene()
	{
		Console.WriteLine($"[Level SkipCutscene] Entered. _isCutsceneSkipFading={_isCutsceneSkipFading} activeScripts={_activeScripts.Count} waitingScripts={_waitingScripts.Count}");
		if (_isCutsceneSkipFading)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		foreach (ScriptAction activeScript in _activeScripts)
		{
			if (activeScript != null)
			{
				if (activeScript.ScriptType == EScriptType.CutsceneStart && activeScript.Arguments.X >= 1f)
				{
					flag = true;
				}
				if (activeScript.IsUnskippable)
				{
					flag2 = true;
				}
			}
		}
		if (!flag2)
		{
			if (flag)
			{
				_isCutsceneSkipFading = true;
				_cutsceneSkipFadeTimer = 0.25f;
				InsertScript(new ScriptAction
				{
					ScriptType = EScriptType.FadeInFadeOut,
					Arguments = new Vector4(0.25f, 0.5f, 0.25f, 0f)
				});
			}
			else
			{
				_isSkippingCutscene = true;
			}
		}
	}

	internal void ShowGameOverScreen()
	{
		IsRequestingGameOverScreen = true;
	}

	internal void ActivateFamiliar(PlayerIndex playerIndex)
	{
		if (MainHero != null && MainHero.IsPrimaryPlayer && MainHero is LunaisObj lunaisObj)
		{
			lunaisObj.FamiliarManager.ActivateFamiliar(playerIndex);
		}
	}

	internal void RefreshProtagonistControls(GameConfigSave config)
	{
		foreach (Protagonist value in _protagonists.Values)
		{
			value.RefreshControls(config);
		}
	}

	internal static EEraType GetEraByLevelID(int id)
	{
		EEraType result = EEraType.Unknown;
		if (id >= 0 && id < 3)
		{
			result = EEraType.Present;
		}
		else if (id >= 3 && id < 9)
		{
			result = EEraType.Past;
		}
		else if (id >= 9 && id < 13)
		{
			result = EEraType.Present;
		}
		return result;
	}

	internal static string GetLevelNameFromID(int levelID)
	{
		return GetLevelNameFromID(levelID, -1);
	}

	internal static string GetLevelNameFromID(int levelID, int roomID)
	{
		return levelID switch
		{
			-1 => Loc.Get("LevelDebug"), 
			0 => Loc.Get("LevelWinderia"), 
			1 => Loc.Get("LevelLakeF"), 
			2 => Loc.Get((roomID == 54) ? "LevelMetropolisLibrary" : "LevelMetropolis"), 
			3 => Loc.Get("LevelForest"), 
			4 => Loc.Get("LevelRamparts"), 
			5 => Loc.Get("LevelInnerCastle"), 
			6 => Loc.Get("LevelCastleTower"), 
			7 => Loc.Get("LevelLakeP"), 
			8 => Loc.Get("LevelCavesP"), 
			9 => Loc.Get("LevelCavesF"), 
			10 => Loc.Get("LevelFortressEntrance"), 
			11 => Loc.Get("LevelLaboratory"), 
			12 => Loc.Get("LevelThroneRoom"), 
			13 => Loc.Get("LevelWinderiaBridge"), 
			14 => Loc.Get("LevelNexus"), 
			15 => Loc.Get("LevelWinderiaForest"), 
			16 => Loc.Get("LevelWinderiaTemple"), 
			17 => Loc.Get("LevelNexus"), 
			_ => "Level " + levelID, 
		};
	}
}
