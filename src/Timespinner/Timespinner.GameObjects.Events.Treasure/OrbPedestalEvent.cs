using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Misc;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Events.Treasure;

internal sealed class OrbPedestalEvent : GameEvent
{
	private const int FallEndOffsetY = -8;

	private const float OscillFrequencyY = (float)Math.PI;

	private const float OscillRadius = 3f;

	private const float OrbGlowFrequency = (float)Math.PI;

	private const float SubGlowColorPercentage = 0.8f;

	private const float FloatingHeightDecayRate = 20f;

	private const float TimeBeforeRefloating = 1f;

	private const int OrbAtlasFrame = 19;

	private const int Anim_BookStart = 49;

	private const int Anim_BookLength = 18;

	private const float Anim_BookSpeed = 0.12f;

	private static readonly Vector4 WhiteColorVector4 = Color.White.ToVector4();

	private readonly bool _isValid;

	private readonly EInventoryOrbType _orbType;

	private readonly int _orbIndex;

	private readonly Point _startingPosition;

	private readonly Color _baseOrbGlowColorAsColor;

	private readonly Vector4 _baseOrbGlowColorAsVector;

	private readonly ObjectTileSpecification _objectSpec;

	private readonly Appendage _orbAppendage;

	private readonly GlowTexture _glowCircle;

	private readonly OrbPedestalLeakParticleSystem _pixelLeakParticleSystem;

	private bool _isAlive;

	private bool _isDoneFloatingDown;

	private bool _hasOrb;

	private bool _isStandless;

	private int _fallTargetY;

	private float _floatingHeight;

	private float _floatDecayVelocity;

	private float _refloatTimer;

	private float _oscillDelta;

	private Vector4 _orbGlowColor;

	private SFXCueInstance _loopSFXInstance;

	internal bool IsAlive => _isAlive;

	internal bool DoesSpawnDespiteBeingOwned { get; set; }

	internal bool HasBeenPickedUp { get; private set; }

