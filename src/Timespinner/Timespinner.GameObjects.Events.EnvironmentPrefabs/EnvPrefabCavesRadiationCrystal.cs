using System;
using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Misc;
using Timespinner.GameObjects.Items;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs;

internal sealed class EnvPrefabCavesRadiationCrystal : EnvironmentPrefabBase
{
	private const float TimeBeforeFlashing = 2f;

	private const float TimeToFlash = 0.2f;

	private const float TimeForTotalFlash = 2.2f;

	private const string DestroyedKey = "CaveRadiationCrystal_{0}";

	private readonly Point _startPositionKey;

	private float _itemFlashTimer;

	public EnvPrefabCavesRadiationCrystal(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		_startPositionKey = inPosition;
		_sprite = _level.GCM.SpItems;
		_bboxOffset = new Point(0, 4);
		Bbox = new Rectangle(0, 0, 16, 12);
		_doesDrawBaseSprite = false;
		_doesUseAppendageCollision = false;
		_doAppendagesInheritDrawColor = true;
		base.DoesCollideWithProjectiles = true;
		base.DrawPlane = EDrawPlane.Back;
	}

	public override void Initialize()
	{
		base.Initialize();
		if (_level.GetLevelSaveBool($"CaveRadiationCrystal_{_startPositionKey}"))
		{
			_level.RequestRemoveObject(this);
			return;
		}
		_isSolid = true;
		base.CanBeTriggered = true;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			UpdateItemFlash(delta);
		}
		base.Update(delta);
	}

	private void UpdateItemFlash(float delta)
	{
		_itemFlashTimer += delta;
		if (_itemFlashTimer >= 2f)
		{
			if (_itemFlashTimer >= 2.2f)
			{
				_itemFlashTimer = 0f;
				base.IsGlowing = false;
				base.GlowColor = Color.White;
				base.DrawColor = Color.White;
			}
			else
			{
				float num = _itemFlashTimer - 2f;
				float num2 = num / 0.2f;
				float alpha = (float)Math.Sin(num2 * (float)Math.PI);
				base.IsGlowing = true;
				base.GlowColor = new Color(1f, 0.75f, 0.85f, alpha);
			}
		}
	}

	public override bool ProjectileTriggerEvent(Projectile projectile, Vector2 depth)
	{
		bool result = false;
		if (!base.IsFrozen)
		{
			result = true;
			BestiaryItemDropSpecification bestiaryItemDropSpecification = new BestiaryItemDropSpecification();
			bestiaryItemDropSpecification.Category = 1;
			bestiaryItemDropSpecification.Item = 38;
			ItemDropPickup item = new ItemDropPickup(bestiaryItemDropSpecification, _level, Bbox.Center, -1);
			_level.AddItem(item);
			_level.PlayCue(ESFX.EnvPlasmaCrystalBreak, Position);
			SetCharacterSequenceByName("Destructable");
			UpdateCharacterSequences(0f);
			UpdateAppendages(0f);
			Vector2 force = projectile.VisibleVelocity * Math.Min(1, projectile.Force);
			force.Y = -600f;
			DebrisEvent.CreateFromObject(this, force, projectile.Bbox.Center, _sprite);
			_appendages.Clear();
			SilentKill();
			_level.SetLevelSaveBool($"CaveRadiationCrystal_{_startPositionKey}", value: true);
		}
		return result;
	}
}
