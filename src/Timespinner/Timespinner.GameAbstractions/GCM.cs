using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.Core.Localization;
using Timespinner.Core.Specifications;

namespace Timespinner.GameAbstractions;

public class GCM
{
	private const int DefaultFontLineSpacing = 16;

	private const int LevelRenderTargetSizeX = 512;

	private const int LevelRenderTargetSizeY = 256;

	private const int MaxBorderFrameWidth = 40;

	private const int MaxBorderFrameHeight = 256;

	private const int BorderFramePieceWidth = 20;

	private const int BorderFramePieceHeight = 16;

	private const int TotalBorderHeight = 272;

	private const int HalfTotalBorderHeight = 136;

	private readonly BestiarySpecification _bestiary;

	private readonly TextureAtlasDatabase _textureAtlasDatabase;

	private bool _areShadersLoaded;

	private bool _areSpritesLoaded;

	private bool _areTilesetsLoaded;

	private bool _areFXLoaded;

	private bool _areEventsLoaded;

	private bool _areBackgroundsLoaded;

	private bool _areUIElementLoaded;

	private Point _displaySize;

	private Vector2 _screenCenter;

	private SpriteFont _latinFont;

	private GraphicsDevice _graphicsDevice;

	private ContentManager _generalContentManager;

	public Effect EfSepiaTone;

	public Effect EfGrayscale;

	public Effect EfBrighten;

	public Effect EfDrawUnder;

	public Effect EfSlidingGradient;

	public Effect EfEnergyPulse;

	public Effect EfTrigDeform;

	public Effect EfSandDraw;

	public Effect EfSandTrigDeform;

	public Effect EfScrollingDeform;

	public Effect EfCampfire;

	public SpriteFont ActiveFont;

	public SpriteSheet SpLunais;

	public SpriteSheet SpAltLunais;

	public SpriteSheet SpAltLunais2;

	public SpriteSheet SpMerchantCrow;

	public SpriteSheet SpSelen;

	public SpriteSheet SpWinderians;

	public SpriteSheet SpForestNPCs;

	public SpriteSheet SpFamiliarMeyef;

	public SpriteSheet SpFamiliarCrow;

	public SpriteSheet SpFamiliarGriffin;

	public SpriteSheet SpFamiliarKobo;

	public SpriteSheet SpFamiliarSprite;

	public SpriteSheet SpFamiliarDemon;

	public SpriteSheet SpFamiliarAltMeyef;

	public SpriteSheet SpFamiliarAltCrow;

	public SpriteSheet SpCheveuxFlying;

	public SpriteSheet SpRobotJunk;

	public SpriteSheet SpSiren;

	public SpriteSheet SpCastleShieldKnight;

	public SpriteSheet SpCopperWyvern;

	public SpriteSheet SpCheveuxTank;

	public SpriteSheet SpWormFlower;

	public SpriteSheet SpCeilingStar;

	public SpriteSheet SpFleshSpider;

	public SpriteSheet SpDiscStatue;

	public SpriteSheet SpCitySecurityGuard;

	public SpriteSheet SpForestBabyCheveux;

	public SpriteSheet SpForestMoth;

	public SpriteSheet SpForestPlantBat;

	public SpriteSheet SpForestRodent;

	public SpriteSheet SpForestWormFlower;

	public SpriteSheet SpCavesMushroomTower;

	public SpriteSheet SpCavesSnail;

	public SpriteSheet SpCavesSporeVine;

	public SpriteSheet SpCavesSlime;

	public SpriteSheet SpCastleArcher;

	public SpriteSheet SpCastleLargeSoldier;

	public SpriteSheet SpCastleEngineer;

	public SpriteSheet SpKeepWarCheveux;

	public SpriteSheet SpKeepAristocrat;

	public SpriteSheet SpKeepDemon;

	public SpriteSheet SpTowerIceMage;

	public SpriteSheet SpTowerPlasmaPod;

	public SpriteSheet SpTowerDemonMage;

	public SpriteSheet SpLakeAnemone;

	public SpriteSheet SpLakeBirdEgg;

	public SpriteSheet SpLakeCheveux;

	public SpriteSheet SpLakeEel;

	public SpriteSheet SpLakeFly;

	public SpriteSheet SpFortressKnight;

	public SpriteSheet SpFortressGunner;

	public SpriteSheet SpFortressLargeSoldier;

	public SpriteSheet SpFortressEngineer;

	public SpriteSheet SpLabTurret;

	public SpriteSheet SpLabChild;

	public SpriteSheet SpLabAdult;

	public SpriteSheet SpLabSpider;

	public SpriteSheet SpCursedCopperWyvern;

	public SpriteSheet SpCursedSiren;

	public SpriteSheet SpCursedMushroomTower;

	public SpriteSheet SpCursedSporeVine;

	public SpriteSheet SpCursedSnail;

	public SpriteSheet SpCursedAnemone;

	public SpriteSheet SpCursedMoth;

	public SpriteSheet SpEmpDemon;

	public SpriteSheet SpEmpRoyalGuard;

	public SpriteSheet SpEmpAristocrat;

	public SpriteSheet SpTempleConviction;

	public SpriteSheet SpGyreMajorUgly;

	public SpriteSheet SpGyreMeteorSparrow;

	public SpriteSheet SpGyreKain;

	public SpriteSheet SpGyreNethershade;

	public SpriteSheet SpGyreRyshia;

	public SpriteSheet SpGyreZel;

	public SpriteSheet SpBirdBoss;

	public SpriteSheet SpRoboKitty;

	public SpriteSheet SpVarndagroth;

	public SpriteSheet SpAelana;

	public SpriteSheet SpDemonBoss;

	public SpriteSheet SpMawBoss;

	public SpriteSheet SpShapeshifter;

	public SpriteSheet SpEmperor;

	public SpriteSheet SpEmperorVilete;

	public SpriteSheet SpEmperorWinderia;

	public SpriteSheet SpSandmanBoss;

	public SpriteSheet SpNightmareBoss;

	public SpriteSheet SpRavenBoss;

	public SpriteSheet SpXarionBoss;

	public SpriteSheet SpZelBoss;

	public SpriteSheet SpCantoranBoss;

	public SpriteSheet SpOrbMeleeBlue;

	public SpriteSheet SpMiscProjectiles;