	public OrbPedestalEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_isAlive = true;
		_startingPosition = inPosition;
		_objectSpec = objectSpec;
		_orbType = (EInventoryOrbType)_objectSpec.Argument;
		_doesDrawBaseSprite = false;
		_sprite = _level.GCM.SpOrbPedestal;
		_bbox = new Rectangle(0, 0, 16, 16);
		Position = new Point(_position.X, _position.Y);
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_doesPersist = true;
		_isAffectedByGravity = false;
		_isRepeatedTrigger = false;
		_isAffectedByTime = true;
		_doAppendagesInheritDrawColor = false;
		_doAppendagesMatchImageFacing = false;
		base.DoesCollideWithTiles = false;
		_doesUseAppendageCollision = true;
		base.DoesCollideWithProjectiles = true;
		_baseOrbGlowColorAsVector = LunaisOrb.GetOrbGlowColorByType(_orbType);
		_baseOrbGlowColorAsColor = new Color(_baseOrbGlowColorAsVector);
		_glowCircle = new GlowTexture(_level)
		{
			BaseColor = _baseOrbGlowColorAsColor
		};
		_orbIndex = (int)_orbType;
		if (_orbIndex == 0)
		{
			_orbIndex = 1;
		}
		_orbIndex += 19;
		if (_appendages.Count > 0)
		{
			_orbAppendage = base.Appendages[3];
			_appendages[7].Position = _startingPosition;
			if (_orbType != EInventoryOrbType.Book)
			{
				_orbAppendage.ChangeAnimation(_orbIndex);
			}
			else
			{
				_orbAppendage.ChangeAnimation(49, 18, 0.12f, EAnimationType.Cycle);
				_orbAppendage.BboxOffset = new Point(4, 4);
			}
			_isValid = true;
		}
		_pixelLeakParticleSystem = new OrbPedestalLeakParticleSystem(_level.GCM.TxParticleEnergy, 10)
		{
			BaseColor = _baseOrbGlowColorAsVector
		};
		_particleSystems.Add(_pixelLeakParticleSystem);
		SetCharacterSequenceByName("Idle1");
		SetCharacterSequenceByName("Idle2");
		SetCharacterSequenceByName("Idle3");
	}

	public override void Initialize()
	{
		base.Initialize();
		_isAlive = DoesSpawnDespiteBeingOwned || !_level.GameSave.Inventory.OrbInventory.Inventory.ContainsKey((int)_orbType);
		if (!_isAlive)
		{
			_level.RequestRemoveObject(this);
			HasBeenPickedUp = true;
			return;
		}
		_hasOrb = true;
		_loopSFXInstance = PlayCue(ESFX.FoleyOrbPedestalLoop, isLooped: true);
		if (_loopSFXInstance != null)
		{
			_loopSFXInstance.UpdateType = SFXCueInstance.ECueInstanceUpdateType.Protagonist;
			_loopSFXInstance.Protagonist = _level.MainHero;
			_loopSFXInstance.Update(0f);
		}
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			UpdateGlowAndFloating(delta);
		}
		base.Update(delta);
	}

	private void UpdateGlowAndFloating(float delta)
	{
		if (!_isValid)
		{
			return;
		}
		_oscillDelta += delta;
		if (_oscillDelta > 10f)
		{
			_oscillDelta -= 10f;
		}
		float num = (float)Math.Sin((0f - _oscillDelta) * (float)Math.PI) * 3f;
		num -= 5f;
		if (_isStandless)
		{
			num = 0f;
		}
		_glowCircle.Update(delta);
		_orbGlowColor = _baseOrbGlowColorAsVector;
		_orbGlowColor.W = (float)Math.Cos(_oscillDelta * (float)Math.PI) * 0.25f + 0.35f;
		if (_orbAppendage != null && _isAlive)
		{
			_orbAppendage.IsGlowing = true;
			_orbAppendage.GlowColor = new Color(_orbGlowColor);
			if (_hasOrb)
			{
				_pixelLeakParticleSystem.AddParticles(_orbAppendage.Bbox.Center.ToVector2());
			}
			Vector4 color = _orbGlowColor.EaseTo(WhiteColorVector4, 0.8f);
			Color glowColor = new Color(color);
			for (int i = 0; i < 7; i++)
			{
				if (i != 3)
				{
					Appendage appendage = _appendages[i];
					appendage.IsGlowing = true;
					appendage.GlowColor = glowColor;
				}
			}
			Position = new Point(_startingPosition.X, _startingPosition.Y + (int)Math.Ceiling(num) + 2);
		}
		else
		{
			if (_isAlive)
			{
				return;
			}
			base.IsGlowing = true;
			base.GlowColor = new Color(_orbGlowColor);
			_pixelLeakParticleSystem.AddParticles(Bbox.Center.ToVector2());
			if (!_isDoneFloatingDown)
			{
				num = 0f;
				_floatDecayVelocity += delta * 20f;
				_floatingHeight -= delta * _floatDecayVelocity;
				if (_floatingHeight <= 0f)
				{
					_floatingHeight = 0f;
					_isDoneFloatingDown = true;
					_isStandless = false;
				}
			}
			else if (_refloatTimer < 1f)
			{
				_refloatTimer += delta;
				float num2 = (float)Math.Sin(_refloatTimer / 1f * ((float)Math.PI / 2f));
				num = num2 * num;
			}
			Position = new Point(Position.X, _fallTargetY - (int)_floatingHeight + (int)Math.Ceiling(num));
		}
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		if (!_isAlive && !base.IsFrozen)
		{
			GiveOrb();
			_level.RequestRemoveObject(this);
		}
		else if (_isAlive && base.IsFrozen && _hasOrb && _appendages.Count > 7 && _appendages[6].AnimationStart == 1 && _appendages[5].AnimationStart == 7 && _appendages[4].AnimationStart == 12 && who.Bbox.Intersects(_orbAppendage.Bbox))
		{
			_level.GameSave.UnlockFeat(EGameFeatType.StealOrb);
			GiveOrb();
			_orbAppendage.ChangeAnimation(-1);
			_hasOrb = false;
		}
		return false;
	}

	private void GiveOrb()
	{
		if (!HasBeenPickedUp)
		{
			_level.GameSave.GiveOrb(_orbType, EOrbSlot.Melee);
			AddWaitScript(0.15f);
			_level.AddScript(new ScriptAction(_orbType, EOrbSlot.Melee));
			Color tintColor = _baseOrbGlowColorAsColor * 0.75f;
			Point inPosition = (_isAlive ? _orbAppendage.Bbox.Center : new Point(Position.X + 1, Position.Y - 3));
			_level.AddAnimation(new OrbGetAnimation(_sprite, inPosition, _level, tintColor));
			HasBeenPickedUp = true;
			if (_loopSFXInstance != null)
			{
				_loopSFXInstance.Stop(0.2f);
			}
		}
	}

	public override bool ProjectileTriggerEvent(Projectile projectile, Vector2 depth)
	{
		bool flag = false;
		if (_isAlive && !base.IsFrozen)
		{
			flag = base.ProjectileTriggerEvent(projectile, depth);
			if (flag)
			{
				PlayCue(ESFX.FoleyOrbPedestalBreak);
				base.ActiveCharacterSequences.Clear();
				SetCharacterSequenceByName(_isStandless ? "NoStandDestructable" : "Destructable");
				UpdateCharacterSequences(0f);
				UpdateAppendages(0f);
				DebrisEvent.CreateFromObject(this, projectile.VisibleVelocity * Math.Min(1, projectile.Force), projectile.Bbox.Center, _sprite);
				_appendages.Clear();
				if (_hasOrb)
				{
					BecomeFallingOrb();
				}
				else
				{
					_level.RequestRemoveObject(this);
				}
			}
		}
		return flag;
	}

	private void BecomeFallingOrb()
	{
		_isAlive = false;
		base.DoesCollideWithProjectiles = false;
		_doesDrawBaseSprite = true;
		_doesUseAppendageCollision = false;
		_bboxOffset = new Point(1, 1);
		Bbox = new Rectangle(0, 0, 8, 8);
		Position = new Point(_orbAppendage.Position.X + 1, _orbAppendage.Position.Y + 1);
		SnapBboxToPosition();
		SnapFrameToBbox();
		if (_orbType != EInventoryOrbType.Book)
		{
			ChangeAnimation(_orbIndex);
		}
		else
		{
			ChangeAnimation(49, 18, 0.12f, EAnimationType.Cycle);
			_bboxOffset = new Point(4, 4);
		}
		Tile tile = _level.FindFirstSolidTileInDirection(Position, EDirection.South);
		if (tile != null)
		{
			_fallTargetY = tile.Bbox.Top + -8;
			_floatingHeight = _fallTargetY - Position.Y;
		}
		else
		{
			_fallTargetY = Position.Y;
			_floatingHeight = 0f;
		}
	}

	internal void RemoteSilentKill()
	{
		_appendages.Clear();
		base.ActiveCharacterSequences.Clear();
		BecomeFallingOrb();
	}

	internal void MakeStandless()
	{
		_isStandless = true;
		SetCharacterSequenceByName("NoStand");
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);
		if (_isValid && _hasOrb)
		{
			_glowCircle.Center = (_isAlive ? _orbAppendage.Bbox.Center : Bbox.Center);
			_glowCircle.Draw(spriteBatch);
		}
	}

	public override void Kill()
	{
		if (_loopSFXInstance != null)
		{
			_loopSFXInstance.Stop();
		}
		base.Kill();
	}
}
