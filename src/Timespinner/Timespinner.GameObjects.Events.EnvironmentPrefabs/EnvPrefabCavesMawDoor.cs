using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.HUD;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Cutscene;
using Timespinner.GameObjects.Events.Misc;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs;

internal sealed class EnvPrefabCavesMawDoor : EnvironmentPrefabBase
{
	private const int RibCount = 5;

	private const int RibStart = 1;

	private const int DestroyAllRibsThreshold = 2;

	private const int DestructableAppendagesStartIndex = 6;

	private const int DestructableAppendageCount = 7;

	private const int RibMaxHP = 50;

	private const int ParticlesOffsetX = -300;

	private const float TimeToChangeColor = 0.75f;

	private const float TimeBetweenKillingRibs = 0.05f;

	private static readonly Color FlashFrame1Color = new Color(1f, 1f, 0.8f, 0.65f);

	private static readonly Color FlashFrame2Color = new Color(1f, 0.3f, 0.1f, 0.8f);

	private static readonly Color PortalColor1 = new Color(96, 8, 16);

	private static readonly Color PortalColor2 = new Color(64, 0, 8);

	private readonly MawBossDoorChargeLazerPS _deathLazerParticles;

	private readonly MawBossWindyParticleSystem _windyParticles;

	private readonly int[] _ribFlashFrames = new int[5];

	private readonly int[] _ribHealthPoints = new int[5];

	private readonly float[] _ribDamageTimeoutTimers = new float[5];

	private readonly List<Appendage> _destructableAppendages = new List<Appendage>();

	private readonly List<Appendage> _portalAppendages = new List<Appendage>();

	private bool _isOpen;

	private bool _isDestroyingRibs;

	private bool _isPortalTargetColorNumber1;

	private bool _isHidden;

	private int _destroyedRibs;

	private float _portalGlowTimer;

	private float _ribKillTimer;

	private Color _lastPortalGlowColor;

	public EnvPrefabCavesMawDoor(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		Position = inPosition.Add(7, 0);
		_sprite = _level.GCM.SpMawBoss;
		Bbox = new Rectangle(0, 0, 32, 112);
		_doesDrawBaseSprite = false;
		_doesUseAppendageCollision = false;
		_doAppendagesInheritDrawColor = false;
		base.DoesCollideWithProjectiles = true;
		base.DrawPlane = EDrawPlane.Back;
		_isSolid = true;
		base.CanBeTriggered = true;
		for (int i = 0; i < 5; i++)
		{
			_ribHealthPoints[i] = 50;
		}
		for (int j = 0; j < 7; j++)
		{
			_destructableAppendages.Add(base.Appendages[6]);
			base.Appendages.RemoveAt(6);
		}
		_lastPortalGlowColor = PortalColor1;
		Appendage appendage = base.Appendages[0];
		foreach (Appendage appendage2 in appendage.Appendages)
		{
			_portalAppendages.Add(appendage2);
		}
		_windyParticles = new MawBossWindyParticleSystem(_level.GCM.SpAnimatedParticlesSmall, 20, Bbox.Height - 32, isBlowingLeft: false);
		_particleSystems.Add(_windyParticles);
		_deathLazerParticles = new MawBossDoorChargeLazerPS(_level.GCM.TxParticleEnergy, 5);
	}

