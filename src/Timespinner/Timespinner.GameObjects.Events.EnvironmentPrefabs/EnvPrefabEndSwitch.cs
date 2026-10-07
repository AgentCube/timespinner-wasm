using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Cutscene;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs;

internal sealed class EnvPrefabEndSwitch : EnvironmentPrefabBase
{
	private const float TimeBeforeCoolingDown = 0.5f;

	private float _cooldownTimer;

	public EnvPrefabEndSwitch(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		Bbox = new Rectangle(0, 0, 32, 32);
		SnapBboxToPosition();
		_doesDrawBaseSprite = false;
		base.DrawPlane = EDrawPlane.Front;
		base.DoesDrawBoundingBox = true;
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_isSolid = false;
		base.CanBeTriggered = true;
		_isRepeatedTrigger = true;
		base.IsTriggerableByMonsters = false;
		base.CannotBeGrabbed = true;
		base.DoesCollideWithTiles = false;
		base.DoesCollideWithProjectiles = true;
		_doAppendagesMatchImageFacing = false;
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
				TriggerCutscene();
			}
		}
		return base.TriggerEvent(who, depth);
	}

	private void TriggerCutscene()
	{
		_cooldownTimer = 0.5f;
		switch (base.PrefabType)
		{
		case EEnvironmentPrefabType.L17_SwitchA:
			CutsceneBase.StartEnding(0, _level);
			break;
		case EEnvironmentPrefabType.L17_SwitchB:
			CutsceneBase.StartEnding(1, _level);
			break;
		case EEnvironmentPrefabType.L17_SwitchC:
			CutsceneBase.StartEnding(2, _level);
			break;
		case EEnvironmentPrefabType.L17_SwitchD:
			CutsceneBase.StartEnding(3, _level);
			break;
		}
	}
}
