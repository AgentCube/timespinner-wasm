using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L00_Prologue;

internal class EnvPrefabProTable : EnvironmentPrefabBase
{
	private readonly CharacterSequenceSpecification _addCakeSequence;

	private readonly CharacterSequenceSpecification _flipTableSequence;

	public EnvPrefabProTable(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		_sprite = _level.GCM.SpPlatforms;
		Bbox = new Rectangle(inPosition.X, inPosition.Y, 16, 16);
		ChangeAnimation(-1);
		_doesDrawBaseSprite = false;
		_addCakeSequence = GetCharacterSequenceByName("AddCake");
		_flipTableSequence = GetCharacterSequenceByName("Flip");
	}

	internal void AddCake()
	{
		SetCharacterSequence(_addCakeSequence);
	}

	internal void FlipTable()
	{
		SetCharacterSequence(_flipTableSequence);
	}
}
