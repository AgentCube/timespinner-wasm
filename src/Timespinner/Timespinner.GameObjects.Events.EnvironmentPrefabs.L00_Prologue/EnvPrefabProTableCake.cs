using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L00_Prologue;

internal sealed class EnvPrefabProTableCake : EnvironmentPrefabBase
{
	private const float TimeBeforeCoolingDown = 0.5f;

	private float _cooldownTimer;

	public EnvPrefabProTableCake(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		_sprite = _level.GCM.SpPlatforms;
		Bbox = new Rectangle(inPosition.X, inPosition.Y, 8, 16);
		base.TriggerBbox = new Rectangle(0, 0, 16, 16);
		SnapBboxToPosition();
		ChangeAnimation(-1);
		_doesDrawBaseSprite = false;
		_isSolid = false;
		base.CanBeTriggered = true;
		_isRepeatedTrigger = true;
		base.IsTriggerableByMonsters = false;
		base.CannotBeGrabbed = true;
		base.DoesCollideWithTiles = false;
		base.DoesCollideWithProjectiles = false;
		_doAppendagesMatchImageFacing = false;
		_doesUseAppendageCollision = false;
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
				_cooldownTimer = 0.5f;
				ShowMessage();
			}
		}
		return base.TriggerEvent(who, depth);
	}

	private void ShowMessage()
	{
		_level.ShowDialogueMessage("cs_pro_cake_00");
	}
}
