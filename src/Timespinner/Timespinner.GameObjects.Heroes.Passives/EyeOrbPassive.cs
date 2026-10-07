using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Heroes.Passives;

internal class EyeOrbPassive : LunaisPassive
{
	private const int RingRadius = 128;

	private const int RingDiameter = 256;

	private const float TimeForRingToExpand = 0.75f;

	private const float TimeBeforeShowingRing = 0.25f;

	private const float TimeBeforeCheckingForBreakableWalls = 2f;

	private static readonly Color BaseRingColor = Color.Orange;

	private readonly Texture2D _ringTexture;

	private readonly Dictionary<int, Point> _breakableWalls = new Dictionary<int, Point>();

	private bool _isThereABreakableWall;

	private float _wallCheckTimer;

	private float _targetRingPercentage;

	private float _targetRingTimer;

	public override EInventoryOrbType PassiveType => EInventoryOrbType.Eye;

	public EyeOrbPassive(LunaisObj parentLunais)
		: base(parentLunais)
	{
		_ringTexture = parentLunais.Level.GCM.TxLargeRing;
		_wallCheckTimer = 2f;
	}

	public override void ChangeRoom()
	{
		_wallCheckTimer = 2f;
		base.ChangeRoom();
	}

	public override void Update(float delta)
	{
		_wallCheckTimer += delta;
		if (_wallCheckTimer > 2f)
		{
			_wallCheckTimer -= 2f;
			_breakableWalls.Clear();
			IEnumerable<GameEvent> eventAllEventsOfType = _level.GetEventAllEventsOfType(EEventTileType.BreakableWall);
			foreach (GameEvent item in eventAllEventsOfType)
			{
				int objectArgument = item.ObjectArgument;
				if (!_breakableWalls.ContainsKey(objectArgument))
				{
					_breakableWalls.Add(objectArgument, item.Position);
				}
				else
				{
					_breakableWalls[objectArgument] = item.Position.Lerp(_breakableWalls[objectArgument], 0.5f);
				}
			}
			_isThereABreakableWall = _breakableWalls.Count > 0;
		}
		if (_isThereABreakableWall)
		{
			_targetRingTimer += delta;
			if (_targetRingTimer > 1f)
			{
				_targetRingTimer = 0f;
			}
			if (_targetRingTimer < 0.25f)
			{
				_targetRingPercentage = 0f;
			}
			else
			{
				_targetRingPercentage = (float)Math.Sin((float)Math.PI / 2f * (_targetRingTimer - 0.25f) / 0.75f);
			}
		}
		base.Update(delta);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (_targetRingTimer > 0f)
		{
			int num = (int)(256f * _targetRingPercentage);
			Color color = BaseRingColor * (1f - _targetRingPercentage);
			foreach (Point value3 in _breakableWalls.Values)
			{
				Vector2 value2 = Vector2.Subtract(value2: new Vector2((float)value3.X - (float)num / 2f, (float)value3.Y - (float)num / 2f), value1: _level.CameraPosition);
				value2 = Vector2.Subtract(_level.LevelRenderCenter, value2);
				spriteBatch.Draw(destinationRectangle: new Rectangle((int)value2.X, (int)value2.Y, num, num), texture: _ringTexture, color: color);
			}
		}
		base.Draw(spriteBatch);
	}
}
