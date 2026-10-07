using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Heroes.Passives;

internal sealed class MoonOrbPassiveBubbleAppendage : Appendage
{
	private const int ShardAnimationStart = 33;

	private const int ShardAnimationLength = 8;

	private const int ShardCount = 16;

	private const int ShardOrbitRadiusA = 30;

	private const int ShardOrbitRadiusB = 27;

	private const int ShardOrbitOffsetY = 2;

	private const int OuterShardCount = 8;

	private const float OuterShardCountShardRotationFrequency = 1.5f;

	private const float InnerShardCountShardRotationFrequency = 1f;

	private const float OffsetBetweenShard = (float)Math.PI / 4f;

	private const float TimeToRegenerate = 10f;

	private const float GlowRotationSpeed = 1.5f;

	private static readonly Color BarrierColor = new Color(0.1f, 0.08f, 0.05f, 0.05f);

	private readonly Appendage _reverseHaloAppendage;

	private readonly Appendage[] _shardAppendages = new Appendage[16];

	private float _outerShardRotationTimer;

	private float _innerShardRotationTimer;

	private float _regenerationTimer;

	public bool IsAvailable { get; private set; }

	public MoonOrbPassiveBubbleAppendage(Animate parent, Level inLevel, SpriteSheet inSprite)
		: base(parent, new Point(64, 64), Point.Zero, inLevel, inSprite)
	{
		base.AnchorOffset = new Point(0, 18);
		base.FollowType = EAppendageFollowType.AnchorLocked;
		DrawOrigin = new Vector2(32f, 32f);
		ChangeAnimation(0);
		base.DrawColor = BarrierColor;
		_reverseHaloAppendage = new Appendage(this, new Point(64, 64), Point.Zero, _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			DrawColor = base.DrawColor,
			DrawOrigin = DrawOrigin,
			AnchorOffset = new Point(0, 0)
		};
		_reverseHaloAppendage.ChangeAnimation(0);
		_appendages.Add(_reverseHaloAppendage);
		for (int i = 0; i < 16; i++)
		{
			Appendage appendage = new Appendage(this, new Point(4, 4), Point.Zero, _level, _sprite)
			{
				DoesInheritDrawColor = false
			};
			appendage.ChangeAnimation(33 + i % 3 * 8, 8, 0.1f, EAnimationType.Cycle);
			_shardAppendages[i] = appendage;
			_appendages.Add(appendage);
		}
		Update(0f);
		UpdateShards(0f);
		Appendage[] shardAppendages = _shardAppendages;
		foreach (Appendage appendage2 in shardAppendages)
		{
			appendage2.Update(0f);
			appendage2.RefreshDrawPos();
		}
	}

	internal void SetTimer(float timer)
	{
		_regenerationTimer = timer;
		if (_regenerationTimer > 0f)
		{
			IsAvailable = false;
		}
	}

	internal float UpdateTimer(float delta)
	{
		if (!IsAvailable)
		{
			_regenerationTimer -= delta;
			if (_regenerationTimer <= 0f)
			{
				IsAvailable = true;
				_regenerationTimer = 0f;
			}
		}
		return _regenerationTimer;
	}

	public override void Update(float delta)
	{
		if (IsAvailable)
		{
			base.Rotation += 1.5f * delta;
			if (base.Rotation > (float)Math.PI * 2f)
			{
				base.Rotation -= (float)Math.PI * 2f;
			}
			_reverseHaloAppendage.Rotation -= 1.5f * delta;
			if (_reverseHaloAppendage.Rotation < 0f)
			{
				_reverseHaloAppendage.Rotation += (float)Math.PI * 2f;
			}
		}
		UpdateShards(delta);
		base.Update(delta);
	}

	private void UpdateShards(float delta)
	{
		_outerShardRotationTimer += delta * 1.5f;
		if (_outerShardRotationTimer >= (float)Math.PI * 2f)
		{
			_outerShardRotationTimer -= (float)Math.PI * 2f;
		}
		_innerShardRotationTimer -= delta * 1f;
		if (_innerShardRotationTimer <= 0f)
		{
			_innerShardRotationTimer += (float)Math.PI * 2f;
		}
		Point center = Bbox.Center;
		float num = 0f;
		int num2 = 0;
		Appendage[] shardAppendages = _shardAppendages;
		foreach (Appendage appendage in shardAppendages)
		{
			bool flag = num2 < 8;
			int num3 = (flag ? 30 : 27);
			float num4 = (flag ? _outerShardRotationTimer : _innerShardRotationTimer) + num;
			int num5 = (int)Math.Ceiling(Math.Cos(num4) * (double)num3);
			int num6 = (int)Math.Ceiling(Math.Sin(num4) * (double)num3);
			appendage.Position = new Point(center.X + num5, center.Y + num6 + 2);
			num += (float)Math.PI / 4f;
			num2++;
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (IsAvailable)
		{
			base.Draw(spriteBatch);
		}
	}

	public int ShieldDamage(int power)
	{
		int orbPassiveDamage = _level.GameSave.GetOrbPassiveDamage(EInventoryOrbType.Moon);
		int num = power - orbPassiveDamage;
		if (num < 0)
		{
			num = 0;
		}
		IsAvailable = false;
		_regenerationTimer = 10f;
		return num;
	}
}