	public SpriteSheet SpOrbMeleeBlade;

	public SpriteSheet SpOrbMeleeFire;

	public SpriteSheet SpOrbMeleeIron;

	public SpriteSheet SpOrbPlasma;

	public SpriteSheet SpOrbMeleeIce;

	public SpriteSheet SpOrbMeleeWind;

	public SpriteSheet SpOrbMeleeUmbra;

	public SpriteSheet SpOrbMeleeEmpire;

	public SpriteSheet SpOrbMeleeEye;

	public SpriteSheet SpOrbMeleeBlood;

	public SpriteSheet SpOrbMeleeGun;

	public SpriteSheet SpOrbMeleeMoon;

	public SpriteSheet SpOrbMeleeBarrier;

	public SpriteSheet SpOrbMeleeNether;

	public SpriteSheet SpOrbMeleeBook;

	public SpriteSheet SpStatusEffects;

	public SpriteSheet SpItems;

	public SpriteSheet SpMenuIcons;

	public SpriteSheet SpSaveStatue;

	public SpriteSheet SpSlidingDoors;

	public SpriteSheet SpKeycardDoor;

	public SpriteSheet SpPlatforms;

	public SpriteSheet SpTheTimespinner;

	public SpriteSheet SpCityTimespinner;

	public SpriteSheet SpTreasureChests;

	public SpriteSheet SpMiscCurtain;

	public SpriteSheet SpMiscHangar;

	public SpriteSheet SpMiscHangar2;

	public SpriteSheet SpMiscLab;

	public SpriteSheet SpLanterns;

	public SpriteSheet SpPetrifiedVines;

	public SpriteSheet SpOrbPedestal;

	public SpriteSheet SpBackerPortraits;

	public SpriteSheet SpBrokenGlass;

	public SpriteSheet TsEventTiles;

	public SpriteSheet TsL1Tileset;

	public SpriteSheet TsL2Tileset;

	public SpriteSheet TsL5Tileset;

	public SpriteSheet TsL7Tileset;

	public SpriteSheet TsCavesTileset;

	public SpriteSheet TsL10Tileset;

	public SpriteSheet TsNexusTileset;

	public SpriteSheet TsCityTileset;

	public SpriteSheet TsForestTileset;

	public SpriteSheet TsForestPastTileset;

	public SpriteSheet TsRuinedTempleTileset;

	public SpriteSheet TsTransitionsTileset;

	public SpriteSheet TsCurtainTileset;

	public SpriteSheet TsTowerTileset;

	public SpriteSheet TsCaves2Tileset;

	public SpriteSheet TsCursedCavesTileset;

	public SpriteSheet TsCursedCaves2Tileset;

	public SpriteSheet TsLabTileset;

	public SpriteSheet TsEmperorTowerTileset;

	public SpriteSheet TsTempleTileset;

	public SpriteSheet TsEndingTileset;

	public SpriteSheet BgSpaceTiled;

	public SpriteSheet BgL1Backdrop1;

	public SpriteSheet BgL1Backdrop2;

	public SpriteSheet BgL1Backdrop3;

	public SpriteSheet BgL1Backdrop4;

	public SpriteSheet BgL1NearBackdrop;

	public SpriteSheet BgL2Fountain;

	public SpriteSheet BgL6Backdrop1;

	public SpriteSheet BgL7Backdrop1;

	public SpriteSheet BgL7Backdrop2;

	public SpriteSheet BgL7Backdrop3;

	public SpriteSheet BgL7Backdrop4;

	public SpriteSheet BgL7NearBackdrop;

	public SpriteSheet BgCaveBackdropWaterfall1;

	public SpriteSheet BgCaveBackdrops1;

	public SpriteSheet BgCaveBackdrops2;

	public SpriteSheet BgCityBackdrops1;

	public SpriteSheet BgCityBackdrops2;

	public SpriteSheet BgCityBackdrops3;

	public SpriteSheet BgCityBackdrops4;

	public SpriteSheet BgCityBackdrops5;

	public SpriteSheet BgCursedCaveBackdropWaterfall1;

	public SpriteSheet BgCursedCaveBackdrops1;

	public SpriteSheet BgCursedCaveBackdrops2;

	public SpriteSheet BgCursedCaveBackdrops3;

	public SpriteSheet BgCurtainsBackdrops1;

	public SpriteSheet BgCurtainsBackdrops2;

	public SpriteSheet BgCurtainsBackdrops3;

	public SpriteSheet BgKeepBackdrops1;

	public SpriteSheet BgKeepBackdrops2;

	public SpriteSheet BgTowerBackdrops1;

	public SpriteSheet BgTowerBackdrops2;

	public SpriteSheet BgTowerBackdrops3;

	public SpriteSheet BgTowerBackdrops4;

	public SpriteSheet BgForestTreesFarthest;

	public SpriteSheet BgForestTreesFar;

	public SpriteSheet BgForestTreesMid;

	public SpriteSheet BgForestTreesNear;

	public SpriteSheet BgForestRocksFar;

	public SpriteSheet BgForestRocksMid;

	public SpriteSheet BgForestRocksNear;

	public SpriteSheet BgForestTemple1;

	public SpriteSheet BgForestTemple2;

	public SpriteSheet BgForestWaterfall1;

	public SpriteSheet BgForestWaterfall2;

	public SpriteSheet BgForestWaterfall3;

	public SpriteSheet BgForestWaterfall4;

	public SpriteSheet BgHangarBackdrops1;

	public SpriteSheet BgHangarBackdrops2;

	public SpriteSheet BgHangarBackdrops3;

	public SpriteSheet BgLabBackdrop1;

	public SpriteSheet BgEmperorTowerBackdrops1;

	public SpriteSheet BgEmperorTowerBackdrops2;

	public SpriteSheet BgEmperorTowerBackdrops3;

	public SpriteSheet BgEmperorTowerBackdrops5;

	public SpriteSheet BgTempleBackdrop1;

	public SpriteSheet BgVileteArchways;

	public SpriteSheet BgVileteBackdrop;

	public SpriteSheet BgVileteBackdrop2;

	public SpriteSheet BgWinderiaBackdrop1;

	public SpriteSheet BgWinderiaBackdrop2;

	public SpriteSheet BgWinderiaBackdrop3;

