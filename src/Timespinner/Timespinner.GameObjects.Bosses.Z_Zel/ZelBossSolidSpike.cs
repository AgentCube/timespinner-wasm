using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Bosses.Z_Zel;

internal class ZelBossSolidSpike : GameEvent
{
	private const float CreationTime = 0.05f;

	private readonly List<Alive> _peopleInsideDuringCreation = new List<Alive>();

	private readonly List<Alive> _peopleWhoTouchedThisFrame = new List<Alive>();

	private readonly Action<ZelInfernoProjectile> _onInfernoHit;

	private bool _isFinishedCreating;

	private float _creationTimer;

	public ZelBossSolidSpike(Level inLevel, Point inPosition, ObjectTileSpecification objectSpec, Action<ZelInfernoProjectile> onInfernoHit)
		: base(inLevel, inPosition, -1, objectSpec)
	{
		_onInfernoHit = onInfernoHit;
		_doesDrawSpriteAndAppendages = false;
		Bbox = new Rectangle(0, 0, 64, 104);
		_isAffectedByGravity = false;
		_isFlying = true;
		_isSolid = false;
		_doesPersist = true;
		_isRepeatedTrigger = true;
		base.IsTriggerableByMonsters = true;
		base.CannotBeGrabbed = false;
		base.DoesCollideWithTiles = false;
		base.DoesCollideWithProjectiles = true;
		base.DrawPlane = EDrawPlane.Normal;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (!_isFinishedCreating)
			{
				_creationTimer += delta;
				if (_creationTimer < 0.05f)
				{
					_peopleInsideDuringCreation.Clear();
				}
				else
				{
					_isSolid = true;
					_isFinishedCreating = true;
				}
			}
			else
			{
				if (_peopleInsideDuringCreation.Count > 0)
				{
					for (int num = _peopleInsideDuringCreation.Count - 1; num >= 0; num--)
					{
						Alive alive = _peopleInsideDuringCreation[num];
						bool flag = false;
						foreach (Alive item in _peopleWhoTouchedThisFrame)
						{
							if (item == alive)
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							_peopleInsideDuringCreation.RemoveAt(num);
						}
					}
				}
				_peopleWhoTouchedThisFrame.Clear();
			}
		}
		base.Update(delta);
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		bool flag = false;
		if (!who.IsABoss)
		{
			if (!_isFinishedCreating)
			{
				_peopleInsideDuringCreation.Add(who);
				flag = base.TriggerEvent(who, depth);
			}
			else
			{
				bool flag2 = false;
				if (_peopleInsideDuringCreation.Count > 0)
				{
					foreach (Alive item in _peopleInsideDuringCreation)
					{
						if (item == who)
						{
							flag2 = true;
							break;
						}
					}
				}
				if (flag2)
				{
					_peopleWhoTouchedThisFrame.Add(who);
				}
				else
				{
					flag = base.TriggerEvent(who, depth);
					if (flag && who.ScriptActionList.Count > 0 && who is LunaisObj)
					{
						_isSolid = false;
					}
				}
			}
		}
		return flag;
	}

	public override bool ProjectileTriggerEvent(Projectile projectile, Vector2 depth)
	{
		bool result = false;
		if (projectile is ZelInfernoProjectile obj)
		{
			_onInfernoHit(obj);
			_isSolid = false;
		}
		else
		{
			if (projectile.DoesCollideWithTiles && projectile.DoesDieOnImpact && !projectile.IsDamageArea)
			{
				projectile.KillOnGround(isVerticalCollision: false, RectangleExtensions.GetIntersectionCenter(projectile.Bbox, Bbox));
			}
			result = base.ProjectileTriggerEvent(projectile, depth);
		}
		return result;
	}

	internal void Finish()
	{
		base.CannotBeGrabbed = true;
	}

	internal void Reset(Point position)
	{
		base.ID = -1;
		Position = position;
		_isFinishedCreating = false;
		_isSolid = false;
		_creationTimer = 0f;
		base.CannotBeGrabbed = false;
		_peopleInsideDuringCreation.Clear();
		_peopleWhoTouchedThisFrame.Clear();
	}
}
