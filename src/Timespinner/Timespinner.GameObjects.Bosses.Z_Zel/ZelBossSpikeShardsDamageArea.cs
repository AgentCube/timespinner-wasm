using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Z_Zel;

internal sealed class ZelBossSpikeShardsDamageArea : DamageArea
{
	private const int ParentOffsetX = 12;

	private const int ParentOffsetY = 60;

	private const int FloorY = 208;

	private const int ColumnCount = 4;

	private const int RowCount = 6;

	private const int ShardCount = 24;

	private const int SpikeWidth = 64;

	private const int SpikeHeight = 96;

	private const int UnitWidth = 16;

	private const int UnitHeight = 16;

	private const int HalfColumnCount = 2;

	private const int UnitBboxWidth = 8;

	private const int UnitBboxHeight = 8;

	private const int UnitBboxOffsetX = 4;

	private const int UnitBboxOffsetY = 4;

	private readonly int _baseDamage;

	private readonly ZelBossSpike _parentSpike;

	private readonly ZelShardDustParticleSystem _shardDustParticles;

	private readonly ZelBossSpikeShard[] _shards = new ZelBossSpikeShard[24];

	public ZelBossSpikeShardsDamageArea(Level inLevel, Point inPosition, ZelBossSpike parentSpike, int baseDamage, SpriteSheet sprite)
		: base(inLevel, inPosition, ETeamSide.Enemies, -1, null)
	{
		_parentSpike = parentSpike;
		_baseDamage = baseDamage;
		_sprite = sprite;
		Bbox = new Rectangle(0, 0, 4, 4);
		SnapBboxToPosition();
		ChangeAnimation(-1);
		_power = baseDamage;
		base.DoesKnockBack = true;
		_damageElement = EDamageElement.Blunt;
		_isAffectedByLevelBounds = false;
		_doesDieOutsideOfVisibleArea = false;
		_doesDieOnTiles = false;
		base.DoesDieToEnemyProjectiles = false;
		_doAppendagesInheritDrawColor = false;
		_doAppendagesMatchImageFacing = false;
		_doesUseAppendageCollision = true;
		_isAffectedByGravity = false;
		_isFlying = true;
		if (parentSpike.Appendages.Count > 0)
		{
			Rectangle frameSource = parentSpike.Appendages[0].FrameSource;
			int num = 0;
			for (int i = 0; i < 4; i++)
			{
				bool flag = i >= 2;
				int num2 = (flag ? ((4 - i - 1) * 16) : (i * 16));
				int x = i * 16;
				for (int j = 0; j < 6; j++)
				{
					int num3 = j * 16;
					int y = j * 16;
					ZelBossSpikeShard zelBossSpikeShard = new ZelBossSpikeShard(offset: new Point(x, y), frameSource: new Rectangle(frameSource.X + num2, frameSource.Y + num3, 16, 16), parent: this, bboxDimensions: new Point(8, 8), inBboxOffset: new Point(4, 4), inLevel: _level, inSprite: _sprite)
					{
						IsFacingLeft = !flag
					};
					_shards[num] = zelBossSpikeShard;
					_appendages.Add(zelBossSpikeShard);
					num++;
				}
			}
		}
		_shardDustParticles = new ZelShardDustParticleSystem(_level.GCM.TxParticleSmoke, 24);
		_particleSystems.Add(_shardDustParticles);
		_doesAutomaticallyEmitParticles = false;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			ZelBossSpikeShard[] shards = _shards;
			foreach (ZelBossSpikeShard zelBossSpikeShard in shards)
			{
				if (!zelBossSpikeShard.HasHitGround && zelBossSpikeShard.Position.Y >= 208)
				{
					zelBossSpikeShard.HasHitGround = true;
					_shardDustParticles.AddParticles(zelBossSpikeShard.Position.ToVector2());
				}
			}
		}
		base.Update(delta);
	}

	internal void Reset()
	{
		base.Life = 2f;
		_isFading = false;
		_fadeTimer = 0f;
		_power = _baseDamage;
		Position = _parentSpike.Position.Add(12, 60);
		SnapBboxToPosition();
		ZelBossSpikeShard[] shards = _shards;
		foreach (ZelBossSpikeShard zelBossSpikeShard in shards)
		{
			zelBossSpikeShard.Reset();
		}
	}

	internal void Push(Vector2 velocity, Point origin)
	{
		ZelBossSpikeShard[] shards = _shards;
		foreach (ZelBossSpikeShard zelBossSpikeShard in shards)
		{
			zelBossSpikeShard.Push(velocity, origin);
		}
	}
}