	public SpriteSheet BgWinderiaBackdrop4;

	public SpriteSheet BgWinderiaBackdrop5;

	public SpriteSheet BgWinderiaBackdrop6;

	public SpriteSheet BgTimeBackdrop1;

	public SpriteSheet BgTimeBackdrop2;

	public SpriteSheet BgTimeBackdrop3;

	public SpriteSheet BgGyreBackdrops1;

	public SpriteSheet BgGyreBackdrops2;

	public SpriteSheet BgGyreBackdrops3;

	public SpriteSheet BgGyreBackdrops4;

	public SpriteSheet BgEndingBackdrops1;

	public SpriteSheet BgFog;

	public SpriteSheet BgHalfFog;

	public SpriteSheet BgClouds;

	public SpriteSheet SpBoomAnimation;

	public SpriteSheet SpEffectsSmall;

	public SpriteSheet SpEffectsMedium;

	public SpriteSheet SpEffectsLarge;

	public SpriteSheet SpTimeGateAnimation;

	public SpriteSheet SpAuraEffects;

	public SpriteSheet SpSoulStream;

	public SpriteSheet SpSandTexture;

	public SpriteSheet SpDifferenceCloud;

	public Texture2D TxBlankSquare;

	public Texture2D TxLargeCircle;

	public Texture2D TxLargeRing;

	public Texture2D TxParticleEnergy;

	public Texture2D TxParticleDust;

	public Texture2D TxParticleSmoke;

	public SpriteSheet SpAnimatedParticlesSmall;

	public Texture2D TxUIBlackGradient;

	public SpriteSheet SpLunaisHUD;

	public SpriteSheet SpNumbers;

	public SpriteSheet SpMiniMap;

	public SpriteSheet SpTextBox;

	public SpriteSheet SpPauseMenu;

	public SpriteSheet SpJournalMenu;

	public SpriteSheet SpMenuCursor;

	public SpriteSheet SpMenuCharacters;

	public SpriteSheet SpPortraits;

	public SpriteSheet SpPortraits2;

	public SpriteSheet SpBossPortraits;

	public SpriteSheet SpUIButtons;

	public SpriteSheet SpLevelUp;

	public SpriteSheet SpSmoothCircles;

	public SpriteSheet SpLoadingScreen;

	public SpriteSheet SpBorderFrame;

	public SpriteSheet SpToolsLogo;

	public SpriteSheet SpAreaTitles;

	public SpriteSheet SpLocToastsLatin;

	public SpriteSheet SpLocToastsAsian;

	public Texture2D TxSelectSquare;

	public SpriteSheet TsObjectsTileset;

	public Point DisplaySize => _displaySize;

	public Vector2 ScreenCenter => _screenCenter;

	public Rectangle TitleSafeArea { get; private set; }

	public BestiarySpecification Bestiary => _bestiary;

	public CharacterSpecificationDatabase CharacterDatabase { get; set; }

	public RenderTarget2D TemporaryLevelRenderTarget { get; private set; }

	public RenderTarget2D LevelRenderTarget { get; private set; }

	public SpriteFont LatinFont => _latinFont;

	public GCM()
	{
		_bestiary = BestiarySpecification.FromCompressedFile("./Content/Bestiary.dat");
		_textureAtlasDatabase = TextureAtlasDatabase.FromCompressedFile("./Content/TextureDatabase.dat");
		CharacterDatabase = CharacterSpecificationDatabase.FromCompressedFile("./Content/CharacterDatabase.dat");
	}

	public void LoadAllResources(ContentManager content, GraphicsDevice gDevice)
	{
		PrepareLoad(content, gDevice);
		LoadShaders(content);
		LoadSprites();
		LoadTilesets(content);
		LoadEvents(content);
		LoadFX(content);
		LoadBackgrounds(content);
		LoadUI(content);
	}

	internal void Dispose()
	{
		if (LevelRenderTarget != null && !LevelRenderTarget.IsDisposed)
		{
			LevelRenderTarget.Dispose();
		}
		if (TemporaryLevelRenderTarget != null && !TemporaryLevelRenderTarget.IsDisposed)
		{
			TemporaryLevelRenderTarget.Dispose();
		}
	}

	internal void PrepareLoad(ContentManager content, GraphicsDevice gDevice)
	{
		_generalContentManager = content;
		_graphicsDevice = gDevice;
		PresentationParameters presentationParameters = _graphicsDevice.PresentationParameters;
		if (TemporaryLevelRenderTarget == null || TemporaryLevelRenderTarget.IsDisposed)
		{
			TemporaryLevelRenderTarget = new RenderTarget2D(_graphicsDevice, 512, 256, mipMap: true, presentationParameters.BackBufferFormat, presentationParameters.DepthStencilFormat, 0, RenderTargetUsage.DiscardContents);
		}
		if (LevelRenderTarget == null || LevelRenderTarget.IsDisposed)
		{
			LevelRenderTarget = new RenderTarget2D(_graphicsDevice, 512, 256, mipMap: true, presentationParameters.BackBufferFormat, presentationParameters.DepthStencilFormat, 0, RenderTargetUsage.PreserveContents);
		}
	}

	public void SetScreenSizeAndTitleSafeArea(Point displaySize, Rectangle titleSafeArea)
	{
		_displaySize = displaySize;
		_screenCenter = new Vector2((float)_displaySize.X / 2f, (float)_displaySize.Y / 2f);
		TitleSafeArea = titleSafeArea;
	}

	private SpriteSheet Get(string contentPath)
	{
		return _textureAtlasDatabase.LoadTextureAtlas(contentPath, _generalContentManager);
	}

	private SpriteSheet Get(string contentPath, ContentManager content)
	{
		return _textureAtlasDatabase.LoadTextureAtlas(contentPath, content);
	}

	internal SpriteSheet GetTextureAtlas(string contentPath, ContentManager content)
	{
		return _textureAtlasDatabase.LoadTextureAtlas(contentPath, content);
	}

