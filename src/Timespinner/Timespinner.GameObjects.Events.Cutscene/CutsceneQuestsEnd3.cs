using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneQuestsEnd3 : CutsceneBase
{
	private const int StartY = 160;

	private const int LunaisStartX = 176;

	private const int LunaisDeathIndex = 204;

	private readonly AnimationSpec _deadAnimation;

	private readonly Protagonist _lunais;

	public CutsceneQuestsEnd3(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		_deadAnimation = new AnimationSpec
		{
			Start = 204,
			Length = 1
		};
		_lunais = _level.MainHero;
		AddScript(new ScriptAction(_deadAnimation, _lunais));
	}

	internal override void DoCutscene()
	{
		AddUnhideFamiliar();
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 205;
		animationSpec.Length = 1;
		animationSpec.Speed = 0.1f;
		animationSpec.Type = EAnimationType.Once;
		AnimationSpec newAnim = animationSpec;
		AnimationSpec animationSpec2 = new AnimationSpec();
		animationSpec2.Start = 244;
		animationSpec2.Length = 1;
		animationSpec2.Speed = 0.1f;
		animationSpec2.Type = EAnimationType.Once;
		AnimationSpec newAnim2 = animationSpec2;
		AnimationSpec animationSpec3 = new AnimationSpec();
		animationSpec3.Start = 212;
		animationSpec3.Length = 2;
		animationSpec3.Speed = 0.1f;
		animationSpec3.Type = EAnimationType.Once;
		AnimationSpec newAnim3 = animationSpec3;
		AnimationSpec animationSpec4 = new AnimationSpec();
		animationSpec4.Start = 0;
		animationSpec4.Length = 5;
		animationSpec4.Speed = 0.11f;
		animationSpec4.Type = EAnimationType.Cycle;
		AnimationSpec newAnim4 = animationSpec4;
		WarpPlayerToPosition(new Point(176, 160));
		AddLevelScriptAction(new ScriptAction(EBGM.Sanctuary));
		AddScript(new ScriptAction(_deadAnimation, _lunais));
		AddWaitScript(0.1f);
		AddScript(new ScriptAction(_deadAnimation, _lunais));
		AddWaitScript(0.4f);
		AddScript(new ScriptAction(_deadAnimation, _lunais));
		AddScript(new ScriptAction(newAnim, _lunais)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(newAnim2, _lunais)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(newAnim3, _lunais)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(newAnim4, _lunais)
		{
			DoesBlockQueue = false
		});
		_level.AddScript(new ScriptAction(EInventoryEquipmentType.NelisteEarring, 1));
	}
}
