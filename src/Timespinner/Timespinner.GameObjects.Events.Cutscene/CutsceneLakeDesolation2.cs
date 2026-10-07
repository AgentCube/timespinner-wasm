using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.EnvironmentPrefabs;
using Timespinner.GameObjects.Events.EnvironmentPrefabs.L01_LakeDesolation;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneLakeDesolation2 : CutsceneBase
{
	private const int EnvPrefabID = 491;

	private const int ShipArgument = 100;

	private const int WalkOffsetX = 48;

	private const int WalkOffset2X = 64;

	private const int ShipStartOffsetX = -216;

	private const int ShipStartY = 48;

	private const int ShipEndOffsetX = 312;

	private Point _shipStart;

	private EnvPrefabLakeDesolationShip _ship;

	public CutsceneLakeDesolation2(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.WallTrigger;
	}

	internal override void DoCutscene()
	{
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 44;
		animationSpec.Length = 5;
		animationSpec.Speed = 0.1f;
		animationSpec.Type = EAnimationType.Once;
		AnimationSpec newAnim = animationSpec;
		AnimationSpec animationSpec2 = new AnimationSpec();
		animationSpec2.Start = 49;
		animationSpec2.Length = 2;
		animationSpec2.Speed = 0.1f;
		animationSpec2.Type = EAnimationType.Once;
		AnimationSpec newAnim2 = animationSpec2;
		Protagonist mainHero = _level.MainHero;
		MovePlayerToPosition(new Point(Position.X + 48, Position.Y), shouldFaceLeftAfter: true, shouldStandFancyAfter: false);
		AddScript(new ScriptAction(newAnim, mainHero)
		{
			DoesBlockQueue = true
		});
		AddDelegateScript(CreateShip);
		AddWaitScript(2f);
		AddPlayerFaceRoomCenter();
		AddWaitScript(3f);
		AddDelegateScript(RemoveShip);
		AddScript(new ScriptAction(newAnim2, mainHero)
		{
			DoesBlockQueue = true
		});
		MovePlayerToPosition(new Point(Position.X + 64, Position.Y), shouldFaceLeftAfter: false, shouldStandFancyAfter: false);
		AddDialogue("cs_lde_2_lun_00");
		AddDialogue("cs_lde_2_lun_01");
		AddDialogue("cs_lde_2_lun_02");
		CutsceneBase.AddLunaisPalmPunch(_level);
		AddDialogue("cs_lde_2_lun_03");
	}

	private void CreateShip()
	{
		Vector2 cameraPosition = _level.CameraPosition;
		_shipStart = new Point((int)cameraPosition.X + -216, 48);
		_ship = new EnvPrefabLakeDesolationShip(_level, _shipStart, -1, new ObjectTileSpecification(491)
		{
			Argument = 100
		}, EEnvironmentPrefabType.L1_Ship);
		_ship.Initialize();
		_level.RequestAddObject(_ship);
		_ship.StartFlight(_shipStart, new Point((int)cameraPosition.X + 312, 48));
	}

	private void RemoveShip()
	{
		if (_ship != null)
		{
			_ship.Remove();
		}
	}
}