	internal void LoadShaders(ContentManager content)
	{
		if (!_areShadersLoaded)
		{
			EfBrighten = content.Load<Effect>("Effects/Brighten");
			EfGrayscale = content.Load<Effect>("Effects/Grayscale");
			EfSepiaTone = content.Load<Effect>("Effects/SepiaTone");
			EfDrawUnder = content.Load<Effect>("Effects/DrawUnder");
			EfSlidingGradient = content.Load<Effect>("Effects/SlidingGradient");
			EfEnergyPulse = content.Load<Effect>("Effects/EnergyPulse");
			EfTrigDeform = content.Load<Effect>("Effects/TrigDeform");
			EfSandDraw = content.Load<Effect>("Effects/SandDraw");
			EfSandTrigDeform = content.Load<Effect>("Effects/SandTrigDeform");
			EfScrollingDeform = content.Load<Effect>("Effects/ScrollingDeform");
			EfCampfire = content.Load<Effect>("Effects/Campfire");
			_areShadersLoaded = true;
		}
	}

	public void LoadSpritesStage(int stage)
	{
		if (_areSpritesLoaded)
		{
			return;
		}
		switch (stage)
		{
		case 0:
			SpLunais = Get("Sprites/Heroes/LunaisSprite");
			SpAltLunais = Get("Sprites/Heroes/LunaisAltSprite");
			SpAltLunais2 = Get("Sprites/Heroes/LunaisAltSprite2");
			SpMerchantCrow = Get("Sprites/Heroes/Merchant");
			SpSelen = Get("Sprites/Heroes/Selen");
			SpWinderians = Get("Sprites/Heroes/Winderians");
			SpForestNPCs = Get("Sprites/Heroes/NPCs");
			SpFamiliarMeyef = Get("Sprites/Heroes/FamiliarMeyef");
			SpFamiliarCrow = Get("Sprites/Heroes/FamiliarCrow");
			SpFamiliarGriffin = Get("Sprites/Heroes/FamiliarGriffin");
			SpFamiliarKobo = Get("Sprites/Heroes/FamiliarKobo");
			SpFamiliarSprite = Get("Sprites/Heroes/FamiliarSprite");
			SpFamiliarDemon = Get("Sprites/Heroes/FamiliarDemon");
			SpFamiliarAltMeyef = Get("Sprites/Heroes/FamiliarAltMeyef");
			SpFamiliarAltCrow = Get("Sprites/Heroes/FamiliarAltCrow");
			SpCheveuxFlying = Get("Sprites/Enemies/CheveuxFlying");
			SpRobotJunk = Get("Sprites/Enemies/Junk");
			SpCopperWyvern = Get("Sprites/Enemies/CavesCopperWyvern");
			SpCheveuxTank = Get("Sprites/Enemies/CheveuxTank");
			SpCeilingStar = Get("Sprites/Enemies/CeilingStar");
			SpWormFlower = Get("Sprites/Enemies/WormFlower");
			SpFleshSpider = Get("Sprites/Enemies/FleshSpider");
			SpDiscStatue = Get("Sprites/Enemies/DiscStatue");
			SpCitySecurityGuard = Get("Sprites/Enemies/CitySecurityGuard");
			SpForestBabyCheveux = Get("Sprites/Enemies/ForestBabyCheveux");
			SpForestMoth = Get("Sprites/Enemies/ForestMoth");
			SpForestPlantBat = Get("Sprites/Enemies/ForestPlantBat");
			SpForestRodent = Get("Sprites/Enemies/ForestRodent");
			SpForestWormFlower = Get("Sprites/Enemies/ForestWormFlower");
			SpCavesMushroomTower = Get("Sprites/Enemies/CavesMushroomTower");
			break;
		case 1:
			SpCavesSnail = Get("Sprites/Enemies/CavesSnail");
			SpCavesSporeVine = Get("Sprites/Enemies/CavesSporeVine");
			SpSiren = Get("Sprites/Enemies/CavesSiren");
			SpCavesSlime = Get("Sprites/Enemies/CavesSlime");
			SpCastleArcher = Get("Sprites/Enemies/CastleArcher");
			SpCastleShieldKnight = Get("Sprites/Enemies/CastleShieldKnight");
			SpCastleLargeSoldier = Get("Sprites/Enemies/CastleLargeSoldier");
			SpCastleEngineer = Get("Sprites/Enemies/CastleEngineer");
			SpKeepWarCheveux = Get("Sprites/Enemies/KeepWarCheveux");
			SpKeepAristocrat = Get("Sprites/Enemies/KeepAristocrat");
			SpKeepDemon = Get("Sprites/Enemies/KeepDemon");
			SpTowerIceMage = Get("Sprites/Enemies/TowerIceMage");
			SpTowerPlasmaPod = Get("Sprites/Enemies/TowerPlasmaPod");
			SpTowerDemonMage = Get("Sprites/Enemies/TowerRoyalGuard");
			SpLakeAnemone = Get("Sprites/Enemies/LakeAnemone");
			SpLakeBirdEgg = Get("Sprites/Enemies/LakeBirdEgg");
			SpLakeCheveux = Get("Sprites/Enemies/LakeCheveux");
			SpLakeEel = Get("Sprites/Enemies/LakeEel");
			SpLakeFly = Get("Sprites/Enemies/LakeFly");
			SpFortressKnight = Get("Sprites/Enemies/FortressKnight");
			SpFortressGunner = Get("Sprites/Enemies/FortressGunner");
			SpFortressLargeSoldier = Get("Sprites/Enemies/FortressLargeSoldier");
			SpFortressEngineer = Get("Sprites/Enemies/FortressEngineer");
			break;
		case 2:
			SpLabTurret = Get("Sprites/Enemies/LabTurret");
			SpLabChild = Get("Sprites/Enemies/LabChild");
			SpLabAdult = Get("Sprites/Enemies/LabAdult");
			SpLabSpider = Get("Sprites/Enemies/LabSpider");
			SpCursedCopperWyvern = Get("Sprites/Enemies/CursedCopperWyvern");
			SpCursedSiren = Get("Sprites/Enemies/CursedSiren");
			SpCursedMushroomTower = Get("Sprites/Enemies/CursedMushroomTower");
			SpCursedSporeVine = Get("Sprites/Enemies/CursedSporeVine");
			SpCursedSnail = Get("Sprites/Enemies/CursedSnail");
			SpCursedAnemone = Get("Sprites/Enemies/CursedAnemone");
			SpCursedMoth = Get("Sprites/Enemies/CursedMoth");
			SpEmpDemon = Get("Sprites/Enemies/EmpDemon");
			SpEmpRoyalGuard = Get("Sprites/Enemies/EmpRoyalGuard");
			SpEmpAristocrat = Get("Sprites/Enemies/EmpAristocrat");
			SpTempleConviction = Get("Sprites/Enemies/TempleConviction");
			SpGyreMajorUgly = Get("Sprites/Enemies/GyreMajorUgly");
			SpGyreMeteorSparrow = Get("Sprites/Enemies/GyreMeteorSparrow");
			SpGyreKain = Get("Sprites/Enemies/GyreKain");
			SpGyreNethershade = Get("Sprites/Enemies/GyreNethershade");
			SpGyreRyshia = Get("Sprites/Enemies/GyreRyshia");
			SpGyreZel = Get("Sprites/Enemies/GyreZel");
			break;
		case 3:
			SpRoboKitty = Get("Sprites/Bosses/Boss01");
			SpVarndagroth = Get("Sprites/Bosses/Boss02");
			SpDemonBoss = Get("Sprites/Bosses/Boss05");
			SpAelana = Get("Sprites/Bosses/Boss06");
			SpBirdBoss = Get("Sprites/Bosses/Boss07");
			SpMawBoss = Get("Sprites/Bosses/Boss08");
			SpShapeshifter = Get("Sprites/Bosses/Boss11");
			SpEmperor = Get("Sprites/Bosses/Boss12");
			SpEmperorVilete = Get("Sprites/Bosses/Boss13");
			SpEmperorWinderia = Get("Sprites/Bosses/Boss14");
			SpSandmanBoss = Get("Sprites/Bosses/Boss15");
			SpNightmareBoss = Get("Sprites/Bosses/Boss16");
			SpRavenBoss = Get("Sprites/Bosses/Boss17");
			SpXarionBoss = Get("Sprites/Bosses/Boss18");
			SpZelBoss = Get("Sprites/Bosses/Boss19");
			SpCantoranBoss = Get("Sprites/Bosses/Boss06b");
			SpOrbMeleeBlue = Get("Sprites/Projectiles/OrbMeleeBlue");
			SpOrbMeleeBlade = Get("Sprites/Projectiles/OrbMeleeBlade");
			SpOrbMeleeFire = Get("Sprites/Projectiles/OrbMeleeFire");
			SpOrbMeleeIron = Get("Sprites/Projectiles/OrbMeleeIron");
			SpOrbPlasma = Get("Sprites/Projectiles/OrbPlasma");
			SpOrbMeleeWind = Get("Sprites/Projectiles/OrbMeleeWind");
			break;
		case 4:
			SpOrbMeleeIce = Get("Sprites/Projectiles/OrbMeleeIce");
			SpOrbMeleeUmbra = Get("Sprites/Projectiles/OrbMeleeUmbra");
			SpOrbMeleeEmpire = Get("Sprites/Projectiles/OrbMeleeEmpire");
			SpOrbMeleeEye = Get("Sprites/Projectiles/OrbMeleeEye");
			SpOrbMeleeBlood = Get("Sprites/Projectiles/OrbMeleeBlood");
			SpOrbMeleeGun = Get("Sprites/Projectiles/OrbMeleeGun");
			SpOrbMeleeMoon = Get("Sprites/Projectiles/OrbMeleeMoon");
			SpOrbMeleeBarrier = Get("Sprites/Projectiles/OrbMeleeBarrier");
			SpOrbMeleeNether = Get("Sprites/Projectiles/OrbMeleeNether");
			SpOrbMeleeBook = Get("Sprites/Projectiles/OrbMeleeBook");
			SpMiscProjectiles = Get("Sprites/Projectiles/MiscProjectiles");
			SpStatusEffects = Get("Sprites/StatusEffects/StatusEffects");
			SpItems = Get("Sprites/Items/Items");
			SpMenuIcons = Get("Sprites/Items/MenuIcons");
			_areSpritesLoaded = true;
			break;
		}
	}

