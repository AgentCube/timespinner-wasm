using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Misc;
using Timespinner.GameObjects.Heroes;
using Timespinner.GameObjects.Items;

namespace Timespinner.GameObjects.Events.Lanterns;

internal class BaseLantern : GameEvent
{
	private const int DefaultSandAmount = 15;

	private const float TimeToStayDormant = 3f;

	private const int DefaultGlowCircleCount = 12;

	private const int DefaultGlowCircleRadius = 64;

	private const float GlowCircleConsecutiveSizeReduction = 0.9f;

	private const float GlowColorMultipler = 0.05f;

	private const float GlowOscillationMultipler = 1f;

	private const float GlowTimeToFadeIn = 1f;

	private const float GlowFrequency = 3f;

	private const float DefaultFlickerSpeed = 0.05f;

	private static readonly Vector4 DefaultOrbGlowColor = new Vector4(0.6f, 0.5f, 0.1f, 1f);

	private readonly ELanternType _lanternType;

	private readonly SpriteSheet _glowSprite;

	protected float _oscillDelta;

	protected Color _glowCircleColor;

	protected Vector4 _orbGlowColor;

	private bool _isFlickeredOff;

	protected bool _isDead;

	private int _glowRadius;

	private float _flickerTimer;

	private float _dormantTimer;

	private float _fadeInTimer;

	private float _glowMultiplier;

	private Rectangle _radianceBbox;

	internal bool DoesRegenerate { get; set; }

	internal bool IsVisible { get; set; }

	internal bool DoesFlicker { get; set; }

	internal bool IsDormant { get; private set; }

	internal bool IsInvulnerable { get; set; }

	internal bool IsBroken => _isDead;

	internal bool DoesDropLoot { get; set; }

	internal bool AreAppendagesVisibleWhenDormant { get; set; }

	internal DebrisEvent.EDebrisDeathType DebrisType { get; set; }

	internal ELanternType LanternType => _lanternType;

	internal int SandAmount { get; set; }

	internal int GlowCircleCount { get; set; }

	internal int GlowRadius
	{
		get
		{
			return _glowRadius;
		}
		set
		{
			_glowRadius = value;
			_radianceBbox = new Rectangle(_bbox.Center.X - value / 2, _bbox.Center.Y - value / 2, value, value);
		}
	}

	internal Point LanternGlowOffset { get; set; }

	internal Point ItemDropOffset { get; set; }

	internal Vector4 OrbGlowColor { get; set; }

	public BaseLantern(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.EventType = EEventTileType.Lantern;
		_lanternType = (ELanternType)objectSpec.Argument;
		_glowSprite = inLevel.GCM.SpSmoothCircles;
		DebrisType = DebrisEvent.EDebrisDeathType.Fire;
		base.DoesCollideWithProjectiles = true;
		DoesRegenerate = true;
		DoesDropLoot = true;
		SandAmount = 15;
		GlowCircleCount = 12;
		GlowRadius = 64;
		OrbGlowColor = DefaultOrbGlowColor;
		_flickerTimer += (float)inLevel.NextRandomDouble();
		_oscillDelta = (float)_level.NextRandomDouble();
	}

