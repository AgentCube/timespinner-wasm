using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.Treasure;

internal sealed class MapComputerEvent : GameEvent
{
	private const string MapSaveKey = "map_get_{0}";

	private readonly int _mapIndex;

	private readonly string _saveKey;

	private readonly GlowTexture _glowTexture;

	private readonly Appendage _screenAppendage;

	private readonly PassiveBuffSparkleParticleSystem _sparkleParticles;

	private bool _isUsable;

	private bool _wasActivating;

	private bool _doesPlayerHaveTablet;

	public MapComputerEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_mapIndex = objectSpec.Argument;
		_saveKey = $"map_get_{_mapIndex}";
		_bboxOffset = new Point(5, 12);
		Bbox = new Rectangle(0, 0, 22, 32);
		Position = new Point(inPosition.X, inPosition.Y);
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_doesPersist = true;
		_isAffectedByGravity = false;
		_isRepeatedTrigger = false;
		_isAffectedByTime = true;
		_doAppendagesInheritDrawColor = false;
		_isUsable = true;
		_sprite = _level.GCM.SpMiscLab;
		ChangeAnimation(23);
		_sparkleParticles = new PassiveBuffSparkleParticleSystem(_level.GCM.TxParticleEnergy, 10);
		_particleSystems.Add(_sparkleParticles);
		_screenAppendage = new Appendage(this, new Point(10, 17), Point.Zero, _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorOffset = new Point(0, -25),
			DrawPriority = 1,
			IsGlowing = true,
			GlowBase = 3.5f,
			GlowColor = new Color(0.9f, 0.5f, 0.2f, 0.6f)
		};
		_screenAppendage.ChangeAnimation(25);
		base.Appendages.Add(_screenAppendage);
		_glowTexture = new GlowTexture(_level)
		{
			GlowSpriteSheet = _sprite,
			FrameIndex = 25,
			GlowCircleCount = 6,
			Center = new Point(Position.X, Position.Y - 36),
			GlowCircleWidth = 32,
			GlowCircleHeight = 32,
			BaseColor = new Color(0.9f, 0.5f, 0.2f, 1f)
		};
	}

	public override void Initialize()
	{
		base.Initialize();
		if (_level.GameSave.GetSaveBool(_saveKey))
		{
			RemoveScreen(shouldUseAnimation: false);
		}
		else
		{
			_doesPlayerHaveTablet = _level.GameSave.Inventory.RelicInventory.IsRelicActive(EInventoryRelicType.Tablet);
		}
	}

	public override void Update(float delta)
	{
		if (_isUsable && !base.IsFrozen)
		{
			_glowTexture.Update(delta);
			if (_doesPlayerHaveTablet)
			{
				_sparkleParticles.AddParticles(_screenAppendage.Position.ToVector2());
			}
		}
		base.Update(delta);
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		bool flag = false;
		if (!base.IsFrozen && _isUsable && !_wasActivating && _doesPlayerHaveTablet)
		{
			Protagonist protagonist = who as Protagonist;
			_isTriggered = true;
			_level.RequestButtonPrompt(4, new Point(Bbox.Center.X, Bbox.Top));
			if (protagonist != null && protagonist.CheckButton(4) && !protagonist.CheckButton(5) && !protagonist.CheckButton(7))
			{
				flag = true;
				if (_doesPlayerHaveTablet)
				{
					GetMapReveal();
				}
			}
		}
		_wasActivating = flag;
		if (!flag)
		{
			return base.TriggerEvent(who, depth);
		}
		return false;
	}

	private void GetMapReveal()
	{
		_level.PlayCue(ESFX.ItemGetDownload, Bbox.Center);
		_level.GameSave.SetValue(_saveKey, value: true);
		_level.Minimap.RevealMapByRevealGroupID(_mapIndex);
		EInventoryUseItemType itemType = (EInventoryUseItemType)(128 + _mapIndex);
		_level.RequestItemGetPopup(itemType);
		RemoveScreen(shouldUseAnimation: true);
	}

	private void RemoveScreen(bool shouldUseAnimation)
	{
		if (_isUsable)
		{
			_isUsable = false;
			if (shouldUseAnimation)
			{
				_screenAppendage.ChangeAnimation(new AnimationSpec[2]
				{
					new AnimationSpec
					{
						Start = 26,
						Length = 3,
						Speed = 0.07f,
						Type = EAnimationType.Once
					},
					new AnimationSpec
					{
						Start = -1,
						Type = EAnimationType.None
					}
				});
			}
			else
			{
				base.Appendages.Remove(_screenAppendage);
			}
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);
		if (_isUsable)
		{
			_glowTexture.Draw(spriteBatch);
		}
	}
}