	internal void LoadSprites()
	{
		if (!_areSpritesLoaded)
		{
			for (int i = 0; i < 5; i++)
			{
				LoadSpritesStage(i);
			}
		}
	}

	public void LoadTilesetsStage(int stage, ContentManager content)
	{
		if (_areTilesetsLoaded)
		{
			return;
		}
		if (_generalContentManager == null)
		{
			_generalContentManager = content;
		}
		if (stage == 0)
		{
			TsEventTiles = Get("Tilesets/EventTiles");
			TsL1Tileset = Get("Tilesets/L1_Tileset");
			TsL2Tileset = Get("Tilesets/L2_Tileset");
			TsL5Tileset = Get("Tilesets/L5_Tileset");
			TsL7Tileset = Get("Tilesets/L7_Tileset");
			TsL10Tileset = Get("Tilesets/L10_Tileset");
			TsNexusTileset = Get("Tilesets/Nexus_Tileset");
			TsCityTileset = Get("Tilesets/City_Tileset");
			TsForestTileset = Get("Tilesets/Forest_Tileset");
			TsForestPastTileset = Get("Tilesets/Forest_Past_Tileset");
			TsRuinedTempleTileset = Get("Tilesets/Ruined_Temple_Tileset");
		}
		else if (stage == 1)
		{
			TsTowerTileset = Get("Tilesets/Tower_Tileset");
			TsTransitionsTileset = Get("Tilesets/Transitions_Tileset");
			TsCurtainTileset = Get("Tilesets/Curtain_Tileset");
			TsCavesTileset = Get("Tilesets/Caves_Tileset");
			TsCaves2Tileset = Get("Tilesets/Caves2_Tileset");
			TsCursedCavesTileset = Get("Tilesets/Cursed_Caves_Tileset");
			TsCursedCaves2Tileset = Get("Tilesets/Cursed_Caves2_Tileset");
			TsLabTileset = Get("Tilesets/Lab_Tileset");
			TsEmperorTowerTileset = Get("Tilesets/Emperor_Tower_Tileset");
			TsTempleTileset = Get("Tilesets/Temple_Tileset");
			TsEndingTileset = Get("Tilesets/Ending_Tileset");
			_areTilesetsLoaded = true;
		}
	}