	public override void Update(float delta)
	{
		if (!IsDormant)
		{
			IsVisible = _level.VisibleArea.Intersects(_radianceBbox);
		}
		if (!base.IsFrozen)
		{
			if (DoesRegenerate && IsDormant)
			{
				IsVisible = false;
				_dormantTimer -= delta;
				if (_dormantTimer <= 0f)
				{
					IsDormant = false;
					_dormantTimer = 0f;
					Revive();
				}
			}
			if (!IsDormant)
			{
				if (_fadeInTimer > 0f)
				{
					_fadeInTimer -= delta;
					if (_fadeInTimer < 0f)
					{
						_fadeInTimer = 0f;
					}
					_glowMultiplier = 1f - (float)Math.Sin(_fadeInTimer / 1f * ((float)Math.PI / 4f));
				}
				else
				{
					_glowMultiplier = 1f;
				}
				_oscillDelta += delta;
				if (_oscillDelta >= (float)Math.PI * 2f)
				{
					_oscillDelta -= (float)Math.PI * 2f;
				}
				_orbGlowColor = OrbGlowColor;
				_orbGlowColor.W = (float)Math.Cos(_oscillDelta * 3f) * 0.1f + 0.35f;
				_glowCircleColor = new Color(_orbGlowColor * 0.05f * 1f * _glowMultiplier);
				base.IsGlowing = true;
				base.GlowColor = new Color(_orbGlowColor * _glowMultiplier);
				if (DoesFlicker)
				{
					_flickerTimer += delta;
					if (_flickerTimer >= 0.05f)
					{
						_flickerTimer -= 0.05f;
						_isFlickeredOff = !_isFlickeredOff;
					}
				}
			}
		}
		else if (DoesFlicker)
		{
			_isFlickeredOff = false;
			_flickerTimer = 0f;
		}
		base.Update(delta);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (!IsDormant)
		{
			base.Draw(spriteBatch);
		}
		else if (AreAppendagesVisibleWhenDormant)
		{
			DrawAppendages(spriteBatch, drawUnder: false);
			DrawParticleSystems(spriteBatch);
			DrawAppendages(spriteBatch, drawUnder: true);
		}
		if (IsVisible)
		{
			Point point = Bbox.Center.Add(LanternGlowOffset);
			float num = GlowRadius;
			int num2 = (int)((float)GlowCircleCount * _glowMultiplier);
			Color glowCircleColor = _glowCircleColor;
			if (_isFlickeredOff)
			{
				glowCircleColor *= 0.9f;
			}
			for (int i = 0; i < num2; i++)
			{
				Vector2 value = new Vector2((float)point.X - num / 2f, (float)point.Y - num / 2f);
				Vector2 vector = Vector2.Subtract(_level.LevelRenderCenter, Vector2.Subtract(_level.CameraPosition, value));
				SmoothCircle.Draw(destination: new Rectangle((int)vector.X, (int)vector.Y, (int)num, (int)num), spriteBatch: spriteBatch, sprite: _glowSprite, color: glowCircleColor);
				num *= 0.9f;
			}
		}
	}

	public override bool ProjectileTriggerEvent(Projectile projectile, Vector2 depth)
	{
		bool result = false;
		if (!IsDormant && !base.IsFrozen && !_isDead && !IsInvulnerable)
		{
			result = true;
			Explode(projectile);
			if (DoesDropLoot)
			{
				DropLoot();
			}
			if (DoesRegenerate)
			{
				IsDormant = true;
				_dormantTimer = 3f;
				_fadeInTimer = 1f;
			}
			else
			{
				_isDead = true;
				Kill();
			}
		}
		return result;
	}

	internal virtual void Explode(Projectile projectile)
	{
	}

	internal virtual void Revive()
	{
	}

	internal virtual void DropLoot()
	{
		bool flag = DoesRegenerate;
		Point position = Bbox.Center.Add(ItemDropOffset);
		if (!DoesRegenerate)
		{
			Protagonist mainHero = _level.MainHero;
			if (mainHero != null)
			{
				flag = mainHero.MP < mainHero.MaxMP;
			}
		}
		if (flag)
		{
			_level.AddItem(EItemType.Sand, SandAmount, position, _level.NextObjectTicketID);
			return;
		}
		int gemAmountFromLotteryRoll = GemItem.GetGemAmountFromLotteryRoll((float)_level.NextRandomDouble());
		_level.AddItem(EItemType.Money, gemAmountFromLotteryRoll, position, _level.NextObjectTicketID);
	}

	internal void CreateDebris(Projectile projectile)
	{
		SetCharacterSequenceByName("Destructable");
		UpdateCharacterSequences(0f);
		UpdateAppendages(0f);
		DebrisEvent.CreateFromObject(this, projectile.VisibleVelocity * Math.Min(1, projectile.Force), projectile.Bbox.Center, _sprite, DebrisType);
		_appendages.Clear();
	}

