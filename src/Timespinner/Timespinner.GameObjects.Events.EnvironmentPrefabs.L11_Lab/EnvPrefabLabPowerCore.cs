using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Treasure;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L11_Lab;

internal sealed class EnvPrefabLabPowerCore : EnvironmentPrefabBase
{
	private const int ForceFieldCount = 5;

	private const int ForceFieldObjectID = 491;

	private const int ForceFieldObjectArgument = 1114;

	private const int ForceFieldOffsetIncrementX = 32;

	private const int ForceFieldOffsetY = -112;

	private const int OrbPedestalObjectID = 480;

	private const int OrbPedestalArgument = 11;

	private const int OrbOffsetX = 0;

	private const int OrbOffsetY = -38;

	private readonly EnvPrefabLabForceField[] _forceFields = new EnvPrefabLabForceField[5];

	private bool _isPowerOff;

	private bool _areForceFieldsValid;

	private OrbPedestalEvent _orbPedestal;

	public EnvPrefabLabPowerCore(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		_sprite = _level.GCM.SpMiscLab;
		_doesDrawBaseSprite = false;
		_isSolid = false;
		_doAppendagesMatchImageFacing = false;
		_doAppendagesInheritDrawColor = false;
		SetCharacterSequenceByName("Idle");
	}

	public override void Initialize()
	{
		base.Initialize();
		_isPowerOff = _level.GameSave.GetSaveBool("11_LabPower");
		if (!_isPowerOff)
		{
			_areForceFieldsValid = true;
			int num = -64;
			ObjectTileSpecification objectTileSpecification = new ObjectTileSpecification(491);
			objectTileSpecification.Argument = 1114;
			ObjectTileSpecification objectSpec = objectTileSpecification;
			for (int i = 0; i < 5; i++)
			{
				_forceFields[i] = new EnvPrefabLabForceField(_level, new Point(Position.X + num, Position.Y + -112), -1, objectSpec, EEnvironmentPrefabType.L11_ForceField);
				num += 32;
			}
			_orbPedestal = new OrbPedestalEvent(_level, new Point(Position.X, Position.Y + -38), -1, new ObjectTileSpecification(480)
			{
				Argument = 11
			})
			{
				DoesSpawnDespiteBeingOwned = true
			};
			_orbPedestal.Initialize();
			_orbPedestal.MakeStandless();
			_level.RequestAddObject(_orbPedestal);
		}
		else
		{
			OrbPedestalEvent orbPedestalEvent = new OrbPedestalEvent(_level, Position, -1, new ObjectTileSpecification(480)
			{
				Argument = 11
			});
			orbPedestalEvent.Initialize();
			orbPedestalEvent.RemoteSilentKill();
			_level.RequestAddObject(orbPedestalEvent);
		}
	}

	public override void Freeze()
	{
		EnvPrefabLabForceField[] forceFields = _forceFields;
		for (int i = 0; i < forceFields.Length; i++)
		{
			forceFields[i]?.Freeze();
		}
		base.Freeze();
	}

	public override void Unfreeze()
	{
		EnvPrefabLabForceField[] forceFields = _forceFields;
		for (int i = 0; i < forceFields.Length; i++)
		{
			forceFields[i]?.Unfreeze();
		}
		base.Unfreeze();
	}

	public override void Update(float delta)
	{
		if (_areForceFieldsValid)
		{
			EnvPrefabLabForceField[] forceFields = _forceFields;
			foreach (EnvPrefabLabForceField envPrefabLabForceField in forceFields)
			{
				envPrefabLabForceField.Update(delta);
			}
		}
		base.Update(delta);
		_ = base.IsFrozen;
		if (_isPowerOff || !_areForceFieldsValid || (_orbPedestal.IsAlive && !_orbPedestal.HasBeenPickedUp))
		{
			return;
		}
		_level.GameSave.LastWarpLevel = 0;
		_level.GameSave.LastWarpRoom = 0;
		IEnumerable<GameEvent> eventAllEventsOfType = _level.GetEventAllEventsOfType(EEventTileType.WestTeleport);
		foreach (GameEvent item in eventAllEventsOfType)
		{
			_level.RequestRemoveObject(item);
		}
		_isPowerOff = true;
		_level.GameSave.SetValue("11_LabPower", value: true);
		PlayCue2D(ESFX.EnvLabPowerDown);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (_areForceFieldsValid)
		{
			EnvPrefabLabForceField[] forceFields = _forceFields;
			foreach (EnvPrefabLabForceField envPrefabLabForceField in forceFields)
			{
				envPrefabLabForceField.Draw(spriteBatch);
			}
		}
		base.Draw(spriteBatch);
	}
}
