using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;

namespace Timespinner.GameObjects.Events.Relics;

internal class TalariaAttachment : RelicItemBase
{
	private const EInventoryRelicType RelicType = EInventoryRelicType.Dash;

	private const int RelicTypeKey = 9;

	public TalariaAttachment(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, SpriteSheet sprite)
		: base(inLevel, inPosition, inID, objectSpec, sprite)
	{
		ChangeAnimation(8);
	}

	public override void Initialize()
	{
		if (_level.GameSave.Inventory.RelicInventory.Inventory.ContainsKey(9))
		{
			SilentKill();
		}
		else
		{
			base.Initialize();
		}
	}

	internal override void OnPickedUp()
	{
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.StopCast, 0f, 0f, Vector4.Zero));
		AddLevelScriptAction(new ScriptAction(EInventoryRelicType.Dash));
		_level.UnlockRelic(EInventoryRelicType.Dash);
	}
}
