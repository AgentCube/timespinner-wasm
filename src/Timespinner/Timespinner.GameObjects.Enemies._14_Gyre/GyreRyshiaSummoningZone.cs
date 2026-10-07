using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Misc;

namespace Timespinner.GameObjects.Enemies._14_Gyre;

internal class GyreRyshiaSummoningZone : GameEvent
{
	private const int PieceCount = 16;

	private const float TimeBetweenAddingPieces = 0.03f;

	private readonly Random _randomizer;

	private readonly SummoningZonePiece[] _pieces = new SummoningZonePiece[16];

	private float _addTimer;

	internal bool IsActive { get; set; }

	internal bool IsReadyToSummon { get; set; }

	internal bool IsSummonFacingLeft { get; set; }

	public GyreRyshiaSummoningZone(Level inLevel, Point inPosition, SpriteSheet sprite)
		: base(inLevel, inPosition, -1, new ObjectTileSpecification())
	{
		_sprite = sprite;
		base.IsAffectedByTime = true;
		ChangeAnimation(-1);
		_doesDrawBaseSprite = false;
		base.DrawPlane = EDrawPlane.Front;
		_randomizer = new Random();
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && IsActive)
		{
			_addTimer -= delta;
			if (_addTimer <= 0f)
			{
				_addTimer += 0.03f;
				AddPiece();
			}
		}
		if (IsReadyToSummon)
		{
			ObjectTileSpecification objectTileSpecification = GyreSpawnerEvent.PickEnemy(_randomizer, IsSummonFacingLeft, canBeRare: false, canBeCeiling: false);
			objectTileSpecification.X = Position.X / 16;
			objectTileSpecification.Y = Position.Y / 16 - 1;
			_level.PlaceEvent(objectTileSpecification, shouldInitialize: true);
			IsReadyToSummon = false;
		}
		base.Update(delta);
	}

	internal void Move(Point position)
	{
		Position = position;
		_addTimer = 0f;
	}

	private void AddPiece()
	{
		for (int i = 0; i < 16; i++)
		{
			if (_pieces[i] == null)
			{
				SummoningZonePiece summoningZonePiece = new SummoningZonePiece(this, _level, _sprite);
				_pieces[i] = summoningZonePiece;
				_appendages.Add(summoningZonePiece);
				break;
			}
			if (!_pieces[i].IsActive)
			{
				_pieces[i].Reset(Position);
				break;
			}
		}
	}
}
