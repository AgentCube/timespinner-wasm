using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Misc;

internal sealed class SandStreamerEvent : GameEvent
{
	internal const int BossDeathStreamerCount = 12;

	internal const int AscendedStreamerCount = 16;

	private const int HeroDeathStreamerCount = 16;

	private const int SandmanDeathStreamerCount = 24;

	private const int NightmareDeathStreamerCount = 24;

	private const int SelenDeathStreamerCount = 12;

	private const int HeroDeathRingOffsetY = -12;

	private const int RingOffsetY = -20;

	private const int BossHealRingDiameter = 64;

	private const int HeroDeathRingDiameter = 128;

	private const float TimeToShowRing = 0.25f;

	private static readonly Color BaseRingColor = new Color(0.9f, 0.5f, 0.25f, 0.5f);

	private readonly bool _isBossStreamer;

	private readonly ESandStreamerType _streamerType;

	private readonly SandStreamerUnit _firstStreamer;

	private readonly HaloRingAnimation _ringAnimation;

	private readonly SFXCueInstance _sandCueInstance;

	private readonly SandStreamerUnit[] _streamers;

	private bool _isShowingRing;

	public SandStreamerEvent(Level inLevel, Point inPosition, ESandStreamerType streamerType)
		: base(inLevel, inPosition, -1, new ObjectTileSpecification())
	{
		_streamerType = streamerType;
		_isBossStreamer = _streamerType == ESandStreamerType.BossDeath;
		_sprite = _level.GCM.SpItems;
		ChangeAnimation(-1);
		Bbox = new Rectangle(0, 0, 16, 16);
		base.DrawPlane = EDrawPlane.Front;
		base.DoesCollideWithTiles = false;
		_isAffectedByTime = true;
		_isAffectedByGravity = false;
		int num;
		int diameter;
		switch (_streamerType)
		{
		case ESandStreamerType.HeroDeath:
			num = 16;
			diameter = 128;
			break;
		case ESandStreamerType.SandmanDeath:
			num = 24;
			diameter = 128;
			break;
		case ESandStreamerType.NightmareDeath:
			num = 24;
			diameter = 128;
			break;
		case ESandStreamerType.Ascended:
			num = 16;
			diameter = 128;
			break;
		case ESandStreamerType.SelenDeath:
			num = 12;
			diameter = 128;
			break;
		default:
			num = 12;
			diameter = 64;
			_sandCueInstance = PlayCue(ESFX.BossDeathSand, Position);
			break;
		}
		_ringAnimation = new HaloRingAnimation(_level)
		{
			Diameter = diameter,
			TimeToExpand = 0.25f,
			BaseDrawColor = BaseRingColor
		};
		_streamers = new SandStreamerUnit[num];
		for (int i = 0; i < num; i++)
		{
			SandStreamerUnit sandStreamerUnit = new SandStreamerUnit(this, new Point(8, 8), Point.Zero, _level, _sprite, i, _streamerType);
			_appendages.Add(sandStreamerUnit);
			_streamers[i] = sandStreamerUnit;
			if (i == 0)
			{
				_firstStreamer = sandStreamerUnit;
			}
		}
		if (_streamerType == ESandStreamerType.HeroDeath || _streamerType == ESandStreamerType.SelenDeath)
		{
			_isShowingRing = true;
			_ringAnimation.Center = inPosition.Add(0, -12);
		}
	}

	public override void Update(float delta)
	{
		if (_isShowingRing && !_ringAnimation.IsFinished)
		{
			_ringAnimation.Update(delta);
		}
		if (_isBossStreamer)
		{
			UpdateBossStreamers();
		}
		foreach (Appendage appendage in base.Appendages)
		{
			appendage.Update(delta);
		}
	}

	private void UpdateBossStreamers()
	{
		if (_firstStreamer != null && _firstStreamer.IsFinished && !_isShowingRing)
		{
			_level.FullyHealPlayer();
			_isShowingRing = true;
			_ringAnimation.Center = _level.GetPlayerPosition().Add(0, -20);
			_level.PlayCue(ESFX.BossDeathSandRestore, _ringAnimation.Center);
			if (_sandCueInstance != null && !_sandCueInstance.IsFinished)
			{
				_sandCueInstance.Stop(0.1f);
			}
		}
		if (_ringAnimation.IsFinished && (_firstStreamer == null || _firstStreamer.IsDead))
		{
			Kill();
		}
	}

	internal void SetStreamerPhase(EAscendStreamerPhase phase)
	{
		SandStreamerUnit[] streamers = _streamers;
		foreach (SandStreamerUnit sandStreamerUnit in streamers)
		{
			sandStreamerUnit.SetPhase(phase);
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);
		if (_isShowingRing && !_ringAnimation.IsFinished)
		{
			_ringAnimation.Draw(spriteBatch);
		}
	}
}