	public override void Initialize()
	{
		base.Initialize();
		if (_level.GameSave.GetSaveBool("IsVileteSaved"))
		{
			_level.RequestRemoveObject(this);
			AddRubble();
		}
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			UpdatePortalColor(delta);
			if (_isOpen || _isHidden)
			{
				_windyParticles.AddParticles(new Vector2(Position.X + -300, Bbox.Center.Y));
			}
			if (_isHidden)
			{
				_deathLazerParticles.AddParticles(new Vector2(Position.X + 32, Bbox.Center.Y));
			}
		}
		if (!_isHidden)
		{
			for (int i = 0; i < 5; i++)
			{
				int index = 1 + i;
				Appendage appendage = base.Appendages[index];
				if (appendage.IsGlowing)
				{
					int num = _ribFlashFrames[i];
					if (num > -1)
					{
						appendage.IsGlowing = true;
						appendage.GlowBase = 1f;
						appendage.GlowColor = ((num % 3 == 0) ? FlashFrame1Color : FlashFrame2Color);
						num--;
						if (num <= -1)
						{
							appendage.IsGlowing = false;
							appendage.DrawColor = Color.White;
						}
						_ribFlashFrames[i] = num;
					}
				}
				float num2 = _ribDamageTimeoutTimers[i];
				if (num2 > 0f)
				{
					num2 -= delta;
					if (num2 < 0f)
					{
						num2 = 0f;
					}
					_ribDamageTimeoutTimers[i] = num2;
				}
				if (_isDestroyingRibs && _ribKillTimer <= 0f && appendage.AnimationStart != -1)
				{
					KillRib(appendage, appendage.Position);
					_ribKillTimer = 0.05f;
					break;
				}
			}
		}
		if (_isDestroyingRibs)
		{
			_ribKillTimer -= delta;
		}
		base.Update(delta);
	}

	private void UpdatePortalColor(float delta)
	{
		_portalGlowTimer += delta;
		Color color = (_isPortalTargetColorNumber1 ? PortalColor1 : PortalColor2);
		float num = 1f;
		if (_portalGlowTimer >= 0.75f)
		{
			_isPortalTargetColorNumber1 = !_isPortalTargetColorNumber1;
			_portalGlowTimer = 0f;
			_lastPortalGlowColor = color;
		}
		else
		{
			num = _portalGlowTimer / 0.75f;
		}
		Color drawColor = ((!(num < 1f)) ? color : _lastPortalGlowColor.SineInterpolate(color, num));
		foreach (Appendage portalAppendage in _portalAppendages)
		{
			portalAppendage.DrawColor = drawColor;
		}
	}

	public override bool ProjectileTriggerEvent(Projectile projectile, Vector2 depth)
	{
		if (!base.IsFrozen && !_isOpen && !_isHidden)
		{
			bool flag = false;
			Point point = projectile.Bbox.Center;
			for (int i = 0; i < 5; i++)
			{
				int index = i + 1;
				Appendage appendage = base.Appendages[index];
				if (!(_ribDamageTimeoutTimers[i] <= 0f) || _ribHealthPoints[i] <= 0)
				{
					continue;
				}
				Rectangle collidingRectangle = appendage.GetCollidingRectangle(projectile);
				if (collidingRectangle != Rectangle.Empty)
				{
					flag = true;
					point = projectile.FindDeathPoint(appendage, collidingRectangle);
					appendage.IsGlowing = true;
					_ribFlashFrames[i] = 4;
					int effectiveDamage = projectile.EffectiveDamage;
					_level.AddNumber(effectiveDamage, point, ENumberColor.White);
					_ribHealthPoints[i] -= effectiveDamage;
					_ribDamageTimeoutTimers[i] = projectile.DamageTimeout;
					if (_ribHealthPoints[i] <= 0)
					{
						KillRib(appendage, point);
					}
				}
			}
			if (flag && projectile.DoesDieOnImpact)
			{
				projectile.Kill(useAnimation: true, point, deathFromInvulnerable: false);
			}
			if (flag)
			{
				PlayCue(ESFX.LunaisOrbImpact, point);
			}
			if (_destroyedRibs >= 2)
			{
				_isDestroyingRibs = true;
				DoOpen();
			}
		}
		return true;
	}

	private void KillRib(Appendage rib, Point impactPoint)
	{
		_destroyedRibs++;
		rib.ChangeAnimation(-1);
		PlayCue(ESFX.BossMawBoneBreak, rib.Position);
		foreach (Appendage destructableAppendage in _destructableAppendages)
		{
			destructableAppendage.Position = rib.Position.Add(destructableAppendage.AnchorOffset);
			destructableAppendage.SnapBboxToPosition();
		}
		DebrisEvent.CreateFromAppendages(force: new Vector2(100 * _level.NextRandomInt(1, 5), -100 * _level.NextRandomInt(1, 5)), appendages: _destructableAppendages, origin: impactPoint, sprite: _sprite, deathType: DebrisEvent.EDebrisDeathType.Dust, offset: Point.Zero);
	}

	private void DoOpen()
	{
		_isSolid = false;
		base.CanBeTriggered = false;
		_isOpen = true;
		CutsceneBase.CreateAndCallCutscene(CutsceneBase.ECutsceneType.CavesPast3_MawSuck, _level, Position);
	}

	internal void HideBones()
	{
		_isSolid = false;
		_isHidden = true;
		for (int num = base.Appendages.Count - 1; num > 0; num--)
		{
			base.Appendages.RemoveAt(num);
		}
		_particleSystems.Add(_deathLazerParticles);
	}

	internal void AddRubble()
	{
		EnvironmentPrefabBase newObject = EnvironmentPrefabBase.Create(_level, new Point(Position.X + 1, Position.Y), -1, new ObjectTileSpecification(491)
		{
			Argument = 805
		});
		_level.RequestAddObject(newObject);
		List<Point> list = new List<Point>();
		foreach (Point key in _level.ForegroundTiles.Keys)
		{
			if (key.X > 12)
			{
				list.Add(key);
			}
		}
		foreach (Point item in list)
		{
			_level.ForegroundTiles.Remove(item);
		}
		SilentKill();
	}
}