	public void LoadTilesets(ContentManager content)
	{
		if (!_areTilesetsLoaded)
		{
			LoadTilesetsStage(0, content);
			LoadTilesetsStage(1, content);
		}
	}

	internal void LoadEvents(ContentManager content)
	{
		if (!_areEventsLoaded)
		{
			if (_generalContentManager == null)
			{
				_generalContentManager = content;
			}
			SpSaveStatue = Get("Sprites/Events/SaveStatue");
			SpSlidingDoors = Get("Sprites/Events/SlidingDoors");
			SpKeycardDoor = Get("Sprites/Events/KeycardDoor");
			SpPlatforms = Get("Sprites/Events/Platforms");
			SpTheTimespinner = Get("Sprites/Events/TheTimespinner");
			SpCityTimespinner = Get("Sprites/Events/CityTimespinner");
			SpTreasureChests = Get("Sprites/Items/TreasureChests");
			SpMiscCurtain = Get("Sprites/Events/Misc_Curtain");
			SpMiscHangar = Get("Sprites/Events/Misc_Hangar");
			SpMiscHangar2 = Get("Sprites/Events/Misc_Hangar2");
			SpMiscLab = Get("Sprites/Events/Misc_Lab");
			SpLanterns = Get("Sprites/Events/Lanterns");
			SpPetrifiedVines = Get("Sprites/Events/PetrifiedVines");
			SpOrbPedestal = Get("Sprites/Events/OrbPedestal");
			SpBackerPortraits = Get("Sprites/Events/BackerPortraits");
			SpBrokenGlass = Get("Sprites/Events/BrokenGlass");
			_areEventsLoaded = true;
		}
	}

	internal void LoadFX(ContentManager content)
	{
		if (!_areFXLoaded)
		{
			SpBoomAnimation = Get("Animations/Boom");
			SpEffectsLarge = Get("Animations/EffectsLarge");
			SpEffectsMedium = Get("Animations/EffectsMedium");
			SpEffectsSmall = Get("Animations/EffectsSmall");
			SpTimeGateAnimation = Get("Animations/TimeGateAnimation");
			SpAuraEffects = Get("Animations/AuraEffects");
			SpSoulStream = Get("Animations/SoulStream");
			SpSandTexture = Get("Animations/SandTexture");
			SpDifferenceCloud = Get("Animations/DifferenceCloud");
			TxParticleEnergy = content.Load<Texture2D>("Particles/BallEnergy");
			TxParticleDust = content.Load<Texture2D>("Particles/Dust");
			TxParticleSmoke = content.Load<Texture2D>("Particles/Smoke");
			SpAnimatedParticlesSmall = Get("Particles/AnimatedParticlesSmall");
			_areFXLoaded = true;
		}
	}

	public void LoadBackgroundsStage(int stage, ContentManager content)
	{
		if (_areBackgroundsLoaded)
		{
			return;
		}
		if (_generalContentManager == null)
		{
			_generalContentManager = content;
		}
		if (stage == 0)
		{
			BgSpaceTiled = Get("Backgrounds/SpaceTiled");
			BgL1Backdrop1 = Get("Backgrounds/L1_Backdrop1");
			BgL1Backdrop2 = Get("Backgrounds/L1_Backdrop2");
			BgL1Backdrop3 = Get("Backgrounds/L1_Backdrop3");
			BgL1Backdrop4 = Get("Backgrounds/L1_Backdrop4");
			BgL1NearBackdrop = Get("Backgrounds/L1_NearBackdrop");
			BgL2Fountain = Get("Backgrounds/L2_Fountain");
			BgL7Backdrop1 = Get("Backgrounds/L7_Backdrop1");
			BgL7Backdrop2 = Get("Backgrounds/L7_Backdrop2");
			BgL7Backdrop3 = Get("Backgrounds/L7_Backdrop3");
			BgL7Backdrop4 = Get("Backgrounds/L7_Backdrop4");
			BgL7NearBackdrop = Get("Backgrounds/L7_NearBackdrop");
			BgCurtainsBackdrops1 = Get("Backgrounds/Curtain_Backdrops1");
			BgCurtainsBackdrops2 = Get("Backgrounds/Curtain_Backdrops2");
			BgCurtainsBackdrops3 = Get("Backgrounds/Curtain_Backdrops3");
			BgCaveBackdropWaterfall1 = Get("Backgrounds/Cave_Backdrop_Waterfall1");
			BgCaveBackdrops1 = Get("Backgrounds/Cave_Backdrops1");
			BgCaveBackdrops2 = Get("Backgrounds/Cave_Backdrops2");
			BgCursedCaveBackdropWaterfall1 = Get("Backgrounds/Cursed_Cave_Backdrop_Waterfall1");
			BgCursedCaveBackdrops1 = Get("Backgrounds/Cursed_Cave_Backdrops1");
			BgCursedCaveBackdrops2 = Get("Backgrounds/Cursed_Cave_Backdrops2");
			BgCursedCaveBackdrops3 = Get("Backgrounds/Cursed_Cave_Backdrops3");
			BgCityBackdrops1 = Get("Backgrounds/City_Backdrops1");
			BgCityBackdrops2 = Get("Backgrounds/City_Backdrops2");
			BgCityBackdrops3 = Get("Backgrounds/City_Backdrops3");
			BgCityBackdrops4 = Get("Backgrounds/City_Backdrops4");
			BgCityBackdrops5 = Get("Backgrounds/City_Backdrops5");
		}
		else if (stage == 1)
		{
			BgForestTreesFarthest = Get("Backgrounds/Forest_Trees_Farthest");
			BgForestTreesFar = Get("Backgrounds/Forest_Trees_Far");
			BgForestTreesMid = Get("Backgrounds/Forest_Trees_Mid");
			BgForestTreesNear = Get("Backgrounds/Forest_Trees_Near");
			BgForestRocksFar = Get("Backgrounds/Forest_Rocks_Far");
			BgForestRocksMid = Get("Backgrounds/Forest_Rocks_Mid");
			BgForestRocksNear = Get("Backgrounds/Forest_Rocks_Near");
			BgForestTemple1 = Get("Backgrounds/Forest_Temple1");
			BgForestTemple2 = Get("Backgrounds/Forest_Temple2");
			BgForestWaterfall1 = Get("Backgrounds/Forest_Waterfall1");
			BgForestWaterfall2 = Get("Backgrounds/Forest_Waterfall2");
			BgForestWaterfall3 = Get("Backgrounds/Forest_Waterfall3");
			BgForestWaterfall4 = Get("Backgrounds/Forest_Waterfall4");
			BgKeepBackdrops1 = Get("Backgrounds/Keep_Backdrops1");
			BgKeepBackdrops2 = Get("Backgrounds/Keep_Backdrops2");
			BgHangarBackdrops1 = Get("Backgrounds/Hangar_Backdrops1");
			BgHangarBackdrops2 = Get("Backgrounds/Hangar_Backdrops2");
			BgHangarBackdrops3 = Get("Backgrounds/Hangar_Backdrops3");
			BgL6Backdrop1 = Get("Backgrounds/L6_Backdrop1");
			BgLabBackdrop1 = Get("Backgrounds/Lab_Backdrop1");
			BgTowerBackdrops1 = Get("Backgrounds/Tower_Backdrops1");
			BgTowerBackdrops2 = Get("Backgrounds/Tower_Backdrops2");
			BgTowerBackdrops3 = Get("Backgrounds/Tower_Backdrops3");
			BgTowerBackdrops4 = Get("Backgrounds/Tower_Backdrops4");
		}
		else if (stage == 2)
		{
			BgEmperorTowerBackdrops1 = Get("Backgrounds/Emperor_Tower_Backdrops1");
			BgEmperorTowerBackdrops2 = Get("Backgrounds/Emperor_Tower_Backdrops2");
			BgEmperorTowerBackdrops3 = Get("Backgrounds/Emperor_Tower_Backdrops3");
			BgEmperorTowerBackdrops5 = Get("Backgrounds/Emperor_Tower_Backdrops5");
			BgTempleBackdrop1 = Get("Backgrounds/Temple_Backdrop1");
			BgTimeBackdrop1 = Get("Backgrounds/Time_Backdrop1");
			BgTimeBackdrop2 = Get("Backgrounds/Time_Backdrop2");
			BgTimeBackdrop3 = Get("Backgrounds/Time_Backdrop3");
			BgGyreBackdrops1 = Get("Backgrounds/Gyre_Backdrops1");
			BgGyreBackdrops2 = Get("Backgrounds/Gyre_Backdrops2");
			BgGyreBackdrops3 = Get("Backgrounds/Gyre_Backdrops3");
			BgGyreBackdrops4 = Get("Backgrounds/Gyre_Backdrops4");
			BgEndingBackdrops1 = Get("Backgrounds/Ending_Backdrops1");
			BgVileteBackdrop = Get("Backgrounds/Vilete_Backdrop");
			BgVileteBackdrop2 = Get("Backgrounds/Vilete_Backdrop2");
			BgVileteArchways = Get("Backgrounds/Vilete_Archways");
			BgWinderiaBackdrop1 = Get("Backgrounds/Win_Backdrop1");
			BgWinderiaBackdrop2 = Get("Backgrounds/Win_Backdrop2");
			BgWinderiaBackdrop3 = Get("Backgrounds/Win_Backdrop3");
			BgWinderiaBackdrop4 = Get("Backgrounds/Win_Backdrop4");
			BgWinderiaBackdrop5 = Get("Backgrounds/Win_Backdrop5");
			BgWinderiaBackdrop6 = Get("Backgrounds/Win_Backdrop6");
			BgFog = Get("Backgrounds/Fog");
			BgHalfFog = Get("Backgrounds/HalfFog");
			BgClouds = Get("Backgrounds/Clouds");
			_areBackgroundsLoaded = true;
		}
	}

