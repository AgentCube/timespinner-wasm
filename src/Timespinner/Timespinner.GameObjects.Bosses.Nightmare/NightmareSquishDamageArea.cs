using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Nightmare;

internal class NightmareSquishDamageArea : DamageArea
{
	private const int BboxWidth = 96;

	private const int BboxHeight = 240;

	private float _hellfireTimer;

	public NightmareSquishDamageArea(Level inLevel, Point inPosition, int baseDamage, SpriteSheet sprite)
		: base(inLevel, inPosition, ETeamSide.Enemies, -1, null)
	{
		ChangeAnimation(-1);
		_sprite = sprite;
		_power = (int)Math.Ceiling((float)baseDamage * 1.35f);
		_life = 10f;
		_damageElement = EDamageElement.Dark;
		Bbox = new Rectangle(0, 0, 96, 0);
		base.DoesDrawBoundingBox = true;
		_isAffectedByGravity = false;
		base.DoesKnockBack = false;
	}

	public override void Update(float delta)
	{
		int num = 16;
		float num2 = 0.75f;
		float num3 = 0.15f;
		float num4 = 0.15f;
		float num5 = num2 + num3;
		float num6 = num5 + num4;
		if (base.IsFrozen)
		{
			_isFrozen = false;
		}
		_hellfireTimer += delta;
		if (_hellfireTimer < num2)
		{
			float num7 = _hellfireTimer / num2;
			int height = (int)Math.Ceiling(Math.Sin(num7 * (float)Math.PI) * (double)num);
			Bbox = new Rectangle(Bbox.X, Bbox.Y, 96, height);
		}
		else if (_hellfireTimer < num5)
		{
			float num8 = (_hellfireTimer - num2) / num3;
			int height2 = (int)Math.Ceiling((1.0 - Math.Cos(num8 * ((float)Math.PI / 2f))) * 240.0);
			Bbox = new Rectangle(Bbox.X, Bbox.Y, 96, height2);
			base.CanDamageThings = true;
		}
		else if (_hellfireTimer < num6)
		{
			float num9 = (_hellfireTimer - num5) / num4;
			int num10 = (int)Math.Ceiling((1.0 - Math.Cos(num9 * ((float)Math.PI / 2f))) * 240.0);
			Bbox = new Rectangle(Bbox.X, Bbox.Y - num10, 96, 240 - num10);
			Position = new Point(Position.X, Bbox.Height);
		}
		else
		{
			SilentKill();
		}
		base.Update(delta);
	}

	public override void SnapBboxToPosition()
	{
		_bbox.Location = new Point(_position.X - _bbox.Width / 2, _position.Y - _bbox.Height);
	}

	internal void Reset(Point position)
	{
		Position = position;
		base.ID = -1;
		_hellfireTimer = 0f;
		_isFading = false;
		_fadeTimer = 0f;
		base.CanDamageThings = false;
		_life = 10f;
		ClearTrailHistory();
		Update(0f);
	}
}