	public static BaseLantern FromArgumentAndLevel(Level level, Point position, int id, ObjectTileSpecification spec)
	{
		return (ELanternType)((spec.Argument >= 10) ? spec.Argument : (spec.Argument = level.ID switch
		{
			1 => 10, 
			2 => 2000, 
			4 => (spec.Argument == 0) ? 17 : 16, 
			5 => (spec.Argument == 1) ? 19 : 18, 
			6 => (spec.Argument == 1) ? 21 : 20, 
			8 => spec.Argument switch
			{
				1 => 25, 
				2 => 26, 
				_ => 24, 
			}, 
			_ => 10, 
		})) switch
		{
			ELanternType.F_1_Lake_Fruit_Pink => new PinkFruitLanternEvent(level, position, id, spec), 
			ELanternType.F_1_Lake_Fruit_Blue => new BlueFruitLanternEvent(level, position, id, spec), 
			ELanternType.F_2_City_Lamp => new MetropolisLanternEvent(level, position, id, spec), 
			ELanternType.F_2_City_Tunnel => new MetropolisLanternTunnelEvent(level, position, id, spec), 
			ELanternType.F_2_City_Library_Hang => new MetropolisLanternLibraryHangEvent(level, position, id, spec), 
			ELanternType.F_2_City_Library_Stand => new MetropolisLanternLibraryStandEvent(level, position, id, spec), 
			ELanternType.F_2_City_Tower_Hang => new HangingLanternEvent(level, position, id, spec), 
			ELanternType.F_2_City_Tower_Stand => new MetropolisLanternTowerStandEvent(level, position, id, spec), 
			ELanternType.P_3_Forest_Lamp => new ForestLampLanternEvent(level, position, id, spec), 
			ELanternType.P_3_Forest_Campfire => new ForestCampfireLanternEvent(level, position, id, spec), 
			ELanternType.P_4_Curtain_Sconce => new SconceLanternEvent(level, position, id, spec), 
			ELanternType.P_4_Curtain_Brazier => new BrazierLanternEvent(level, position, id, spec), 
			ELanternType.P_5_Castle_Candelabra => new CandelabraLanternEvent(level, position, id, spec), 
			ELanternType.P_5_Castle_Sconce => new StairwellSconceLanternEvent(level, position, id, spec), 
			ELanternType.P_6_Tower_Candelabra => new TowerCandelabraLanternEvent(level, position, id, spec), 
			ELanternType.P_6_Tower_Lantern => new HangingLanternEvent(level, position, id, spec), 
			ELanternType.P_7_Lake_Candle => new LakeCandleLanternEvent(level, position, id, spec), 
			ELanternType.P_7_Lake_Coral => new LakeCoralLanternEvent(level, position, id, spec), 
			ELanternType.P_8_Cave_Candelabra => new CaveBrickCandelabraLanternEvent(level, position, id, spec), 
			ELanternType.P_8_Cave_Sconce => new CaveBrickSconceLanternEvent(level, position, id, spec), 
			ELanternType.P_8_Cave_Lantern => new CaveMineLanternEvent(level, position, id, spec), 
			ELanternType.F_9_Cursed_Cave_Candelabra => new CursedCaveCandelabraLanternEvent(level, position, id, spec), 
			ELanternType.F_9_Cursed_Cave_Sconce => new CursedCaveSconceLanternEvent(level, position, id, spec), 
			ELanternType.F_9_Cursed_Cave_Lantern => new CursedCaveMineLanternEvent(level, position, id, spec), 
			ELanternType.F_10_Hangar_Hanging => new HangarHangingLanternEvent(level, position, id, spec), 
			ELanternType.F_10_Hangar_Tower => new HangarTowerLanternEvent(level, position, id, spec), 
			ELanternType.F_11_Lab_Tower => new LabTowerLanternEvent(level, position, id, spec), 
			ELanternType.F_11_Lab_Coffee => new LabCoffeeLanternEvent(level, position, id, spec), 
			ELanternType.F_12_EmpTower_Candelabra => new EmpTowerCandelabraLanternEvent(level, position, id, spec), 
			ELanternType.Q_13_Forest_Lantern => new PrologueLanternEvent(level, position, id, spec), 
			ELanternType.Q_14_Temple_Candelabra => new TempleCandelabraLanternEvent(level, position, id, spec), 
			ELanternType.Q_14_Temple_Candle => new TempleCandleLanternEvent(level, position, id, spec), 
			ELanternType.Q_15_Gyre_Lantern => new GyreLanternEvent(level, position, id, spec), 
			_ => new PinkFruitLanternEvent(level, position, id, spec), 
		};
	}
}
