using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs;

internal sealed class EnvPrefabCityConveyorSwitch : EnvironmentPrefabBase
{
	private const float TimeBeforeCoolingDown = 0.5f;

	private readonly Appendage _arrowAppendage;

	private bool _isPointingLeft;

	private float _cooldownTimer;

	public EnvPrefabCityConveyorSwitch(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		Bbox = new Rectangle(0, 0, 32, 32);
		SnapBboxToPosition();
		_sprite = _level.GCM.SpMiscLab;
		_doesDrawBaseSprite = false;
		base.DrawPlane = EDrawPlane.Front;
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_isSolid = false;
		base.CanBeTriggered = true;
		_isRepeatedTrigger = true;
		base.IsTriggerableByMonsters = false;
		base.CannotBeGrabbed = true;
		base.DoesCollideWithTiles = false;
		base.DoesCollideWithProjectiles = true;
		_doAppendagesMatchImageFacing = false;
		_arrowAppendage = new Appendage(this, new Point(8, 8), Point.Zero, _level, _sprite)
		{
			FollowType = EAppendageFollowType.ParentObjectLocked,
			AnchorOffset = new Point(0, -16)
		};
		_arrowAppendage.ChangeAnimation(20, 3, 0.1f, EAnimationType.Cycle);
		base.Appendages.Add(_arrowAppendage);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && _cooldownTimer > 0f)
		{
			_cooldownTimer -= delta;
		}
		base.Update(delta);
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		if (!base.IsFrozen && _cooldownTimer <= 0f && who is Protagonist protagonist)
		{
			_level.RequestButtonPrompt(4, new Point(Bbox.Center.X, Bbox.Top));
			if (protagonist.CheckButton(4) && !protagonist.CheckButton(5) && !protagonist.CheckButton(7))
			{
				SwitchAllConveyorBelts();
			}
		}
		return base.TriggerEvent(who, depth);
	}

	private void SwitchAllConveyorBelts()
	{
		_cooldownTimer = 0.5f;
		FlipHorizontalDirection();
		PlayCue(ESFX.EnvConveyorSwap);
		IEnumerable<GameEvent> eventAllEventsOfType = _level.GetEventAllEventsOfType(EEventTileType.EnvironmentPrefab);
		foreach (GameEvent item in eventAllEventsOfType)
		{
			if (item is EnvPrefabCityConveyorFloor envPrefabCityConveyorFloor)
			{
				envPrefabCityConveyorFloor.IsFacingLeft = !envPrefabCityConveyorFloor.IsFacingLeft;
			}
			else if (item != this && item is EnvPrefabCityConveyorSwitch envPrefabCityConveyorSwitch)
			{
				envPrefabCityConveyorSwitch.FlipHorizontalDirection();
			}
		}
		IEnumerable<GameEvent> eventAllEventsOfType2 = _level.GetEventAllEventsOfType(EEventTileType.ConveyorBelt);
		foreach (GameEvent item2 in eventAllEventsOfType2)
		{
			if (item2 is ConveyorBeltFloorEvent conveyorBeltFloorEvent)
			{
				conveyorBeltFloorEvent.FlipHorizontalDirection();
			}
		}
	}

	private void FlipHorizontalDirection()
	{
		_isPointingLeft = !_isPointingLeft;
		_arrowAppendage.IsFacingLeft = !_arrowAppendage.IsFacingLeft;
	}
}
