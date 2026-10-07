using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Doors;

internal sealed class BossDoorEvent : SlidingDoorEvent
{
	private const float OscillFrequencyY = (float)Math.PI;

	private const float OrbGlowFrequency = (float)Math.PI;

	private const float TimeToWaitBeforeBeingClosed = 0.1f;

	private static readonly Vector4 BaseOrbGlowColor = new Vector4(0.9f, 0.95f, 1f, 1f);

	private readonly bool _isDemonDoor;

	private readonly Appendage _stoneAppendage;

	private bool _isDemonLocked;

	private float _oscillDelta;

	private float _timeWithoutBeingTriggered;

	public BossDoorEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_isLocked = false;
		_sprite = _level.GCM.SpSlidingDoors;
		base.EventType = EEventTileType.BossDoor;
		base.CanBeTriggeredByFamiliar = true;
		IsFacingLeft = true;
		ChangeAnimation(1);
		if (objectSpec != null)
		{
			_isDemonDoor = objectSpec.Argument == 1;
		}
		_stoneAppendage = new Appendage(this, new Point(16, 80), Point.Zero, _level, _sprite)
		{
			AnchorObject = this,
			FollowType = EAppendageFollowType.AnchorLocked
		};
		_stoneAppendage.ChangeAnimation(0);
		base.Appendages.Add(_stoneAppendage);
	}

	public override void Initialize()
	{
		bool flag;
		if (_level.ID != 14)
		{
			string bossKillKeyFromLevelID = BossClass.GetBossKillKeyFromLevelID(_level.ID);
			flag = _level.GameSave.GetSaveBool(bossKillKeyFromLevelID);
		}
		else
		{
			flag = _level.GetLevelSaveBool("IsGyreBossDead");
		}
		if (flag)
		{
			_doorState = ESlidingDoorState.Opened;
			base.IsOpenForever = true;
			SetPositionToOpen();
		}
		else if (_isDemonDoor)
		{
			_isDemonLocked = !_level.GameSave.GetSaveBool(BossClass.GetSaveKeyByBossType(EBossType.Demon));
			if (_isDemonLocked)
			{
				base.IsLocked = true;
			}
		}
		base.Initialize();
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		bool result = false;
		if (_timeWithoutBeingTriggered < 0.1f)
		{
			OpenDoor(-1f);
		}
		else
		{
			result = base.TriggerEvent(who, depth);
		}
		return result;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (_timeWithoutBeingTriggered < 0.1f)
			{
				_timeWithoutBeingTriggered += delta;
			}
			if (!base.IsLocked)
			{
				_oscillDelta += delta * (float)Math.PI;
				if (_oscillDelta >= (float)Math.PI * 2f)
				{
					_oscillDelta -= (float)Math.PI * 2f;
				}
				Vector4 baseOrbGlowColor = BaseOrbGlowColor;
				baseOrbGlowColor.W = (float)Math.Cos(_oscillDelta) * 0.2f + 0.8f;
				_stoneAppendage.IsGlowing = true;
				_stoneAppendage.GlowColor = new Color(baseOrbGlowColor);
			}
			else
			{
				_stoneAppendage.IsGlowing = false;
			}
		}
		base.Update(delta);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (_isDemonLocked)
		{
			spriteBatch.End();
			_level.GCM.EfSepiaTone.Parameters["sepiaAmount"].SetValue(0f);
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, _level.GCM.EfSepiaTone);
		}
		base.Draw(spriteBatch);
		if (_isDemonLocked)
		{
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null);
		}
	}
}