	public void LoadBackgrounds(ContentManager content)
	{
		if (!_areBackgroundsLoaded)
		{
			LoadBackgroundsStage(0, content);
			LoadBackgroundsStage(1, content);
			LoadBackgroundsStage(2, content);
		}
	}

	public void LoadUI(ContentManager content)
	{
		if (!_areUIElementLoaded)
		{
			if (_generalContentManager == null)
			{
				_generalContentManager = content;
			}
			_latinFont = content.Load<SpriteFont>("Fonts/LatinFont");
			_latinFont.LineSpacing = 16;
			ActiveFont = _latinFont;
			SpNumbers = Get("Overlays/PlayerHUD/Numbers");
			TxBlankSquare = content.Load<Texture2D>("Overlays/BlankSquare");
			TxLargeCircle = content.Load<Texture2D>("Overlays/LargeCircle");
			TxLargeRing = content.Load<Texture2D>("Overlays/LargeRing");
			TxUIBlackGradient = content.Load<Texture2D>("Overlays/Menu/BlackGradientBox");
			SpMiniMap = Get("Overlays/PlayerHUD/Minimap");
			SpTextBox = Get("Overlays/Menu/TextBox");
			SpLunaisHUD = Get("Overlays/PlayerHUD/LunaisHUD");
			SpMenuCursor = Get("Overlays/Menu/MenuCursor");
			SpPauseMenu = Get("Overlays/Menu/PauseMenu");
			SpJournalMenu = Get("Overlays/Menu/JournalMenu");
			SpMenuCharacters = Get("Overlays/Menu/MenuCharacters");
			SpLoadingScreen = Get("Overlays/Title/LoadingScreen");
			SpPortraits = Get("Sprites/Heroes/Portraits");
			SpPortraits2 = Get("Sprites/Heroes/Portraits2");
			SpBossPortraits = Get("Sprites/Bosses/BossPortraits");
			SpUIButtons = Get("Overlays/Menu/ControllerButtons");
			SpLevelUp = Get("Overlays/PlayerHUD/LevelUp");
			SpLocToastsLatin = Get("Overlays/PlayerHUD/LocToastLatin");
			SpLocToastsAsian = Get("Overlays/PlayerHUD/LocToastAsian");
			SpSmoothCircles = Get("Overlays/SmoothCircles");
			SpBorderFrame = Get("Overlays/BorderFrame");
			SpToolsLogo = Get("Overlays/ToolsLogo");
			_areUIElementLoaded = true;
		}
	}

