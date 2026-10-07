using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs;

internal sealed class EnvPrefabCurtainOneWayDoor : EnvironmentPrefabBase
{
	private const string SaveKey = "IsCurtainDungeonDoorOpen";

	private readonly CharacterSequenceSpecification _openSequence;

	private readonly CharacterSequenceSpecification _instantOpenSequence;

	private bool _isOpen;

	public EnvPrefabCurtainOneWayDoor(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		_sprite = _level.GCM.SpMiscCurtain;
		Bbox = new Rectangle(0, 0, 16, 80);
		IsFacingLeft = objectSpec?.IsFlippedHorizontally ?? false;
		_doesDrawBaseSprite = false;
		_doesUseAppendageCollision = false;
		_doAppendagesMatchImageFacing = true;
		if (base.CharacterSpecification != null && base.CharacterSpecification.Sequences.Count > 1)
		{
			_openSequence = base.CharacterSpecification.Sequences[0];
			_instantOpenSequence = base.CharacterSpecification.Sequences[1];
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		_isOpen = _level.GameSave.GetSaveBool("IsCurtainDungeonDoorOpen");
		if (_isOpen)
		{
			_isSolid = false;
			base.CanBeTriggered = false;
			base.DrawPlane = EDrawPlane.Back;
			SetCharacterSequence(_instantOpenSequence);
		}
		else
		{
			_isSolid = true;
			base.CanBeTriggered = true;
			base.DrawPlane = EDrawPlane.Front;
		}
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		bool flag = base.TriggerEvent(who, depth);
		if (flag && !_isOpen && who.Position.X < Position.X)
		{
			DoOpen();
		}
		return flag;
	}

	private void DoOpen()
	{
		PlayCue(ESFX.DoorDungeonOpen);
		SetCharacterSequence(_openSequence);
		_level.GameSave.SetValue("IsCurtainDungeonDoorOpen", value: true);
		_isSolid = false;
		base.CanBeTriggered = false;
		base.DrawPlane = EDrawPlane.Back;
	}
}
