using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.LevelEffects;

internal sealed class ForestCampfireLevelEffect : LevelEffect
{
	private const int StartOffsetX = 8;

	private const int StartOffsetY = 118;

	private const float FireFrequency = 5f;

	private const float FireColorR = 0.1f;

	private const float FireColorG = 0.075f;

	private const float FireColorB = 0.05f;

	private float _fireTimer;

	public ForestCampfireLevelEffect(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		Position = new Point(Position.X + 8, Position.Y + 118);
		base.DrawPlane = EDrawPlane.Front;
		base.DoesCollideWithTiles = false;
		base.DoesCollideWithProjectiles = false;
		Bbox = new Rectangle(Position.X, Position.Y, 256, 256);
		SnapBboxToPosition();
		_sprite = _level.GCM.BgFog;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			_fireTimer += delta * 5f;
			if (_fireTimer >= (float)Math.PI * 2f)
			{
				_fireTimer -= (float)Math.PI * 2f;
			}
			float num = _fireTimer / ((float)Math.PI * 2f);
			num = (float)(int)(num * 4f) * 0.25f;
			base.DrawColor = new Color(0.1f, 0.075f, 0.05f, num);
		}
		base.Update(delta);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.End();
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, _level.GCM.EfCampfire);
		base.Draw(spriteBatch);
		spriteBatch.End();
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null);
	}
}
