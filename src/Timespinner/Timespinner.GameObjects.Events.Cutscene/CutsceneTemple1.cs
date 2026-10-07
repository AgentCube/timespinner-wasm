using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Events.EnvironmentPrefabs;
using Timespinner.GameObjects.Events.EnvironmentPrefabs.L16_Temple;
using Timespinner.GameObjects.Events.Misc;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneTemple1 : CutsceneBase
{
	private const int PrefabID = 491;

	private const int HellfireArgument = 1603;

	private const float TimeForSandToDoItsThing = 3.58f;

	private SandStreamerEvent _streamers;

	private EnvPrefabTempleHellfire _hellfire;

	public CutsceneTemple1(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		base.DoesFadeOutWhenSkipped = false;
		base.IsWarpingAtEndOfCutscene = true;
	}

	internal override void DoCutscene()
	{
		_level.FullyHealPlayer();
		AddDelegateScript(CreateSand);
		AddUnskippableWaitScript(3.58f);
		AddDelegateScript(AddHellfire);
		AddUnskippableWaitScript(1.5f);
		AddDelegateScript(ChangeRooms);
	}

	private void ChangeRooms()
	{
		LevelChangeRequest levelChangeRequest = new LevelChangeRequest();
		levelChangeRequest.LevelID = 16;
		levelChangeRequest.PreviousLevelID = _level.ID;
		levelChangeRequest.RoomID = 26;
		levelChangeRequest.CutsceneToCall = ECutsceneType.None;
		levelChangeRequest.IsUsingWhiteFadeOut = true;
		levelChangeRequest.AdditionalBlackScreenTime = 0.25f;
		levelChangeRequest.FadeOutTime = 0.5f;
		levelChangeRequest.FadeInTime = 0.5f;
		LevelChangeRequest request = levelChangeRequest;
		_level.RequestChangeLevel(request);
	}

	private void CreateSand()
	{
		_streamers = new SandStreamerEvent(_level, Position, ESandStreamerType.SandmanDeath);
		_level.RequestAddObject(_streamers);
	}

	private void AddHellfire()
	{
		if (_hellfire == null)
		{
			_hellfire = new EnvPrefabTempleHellfire(inPosition: new Point(_level.RoomSize.X / 2, _level.RoomSize.Y / 2), inLevel: _level, inID: -1, objectSpec: new ObjectTileSpecification(491)
			{
				Argument = 1603
			}, prefabType: EEnvironmentPrefabType.L16_Hellfire, isOpening: true);
			_level.RequestAddObject(_hellfire);
		}
	}
}