	internal void LoadLocalizedContent(ContentManager localizedContent, ELanguageLocale locale)
	{
		switch (locale)
		{
		case ELanguageLocale.BP:
			SpAreaTitles = Get("Overlays/PlayerHUD/AreaTitles_BP", localizedContent);
			break;
		case ELanguageLocale.CN:
			SpAreaTitles = Get("Overlays/PlayerHUD/AreaTitles_CN", localizedContent);
			break;
		case ELanguageLocale.DE:
			SpAreaTitles = Get("Overlays/PlayerHUD/AreaTitles_DE", localizedContent);
			break;
		case ELanguageLocale.ES:
			SpAreaTitles = Get("Overlays/PlayerHUD/AreaTitles_ES", localizedContent);
			break;
		case ELanguageLocale.FR:
			SpAreaTitles = Get("Overlays/PlayerHUD/AreaTitles_FR", localizedContent);
			break;
		case ELanguageLocale.JP:
			SpAreaTitles = Get("Overlays/PlayerHUD/AreaTitles_JP", localizedContent);
			break;
		case ELanguageLocale.RU:
			SpAreaTitles = Get("Overlays/PlayerHUD/AreaTitles_RU", localizedContent);
			break;
		default:
			SpAreaTitles = Get("Overlays/PlayerHUD/AreaTitles", localizedContent);
			break;
		}
		switch (locale)
		{
		case ELanguageLocale.CN:
			ActiveFont = localizedContent.Load<SpriteFont>("Fonts/CNFont");
			break;
		case ELanguageLocale.JP:
			ActiveFont = localizedContent.Load<SpriteFont>("Fonts/JPFont");
			break;
		default:
			ActiveFont = _latinFont;
			break;
		}
		ActiveFont.LineSpacing = 16;
		_bestiary.RefreshNamesAndDescriptions();
	}

	public void LoadEditorContent(ContentManager content)
	{
		if (_generalContentManager == null)
		{
			_generalContentManager = content;
		}
		TxSelectSquare = content.Load<Texture2D>("Overlays/SelectSquare");
		TsObjectsTileset = new SpriteSheet("Tilesets/Objects_Tileset", content.Load<Texture2D>("Tilesets/Objects_Tileset"), new Point(16, 16));
	}

	public SpriteSheet GetTileset(string tilesetName)
	{
		return GetTileset(EnumExtensions.EnumParse<ETilesetType>(tilesetName));
	}

	public SpriteSheet GetTileset(ETilesetType tileset)
	{
		return tileset switch
		{
			ETilesetType.L1_Tileset => TsL1Tileset, 
			ETilesetType.L2_Tileset => TsL2Tileset, 
			ETilesetType.L5_Tileset => TsL5Tileset, 
			ETilesetType.L7_Tileset => TsL7Tileset, 
			ETilesetType.L10_Tileset => TsL10Tileset, 
			ETilesetType.Nexus_Tileset => TsNexusTileset, 
			ETilesetType.City_Tileset => TsCityTileset, 
			ETilesetType.Forest_Tileset => TsForestTileset, 
			ETilesetType.Forest_Past_Tileset => TsForestPastTileset, 
			ETilesetType.Ruined_Temple_Tileset => TsRuinedTempleTileset, 
			ETilesetType.Tower_Tileset => TsTowerTileset, 
			ETilesetType.Transitions_Tileset => TsTransitionsTileset, 
			ETilesetType.Curtain_Tileset => TsCurtainTileset, 
			ETilesetType.Caves_Tileset => TsCavesTileset, 
			ETilesetType.Caves2_Tileset => TsCaves2Tileset, 
			ETilesetType.Cursed_Caves_Tileset => TsCursedCavesTileset, 
			ETilesetType.Cursed_Caves2_Tileset => TsCursedCaves2Tileset, 
			ETilesetType.Lab_Tileset => TsLabTileset, 
			ETilesetType.Emperor_Tower_Tileset => TsEmperorTowerTileset, 
			ETilesetType.Temple_Tileset => TsTempleTileset, 
			ETilesetType.EndingTileset => TsEndingTileset, 
			_ => TsL1Tileset, 
		};
	}

	public void CropScreen(SpriteBatch spriteBatch, Rectangle cropRect, Point screenRect)
	{
		if (cropRect.Left > 0)
		{
			spriteBatch.Draw(TxBlankSquare, new Rectangle(0, 0, cropRect.Left, screenRect.Y), Color.Black);
			spriteBatch.Draw(TxBlankSquare, new Rectangle(cropRect.Right, 0, cropRect.Left, screenRect.Y), Color.Black);
		}
		if (cropRect.Top > 0)
		{
			spriteBatch.Draw(TxBlankSquare, new Rectangle(0, 0, screenRect.X, cropRect.Top), Color.Black);
			spriteBatch.Draw(TxBlankSquare, new Rectangle(0, cropRect.Bottom, screenRect.X, cropRect.Top), Color.Black);
		}
	}

	public void DrawScreenBorderFrame(SpriteBatch spriteBatch, Rectangle cropRect, Point screenRect)
	{
		if (cropRect.Left > 0)
		{
			int inGameZoom = Constants.InGameZoom;
			Rectangle frameSource = SpBorderFrame.GetFrameSource(0);
			int num = screenRect.Y / 2 - inGameZoom * 136;
			int num2 = 40 * inGameZoom;
			int num3 = 256 * inGameZoom;
			int y = num + num3;
			int num4 = cropRect.Left - num2;
			spriteBatch.Draw(SpBorderFrame.Texture, new Rectangle(num4, num, num2, num3), frameSource, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
			spriteBatch.Draw(SpBorderFrame.Texture, new Rectangle(cropRect.Right, num, num2, num3), frameSource, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
			int num5 = 20 * inGameZoom;
			int height = 16 * inGameZoom;
			frameSource = SpBorderFrame.GetFrameSource(1);
			spriteBatch.Draw(SpBorderFrame.Texture, new Rectangle(num4, y, num5, height), frameSource, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
			spriteBatch.Draw(SpBorderFrame.Texture, new Rectangle(cropRect.Right + num5, y, num5, height), frameSource, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
			frameSource = SpBorderFrame.GetFrameSource(2);
			spriteBatch.Draw(SpBorderFrame.Texture, new Rectangle(num4 + num5, y, num5, height), frameSource, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
			spriteBatch.Draw(SpBorderFrame.Texture, new Rectangle(cropRect.Right, y, num5, height), frameSource, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
		}
	}
}
